using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocGLJournalLine))]
	sealed class DocGLJournalLineTest : DocTransactionLineTest
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocGLJournalLine.New(Line, Factory), };
		}

		protected override DocTransactionLine GetLineWrapper()
		{
			return (DocGLJournalLine)GetDocumentWrappers()[0];
		}

		public override void TestExchangeRate()
		{
			Line.AL_ExchangeRate = 0.7890M;
			AssertEquals(0.7890M, LineWrapper.ExchangeRate);
		}
	}
}
