using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public static class AddressValidationHelper
	{
		static string[] invalidStatuses => new[] { AddressValidationStatus.Invalid, AddressValidationStatus.ToBeVerified };

		static CodeDescriptionPairList addressValidationStatusList
		{
			get
			{
				var codeDescriptionPairList = new CodeDescriptionPairList();
				codeDescriptionPairList.AddPair(AddressValidationStatus.CountryNotAvailable, Res.GetString("F6B5B2B8-1A11-4AAE-8F56-0F17AA24B012", "is not yet possible to verify addresses for this country/region"));
				codeDescriptionPairList.AddPair(AddressValidationStatus.Invalid, Res.GetString("74A3E00C-2BFB-47DC-A865-A3F5684EF976", "has a verification status of Invalid"));
				codeDescriptionPairList.AddPair(AddressValidationStatus.ManuallyVerified, Res.GetString("1F6CCC1B-497B-4B00-99AA-C49EAC38EE28", "is manually verified"));
				codeDescriptionPairList.AddPair(AddressValidationStatus.ToBeVerified, Res.GetString("EA129561-7491-4EC2-B07A-B1D3CA4658C6", "has a verification status of Invalid"));
				codeDescriptionPairList.AddPair(AddressValidationStatus.Unverifiable, Res.GetString("848D07AD-308A-4C4E-A1F9-D4EB52330F0E", "has a verification status of Invalid"));
				codeDescriptionPairList.AddPair(AddressValidationStatus.Verified, Res.GetString("BEBB82E2-3390-4937-9E73-36886F37A8E2", "is verified"));
				codeDescriptionPairList.AddPair(AddressValidationStatus.VerifiedToStreet, Res.GetString("23263EE1-E6BC-4DFD-9947-C24C8E3EB4E3", "verified to street number"));
				codeDescriptionPairList.AddPair(AddressValidationStatus.ExcludeBackgroundValidation, Res.GetString("C2C9ACAB-C7F0-43D6-BA59-E5622D4730F5", "needs to be verified"));
				codeDescriptionPairList.AddPair(AddressValidationStatus.NotRequired, Res.GetString("92C4262F-3C90-400F-9313-12FE83CC53A7", "not required"));
				return codeDescriptionPairList;
			}
		}

		public static void CheckAddressStatusAndStreetNumber(ZPropertyInfo targetInfo, OrgAddress orgAddress)
		{
			if (orgAddress != null)
			{
				var status = orgAddress.ValidationStatus;
				ZString streetNumber = orgAddress.StreetNumber;
				var isInvalidStatus = invalidStatuses.Contains(status.ToString());

				if (isInvalidStatus)
				{
					targetInfo.AddWarning(Res.GetString("B33BDBDC-D5F9-44A5-9FF8-E22F3A4958FD", "Address \"{0}\" {1}.", orgAddress.Address1, addressValidationStatusList.GetDescriptionFromCode(status)));
				}

				if (streetNumber.IsEmpty)
				{
					var message = Res.GetString("7b360a31-afde-433f-9101-6f9228b75101", "Address \"{0}\" has no Address Number. Address number will not be sent.", orgAddress.Address1);

					if (isInvalidStatus)
					{
						targetInfo.AddMessageError(message);
					}
					else
					{
						targetInfo.AddWarning(message);
					}
				}
			}
		}
	}
}
