namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BondedWarehousingHelper : Customs.Business.BondedWarehousingHelper
	{
		public BondedWarehousingHelper(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool HasBondedWarehouseEntryDetailsCore(Customs.Business.BaseJobComInvoiceLine invoiceLine, bool isInward, bool isOutward, bool isChangeOfOwnership)
		{
			var auInvoiceLine = (JobComInvoiceLine)invoiceLine;
			return (declaration.IsExWarehouse || declaration.IsWarehousedByExternalAgent) ? HasBondedWarehouseEntryDetailsForOutward(auInvoiceLine) : HasBondedWarehouseEntryDetailsForInward(auInvoiceLine);
		}

		bool HasBondedWarehouseEntryDetailsForOutward(JobComInvoiceLine invoiceLine)
		{
			return !invoiceLine.AddInfo.ZA_WRN.IsEmpty && !invoiceLine.AddInfo.ZA_WRL.IsEmpty;
		}

		bool HasBondedWarehouseEntryDetailsForInward(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.CusEntryLine?.Header != null;
		}

		protected override bool IsMarkedForBondedWarehousingCore(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			return declaration.IsExWarehouse ? invoiceLine.UseBondedWarehouseAutomation : invoiceLine.IsGoingIntoBondedWarehouse;
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}
	}
}
