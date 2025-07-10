using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class LocalGlbBranchCollectionProvider : GlbBranchCollectionProvider
	{
		public LocalGlbBranchCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		public override void AddValidationAndDefault(FilterField parentFilterField, ValidatorPack validatorPack)
		{
			base.AddValidationAndDefault(parentFilterField, validatorPack);
			parentFilterField.Validators.Add(new LocalGlbBranchCollectionValidator());
		}
	}

	class LocalGlbBranchCollectionValidator : FilterCollectionValidator
	{
		public LocalGlbBranchCollectionValidator()
		{
		}

		public override bool IsValid(FilterField filterToValidate)
		{
			nonCurrentCompanyBrancheCodes = System.Array.Empty<ZString>();
			var nonCurrentCompanyBranches = ((filterToValidate as MultipleSelectionLookup).BindToList as GlbBranchCollection).Cast<GlbBranch>().Where(x => x.GB_GC != GlbCompany.CurrentCompany.PK);
			if (nonCurrentCompanyBranches.Any())
			{
				nonCurrentCompanyBrancheCodes = nonCurrentCompanyBranches.Select(x => x.GB_Code).ToArray();
				return false;
			}

			return true;
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			return Res.GetString("0CBADB56-3BA0-43BE-81CC-57B23EF10817", "Only Local Branch can be chosen here. Please remove {0}.", string.Join(", ", nonCurrentCompanyBrancheCodes));
		}

		ZString[] nonCurrentCompanyBrancheCodes;
	}
}
