using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(ContactDeactivationWrapper))]
	public class ContactUserAccountGroupingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.Header.OH_Code = "WISBNEMEL";
			contact.Header.OH_FullName = "Wise Melbourne";
			contact.OC_Email = "mr@scissors.com";
			var contactWrapper = new ContactDeactivationWrapper(contact);
			AssertEquals("WISBNEMEL - Wise Melbourne (mr@scissors.com)", contactWrapper.Organisation);
			AssertEquals(string.Empty, contactWrapper.LicenceType);
			AssertEquals(string.Empty, contactWrapper.SystemInfo);
			AssertEquals(-1, contactWrapper.ReferenceNumber);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ContactDeactivationWrapper(Factory.NewWithValidTestData<OrgContact>());
		}
		#endregion
	}
}
