using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class SupplementaryCodeValidation : BaseSupplementaryCodeValidation
	{
		public SupplementaryCodeValidation(BaseSupplementaryCode parent)
			: base(parent)
		{
		}

		protected new SupplementaryCode Parent
		{
			get { return (SupplementaryCode)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
			var code = Parent.CY_Code;
			var targetInfo = Parent.CY_CodeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			if (!code.IsEmpty && code.Length != 4)
			{
				targetInfo.AddMessageError(SupplementaryCodeLength(targetInfo));
			}

			var supporter = Parent.SupplementaryCodeSupporter;
			if (
				supporter != null
				&& supporter.SupplementaryCodes
					.OfType<SupplementaryCode>()
					.Any(x => x.PK != Parent.PK && x.CY_Code == code)
			)
			{
				targetInfo.AddMessageError(DuplicateSupplementaryCode(targetInfo));
			}

			var invoiceLine = Parent.CY_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? Parent.Parent as JobComInvoiceLine : null;
			if (invoiceLine?.UniversalTariff != null)
			{
				invoiceLine.Validation.ValidateJI_Tariff();
			}
		}

		public static string SupplementaryCodeLength(ZPropertyInfo info) => Res.GetString("1161B7C6-857A-46CF-AFAF-3796AD245E28", "{0} must be 4 characters long if supplied", info.HumanReadableName);

		public static string DuplicateSupplementaryCode(ZPropertyInfo info) => Res.GetString("751D03F5-85F7-4D77-9F0C-E66E548F1337", "{0} already specified. {1} may not be duplicated", info.HumanReadableName, Grammar.Instance.Pluralize(info.HumanReadableName));
	}
}
