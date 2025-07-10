using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ImportJobComInvoiceLineValidation(JobComInvoiceLine line)
			: base(line)
		{
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			if (!Parent.JI_CountryOfOrigin.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo);
			}
		}

		protected override void CheckJI_IsPackToBondForLine()
		{
			base.CheckJI_IsPackToBondForLine();
			Parent.AddInfo.Validation.ValidateZA_OA_WarehouseAddress_Hidden();
			Parent.AddInfo.Validation.ValidateZA_WRQ();

			JobDeclaration declaration = Parent.Declaration;
			if (declaration != null && declaration.IsImportCMR)
			{
				foreach (Package pack in declaration.Packages)
				{
					pack.Validation.ValidateCW_InBondPackQty();
				}
			}
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			ListValidation.WarnIfInvalidCode(Parent.JI_InvoiceUQInfo, CombinedUQList);
		}

		protected override void CheckJI_ConcessionOrder()
		{
			base.CheckJI_ConcessionOrder();
			if (Parent.JI_ConcessionOrder == "|")
			{
				Parent.JI_ConcessionOrderInfo.AddMessageError("Invalid Concession Order - may cause line merging problems");
			}
		}

		protected virtual int AllowedDescriptionLength
		{
			get { return 70; }
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			if (Parent.JI_Description.Length > AllowedDescriptionLength)
			{
				Parent.JI_DescriptionInfo.AddWarning("The description should be restricted to " + AllowedDescriptionLength + " characters.");
			}
		}

		public override CodeDescriptionPairList CustomsUQList
		{
			get { return Parent.Factory.GetCachedValue<AUCustomsImportUQList>(); }
		}

		#region InstrumentCode/Number

		protected override void CheckInstrumentCode()
		{
			base.CheckInstrumentCode();
			if (!Parent.Declaration.IsImportCMR)
			{
				if (Parent.InstrumentType == CustomsInstrumentTypeList.Codes.ByLaw
					|| Parent.InstrumentType == CustomsInstrumentTypeList.Codes.TariffConcession)
				{
					MandatoryValidation.CheckEntered(Parent.InstrumentCodeInfo);
					ListValidation.MessageErrorIfInvalidCode(Parent.InstrumentCodeInfo, InstrumentCodeList);
				}
			}
		}

		#endregion

	}
}
