using System.Collections.Generic;
using NUnit.Framework;
#if NETFRAMEWORK
using NUnit.Framework.TestHelper;
#endif

namespace CargoWise.Windows.UI.Testing
{
	sealed class KDataBindingExceptionTest : TestCase
	{
		public void TestSerializeDeserialize_WithMultipleExceptions()
		{
			var ex = new KDataBindingException();
			ex.Add(new KDataBindingException("Message1"));
			ex.Add(new KDataBindingException("Message2"));
			AssertSerializeDeserialize(ex);
		}

		public void TestSerializeDeserialize_WithMessage()
		{
			var ex = new KDataBindingException("Message");
			AssertSerializeDeserialize(ex);
		}

		public void TestSerializeDeserialize_WithDataSource()
		{
			var ex = new KDataBindingException(new List<object>());
			AssertSerializeDeserialize(ex);
		}

		void AssertSerializeDeserialize(KDataBindingException ex)
		{
#if NETFRAMEWORK
			var deserialized = SerializeDeserialize(ex);
			AssertEquals("Message after serialize / deserialize", ex.Message, deserialized.Message);
#else
			Assert(true);
#endif

		}

#if NETFRAMEWORK
		KDataBindingException SerializeDeserialize(KDataBindingException e)
		{
			return SerializationTestWithAppDomainHelper.PassBetweenAppDomains(e) as KDataBindingException;
		}
#endif
	}
}
