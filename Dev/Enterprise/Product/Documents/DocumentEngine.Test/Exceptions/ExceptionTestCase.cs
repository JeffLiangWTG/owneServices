using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	abstract class ExceptionTestCase<T> : TestCaseWithFactory where T : Exception
	{
		//TODO: WI00740855 - Investigate the use of serialization with these exceptions. Check if the test can be removed.
		public void TestExceptionCanBeSerialisedAndDeserialisedUsingTheBinaryFormatter()
		{
			T exception = GetNewExceptionToTest("YeeHa!");
			Assert("Precondition: !string.IsNullOrEmpty(exception.Message)", !string.IsNullOrEmpty(exception.Message));

			T deSerializedException = SerializeAndDeserializeExceptionJSON(exception);

			AssertEquals("deSerializedException.Message", exception.Message, deSerializedException?.Message);
			AssertEquals("deSerializedException.InnerException.Message", exception.InnerException?.Message, deSerializedException?.InnerException?.Message);
		}

		protected static T SerializeAndDeserializeExceptionJSON(T exception)
		{
			var result = JsonConverterHelper.Serialize(exception);
			return (T)JsonConverterHelper.Deserialize(result, typeof(T));
		}

		protected abstract T GetNewExceptionToTest(string message);
	}
}
