using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	class BondedWarehousingHelper : Customs.Business.BondedWarehousingHelper
	{
		public BondedWarehousingHelper(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool HasBondedWarehouseEntryDetailsCore(BaseJobComInvoiceLine invoiceLine, bool isInward, bool isOutward, bool isChangeOfOwnership)
		{
			return (isInward && HasBondedWarehouseEntryDetailsForInward(invoiceLine))
				|| (isOutward && HasBondedWarehouseEntryDetailsForOutward(invoiceLine));
		}

		bool HasBondedWarehouseEntryDetailsForInward(BaseJobComInvoiceLine invoiceLine)
		{
			var entryLine = invoiceLine.CusEntryLine;
			var entry = entryLine?.Header;
			return entry != null && !entry.EntryNumber.IsEmpty && !entryLine.CL_LineNumber.IsEmpty;
		}

		bool HasBondedWarehouseEntryDetailsForOutward(BaseJobComInvoiceLine invoiceLine)
		{
			return !invoiceLine.JI_PreviousEntryNumber.IsEmpty && !invoiceLine.JI_PreviousEntryLineNumber.IsEmpty;
		}

		protected override bool IsMarkedForBondedWarehousingCore(BaseJobComInvoiceLine invoiceLine)
		{
			return invoiceLine.UseBondedWarehouseAutomation;
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}
	}
}
