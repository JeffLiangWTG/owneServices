using System;

namespace Enterprise.Customs.CustomsWare.Services.Testing
{
	public class CustomsForceWebServiceSettingsWithException : ICustomsForceWebServiceSettings
	{
		#region ICustomsForceWebServiceSettings

		public string Uri
		{
			get
			{
				var inner = new Exception("Yo mama was a snowblower");
				throw new NotImplementedException("Invalid URI: The format of the URI could not be determined.", inner);
			}
		}

		public string UserName
		{
			get { return ""; }
		}

		public string Password
		{
			get { return ""; }
		}

		public string Company
		{
			get { return ""; }
		}

		public string ApplicationID
		{
			get { return ""; }
		}

		#endregion
	}
}
