using System;
using System.Text.RegularExpressions;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	sealed class MultiLineIncoBasedConverterTest : TestCase
	{
		public void TestMultiLineConvertByIncoTerm()
		{
			TestImplementation provider = new TestImplementation();

			AssertEquals("", provider.CallMultiLineConvertByIncoTerm(true, "", "EXW"));

			AssertEquals("Invalid Incoterm", provider.CallMultiLineConvertByIncoTerm(true, "ORG\nDST", "BLAH"));
			AssertEquals("Load\nDisc", provider.CallMultiLineConvertByIncoTerm(true, "ORG\nDST", "DDP"));
			AssertEquals("Disc\nLoad", provider.CallMultiLineConvertByIncoTerm(true, "DST\nORG", "DES"));
			AssertEquals("Load\nDisc", provider.CallMultiLineConvertByIncoTerm(true, "ORG\nDST", "DES"));
			AssertEquals("Disc\nDisc", provider.CallMultiLineConvertByIncoTerm(true, "DST\nORG", "EXW"));

			AssertEquals("Invalid Incoterm", provider.CallMultiLineConvertByIncoTerm(false, "BLAH", "DST\nORG"));
			AssertEquals("Load\nLoad", provider.CallMultiLineConvertByIncoTerm(false, "ORG\nDST", "DDP"));
			AssertEquals("Disc\nLoad", provider.CallMultiLineConvertByIncoTerm(false, "DST\nORG", "DES"));
			AssertEquals("Load\nDisc", provider.CallMultiLineConvertByIncoTerm(false, "ORG\nDST", "DES"));
			AssertEquals("Disc\nLoad", provider.CallMultiLineConvertByIncoTerm(false, "DST\nORG", "EXW"));
		}

		#region TestImplementation

		class TestImplementation : MultiLineIncoBasedConverter
		{
			protected override ValueProviderDocumenter GetDocumentation()
			{
				throw new NotImplementedException();
			}

			public ZString CallMultiLineConvertByIncoTerm(bool isImport, ZString chargeGroupsString, ZString incoTermString)
			{
				return MultiLineConvertByIncoTerm(isImport, chargeGroupsString, incoTermString, "Load", "Disc");
			}

			public override Regex Regex
			{
				get { return new Regex("<Blah>"); }
			}

			protected override object GetReplacementCore(string macro, Report report)
			{
				return 0;
			}
		}

		#endregion
	}
}
