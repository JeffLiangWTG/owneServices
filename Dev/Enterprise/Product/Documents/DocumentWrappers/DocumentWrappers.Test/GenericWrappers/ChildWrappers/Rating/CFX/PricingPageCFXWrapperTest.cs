using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageCFXWrapper))]
	sealed class PricingPageCFXWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			PricingPageCFXWrapper wrapper = new PricingPageCFXWrapper(Factory, "", 0m);
			AssertEquals(0m, wrapper.Value);
			AssertEquals("", wrapper.Name);
			AssertEquals(" 0.00%", wrapper.NameAndValue);
		}

		public void TestPopulated()
		{
			PricingPageCFXWrapper wrapper = new PricingPageCFXWrapper(Factory, "Name", 4.5m);
			AssertEquals("wrapper.Name", "Name", wrapper.Name);
			AssertEquals("wrapper.Value", 4.5m, wrapper.Value);
			AssertEquals("wrapper.NameAndValue", "Name 4.50%", wrapper.NameAndValue);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new PricingPageCFXWrapper(Factory, "Name", 5.32m);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Pricing Page CFX                         (Default Field: NameAndValue)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Name                                    String
NameAndValue                            String
Value                                   Decimal
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PricingPageCFXWrapper(Factory, "Name", 3.2m);
		}

		#endregion
	}
}
