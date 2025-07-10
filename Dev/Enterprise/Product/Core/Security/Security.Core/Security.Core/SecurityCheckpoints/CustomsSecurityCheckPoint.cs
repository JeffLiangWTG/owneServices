using System;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security
{
	public class CustomsSecurityCheckPoint : SecurityCheckpoint
	{
		public CustomsSecurityCheckPoint(string code, MultilingualString displayText, ISecurityCheckpoint parent, IZSecurity security, string country)
			: base(code, displayText, parent, security, true, country, Guid.Empty)
		{
		}

		public override bool Visible
		{
			get { return string.IsNullOrEmpty(Country) || (Country == Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(EnvProxy.Instance.CurrentCompany.Country.Code)); }
		}
	}
}
