using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Service.Testing;

[TestedType(typeof(ProfessionalServicesQuote))]
class ProfessionalServicesQuoteTest : EnterpriseBusinessObjectTestCase
{
	public void TestHTMLLink()
	{
		ZString expectedString = "<a href=\"" + ShowEditFormUrlHandler.Instance.CreateWithoutApplicationContext(ClientControllerRegistration.ProfessionalServicesQuote, BizObj.PK) + "\">" + BizObj.IM_IncidentNumber + "</a>";

		AssertEquals("PSQ link", expectedString, BizObj.HTMLLink);
	}

	ProfessionalServicesQuote BizObj
	{
		get { return fBizObj ?? (fBizObj = Factory.New<ProfessionalServicesQuote>()); }
	}

	ProfessionalServicesQuote fBizObj;
}
