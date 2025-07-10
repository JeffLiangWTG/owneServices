using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACFIAEndUseCodes))]
	sealed class CACFIAEndUseCodesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			((CACFIAEndUseCodes)BusinessObject).FE_Code = "12";
			AssertEquals("HumanReadableName", "End Use Code: '12'", BusinessObject.HumanReadableName);
		}
	}
}
