using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Testing
{
	[TestedType(typeof(TSTCustomsNumberViewStmNumsAuthorisationProvider))]
	class TSTCustomsNumberViewStmNumsAuthorisationProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetEditorForm()
		{
			var stmNum = provider.CustomsNumbers.AddNew();
			var wrapper = stmNum.Wrapper;
			using (var editForm = provider.GetEditorForm(wrapper))
			{
				AssertType<TSTCustomsNumberViewStmNumsEditorForm>(editForm);
			}
		}

		public void TestGetUserControl()
		{
			using (var userControl = provider.GetUserControl())
			{
				AssertType<TSTCustomsNumberViewStmNumsUserControl>(userControl);
			}
		}

		public void TestCreateNewStmNums()
		{
			var stmNums = provider.CustomsNumbers.AddNew();
			Factory.Save();
			AssertEquals(true, stmNums.SN_Name.StartsWith("C#T1"));
		}

		public void TestParent()
		{
			AssertEquals(authorisation, provider.Parent);
		}

		public void TestProviderKey()
		{
			AssertEquals(CusAuthorisationHeaderCustomsNumberProviderKeyList.Codes.TemporaryStorage, provider.ProviderKey);
		}

		public void TestLookups()
		{
			var stmNum = provider.CustomsNumbers.AddNew();
			AssertType<TSTCustomsNumberViewStmNumsLookups>(stmNum.Lookups);
		}

		public void TestTypeList()
		{
			var stmNum = provider.CustomsNumbers.AddNew();
			AssertEquals("DDT", stmNum.Lookups.TypeList.CodesAsString);
		}

		public void TestUserControl()
		{
			using (var control = provider.GetUserControl())
			{
				AssertType<TSTCustomsNumberViewStmNumsUserControl>(control);
			}
		}

		public void TestEditorForm()
		{
			var stmNum = provider.CustomsNumbers.AddNew();
			var wrapper = stmNum.Wrapper;
			using (var form = provider.GetEditorForm(wrapper))
			{
				AssertType<TSTCustomsNumberViewStmNumsEditorForm>(form);
			}
		}

		public void TestSettingType()
		{
			var stmNum = provider.CustomsNumbers.AddNew();
			var setting = stmNum.Setting;
			AssertType<TSTCustomsNumberViewStmNumsSetting>(setting);
		}

		public void TestWrapperType()
		{
			var stmNum = provider.CustomsNumbers.AddNew();
			var wrapper = stmNum.Wrapper;
			AssertType<TSTCustomsNumberViewStmNumsWrapper>(wrapper);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return provider;
		}

		protected override void SetUp()
		{
			base.SetUp();
			authorisation = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;
			authorisation.CPH_Number = "TS000040";
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			provider = new TSTCustomsNumberViewStmNumsAuthorisationProvider(Factory, authorisation.PK);
		}

		Customs.Business.CusAuthorisationHeader authorisation;
		TSTCustomsNumberViewStmNumsAuthorisationProvider provider;
	}
}
