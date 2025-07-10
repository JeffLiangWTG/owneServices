using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ScreeningMethod))]
	class ScreeningMethodTest : CusSupportingInfoTest<ScreeningMethod>
	{
		public void TestHumanReadableName()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var screeningMethod = header.ScreeningMethods.AddNew();
			AssertEquals("Screening Method", screeningMethod.HumanReadableName);
		}

		[TestDate()]
		public void TestScreeningMethodDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2SM", "IC2SM");
			helper.CreateCusCodeList("EUN", "IC2SM", "0683", "TestDescription", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var screeningMethod = header.ScreeningMethods.AddNew();

			screeningMethod.CSI_Code = "0683";
			AssertEquals("ScreeningMethodDescription is populated if code is valid", "TestDescription", screeningMethod.ScreeningMethodDescription);

			screeningMethod.CSI_Code = "1111";
			AssertEquals("ScreeningMethodDescription is set to empty if code is invalid", string.Empty, screeningMethod.ScreeningMethodDescription);
		}

		public void TestFieldCaption()
		{
			var screeningMethod = Factory.New<ScreeningMethod>();

			var codeResourceStringDataAttribute = screeningMethod.CSI_CodeInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Code", codeResourceStringDataAttribute.Caption);

			var descriptionResourceStringDataAttribute = screeningMethod.ScreeningMethodDescriptionInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Description", descriptionResourceStringDataAttribute.Caption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.ScreeningMethods.AddNew();
		}

		protected override IEnumerable<ScreeningMethod> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var screeningMethodOnHeader = header.ScreeningMethods.AddNew();

			var bill = header.Bills.AddNew();
			var screeningMethodOnBill = bill.ScreeningMethods.AddNew();

			Factory.Save();

			yield return screeningMethodOnHeader;
			yield return screeningMethodOnBill;
		}
	}
}
