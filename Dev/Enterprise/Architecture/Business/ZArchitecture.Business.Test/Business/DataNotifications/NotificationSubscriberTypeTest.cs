using NUnit.Framework;
#if NETFRAMEWORK
using NUnit.Framework.TestHelper;
#endif

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestsSubclassesOf(typeof(CargoWise.EntityFramework.NotificationType))]
	public abstract class NotificationSubscriberTypeTest<T> : TestCase where T : NotificationSubscriberType
	{
		public void TestSerialization()
		{
			T constructed = NewNotificationType("Name");
			T deserialized = (T)SerializeDeserialize(constructed);
			AssertNotificationTypesEqual("Constructed NotificationType should be the same after serialized/deserialized", constructed, deserialized);
		}

		protected virtual void AssertNotificationTypesEqual(string message, T lhs, T rhs)
		{
			AssertEquals(message, true, lhs == rhs);
			AssertEquals(message, lhs.GetDisplayMessage("AdditionalInfo"), rhs.GetDisplayMessage("AdditionalInfo"));
		}

		protected abstract T NewNotificationType(string message);
		protected abstract T NewNotificationType(string name, string message);

		#region Implementation

		object SerializeDeserialize(object obj)
		{
#if NETFRAMEWORK
			return SerializationTestWithAppDomainHelper.PassBetweenAppDomains(obj);
#else
			throw new System.Exception("not support netcore");
#endif
		}

		#endregion
	}
}
