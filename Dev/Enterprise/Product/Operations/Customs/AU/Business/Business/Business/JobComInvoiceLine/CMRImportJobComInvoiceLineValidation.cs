using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRImportJobComInvoiceLineValidation : ImportJobComInvoiceLineValidation
	{
		public CMRImportJobComInvoiceLineValidation(JobComInvoiceLine line)
			: base(line)
		{
		}

		protected JobComInvoiceLine InvoiceLine => base.Parent;

		public const string DefaultWarningForPreference = "System will DEFAULT preference and origin fields for this line from the Invoice Header details unless overriden in this line. Goods Origin and/or Preference Origin as entered on the line differs from those entered on the Invoice Header. You have not entered complete Preference Details on the line.";

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			if (!Parent.JI_Tariff.IsEmpty && (Parent.ImportTariff == null || (!AUCClassWrapper.UseCustomsReferenceData && InvoiceLine.StatClassificationWrapper == null)))
			{
				Parent.JI_TariffInfo.AddMessageError("Invalid Tariff/Stat code combination for " + Parent.JI_Tariff + ", for the date " + Parent.EffectiveDutyDate.ToString("d"));
			}

			Parent.AddInfo.Validation.ValidateZA_RNO();
			Parent.AddInfo.Validation.ValidateZA_InstrumentCode_Hidden();
			if (Parent.Declaration.IsSOFADeclaration)
			{
				Parent.JI_TariffInfo.AddWarning(TariffMayNotQualifyForSOFA);
			}
		}

		public const string TariffMayNotQualifyForSOFA = "Please be aware that not all Tariffs are applicable under the Status of Force Agreement (SOFA). Please refer to any errors returned from the ICS.";

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();

			if ((!AUCClassWrapper.UseCustomsReferenceData || CustomsUQ != AUConstants.AdditionalUQCodes.ERR || Parent.JI_CustomsUnitQty.IsEmpty)
				&& NeedsCustomsUQ && Parent.JI_CustomsUnitQty != CustomsUQ)
			{
				Parent.JI_CustomsUnitQtyInfo.AddWarning(string.Format("The tariff requires a different unit of quantity, {0}", CustomsUQ));
			}
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();

			if (IsPreferenceDefaultedFromInvoiceHeader && IsOriginDifferentToInvoiceHeaderOrigin)
			{
				InvoiceLine.JI_CountryOfOriginInfo.AddWarning(DefaultWarningForPreference);
			}
		}

		bool IsPreferenceDefaultedFromInvoiceHeader
		{
			get
			{
				bool result = false;
				if (InvoiceLine.InvoiceHeader != null)
				{
					AUAddInfo invoiceAddInfo = InvoiceLine.InvoiceHeader.AddInfo;
					result = !invoiceAddInfo.ZA_POC.IsEmpty && !invoiceAddInfo.ZA_PST.IsEmpty && !invoiceAddInfo.ZA_PRT.IsEmpty
						&& InvoiceLine.AddInfo.ZA_POC.IsEmpty && InvoiceLine.AddInfo.ZA_PST.IsEmpty && InvoiceLine.AddInfo.ZA_PRT.IsEmpty && !InvoiceLine.IsGeneralRate;
				}
				return result;
			}
		}

		bool IsOriginDifferentToInvoiceHeaderOrigin
		{
			get
			{
				bool result = false;
				if (InvoiceLine.InvoiceHeader != null)
				{
					AUAddInfo invoiceAddInfo = InvoiceLine.InvoiceHeader.AddInfo;
					result = !InvoiceLine.AddInfo.ZA_ORG.IsEmpty && InvoiceLine.AddInfo.ZA_ORG != invoiceAddInfo.ZA_ORG && InvoiceLine.AddInfo.ZA_ORG != invoiceAddInfo.ZA_POC;
				}
				return result;
			}
		}

		public override bool NeedsCustomsUQ => !InvoiceLine.JI_Tariff.IsEmpty && !CustomsUQ.IsEmpty;

		public override ZString CustomsUQ => InvoiceLine.Factory.GetValue(ref cachedCustomsUQ, () =>
		{
			if (AUCClassWrapper.UseCustomsReferenceData)
			{
				return InvoiceLine.Tariff?.ZZ1_ZZ8_UQ1 ?? ZString.Empty;
			}
			else
			{
				return InvoiceLine.StatClassificationWrapper?.QuantityUnit ?? ZString.Empty;
			}
		});
		CachedProperty<ZString> cachedCustomsUQ;

		public override ZString SecondUQ => InvoiceLine.Factory.GetValue(ref cachedSecondUQ, () =>
		{
			if (AUCClassWrapper.UseCustomsReferenceData)
			{
				return InvoiceLine.Tariff?.ZZ1_ZZ8_UQ2 ?? ZString.Empty;
			}
			else
			{
				return InvoiceLine.StatClassificationWrapper?.SecondQuantityUnit ?? ZString.Empty;
			}
		});
		CachedProperty<ZString> cachedSecondUQ;
	}
}
