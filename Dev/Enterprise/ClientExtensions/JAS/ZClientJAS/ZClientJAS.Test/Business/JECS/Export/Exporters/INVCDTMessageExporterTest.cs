using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class INVCDTMessageExporterTest : JXCMessageExporterTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", InvoiceWrapper, Exporter.HeaderData);
			AssertEquals("Export Logger should be initialised in the constructor with the Invoice as the Export Source", InvoiceWrapper.Invoice, ((JXCExportLogger)Exporter.NotificationSubscriber).ExportSource);
		}

		public void TestExportValidationTypeToUse()
		{
			AssertEquals("Should use JXC Invoicing validation", JXCExportValidationType.Invoicing, Exporter.ExportValidationTypeToUse);
		}

		public void TestExportedMessage()
		{
			SetupInvoiceForTestExportedMessage();
			SetupDummyExporterForTestExportedMessage();
			AssertExportedMessage(Exporter);
		}

		#region Implementation
		void SetupInvoiceForTestExportedMessage()
		{
			Invoice.AH_TransactionNum = "TRAN_/0001";
			Invoice.AH_InvoiceDate = new ZDateTime(2005, 12, 12);
			Invoice.AH_JH = Factory.NewJobForTesting<JobHeader>().PK;
			Invoice.Job.JH_JobNum = "JOB101";
			Invoice.AH_RX_NKTransactionCurrency = "IDR";
			Invoice.AH_OSTotal = 1000;
			JASOrgHeader sendingForwarder = GlbBranch.CurrentBranch.OrgProxy as JASOrgHeader;
			sendingForwarder.NettingCode = "AUCOR";
			sendingForwarder.OfficeCode = "AUSYD";
			JASOrgHeader receivingForwarder = Factory.New<JASOrgHeader>();
			receivingForwarder.OH_Code = "RECVFWDORG";
			receivingForwarder.NettingCode = "ITMIL";
			receivingForwarder.OfficeCode = "ITVAL";
			Invoice.AH_OH = receivingForwarder.PK;
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			Invoice.Job.JH_ParentID = shipment.PK;
			Invoice.Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "08/329-*04";
			shipment.JS_UniqueConsignRef = "SHIP101";
			shipment.JS_ActualChargeable = 440m;
			shipment.JS_RL_NKDestination = "ITROM";
			InvoicingLineBase invoiceLine1 = (InvoicingLineBase)Invoice.Lines.AddNew();
			invoiceLine1.AL_Desc = "Line 1";
			InvoicingLineBase invoiceLine2 = (InvoicingLineBase)Invoice.Lines.AddNew();
			invoiceLine2.AL_Desc = "second line";
			InvoicingLineBase invoiceLine3 = (InvoicingLineBase)Invoice.Lines.AddNew();
			invoiceLine3.AL_Desc = "third one";
			InvoicingLineBase invoiceLine4 = (InvoicingLineBase)Invoice.Lines.AddNew();
			invoiceLine4.AL_Desc = "Line 04";
			InvoicingLineBase invoiceLine5 = (InvoicingLineBase)Invoice.Lines.AddNew();
			invoiceLine5.AL_Desc = "555";
		}

		void SetupDummyExporterForTestExportedMessage()
		{
			ExpectedMessageExporter.HeaderData.FreightDest = "ITROM";
			ExpectedMessageExporter.HeaderData.SetSendingForwarder("AUSYD", "AUCOR");
			ExpectedMessageExporter.HeaderData.SetDestinationForwarder("ITVAL", "ITMIL");
			MessageLine[] expectedMessageLines = new MessageLine[6];
			expectedMessageLines[0] = INVCDTLine.New(InvoiceWrapper);
			expectedMessageLines[1] = new INVDLine(Invoice.Lines[0]);
			expectedMessageLines[2] = new INVDLine(Invoice.Lines[1]);
			expectedMessageLines[3] = new INVDLine(Invoice.Lines[2]);
			expectedMessageLines[4] = new INVDLine(Invoice.Lines[3]);
			expectedMessageLines[5] = new INVDLine(Invoice.Lines[4]);
			JXCMessageExporter.MessageFileNameAndContents expected = new JXCMessageExporter.MessageFileNameAndContents("TRAN0001_0832904.txt", expectedMessageLines);
			ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = new JXCMessageExporter.MessageFileNameAndContents[1] { expected };
		}

		INVCDTMessageExporter Exporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new INVCDTMessageExporter(InvoiceWrapper);
				}

				return fExporter;
			}
		}

		InvoiceWrapper InvoiceWrapper
		{
			get
			{
				if (fInvoiceWrapper == null)
				{
					fInvoiceWrapper = new InvoiceWrapper(Invoice);
				}

				return fInvoiceWrapper;
			}
		}

		JASARInvoice Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = Factory.New<JASARInvoice>();
				}

				return fInvoice;
			}
		}

		INVCDTMessageExporter fExporter;
		InvoiceWrapper fInvoiceWrapper;
		JASARInvoice fInvoice;
		ZGuid initialProxyOrgPK;
		protected override void SetUp()
		{
			initialProxyOrgPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "1234567890123456789012345678901234567890";
			org.MainAddress.OA_Address1 = "2345678901234567890123456789012345678901";
			org.MainAddress.OA_Address2 = "3456789012345678901234567890123456789012";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = initialProxyOrgPK;
		}
		#endregion
	}
}
