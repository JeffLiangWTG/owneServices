using System;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[Serializable]
	class NullReferenceExceptionOnIISStartupForTest : NullReferenceException
	{
		public NullReferenceExceptionOnIISStartupForTest(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected NullReferenceExceptionOnIISStartupForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public override string StackTrace => "at ASP.defaultwsdlhelpgenerator_aspx.Page_Load(Object sender, EventArgs e)";
	}
}
