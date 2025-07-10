using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalFiscalReferenceProvider : IIdentifierTypePair
	{
		AdditionalFiscalReferenceProvider(AdditionalFiscalReference fiscalReference)
		{
			Identifier = fiscalReference.CFR_Reference;
			Type = fiscalReference.CFR_Code;
		}

		public static AdditionalFiscalReferenceProvider NewOrNull(AdditionalFiscalReference fiscalReference) => fiscalReference?.CFR_Reference is { IsEmpty: false } ? new AdditionalFiscalReferenceProvider(fiscalReference) : null;

		public string Identifier { get; }

		public string Type { get; }
	}
}
