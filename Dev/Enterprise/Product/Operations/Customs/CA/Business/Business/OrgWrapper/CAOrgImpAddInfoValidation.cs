//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCAOrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoCAOrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;

	public class CAOrgImpAddInfoValidation : AutoCAOrgImpAddInfoValidation
	{
		public CAOrgImpAddInfoValidation(AutoCAOrgImpAddInfo parent)
			: base(parent)
		{
		}

		new OrgImpAddInfo Parent
		{
			get { return base.Parent as OrgImpAddInfo; }
		}

		protected override void CheckZO_CFIAFeePaymentMethod()
		{
			base.CheckZO_CFIAFeePaymentMethod();
			ListValidation.ErrorIfInvalidCode(Parent.ZO_CFIAFeePaymentMethodInfo);
			if (Parent.ZO_EffectiveCFIAFeePaymentMethod == CFIAPaymentMethods.Codes.Importer)
			{
				var orgCusCode = Parent.OrgHeader?.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CACodeTypes.CFIAAccountNumber, Core.Constants.CountryCodes.Canada);
				if (orgCusCode == null || orgCusCode.Length == 0)
				{
					Parent.ZO_CFIAFeePaymentMethodInfo.AddMessageError(OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnImporterErrorText);
				}
			}
		}

		protected override void CheckZO_LVSInvoiceDetailCode()
		{
			base.CheckZO_LVSInvoiceDetailCode();
			ListValidation.ErrorIfInvalidCode(Parent.ZO_LVSInvoiceDetailCodeInfo);
		}

		protected override void CheckZO_HVSDelayIntervalTypeAutoSend()
		{
			base.CheckZO_HVSDelayIntervalTypeAutoSend();
			ListValidation.ErrorIfInvalidCode(Parent.ZO_HVSDelayIntervalTypeAutoSendInfo);
		}

		protected override void CheckZO_CONDelayIntervalTypeAutoSend()
		{
			base.CheckZO_CONDelayIntervalTypeAutoSend();
			ListValidation.ErrorIfInvalidCode(Parent.ZO_CONDelayIntervalTypeAutoSendInfo);
		}

		protected override void CheckZO_HVSDelayIntervalTypeFailSafe()
		{
			base.CheckZO_HVSDelayIntervalTypeFailSafe();
			ListValidation.ErrorIfInvalidCode(Parent.ZO_HVSDelayIntervalTypeFailSafeInfo);
		}

		protected override void CheckZO_CONDelayIntervalTypeFailSafe()
		{
			base.CheckZO_CONDelayIntervalTypeFailSafe();
			ListValidation.ErrorIfInvalidCode(Parent.ZO_CONDelayIntervalTypeFailSafeInfo);
		}

		protected override void CheckZO_DeferredNormalB3SendAction()
		{
			base.CheckZO_DeferredNormalB3SendAction();
			ListValidation.ErrorIfInvalidCode(Parent.ZO_DeferredNormalB3SendActionInfo);
		}

		protected override void CheckZO_DeferredLowValueB3SendAction()
		{
			base.CheckZO_DeferredLowValueB3SendAction();
			ListValidation.ErrorIfInvalidCode(Parent.ZO_DeferredLowValueB3SendActionInfo);
		}

		protected override void CheckZO_CONDelayIntervalAutoSend()
		{
			base.CheckZO_CONDelayIntervalAutoSend();
			if (Parent.ZO_CONDelayIntervalTypeAutoSend == DelayIntervalTypeCodes.Codes.DAY)
			{
				MandatoryValidation.CheckNotZero(Parent.ZO_CONDelayIntervalAutoSendInfo);
				MandatoryValidation.CheckNotNegative(Parent.ZO_CONDelayIntervalAutoSendInfo);
				if (Parent.ZO_CONDelayIntervalAutoSend >= 25)
				{
					Parent.ZO_CONDelayIntervalAutoSendInfo.AddError(GoodsNotLateThan25ThMessage);
				}
			}
		}

		protected override void CheckZO_CONDelayIntervalFailSafe()
		{
			base.CheckZO_CONDelayIntervalFailSafe();
			if (Parent.ZO_CONDelayIntervalTypeFailSafe == DelayIntervalTypeCodes.Codes.DAY)
			{
				MandatoryValidation.CheckNotZero(Parent.ZO_CONDelayIntervalFailSafeInfo);
				MandatoryValidation.CheckNotNegative(Parent.ZO_CONDelayIntervalFailSafeInfo);
				if (Parent.ZO_CONDelayIntervalFailSafe >= 25)
				{
					Parent.ZO_CONDelayIntervalFailSafeInfo.AddError(GoodsNotLateThan25ThMessage);
				}
			}
		}

		protected override void CheckZO_AccountingTimeOption()
		{
			base.CheckZO_AccountingTimeOption();
			if (Parent.IsCSAApprovedImporter)
			{
				MandatoryValidation.CheckEntered(Parent.ZO_AccountingTimeOptionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ZO_AccountingTimeOptionInfo);
			}
		}

		static string GoodsNotLateThan25ThMessage => Res.GetString("42AE9FBA-D4E8-4ABE-A4D3-BA43E48D8658", "Goods must be accounted for by the 24th of the month.");
	}
}
