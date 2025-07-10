using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class ComprehensiveValuationValidation : JPCusReferenceValidation
	{
		public ComprehensiveValuationValidation(ComprehensiveValuation parent) : base(parent)
		{
		}

		protected override void CheckCFR_Reference()
		{
			base.CheckCFR_Reference();

			var reference = Parent.CFR_Reference;
			var propertyInfo = Parent.CFR_ReferenceInfo;
			if (!string.IsNullOrWhiteSpace(reference))
			{
				if (!new Regex(@"^[A-Z0-9]*$").IsMatch(reference))
				{
					propertyInfo.AddError(Res.GetString("A94BD235-E1EF-4595-9A77-3EA59267066B", "Only capital letters and digits are allowed."));
				}

				var invoiceHeader = Parent.Parent;
				var declaration = invoiceHeader?.JobDeclaration;
				if (declaration != null && declaration.IsImport)
				{
					var declarationTypes = declaration.CustomsEntryInstructions.Select(e => e.CEI_Style);
					if (declarationTypes.Contains(JPImportDeclarationTypeList.Codes.Y) ||
						declarationTypes.Contains(JPImportDeclarationTypeList.Codes.H) ||
						declarationTypes.Contains(JPImportDeclarationTypeList.Codes.N))
					{
						propertyInfo.AddMessageError(Res.GetString("F49193AF-C5D4-4B17-A3DB-19B4A6482178", "Comprehensive Valuation Number cannot be entered when Shipment Type is IMP and Declaration Type is Y, H, or N."));
					}
				}

				if (invoiceHeader != null && invoiceHeader.ComprehensiveValuations.Cast<ComprehensiveValuation>().Any(valuation => valuation != Parent && valuation.CFR_Reference == reference))
				{
					propertyInfo.AddMessageError(Res.GetString("2B673BD2-AE5B-444F-8797-9B83F4ABE39C", "The same information has been entered."));
				}
			}
		}

		protected new ComprehensiveValuation Parent => (ComprehensiveValuation)base.Parent;
	}
}
