using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(MultilinePrePaidCharges))]
	sealed class MultilinePrePaidChargesTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<MultiLinePrePaidCharge(Y,B)	>");
			AssertNotResponsibleForReplacing("<MultiLinePrePaidCharges(Y,B,C,D)>");
			AssertNotResponsibleForReplacing("<MultiLinePrePaidCharges(Y,B)>");
			AssertNotResponsibleForReplacing("<MultiLinePrePaidCharges(A,B,C,D)>");
			AssertNotResponsibleForReplacing("<MultiLinePrePaidCharges(NN,B,C)>");
			AssertNotResponsibleForReplacing("<MultiLinePrePaidCharges(YY,B,C)>");

			AssertIsResponsibleForReplacing("< MultilinePrePaidCharges (	Y	,	B	,C 	 )   >");
			AssertIsResponsibleForReplacing("<MultilinePrePaidCharges(N, B, C\nCC)>");
			AssertIsResponsibleForReplacing("<MultiLinePrePaidCharges(Y,B,C)>");
			AssertIsResponsibleForReplacing("<MultiLinePrePaidCharges(N,B,C)>");
			AssertIsResponsibleForReplacing("<MultilinePrePaidCharges(N, <B>, <C>)>");
		}

		public void TestGetReplacement()
		{
			var provider = new MultilinePrePaidCharges();

			AssertIsReplacedWith("Invalid Incoterm", "<MultilinePrePaidCharges(Y, BLAH, DST\nORG)>");
			AssertIsReplacedWith("Prepaid\nCollect", "<MultilinePrePaidCharges(Y, DDP, ORG\nDST)	>");
			AssertIsReplacedWith("Collect\nPrepaid", "<MultilinePrePaidCharges(Y, DES, DST\nORG) >");
			AssertIsReplacedWith("Prepaid\nCollect", "<MultilinePrePaidCharges(Y, DES, ORG\nDST)>");
			AssertIsReplacedWith("Collect\nCollect", "<MultilinePrePaidCharges(Y, EXW, DST\nORG)>");

			AssertIsReplacedWith("Invalid Incoterm", "<MultilinePrePaidCharges(N, BLAH, DST\nORG)>");
			AssertIsReplacedWith("Prepaid\nPrepaid", "<MultilinePrePaidCharges(N, DDP, ORG\nDST)	>");
			AssertIsReplacedWith("Collect\nPrepaid", "<MultilinePrePaidCharges(N, DES, DST\nORG) >");
			AssertIsReplacedWith("Prepaid\nCollect", "<MultilinePrePaidCharges(N, DES, ORG\nDST)>");
			AssertIsReplacedWith("Collect\nPrepaid", "<MultilinePrePaidCharges(N, EXW, DST\nORG)>");

			AssertIsReplacedWith("", "<MultilinePrePaidCharges(Y, EXW, )>");
			AssertIsReplacedWith("", "<MultilinePrePaidCharges(Y, EXW,)>");
			AssertIsReplacedWith("", "<MultilinePrePaidCharges(N, EXW, )>");
			AssertIsReplacedWith("", "<MultilinePrePaidCharges(N, EXW,)>");
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new MultilinePrePaidCharges();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Inco Term", "EXW"));
		}

		#endregion
	}
}
