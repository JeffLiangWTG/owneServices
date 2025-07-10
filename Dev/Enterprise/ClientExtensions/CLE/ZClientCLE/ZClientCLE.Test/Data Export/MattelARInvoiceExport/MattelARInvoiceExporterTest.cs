using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	public class MattelARInvoiceExporterTest : TestCaseWithFactory
	{
		#region TestExportWithException
		public void TestExportWithException()
		{
			ForwardingShipment shipment = SetupShipmentWithARInvoices(Core.Constants.ContainerModes.BuyersConsol, Core.Constants.ContainerModes.BuyersConsol, "SHIPMENT", "invoice");
			ZString expectedErrorMesg = "Error occurs while exporting AR Invoices of Shipment(SHIPMENT)";
			Assert("Logger is empty", new ZString(Buffer.AsString).IsEmpty);
			Exporter.ThrowException = true;
			Exporter.ExportARInvoice();
			Assert("Logger should contain the error message", Buffer.AsString.Contains(expectedErrorMesg));
		}

		#endregion
		#region TestExportARInvoiceForShipment
		[TestDate(2006, 12, 12, 12, 12, 0)]
		public void TestExportARInvoiceForShipmentfromBuyersConsol()
		{
			DateTime currentDate = TestDateAttribute.Date;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			ForwardingShipment shipment = SetupShipmentWithARInvoices(Core.Constants.ContainerModes.BuyersConsol, Core.Constants.ContainerModes.BuyersConsol, "SHIPMENT", "invoice");
			AssertEquals("Precondition: DSB Invoice doesn't have DEX event", false, DSBInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Precondition: Final Invoice doesn't have DEX Event", false, FinalInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Precondition: Final Invoice 1 doesn't have DEX Event", false, FinalInvoice1.Logs.Find(Query).Length > 0);
			Exporter.ExportARInvoice();
			CombineAssertions(() =>
			{
				AssertContains("Notification should contain Invoice export log", "Invoices of Job Number", Buffer.AsString);
				AssertEquals("DSB Invoice have DEX event", true, DSBInvoice.Logs.Find(Query).Length > 0);
				AssertEquals("Final Invoice have DEX Event", true, FinalInvoice.Logs.Find(Query).Length > 0);
				AssertEquals("Final Invoice 1 have DEX Event", false, FinalInvoice1.Logs.Find(Query).Length > 0);
				AssertEquals("1 Email is sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				ZString expectedAttachmentName = "INV_200612121212_" + shipment.JobNumber + ".EDI";
				AssertExportResultEmailSent(email, expectedAttachmentName);
			});
			Env.OutgoingMailManager.EmailsCreated.Clear();
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(-4).ToDateTime();
			shipment = SetupShipmentWithARInvoices(Core.Constants.ContainerModes.BuyersConsol, Core.Constants.ContainerModes.BuyersConsol, "SHIPMENT1", "invoice1");
			AssertEquals("Precondition: DSB Invoice doesn't have DEX event", false, DSBInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Precondition: Final Invoice doesn't have DEX Event", false, FinalInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Precondition: Final Invoice 1 doesn't have DEX Event", false, FinalInvoice1.Logs.Find(Query).Length > 0);
			TestDateAttribute.Date = currentDate;
			Exporter.ExportARInvoice();
			AssertEquals("DSB Invoice is not exported", false, DSBInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Final Invoice is not exported", false, FinalInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Final Invoice 1 is not exported", false, FinalInvoice1.Logs.Find(Query).Length > 0);
			AssertEquals("No Email is sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion
		#region TestExportARInvoiceForShipment
		[TestDate(2006, 12, 12, 12, 12, 0)]
		public void TestExportARInvoiceForShipmentfromLCLConsol()
		{
			DateTime currentDate = TestDateAttribute.Date;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			ForwardingShipment shipment = SetupShipmentWithARInvoices(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModes.LCL, "SHIPMENT", "invoice");
			AssertEquals("Precondition: DSB Invoice doesn't have DEX event", false, DSBInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Precondition: Final Invoice doesn't have DEX Event", false, FinalInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Precondition: Final Invoice 1 doesn't have DEX Event", false, FinalInvoice1.Logs.Find(Query).Length > 0);
			Exporter.ExportARInvoice();
			CombineAssertions(() =>
			{
				AssertContains("Notification should contain Invoice export log", "Invoices of Job Number", Buffer.AsString);
				AssertEquals("DSB Invoice have DEX event", true, DSBInvoice.Logs.Find(Query).Length > 0);
				AssertEquals("Final Invoice have DEX Event", true, FinalInvoice.Logs.Find(Query).Length > 0);
				AssertEquals("Final Invoice 1 have DEX Event", false, FinalInvoice1.Logs.Find(Query).Length > 0);
				AssertEquals("1 Email is sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				ZString expectedAttachmentName = "INV_200612121212_" + shipment.JobNumber + ".EDI";
				AssertExportResultEmailSent(email, expectedAttachmentName);
			});
			Env.OutgoingMailManager.EmailsCreated.Clear();
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(-4).ToDateTime();
			shipment = SetupShipmentWithARInvoices(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModes.LCL, "SHIPMENT1", "invoice1");
			AssertEquals("Precondition: DSB Invoice doesn't have DEX event", false, DSBInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Precondition: Final Invoice doesn't have DEX Event", false, FinalInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Precondition: Final Invoice 1 doesn't have DEX Event", false, FinalInvoice1.Logs.Find(Query).Length > 0);
			TestDateAttribute.Date = currentDate;
			Exporter.ExportARInvoice();
			AssertEquals("DSB Invoice is not exported", false, DSBInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Final Invoice is not exported", false, FinalInvoice.Logs.Find(Query).Length > 0);
			AssertEquals("Final Invoice 1 is not exported", false, FinalInvoice1.Logs.Find(Query).Length > 0);
			AssertEquals("No Email is sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion
		#region AssertExportResultEmailSent
		public void AssertExportResultEmailSent(EmailDef email, string attachmentFilename)
		{
			AssertNotNull("Email should not be null", email);
			AssertEquals("Email Subject", CLEDataRegistry.Instance.MattelEmailSubject, email.Subject);
			AssertEquals("Email Address", CLEDataRegistry.Instance.MattelEmailAddress, email.Recipients[0]);
			AssertEquals("Email Attachment Count", 1, email.Attachments.Count);
			AssertEquals("Email Attachment Name", email.Attachments[0].DisplayName, attachmentFilename);
			AssertEquals("Email/Reply To Address", CLEDataRegistry.Instance.ClemengerEmailAddress, email.FromAddress);
		}

		#endregion
		#region Implementation
		OrgHeader Importer, Supplier;
		NotificationBuffer Buffer;
		MattelARInvoiceExporterForTest Exporter;
		ARInvoice DSBInvoice, FinalInvoice, FinalInvoice1;
		readonly ZString Reference = "AR Invoice Export (Interchange: 0000001)";
		ZQuery Query
		{
			get
			{
				if (fQuery == null)
				{
					fQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
					fQuery.AddToFilter(StmALogSchema.SL_Table, AccTransactionHeaderSchema.Constants.TableName);
					fQuery.AddToFilter(StmALogSchema.SL_Reference, Reference);
				}

				return fQuery;
			}
		}

		ZQuery fQuery;
		protected override void SetUp()
		{
			base.SetUp();
			SetupOrganisation();
			CLEDataRegistry.Instance.ClemengerMailboxNumber = "123";
			CLEDataRegistry.Instance.MattelMailboxNumber = "456";
			CLEDataRegistry.Instance.MattelDebtor = Importer.PK.ToGuid();
			CLEDataRegistry.Instance.MattelEmailSubject = "Subject";
			CLEDataRegistry.Instance.MattelEmailAddress = "mattel@abc.com";
			CLEDataRegistry.Instance.ClemengerEmailAddress = "EmailAddress@CLE.com";
			GlbCompany.CurrentCompany.SetCountry("AU");
			Buffer = new NotificationBuffer();
			Exporter = new MattelARInvoiceExporterForTest(Buffer, Factory);
		}

		#region SetupOrganisation
		void SetupOrganisation()
		{
			Supplier = Factory.New<OrgHeader>();
			Supplier.OH_FullName = "Supplier";
			Supplier.OH_Code = "Supp";
			Supplier.OH_IsConsignor = true;
			Supplier.OH_RL_NKClosestPort = "NZAKL";
			Importer = Factory.New<OrgHeader>();
			Importer.OH_FullName = "Importer";
			Importer.OH_Code = "Import";
			Importer.OH_IsConsignee = true;
			Importer.OH_IsDebtor = true;
			Importer.OH_RL_NKClosestPort = "AUSYD";
		}

		#endregion
		#region SetupShipmentWithARInvoices
		ForwardingShipment SetupShipmentWithARInvoices(ZString consolMode, ZString packingMode, ZString jobNumber, ZString invoiceNum)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = consolMode;
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_RL_NKLoadPort = "NZAKL";
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = jobNumber;
			shipment.ConsigneePK = Importer.PK;
			shipment.ConsignorPK = Supplier.PK;
			shipment.JS_PackingMode = packingMode;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_ActualVolume = 100m;
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = jobNumber;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			Factory.Save();
			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_JobNum = jobNumber;
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			DSBInvoice = CreateInvoice(header.PK, true, Importer.PK, invoiceNum + "1", 1000m, 0m, false);
			FinalInvoice = CreateInvoice(header.PK, false, Importer.PK, invoiceNum + "2", 2000m, 0m, false);
			FinalInvoice1 = CreateInvoice(header.PK, false, Importer.PK, invoiceNum + "3", 3000m, 0m, true);
			Factory.Save();
			return shipment;
		}

		#endregion
		ARInvoice CreateInvoice(ZGuid headerPK, bool isDisbursement, ZGuid debtorPK, ZString invoiceNo, ZDecimal amount, ZDecimal tax, bool isCancelled)
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_JH = headerPK;
			if (isDisbursement)
			{
				invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			}

			invoice.AH_OH = debtorPK;
			invoice.AH_TransactionNum = invoiceNo;
			invoice.AH_InvoiceAmount = amount;
			invoice.AH_OutstandingAmount = amount;
			invoice.AH_IsCancelled = isCancelled;
			if (isCancelled)
			{
				((IMatching)invoice).CurrentMatchGroup.AddNew().AP_AH = invoice.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(invoice);
			}

			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GSTAmount = tax;
			invoice.AH_OSTotal = amount + tax;
			Factory.Save();
			return invoice;
		}

		class MattelARInvoiceExporterForTest : MattelARInvoiceExporter
		{
			public MattelARInvoiceExporterForTest(INotifications buffer, BusinessObjectFactory factory) : base(buffer, factory)
			{
				ThrowException = false;
			}

			public bool ThrowException;
			protected override void GenerateMessageFile(JobInvoiceRecord record)
			{
				if (!ThrowException)
				{
					base.GenerateMessageFile(record);
				}
				else
				{
					throw new Exception();
				}
			}
		}
		#endregion
	}
}
