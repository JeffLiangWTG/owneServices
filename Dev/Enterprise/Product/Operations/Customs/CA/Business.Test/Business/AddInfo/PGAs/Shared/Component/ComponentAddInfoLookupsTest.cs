using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ComponentAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestComponentTypes()
		{
			var cnscpgaHeader = Factory.New<CNSCPGAHeader>();
			var component = cnscpgaHeader.Components.AddNew();
			var componentTypes = component.AddInfoLookups.ComponentTypes;

			AssertEquals(0, componentTypes.Count);
			AssertEquals(typeof(CodeDescriptionPairList), componentTypes.GetType());

			var ecccpgaHeader = Factory.New<ECCCPGAHeader>();
			ecccpgaHeader.CA_WENProgramInd = YesNoList.Codes.Yes;

			component = ecccpgaHeader.Components.AddNew();
			componentTypes = component.AddInfoLookups.ComponentTypes;

			var expectedCodes = new IDTypeCodes();
			expectedCodes.RemoveCode(IDTypeCodes.Codes.BN);
			expectedCodes.RemoveCode(IDTypeCodes.Codes.EE);
			expectedCodes.RemoveCode(IDTypeCodes.Codes.VV);

			var actualCodes = componentTypes.GetAllCodes();
			AssertContainsExactElementsInAnyOrder(expectedCodes.GetAllCodes(), actualCodes);
		}

		public void TestUnitList()
		{
			var cnscHeader = Factory.New<CNSCPGAHeader>();
			var chemicalSubstance = cnscHeader.Components.AddNew();
			cnscHeader.CA_Category = CNSCCategories.Codes.CNS;
			var volumnUnitList = chemicalSubstance.AddInfoLookups.UnitList;

			AssertEquals(17, volumnUnitList.Count);
			Assert(volumnUnitList.ContainsCode(ComponentVolumnUnitList.Codes.Metre));

			cnscHeader.CA_Category = CNSCCategories.Codes.NE;
			volumnUnitList = chemicalSubstance.AddInfoLookups.UnitList;
			AssertEquals(0, volumnUnitList.Count);

			cnscHeader.CA_Category = CNSCCategories.Codes.RD;
			volumnUnitList = chemicalSubstance.AddInfoLookups.UnitList;
			AssertEquals(18, volumnUnitList.Count);
			Assert(volumnUnitList.ContainsCode(ComponentVolumnUnitList.Codes.BecquerelGram));

			var hcHeader = Factory.New<HCPGAHeader>();
			var hcComponent = hcHeader.Components.AddNew();
			var hcUnitList = hcComponent.AddInfoLookups.UnitList;
			AssertEquals(6, hcUnitList.Count);

			var ecccHeader = Factory.New<ECCCPGAHeader>();
			var ecccComponent = ecccHeader.Components.AddNew();
			var ecccUnitList = ecccComponent.AddInfoLookups.UnitList;
			AssertEquals(6, ecccUnitList.Count);
		}
	}
}
