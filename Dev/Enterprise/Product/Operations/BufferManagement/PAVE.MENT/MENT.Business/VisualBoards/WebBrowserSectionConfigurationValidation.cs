using System;
using CargoWise.EntityFramework;

namespace Enterprise.PAVE.MENT.Business
{
	public class WebBrowserSectionConfigurationValidation : ZValidation
	{
		public WebBrowserSectionConfigurationValidation(WebBrowserSectionConfiguration sectionConfiguration)
			: base(sectionConfiguration)
		{
			Parent = sectionConfiguration;
		}

		readonly WebBrowserSectionConfiguration Parent;

		public void ValidateURL()
		{
			ValidateCalculatedProperty(Parent.URLInfo);
		}

		protected void CheckURL()
		{
			if (Parent.Address == null)
			{
				Parent.URLInfo.AddError(Res.GetString("0871669f-300c-4ea5-89c7-eb1085c6353f", "The URL you have entered does not contain a protocol. Please include {0}, {1} in front of the address. The address should look like: {2}", "http://", "https://", "http://www.somewebsite.com"));
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(WebBrowserSectionConfigurationValidation); }
		}

		public override void ValidateAll()
		{
			ValidateURL();
		}
	}
}
