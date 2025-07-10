using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CustomsNumberViewStmNums.Testing
{
	[TestedType(typeof(TSTCustomsNumberViewStmNumsWrapperCollection))]
	public class TSTCustomsNumberViewStmNumsWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TSTCustomsNumberViewStmNumsWrapperCollection>
	{
		protected override TSTCustomsNumberViewStmNumsWrapperCollection GetCollectionToTest()
		{
			return (TSTCustomsNumberViewStmNumsWrapperCollection)provider.CustomsNumberWrappers;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var stmNum = provider.CustomsNumbers.AddNew();
			return provider.GetOrCreateWrapper(stmNum);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var authorisation = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;
			authorisation.CPH_Number = "TS000040";
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			provider = authorisation.CustomsNumberProvider;
		}

		CustomsNumberViewStmNumsBusinessProvider provider;
	}
}
