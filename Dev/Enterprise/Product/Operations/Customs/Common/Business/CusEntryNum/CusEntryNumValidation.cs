using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common
{
	public class CusEntryNumValidation : AutoCusEntryNumValidation
	{
		public CusEntryNumValidation(AutoCusEntryNum parent) : base(parent)
		{
		}

		public new CusEntryNumber Parent
		{
			get { return (CusEntryNumber)base.Parent; }
		}

		#region CE_EntryType

		protected override void CheckCE_EntryType()
		{
			base.CheckCE_EntryType();

			var additionalReferenceNumberValidationProvider = Parent.Parent as IAdditionalReferenceNumberValidationProvider;
			if (additionalReferenceNumberValidationProvider != null)
			{
				additionalReferenceNumberValidationProvider.ValidateEntryType(Parent.CE_EntryTypeInfo, Parent.CE_Category, Parent.CE_RN_NKCountryCode);
			}
			if (Parent.CE_Category == CusEntryNumber.Categories.AdditionalReferenceNumber)
			{
				MandatoryValidation.CheckEntered(Parent.CE_EntryTypeInfo);

				var entryType = Parent.CE_EntryType;
				var parentImportStatus = Parent.Parent as ISupportDataImporting;
				var customsReferenceNumberType = Parent.Lookups.AdditionalReferenceNumberTypes.OfType<ICustomsNumberTypeCodeDescription>().FirstOrDefault(codeDescription => codeDescription.Code.Equals(Parent.CE_EntryType, StringComparison.OrdinalIgnoreCase));
				var isControlledByAutomation = !Parent.IsInDatabase && (customsReferenceNumberType?.IsAutomation ?? false) && !(parentImportStatus?.IsImportingData ?? false);

				if (!entryType.IsEmpty && isControlledByAutomation)
				{
					var message = ResString.GetMultilingualString(
						"D1253874-3ACF-4B7A-8BEA-BF8DFF6D7E2D",
						"Entry Type [{0}] can only be added by Universal XML.",
						entryType.ToUpperInvariant());

					Parent.CE_EntryTypeInfo.AddError(message);
				}

				if (!entryType.IsEmpty && !Parent.Lookups.AdditionalReferenceNumberTypes.ContainsCode(entryType))
				{
					var message = ResString.GetMultilingualString("50712a6c-4303-4c54-982a-807750bf11b4"
						, "{0} Please check whether the code or the {1} is correct."
						, ListValidation.InvalidCodeMessageError
						, Parent.CE_RN_NKCountryCodeInfo.HumanReadableName);

					Parent.CE_EntryTypeInfo.AddError(message);
				}

				if (additionalReferenceNumberValidationProvider == null || additionalReferenceNumberValidationProvider.EntryTypeShouldBeUnique(Parent.CE_EntryType, Parent.CE_Category, Parent.CE_RN_NKCountryCode))
				{
					ValidateCE_EntryTypeUniqueness();
				}

				if (!Parent.CE_EntryType.IsEmpty)
				{
					ValidateCE_EntryNum();
				}

				ValidateCustomsAuthorisationReferenceNumberType_ForGBOnly(Parent.CE_EntryTypeInfo);
			}
		}

		void ValidateCE_EntryTypeUniqueness()
		{
			var otherEntryQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, Parent.CE_ParentID);
			otherEntryQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
			otherEntryQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Parent.CE_RN_NKCountryCode);
			otherEntryQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.NotEqual, Parent.Lookups.AdditionalReferenceNumberTypes.NonUnique());

			var entryNumbers = Parent.Factory.Load<CusEntryNumber>(otherEntryQuery);

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.CE_EntryTypeInfo, entryNumbers);

			foreach (var number in entryNumbers)
			{
				if (number.PK != Parent.PK)
				{
					number.Validation.ValidateCE_EntryType();
				}
			}
		}

		#endregion

		protected override void CheckCE_EntryNum()
		{
			base.CheckCE_EntryNum();

			if (Parent.CE_Category == CusEntryNumber.Categories.AdditionalReferenceNumber)
			{
				var duplicatedEntryNumbersQuery = new ZQuery(CusEntryNumSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				duplicatedEntryNumbersQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, Parent.CE_ParentID);
				duplicatedEntryNumbersQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
				duplicatedEntryNumbersQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Parent.CE_RN_NKCountryCode);
				duplicatedEntryNumbersQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Parent.CE_EntryType);
				duplicatedEntryNumbersQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, Parent.CE_EntryNum);

				if (Parent.Factory.LoadTop1<CusEntryNumber>(duplicatedEntryNumbersQuery) != null)
				{
					Parent.CE_EntryNumInfo.AddError(Res.GetString("0FC8696A-58EC-4E17-84B2-AAFDA1A421EA", "There is another entry with the same type and number."));
				}

				if (!Parent.CE_EntryType.IsEmpty)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.CE_EntryNumInfo);
				}

				if (Parent.CE_EntryType == UnitedStatesAdditionalReferenceNumberTypes.Codes.IT && Parent.CE_EntryNum.Length > 11)
				{
					Parent.CE_EntryNumInfo.AddWarning(ITNumberLengthExceeded);
				}
				if (Parent.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomsAuthorisationReference)
				{
					ValidateCustomsAuthorisationReferenceNumberType_Format(Parent.CE_RN_NKCountryCode, Parent.CE_EntryNumInfo, Parent.Parent as IImportExport);
				}

				CheckCE_EntryNumForAE();

				CheckCE_EntryNumForEG(Parent.CE_EntryNumInfo);

				if (Parent.Parent != null)
				{
					var binding = Parent.Parent as IAdditionalReferenceNumberSupporter;
					if (binding != null)
					{ binding.AdditionalEntryNumberValidation(Parent.CE_EntryNumInfo, Parent.CE_EntryType, Parent.CE_EntryNum); }
				}

				if (Parent.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN && Parent.CE_ParentTable == JobShipmentSchema.Constants.TableName && Parent.CE_EntryNum.Trim().Length > 0)
				{
					var duplicatedCCNQuery = new ZQuery(CusEntryNumSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					duplicatedCCNQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.NotEqual, Parent.CE_ParentID);
					duplicatedCCNQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, Parent.CE_ParentTable);
					duplicatedCCNQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
					duplicatedCCNQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
					duplicatedCCNQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Parent.CE_EntryType);
					duplicatedCCNQuery.AddToFilter(GetCCNNumberEqualQuery(CusEntryNumSchema.CE_EntryNum, Parent.CE_EntryNum));
					if (Parent.Factory.Exists(typeof(CusEntryNumber), duplicatedCCNQuery))
					{
						Parent.CE_EntryNumInfo.AddWarning(AlreadyContainsCCN);
					}
				}
			}
			if (Parent.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber && !Parent.CE_EntryIsSystemGenerated)
			{
				string warningMessage = MovementReferenceNumberValidator.ApplyAdditionalValidationOnMrn(Parent.CE_EntryNum, MrnTypes.Unknown);
				if (!warningMessage.Equals(ZString.Empty))
				{
					Parent.CE_EntryNumInfo.AddWarning(warningMessage);
				}
			}
		}

		void CheckCE_EntryNumForAE()
		{
			var entryType = Parent.CE_EntryType;
			var entryNumber = Parent.CE_EntryNum;
			var entryNumberInfo = Parent.CE_EntryNumInfo;
			if (entryType == UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.UAEInstalmentNumber)
			{
				if (!entryNumber.IsNumbersOnlyOrEmpty)
				{
					entryNumberInfo.AddError(Res.GetString("E9A55220-3C9F-4A79-864B-A68C47C2D33F", "UAE Installment Number must be a whole number."));
				}
				else
				{
					if (!entryNumber.IsEmpty)
					{
						if (entryNumber.Replace("0", "").IsEmpty)
						{
							entryNumberInfo.AddError(Res.GetString("5D15E33A-941C-44C9-809D-C1C49C048FCC", "UAE Installment Number must be greater than or equal to 1."));
						}
					}
					else
					{
						entryNumberInfo.AddError(Res.GetString("D410A26A-3DDF-498C-AD0F-75E0C3DA6275", "You have not entered a UAE Installment Number."));
					}
				}
			}

			if (entryType == UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.RotationNumber && (!entryNumber.IsLettersAndNumbersOnlyOrEmpty || entryNumber.Length != 6))
			{
				entryNumberInfo.AddError(Res.GetString("BAFE6DE0-64C3-47B9-B4B3-46B88D85C593", "ROT Number should only contain 6 letters and digits."));
			}
		}

		void CheckCE_EntryNumForEG(ZPropertyInfo info)
		{
			if (Parent.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference
				&& Parent.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Egypt
				&& (Parent.CE_EntryNum.Length != 19 || !Parent.CE_EntryNum.IsNumbersOnlyOrEmpty))
			{
				info.AddError(Res.GetString("bf2f9f31-204b-474c-b29b-0a9d88e09b9d", "ACI Number should contain 19 digits when country/region of issue is EG."));
			}
		}

		public static ZQuery GetCCNNumberEqualQuery(SchemaStringColumn column, ZString ccNumber)
		{
			var result = new ZQuery(column, ccNumber);
			var equivalentCCNumber = GetEquivalentCCNumber(ccNumber);
			if (equivalentCCNumber != ccNumber)
			{
				result.AddToFilter(JoinCondition.Or, column, equivalentCCNumber);
			}

			return result;
		}

		const int carrierCodeLength = 4;
		const string delimiter = " ";

		static ZString GetEquivalentCCNumber(ZString ccNumber)
		{
			var result = ccNumber;

			if (ccNumber.Length > carrierCodeLength)
			{
				if (ccNumber[carrierCodeLength].ToString() == delimiter)
				{
					result = ccNumber.RemoveSafe(carrierCodeLength, 1);
				}
				else
				{
					result = ccNumber.InsertSafe(carrierCodeLength, delimiter);
				}
			}
			return result;
		}

		internal static string AlreadyContainsCCN
		{
			get { return Res.GetString("de3c5ece-7875-42ab-8098-159a7a0c7d85", "Another Shipment already contains the same CCN as this shipment."); }
		}

		internal static string ITNumberLengthExceeded
		{
			get { return Res.GetString("1315A67D-1628-4784-B39E-D0D31F8D2143", "Length of Inbond Transit (IT) Number is exceeded and should be 11 numerics. The IT Number entered will be truncated to 11 characters during synchronization with Customs Declaration."); }
		}

		protected override void CheckCE_RN_NKCountryCode()
		{
			base.CheckCE_RN_NKCountryCode();

			if (!((Parent.CE_RN_NKCountryCode == Constants.CountryCodes.EuropeanUnion) &&
				  (Parent.CE_Category == CusEntryNumber.Categories.InspectionStatus || Parent.CE_Category == CusEntryNumber.Categories.CustomsPermitClearanceNumber)))
			{
				ListValidation.ErrorIfInvalidCode(Parent.CE_RN_NKCountryCodeInfo);
			}
		}

		/// <summary>
		///  This lives here and not in GB because a CusEntryNum of type CAR can applied to a GB declaration, GB entry, shipment or consol.
		/// </summary>
		public static void ValidateCustomsAuthorisationReferenceNumberType_Format(ZString numberCountry, ZPropertyInfo entryNumberInfo, IImportExport parentJob)
		{
			if (numberCountry == Core.Constants.CountryCodes.UnitedKingdom)
			{
				// xFBnn-abc-ynnnn
				//	x	=‘I’ (import) or ‘E’ (export)  
				//	nn	= fallback code specific to event (01-99) – a unique number relating to a specific period of fallback
				//	abc	= the badge code of the agent to whom the reference was allocated
				//	y	= the level at which the authorisation was granted - ‘M’ (Header) or ‘C’ (Consignment), or ‘D’ (Declaration)
				//	n	= sequential number (4 digits) 0001 - 9999
				ZString entryNumber = entryNumberInfo.Value.ToString();
				var regularExpression = new Regex(@"[IE]FB[0-9]{2}-[A-Z]{3}-[MCD][0-9]{4}");
				var patternMismatch = false;
				if (!regularExpression.IsMatch(entryNumber))
				{
					entryNumberInfo.AddMessageError(Res.GetString("1F133FE9-EDDD-4891-8336-3CCCCFAC2700", "Customs Authorization Reference does not match required pattern of xFBnn-abc-ynnnn.  Please check your data."));
					patternMismatch = true;
				}
				if (!patternMismatch)
				{
					if (parentJob != null)
					{
						if (!entryNumber.StartsWith("E") && parentJob.IsExport())
						{
							entryNumberInfo.AddMessageError(Res.GetString("39BDA69D-85DB-48AA-99D7-4F185440BD39", "Parent is an export job.  CAR must start with 'E'."));
						}
						else if (!entryNumber.StartsWith("I") && parentJob.IsImport())
						{
							entryNumberInfo.AddMessageError(Res.GetString("55C059BD-3D2D-4A10-8936-21926C434DF7", "Parent is an import job.  CAR must start with 'I'."));
						}
					}
				}
			}
		}

		void ValidateCustomsAuthorisationReferenceNumberType_ForGBOnly(ZPropertyInfo zPropertyInfo)
		{
			if (Parent.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomsAuthorisationReference
				&& !Parent.CE_RN_NKCountryCode.IsEmpty
				&& Parent.CE_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedKingdom)
			{
				zPropertyInfo.AddMessageError(Res.GetString("29A89ECC-C690-4724-8CE6-24E143044E09", "CAR is only for United Kingdom"));
			}
		}
	}
}
