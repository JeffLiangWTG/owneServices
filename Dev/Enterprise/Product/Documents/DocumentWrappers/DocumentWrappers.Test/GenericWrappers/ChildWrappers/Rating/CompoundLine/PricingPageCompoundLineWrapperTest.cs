using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageCompoundLineWrapper))]
	sealed class PricingPageCompoundLineWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CombineAssertions(delegate
			{
				PricingPageCompoundLineWrapper wrapper = new PricingPageCompoundLineWrapper(Factory);
				AssertEquals("wrapper.Label", ZString.Empty, wrapper.Label);
				AssertEquals("wrapper.SubLabel", ZString.Empty, wrapper.SubLabel);
				AssertEquals("wrapper.LabelOrdinal", 0, wrapper.LabelOrdinal);
				AssertEquals("wrapper.SubLabelOrdinal", 0, wrapper.SubLabelOrdinal);
				AssertEquals("wrapper.Page", null, wrapper.Page);
				AssertEquals("wrapper.Row", null, wrapper.Row);
				AssertEquals("wrapper.SubRow", null, wrapper.SubRow);
				AssertEquals("wrapper.RateLine", null, wrapper.RateLine);
			});
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Page :  is null
RateLine :  is null
Registry : (No Default Field Value Available on Registry)
Row :  is null
SubRow :  is null
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new PricingPageCompoundLineWrapper(Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Compound Line
======================================================================
Name                                    Type
----------------------------------------------------------------------
Page                                    Pricing Page
RateLine                                Pricing Page Line
Row                                     Pricing Page Table Row
SubRow                                  Pricing Page Table Sub Row
Label                                   String
LabelOrdinal                            Int
SubLabel                                String
SubLabelOrdinal                         Int
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PricingPageCompoundLineWrapper(Factory);
		}

		#endregion
	}
}
