using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidation(JobComInvoiceLine line)
			: base(line)
		{
		}

		public override CodeDescriptionPairList CustomsUQList
		{
			get { return Parent.Factory.GetCachedValue<AUCustomsExportUQList>(); }
		}

		public override bool NeedsCustomsUQ
		{
			get { return Parent.ExportTariff != null && Parent.ExportTariff.ZZ1_ZZ8_UQ1 != QuantityNotRequired; }
		}

		public override ZString CustomsUQ
		{
			get
			{
				var result = ZString.Empty;

				if (Parent.ExportTariff != null)
				{
					result = Parent.ExportTariff.ZZ1_ZZ8_UQ1;
				}
				else if (Parent.JI_Tariff.Left(2) == "99")
				{
					result = QuantityNotRequired;
				}

				return result;
			}
		}

		const string QuantityNotRequired = "NR";

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			if (Parent.JI_Tariff.Length != 10)
			{
				Parent.JI_TariffInfo.AddMessageError("Export Tariff is incomplete.");
			}
			else if (Parent.JI_Tariff.Left(2) != "99" && Parent.ExportTariff == null)
			{
				Parent.JI_TariffInfo.AddMessageError("Export Tariff is invalid.");
			}
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			if (Parent.JI_Description.Length < 2)
			{
				Parent.JI_DescriptionInfo.AddMessageError("The goods description must be at least 2 characters long");
			}

			if ((Parent.JI_Description.ToUpper()).IndexOfAny("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()) == -1)
			{
				Parent.JI_DescriptionInfo.AddMessageError("The goods description must contain at least one non-numeric character");
			}

			if (Parent.JI_Description.Length > 128)
			{
				Parent.JI_DescriptionInfo.AddWarning("Only the first 128 characters of this description will be sent to Customs.");
			}
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo);

			if (Parent.JI_CountryOfOrigin.IsEmpty)
			{
				Parent.JI_CountryOfOriginInfo.AddMessageError("A country/region of origin is required.");
			}
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			ListValidation.WarnIfInvalidCode(Parent.JI_InvoiceUQInfo, CombinedUQList);
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			if (Parent.JI_WeightUQ.ToString() == Parent.JI_CustomsUnitQty.ToString())
			{
				if (Parent.JI_CustomsQuantity > Parent.JI_Weight)
				{
					Parent.JI_WeightInfo.AddMessageError("The net weight must be greater than or equal to the customs quantity when they are of the same unit.");
				}
			}

			if (Parent.JI_Weight == 0)
			{
				Parent.JI_WeightInfo.AddMessageError("Weight is a required field on an invoice line.");
			}
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			ValidateJI_Weight();
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_LinePriceInfo);
		}
	}
}
