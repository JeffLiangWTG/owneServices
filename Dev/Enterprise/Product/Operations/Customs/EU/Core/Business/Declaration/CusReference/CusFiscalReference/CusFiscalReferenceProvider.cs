using System.Collections;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusFiscalReferenceProvider
	{
		public static CusFiscalReferenceProvider GetByDataGroupingCode(ZString dataGroupingCode)
		{
			CusFiscalReferenceProvider result = null;
			if (!dataGroupingCode.IsEmpty)
			{
				var types = ObjectFactory.Get<Hashtable>("CusFiscalReferenceProviders");
				var objectHandle = (ObjectHandle)types[dataGroupingCode.ToString()];
				result = (CusFiscalReferenceProvider)objectHandle?.GetObject(dataGroupingCode);
			}
			return result ?? new CusFiscalReferenceProvider(dataGroupingCode);
		}

		protected CusFiscalReferenceProvider(ZString dataGroupingCode)
		{
			DataGroupingCode = dataGroupingCode;
		}

		public ZString DataGroupingCode { get; }

		public CusFiscalReferenceLookups GetNewLookups(CusFiscalReference reference) => GetNewLookupsCore(reference);

		public CusFiscalReferenceValidation GetNewValidation(CusFiscalReference reference) => GetNewValidationCore(reference);

		protected virtual CusFiscalReferenceLookups GetNewLookupsCore(CusFiscalReference reference) => reference.Declaration?.IsUCC6AndIsImport ?? false ? new UCC6ImportCusFiscalReferenceLookups(reference) : new CusFiscalReferenceLookups(reference);

		protected virtual CusFiscalReferenceValidation GetNewValidationCore(CusFiscalReference reference) => new CusFiscalReferenceValidation(reference);

		public virtual void RecalculateOwnerIfNeeded(CusFiscalReference reference)
		{ }

		public void RecalculateReferenceIfNeeded(CusFiscalReference reference)
		{
			var factory = reference.Factory;
			if (!reference.CFR_OA_Owner.IsEmpty && DeclarationConfiguration.GetConfiguration(factory, DataGroupingCode).UseEoriForFiscalReference)
			{
				var organisation = reference.Owner?.Header;
				if (organisation != null)
				{
					reference.CFR_Reference = organisation.GetEuIdentificationNumber();
				}
			}
			RecalculateReferenceIfNeededCore(reference);
		}

		protected virtual void RecalculateReferenceIfNeededCore(CusFiscalReference reference)
		{ }

		public virtual bool ReferenceIsReadOnly(CusFiscalReference reference) => false;

		public virtual bool OwnerIsReadOnly(CusFiscalReference reference) => false;

		public ZInt GetReferenceMaxLength(CusFiscalReference reference) => GetReferenceMaxLengthCore(reference);

		protected virtual ZInt GetReferenceMaxLengthCore(CusFiscalReference reference) => CusFiscalReference.Schema.CFR_ReferenceMaxLength;
	}
}
