using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(MacroWrapper))]
	sealed class MacroWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			MacroWrapper wrapperEmpty = new MacroWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Useage", ZString.Empty, wrapperEmpty.Useage);
			AssertEquals("wrapperEmpty.Explanation", ZString.Empty, wrapperEmpty.Explanation);
		}

		public void TestFull()
		{
			MacroWrapper wrapper = new MacroWrapper(new MacroValueProviderMap("Freddo", "Freddo", "Frog"), Factory);
			AssertEquals("wrapper.ToString()", "Freddo", wrapper.ToString());
			AssertEquals("wrapper.Useage", "Freddo", wrapper.Useage);
			AssertEquals("wrapper.Explanation", "Frog", wrapper.Explanation);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Macro                                          (Default Field: Useage)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Explanation                             String
TypeName                                String
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
			return new MacroWrapper(new MacroValueProviderMap("Freddo", "Frog"), Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new MacroWrapper(null, Factory);
		}
	}
}
