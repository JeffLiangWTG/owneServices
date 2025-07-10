using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.Common.GUI
{
	public class FindBoxWrapperForBorderWise : IFindBoxPopup
	{
		BorderWiseLauncher borderWiseLauncher;
		BorderWiseFilters borderWiseFilters;

		public FindBoxWrapperForBorderWise(string parameterForBorderWise)
		{
			AdditionalData = new AdditionalDataForBorderWise(parameterForBorderWise, ZDateTime.Today);
		}

		public FindBoxWrapperForBorderWise(string parameterForBorderWise, string countryCode)
		{
			AdditionalData = new AdditionalDataForBorderWise(parameterForBorderWise, ZDateTime.Today, countryCode);
		}

		public FindBoxWrapperForBorderWise(AdditionalDataForBorderWise additionalData)
		{
			AdditionalData = additionalData;
		}

		public FindBoxWrapperForBorderWise(BorderWiseFilters borderWiseFilters)
		{
			this.borderWiseFilters = borderWiseFilters;
			AdditionalData = borderWiseFilters.AdditionalData;
		}

		#region IFindBoxPopup Members

		public void SelectRowByPK(ZGuid pK)
		{
		}

		public void Dispose()
		{
			if (borderWiseFilters != null)
			{
				borderWiseFilters = null;
			}

			if (borderWiseLauncher != null)
			{
				borderWiseLauncher = null;
			}
		}

#if DEBUG
		public
#endif
		readonly AdditionalDataForBorderWise AdditionalData;

		public BorderWiseLauncher BorderWiseLauncher
		{
			get
			{
				return borderWiseLauncher ?? (borderWiseLauncher = new BorderWiseLauncher(AdditionalData.CountryCodeOverride));
			}
			set
			{
				borderWiseLauncher = value;
			}
		}

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			try
			{
				if (BorderWiseLauncher != null)
				{
					if (borderWiseFilters == null)
					{
						borderWiseFilters = new BorderWiseFilters(findBox.Code)
						{
							AdditionalData = AdditionalData,
							ImpExp = AdditionalData.ParameterForBorderWise,
							CountryCodeOverride = AdditionalData.CountryCodeOverride
						};
					}

					BorderWiseLauncher.ClassifyCommoditiesInBorderWise(findBox, parentForm, borderWiseFilters);
				}
			}
			finally
			{
				borderWiseFilters = null;
			}
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		event EventHandler IFindBoxPopup.Closed
		{
			add { }
			remove { }
		}

		#endregion

		public void UpdateAdditionalDataCountryCode(string countryCode)
		{
			AdditionalData.CountryCodeOverride = countryCode;
		}
	}
}
