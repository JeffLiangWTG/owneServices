using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public partial class JobComInvoiceLineValidation : AutoBRJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		protected override bool IsJIProcedureMandatory => Parent.IsExport;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNaladiNcca();
			ValidateNaladiHs();
			ValidateComplementaryDescription();
			ValidateMercosulForeignDeclarationType();
			ValidateDutyVigentRateValue();
			ValidateIPIVigentRateValue();
			ValidateIPIRateIsOverridden();
			ValidateFTAMarginRateValue();
			ValidateReductionMarginRateValue();
			ValidateReducedDutyRateValue();
			ValidatePisVigentRateValue();
			ValidatePisRateIsOverridden();
			ValidateCofinsVigentRateValue();
			ValidateCofinsRateIsOverridden();
		}

		protected override void CheckJI_CEI()
		{
			if (Parent.Declaration?.IsPersistent ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CEIInfo);
			}
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_InvoiceQuantityInfo);
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_NetWeightInfo);
		}

		protected override void CheckJI_NetWeightUQ()
		{
			base.CheckJI_NetWeightUQ();

			if (Parent.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_NetWeightUQInfo);
			}
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			ValidateJI_CustomsQuantity();
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CustomsQuantityInfo);

			if (Parent.IsImport && Parent.JI_CustomsQuantity > 0 && !Parent.JI_CustomsUnitQty.IsEmpty)
			{
				if (Parent.JI_InvoiceQuantity > 0 && !Parent.JI_InvoiceUQ.IsEmpty && Parent.Lookups.InvoiceUQList.ContainsCode(Parent.JI_InvoiceUQ))
				{
					var convertedCustomsQty = Parent.UnitConverter.Convert(Parent.JI_InvoiceQuantity, Parent.JI_InvoiceUQ, Parent.JI_CustomsUnitQty).Round(5);
					if (convertedCustomsQty > 0 && Parent.JI_CustomsQuantity != convertedCustomsQty)
					{
						Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("4A57E96A-066F-4D70-99E3-FA39CB1599C7", "Customs Qty is different from the value calculated by the system"));
					}
				}

				if (Parent.JI_NetWeight > 0 && !Parent.JI_NetWeightUQ.IsEmpty && Parent.Lookups.WeightUQList.ContainsCode(Parent.JI_NetWeightUQ))
				{
					var convertedCustomsQty = Parent.UnitConverter.Convert(Parent.JI_NetWeight, Parent.JI_NetWeightUQ, Parent.JI_CustomsUnitQty).Round(5);
					if (convertedCustomsQty > 0 && Parent.JI_CustomsQuantity != convertedCustomsQty)
					{
						Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("59B271D0-B202-41A7-92C0-D66AA0F7525B", "Customs Qty is different from the value calculated by the system"));
					}
				}
			}
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_LinePriceInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();

			if (Parent.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_InvoiceUQInfo);
			}

			if (Parent.IsImportLicense || Parent.IsImportSiscomex)
			{
				if (!Parent.JI_InvoiceUQ.IsEmpty)
				{
					var (descInEn, descInPt) = Parent.Factory.GetInvoiceUQDescriptions(Parent.Lookups.InvoiceUQList, Parent.JI_InvoiceUQ);
					if (!descInEn.IsEmpty && !descInPt.IsEmpty && descInEn == descInPt)
					{
						Parent.JI_InvoiceUQInfo.AddMessageError(Res.GetString("1B12C1B3-B52A-4821-A9F6-58E6122EFDED", "Invoice Qty- UQ Portuguese description is the same as English Description"));
					}
				}
			}
		}

		public void ValidateNaladiHs()
		{
			ValidateCalculatedProperty(Parent.NaladiHsInfo);
		}

		protected void CheckNaladiHs()
		{
			if ((Parent.IsImportSiscomex || Parent.IsImportLicense) && !Parent.NaladiHs.IsEmpty && !Parent.NaladiHs.IsNumbersOnlyOrEmpty)
			{
				Parent.NaladiHsInfo.AddMessageError(Res.GetString("8e47daf3-1244-49d8-94d8-b55791546d7e", "NALADI/HS must consist 8 numeric characters."));
			}
		}

		public void ValidateNaladiNcca()
		{
			ValidateCalculatedProperty(Parent.NaladiNccaInfo);
		}

		protected void CheckNaladiNcca()
		{
			if (Parent.IsImportSiscomex && !Parent.NaladiNcca.IsEmpty && !Parent.NaladiNcca.IsNumbersOnlyOrEmpty)
			{
				Parent.NaladiNccaInfo.AddMessageError(Res.GetString("574040dd-a412-4af0-93eb-c3aaf1c948f0", "NALADI/NCCA must consist 8 numeric characters."));
			}
		}

		public void ValidateComplementaryDescription()
		{
			ValidateCalculatedProperty(Parent.ComplementaryDescriptionInfo);
		}

		protected virtual void CheckComplementaryDescription()
		{
		}

		public void ValidateMercosulForeignDeclarationType()
		{
			ValidateCalculatedProperty(Parent.MercosulForeignDeclarationTypeInfo);
		}

		protected virtual void CheckMercosulForeignDeclarationType()
		{
		}

		public void ValidatePisRateIsOverridden()
		{
			ValidateCalculatedProperty(Parent.PisRateIsOverriddenInfo);
		}

		protected virtual void CheckPisRateIsOverridden()
		{
		}

		public void ValidateCofinsRateIsOverridden()
		{
			ValidateCalculatedProperty(Parent.CofinsRateIsOverriddenInfo);
		}

		protected virtual void CheckCofinsRateIsOverridden()
		{
		}

		public void ValidateIPIRateIsOverridden()
		{
			ValidateCalculatedProperty(Parent.IPIRateIsOverriddenInfo);
		}

		protected virtual void CheckIPIRateIsOverridden()
		{
		}

		public void ValidateIPIVigentRateValue()
		{
			ValidateCalculatedProperty(Parent.IPIVigentRateValueInfo);
		}

		protected virtual void CheckIPIVigentRateValue()
		{
		}

		public void ValidateDutyVigentRateValue()
		{
			ValidateCalculatedProperty(Parent.DutyVigentRateValueInfo);
		}

		protected virtual void CheckDutyVigentRateValue()
		{
		}

		public void ValidateFTAMarginRateValue()
		{
			ValidateCalculatedProperty(Parent.FTAMarginRateValueInfo);
		}

		protected virtual void CheckFTAMarginRateValue()
		{
		}

		public void ValidateReductionMarginRateValue()
		{
			ValidateCalculatedProperty(Parent.ReductionMarginRateValueInfo);
		}

		protected virtual void CheckReductionMarginRateValue()
		{
		}

		public void ValidateReducedDutyRateValue()
		{
			ValidateCalculatedProperty(Parent.ReducedDutyRateValueInfo);
		}

		protected virtual void CheckReducedDutyRateValue()
		{
		}

		public void ValidatePisVigentRateValue()
		{
			ValidateCalculatedProperty(Parent.PisVigentRateValueInfo);
		}

		protected virtual void CheckPisVigentRateValue()
		{
		}

		public void ValidateCofinsVigentRateValue()
		{
			ValidateCalculatedProperty(Parent.CofinsVigentRateValueInfo);
		}

		protected virtual void CheckCofinsVigentRateValue()
		{
		}

		public void ValidateFullGoodsDescription()
		{
			ValidateCalculatedProperty(Parent.FullGoodsDescriptionInfo);
		}

		protected virtual void CheckFullGoodsDescription()
		{
		}

		public void ValidateICMSFCPRateValue()
		{
			ValidateCalculatedProperty(Parent.ICMSFCPRateValueInfo);
		}

		protected virtual void CheckICMSFCPRateValue()
		{
		}
	}
}
