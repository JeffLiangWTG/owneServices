using System;
using Enterprise.Customs.CustomsWare.Services;
using Enterprise.Customs.DataRegistry.Business;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class Settings : ICustomsForceWebServiceSettings
	{
		#region Construction

		public static Settings Instance
		{
			get { return instance ?? (instance = new Settings()); }
		}

		[ThreadStatic]
		static Settings instance;

		Settings()
		{
		}

		#endregion

		#region ICustomsForceWebServiceSettings

		public string Uri
		{
			get { return CustomsWareRegistry.Instance.URI.Value; }
		}

		public string UserName
		{
			get { return CustomsWareRegistry.Instance.UserName.Value; }
		}

		public string Password
		{
			get { return CustomsWareRegistry.Instance.Password.Value; }
		}

		public string Company
		{
			get { return CustomsDataRegistry.Instance.CustomsWareCompany.Value; }
		}

		public string ApplicationID
		{
			get { return CustomsWareRegistry.Instance.ApplicationID.Value; }
		}

		#endregion
	}
}
