using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class PackageValidation : Customs.Business.CusDecHouseContainerPackValidation
	{
		public PackageValidation(Customs.Business.AutoCusDecHouseContainerPack parent)
			: base(parent)
		{
		}

		public new Package Package => (Package)base.Package;

		protected override void CheckCW_MarksAndNos()
		{
			if (!Package.IsEmptyPackTypeAllowed)
			{
				base.CheckCW_MarksAndNos();
			}

			CheckRuleC0820();
		}

		protected override void CheckCW_PackType()
		{
			var package = Package;
			if (!package.CW_PackType.IsEmpty && package.Declaration is JobDeclaration declaration)
			{
				var unitTypesList = declaration.Lookups.PackingUnitTypesList;
				if (unitTypesList.Count > 0)
				{
					ListValidation.MessageErrorIfInvalidCode(package.CW_PackTypeInfo, unitTypesList);
				}
			}
		}

		IPackageValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<IPackageValidationDecider> validationDeciderCached;

		IPackageValidationDecider GetValidationDecider()
		{
			var declaration = Package.Declaration as JobDeclaration;
			return declaration?.Configuration.GetPackageValidationDecider(declaration);
		}

		void CheckRuleC0820()
		{
			var package = Package;

			if (ValidationDecider is IPackageValidationDecider validationDecider
				&& validationDecider.IsRuleC0820ActiveForCW_MarksAndNos
				&& package.CW_MarksAndNos.IsEmpty)
			{
				var linkedInvoiceLines = package.InvoiceLinePivotCollection.Select(p => p.InvoiceLine);
				if (linkedInvoiceLines.Any(line => line.JI_Calc_Concession != Constants.Customs.Universal.RefCusProcedure.Concession.F15))
				{
					JobComInvoiceLineValidation.AddMessageError_RuleC0820(package.CW_MarksAndNosInfo);
				}
			}
		}
	}
}
