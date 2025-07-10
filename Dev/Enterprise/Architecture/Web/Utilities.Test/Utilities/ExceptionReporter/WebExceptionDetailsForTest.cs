using System;
using System.Xml;

namespace Enterprise.ZArchitecture.Web.Utilities.Exceptions.Testing
{
	internal class WebExceptionDetailsForTest : WebExceptionDetails
	{
		public WebExceptionDetailsForTest(Exception ex)
		: base(ex) { }

		public void WriteWebInfoForTest(XmlTextWriter xtw)
		{
			base.WriteWebInfo(xtw);
		}
	}
}
