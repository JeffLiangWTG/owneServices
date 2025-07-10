using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public partial class MultipleInvoiceXmlDataTransferDirector : XmlDataTransferDirector
	{
		public MultipleInvoiceXmlDataTransferDirector(bool checkLicence)
			: base(new FinancialInvoiceDataAdapter(true), checkLicence)
		{
			((FinancialInvoiceDataAdapter)Adapter).RunExtraValidation = true;
		}

		public override XmlValueObjectSerializer Serializer
		{
			get
			{
				return new InvoiceXmlValueObjectSerializer(ZArchitecture.Core.LedgerTypes.General, importInSingleFactory: base.OnlySaveDataWhenNoRecordsHaveErrors);
			}
		}

		public XmlDataImporter GetNewXmlDataImporter()
		{
			return this.NewXmlDataImporter();
		}

		public XmlDataImporter GetNewXmlDataImporter(BusinessObjectFactoryProvider factoryProvider)
		{
			return new MultipleInvoiceDirectorXmlDataImporter(this, factoryProvider);
		}

		protected override XmlDataImporter NewXmlDataImporter()
		{
			return new MultipleInvoiceDirectorXmlDataImporter(this);
		}

		class MultipleInvoiceDirectorXmlDataImporter : DirectorXmlDataImporter
		{
			public MultipleInvoiceDirectorXmlDataImporter(MultipleInvoiceXmlDataTransferDirector outer)
				: base(outer)
			{
			}

			public MultipleInvoiceDirectorXmlDataImporter(MultipleInvoiceXmlDataTransferDirector outer, BusinessObjectFactoryProvider factoryProvider)
				: base(outer, factoryProvider)
			{
			}

			protected override void ImportXml(TextReader reader, BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
			{
				factoryProvider.CurrentFactoryChanged += HandleCurrentFactoryChanged;
				factoryProvider.Current.SetContext(BusinessContext.AllowReopenJobWhenImporting);
				factoryProvider.Current.SetContext(BusinessContext.LegacyXMLImport);

				try
				{
					base.ImportXml(reader, factoryProvider, notifications);
				}
				finally
				{
					factoryProvider.CurrentFactoryChanged -= HandleCurrentFactoryChanged;
					factoryProvider.Current.RemoveContext(BusinessContext.LegacyXMLImport);
				}
			}

			void HandleCurrentFactoryChanged(object sender, EventArgs e)
			{
				var factoryProvider = sender as BusinessObjectFactoryProvider;
				if (factoryProvider != null)
				{
					factoryProvider.Current.SetContext(BusinessContext.AllowReopenJobWhenImporting);
					factoryProvider.Current.SetContext(BusinessContext.LegacyXMLImport);
				}
			}

			protected override bool ShouldSuspendValidation
			{
				get { return false; }
			}
		}
	}
}
