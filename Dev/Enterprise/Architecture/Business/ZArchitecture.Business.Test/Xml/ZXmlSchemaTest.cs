using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Xml
{
	sealed class ZXmlSchemaTest : TestCase
	{
		public void TestTry3Times_WithinRetryLimit()
		{
			AssertEquals("Precondition: performedAction should be false.", false, performedAction);
			exceptionsToThrow = 2;
			schema.Try3Times(delegate
			{ PerformAction(); });
			AssertEquals("performedAction", true, performedAction);
		}

		[ExpectExceptionMessage(typeof(ExternalException), "Exception thrown.")]
		public void TestTry3Times_ExceedRetryLimit()
		{
			exceptionsToThrow = 3;
			schema.Try3Times(delegate
			{ PerformAction(); });
		}

		void PerformAction()
		{
			if (exceptionsToThrow > 0)
			{
				exceptionsToThrow--;
				throw new ExternalException("Exception thrown.");
			}
			else
			{
				performedAction = true;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			schema = new ZXmlSchema();
			exceptionsToThrow = 0;
			performedAction = false;
		}

		ZXmlSchema schema;
		int exceptionsToThrow;
		bool performedAction;
	}
}
