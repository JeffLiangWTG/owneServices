using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.Business
{
	public class OfficeCodeValidation : EuOfficeCodeValidation
	{
		public OfficeCodeValidation(EuOfficeCode parent) : base(parent)
		{
		}

		protected new OfficeCode Parent => (OfficeCode)base.Parent;

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			if (Declaration != null)
			{
				ValidateESCustomForUCC6ExportPRE();
			}
		}

		void ValidateESCustomForUCC6ExportPRE()
		{
			if (Declaration.IsUCC6AndIsExport && Parent.CY_Code.Equals(EuOfficeCodesTypes.Codes.OfficeOfPresentation) && Parent.CY_Data.StartsWith("ES"))
			{
				Parent.CY_DataInfo.AddWarning(Res.GetString("0BDB5441-1CB7-4B98-BAB6-B1F4F0F18690", "Customs Office of Presentation should only be used in CCE (Centralized Clearance in Europe) and cannot be a Spanish office (should not start with ES)"));
			}
		}

		protected override bool HasOfficeAnyRolOnAttribute(IEnumerable<ZString> roles, ZZRefCusCodeListCombined office) =>
			Declaration.IsUCC6AndIsExport && roles.Count() == 1 && roles.FirstOrDefault().Equals(EuOfficeCodesTypes.Codes.OfficeOfPresentation) ? base.HasOfficeAnyRolOnAttribute(new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExport }, office) : base.HasOfficeAnyRolOnAttribute(roles, office);

		JobDeclaration Declaration => Parent.Declaration;
	}
}
