using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusFiscalReferenceProvider : EU.Business.Declaration.CusFiscalReferenceProvider
	{
		protected CusFiscalReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override CusFiscalReferenceValidation GetNewValidationCore(EU.Business.Declaration.CusFiscalReference fiscalReference)
		{
			CusFiscalReferenceValidation result = null;
			if (fiscalReference is CusFiscalReference ieFiscalReference)
			{
				if (ieFiscalReference.IsImportInvoiceLine())
				{
					result = new ImportInvoiceLineCusFiscalReferenceValidation(ieFiscalReference, ieFiscalReference.InvoiceLine);
				}
				else if (ieFiscalReference.IsImportEntryInstruction())
				{
					result = new ImportEntryInstructionCusFiscalReferenceValidation(ieFiscalReference);
				}
			}
			return result ?? base.GetNewValidationCore(fiscalReference);
		}
	}
}
