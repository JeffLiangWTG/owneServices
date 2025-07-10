using System;
using System.Threading;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Test
{
	sealed class NewSchemaFormTest : TestCase
	{
		public void TestUsingDbConnectionOnControllerThreadStart()
		{
			var exMsg = "";
			var lastMessageReported = "";
			ErrorReporter.Clear();

			var thread = new Thread(() =>
			{
				try
				{
					using (var form = new NewSchemaFormForTest())
					{
						form.OnControllerThreadStart_Exposed();
					}
				}
				catch (InvalidOperationException ex)
				{
					exMsg = ex.Message;
					lastMessageReported = ErrorReporter.LastMessageReported;
				}
			});
			thread.Start();
			thread.Join();

			ErrorReporter.Clear();
			AssertEquals("", lastMessageReported);
			AssertEquals("throw exception from ControllerFactory", exMsg);
		}
	}
}
