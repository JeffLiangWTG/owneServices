using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CusReconSnapshot = Enterprise.Customs.Business.CusReconSnapshot;
using RefCusCodeListType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.DE.Business
{
	public static class Extensions
	{
		public static ZString GetATLASParticipantIdentificationNumber(this OrgAddress orgAddress) => orgAddress?.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, Core.Constants.CountryCodes.Germany) ?? ZString.Empty;

		public static ZString GetCustomsRegNo(this OrgHeader orgHeader, string codeType) => orgHeader?.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.Germany) ?? ZString.Empty;

		public static ZString GetCustomsRegNo(this OrgAddress orgAddress, string codeType) => orgAddress?.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.Germany) ?? ZString.Empty;

		public static OrgCusCode GetOrgCusCode(this OrgHeader orgHeader, string codeType, string regNo) => orgHeader?.CustomsCodes.GetOrgCusCodesForCodeAndCountry(codeType, Core.Constants.CountryCodes.Germany).SingleOrDefault(x => x.OK_CustomsRegNo == regNo);

		public static ZBool HasEUEoriRegNo(this OrgHeader orgHeader)
		{
			var cusCodes = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			var result = cusCodes?.Where(x => x.OK_RN_NKCodeCountry.In(CountryListCL010(orgHeader.Factory)));
			return result != null && result.Count() == 1;
		}

		public static ZBool HasEoriOrTcu(this OrgHeader orgHeader)
		{
			return (orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).Any() ?? false)
				|| (orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU).Any() ?? false);
		}

		public static ZString GetEUEoriNumber(this OrgHeader header, ZString countryCode)
		{
			var result = ZString.Empty;
			if (countryCode.In(CountryListCL010(header.Factory)))
			{
				result = header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, countryCode) ?? ZString.Empty;
			}
			return result;
		}

		public static bool HasEUEoriNumber(this OrgHeader orgHeader)
		{
			var result = false;
			if (orgHeader != null)
			{
				var countryListCL010 = Universal.RefCusCodeListTypes.GetCachedList(orgHeader.Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, ZDateTime.Today);
				var allEoriNumbers = orgHeader.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				result = allEoriNumbers.Where(x => x.OK_RN_NKCodeCountry.In(countryListCL010.GetAllCodesZString())).Any();
			}
			return result;
		}

		public static ZString GetEUEoriDetails(this OrgHeader orgHeader, bool errorOnMultiple = false)
		{
			return orgHeader?.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, errorOnMultiple) ?? ZString.Empty;
		}

		public static ZString GetConcatenatedSingleOrgCusCodeIgnoringCountry(this OrgHeader orgHeader, ZString codeType, bool errorOnMultiple = false)
		{
			var array = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType);
			var euOrgCusCodes = array?.Where(x => x.OK_RN_NKCodeCountry.In(CountryListCL010(orgHeader.Factory)));
			if (euOrgCusCodes != null && euOrgCusCodes.Any())
			{
				if (euOrgCusCodes.Count() == 1)
				{
					var orgCusCode = euOrgCusCodes.First();
					return orgCusCode.OK_CustomsRegNo.AddPrefixToNumber(orgCusCode.OK_RN_NKCodeCountry);
				}

				if (errorOnMultiple)
				{
					return $"* multiple {codeType} *";
				}
			}
			return ZString.Empty;
		}

		static ZString[] CountryListCL010(BusinessObjectFactory factory) => RefCusCodeListTypes.GetCachedList(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, ZDateTime.Today).GetAllCodesZString();

		public static ZString GetOfficeReferenceNumber(this Declaration.JobDeclaration dec, ZString type) => dec?.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == type)?.CY_Data ?? ZString.Empty;

		public static T GetAllSameValue<T>(this Declaration.CusEntryHeader entryHeader, Func<JobComInvoiceHeader, T> fieldGetter)
		{
			var result = entryHeader != null && fieldGetter != null ? entryHeader.InvoiceHeaders.Select(x => fieldGetter(x)).Distinct().Take(2).ToArray() : null;
			return result == null || result.Length != 1 ? default : result[0];
		}

		public static CusTempStorageRegLine GetRegLine(this CusTempStorageRegHeader regHeader, ZString sequenceNumber)
		{
			CusTempStorageRegLine line = null;

			if (!sequenceNumber.IsEmpty
				&& ZInt.TryParse(sequenceNumber, out var sequenceNo))
			{
				line = regHeader.CusTempStorageRegLines
					.Cast<CusTempStorageRegLine>()
					.FirstOrDefault(x => x.SRL_LineNumber == sequenceNo);
			}
			return line;
		}

		public static ZInt CalculatePackageQtySumFromTransactions(this CusTempStorageRegLine regLine) => regLine.CusTempStorageRegLineTransactions.Sum(t => t.SRT_PackageQty);

		public static ZString XmlEnumToString<TEnum>(this TEnum value) where TEnum : struct, IConvertible
		{
			var result = ZString.Empty;

			var type = typeof(TEnum);
			if (type.IsEnum)
			{
				var name = Enum.GetName(type, value);
				if (name != null)
				{
					var attribute = type.GetField(name).GetCustomAttribute<XmlEnumAttribute>(false);
					result = attribute?.Name ?? name;
				}
			}

			return result;
		}

		public static bool IsDigitCheckSatisfied(this ZString value, ZInt index, char expectedCharacter) => value.SubstringSafe(index, 1).EqualsIgnoringCase(expectedCharacter.ToString());

		public static ZDecimal GetCustomsDeclarationValue(this BusinessObjectFactory factory)
			=> new RefCusTaxOrFee.Loader(factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.TaxesOrFees.Codes.DV1Value, ZDateTime.Today)?.ZZF_Value ?? ZDecimal.Zero;

		public static IEnumerable<OrgCusAccount> GetDefermentAccounts(this OrgHeader org)
		{
			return org?.DefermentAccountNumberCollection.Cast<OrgCusAccount>()
						.Where(x => x.CZ_RN_NKCountryCode == Core.Constants.CountryCodes.Germany)
						?? Enumerable.Empty<OrgCusAccount>();
		}

		public static CodeDescriptionPairList GetCountriesInC0063(this BusinessObjectFactory factory) => factory.GetCachedListForCodeType(RefCusCodeListType.Code_C0063);

		public static CodeDescriptionPairList GetCountriesInNC010(this BusinessObjectFactory factory) => factory.GetCachedListForCodeType(RefCusCodeListType.Code_NC010);

		static CodeDescriptionPairList GetCachedListForCodeType(this BusinessObjectFactory factory, string codeType)
			=> RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Germany, codeType, ZDateTime.Today);

		public static bool HasSupportingDocumentsOfType(this Declaration.JobComInvoiceHeader invoice, ZString csiCode) => invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == csiCode);

		public static bool HasSupportingDocumentsOfType(this Declaration.JobComInvoiceLine invoiceLine, ZString csiCode) => invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == csiCode);

		public static bool HasConcessionOfType(this Declaration.JobComInvoiceLine invoiceLine, ZString type) => invoiceLine.Concession == type;

		public static bool HasPreviousDocumentsOfType(this Declaration.JobComInvoiceHeader invoice, ZString csiCode) => invoice.PreviousDocuments.Cast<Declaration.PreviousDocument>().Any(x => x.CSI_Code == csiCode);

		public static bool HasPreviousDocumentsOfType(this Declaration.JobComInvoiceLine invoiceLine, ZString csiCode) => invoiceLine.PreviousDocuments.Cast<Declaration.PreviousDocument>().Any(x => x.CSI_Code == csiCode);

		public static OrgHeader GetOrgHeaderByCustomsRegNo(this BusinessObjectFactory factory, ZString countryCode, ZString codeType, ZString regNo)
		{
			OrgHeader result = null;
			if (factory != null && !countryCode.IsEmpty && !codeType.IsEmpty && !regNo.IsEmpty)
			{
				result = new OrgHeader.Loader(factory).LoadDBOrganisations(countryCode, codeType, regNo).OrderBy(x => x.OH_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}

		public static OrgAddress GetOrgAddressByCustomsRegNo(this BusinessObjectFactory factory, ZString countryCode, ZString codeType, ZString regNo)
		{
			OrgAddress result = null;
			if (factory != null && !countryCode.IsEmpty && !codeType.IsEmpty && !regNo.IsEmpty)
			{
				result = new OrgAddress.Loader(factory).LoadDBAddresses(countryCode, codeType, regNo).OrderBy(x => x.OA_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}

		public static Declaration.InvoiceLineCharge GetCharge(this JobComInvoiceLine invoiceLine, ZString chargeCode)
		{
			return invoiceLine.Charges.Cast<Declaration.InvoiceLineCharge>().SingleOrDefault(c => c.J7_ChargeType == chargeCode);
		}

		public static ZDecimal GetTobaccoRetailSellingPriceOrZero(this BusinessObjectFactory factory, ZString uq)
		{
			var mappedTobaccoRetailSellingPriceCode = ZString.Empty;
			if (uq == Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems)
			{
				mappedTobaccoRetailSellingPriceCode = UniversalReferenceConstants.TaxesOrFees.Codes.TobaccoRetailSellingPricePerUnit;
			}
			else if (uq == Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram)
			{
				mappedTobaccoRetailSellingPriceCode = UniversalReferenceConstants.TaxesOrFees.Codes.TobaccoRetailSellingPricePerKilo;
			}

			return mappedTobaccoRetailSellingPriceCode.IsEmpty || factory == null
				? ZDecimal.Zero
				: new RefCusTaxOrFee.Loader(factory).LoadTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Germany, mappedTobaccoRetailSellingPriceCode, ZDateTime.Today)?
					.SingleOrDefault(x => x.ZZF_ZX0_NKTaxOrFeeType == UniversalReferenceConstants.TaxesOrFees.Types.TobaccoRetailSellingPrice)?
					.ZZF_Value ?? ZDecimal.Zero;
		}

		public static CusReconEntry GetCusReconEntry(this CusEntryHeader entryHeader)
		{
			CusReconEntry result = null;
			if (entryHeader != null)
			{
				var query = new ZQuery(CusReconEntrySchema.CRE_CH_OriginalEntry, entryHeader.PK);
				result = entryHeader.Factory.LoadTop1<CusReconEntry>(query);
			}
			return result;
		}

		public static CusReconEntry GetCusReconEntryFromDB(this CusEntryHeader entryHeader)
		{
			CusReconEntry result = null;
			if (entryHeader != null)
			{
				var query = new ZDBOnlyQuery(typeof(CusReconEntry));
				query.AddToFilter(CusReconEntrySchema.CRE_CH_OriginalEntry, entryHeader.PK);
				result = entryHeader.Factory.LoadTop1<CusReconEntry>(query);
			}
			return result;
		}

		public static CusReconEntryLine GetCusReconEntryLineByOriginalEntryLineNumber(this CusReconEntry cusReconEntry, string originalEntryLineNumber)
		{
			return (CusReconEntryLine)cusReconEntry?.CusReconEntryLines.FirstOrDefault(x => x.CRL_OriginalEntryLineNumber.ToString() == originalEntryLineNumber);
		}

		public static CusReconEntry DuplicateCusReconEntryAndUnlinkFromDeclaration(this CusReconEntry originalCusReconEntry)
		{
			var duplicateCusReconEntry = originalCusReconEntry?.Clone(new BusinessObjectCloneArgs(new string[] { CusReconEntry.Schema.CRE_CRD })) as CusReconEntry;
			if (duplicateCusReconEntry != null)
			{
				duplicateCusReconEntry.CusReconSnapshots.AddRange(CopyCusReconEntrySnapshots());
			}
			return duplicateCusReconEntry;

			IEnumerable<CusReconSnapshot> CopyCusReconEntrySnapshots()
			{
				foreach (var snapshotToCopy in originalCusReconEntry.CusReconSnapshots)
				{
					yield return snapshotToCopy.Clone(new BusinessObjectCloneArgs(new string[] { CusReconSnapshot.Schema.CRS_CRE_Entry })) as CusReconSnapshot;
				}
			}
		}

		public static CusReconEntry GetUnlinkedCusReconEntryWithTheSameOriginalEntryNumber(this CusReconEntry originalReconEntry)
		{
			CusReconEntry result = null;
			var originalEntryNumber = originalReconEntry?.CRE_OriginalEntryNumber ?? ZString.Empty;
			if (!originalEntryNumber.IsEmpty)
			{
				var query = new ZQuery(CusReconEntrySchema.CRE_OriginalEntryNumber, originalEntryNumber);
				query.AddToFilter(CusReconEntrySchema.CRE_GB_Branch, originalReconEntry.CRE_GB_Branch);
				query.AddToFilter(CusReconEntrySchema.CRE_CRD, null);
				result = originalReconEntry.Factory.Load<CusReconEntry>(query).SingleOrDefault();
			}
			return result;
		}

		public static void CreateOrUpdateNote(this EnterpriseBusinessObject businessObject, ZString noteDescription, ZString value)
		{
			if (!value.IsEmpty)
			{
				businessObject.CreateOrUpdateNote(noteDescription, new ZString[] { value }, ZString.Empty);
			}
		}

		public static void CreateOrUpdateNote(this EnterpriseBusinessObject businessObject, ZString noteDescription, ZString[] values, ZString seperator)
		{
			if (businessObject != null && !noteDescription.IsEmpty && values.Length > 0)
			{
				var stmNote = businessObject.Notes.FindByDescription(noteDescription).SingleOrDefault();
				if (stmNote == null)
				{
					stmNote = businessObject.Notes.AddNew();
					stmNote.ST_Description = noteDescription;
				}
				stmNote.ST_NoteDataAsText = ZString.Join(seperator, values);
			}
		}

		public static string GetNote(this EnterpriseBusinessObject businessObject, ZString noteDescription)
			=> businessObject?.Notes.FindByDescription(noteDescription).SingleOrDefault()?.ST_NoteText;

		public static string GetCusAuthorizationUsageNumber(this CusEntryInstruction entryInstruction, ZString usageCode)
			=> entryInstruction?.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().FirstOrDefault(x => x.AGC_Code == usageCode)?.AGC_Number;

		public static void AddCustomsEntryStatusLog(this CusEntryHeader entryHeader, ZString reference)
		{
			entryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, reference, ZDateTimeOffset.Now);
		}

		public static OrgAddress GetOrgAddressFromOrgHeaderCodeAndAddressCode(this BusinessObjectFactory factory, ZString orgHeaderCode, ZString addressShortCode)
		{
			OrgAddress result = null;
			if (!orgHeaderCode.IsEmpty)
			{
				var organisation = factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgHeaderCode);
				result = organisation?.Addresses.Cast<OrgAddress>().SingleOrDefault(x => x.OA_Code == addressShortCode);
			}
			return result;
		}
	}
}
