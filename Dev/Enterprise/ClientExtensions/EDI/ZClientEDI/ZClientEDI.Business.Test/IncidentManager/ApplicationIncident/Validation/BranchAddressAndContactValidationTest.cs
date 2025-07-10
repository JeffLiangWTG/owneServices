using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class BranchAddressAndContactValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIM_OC_Contact()
		{
			AssertNoErrors("Precondition: IM_OC_Contact should not have errors.", BizObj.IM_OC_ContactInfo);

			BizObj.IM_OC_Contact = ZGuid.Empty;
			AssertHasError(BizObj.IM_OC_ContactInfo, "Please enter a Contact.");

			BizObj.IM_OC_Contact = ZGuid.NewZGuid();
			AssertNoErrors(BizObj.IM_OC_ContactInfo);
		}

		public void TestCheckIM_OA_BranchAddress()
		{
			AssertNoErrors("Precondition: IM_OA_BranchAddress should not have errors.", BizObj.IM_OA_BranchAddressInfo);

			BizObj.IM_OA_BranchAddress = ZGuid.Empty;
			AssertHasError(BizObj.IM_OA_BranchAddressInfo, "Please enter a Branch Address.");

			OrgAddress address = Factory.New<OrgAddress>();
			BizObj.IM_OA_BranchAddress = address.PK;
			AssertNoErrors(BizObj.IM_OA_BranchAddressInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBizObj();
		}

		protected virtual ProfessionalServicesQuote GetNewBizObj()
		{
			return Factory.New<DummyIncidentMain>();
		}

		protected ProfessionalServicesQuote BizObj;

		#region class DummyIncidentMain

		class DummyIncidentMain : ProfessionalServicesQuote
		{
			public DummyIncidentMain(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override IncidentMainValidation GetNewValidation()
			{
				IncidentMainValidation result = base.GetNewValidation();
				result.Add(new BranchAddressAndContactValidation(this));
				return result;
			}

			protected override string GetNewIncidentNumber()
			{
				return "TEST123";
			}
		}

		#endregion

		#endregion
	}
}
