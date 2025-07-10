using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CommodityWrapper))]
	sealed class CommodityWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappingFull()
		{
			RefCommodityCode commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "AAA";
			commodity.RH_Description = "Desc AAA";

			CommodityWrapper wrapperFull = new CommodityWrapper(commodity, Factory);
			AssertEquals("wrapperFull.ToString()", "AAA (Desc AAA)", wrapperFull.ToString());
			AssertEquals("wrapperFull.Code", "AAA", wrapperFull.Code);
			AssertEquals("wrapperFull.Description", "Desc AAA", wrapperFull.Description);
			AssertEquals("wrapperFull.CodeAndDescription", "AAA (Desc AAA)", wrapperFull.CodeAndDescription);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new CommodityWrapper(null, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CommodityWrapper(null, Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			CommodityWrapper wrapperEmpty = (CommodityWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Code", ZString.Empty, wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.Description", ZString.Empty, wrapperEmpty.Description);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Commodity                          (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndDescription                      String
Description                             String
";
			}
		}
	}
}
