using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(SyntaxAndFormattingWrapper))]
	sealed class SyntaxAndFormattingWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			SyntaxAndFormattingWrapper wrapperEmpty = new SyntaxAndFormattingWrapper("", "", Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.TitleText", "", wrapperEmpty.TitleText);
			AssertEquals("wrapperEmpty.LineText", "", wrapperEmpty.LineText);
		}

		public void TestWrapperMappingsFull()
		{
			SyntaxAndFormattingWrapper wrapper = new SyntaxAndFormattingWrapper("Johnny", "Cash", Factory);
			AssertEquals("wrapper.ToString()", "Johnny", wrapper.ToString());
			AssertEquals("wrapper.TitleText", "Johnny", wrapper.TitleText);
			AssertEquals("wrapper.LineText", "Cash", wrapper.LineText);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
SyntaxAndFormatting                         (Default Field: TitleText)
======================================================================
Name                                    Type
----------------------------------------------------------------------
LineText                                String
TitleText                               String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new SyntaxAndFormattingWrapper("BIG", "Moron", Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new SyntaxAndFormattingWrapper("", "", Factory);
		}
	}
}
