using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public static class ValidationConstants
	{
		public static class Constants
		{
			#region MaxLength

			#region Port

			public const int PortNameMaxLength = 20;

			#endregion

			#region OrgAddress

			public const int Address2MaxLenth = 35;
			public const int PostCodeMaxLenth = 9;
			public const int PhoneMaxLength = 14;

			#endregion

			#region Container

			public const int ContainerNumMaxLength = 12;

			#endregion

			#endregion

			public const string ContainerOperatorBasketCode = "99999";

			public const string NonUniversalHSCode98 = "98";
			public const string NonUniversalHSCode99 = "99";

			#region InvalidCharacters

			public const string ValidNACCSCharacters = " !\"#%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz\r\n";
			public const string ValidNACCSCharactersForBillNumber = " !\"#%&'()*+-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

			#endregion
		}

		public static class Shared
		{
			public static string ChangingFieldWhenBillIsRegistered(string fieldName, string oldValue, string newValue)
			{
				return ResString.GetMultilingualString("JPAFRShared|1E4485AE-2500-4335-AA82-0A193A7A33FD", "Cannot change {2} from '{0}' to '{1}' when the previous value '{0}' is on Customs file.\r\nIf you want to correct the Vessel Information, please use 'Amendment Manifest' in AFR menu.", oldValue, newValue, fieldName);
			}

			public static string ChangingFieldWhenMessagingIsInProgress(string fieldName, string oldValue, string newValue)
			{
				return ResString.GetMultilingualString("JPAFRShared|CB62A550-DB80-4BA8-A160-3BE8FE3D8B94", "Cannot change {2} from '{0}' to '{1}' when the previous value '{0}' is being reported to Customs.\r\nYou need to wait for Customs response for corresponding Bill(s) before changing it.", oldValue, newValue, fieldName);
			}

			public static string PortNameLengthExceeded
			{
				get { return ResString.GetMultilingualString("JPAFRShared|3F5B3EF2-36F6-45F6-BD58-7CB761E12689", "The name of the port is exceeding the limit of 20 characters for JP AFR messaging, it will be truncated in the message delivered."); }
			}

			public static string BOLNumMissingCarrierCode
			{
				get { return ResString.GetMultilingualString("JPAFRShared|100289F5-F976-4AE6-8DB5-436CD1AAC654", "Bill Of Lading Number should be at least 6 characters long with the carrier code for NACCS in the first 4 characters. If the carrier code is 3-digit, please make sure the character in position 4 is '-' (hyphen sign)."); }
			}

			public static string InvalidNACCSCharInBOLNumber
			{
				get { return ResString.GetMultilingualString("JPAFRShared|8812D836-2391-4218-A265-364157F89DE7", "The Bill of Lading Number contains invalid NACCS characters.\r\nOnly alphanumeric characters and following punctuation marks are allow.\r\nAllowed marks are: '!', '\"', '#', '%', '&', ''', '(', ')', '*', '+', '-', '.', '/', ':', ';', '<', '=', '>', '?' and '@'"); }
			}

			public static string InvalidNACCSChar(string fieldName)
			{
				return ResString.GetMultilingualString("JPAFRShared|E3788218-560B-4DAF-AB14-090C311F9F61", "{0} contains invalid NACCS characters.\r\nOnly alphanumeric characters and following punctuation marks are allow.\r\nAllowed marks are: '!', '\"', '#', '%', '&', ''', '(', ')', '*', '+', '-', ',', '.', '/', ':', ';', '<', '=', '>', '?' and '@'\r\nPlease note that some invalid characters may be invisible.", fieldName);
			}

			public static string InvalidNACCSCharInBOLNumberMessageForSending(string billNumber)
			{
				return ResString.GetMultilingualString("JPAFRShared|B4854417-14BA-40E9-BD02-E5D30157FD9E",
					"Bill of Lading Number:'{0}' contains invalid NACCS characters.", billNumber);
			}

			public static string InvalidNACCSCharMessageForSending(string fieldName)
			{
				return ResString.GetMultilingualString("JPAFRShared|F0A609FF-0FD4-499A-A74F-55ACC43F3208",
					"{0} contains invalid NACCS characters.", fieldName);
			}

			public static string FieldLengthReachedMaxAllowedWillBeTruncated(ZString fieldName, int maxLength)
			{
				return ResString.GetMultilingualString("JPAFRShared|5B1BE41B-BCF4-41A9-8A4B-418E954B57E1", "{0} allow maximum {1} characters in JP AFR messages, the exceeding parts will be truncated in the message delivered", fieldName, maxLength);
			}
		}

		public static class Header
		{
			public static string MBOLDuplicated
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|DEFD4EDE-ABEE-4D8B-A3BD-1E79AB8C1885", "This Master Bill Number exists on another JP AFR job"); }
			}

			public static string MBOLStartWithInvalidCarrierCode
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|B369C067-1FCF-4486-81B5-2666D274DE9D", "This Master Bill Number doesn't start with a valid Japan Customs Carrier Code"); }
			}

			public static string VesselMissingCallSign
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|945833D9-DAD3-4A05-BD53-FC8EE80C854F", "This Vessel doesn't have the Radio Call Sign which is mandatory for JP AFR Messaging."); }
			}

			public static string VesselCallSignReachingMaxAllowed
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|3A0C7022-EB7A-4620-A0FF-0A4B36D005FB", "The Call Sign for Vessel should be no more than 9 characters."); }
			}

			public static string InappropriateVesselCode(string vesselCode)
			{
				return ResString.GetMultilingualString("JPAFRBills|70227A63-1645-4843-AC8B-8278B38D8755", "The Vessel Call Sign \"{0}\" doesn't seem to be appropriate, please double check the correctness of the code.", vesselCode);
			}

			public static string VesselMissingCountryOfReg
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|9882EF98-8501-482F-A419-03C75D670028", "The selected Vessel doesn't have a Country/Region of Registration set."); }
			}

			public static string CarrierOrgHasNoJPCarrierCode
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|75D45DC4-A2CF-4DDB-9BCD-A60F85BCDEE0", "The selected carrier doesn't have a Carrier Code registered under JP, please go to the organization detail to add it in or specify it for this Bill."); }
			}

			public static string CarrierOrgHaveJPCarrierCodeReachedMaxAllowed
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|35D64E65-4C2A-4164-AE07-47E241867039", "The selected carrier have a invalid Carrier Code registered under JP, please go to the organization detail to modify in or specify it for this Bill."); }
			}

			public static string CarrierCodeInvalid
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|F26BD485-0215-40BE-AB11-BF537EA9294E", "Please enter a valid Japan Customs Carrier Code, which should be 3-4 characters long with only alphanumeric characters."); }
			}

			public static string CarrierNotOnRouting
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|5D6287C5-62C8-426F-BFB6-258577148ABE", "The carrier does not match a carrier on any of the routing legs."); }
			}

			public static string LoadingPortCannotBeInJP
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|49F5C2DA-C4B8-4F37-9468-B048B7CDD9A4", "Only the code of foreign port registered in NACCS is allowed for the Port of Loading."); }
			}

			public static string ETAShouldBeNoEarlierThanETD
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|A7643F77-B418-494A-9C68-C6151B90FB15", "The Entered ETA should be no later than the ETD."); }
			}

			public static string PastDateNotAllowedForETA
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|FF1507BA-FD2D-4C87-B341-2DF9FF64EE79", "The past date is not allowed for ETA, please enter future date."); }
			}

			public static string MBOLNumDoesntMatchCarrierCode
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|D8A13EBE-044D-44AB-8EDF-512E7F353D0F", "JP AFR system require the Bill Number start with the valid Carrier Code. Please double check the correctness of your Bill Number or the Carrier Code."); }
			}

			public static string MBOLNumStartWithInvalidCarrierCode
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|C82EA14C-1D8C-4D4C-ACB4-1FF60E925959", "The Bill Number doesn't start with a valid carrier code."); }
			}

			public static string BOLNumberCannotStartWithSpace
			{
				get { return ResString.GetMultilingualString("JPAFRShared|BA06EA33-B71B-47B6-AA20-98416451E9DF", "The Master Bill Number cannot start with spaces."); }
			}

			public static string InvalidPortSuffix
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|1F276027-41A3-4483-AABB-47B302A73C62", "Port Suffix should be a single-digit number between 1 and 9."); }
			}

			public static string VesselDetailsChangedIsNotActive
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|33CE2DD7-BBDD-48C2-995A-6889056133E7", "This feature is not currently active at NACCS. The item setting will be ignored."); }
			}

			public static string VesselIsNotOnFile
			{
				get { return ResString.GetMultilingualString("JPAFRHeader|F96D6BC0-0E2C-41D5-9158-9E452F39AF5A", "Vessel is not on file."); }
			}
		}

		public static class Bill
		{
			#region BOLChecking

			public static string BOLNumMissingNVOCCCode
			{
				get { return ResString.GetMultilingualString("JPAFRShared|913399B0-06CE-48EF-9ED9-AA4BAABB1F5F", "Bill Of Lading Number should be at least 6 characters long with the NVOCC code for NACCS in the first 4 characters. If the NVOCC code is 3-digit, please make sure the character in position 4 is '-' (hyphen sign)."); }
			}

			public static string BOLNumberNotStartWithNVOCCCodeInRegistry(ZString nVOCCInRegistry)
			{
				string result = ResString.GetMultilingualString("JPAFRBills|4CF11C5A-F0B9-4A34-84F5-BF280F9D7BF1", "The Bill Of Lading Number does not start with the NVOCC code ‘{0}’ specified in the registry (Maintain > System > Registry > Customs > Japan > AFR > House Bill NVOCC Code).", nVOCCInRegistry);
				if (nVOCCInRegistry.Length < 4)
				{
					result += ResString.GetMultilingualString("JPAFRBills|FEAE52D9-29BD-4C03-B7DC-3FA20007880F", "Please make sure the character in position 4 is '-' (hyphen sign) if your NVOCC code is 3-digit.", nVOCCInRegistry);
				}
				return result;
			}

			public static string BOLNumberStartWithInvalidNVOCCCode
			{
				get { return ResString.GetMultilingualString("JPAFRBills|516E444F-1571-4165-B869-944C62AF37DD", "The House Bill Number doesn't start with a valid NVOCC code."); }
			}

			public static string DuplicatedBillNumber
			{
				get { return ResString.GetMultilingualString("JPAFRBills|2B0951A8-FE60-4774-A4E9-82D9088F5D38", "The Bill Number already exists in this JP AFR job."); }
			}

			public static string HBOLReachedMaxAllowed
			{
				get { return ResString.GetMultilingualString("JPAFRBills|3BF11A1A-1983-4968-BDBE-6B7F216DC1A8", "JP AFR system allow maximum 99 House Bill per Master Bill."); }
			}

			public static string BOLReachedMaxAllowed
			{
				get { return ResString.GetMultilingualString("JPAFRBills|63514A16-56B5-4B45-B781-3CC5BDCA84D1", "JP AFR system allow maximum 9999 Bill per Vessel Information."); }
			}

			#endregion

			#region ContainerCollection

			public static string AtLeastOneContainerIsRequired
			{
				get { return ResString.GetMultilingualString("JPAFRBills|BFA6996A-9D93-4449-BC0D-4A8DA30EF68A", "A Bill of Lading should have at least one Container Detail."); }
			}

			public static string MaximumContainersCountExceeded
			{
				get { return ResString.GetMultilingualString("JPAFRBills|440707F9-5964-49D9-BA44-5614561D637E", "You can only specify up to 100 Container Details per Bill of Lading."); }
			}

			#endregion

			#region CusCodeData/CusCodeDataCollection

			public static string MaximumNotificationForwardingPartyExceeded
			{
				get { return ResString.GetMultilingualString("JPAFRBills|CA0942B4-58B5-4FB7-9D3C-D2BE704D10F9", "A Bill Of Lading should only have a maximum of 3 Notification Forwarding Party records. Only the first 3 non-empty Notification Forwarding Party will be include in the message delivered."); }
			}

			public static string MaximumOtherRelevantLawExceeded
			{
				get { return ResString.GetMultilingualString("JPAFRBills|2F241198-9B19-4BB5-89AA-AD2348A4ED39", "A Bill Of Lading should only have a maximum of 5 Other Relevant Law Code, Only the first 5 non-empty Other Relevant Law Code will be included in the message delivered."); }
			}

			public static string EmptyCusCodeDataWillNotBeIncluded
			{
				get { return ResString.GetMultilingualString("JPAFRBills|BB3640F6-F4F6-4AF8-9CB2-A091441DA665", "The empty value will not be included in the message delivered."); }
			}

			public static string NotificationForwardingPartyInvalid
			{
				get { return ResString.GetMultilingualString("JPAFRBills|E84C223C-9A24-428C-9304-0422A9400FD2", "Please enter a valid Notification Forwarding Party ID which should be 5 characters long with only alphanumeric characters."); }
			}

			#endregion

			public static string ValueCannotBeNegative
			{
				get { return ResString.GetMultilingualString("JPAFRBills|1144FA8C-83C6-4030-9656-FDAEE83F8B61", "The value can't be negative."); }
			}

			public static string ManifestQtyUseOneForUndescribable
			{
				get { return ResString.GetMultilingualString("JPAFRBills|11B83F89-5A10-4733-B047-A1319990820C", "JP AFR messages doesn't allow Manifest Quantity to be 0, please use 1 if the cargo cannot be described as a countable piece."); }
			}

			public static string ManifestQtyExceedingMaximumNumber
			{
				get { return ResString.GetMultilingualString("JPAFRBills|456F7D4D-9B2D-4533-92D6-0F00A1FD7D99", "The maximum allowed Manifest Quantity is 99,999,999."); }
			}

			public static string FrightValueNoDecimalPartAllowedForJPY
			{
				get { return ResString.GetMultilingualString("JPAFRBills|285A0528-CF39-4787-A058-C96716D6C068", "No decimal part is allowed when the currency is JPY, the decimal part will be truncated in the message delivered."); }
			}

			public static string FrightValueDecimalPartLimitForOtherCurrency
			{
				get { return ResString.GetMultilingualString("JPAFRBills|4768B2C7-D870-49EF-BB8A-8EFD6695CBDF", "Maximum 2 decimal part is allowed, the exceeding part will be truncated in the message delivered."); }
			}

			public static string HSCodeInvalid
			{
				get { return ResString.GetMultilingualString("JPAFRBills|223775BF-CD95-415A-A829-AA817C82E2F3", "Please enter a valid Harmonized code which should be a 6-digit number."); }
			}

			public static string HSCodeNonUniversal
			{
				get { return ResString.GetMultilingualString("JPAFRBills|9A8A938A-0A0F-49F9-9A03-9BA736651B04", "The Harmonized code seems to be inappropriate since it is a non-Universal HS Code."); }
			}

			public static string DeliveryPortCannotEqualDischargePortForTranshipment
			{
				get { return ResString.GetMultilingualString("JPAFRBills|175B9EE8-51B9-47DB-94D4-69D559C548A7", "For shipment which Customs Transit is intended, the Delivery Port should not be the same as the Discharge Port."); }
			}

			public static string AddressIsMandatory
			{
				get { return ResString.GetMultilingualString("JPAFRBills|1EFE89B2-5E41-4E59-A4EA-825F5B9A7130", "The selected address is required for JP AFR messaging."); }
			}

			public static string SpecialCargoCodeIsRequiredForDangerousGoods
			{
				get { return ResString.GetMultilingualString("JPAFRBills|FD59F2C5-C5B6-4F2B-B287-44657BE25D44", "Special Cargo Code is required for Dangerous goods."); }
			}

			public static string InappropriateDescriptionOfGoods(string goodsDescription)
			{
				return ResString.GetMultilingualString("JPAFRBills|CE6AE59C-08E2-4CFA-8D2E-E70DF79A3FA5", "Japan Customs requires a detailed information on Description of Goods to easily identify the contents of cargo in conducting risk assessment.\r\n'{0}' is not an acceptable Description of Goods.", goodsDescription);
			}

			public static string InvalidPortOfOrigin
			{
				get { return ResString.GetMultilingualString("JPAFRBills|E1A42B01-9294-4814-A90F-5E366F072CDE", "The Port of Origin should be outside Japan"); }
			}

			public static string EntryInvalidAsCOCCodeNotValid(ZString fieldName)
			{
				return ResString.GetMultilingualString("JPAFRBills|CE5626ED-1C40-4C6F-94CC-186E4B83EAFA", "Entry of {0} is not allowed because entered Container Operator Code is empty or \"99999\"", fieldName);
			}

			public static string CusCodeExceedingMaximumNumber(string fieldName, int maxNum)
			{
				return ResString.GetMultilingualString("JPAFRBills|A2F8A1D1-D86E-4929-9DF1-52D22A343BD4", "{0} only allow up to {1} codes, exceeding codes may not be included in the message delivered.", fieldName, maxNum);
			}

			public static string ValueEmpty(ZString fieldName)
			{
				return ResString.GetMultilingualString("JPAFRBills|4808DCE3-D354-4D61-B8AC-0F57377976CE", "The {0} can not be 0, please enter a valid value", fieldName);
			}

			public static string DuplicatedUNDG(UNDGSubstance substance)
			{
				return ResString.GetMultilingualString("JPAFRBills|3325a559-ac40-4c69-8dc7-d3c3507e54c3", "The {0} is already selected on this bill.", substance.HumanReadableName);
			}
		}

		public static class JobDocAddress
		{
			public static string AddressFieldIsRequired(string fieldName)
			{
				return ResString.GetMultilingualString("JobDocAddress|1B8AA48C-C78F-44D7-9F01-03632A41913D", "The address you selected doesn't have the {0} information which is required for JP AFR Messaging.", fieldName);
			}

			public static string PhoneNumberLengthReachedMaxAllowed(ZString fieldName, int maxLength)
			{
				return ResString.GetMultilingualString("JobDocAddress|F49F115B-5A04-4494-8D93-2B071E2984C8", "{0} allow maximum {1} characters in JP AFR messages, the number will be re-formatted and then truncated to meet the requirement in the message delivered", fieldName, maxLength);
			}

			public static string InappropriateCompanyInfo(ZString fieldName)
			{
				return ResString.GetMultilingualString("JobDocAddress|0AB11534-C043-4EC8-A0BF-6867C7A2B2F8", "{0} doesn't seem to be appropriate, please double check the correctness of the value.", fieldName);
			}
		}

		public static class Container
		{
			public static string MustOnlyContainAlphaNumerics
			{
				get { return ResString.GetMultilingualString("JPAFRContainer|3E734F3D-26FF-403C-A0BE-3547E4794621", "Invalid Characters in Container Number - container number must only contain alphanumeric characters."); }
			}

			public static string ContainerNumberIsRequired
			{
				get { return ResString.GetMultilingualString("JPAFRContainer|0EFB3912-2304-43F3-87E9-65E1635A2912", "Please enter a valid container number associated with the bill of lading exactly as it physically appears on the container."); }
			}

			public static string ContainerNumberIsDuplicated
			{
				get { return ResString.GetMultilingualString("JPAFRContainer|64F84FBE-9DA4-45F0-8ADA-558D63D84F3F", "Please enter a different Container Number; there is already a Container entered with this number."); }
			}

			public static string MaximumContainerNumberLengthExceeded
			{
				get { return ResString.GetMultilingualString("JPAFRContainer|5900B5E6-6660-4AA8-A4E5-7F1083F1F6C6", "The container number should be no longer than 12 characters."); }
			}

			public static string ContainerNumberShouldContainOnlyAlphanumeric
			{
				get { return ResString.GetMultilingualString("JPAFRContainer|72CC680F-8E48-4E7F-982D-8C6F0787D03C", "Invalid Characters in Container Number - container number must only contain alphanumeric characters."); }
			}

			public static string NoSealRequiredWhenNoSealNumberPresents
			{
				get { return ResString.GetMultilingualString("JPAFRContainer|556481C1-3A0D-418D-B975-2ABEEB36A750", "JP AFR doesn't allow the Seal Number field to be empty, please enter 'NO SEAL' if there is no seal."); }
			}
		}

		public static class InbondDetails
		{
			public static string GoodValueNoDecimalPartAllowedFotJPY
			{
				get { return ResString.GetMultilingualString("JPAFRInbondDetails|CC7BAAF3-7E60-4F9A-8673-3AD1455CA095", "No decimal part is allowed when the currency is JPY, only the integer part will be included in the message delivered."); }
			}

			public static string GoodValueDecimalPartLimitForOtherCurrency
			{
				get { return ResString.GetMultilingualString("JPAFRInbondDetails|F480EC9C-FCB6-49F6-BE21-0155AF877E42", "Maximum 2 decimal part is allowed, the exceeding part will be truncated in the message delivered."); }
			}

			public static string GoodValueCannotBeEmptyForTranshipment
			{
				get { return ResString.GetMultilingualString("JPAFRInbondDetails|6DC168CC-715A-480F-AADB-464E0461EC13", "Please enter the valid Good Value if customs transit is intended."); }
			}

			public static string TemporaryLandingDurationZeroIsNotAllowed
			{
				get { return ResString.GetMultilingualString("JPAFRInbondDetails|BBBD7CA3-F120-4756-97EC-F106C82ACE92", "The temporary landing duration should at least be 1 day if Temporary Landing is intended."); }
			}

			public static string EstimatesDateShouldBeAfterSystemDate
			{
				get { return ResString.GetMultilingualString("JPAFRInbondDetails|08D75F02-86F0-45F3-BD99-E09587FDD212", "The date should be no earlier than current date(Japan Standard Time)"); }
			}

			public static string EFDTShouldBeAfterESDT
			{
				get { return ResString.GetMultilingualString("JPAFRInbondDetails|857AC47D-3960-4008-A177-9831B45CE52F", "The Estimated Finish Date of temporary landing should be no earlier than the Estimated Start Date."); }
			}

			public static string InvalidBondedAreaCode
			{
				get { return ResString.GetMultilingualString("JPAFRInbondDetails|CEBC4399-A333-4B66-9149-358272BADAB9", "The Bonded Area Code should be 5 characters long with only alphanumeric characters. Please refer to NACCS website for latest list of Bonded Area Codes."); }
			}

			public static string EntryInvalidAsNotDischargeInJapan(ZString fieldName)
			{
				return ResString.GetMultilingualString("JPAFRInbondDetails|F0C16924-DC84-4EDE-8525-FADB125A5AE7", "{0} entry is not allowed because entered Port of Discharge Code is port outside Japan.", fieldName);
			}

			public static string NoValueAllowedWhenGeneralCustomsTransitIntended
			{
				get { return ResString.GetMultilingualString("JPAFRInbondDetails|62A42985-439C-4228-A9E4-A5607D8C7622", "Please leave this field empty if General Customs Transit is intended. Or please clear the General Customs Transit Approval Number if Customs Transit of Temporary Landing is intended."); }
			}

			public static string ValueMissingWhenGeneralCustomsTransitIntended
			{
				get { return ResString.GetMultilingualString("JPAFRInbondDetails|1A7169E0-F04E-4377-989C-BD1CD2733C66", "Please enter the Approval Number if General Customs Transit is intended. Or please fill in the Temporary Landing details if Customs Transit of Temporary Landing is intended."); }
			}

			public static string ValueMissingWhenTemporaryLandingIntended(ZString fieldName)
			{
				return ResString.GetMultilingualString("JPAFRInbondDetails|A6C5F181-F5B3-4325-8CF2-F6CB3D71CEBD", "Please enter the {0} if Temporary Landing is intended.", fieldName);
			}
		}

		public static class MessageSending
		{
			public static string ConsolDoesNotDischargeInJapan
			{
				get { return ResString.GetMultilingualString("MessageSendingAction|42EBE18B-274B-45D7-B9BB-0959AA63ABD7", "The consolidation does not have a port of discharge in Japan.  AFR filing is not required. Do you want to proceed? "); }
			}

			public static string BillsToSendThatAreWaitingForResponseMessage(string bills)
			{
				return Res.GetString("MessageSendingAction|5909C2F8-416A-4DD9-84E1-B478C614B9D0", "The following bills have pending responses:\r\n\r\n{0}\r\n\r\nDo you still want to send messages to Customs?", bills);
			}

			public static string BillIsAlreadyRegisteredUseUpdateInsteadOfAddAction
			{
				get { return ResString.GetMultilingualString("AFRMessageSendingObject|91714EB5-500E-4F50-97A2-BCF8FD661DAC", "This Bill Of Lading is already registered; please send an Amendment Message with Action Code 'U' instead."); }
			}

			public static string BillIsAlreadyRegisteredUseUpdateInstead
			{
				get { return ResString.GetMultilingualString("AFRMessageSendingObject|8D8B62C1-E3F3-4B98-9671-EB45374E3DAD", "Action Code 'Add' should not be used when Bill Of Lading is already registered; please use Action Code 'Update' instead."); }
			}

			public static string UnordinaryActionCode(ZString currentAction, ZString suggestAction)
			{
				return ResString.GetMultilingualString("AFRMessageSendingObject|D60C7DCD-517E-48CD-A520-96DA013F0A58", "'{0}' is not an ordinary message action for the bill at current status, please try '{1}'.", currentAction, suggestAction);
			}

			public static string BOLIsEmpty(string bOLField)
			{
				return ResString.GetMultilingualString("AFRMessageSendingObject|74E1F511-BBEA-49B3-B914-04AB7450F890", "{0} is missing", bOLField);
			}

			public static string UpdateDeleteAreNotAllowForMasterWithATD
			{
				get { return ResString.GetMultilingualString("AFRMessageSendingObject|A5AC9CBF-514E-4C57-B641-0EB15CEE1477", "Amendment - Update/Delete are not allow once the Carrier has submitted the ATD message. Unless you have received a Risk Assessment Result for the corresponding bill."); }
			}

			public static string DuplicationBillNumber
			{
				get { return ResString.GetMultilingualString("AFRMessageSendingAction|E7A4DD4F-7F1E-40DF-B19F-FBFAC9285AF0", "There are multiple bills in this Master bill using a same Bill Number which will cause registration with wrong information and status update mismatch. Please correct the Bill Number or merge the information if they are intended for a same Bill."); }
			}

			public static string DeleteReasonCodeIsRequired
			{
				get { return ResString.GetMultilingualString("AFRMessageSendingObject|36FF575F-00CB-4BF6-8F6A-947E898E30EE", "Delete Reason Code is required"); }
			}

			public static string ADeleteReasonMustBeSupplied
			{
				get { return ResString.GetMultilingualString("AFRMessageSendingObject|CB0089CC-202F-4840-88A2-94DD1E36C25B", "A Delete reason must be supplied"); }
			}

			public static string ADeleteReasonMustBeSpecific
			{
				get { return ResString.GetMultilingualString("AFRMessageSendingObject|47F994FC-0109-423D-8122-E028674976AD", "A Delete reason should be specific"); }
			}

			public static string NoBillsHaveBeenChecked => ResString.GetMultilingualString(
				"AFRMessageSendingObject|6BCB1E77-E4DC-4A39-B400-A0D22B9F2BC5",
				"No Bills have been checked.  This will result in no bills being changed.");

			public static string AnyBillIsLeftUnchecked => ResString.GetMultilingualString(
				"AFRMessageSendingObject|807EDF8D-2A31-4B99-933D-F6A20ED3182B",
				"This may result in some Bills not matching the AFR Manifest header details.");

			public static string NoVesselDetailsHaveChanged => ResString.GetMultilingualString(
				"AFRMessageSendingObject|DDC5A769-3CA7-4580-8AC1-3781C5C54862",
				"No Vessel Details have changed from the Original AFR Manifest.");
		}
	}
}
