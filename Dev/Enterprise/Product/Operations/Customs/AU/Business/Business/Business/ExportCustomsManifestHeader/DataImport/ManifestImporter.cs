using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ManifestImporter : DataImporter
	{
		public ManifestImporter(BusinessObjectFactory factory)
			: base(new SingleBusinessObjectFactoryProvider(factory))
		{
		}

		public static ManifestImporter GetImporter(BusinessObjectFactory factory, string filename)
		{
			ManifestImporter result;
			Type concreteType = TypeDecider.GetTypeForBinding(typeof(ManifestImporter));
			if (concreteType == typeof(ManifestImporter))
			{
				if (filename.ToUpper().EndsWith(".CSV"))
				{
					result = new RCLShippingManifestImporter(factory);
				}
				else
				{
					result = new EuroPacificManifestImporter(factory);
				}
			}
			else
			{
				result = (ManifestImporter)Activator.CreateInstance(concreteType, new object[] { factory });
			}
			return result;
		}

		public bool ImportDataToHeader(ExportCustomsManifestHeader header, StreamReader dataStream, INotifications notify)
		{
			generatedHeader = header;
			ITransactionParticipant[] transactionActions;
			return ImportDataToFactory(dataStream, string.Empty, notify, new SourceInfo(BillingDataSource.InterfaceConnector, this.InterfaceName, ZGuid.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty), out transactionActions);
		}

		protected override bool ImportDataToFactoryCore(TextReader dataStream, string attachmentFileName, INotifications notify, out ITransactionParticipant[] transactionActions)
		{
			bool result = false;
			transactionActions = null;
			if (GeneratedHeader.Lines.Count > 0)
			{
				notify.Notify(new InfoNotification("You can't import to this record because it already has one or more lines entered for it."));
			}
			else
			{
				DoImport(dataStream.ReadToEnd());
				result = true;
			}
			return result;
		}

		#region Implementation

		protected virtual void DoImport(string data)
		{
			string[] lines = StringParser.TrimBlankLinesAndNewLines(data.Split('\r', '\n'));
			foreach (string line in lines)
			{
				ProcessLine(line);
			}
		}

		protected abstract void ProcessLine(string line);

		public ExportCustomsManifestHeader GeneratedHeader
		{
			get { return generatedHeader ?? (generatedHeader = FactoryProvider.Current.New<ExportCustomsManifestHeader>()); }
		}
		ExportCustomsManifestHeader generatedHeader;

		protected void ResetGeneratedHeader()
		{
			generatedHeader = null;
		}

		protected StringToBusinessObjectFieldConverter Mapper
		{
			get { return StringToBusinessObjectFieldConverter.InstanceForCurrentCompany; }
		}

		protected abstract BillingInterfaceName InterfaceName { get; }

		public ZString CurrentCountryOfDestination;
		public ZString CurrentOwnerName;

		public bool NewLineAdded;
		#endregion
	}
}
