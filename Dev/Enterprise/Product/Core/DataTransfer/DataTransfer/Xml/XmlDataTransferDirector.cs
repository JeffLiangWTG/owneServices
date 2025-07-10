using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Business
{
	public class XmlDataTransferDirector : IXmlDataTransferImporter
	{
		public XmlDataTransferDirector(IValueObjectDataAdapter adapter, bool checkForPermission)
		{
			this.Adapter = adapter;
			this.CheckForPermission = checkForPermission;
		}

		public XmlDataTransferDirector() : this(new StandardManualAndBatchImportOrganisationValueObjectDataAdapter(), false) { }

		protected readonly IValueObjectDataAdapter Adapter;

		public virtual XmlValueObjectSerializer Serializer
		{
			get
			{
				return serializer ?? new XmlValueObjectSerializer(Adapter.ValueObjectType, importInSingleFactory: OnlySaveDataWhenNoRecordsHaveErrors);
			}
			set
			{
				serializer = value;
			}
		}

		protected virtual void SetImportSettings(bool onlySaveDataWhenNoRecordsHaveErrors)
		{
			OnlySaveDataWhenNoRecordsHaveErrors = onlySaveDataWhenNoRecordsHaveErrors;
		}

		XmlValueObjectSerializer serializer;

		protected bool OnlySaveDataWhenNoRecordsHaveErrors
		{
			get;
			private set;
		}

		#region Import

		public void PromptUserAndImport(string fileName, BillingInterfaceName interfaceName)
		{
			if (!IsAllowedToImport())
			{
				return;
			}

			var importer = NewXmlDataImporter();
			using (var form = ObjectFactory.Get<IXmlDataImporterForm>(nameof(IXmlDataImporterForm), interfaceName))
			{
				form.Importer = importer;
				form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(Adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked, Adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible);
				form.ImportFromFile(fileName);
				form.ShowDialog();
			}
		}

		public void PromptUserAndImport(BillingInterfaceName interfaceName)
		{
			if (IsAllowedToImport())
			{
				PromptUserAndImportCore(interfaceName);
			}
		}

		public void Import(string fileName, INotifications notify, ISourceInfo info)
		{
			if (IsAllowedToImport())
			{
				ImportCore(fileName, notify, info);
			}
		}

		protected virtual void PromptUserAndImportCore(BillingInterfaceName interfaceName)
		{
			var importer = NewXmlDataImporter();
			using (var form = ObjectFactory.Get<IXmlDataImporterForm>(nameof(IXmlDataImporterForm), interfaceName))
			{
				form.Importer = importer;
				form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(Adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked, Adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible);
				form.ShowDialog();
			}
		}

		protected virtual void ImportCore(string fileName, INotifications notify, ISourceInfo info)
		{
			var importer = NewXmlDataImporter();
			importer.ImportData(fileName, notify, info);
		}

		protected virtual bool UseTransaction => true;
		protected virtual void DoImport(TextReader reader, BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
		{
			var collection = new ImportedBusinessObjectCollection(factoryProvider);
			var connection = ((IDbConnected)factoryProvider.Current).Connection;

			if (UseTransaction)
			{
				using (var manager = connection.BeginTransactionWithManager())
				{
					Serializer.ImportXmlData(((StreamReader)reader).BaseStream, Adapter, collection, factoryProvider, notifications);
					if (connection.MustRollBack)
					{
						manager.RollbackTransaction();
						notifications.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("00784a7b-47c6-4a04-91a9-d69036ba1d28", "All changes have been rolled back")));
					}
					else
					{
						manager.CommitTransaction();
					}
				}
			}
			else
			{
				Serializer.ImportXmlData(((StreamReader)reader).BaseStream, Adapter, collection, factoryProvider, notifications);
			}
		}

		protected virtual XmlDataImporter NewXmlDataImporter()
		{
			return new DirectorXmlDataImporter(this);
		}

		protected class DirectorXmlDataImporter : XmlDataImporter
		{
			public DirectorXmlDataImporter(XmlDataTransferDirector outer)
				: base(outer.Adapter)
			{
				this.Outer = outer;
			}

			public DirectorXmlDataImporter(XmlDataTransferDirector outer, BusinessObjectFactoryProvider factoryProvider)
				: base(factoryProvider, outer.Adapter)
			{
				this.Outer = outer;
			}

			protected override void ImportXml(TextReader reader, BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
			{
				Outer.SetImportSettings(base.OnlySaveDataWhenNoRecordsHaveErrors);
				Outer.DoImport(reader, factoryProvider, notifications);
			}

			protected internal override XmlValueObjectSerializer GetSerializer()
			{
				return Outer.Serializer;
			}

			readonly XmlDataTransferDirector Outer;
		}

		/// <remarks>
		/// This collection is private and XmlDataTransferDirector is not really interested in its content.
		/// So method Add() does not really adds items into collection, but only counts them and triggers Factory changing.
		/// </remarks>
		[TestExcludeBusinessObjectsAllHaveTestCases]
		class ImportedBusinessObjectCollection : BusinessObjectCollection<BusinessObject>
		{
			public ImportedBusinessObjectCollection(BusinessObjectFactoryProvider factoryProvider)
				: base(factoryProvider.Current)
			{
				this.factoryProvider = factoryProvider;
			}

			readonly BusinessObjectFactoryProvider factoryProvider;

			public override void Add(BusinessObject businessObject)
			{
				if (++counter >= 100)
				{
					factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
					counter = 0;
				}
			}

			int counter;
		}

		#endregion

		#region Licence

		readonly bool CheckForPermission;

		public virtual bool IsPermitted
		{
			get { return Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasInterfaceConnector; }
		}

		public static string InterfaceConnectorPermissionError
		{
			get { return Res.GetString("247D29C9-2191-4408-8E24-31F65FEA7E71", "Interface Connector is not enabled on this system."); }
		}

		[SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer")]
		protected virtual void ShowPermissionError()
		{
			Globals.Message.ShowError(InterfaceConnectorPermissionError);
		}

		protected bool IsAllowedToExport()
		{
			return true;
		}

		protected bool IsAllowedToImport()
		{
			bool result;

			if (CheckForPermission)
			{
				if (IsPermitted)
				{
					result = true;
				}
				else
				{
					ShowPermissionError();
					result = false;
				}
			}
			else
			{
				result = true;
			}

			return result;
		}

		#endregion
	}
}
