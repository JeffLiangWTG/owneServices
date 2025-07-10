using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CYDReleaseAdviceLineWrapper))]
	sealed class CYDReleaseAdviceLineWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappings()
		{
			var wrapper = (CYDReleaseAdviceLineWrapper)GetSetupWrapperForDefaultFormatting();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.PreviousOnHireDate", ZDate.Today, wrapper.OnHireDate);
				AssertEquals("wrapper.OffHireDate", ZDate.Today, wrapper.OffHireDate);
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (CYDReleaseAdviceLineWrapper)GetNewDocumentWrapper();
			CombineAssertions(() =>
			{
				AssertNotEquals("wrapper.PreviousOnHireDate", ZDate.Today, wrapper.OnHireDate);
				AssertNotEquals("wrapper.OffHireDate", ZDate.Today, wrapper.OffHireDate);
			});
		}

		protected override ZString ExpectedDefaultFormatting => @"
Registry : (No Default Field Value Available on Registry)";

		protected override string ExpectedFieldMap => @"
CYDReleaseAdviceLine
======================================================================
Name                                    Type
----------------------------------------------------------------------";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CYDReleaseAdviceLineWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var releaseAdviceLine = GetNewReleaseLine();
			return new CYDReleaseAdviceLineWrapper(releaseAdviceLine, Factory);
		}

		CYDReleaseAdviceLine GetNewReleaseLine()
		{
			var releaseAdviceLine = Factory.New<CYDReleaseAdviceLine>();
			releaseAdviceLine.YEL_OffHireDate = ZDate.Today;
			releaseAdviceLine.YEL_OnHireDate = ZDate.Today;
			return releaseAdviceLine;
		}
	}
}
