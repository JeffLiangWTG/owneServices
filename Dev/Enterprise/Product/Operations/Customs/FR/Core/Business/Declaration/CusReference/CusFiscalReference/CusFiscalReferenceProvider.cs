using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusFiscalReferenceProvider : EU.Business.Declaration.CusFiscalReferenceProvider
	{
		protected CusFiscalReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override EU.Business.Declaration.CusFiscalReferenceValidation GetNewValidationCore(EU.Business.Declaration.CusFiscalReference reference)
		{
			var isUCC6 = reference.Declaration?.IsUCC6 ?? false;
			return isUCC6 ? new CusFiscalReferenceValidation(reference) : base.GetNewValidationCore(reference);
		}

		protected override EU.Business.Declaration.CusFiscalReferenceLookups GetNewLookupsCore(EU.Business.Declaration.CusFiscalReference reference)
		{
			var isUCC6 = reference.Declaration?.IsUCC6 ?? false;
			return isUCC6 ? new DeltaIECusFiscalReferenceLookups(reference) : base.GetNewLookupsCore(reference);
		}

		protected override void RecalculateReferenceIfNeededCore(EU.Business.Declaration.CusFiscalReference reference)
		{
			base.RecalculateReferenceIfNeededCore(reference);

			if (reference.CFR_Reference.IsEmpty)
			{
				if (reference.CFR_Code == DeltaIEFiscalReferenceCodeList.Codes.FR7)
				{
					if (reference.Declaration is JobDeclaration declaration)
					{
						var vatDeferNumber = declaration.VATNumberSupporter.GetVATDeferNumberFromImporter();
						reference.CFR_Reference = vatDeferNumber;
					}
				}
			}
		}
	}
}
