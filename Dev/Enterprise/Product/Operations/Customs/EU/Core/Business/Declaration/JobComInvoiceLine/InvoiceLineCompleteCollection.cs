using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceLineCompleteCollection : TypeSafeInvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var invLine = (JobComInvoiceLine)child;
			invLine.ZG_StatisticalValueManualOverride = JobDeclaration.IsImport;

			if (JobDeclaration.IsImport)
			{
				invLine.ZG_CountryOfDestination = JobDeclaration?.CountryCode ?? ZString.Empty;
				SetDefaultCountryOfSupply(invLine);
			}

			if (invLine.EntryInstruction is CusEntryInstruction instruction && instruction.IsRequestedProcedureValid)
			{
				invLine.SetFirst2CharactersOfJI_Procedure(instruction.CEI_Procedure);
			}
		}

		void SetDefaultCountryOfSupply(JobComInvoiceLine line)
		{
			var invoiceHeader = line.InvoiceHeader;
			if (invoiceHeader != null)
			{
				var defaultCountryOfSupply = invoiceHeader.DefaultCountryOfSupply;
				if (!defaultCountryOfSupply.IsEmpty)
				{
					line.ZG_CountryOfSupply = defaultCountryOfSupply;
				}
			}
		}
	}
}
