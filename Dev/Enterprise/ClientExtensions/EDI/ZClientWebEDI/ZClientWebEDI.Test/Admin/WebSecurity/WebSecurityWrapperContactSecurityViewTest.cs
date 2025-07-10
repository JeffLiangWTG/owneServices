using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(WebSecurityDataSource.ContactSecurityView))]
	public class WebSecurityWrapperContactSecurityViewTest : BusinessObjectCollectionViewTestCase<WebSecurityDataSource.ContactSecurityView>
	{
		protected override WebSecurityDataSource.ContactSecurityView GetCollectionToTest() => WebSecurityDataSourceTest.GetWrapperToTest(Factory).ContactSecurityRights;
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var obj = Factory.NewWithValidTestData<OrgSecurityContacts>();
			obj.Security.OX_IsCustomerManaged = true;
			obj.Contact.OC_WebAccessEnabled = true;
			obj.Contact.OC_Email = "a@cw1.com";
			return obj;
		}
	}
}
