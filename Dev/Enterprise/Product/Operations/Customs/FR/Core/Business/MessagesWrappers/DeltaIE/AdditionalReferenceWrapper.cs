using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class AdditionalReferenceWrapper : IAdditionalReference
	{
		AdditionalReferenceWrapper(CusSupportingInfo additionalReference, string customsOffice)
		{
			this.additionalReference = Argument.NotNull(additionalReference, nameof(additionalReference));
			this.customsOffice = customsOffice;
		}

		readonly CusSupportingInfo additionalReference;
		readonly string customsOffice;

		public static AdditionalReferenceWrapper New(CusSupportingInfo additionalReference, string customsOffice) => additionalReference == null ? null : new AdditionalReferenceWrapper(additionalReference, customsOffice);

		public string CcQualifier => ccQualifier ?? (ccQualifier = customsOffice.StartsWith(Core.Constants.CountryCodes.France) ? string.Empty : Core.Constants.CountryCodes.France);
		string ccQualifier;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = additionalReference.CSI_ReferenceNumber);
		string referenceNumber;

		public string Type => type ?? (type = additionalReference.CSI_Code);
		string type;
	}
}
