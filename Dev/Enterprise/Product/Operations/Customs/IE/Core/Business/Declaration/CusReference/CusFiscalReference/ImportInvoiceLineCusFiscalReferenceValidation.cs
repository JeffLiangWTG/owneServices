using System.Linq;
using CargoWise.Common;
using FiscalReferenceCodeList = Enterprise.Customs.EU.Business.FiscalReferenceCodeList;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportInvoiceLineCusFiscalReferenceValidation : EU.Business.Declaration.CusFiscalReferenceValidation
	{
		public ImportInvoiceLineCusFiscalReferenceValidation(EU.Business.Declaration.CusFiscalReference parent, JobComInvoiceLine invoiceLine)
			: base(parent)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		protected override void CheckCFR_Code()
		{
			base.CheckCFR_Code();
			if (Parent.CFR_Code.EqualsIgnoringCase(FiscalReferenceCodeList.Codes.FR5_Vendor))
			{
				ValidateRuleBR600010();
				ValidateRuleBR600011();
			}
		}

		void ValidateRuleBR600010()
		{
			var parent = Parent;
			var parentPK = parent.PK;
			var hasMoreThanOneFR5FiscalReference = invoiceLine.FiscalReferences.Any(x => x.PK != parentPK && x.CFR_Code.EqualsIgnoringCase(FiscalReferenceCodeList.Codes.FR5_Vendor));
			if (hasMoreThanOneFR5FiscalReference)
			{
				Parent.CFR_CodeInfo.AddMessageError(
					Res.GetString(
						"BB84ED1A-72D4-410A-8BDD-4954F9437C4F",
						"[BR600010] An invoice line can have none or just one IOSS (FR5) Fiscal Reference declared."));
			}
		}

		void ValidateRuleBR600011()
		{
			var parent = Parent;
			if (invoiceLine.EntryInstruction is CusEntryInstruction instruction && !instruction.DoAllInvoiceLinesHaveFR5FiscalReference)
			{
				parent.CFR_CodeInfo.AddMessageError(
						Res.GetString(
							"D5722F0E-C234-4658-B278-03D4823194D4",
							"[BR600011] Either all invoice lines have just one IOSS (FR5) Fiscal Reference declared or none."));
			}
		}
	}
}
