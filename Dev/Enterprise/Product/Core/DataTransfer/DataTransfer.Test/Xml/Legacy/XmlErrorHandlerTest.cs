using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	public class XmlErrorHandlerTest : TestCase
	{
		public void TestHasErrors()
		{
			XmlErrorHandler.Errors.Add("TEST");
			AssertEquals(true, XmlErrorHandler.HasErrors);

			XmlErrorHandler.Errors.Clear();
			AssertEquals(false, XmlErrorHandler.HasErrors);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			XmlErrorHandler = new XmlErrorHandlerTestClass();
		}

		XmlErrorHandlerTestClass XmlErrorHandler;

		protected class XmlErrorHandlerTestClass : XmlErrorHandler
		{
			public XmlErrorHandlerTestClass() : base()
			{
			}
		}

		#endregion
	}
}
