using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.AU;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ASPChargeCodeGuidFindBoxRegistryItemEditor : GuidFindBoxRegistryItemEditor
	{
		public ASPChargeCodeGuidFindBoxRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallback, BusinessObjectFactory factory) : base(dataType, editorInfo, fallback)
		{
			this.Factory = factory;
		}
		protected BusinessObjectFactory Factory;

		public override string GetCustomValidation(Control editorPane)
		{
			string errorMessage = base.GetCustomValidation(editorPane);

			if (string.IsNullOrEmpty(errorMessage))
			{
				var companyPK = Fallback.CompanyPK(false);

				var disbursementChargeCodes = Business.RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
				if (disbursementChargeCodes.Cast<Business.Customs.EntryChargeTypeSetting>().Any(x => x.ChargeType == EntryChargeTypeList.Codes.AQISServicePaymentAmount))
				{
					errorMessage = Res.GetString("F99C3E88-4B26-443F-9250-A5B59272605E", "ASP Charge Type has already been mapped in Disbursement Charge Code Override");
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					var defaultDisbursementChargeCode = Business.RatingDataRegistry.Instance.CustomsDisbursementChargeCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
					var deferredChargeCode = Business.RatingDataRegistry.Instance.CustomDeferredChargeCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);

					var currentData = (Guid)base.GetValueFromEditorPaneCore(editorPane);

					if (currentData != Guid.Empty && (defaultDisbursementChargeCode == currentData || deferredChargeCode == currentData || disbursementChargeCodes.Cast<Business.Customs.EntryChargeTypeSetting>().Any(x => x.AC_ChargeCode == currentData)))
					{
						errorMessage = Res.GetString("3B51DF0F-3718-40DD-AF45-CE77355178BA", "The value should not be same to \"Default Disbursement Charge Code\", \"Deferred Charge Code\" and \"Disbursement Charge Code Override\".");
					}
				}
			}

			return errorMessage;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			var currentData = (editorPane.Tag as ChargeCodeWithDate) ?? new ChargeCodeWithDate();
			currentData.ChargeCode = (Guid)base.GetValueFromEditorPaneCore(editorPane);

			return currentData;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var currentData = (value as ChargeCodeWithDate) ?? new ChargeCodeWithDate();
			editorPane.Tag = currentData.Clone(Fallback, Factory);

			var chargeCode = currentData.ChargeCode.IsValid ? currentData.ChargeCode.ToGuid() : Guid.Empty;
			base.SetValueFromEditorPaneCore(editorPane, chargeCode);
		}
	}
}
