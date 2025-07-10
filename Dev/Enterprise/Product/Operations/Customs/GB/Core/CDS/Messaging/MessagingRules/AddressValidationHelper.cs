using CargoWise.EntityFramework;
namespace Enterprise.Customs.GB.CDS.MessagingRules
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.EU.Business.Declaration;
	using Enterprise.MasterFiles.Business;
	using EU.Business;

	public static class AddressValidationHelper
	{
		public static void CheckConsigneePostcode(JobDeclaration dec)
		{
			if (dec.ImporterDocumentaryAddress == null || dec.ImporterDocumentaryAddress.Address == null)
			{
				return;
			}

			JobDocAddress docAddress = dec.ImporterDocumentaryAddress;
			ZString postCode = docAddress.E2_Postcode;
			RefCountry importerCountry = docAddress.Country;
			ZPropertyInfo consigneeInfo = dec.ImporterDocumentaryAddress.E2_OA_AddressInfo;

			if (dec.IsExport)
			{   // Exports - postcode is MANDATORY 
				if (postCode.IsEmpty && importerCountry != null)
				{
					consigneeInfo.AddMessageError(ErrorMessageConsigneePostcodeMandatory);
				}
			}
			else if (dec.IsImport)
			{
				ZString eori = docAddress.E2_AddressOverride ? docAddress.E2_GovRegNum : docAddress.Organisation.GetEuIdentificationNumber();
				if (!eori.IsEmpty && eori.StartsWith(Enterprise.Core.Constants.CountryCodes.UnitedKingdom))
				{
					// OK - Chief knows about UK Turns as they are all on file.  Postcode optional.
				}
				else
				{   // Foreign turn.... postcode mandatory, so add an error if it's missing
					if (postCode.IsEmpty)
					{
						consigneeInfo.AddMessageError(ErrorMessageConsigneePostcodeMandatory);
					}
				}
			}
		}

		public static void CheckAddressAndNameEmpty(JobDocAddress docAddress, ZPropertyInfo info)
		{
			if (docAddress == null || docAddress.Address == null)
			{
				return;
			}

			List<string> whatIsMissing = new List<string>();
			ZPropertyInfo[] supplierElementsThatNeedToBeCompleted = new ZPropertyInfo[] { docAddress.E2_CityInfo, docAddress.E2_RN_NKCountryCodeInfo, docAddress.E2_CompanyNameInfo, docAddress.E2_Address1Info };

			// Ensure all required address fields are present.  Note: out own validation will catch missing city, country, etc, but in the case of legacy data these might be missing. So add error. 
			foreach (ZPropertyInfo oneElementOfAddress in supplierElementsThatNeedToBeCompleted)
			{
				if (oneElementOfAddress.Value.IsEmpty)
				{
					whatIsMissing.Add(oneElementOfAddress.HumanReadableName);
				}
			}

			// Note - we do not validate the postcode because we'll flip-in "N/A" if needed.  See GbExportHeader & GbImportHeader

			if (whatIsMissing.Count > 0)
			{
				string list = string.Join(", ", whatIsMissing.ToArray());
				string err = ErrorMessageAddressAndNameEmpty + list;
				if (!info.HasMessageError(err))
				{
					info.AddMessageError(err);
				}
			}

			List<string> whatIsRecommended = new List<string>();
			ZPropertyInfo[] supplierElementsThatRecommendToBeCompleted = new ZPropertyInfo[] { docAddress.E2_PostcodeInfo };

			// Check all recommended address fields (postcode) are present. If not, add warning. 
			foreach (ZPropertyInfo oneElementOfAddress in supplierElementsThatRecommendToBeCompleted)
			{
				if (oneElementOfAddress.Value.IsEmpty)
				{
					whatIsRecommended.Add(oneElementOfAddress.HumanReadableName);
				}
			}

			if (whatIsRecommended.Count > 0)
			{
				string list = string.Join(", ", whatIsRecommended.ToArray());
				string err = ErrorMessageAddressAndNameEmpty + list;
				if (!info.HasWarning(err))
				{
					info.AddWarning(err);
				}
			}
		}

		public const string ErrorMessageAddressAndNameEmpty = "All Name and Address details must be completed. (N.B. foreign parties must have a postcode for CDS messaging; without one 'NA' will be sent instead). Missing properties: ";

		public const string ErrorMessageConsigneePostcodeMandatory = "Consignee postcode is mandatory";
	}
}
