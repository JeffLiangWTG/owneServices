using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class AdditionalFiscalReferenceWrapper : IAdditionalFiscalReference
	{
		AdditionalFiscalReferenceWrapper(CusReference cusReference)
		{
			this.cusReference = Argument.NotNull(cusReference, nameof(cusReference));
		}

		readonly CusReference cusReference;

		public static AdditionalFiscalReferenceWrapper New(CusReference cusReference) => cusReference == null ? null : new AdditionalFiscalReferenceWrapper(cusReference);

		public string Role => role ?? (role = cusReference.CFR_Code);
		string role;

		public string VATIdentificationNumber => vATIdentificationNumber ?? (vATIdentificationNumber = cusReference.CFR_Reference);
		string vATIdentificationNumber;
	}
}
