using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	sealed class SerializationKeysResultTest : TestCaseWithFactory
	{
		public void TestSerialProcessingInReceiveOrder()
		{
			AssertEquals(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder, new HashSet<string> { SerializationKeysResult.ForceSerialProcessingKey }), SerializationKeysResult.SerialProcessingInReceivedOrder);
		}

		public void TestUnconstrainedParallelProcessing()
		{
			AssertEquals(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.UnconstrainedParallelProcessing, new HashSet<string>()), SerializationKeysResult.UnconstrainedParallelProcessing);
		}

		public void TestEquals()
		{
			var result1 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "HELLO" });
			var result2 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "HELLO" });
			var result3 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "HI" });
			var result4 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder, new HashSet<string> { "HELLO" });
			var result5 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.UnconstrainedParallelProcessing, new HashSet<string> { "HELLO" });
			AssertEquals("result1.Equals(result2)", true, result1.Equals(result2));
			AssertEquals("result1.Equals(result3)", false, result1.Equals(result3));
			AssertEquals("result1.Equals(result4)", false, result1.Equals(result4));
			AssertEquals("result1.Equals(result5)", false, result1.Equals(result5));
		}

		public void TestGetHashCode()
		{
			var result1 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "HELLO" });
			var result2 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "HELLO" });
			var result3 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "HI" });
			var result4 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder, new HashSet<string> { "HELLO" });
			var result5 = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.UnconstrainedParallelProcessing, new HashSet<string> { "HELLO" });
			AssertEquals(result1.GetHashCode(), result2.GetHashCode());
			AssertNotEquals(result1.GetHashCode(), result3.GetHashCode());
			AssertNotEquals(result1.GetHashCode(), result4.GetHashCode());
			AssertNotEquals(result1.GetHashCode(), result5.GetHashCode());
		}

		public void TestToString()
		{
			var result = new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "HELLO", "HI" });
			AssertEquals(typeof(SerializationKeysResult).ToString() + $" (ResultType: {SerializationKeysResult.SerializationKeysResultType.KeysProvided}, Keys: HELLO|HI)", result.ToString());
		}
	}
}
