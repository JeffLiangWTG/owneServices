using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface.Testing
{
	[TestedType(typeof(PODErrorType))]
	internal class PODErrorTypeTest : NotificationSubscriberTypeTest<PODErrorType>
	{
		public void TestStaticErrorTypes()
		{
			TestStaticErrorTypesCore();
		}

		protected virtual void TestStaticErrorTypesCore()
		{
			AssertEquals("ImportError", PODErrorType.ImportError.Name);
			AssertEquals("Import Error", PODErrorType.ImportError.Message);
		}

		#region Implementation
		protected override PODErrorType NewNotificationType(string name)
		{
			return new PODErrorType(name);
		}

		protected override PODErrorType NewNotificationType(string name, string message)
		{
			return new PODErrorType(name, message);
		}
		#endregion
	}
}
