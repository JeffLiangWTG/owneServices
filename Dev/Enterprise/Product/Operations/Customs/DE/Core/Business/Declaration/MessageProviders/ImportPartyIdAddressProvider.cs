using System.Text;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ImportPartyIdAddressProvider : IImportPartyIdAddress
	{
		public static IImportPartyIdAddress NewOrNull(OrgAddress orgAddress) => NewOrNull(orgAddress, true);

		public static IImportPartyIdAddress NewOrNull(OrgAddress orgAddress, bool truncate)
		{
			if (orgAddress == null)
			{
				return null;
			}

			IImportPartyIdAddress provider = new ImportPartyIdAddressProvider(orgAddress);

			if (truncate)
			{
				provider = TruncatedImportPartyIdAddressProvider.NewOrNull(provider);
			}

			return provider;
		}

		public static IImportPartyIdAddress NewOrNull(JobDocAddress jobDocAddress)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? NewOrNull(jobDocAddress.Address) : null;

		public static IImportPartyIdAddress NewOrNull(JobDocAddress jobDocAddress, bool truncate)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? NewOrNull(jobDocAddress.Address, truncate) : null;

		ImportPartyIdAddressProvider(OrgAddress orgAddress)
		{
			this.orgAddress = orgAddress;
		}
		readonly OrgAddress orgAddress;

		public string Name
		{
			get
			{
				var stringBuilder = new StringBuilder();
				var fullName = orgAddress.Header?.OH_FullName ?? string.Empty;
				var additionalAddressInformation = orgAddress.OA_AdditionalAddressInformation;

				if (!fullName.IsEmpty)
				{
					stringBuilder.Append(fullName);
				}
				if (!additionalAddressInformation.IsEmpty)
				{
					if (!fullName.IsEmpty)
					{
						stringBuilder.Append(" ");
					}

					stringBuilder.Append(additionalAddressInformation);
				}
				return stringBuilder.ToString().ValueOrNullIfEmpty();
			}
		}

		public string District => orgAddress.OA_Address2;

		public string Address => orgAddress.OA_Address1;

		public string City => orgAddress.OA_City;

		public string Postcode => orgAddress.OA_PostCode;

		public string Country => orgAddress.OA_RN_NKCountryCode;
	}
}
