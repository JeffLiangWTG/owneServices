using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(CodeMultilingualDescriptionWrapper))]
	sealed class CodeMultilingualDescriptionWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new CodeMultilingualDescriptionWrapper("", (NoResString)"", Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Useage", ZString.Empty, wrapperEmpty.Useage);
			AssertEquals("wrapperEmpty.Explanation", ZString.Empty, wrapperEmpty.Explanation);
		}

		public void TestFull()
		{
			var wrapper = new CodeMultilingualDescriptionWrapper("tab", (NoResString)"desides which group the filter belongs to", Factory);
			AssertEquals("wrapper.ToString()", "tab", wrapper.ToString());
			AssertEquals("wrapper.Useage", "tab", wrapper.Useage);
			AssertEquals("wrapper.Explanation", "desides which group the filter belongs to", wrapper.Explanation);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
CodeMultilingualDescription                    (Default Field: Useage)
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

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new CodeMultilingualDescriptionWrapper("tab", (NoResString)"desides which group the filter belongs to", Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CodeMultilingualDescriptionWrapper("", (NoResString)"", Factory);
		}
	}
}
