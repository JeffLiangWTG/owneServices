using System.Linq;
using System.ServiceModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CustomsWare.Services.Testing
{
	public class CustomsWareServicesTest : TestCaseWithFactory
	{
		public void TestExecAPI()
		{
			var response = new CustomsWareServices().ExecApiSafe(new CustomsForceWebServiceSettingsWithException(), "<Nothing></Nothing>");
			var errors =
				from error in response.DescendantsAndSelf("ErrorItem") select error;
			var errorList = new ZStringBuilder();
			foreach (var error in errors)
			{
				var element = error.Descendants("ErrorDescription").FirstOrDefault();
				errorList.Append("Error Description: ");
				errorList.Append(element.Value);
			}

			AssertEquals("Error Description: Invalid URI: The format of the URI could not be determined.\r\nYo mama was a snowblower", errorList.ToString());
		}

		public void TestSchema()
		{
			var settings = new CustomsForceWebServiceSettingsForTest { Uri = "http://www.google.com" };
			var binding = new CustomsWareServices().CreateHttpBinding(settings);
			AssertEquals("If the url is http, we set security mode to None.", BasicHttpSecurityMode.None, binding.Security.Mode);
			settings.Uri = "https://www.google.com";
			binding = new CustomsWareServices().CreateHttpBinding(settings);
			AssertEquals("If the url is https, we set the security mode to Transport.", BasicHttpSecurityMode.Transport, binding.Security.Mode);
		}
	}
}
