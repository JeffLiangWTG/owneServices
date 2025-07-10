using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DocumentPreviewExceptionTest : ExceptionTestCase<DocumentPreviewException>
	{
		protected override DocumentPreviewException GetNewExceptionToTest(string message)
		{
			return new DocumentPreviewException(message, new FlexCelXlsAdapterException());
		}
	}
}
