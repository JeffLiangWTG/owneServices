using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(GateYardUnitWrapper))]
	sealed class GateYardUnitWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappings()
		{
			var wrapper = (GateYardUnitWrapper)GetSetupWrapperForDefaultFormatting();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.Quality.CodeAndDescription", "DRC - Container Dry and Clean", wrapper.Quality.CodeAndDescription);
				AssertEquals("wrapper.TypeSize.CodeAndDescription", "CCC - Type Size Description", wrapper.TypeSize.CodeAndDescription);
				AssertEquals("wrapper.WeightGross", "12.000 KG", wrapper.WeightGross.ValueAndUnitCode);
				AssertEquals("wrapper.SealNumber", "SEAL111111", wrapper.SealNumber);
				AssertEquals("wrapper.IsEmptyContainer", true, wrapper.IsEmptyContainer);
				AssertEquals("wrapper.UnitNumber", "C00001", wrapper.UnitNumber);
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (GateYardUnitWrapper)GetNewDocumentWrapper();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.Quality.CodeAndDescription", "", wrapper.Quality.CodeAndDescription);
				AssertEquals("wrapper.TypeSize.CodeAndDescription", "", wrapper.TypeSize.CodeAndDescription);
				AssertEquals("wrapper.WeightGross.ValueAndUnitCode", "0.000", wrapper.WeightGross.ValueAndUnitCode);
				AssertEquals("wrapper.SealNumber", "", wrapper.SealNumber);
				AssertEquals("wrapper.IsEmptyContainer", false, wrapper.IsEmptyContainer);
				AssertEquals("wrapper.UnitNumber", "", wrapper.UnitNumber);
			});
		}

		protected override ZString ExpectedDefaultFormatting => @"
Quality : DRC - Container Dry and Clean
Registry : (No Default Field Value Available on Registry)
TypeSize : CCC - Type Size Description
WeightGross : 12.000 KG";

		protected override string ExpectedFieldMap => @"
GateYardUnit
======================================================================
Name                                    Type
----------------------------------------------------------------------
Quality                                 CodeAndDescription
TypeSize                                ContainerType
WeightGross                             Weight
IsEmptyContainer                        Bool
SealNumber                              String
UnitNumber                              String";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new GateYardUnitWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var yardUnit = GetNewYardUnit();
			return new GateYardUnitWrapper(yardUnit, Factory);
		}

		YardUnit GetNewYardUnit()
		{
			var yardUnit = Factory.New<YardUnit>();

			yardUnit.GTY_Quality = "DRC";
			yardUnit.GTY_GrossWeight = 12;
			yardUnit.GTY_GrossWeightUQ = "KG";
			yardUnit.GTY_Seal1 = "SEAL111111";
			yardUnit.GTY_IsEmpty = true;
			yardUnit.GTY_UnitNumber = "C00001";

			var container = Factory.New<RefContainer>();
			container.RC_Code = "CCC";
			container.RC_Description = "Type Size Description";
			yardUnit.GTY_RC_TypeSize = container.PK;

			return yardUnit;
		}
	}
}
