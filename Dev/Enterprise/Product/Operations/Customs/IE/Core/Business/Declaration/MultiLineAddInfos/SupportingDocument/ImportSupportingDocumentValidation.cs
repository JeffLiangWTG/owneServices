using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportSupportingDocumentValidation : CommonSupportingDocumentValidation
	{
		public ImportSupportingDocumentValidation(SupportingDocument parent) : base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			ValidateReferenceNumber_BR20326();
			ValidateReferenceNumber_BR20329();
			ValidateReferenceNumber_BR20314();
			ValidateReferenceNumber_1D24();
			ValidateReferenceNumber_UCC5MaxLength();
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = Parent;
			var code = parent.CSI_Code;

			ValidateCode_BR20312();
			ValidateCode_BR5149();
			ValidateCode_BR5153();

			#region BR5149

			void ValidateCode_BR5149()
			{
				if (IsValidForBR5149())
				{
					var message = Res.GetString("C7E2842F-2F0D-4D5C-A17A-80B3A514AB33", "[BR5149] 'N990' or 'C990' Supporting Document can only be declared when Requested Procedure is '44'.");
					var info = parent.CSI_CodeInfo;
					var bo = parent.Parent;
					if (bo is CusEntryInstruction instruction && !instruction.IsProcedureCode44)
					{
						info.AddMessageError(message);
					}
					else if (bo is JobComInvoiceHeader invoiceHeader && !invoiceHeader.IsProcedureCode44)
					{
						info.AddMessageError(message);
					}
					else if (bo is JobComInvoiceLine invoiceLine && !invoiceLine.EntryInstruction.IsProcedureCode44)
					{
						info.AddMessageError(message);
					}
				}
			}

			bool IsValidForBR5149()
			{
				switch (code)
				{
					case Constants.SupportingDocumentCodes._C990:
					case Constants.SupportingDocumentCodes._N990:
						return true;
					default:
						return false;
				}
			}

			#endregion

			void ValidateCode_BR20312()
			{
				if (SupportingDocumentCollectionExtensions.IsValidForBR20312(code))
				{
					var messageError = Res.GetString("6C43A76A-3421-424E-8505-EB0753EF5DB5", "[BR20312] Supporting Document Type 'U164', 'U165', 'U166' and 'U167' are mutually exclusive, i.e. only one of them is allowed per item. The exception is the combination of 'U165' and 'U167', which is allowed.");
					var info = parent.CSI_CodeInfo;
					var bo = parent.Parent;
					if (bo is JobComInvoiceHeader invoiceHeader && invoiceHeader.HasMutuallyExclusiveSupportingDocument)
					{
						info.AddMessageError(messageError);
					}
					else if (bo is CusEntryInstruction instruction && instruction.HasMutuallyExclusiveSupportingDocument)
					{
						info.AddMessageError(messageError);
					}
					else if (bo is JobComInvoiceLine invoiceLine && invoiceLine.HasMutuallyExclusiveSupportingDocument)
					{
						info.AddMessageError(messageError);
					}
				}
			}

			void ValidateCode_BR5153()
			{
				if (parent.IsAuthorisationInwardProcessingProcedure()
					&& (parent.Parent is CusEntryInstruction instruction && instruction.IsInwardProcessingProcedure51 && instruction.HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel ||
					parent.Parent is JobComInvoiceHeader invoiceHeader && invoiceHeader.Has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction))
				{
					parent.CSI_CodeInfo.AddMessageError(Res.GetString("3638657A-9936-4F95-A634-2DF41ADEE2C3", "[BR5153] If Requested Procedure is '51', 'C601' Supporting Document should not be declared when there is a '00100' Additional Information entered under the Entry Instructions > Additional Documents tab or the Invoice Headers > Additional Documents tab."));
				}
			}
		}

		void ValidateReferenceNumber_BR20326()
		{
			var parent = Parent;

			if (!parent.CSI_ReferenceNumber.IsEmpty && ShouldCheckReferenceNumberForBR20326(parent.CSI_Code) && !NctsHelper.IsValidCountry(parent.CSI_ReferenceNumber.Left(2), parent.Factory))
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("D445B492-1F39-49AB-9E9A-6E49D4286937", "[BR20326] The first 2 characters of a 'C501', 'C502', or 'C503' Supporting Document Reference Number must be a valid country code"));
			}

			bool ShouldCheckReferenceNumberForBR20326(ZString documentCode)
			{
				switch (documentCode)
				{
					case SupportingDocumentCodes._C501:
					case SupportingDocumentCodes._C502:
					case SupportingDocumentCodes._C503:
						return true;
					default:
						return false;
				}
			}
		}

		void ValidateReferenceNumber_BR20329()
		{
			var parent = Parent;
			var referenceNumber = parent.CSI_ReferenceNumber;

			if (referenceNumber.IsEmpty)
			{
				return;
			}

			var csi_Code = parent.CSI_Code.ToUpper();
			switch (csi_Code)
			{
				case SupportingDocumentCodes._C644:
					ValidateReferenceNumberBR20329_C644();
					break;

				case SupportingDocumentCodes._L100:
					ValidateReferenceNumberBR20329_L100();
					break;

				case SupportingDocumentCodes._C085:
				case SupportingDocumentCodes._C678:
				case SupportingDocumentCodes._N853:
				case SupportingDocumentCodes._C640:
					ValidateReferenceNumberBR20329_C085_C678_N853_C640();
					break;

				case SupportingDocumentCodes._C057:
				case SupportingDocumentCodes._C082:
				case SupportingDocumentCodes._C079:
					// Do nothing
					break;

				case SupportingDocumentCodes._Y120:
				case SupportingDocumentCodes._Y123:
				case SupportingDocumentCodes._Y124:
				case SupportingDocumentCodes._Y125:
				case SupportingDocumentCodes._Y951:
				case SupportingDocumentCodes._Y986:
					if (parent.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference))
					{
						// Do nothing
					}
					break;

				default:
					break;
			}

			void ValidateReferenceNumberBR20329_C644()
			{
				if (!IsValidReferenceNumberBR20329_C644())
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("5B053E13-19B3-4DA2-B0E2-EDC68706FF08", "[BR20329] The format for a COI Certificate Number should be either COI.XX.YYYY.nnnnnnn or COI.XX.YYYY.nnnnnnn/mm, where XX is the country code of issuance, YYYY is the year, nnnnnnn is the 7-digit number, and mm is the 2-digit extract number."));
				}

				ZBool IsValidReferenceNumberBR20329_C644()
				{
					var referenceNumberParts = referenceNumber.Split('.');

					if (referenceNumberParts.Length == 4)
					{
						var targetSpecificCodes = SupportingDocumentCodes.BR20239SpecificCode[parent.CSI_Code];
						var currentSpecificCode = referenceNumberParts[0];
						var countryCode = referenceNumberParts[1];
						var yearInString = referenceNumberParts[2];
						var sequenceNumber = referenceNumberParts[3];

						if (targetSpecificCodes.Contains(currentSpecificCode) &&
							RefCountry.LoadFromCountryCode(parent.Factory, countryCode) != null &&
							yearInString.Length == 4 && ushort.TryParse(yearInString, out var issuanceYear) && issuanceYear <= ZDate.Today.Year &&
							((sequenceNumber.Length == 7 && uint.TryParse(sequenceNumber, out _)) ||
							 (sequenceNumber.Length == 10 && sequenceNumber[7] == '/' && uint.TryParse(sequenceNumber.Left(7), out _) && ushort.TryParse(sequenceNumber.Right(2), out _))))
						{
							return true;
						}
					}
					return false;
				}
			}

			void ValidateReferenceNumberBR20329_L100()
			{
				if (!IsValidReferenceNumberBR20329_L100())
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("F3829B4A-8C8C-4BC4-AF94-95C3BA802D1D", "[BR20329] The format for a ODS License should be ZZZ-XXXX-KKKK-YYYY-NNNNNNNN, where ZZZ is 'EXP' or 'IMP', XXXX is the ID composed of 2 letters followed by a 2-digit number, KKKK is the license type, YYYY is the license year, NNNNNNNN is the 8-digit number."));
				}

				ZBool IsValidReferenceNumberBR20329_L100()
				{
					var referenceNumberParts = referenceNumber.Split('-');

					if (referenceNumberParts.Length == 5)
					{
						var targetSpecificCodes = SupportingDocumentCodes.BR20239SpecificCode[parent.CSI_Code];
						var currentSpecificCode = referenceNumberParts[0];
						if (!targetSpecificCodes.Contains(currentSpecificCode))
						{
							return false;
						}

						var undertakingConcerned = referenceNumberParts[1];
						var targetLicenceTypes = SupportingDocumentCodes.LicenceTypeBR20329[currentSpecificCode];

						var currentLicenceType = referenceNumberParts[2];
						var yearInString = referenceNumberParts[3];
						var sequenceNumber = referenceNumberParts[4];

						if (undertakingConcerned.Left(2).IsLettersOnlyOrEmpty && undertakingConcerned.Right(2).IsNumbersOnlyOrEmpty &&
							targetLicenceTypes.Contains(currentLicenceType) &&
							yearInString.Length == 4 && ushort.TryParse(yearInString, out var issuanceYear) && issuanceYear <= ZDate.Today.Year &&
							sequenceNumber.Length == 8 && uint.TryParse(sequenceNumber, out _))
						{
							return true;
						}
					}
					return false;
				}
			}

			void ValidateReferenceNumberBR20329_C085_C678_N853_C640()
			{
				if (!IsValidReferenceNumberBR20329_C085_C678_N853_C640())
				{
					switch (parent.CSI_Code)
					{
						case SupportingDocumentCodes._C085:
							parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("051EBCFB-D9F9-4CDC-B08F-C8E2C334B0A4", "[BR20329] The format for a CHED_PP Certificate should be either CHEDPP.XX.YYYY.nnnnnnn or CHEDPP.XX.YYYY.nnnnnnnR or CHEDPP.XX.YYYY.nnnnnnnV, where XX is the country code of issuance, YYYY is the year, nnnnnnn is the 7-digit number, can end with 'R' is partially Rejected, 'V' is partially Validated."));
							break;

						case SupportingDocumentCodes._C678:
							parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("A7F39451-FC5A-43C6-BB7D-035801B17898", "[BR20329] The format for a CHED_D Certificate should be either CHEDD.XX.YYYY.nnnnnnn or CHEDD.XX.YYYY.nnnnnnnR or CHEDD.XX.YYYY.nnnnnnnV, where XX is the country code of issuance, YYYY is the year, nnnnnnnn is the 7-digit number, can end with 'R' is partially Rejected, 'V' is partially Validated."));
							break;

						case SupportingDocumentCodes._N853:
							parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("DEE6CFA3-D862-4FA2-B999-7F487F9FC47A", "[BR20329] The format for a CHED_P/CVED_P Certificate should be either CHEDP/CVEDP.XX.YYYY.nnnnnnnR or CHEDP/CVEDP.XX.YYYY.nnnnnnnV, CHEDP/CVEDP.XX.YYYY.nnnnnnn where XX is the country code of issuance, YYYY is the year, nnnnnnn is the 7-digit number, can end with 'R' is partially Rejected, 'V' is partially Validated."));
							break;

						case SupportingDocumentCodes._C640:
							parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("56A2F31A-2B27-4520-A7CA-5FA11FC06F43", "[BR20329] The format for a CHED_A Certificate should be either CHEDA.XX.YYYY.nnnnnnn or CHEDA.XX.YYYY.nnnnnnnR or CHEDA.XX.YYYY.nnnnnnnV, where XX is the country code of issuance, YYYY is the year, nnnnnnn is the 7-digit number, can end with 'R' is partially Rejected, 'V' is partially Validated."));
							break;
					}
				}

				ZBool IsValidReferenceNumberBR20329_C085_C678_N853_C640()
				{
					var referenceNumberParts = referenceNumber.Split('.');

					if (referenceNumberParts.Length == 4)
					{
						var targetSpecificCodes = SupportingDocumentCodes.BR20239SpecificCode[parent.CSI_Code];
						var currentSpecificCode = referenceNumberParts[0];

						var countryCode = referenceNumberParts[1];
						var yearInString = referenceNumberParts[2];
						var sequenceCode = referenceNumberParts[3];

						if (targetSpecificCodes.Contains(currentSpecificCode) &&
							RefCountry.LoadFromCountryCode(parent.Factory, countryCode) != null &&
							yearInString.Length == 4 && ushort.TryParse(yearInString, out _) &&
							((sequenceCode.Length == 7 && uint.TryParse(sequenceCode.Left(7), out _)) ||
							 (sequenceCode.Length == 8 && uint.TryParse(sequenceCode.Left(7), out _) && SupportingDocumentCodes.PartiallyStatusBR20329.Contains(sequenceCode.Right(1)))))
						{
							return true;
						}
					}
					return false;
				}
			}
		}

		void ValidateReferenceNumber_BR20314()
		{
			var parent = Parent;
			if (parent.CSI_Code == SupportingDocumentCodes._C100 && !parent.CSI_ReferenceNumber.IsEmpty)
			{
				var referenceNumber = parent.CSI_ReferenceNumber;
				var parentOfSupportingDocument = parent.Parent;
				if (BR20314_C100Duplicating(parentOfSupportingDocument, referenceNumber))
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(BR20314ErrorMessage);
				}
			}
		}

		void ValidateReferenceNumber_1D24()
		{
			var parent = Parent;
			if (Constants.SupportingDocumentCodes.IsEstimatedDestination(parent) && (
					!ZDateTime.TryParseExact(parent.CSI_ReferenceNumber, out var estimatedDestination, Constants.DateTimeFormat.AdditionalReferenceDateTime)
					|| estimatedDestination.IsEmpty
			))
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString(
					"13E76EC6-DA17-40FB-9E89-0E828F79FA32", $"1D24 Reference is mandatory and must be in the correct format: '{Constants.DateTimeFormat.AdditionalReferenceDateTime}'"
				));
			}
		}

		void ValidateReferenceNumber_UCC5MaxLength()
		{
			var parent = Parent;
			if (parent.Declaration is JobDeclaration declaration && declaration.IsUCC5 && parent.CSI_ReferenceNumber.Length > SupportingDocument.CSI_ReferenceNumberMaxLength_AISUCC5)
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("8B149568-5F07-4D40-A2AF-97D557EC9F90", "Supporting Document Reference can have up to {0} alpha numeric characters.", SupportingDocument.CSI_ReferenceNumberMaxLength_AISUCC5));
			}
		}

		protected virtual bool BR20314_C100Duplicating(BusinessObject parentOfSupportingDocument, ZString c100ReferenceNumber)
		{
			var result = false;
			if (parentOfSupportingDocument is CusEntryInstruction entryInstruction)
			{
				result = entryInstruction.DuplicatedEntryInstructionAndInvoiceC100SupportingDocReferences.Contains(c100ReferenceNumber);
			}
			else if (parentOfSupportingDocument is JobComInvoiceHeader invoice)
			{
				result = invoice.CusEntryInstructions.Cast<CusEntryInstruction>().Any(instruction => BR20314_C100Duplicating(instruction, c100ReferenceNumber));
			}
			return result;
		}

		protected virtual string BR20314ErrorMessage => Res.GetString("3391BE88-505B-4889-A2F7-C7ECA92760E3", "[BR20314] Supporting document type C100 (REX Registered Exporter Number) must be unique across this entry instruction and all its related invoice headers.");
	}
}
