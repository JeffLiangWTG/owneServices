using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(WebSecurityDataSource.SecurityView))]
	public class WebSecurityWrapperSecurityViewTest : BusinessObjectCollectionViewTestCase<WebSecurityDataSource.SecurityView>
	{
		protected override WebSecurityDataSource.SecurityView GetCollectionToTest()
		{
			var collection = WebSecurityDataSourceTest.GetWrapperToTest(Factory).SecurityRights;
			collection.HasChanges = false;
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var wrapper = WebSecurityDataSourceTest.GetWrapperToTest(Factory);
			var obj = Factory.NewWithValidTestData<OrgSecurity>();
			obj.OX_SecurityItemName = wrapper.SecurityRights[0].OX_SecurityItemName;
			obj.OX_IsCustomerManaged = true;
			return obj;
		}
	}
}
