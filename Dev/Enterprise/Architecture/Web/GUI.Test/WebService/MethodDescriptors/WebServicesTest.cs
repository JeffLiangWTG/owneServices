using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	[HttpContextEnabledTest]
	public class WebServicesTest : TestCaseWithFactory
	{
		#region Test Cases

		public virtual void TestInstance()
		{
			AssertNotNull(WebServices.Instance);
			AssertEquals(typeof(WebServices), WebServices.Instance.GetType());
		}

		public virtual void TestSharedWebServiceReference()
		{
			AssertNotNull(WebServices.Instance.SharedWebServiceReference);
			AssertEquals("/WebService/WebServiceShared.asmx", WebServices.Instance.SharedWebServiceReference.Path);
		}

		#endregion
	}
}
