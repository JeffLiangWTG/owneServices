using System;
using System.Xml.Schema;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[Serializable]
	class XmlSchemaExceptionOnIISStartupForTest : XmlSchemaException
	{
		public XmlSchemaExceptionOnIISStartupForTest(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected XmlSchemaExceptionOnIISStartupForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public override string StackTrace => "at ASP.defaultwsdlhelpgenerator_aspx.Page_Load(Object sender, EventArgs e)";
	}
}
