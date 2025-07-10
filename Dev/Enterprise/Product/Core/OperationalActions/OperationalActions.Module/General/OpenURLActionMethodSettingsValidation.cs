using System;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Module
{
	public class OpenURLActionMethodSettingsValidation : ZValidation
	{
		public OpenURLActionMethodSettingsValidation(OpenURLActionMethodSettings parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public override void ValidateAll()
		{
			ValidateURL();
		}

		#region ValidateURL

		public void ValidateURL()
		{
			((IValidationInternals)this).Validate(Parent.URLInfo, CheckURL);
		}

		public void CheckURL()
		{
			MandatoryValidation.CheckEntered(Parent.URLInfo);
			if (!UrlValidation.IsValidUrl(Parent.URL))
			{
				Parent.URLInfo.AddError(Res.GetString("cdca7636-5fb0-487e-8cf4-6edf7f46a9da", "Please enter a valid URL.\r\n\r\nA valid address is commonly found in the format \"{0}\" or \"{1}\"", "http://", "www."));
			}
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(OpenURLActionMethodSettingsValidation); }
		}

		public OpenURLActionMethodSettings Parent
		{
			get { return parent; }
		}
		readonly OpenURLActionMethodSettings parent;

		#endregion
	}
}
