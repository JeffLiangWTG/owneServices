using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Freight.Forwarding.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAPivotLookupsTest : TestCaseWithFactory
	{
		public void TestCV_OceanBillContainersList()
		{
			CusSCATestHelper helper = new CusSCATestHelper();
			ForwardingContainer c1 = helper.Container1;
			ForwardingContainer c2 = helper.Container2;
			ForwardingContainer c3 = helper.Container3;
			AssertEquals("OceanBillContainers List", typeof(CodeDescriptionPairList), helper.PackLine1.Lookups.CV_OceanBillContainers_List.GetType());
			CodeDescriptionPairList list = helper.PackLine1.Lookups.CV_OceanBillContainers_List;
			AssertEquals("3 containers in list", 3, list.Count);
			Assert("contains C1", list.ContainsCode(CusSCATestHelper.Container1Num));
			Assert("contains C2", list.ContainsCode(CusSCATestHelper.Container2Num));
			Assert("contains C3", list.ContainsCode(CusSCATestHelper.Container3Num));
		}

		public void TestAcrossPackageTypes()
		{
			CusSCATestHelper helper = new CusSCATestHelper();
			AssertEquals("AcrossPackageTypes List", typeof(ACROSSPackageTypes), helper.PackLine1.Lookups.AcrossPackageTypes.GetType());
			CodeDescriptionPairList list = helper.PackLine1.Lookups.AcrossPackageTypes;
			AssertEquals("150 AcrossPackageTypes in list", 151, list.Count);
			Assert("Contains Alpha: Ammo Pack", list.ContainsCode("AMM"));
			Assert("Contains Omega: Wrapped", list.ContainsCode("WRP"));
		}

		public void TestACIWeightUnits()
		{
			CusSCATestHelper helper = new CusSCATestHelper();
			AssertEquals("ACIWeightUnits List", typeof(CodeDescriptionPairList), helper.PackLine1.Lookups.ACIWeightUnits.GetType());
			CodeDescriptionPairList list = helper.PackLine1.Lookups.ACIWeightUnits;
			AssertEquals("3 ACIWeightUnits in list", 3, list.Count);
			Assert("ACIWeightUnits KG", list.ContainsCode(Core.Constants.Weight.Kilograms));
			Assert("ACIWeightUnits TN", list.ContainsCode(Core.Constants.Weight.Tonnes));
			Assert("ACIWeightUnits LB", list.ContainsCode(Core.Constants.Weight.Pounds));
		}

		public void TestVolumeUQList()
		{
			CusSCATestHelper helper = new CusSCATestHelper();
			AssertEquals("VolumeUQList List", typeof(CodeDescriptionPairList), helper.PackLine1.Lookups.VolumeUQList.GetType());
			CodeDescriptionPairList list = helper.PackLine1.Lookups.VolumeUQList;
			AssertEquals("27 VolumeUQList in list", 27, list.Count);
			Assert("VolumeUQList M3", list.ContainsCode(Core.Constants.Volume.CubicMetres));
			Assert("VolumeUQList CubicCentimetre", list.ContainsCode(MessageConstants.ACIVolumeUnits.CubicCentimetre));
			Assert("VolumeUQList LoadForEnterprise", list.ContainsCode(MessageConstants.ACIVolumeUnits.LoadForEnterprise));
			Assert("VolumeUQList L", list.ContainsCode(Core.Constants.Volume.Litre));
		}
	}
}
