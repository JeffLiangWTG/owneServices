using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusFiscalReferenceValidation : EU.Business.Declaration.CusFiscalReferenceValidation
	{
		public CusFiscalReferenceValidation(CusFiscalReference parent) : base(parent)
		{
		}

		void CheckRuleNat_030(ZPropertyInfo propertyInfo)
		{
			var cusFiscalReference = Parent;
			var fiscalReferenceParent = cusFiscalReference.Parent;

			if (cusFiscalReference.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Vendor)
			{
				var message1 = Res.GetString("348C64C4-B576-4C18-BFBF-A3A78BB9BE47", "[NAT_030] Fiscal reference FR5 should only be used for concession F48.");
				var message2 = Res.GetString("945F41AD-7D2A-4742-BFB8-F982E7C02BFC", "[NAT_030] There should be only one fiscal reference FR5 used for concession F48.");

				if (fiscalReferenceParent is JobComInvoiceLine invoiceLine && invoiceLine.Validation.ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleNAT_030Active: true })
				{
					var invoiceLineHasF48 = invoiceLine.JI_Calc_Concession == UniversalReferenceConstants.RefCusProcedure.Concession.F48;
					if (invoiceLineHasF48)
					{
						var fr5References = invoiceLine.FiscalReferences.Union(invoiceLine.EntryInstruction?.FiscalReferences.ToArray() ?? System.Array.Empty<CusFiscalReference>()).Cast<CusFiscalReference>().ToArray();
						if (fr5References.Count(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Vendor) > 1)
						{
							propertyInfo.AddMessageError(message2);
						}
					}
					else
					{
						propertyInfo.AddMessageError(message1);
					}
				}
				else if (fiscalReferenceParent is CusEntryInstruction instruction && instruction.Validation.ValidationDecider is IEntryInstructionValidationDecider { IsRuleNAT_030Active: true })
				{
					var allRelatedInvoiceLinesHaveF48 = instruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Calc_Concession == UniversalReferenceConstants.RefCusProcedure.Concession.F48);
					if (allRelatedInvoiceLinesHaveF48)
					{
						var fr5References = instruction.FiscalReferences.Union(instruction.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.FiscalReferences)).Cast<CusFiscalReference>().ToArray();
						if (fr5References.Count(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Vendor) > 1)
						{
							propertyInfo.AddMessageError(message2);
						}
					}
					else
					{
						propertyInfo.AddMessageError(message1);
					}
				}
			}
		}

		void CheckRuleNat_031(ZPropertyInfo propertyInfo)
		{
			var cusFiscalReference = Parent;
			var fiscalReferenceParent = cusFiscalReference.Parent;

			if (cusFiscalReference.CFR_Code != FiscalReferenceCodeList.Codes.FR5_Vendor)
			{
				if ((fiscalReferenceParent is CusEntryInstruction instruction && instruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Calc_Concession == UniversalReferenceConstants.RefCusProcedure.Concession.F48))
					|| (fiscalReferenceParent is JobComInvoiceLine invoiceLine && invoiceLine.JI_Calc_Concession == UniversalReferenceConstants.RefCusProcedure.Concession.F48))
				{
					propertyInfo.AddMessageError(Res.GetString("98FCCD91-4F79-4A6F-9A11-597E51EA0C9E", "It is not possible to provide fiscal references other than FR5 for concession F48."));
				}
			}
		}

		protected override void CheckCFR_Code()
		{
			base.CheckCFR_Code();
			CheckRuleNat_030(Parent.CFR_CodeInfo);
			CheckRuleNat_031(Parent.CFR_CodeInfo);
		}
	}
}
