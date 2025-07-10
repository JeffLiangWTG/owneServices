using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusFiscalReference : EU.Business.Declaration.CusFiscalReference
	{
		public CusFiscalReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		public bool IsImportInvoiceLine() => InvoiceLine?.Declaration is JobDeclaration declaration && declaration.IsImport;

		public bool IsImportEntryInstruction() => Instruction?.JobDeclaration is JobDeclaration declaration && declaration.IsImport;

		public override void OnLoaded()
		{
			base.OnLoaded();
			SaveForNoAmendCheck();
		}

		public new JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

		public new CusEntryInstruction Instruction => Parent as CusEntryInstruction;

		internal ZString OriginalCFR_Reference { get; private set; }

		void SaveForNoAmendCheck()
		{
			if (IsImportEntryInstruction() && Instruction.IsAmendmentValidationMode)
			{
				OriginalCFR_Reference = CFR_Reference;
			}
		}
	}
}
