using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using static Enterprise.ZClientWebCargoWiseEDI.BorderWise.BorderWiseLicenceControllerBase;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise.Testing
{
	public abstract class BorderWiseLicenceControllerBaseTestCase<TController> : TestCaseWithFactory where TController : BorderWiseLicenceControllerBase
	{
		protected abstract TController GetNewController(ZGuid contactPK);

		internal EDIOrgContact CreateOrgAndContact(string licenceMachineIdentifier = null, string orgCode = "ZUB", string email = "rylan@zayden.com", string password = "zubin123")
		{
			return CreateOrgAndContact(Factory, licenceMachineIdentifier, orgCode, email, password);
		}

		internal static EDIOrgContact CreateOrgAndContact(BusinessObjectFactory factory, string licenceMachineIdentifier, string orgCode, string email, string password)
		{
			var org = factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = orgCode;
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_Email = email;
			contact.SetHashedPassword(password);
			contact.OC_WebAccessEnabled = true;
			BorderWiseUtilities.ChangeSecurityRight(contact, true, factory);

			factory.Save();

			return contact;
		}

		protected static void AssertForbidden(string expectedMessage, IHttpActionResult result)
		{
			var castedResult = (NegotiatedContentResult<ForbiddenResponse>)result;
			AssertEquals(HttpStatusCode.Forbidden, castedResult.StatusCode);
			AssertEquals(expectedMessage, castedResult.Content.Message);
		}
	}
}
