using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class DuimpTaxRegimeCollection : Customs.Business.CusSupportingInfoCollection<DuimpTaxRegime>
	{
		public DuimpTaxRegimeCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.DuimpTaxRegime)
		{
		}
		public JobComInvoiceLine InvoiceLine => Master as JobComInvoiceLine;

		protected override bool AllowNewCore => false;

		public DuimpTaxRegime AddNew(TariffProfile profile)
		{
			var taxRegime = FindByProfile(profile) ?? AddNew();
			taxRegime.AddProfile(profile);
			return taxRegime;
		}

		public void Rebuild()
		{
			RemoveAndDeleteAll();

			if (InvoiceLine != null && InvoiceLine.IsImportOnly)
			{
				InvoiceLine.GetRequiredTTProfiles(isMandatory: true).ForEach(FindAndSetProfile);
			}
		}

		public override void Load()
		{
			base.Load();

			this.Cast<DuimpTaxRegime>().ForEach(x => x.Profiles.Clear());

			if (InvoiceLine?.IsImportOnly ?? false)
			{
				InvoiceLine.GetRequiredTTProfiles().ForEach(FindAndSetProfile);
			}

			RemoveNullProfile();
		}

		void FindAndSetProfile(TariffProfile profile)
		{
			var taxRegime = FindByProfile(profile);
			if (taxRegime == null && profile.IsMandatory)
			{
				taxRegime = AddNew();
			}
			if (taxRegime != null)
			{
				taxRegime.AddProfile(profile);
			}
		}

		void RemoveNullProfile()
		{
			foreach (var taxRegime in this.Cast<DuimpTaxRegime>().Where(x => x.Profiles.Count == 0).ToArray())
			{
				RemoveAndDelete(taxRegime);
			}
		}

		public DuimpTaxRegime GetFirstElementHaving(ZString regime, ZString legalCode, ZString taxType)
		{
			return Where(x => x.CSI_Code == regime && x.CSI_Procedure == legalCode && x.CSI_SubType == taxType).FirstOrDefault();
		}

		DuimpTaxRegime FindByProfile(TariffProfile profile) => GetFirstElementHaving(profile.Regime, profile.LegalCode, profile.TaxType);

		public DuimpTaxRegime[] FindByLegalCode(string legalCode) => Where(x => x.CSI_Procedure == legalCode).ToArray();
	}
}
