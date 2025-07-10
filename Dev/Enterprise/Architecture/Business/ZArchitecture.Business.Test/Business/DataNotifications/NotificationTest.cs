using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
#if NETFRAMEWORK
using NUnit.Framework.TestHelper;
#endif

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestsSubclassesOf(typeof(NotificationSubscriberNotification))]
	public abstract class NotificationTest<T> : TestCaseWithFactory where T : NotificationSubscriberNotification
	{
		public void TestSerialization()
		{
			T constructed = NewTestNotification();
			if (IsSerializable)
			{
				T deserialized = SerializeDeserialize(constructed);
				AssertNotificationsEqual("Constructed and Deserialized notification objects should equal", constructed, deserialized);
			}
			else
			{
				try
				{
					T deserialized = SerializeDeserialize(constructed);
					Fail("Expected a SerializationException so that if serialization is not supported, the code fails fast so that unexpected (and inconsistent) results don't occur");
				}
				catch (System.Runtime.Serialization.SerializationException)
				{
					Assert(true);
				}
			}
		}

		protected virtual void AssertNotificationsEqual(string message, T lhs, T rhs)
		{
			AssertEquals(message + "; Type", lhs.Type, rhs.Type);
			AssertEquals(message + "; AdditionalInfo", lhs.AdditionalInfo, rhs.AdditionalInfo);
			AssertEquals(message + "; DisplayMessage", lhs.Message.Trim(), rhs.Message.Trim());
			AssertEquals(message + "; MultiLineDisplayMessage", lhs.MultiLineDisplayMessage.Trim(), rhs.MultiLineDisplayMessage.Trim());
			AssertEquals(message + "; ShouldBeDisplayedOnBatchProcessor", lhs.ShouldBeDisplayedOnBatchProcessor, rhs.ShouldBeDisplayedOnBatchProcessor);
		}

		protected abstract T NewTestNotification();

		protected virtual bool IsSerializable
		{
			get { return true; }
		}

		#region Implementation

		T SerializeDeserialize(T notification)
		{
#if NETFRAMEWORK
			return (T)SerializationTestWithAppDomainHelper.PassBetweenAppDomains(notification);
#else
			throw new System.Exception("not support netcore");
#endif
		}

		#endregion
	}
}
