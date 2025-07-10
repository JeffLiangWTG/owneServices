using System;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[Serializable]
	class InvalidOperationExceptionOnIISStartupForTest : InvalidOperationException
	{
		public InvalidOperationExceptionOnIISStartupForTest(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected InvalidOperationExceptionOnIISStartupForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public override string StackTrace => "at ASP.defaultwsdlhelpgenerator_aspx.Page_Load(Object sender, EventArgs e)";
	}
}
