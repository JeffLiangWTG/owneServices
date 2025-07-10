using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIOrgContactsController))]
	public class EDIOrgContactsControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			Assert(true);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgContacts;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = Factory.New<EDIOrgContact>();
			contact.OC_OH = org.PK;
			Factory.Save();
			return contact;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader.MainAddress.OA_Address1 = "Test Address 1";
			orgHeader.MainAddress.OA_Phone = "(02) 1234 5678";
			orgHeader.MainAddress.OA_PostCode = "123456";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			orgHeader.OH_FullName = "Test Consignor";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			var contact = Factory.New<EDIOrgContact>();
			contact.OC_ContactName = "name";
			contact.OC_OH = orgHeader.PK;
			return contact;
		}
	}
}
