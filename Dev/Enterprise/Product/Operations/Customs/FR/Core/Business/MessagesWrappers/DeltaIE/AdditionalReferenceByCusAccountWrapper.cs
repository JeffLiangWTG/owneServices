using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class AdditionalReferenceByCusAccountWrapper : IAdditionalReference
	{
		AdditionalReferenceByCusAccountWrapper(OrgCusAccount cusAccountReference, string customsOffice)
		{
			this.cusAccountReference = Argument.NotNull(cusAccountReference, nameof(cusAccountReference));
			this.customsOffice = customsOffice;
		}

		readonly OrgCusAccount cusAccountReference;
		readonly string customsOffice;

		public static AdditionalReferenceByCusAccountWrapper New(OrgCusAccount cusAccountReference, string customsOffice) => cusAccountReference == null ? null : new AdditionalReferenceByCusAccountWrapper(cusAccountReference, customsOffice);

		public string CcQualifier => ccQualifier ?? (ccQualifier = customsOffice.StartsWith(Core.Constants.CountryCodes.France) ? string.Empty : Core.Constants.CountryCodes.France);
		string ccQualifier;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = cusAccountReference.CZ_Account);
		string referenceNumber;

		public string Type => type ?? (type = accountType);
		string type;

		const string accountType = "1DEC"; // Should always be 1DEC for type of additional reference tag from CusAccount
	}
}
