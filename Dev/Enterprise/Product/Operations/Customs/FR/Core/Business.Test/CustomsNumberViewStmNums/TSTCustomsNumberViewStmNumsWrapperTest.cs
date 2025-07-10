using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CustomsNumberViewStmNums.Testing
{
	[TestedType(typeof(TSTCustomsNumberViewStmNumsWrapper))]
	public class TSTCustomsNumberViewStmNumsWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSN_FountainName()
		{
			AssertEquals("Prefix", DataBoundResourceStrings.GetDataForProperty(wrapper.SN_FountainNameInfo).Caption);
			wrapper.SN_FountainName = "ItsMyDDT";
			Factory.Save();
			AssertEquals("ItsMyDDT|1", wrapper.StmNums.SN_Prefix);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return wrapper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var authorisation = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;
			authorisation.CPH_Number = "TS000040";
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			var provider = authorisation.CustomsNumberProvider;
			var stmNums = provider.CustomsNumbers.AddNew();
			wrapper = (TSTCustomsNumberViewStmNumsWrapper)stmNums.Wrapper;
		}

		TSTCustomsNumberViewStmNumsWrapper wrapper;
	}
}
