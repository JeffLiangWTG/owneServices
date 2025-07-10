using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.SWT.Testing
{
	class BatchReportRunnerTest : TestCaseWithFactory
	{
		public void TestRunBatchReports()
		{
			int printJobCount = Factory.GetDatabaseCount(typeof(StmPrintJob));
			AssertEquals("Precondition", 0, printJobCount);
			TestCaseHelper.RunClientDbCreateScripts();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			NotificationBuffer buffer = new NotificationBuffer();
			BatchReportRunner reportRunner = GetReportRunner();
			reportRunner.InternalRunBatchReportsTest(buffer);
			AssertEquals("Message when no data", true, buffer.AsString.Contains("No data has been found to run the 'Not Yet Arrived' report"));
			Shipment.Consols.Add(Consol);
			Factory.Save();
			buffer.Clear();
			reportRunner = GetReportRunner();
			reportRunner.InternalRunBatchReportsTest(buffer);
			printJobCount = Factory.GetDatabaseCount(typeof(StmPrintJob));
			AssertPrintJobCreated(printJobCount);
			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertPrintJob(printJob);
			consol.Transports[0].JW_ETA = ZDateTime.Empty;
			consol.Transports[0].JW_ATD = ZDateTime.Empty;
			Factory.Save();
			buffer.Clear();
			reportRunner = GetReportRunner();
			reportRunner.InternalRunBatchReportsTest(buffer);
			int printJobCountaferReportRun = Factory.GetDatabaseCount(typeof(StmPrintJob));
			AssertEquals("No print job should have been created", printJobCount, printJobCountaferReportRun);
		}

		protected virtual void AssertPrintJobCreated(int printJobCount)
		{
			AssertEquals("One print job should have been created", 2, printJobCount);
		}

		protected virtual BatchReportRunner GetReportRunner()
		{
			return new BatchReportRunnerForTesting();
		}

		protected virtual void AssertPrintJob(StmPrintJob printJob)
		{
			AssertEquals("Job type should have been email", nameof(PrintType.EML), printJob.SP_JobType);
		}

		protected ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Shipment.Consols.AddNew();
					Transport transportLeg = consol.Transports[0];
					transportLeg.JW_IsLinked = false;
					transportLeg.JW_Vessel = "Vessel";
					transportLeg.JW_RL_NKLoadPort = "NZAKL";
					transportLeg.JW_RL_NKDiscPort = "AUBNE";
					transportLeg.JW_ETA = CurrentDate.AddDays(2);
					transportLeg.JW_ETD = CurrentDate.AddDays(-3);
					transportLeg.JW_ATD = CurrentDate.AddDays(-2);
				}

				return consol;
			}
		}

		ForwardingConsol consol;
		protected ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
					shipment.JS_RL_NKOrigin = "NZAKL";
					shipment.JS_RL_NKDestination = "AUBNE";
					shipment.ConsigneePK = Consignee.PK;
					shipment.ConsignorPK = Consignor.PK;
					shipment.JS_E_ARV = CurrentDate.AddDays(3);
					shipment.JS_E_DEP = CurrentDate.AddDays(-2);
					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				}

				return shipment;
			}
		}

		ForwardingShipment shipment;
		OrgHeader Consignee
		{
			get
			{
				if (consignee == null)
				{
					consignee = Factory.NewWithValidTestData<OrgHeader>();
					consignee.OH_RL_NKClosestPort = "AUBNE";
					OrgContact contact = consignee.Contacts.AddNew();
					contact.OC_ContactName = "Contact1";
					contact.OC_Email = "contact@test.com";
					OrgDocument docContact = contact.Documents.AddNew();
					docContact.OD_DocumentGroup = ContactType.Consignee.Code;
					docContact.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
					docContact.OD_AttachmentType = OrgConstants.AttachmentType.XLS;
					contact = consignee.Contacts.AddNew();
					contact.OC_ContactName = "Contact2";
					contact.OC_Email = "contact2@test.com";
					docContact = contact.Documents.AddNew();
					docContact.OD_DocumentGroup = ContactType.Warehouse3PL.Code;
					docContact.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
					docContact.OD_AttachmentType = OrgConstants.AttachmentType.XLS;
				}

				return consignee;
			}
		}

		OrgHeader consignee;
		OrgHeader Consignor
		{
			get
			{
				return consignor ?? (consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "NZAKL")));
			}
		}

		OrgHeader consignor;
		readonly ZDateTime CurrentDate = ZDateTime.Now;
		class BatchReportRunnerForTesting : BatchReportRunner
		{
			protected override ReportCommand GetReportCommand()
			{
				return FactoryProvider.Current.Load<ReportCommand>(new ZGuid("B244B421-D343-407A-BD09-3EF80EDB4C6B"));
			}
		}
	}
}
