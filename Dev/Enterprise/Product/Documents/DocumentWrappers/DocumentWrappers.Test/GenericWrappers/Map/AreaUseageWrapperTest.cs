using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(AreaUseageWrapper))]
	sealed class AreaUseageWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			AreaUseageWrapper wrapperEmpty = new AreaUseageWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Useage", ZString.Empty, wrapperEmpty.Useage);
			AssertEquals("wrapperEmpty.Explanation", ZString.Empty, wrapperEmpty.Explanation);
		}

		public void TestFull()
		{
			AreaUseageWrapper wrapper = new AreaUseageWrapper(new ValueProviderDocumenter("Freddo", (NoResString)"Frog"), Factory);
			AssertEquals("wrapper.ToString()", "Freddo", wrapper.ToString());
			AssertEquals("wrapper.Useage", "Freddo", wrapper.Useage);
			AssertEquals("wrapper.Explanation", "Frog", wrapper.Explanation);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
AreaUseage                                     (Default Field: Useage)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Explanation                             String
Useage                                  String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new AreaUseageWrapper(new ValueProviderDocumenter("WHAT", (NoResString)"EVER"), Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new AreaUseageWrapper(null, Factory);
		}
	}
}
