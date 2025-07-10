using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageColumnWrapper))]
	sealed class PricingPageColumnWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CombineAssertions(delegate
			{
				var wrapper = new PricingPageColumnWrapper("", DocAmount.Empty, Factory);
				AssertEquals("wrapper.Heading", "", wrapper.Heading);
				AssertEquals("wrapper.Value", "", wrapper.Value);
			});
		}

		public void TestPopulated()
		{
			CombineAssertions(delegate
			{
				var wrapper = new PricingPageColumnWrapper("bob", DocAmount.Create((NoResString)"blat"), Factory);
				AssertEquals("wrapper.Heading", "bob", wrapper.Heading);
				AssertEquals("wrapper.Value", "blat", wrapper.Value);
			});
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
			return new PricingPageColumnWrapper("heading", DocAmount.Create((NoResString)"value"), Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Pricing Page Table Column                       (Default Field: Value)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Currency                                String
Heading                                 String
Value                                   String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PricingPageColumnWrapper("heading", DocAmount.Create((NoResString)"value"), Factory);
		}

		#endregion
	}
}
