using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CustomsNumberViewStmNums.Testing
{
	[TestedType(typeof(TSTCustomsNumberViewStmNumsSetting))]
	public class TSTCustomsNumberViewStmNumsSettingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTypeRangeMax()
		{
			AssertEquals(999999L, setting.DefaultTypeRangeMax());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return setting;
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
			setting = (TSTCustomsNumberViewStmNumsSetting)stmNums.Setting;
		}

		TSTCustomsNumberViewStmNumsSetting setting;
	}
}
