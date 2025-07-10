using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGVATDeferStrategy : VATDeferStrategy
	{
		public DeltaGVATDeferStrategy(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override OrgHeader GetSourceForDeferment(ZString deferType)
		{
			var defermentSource = Declaration.IsImport ? Declaration.Importer : Declaration.IsExport ? Declaration.Supplier : null;

			var defermentAccountNumber = defermentSource?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, Core.Constants.CountryCodes.France) ?? ZString.Empty;

			return defermentAccountNumber == ZString.Empty ? Declaration.Declarant?.Header : defermentSource;
		}
	}
}
