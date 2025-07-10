using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.PAVE.MENT.Business
{
	public class WebBrowserSectionConfiguration : NonPersistentBusinessObject<WebBrowserSectionConfigurationValidation>, IBoardSectionConfigurationBizo
	{
		public WebBrowserSectionConfiguration(IBMBoardSection section)
			: base(section.Factory)
		{
			Argument.NotNull(section, nameof(section));
		}

		#region XML Properties

		[XmlColumnProperty]
		public ZString URL
		{
			get { return GetXmlColumnPropertyValue<ZString>(URLInfo); }
			set
			{
				SetXmlColumnPropertyValue(URLInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateURL();
				}
			}
		}

		public ZPropertyInfo URLInfo
		{
			get { return GetZPropertyInfo(nameof(URL)); }
		}

		#endregion

		#region New Properties

		public Uri Address
		{
			get
			{
				Uri address = null;

				if (Uri.IsWellFormedUriString(URL, UriKind.Absolute))
				{
					address = new Uri(URL);
				}

				return address;
			}
		}

		#endregion

		#region BusinessObject Overrides

		public override WebBrowserSectionConfigurationValidation GetNewValidation()
		{
			return new WebBrowserSectionConfigurationValidation(this);
		}

		#endregion

		#region IBoardSectionConfigurationBizo Members

		public ZString SectionName
		{
			get { return URL; }
		}

		public ZPropertyInfo SectionNameInfo
		{
			get { return GetZPropertyInfo(nameof(SectionName)); }
		}

		public void CopyConfigurationPropertiesToNewSection(IBMBoardSection section)
		{
		}

		#endregion
	}
}
