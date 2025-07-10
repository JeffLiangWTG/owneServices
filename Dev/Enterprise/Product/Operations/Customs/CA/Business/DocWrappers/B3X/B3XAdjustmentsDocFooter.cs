using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class B3XAdjustmentsDocFooter : AdjustmentsDocFooter
	{
		public B3XAdjustmentsDocFooter() : base()
		{
		}

		public B3XAdjustmentsDocFooter(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void SetTotalValues(JobDeclaration declaration, IEnumerable<AdjustmentsDocPage> pages)
		{
			base.SetTotalValues(declaration, pages);

			Deposit = declaration.CA_AnySightDepositAmount;
			WarehouseNumber = declaration.IsWarehouseEntry ? IB3HeaderHelper.GetCustomsRegNo(declaration.WarehouseDocAddress.Organisation, OrgCusCode.CodeTypes.WarehouseControlledPremisesID) : ZString.Empty;
			CargoControlNumber = declaration.CargoControlNumbers.Count > 1 ? new ZString("B3B") : declaration.CargoControlNumbers.Count == 0 ? ZString.Empty : declaration.CargoControlNumbers.ToArray()[0].CY_CargoControlNumber;
			CarrierCodeAtImportation = declaration.JE_CarrierCode;
			TotalAllDutyAndTaxes = TotalDutyAndSIMAAndTaxAndGST + Deposit;
		}
	}
}
