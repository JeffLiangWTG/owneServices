using System;
using System.Linq;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDIInterchangeFilterBusinessObject))]
	sealed class EDIInterchangeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIInterchangeFilterBusinessObject();
		}

		public void TestFilterByFromOrTo()
		{
			int interchangeStartCount = Factory.GetDatabaseCount(typeof(EDIInterchange));

			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2");
			testInterchange1.EI_From = "EDIEDITST";
			testInterchange1.EI_To = "TSTEDIEDI";

			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange2.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDa0");
			testInterchange2.EI_From = "TSTEDIEDI";
			testInterchange2.EI_To = "EDIEDITST";
			Factory.Save();

			EDIInterchangeFilterBusinessObject interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)interchangeFilterBusinessObject["Sender"];
			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Sender StartsWith 'EDI' should be 1", 1, interchanges.Length);
				Assert("Sender StartsWith 'EDI' Should contain testInterchane1", interchanges.Contains<EDIInterchange>(testInterchange1));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Sender Contains 'EDI' should be 2", 2, interchanges.Length);
				Assert("Sender Contains 'EDI' Should contain testInterchane1", interchanges.Contains<EDIInterchange>(testInterchange1));
				Assert("Sender Contains 'EDI' Should contain testInterchane2", interchanges.Contains<EDIInterchange>(testInterchange2));
			}

			filter = (ModuleTextFilter)interchangeFilterBusinessObject["Receiver"];
			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Receiver StartsWith 'EDI' should be 1", 1, interchanges.Length);
				Assert("Receiver StartsWith 'EDI' Should contain testInterchane2", interchanges.Contains<EDIInterchange>(testInterchange2));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Receiver Contains 'EDI' should be 2", 2, interchanges.Length);
				Assert("Receiver Contains 'EDI' Should contain testInterchane1", interchanges.Contains<EDIInterchange>(testInterchange1));
				Assert("Receiver Contains 'EDI' Should contain testInterchane2", interchanges.Contains<EDIInterchange>(testInterchange2));
			}
		}

		public void TestFilterByEHubID()
		{
			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2");

			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange2.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDa0");
			Factory.Save();

			var interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			var filter = (EDIInterchangeEHubIdFilter)interchangeFilterBusinessObject[EDIInterchangeFilterBusinessObject.Descriptions.eHubFilter];
			{
				filter.Property = "C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDa0";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Exact positive", 1, interchanges.Length);
			}

			{
				filter.Property = "C94DC724-4943-47E1-AB1A-E0F8196D5F4D";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Exact negative", 0, interchanges.Length);
			}
		}

		public void TestFilterByBodyText()
		{
			int interchangeStartCount = Factory.GetDatabaseCount(typeof(EDIInterchange));

			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;
			testInterchange1.ForceDeprecatedNTextUsageForTesting = true;
			testInterchange1.EI_BodyNText = "我们";

			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange2.EI_ApplicationCode = EDIInterchange.ApplicationCodes.AirCargo;
			testInterchange2.EI_BodyText = "us";
			Factory.Save();

			EDIInterchangeFilterBusinessObject interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			EDIInterchangeTextFilter filter = (EDIInterchangeTextFilter)interchangeFilterBusinessObject["Body Text"];
			var subGroup = (ModuleFilterSubGroup)filter.SubGroup;
			{
				filter.Property = "我";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("StartsWith positive", 1, interchanges.Length);
			}

			{
				filter.Property = "u";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("StartsWith positive", 1, interchanges.Length);
			}

			{
				filter.Property = "";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("StartsWith empty", interchangeStartCount + 2, interchanges.Length);
			}

			{
				filter.Property = "我们";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Exact positive", 1, interchanges.Length);
			}

			{
				filter.Property = "us";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Exact negative", 1, interchanges.Length);
			}

			{
				filter.Property = "们";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Contains positive", 1, interchanges.Length);
			}

			{
				filter.Property = "XYZ";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Contains negative", 0, interchanges.Length);
			}

			{
				filter.Property = "u";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Not starts with", 1, interchanges.Length);
			}

			{
				filter.Property = "s";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Not contains", 1, interchanges.Length);
			}

			{
				filter.Property = "us";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Not equals", 1, interchanges.Length);
			}
		}

		public void TestFilterByHeaderText()
		{
			int interchangeStartCount = Factory.GetDatabaseCount(typeof(EDIInterchange));

			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;
			testInterchange1.EI_HeaderText = "我们";

			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange2.EI_ApplicationCode = EDIInterchange.ApplicationCodes.AirCargo;
			testInterchange2.EI_HeaderText = "us";
			Factory.Save();

			EDIInterchangeFilterBusinessObject interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)interchangeFilterBusinessObject["Header Text"];
			{
				filter.Property = "我";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("StartsWith positive", 1, interchanges.Length);
			}

			{
				filter.Property = "u";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("StartsWith positive", 1, interchanges.Length);
			}

			{
				filter.Property = "";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("StartsWith empty", interchangeStartCount + 2, interchanges.Length);
			}

			{
				filter.Property = "我们";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Exact positive", 1, interchanges.Length);
			}

			{
				filter.Property = "us";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Exact negative", 1, interchanges.Length);
			}

			{
				filter.Property = "们";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Contains positive", 1, interchanges.Length);
			}

			{
				filter.Property = "XYZ";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(filter.Query);
				AssertEquals("Contains negative", 0, interchanges.Length);
			}
		}

		public void TestFilterByStatus()
		{
			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2");
			testInterchange1.EI_Status = EDIInterchange.Status.eHubPending;

			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange2.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDa0");
			testInterchange2.EI_Status = EDIInterchange.Status.eHubQueued;
			Factory.Save();

			EDIInterchangeFilterBusinessObject interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			ModuleFilterWithList filter = (ModuleFilterWithList)interchangeFilterBusinessObject["Status"];

			AssertEquals("Filter should have 14 items in the list", 14, filter.List.Count);

			string temp = "|";
			foreach (var fil in filter.List)
			{
				temp += ((Enterprise.ZArchitecture.Core.CodeDescriptionPair)fil).Code + "|";
			}
			AssertEquals("filters should contain HQU", true, temp.Contains("|HQU|"));
			AssertEquals("filters should contain HPN", true, temp.Contains("|HPN|"));
			AssertEquals("filters should contain DCD", true, temp.Contains("|DCD|"));
		}

		public void TestFilterWithSYS()
		{
			UserForTest user = new UserForTest();
			UserContext context = new UserContextForTest(user, Env.CurrentCompany);

			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2");
			testInterchange1.EI_From = "EDIEDITST";
			testInterchange1.EI_To = "TSTEDIEDI";

			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange2.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDa0");
			testInterchange2.EI_From = "TSTEDIEDI";
			testInterchange2.EI_To = "EDIEDITST";
			testInterchange2.EI_ApplicationCode = ApplicationCodeList.Codes.SYS;

			EDIInterchangeFilterBusinessObject interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();

			user.LoggedInWithMasterPassword = false;
			using (Env.SetTemporaryUserContext(context))
			{
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(interchangeFilterBusinessObject.Filter);
				AssertEquals("Only interchange1 (non SYS)", 1, interchanges.Length);
				Assert("Must contain interchange1", interchanges.Any(x => x.EI_SessionGUID == testInterchange1.EI_SessionGUID));
			}

			user.LoggedInWithMasterPassword = true;
			using (Env.SetTemporaryUserContext(context))
			{
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(interchangeFilterBusinessObject.Filter);
				AssertEquals("interchanges", 2, interchanges.Length);
				Assert("Must contain interchange1", interchanges.Any(x => x.EI_SessionGUID == testInterchange1.EI_SessionGUID));
				Assert("Must contain interchange2", interchanges.Any(x => x.EI_SessionGUID == testInterchange2.EI_SessionGUID));
			}
		}

		public void TestUpdateComparisionOperator()
		{
			EDIInterchangeFilterBusinessObject filterBusinessObject = new EDIInterchangeFilterBusinessObject();
			ModuleTextFilter filter = new ModuleFilterCollection().AddNumberFilter("Test", EDIMessageSchema.EM_MessageNum);

			AssertEquals("PRE: There are 8 comparison operators originally", 8, filter.ComparisonOperator_List.Count);

			filterBusinessObject.UpdateComparisionOperator(filter);

			AssertEquals("There should be only 1 comparison operators after update", 1, filter.ComparisonOperator_List.Count);
			Assert("Comparison operators", filter.ComparisonOperator_List.ContainsOnly("exact"));
		}

		public void TestFilterByTransportType()
		{
			var interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			var filter = (ModuleTextFilter)interchangeFilterBusinessObject["Transport Type"];
			AssertNotNull("Filter is present", filter);

			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			Factory.Save();

			EDIInterchange[] interchanges = null;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			{
				testInterchange1.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				testInterchange2.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;

				filter.Property = EDIInterchangeTransportTypeList.Codes.eHub;
				interchanges = Factory.Load<EDIInterchange>(filter.Query);

				AssertEquals("Contains 2 elements", 2, interchanges.Length);
			}
			{
				testInterchange1.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
				testInterchange2.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;

				filter.Property = EDIInterchangeTransportTypeList.Codes.eHub;
				interchanges = Factory.Load<EDIInterchange>(filter.Query);

				AssertEquals("Contains 0 element", 0, interchanges.Length);
			}
			{
				testInterchange1.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				testInterchange2.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;

				filter.Property = EDIInterchangeTransportTypeList.Codes.eHub;
				interchanges = Factory.Load<EDIInterchange>(filter.Query);

				AssertEquals("Contains 1 element", 1, interchanges.Length);
			}
			{
				testInterchange1.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
				testInterchange2.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;

				filter.Property = EDIInterchangeTransportTypeList.Codes.eAdaptor;
				interchanges = Factory.Load<EDIInterchange>(filter.Query);

				AssertEquals("Contains 2 element", 2, interchanges.Length);
			}
			{
				testInterchange1.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				testInterchange2.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;

				filter.Property = EDIInterchangeTransportTypeList.Codes.eAdaptor;
				interchanges = Factory.Load<EDIInterchange>(filter.Query);

				AssertEquals("Contains 0 elements", 0, interchanges.Length);
			}
			{
				testInterchange1.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				testInterchange2.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;

				filter.Property = EDIInterchangeTransportTypeList.Codes.eAdaptor;
				interchanges = Factory.Load<EDIInterchange>(filter.Query);

				AssertEquals("Contains 1 element", 1, interchanges.Length);
			}
		}
	}
}
