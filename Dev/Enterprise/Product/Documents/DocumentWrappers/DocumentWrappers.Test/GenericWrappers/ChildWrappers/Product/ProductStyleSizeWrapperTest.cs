using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ProductStyleSizeWrapper))]
	sealed class ProductStyleSizeWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new ProductStyleSizeWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "(No Default Field Value Available on ProductStyleSize)", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Sequence", (ZByte)0, wrapperEmpty.Sequence);
			AssertEquals("wrapperEmpty.Size", "", wrapperEmpty.Size);
			AssertNull("wrapperEmpty.Style", wrapperEmpty.Style.WrappedObject);
		}

		public void TestWrapperMappingFull()
		{
			var style = Factory.New<WhsProductStyle>();
			var styleSize = style.Sizes.AddNew();

			styleSize.WSZ_Sequence = 2;
			styleSize.WSZ_Size = "XXXL";

			var wrapper = new ProductStyleSizeWrapper(styleSize, Factory);
			AssertEquals("wrapper.ToString()", "(No Default Field Value Available on ProductStyleSize)", wrapper.ToString());
			AssertEquals("wrapper.Sequence", (ZByte)2, wrapper.Sequence);
			AssertEquals("wrapper.Size", "XXXL", wrapper.Size);
			AssertEquals("wrapper.Style", style, wrapper.Style.WrappedObject);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"ProductStyleSize
======================================================================
Name                                    Type
----------------------------------------------------------------------
Style                                   ProductStyle
Sequence                                Byte
Size                                    String";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)
Style : (No Default Field Value Available on ProductStyle)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var style = Factory.New<WhsProductStyleSize>();
			return new ProductStyleSizeWrapper(style, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var style = Factory.New<WhsProductStyleSize>();
			return new ProductStyleSizeWrapper(style, Factory);
		}
	}
}
