using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(MultilinePayableAt))]
	sealed class MultilinePayableAtTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<MultilinePayableAt(A, B, C)>");
			AssertNotResponsibleForReplacing("<MultilinePayableAt(A, B, C, D, E)>");
			AssertNotResponsibleForReplacing("<MultilinePayable(A, B, C, D)>");

			AssertIsResponsibleForReplacing("<MultilinePayableAt(<A>, <B>, <C>, <D>)>");
			AssertIsResponsibleForReplacing("< MULTILINEPAYABLEAT (	AAAAA	,	BBBBB	,	C	 ,  D 	 )   >");
			AssertIsResponsibleForReplacing("<multilinepayableat(AAAAA,BBBBB,C,D\nDD\nDDD)>");
		}

		public void TestGetReplacement()
		{
			var provider = new MultilinePrePaidCharges();

			AssertIsReplacedWith("Invalid Incoterm", "<MultilinePayableAt(SGSIN, AUBNE, BLAH, DST\nORG)>");
			AssertIsReplacedWith("SGSIN\nAUBNE", "<MultilinePayableAt(SGSIN, AUBNE, DDP, ORG\nDST)	>");
			AssertIsReplacedWith("AUBNE\nSGSIN", "<MultilinePayableAt(SGSIN, AUBNE, DES, DST\nORG) >");
			AssertIsReplacedWith("SGSIN\nAUBNE", "<MultilinePayableAt(SGSIN, AUBNE, DES, ORG\nDST)>");
			AssertIsReplacedWith("AUBNE\nAUBNE", "<MultilinePayableAt(SGSIN, AUBNE, EXW, DST\nORG)>");

			AssertIsReplacedWith("Invalid Incoterm", "<MultilinePayableAt(AUBNE, SGSIN, BLAH, DST\nORG)>");
			AssertIsReplacedWith("AUBNE\nAUBNE", "<MultilinePayableAt(AUBNE, SGSIN, DDP, ORG\nDST)	>");
			AssertIsReplacedWith("SGSIN\nAUBNE", "<MultilinePayableAt(AUBNE, SGSIN, DES, DST\nORG) >");
			AssertIsReplacedWith("AUBNE\nSGSIN", "<MultilinePayableAt(AUBNE, SGSIN, DES, ORG\nDST)>");
			AssertIsReplacedWith("SGSIN\nAUBNE", "<MultilinePayableAt(AUBNE, SGSIN, EXW, DST\nORG)>");

			AssertIsReplacedWith("", "<MultilinePayableAt(SGSIN, AUBNE, EXW, )>");
			AssertIsReplacedWith("", "<MultilinePayableAt(SGSIN, AUBNE, EXW,)>");
			AssertIsReplacedWith("", "<MultilinePayableAt(AUBNE, SGSIN, EXW, )>");
			AssertIsReplacedWith("", "<MultilinePayableAt(AUBNE, SGSIN, EXW,)>");
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new MultilinePayableAt();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Shipment.Origin", "SGSIN"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Shipment.Destination", "AUBNE"));
		}

		#endregion
	}
}
