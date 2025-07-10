using System;
using System.IO;
using System.Net;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.IO.Testing
{
	public class FtpExceptionTest : TestCase
	{
		public void TestFullMessage()
		{
			FtpException ex = new FtpException(FtpException.FtpExceptionType.Connect, "");
			AssertEquals("", ex.FullMessage);

			ex = new FtpException(FtpException.FtpExceptionType.Connect, "asdasd");
			AssertEquals("asdasd", ex.FullMessage);

			ex = new FtpException(FtpException.FtpExceptionType.Connect, "asdasd", new ArgumentNullException("asdfasd"));
			AssertEquals("asdasd", ex.FullMessage);

			WebException innerEx = new WebException("9999999", null, WebExceptionStatus.UnknownError, null);
			ex = new FtpException(FtpException.FtpExceptionType.Connect, "asdasd", innerEx);
			AssertEquals("asdasd\r\n9999999", ex.FullMessage);

			ex = new FtpException(FtpException.FtpExceptionType.Connect, "", innerEx);
			AssertEquals("9999999", ex.FullMessage);

			ConstructorInfo constr =
				typeof(FtpWebResponse).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
				new Type[] { typeof(Stream), typeof(long), typeof(Uri), typeof(FtpStatusCode), typeof(string), typeof(DateTime), typeof(string), typeof(string), typeof(string) }, null);
			AssertNotNull("FtpWebResponse constructor", constr);
			FtpWebResponse response = (FtpWebResponse)constr.Invoke(new object[] { null, 0, null, FtpStatusCode.ArgumentSyntaxError, "12345", DateTime.Now, "", "", "" });
			innerEx = new WebException("9999999", null, WebExceptionStatus.UnknownError, response);
			ex = new FtpException(FtpException.FtpExceptionType.Connect, "sdfsdfsdf", innerEx);
			AssertEquals("sdfsdfsdf\r\n9999999\r\n12345", ex.FullMessage);

			response = (FtpWebResponse)constr.Invoke(new object[] { null, 0, null, FtpStatusCode.ArgumentSyntaxError, "12345", DateTime.Now, "", "", "" });
			innerEx = new WebException("", null, WebExceptionStatus.UnknownError, response);
			ex = new FtpException(FtpException.FtpExceptionType.Connect, "", innerEx);
			AssertEquals("12345", ex.FullMessage);

			response = (FtpWebResponse)constr.Invoke(new object[] { null, 0, null, FtpStatusCode.ArgumentSyntaxError, "12345", DateTime.Now, "", "", "" });
			innerEx = new WebException("", null, WebExceptionStatus.UnknownError, response);
			ex = new FtpException(FtpException.FtpExceptionType.Connect, "sdfsdf", innerEx);
			AssertEquals("sdfsdf\r\n12345", ex.FullMessage);

			response = (FtpWebResponse)constr.Invoke(new object[] { null, 0, null, FtpStatusCode.ArgumentSyntaxError, "12345", DateTime.Now, "", "", "" });
			innerEx = new WebException("2344", null, WebExceptionStatus.UnknownError, response);
			ex = new FtpException(FtpException.FtpExceptionType.Connect, "", innerEx);
			AssertEquals("2344\r\n12345", ex.FullMessage);
		}
	}
}
