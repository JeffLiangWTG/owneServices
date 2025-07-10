using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class AuthorisationHelperTest : TestCase
{
	public void TestGetAuthorisationCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("C517", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, AuthorisationHelper.GetAuthorisationCode(Constants.SupportingDocumentTypes.C517));
			AssertEquals("C518", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, AuthorisationHelper.GetAuthorisationCode(Constants.SupportingDocumentTypes.C518));
			AssertEquals("C519", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, AuthorisationHelper.GetAuthorisationCode(Constants.SupportingDocumentTypes.C519));
			AssertEquals("Empty", ZString.Empty, AuthorisationHelper.GetAuthorisationCode(ZString.Empty));
		});
	}
}
