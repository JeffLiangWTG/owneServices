using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class OrgBatchListenerTest : NavisionBatchListenerTestCase
	{
		public void TestEventIsAddOrEdit()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org.OH_IsDebtor = true;
			StmALog log = org.Logs.AddNew(Events.ServiceRequested);
			Factory.Save();
			Assert("Should be false, event is not add or edit", !Listener.AdditionalMatching(log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Assert("Should be true when event is EDT", Listener.AdditionalMatching(log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Assert("Should be true when event is ADD", Listener.AdditionalMatching(log));
		}

		public void TestOrgIsDebtor()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org.OH_IsDebtor = false;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog log = org.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			Assert("Should be false when Org is not a debtor", !Listener.AdditionalMatching(log));
			org.OH_IsDebtor = true;
			Factory.Save();
			Assert("Should be true when Org is a debtor", Listener.AdditionalMatching(log));
		}

		public void TestOrgHeaderIsCorrectlyLoaded()
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsDebtor = true;
			Factory.Save();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog log = org.CompanyData.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			Assert("valid events from the OrgCompanyData should match", Listener.AdditionalMatching(log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.MainAddress.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			Assert("valid events from the OrgAddress should match", Listener.AdditionalMatching(log));
			OrgStaffAssignments staffAssignment = org.StaffAssignments.AddNew();
			staffAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = staffAssignment.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			Assert("valid events from the StaffAssignments should match", Listener.AdditionalMatching(log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			Assert("valid events from the OrgHeader should match", Listener.AdditionalMatching(log));
		}

		[TestDate(2006, 5, 18, 14, 7, 32)]
		public void TestShouldNotMatchIfOrgHasADataExportEvent()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org.OH_IsDebtor = true;
			StmALog log = org.Logs.AddNew(Events.DataExport);
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2006, 5, 18, 15, 0, 0);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.CompanyData.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			Assert("Should match because DEX event is before the EDT event", Listener.AdditionalMatching(log));
			TestDateAttribute.Date = new DateTime(2006, 5, 18, 16, 42, 59);
			log = org.Logs.AddNew(Events.DataExport);
			Factory.Save();
			Assert("should NOT match because DEX is after the EDT event", !Listener.AdditionalMatching(log));
		}

		[ExpectNoExceptions("If Org is Null, result should be false")]
		public void TestNullOrganisationIsLoaded()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org.OH_IsDebtor = true;
			OrgAddress address = org.Addresses.AddNew();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog log = address.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			address.Delete();
			Factory.Save();
			Assert("Should not match, Organisation should be null, hence Result should be false", !Listener.AdditionalMatching(log));
		}

		public void TestMatchTableName()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var log = orgHeader.Logs.AddNew();
			Assert("Should match OrgHeader", Listener.MatchTableName(log));
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			log = orgCompanyData.Logs.AddNew();
			Assert("Should match OrgCompanyData", Listener.MatchTableName(log));
			var orgStaffAssignments = Factory.NewWithValidTestData<OrgStaffAssignments>();
			log = orgStaffAssignments.Logs.AddNew();
			Assert("Should match OrgStaffAssignments", Listener.MatchTableName(log));
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			log = orgAddress.Logs.AddNew();
			Assert("Should match OrgAddress", Listener.MatchTableName(log));
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			log = dummy.GetLogs().AddNew();
			Assert("Anything but, OrgHeader, OrgCompanyData, OrgStaffAssignments, OrgAddress and OrgMiscServ sould be false", !Listener.MatchTableName(log));
		}

		[TestDate(2006, 5, 18, 16, 27, 53)]
		public void TestOrgIsExportedToFileWhenSourceEventIsNotOrgHeader()
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABIGAS"));
			org.Addresses.MainAddress.OA_Address2 = "";
			org.Addresses.MainAddress.OA_State = "QLD";
			org.Addresses.MainAddress.OA_PostCode = "4006";
			org.Addresses.MainAddress.OA_City = "MAYNE";
			org.MiscServ.OM_OJ_ARDebtorGroup = new ZGuid();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			org.CompanyData.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			ZQuery filter = new ZQuery(OrgRelatedPartySchema.PR_FreightDirection, "AR");
			filter.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, org.PK);
			OrgRelatedParty relatedParty = Factory.LoadTop1<OrgRelatedParty>(filter);
			if (relatedParty != null)
			{
				relatedParty.Delete();
			}

			Factory.Save();
			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			StmALog[] logs = (StmALog[])org.Logs.GetAllLogs().Find(query);
			AssertEquals("Pre-Condition: Should be no DEX Logs", 0, logs.Length);
			TestDateAttribute.Date = new DateTime(2006, 5, 18, 17, 45, 31);
			STIDataRegistry.Instance.DataExportHighWaterMark = new ZDateTime(2006, 5, 18, 15, 0, 0);
			STIDataRegistry.Instance.OrganisationExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.ShipmentExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.InvoiceHeaderExportDirectory = Env.TempPath;
			STIDataRegistry.Instance.InvoiceLinesExportDirectory = Env.TempPath;
			SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2006, 5, 18, 14, 0, 0));
			string fileName = ZDateTime.UtcNow.ToLocalBranchTime().ToString(Constants.DateTimeFileNameFormat);
			ZString exportedFile = Path.Combine(Env.TempPath, Constants.FileNamePrefixes.OrganisationFiles + fileName) + ".csv";
			DeleteIfExists(exportedFile);
			try
			{
				new NavisionServiceTask()
				{ ServiceLogger = new TestServiceLogger() }.RunTask();
				Assert("File should exist", File.Exists(exportedFile));
				AssertASCIIFileSameAsString(exportedFile, "ABIGAS,ABI GAS & TOOLS,ABI GAS & TOOLS,,171 ABBOTSFORD ROAD,,MAYNE,,,,,2000.00,,,COD0,,FOB,,AU,,,ABIGAS,,,,4006,QLD,,,No GST,,,");
				org.Logs.GetAllLogs().Load();
				logs = (StmALog[])org.Logs.GetAllLogs().Find(query);
				AssertEquals("Should be 1 Log", 1, logs.Length);
			}
			finally
			{
				DeleteIfExists(exportedFile);
			}
		}

		#region Overrides
		protected override NavisionBatchListener BatchListener
		{
			get
			{
				return new OrgBatchListenerTestClass();
			}
		}

		protected override Type BusinessObjectCollectionType
		{
			get
			{
				return typeof(OrgHeaderCollection);
			}
		}

		protected override string BusinessObjectTableName
		{
			get
			{
				return OrgHeaderSchema.Constants.TableName;
			}
		}

		protected override Type BusinessObjectType
		{
			get
			{
				return typeof(OrgHeader);
			}
		}

		protected override string ExportDirectory
		{
			get
			{
				return STIDataRegistry.Instance.OrganisationExportDirectory;
			}

			set
			{
				STIDataRegistry.Instance.OrganisationExportDirectory = value;
			}
		}

		protected override Type ExporterType
		{
			get
			{
				return typeof(OrgFlatFileExporter);
			}
		}

		protected override string FileNamePreFix
		{
			get
			{
				return Constants.FileNamePrefixes.OrganisationFiles;
			}
		}

		protected override BusinessObject BusinessObjectForTesting()
		{
			return NavisionTestHelper.OrgForTesting(Factory);
		}

		#endregion
		#region Setup
		OrgBatchListenerTestClass Listener;
		protected override void SetUp()
		{
			base.SetUp();
			Listener = new OrgBatchListenerTestClass();
			STIDataRegistry.Instance.ExportingOrganisationsForTheFirstTime = false;
		}

		class OrgBatchListenerTestClass : OrgBatchListener, NavisionBatchListenerTest.IBatchListenerTestClass
		{
			public OrgBatchListenerTestClass() : base(ZDateTime.Now)
			{
			}

			public new void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications)
			{
				base.Process(matchingBusinessObject, log, notifications);
			}

			public Type BusinessObjectCollectionTypeExposed
			{
				get
				{
					return base.BusinessObjectCollectionType;
				}
			}

			public NavisionFlatFileExporter ExporterExposed
			{
				get
				{
					return base.Exporter;
				}
			}

			public new bool AdditionalMatching(StmALog log)
			{
				return base.AdditionalMatching(log);
			}

			public new bool MatchTableName(StmALog log)
			{
				return base.MatchTableName(log);
			}
		}
		#endregion
	}
}
