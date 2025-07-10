
namespace Enterprise.Customs.CA.Business
{
	using Enterprise.Customs.Common;

	public abstract partial class AutoJobComInvoiceLine
	{
		#region AddInfo Properties

		#region CA_99TariffCode

		public TariffPropertyInfo CA_99TariffCodeTariffInfo
		{
			get { return new TariffPropertyInfo(TariffType.Import, EffectiveDateForDutyRate, "99"); }
		}

		#endregion

		#region CA_CFIAUSStateOfOrigin

		protected bool CA_CFIAUSStateOfOrigin_ReadOnly
		{
			get
			{
				return
					InvoiceHeader != null &&
					InvoiceHeader.IsImport &&
					CA_RN_NKCFIAOrigin != Core.Constants.CountryCodes.UnitedStates;
			}
		}

		#endregion

		#region CA_CustomsValue

		public bool CA_CustomsValue_ReadOnly
		{
			get { return !CA_CustomsValueOvr; }
		}

		#endregion

		#region CA_CVforCurrConv

		public bool CA_CVforCurrConv_ReadOnly
		{
			get { return !CA_CVforCurrConvOvr; }
		}

		#endregion

		#region JI_StateOrRegionOfOrigin

		internal bool JI_StateOrRegionOfOrigin_ReadOnly
		{
			get
			{
				return InvoiceHeader != null
					&& InvoiceHeader.IsImport
					&& JI_CountryOfOrigin != Core.Constants.CountryCodes.UnitedStates
					&& JI_CountryOfOrigin != Core.Constants.CountryCodes.Canada;
			}
		}

		#endregion

		#region CA_CFIAStateOfSource

		protected bool CA_CFIAStateOfSource_ReadOnly
		{
			get
			{
				return
					InvoiceHeader != null &&
					InvoiceHeader.IsImport &&
					CA_CFIACountryOfSource != Core.Constants.CountryCodes.UnitedStates;
			}
		}

		#endregion

		#region CA_USStateOfExport

		protected bool CA_USStateOfExport_ReadOnly
		{
			get { return InvoiceHeader != null && InvoiceHeader.IsImport && CA_RN_NKExport != Core.Constants.CountryCodes.UnitedStates; }
		}

		#endregion

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public override void OnLoaded()
		{
			var oldHasChanges = AddInfo.HasChanges;
			try
			{
				base.OnLoaded();
				AddInfo.LoadPropertiesFromString(JI_AddInfo);
			}
			finally
			{
				AddInfo.HasChanges = oldHasChanges;
			}
		}

		internal AddInfoJobComInvoiceLine GetAddInfo()
		{
			return AddInfo;
		}

		#endregion
	}
}
