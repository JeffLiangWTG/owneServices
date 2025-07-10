using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(RefVesselZZForRadioCallSign))]
	sealed class RefVesselZZForRadioCallSignTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCodeProperty()
		{
			var refVesselZZForRadioCallSign = Factory.New<RefVesselZZForRadioCallSign>();
			refVesselZZForRadioCallSign.ZZO_RadioCallSign = "LXBH";
			var code = ((ICodeDescription)refVesselZZForRadioCallSign).Code;
			AssertEquals("RefVesselZZForRadioCallSign code should equal Radio Call Sign", refVesselZZForRadioCallSign.ZZO_RadioCallSign, code);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.Japan, factory);
			return base.GetNewBusinessObjectForDeleteTest(factory);
		}
	}
}
