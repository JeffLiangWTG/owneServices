using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CreditControlledDocumentsApprovalFilterBusinessObject))]
	public class CreditControlledDocumentsApprovalFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestApprovalDateQuery()
		{
			SetupSimpleMultiRequestScenario();

			req_simp_shipment1.XP_ApprovalDate = new ZDateTime(2014, 03, 20);
			req_simp_shipment2.XP_ApprovalDate = new ZDateTime(2014, 03, 21);
			req_simp_shipment3.XP_ApprovalDate = new ZDateTime(2014, 03, 22);

			Factory.Save();

			var filter = GetFilter<ModuleDateFilter>("Approval Date");
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2014, 03, 18);
			filter.Property2 = new ZDateTime(2014, 03, 20);
			filter.IsActive = true;

			var requests = Factory.Load<GenApprovalRequest>(filter.Query);
			AssertEquals("Finds all between 18th and 20th", 1, requests.Length);
			Assert("Finds all between 18th and 20th", requests.Contains(req_simp_shipment1));

			filter.Property2 = new ZDateTime(2014, 03, 21);
			requests = Factory.Load<GenApprovalRequest>(filter.Query);
			AssertEquals("Finds all between 18th and 21st", 2, requests.Length);
			Assert("Finds all between 18th and 21st", requests.Contains(req_simp_shipment1));
			Assert("Finds all between 18th and 21st", requests.Contains(req_simp_shipment2));
		}

		public void TestApprovingUserQuery()
		{
			var user1 = Factory.New<GlbStaff>();
			user1.GS_Code = "U1";
			user1.GS_LoginName = "User1";
			var user2 = Factory.New<GlbStaff>();
			user2.GS_Code = "U2";
			user2.GS_LoginName = "User2";
			var user3 = Factory.New<GlbStaff>();
			user3.GS_Code = "U3";
			user3.GS_LoginName = "User3";

			SetupSimpleMultiRequestScenario();

			req_simp_shipment1.XP_GS_NKApprovingUser1 = user1.GS_Code;
			req_simp_shipment2.XP_GS_NKApprovingUser1 = user2.GS_Code;
			req_simp_shipment3.XP_GS_NKApprovingUser1 = user3.GS_Code;

			Factory.Save();

			var filter = GetFilter<ModuleNkFilter>("Approving User");
			filter.Property = "U1";
			filter.IsActive = true;

			var requests = Factory.Load<GenApprovalRequest>(filter.Query);
			AssertEquals("Finds U1", 1, requests.Length);
			Assert("Finds U1", requests.Contains(req_simp_shipment1));
		}

		public void TestApprovalStatusQuery()
		{
			SetupSimpleMultiRequestScenario();

			req_simp_shipment1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			req_simp_shipment2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			req_simp_shipment3.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;

			Factory.Save();

			var filter = GetFilter<ModuleTextFilter>("Approval Status");
			filter.Property = Constants.GenApprovalRequestApprovalStatus.Requested;
			filter.IsActive = true;

			var requests = Factory.Load<GenApprovalRequest>(filter.Query);
			AssertEquals("Finds Requested", 1, requests.Length);
			Assert("Finds Requested", requests.Contains(req_simp_shipment3));
		}

		public void TestApprovalRequiredQuery()
		{
			SetupSimpleMultiRequestScenario();

			req_simp_shipment1.XP_PrivledgeRequired = "1";
			req_simp_shipment2.XP_PrivledgeRequired = "1";
			req_simp_shipment3.XP_PrivledgeRequired = "3";

			Factory.Save();

			var filter = GetFilter<ModuleTextFilter>("Approval Required");
			filter.Property = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			filter.IsActive = true;

			var requests = Factory.Load<GenApprovalRequest>(filter.Query);
			AssertEquals("Finds Requested", 2, requests.Length);
			Assert("Finds Requested", requests.Contains(req_simp_shipment1));
			Assert("Finds Requested", requests.Contains(req_simp_shipment2));

			filter = GetFilter<ModuleTextFilter>("Approval Required");
			filter.Property = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			filter.IsActive = true;

			requests = Factory.Load<GenApprovalRequest>(filter.Query);
			AssertEquals("Finds Requested", 0, requests.Length);

			filter = GetFilter<ModuleTextFilter>("Approval Required");
			filter.Property = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			filter.IsActive = true;

			requests = Factory.Load<GenApprovalRequest>(filter.Query);
			AssertEquals("Finds Requested", 1, requests.Length);
			Assert("Finds Requested", requests.Contains(req_simp_shipment3));
		}

		#region Implementation

		BusinessObject simp_shipment1;
		BusinessObject simp_shipment2;
		BusinessObject simp_shipment3;
		GenApprovalRequest req_simp_shipment1;
		GenApprovalRequest req_simp_shipment2;
		GenApprovalRequest req_simp_shipment3;

		void SetupSimpleMultiRequestScenario()
		{
			simp_shipment1 = CreateShipment();
			simp_shipment2 = CreateShipment();
			simp_shipment3 = CreateShipment();
			req_simp_shipment1 = WrapShipmentInRequest(simp_shipment1);
			req_simp_shipment2 = WrapShipmentInRequest(simp_shipment2);
			req_simp_shipment3 = WrapShipmentInRequest(simp_shipment3);
		}

		protected virtual GenApprovalRequest WrapShipmentInRequest(BusinessObject shipment)
		{
			var request = Factory.NewWithValidTestData<GenApprovalRequest>();
			request.XP_ParentID = shipment.PK;
			request.XP_ParentTableCode = shipment.TablePrefix;
			return request;
		}
		int nextShipmentNumber = 1;

		BusinessObject CreateShipment()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = (nextShipmentNumber++).ToString();
			return shipment;
		}

		T GetFilter<T>(string name) where T : ModuleFilter
		{
			return (T)new CreditControlledDocumentsApprovalFilterBusinessObject().ModuleFilters[name];
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CreditControlledDocumentsApprovalFilterBusinessObject();
		}

		#endregion
	}
}
