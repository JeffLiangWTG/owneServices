using Enterprise.DocumentEngineCore.Exceptions;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class ImageFormatExceptionTest : ExceptionTestCase<ImageFormatException>
	{
		protected override ImageFormatException GetNewExceptionToTest(string message)
		{
			return new ImageFormatException();
		}
	}
}
