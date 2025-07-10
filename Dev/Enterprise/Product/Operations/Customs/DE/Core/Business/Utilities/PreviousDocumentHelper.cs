using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.DE.Business.RegistrationNumberValidationHelper;

namespace Enterprise.Customs.DE.Business
{
	public static class PreviousDocumentHelper
	{
		public static CodeDescriptionPairList GetCachedSubTypeList_ATNEU(this BusinessObjectFactory factory, ZString officeCode)
		{
			return factory.GetCachedValue((NoResString)"ATNEU SubTypeList for " + officeCode, () => // Cache Key
			{
				var isOfficeAIR = false;

				if (!officeCode.IsEmpty)
				{
					var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, officeCode, officeCode.Left(2), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now);
					isOfficeAIR = cusCode?.Attributes?.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_TransportModes.Contains(RefTransportModeList.Codes.AIR, StringComparison.OrdinalIgnoreCase)) ?? false;
				}

				var result = new PreviousDocSubTypeList();
				if (!isOfficeAIR)
				{
					result.RemoveCode(PreviousDocSubTypeList.Codes.AWB);
					result.RemoveCode(PreviousDocSubTypeList.Codes.ULD);
				}
				return result;
			});
		}

		public static bool Requires18Or21CharactersReference(this PreviousDocument doc) =>
			(doc.IsProcedureATNEU && doc.CSI_SubType == PreviousDocSubTypeList.Codes.REG) ||
			(doc.IsImport && doc.Status && (doc.IsProcedureATZL || doc.IsProcedureATAV));

		public static bool IsValidAtlasReferenceForBondedWarehouse(this ZString reference) => reference switch
		{
			{ Length: MRNLength } => Regex.IsMatch(reference, @"^[0-9]{2}DE[A-Z0-9]{5}[CDEHT]M[A-Z0-9]{6}[0-9]$") && MRNAndGRNFormatValidatorHelper.IsMRNDigitValid(reference).isMRNDigitValid,
			{ Length: RegistrationNumberLength } => Regex.IsMatch(reference, @"^AT[CDEHT]71[0-9]{16}$"),
			_ => false,
		};

		public static bool IsValidAtlasReferenceForInwardProcessing(this ZString reference) => reference switch
		{
			{ Length: MRNLength } => Regex.IsMatch(reference, @"^[0-9]{2}DE[A-Z0-9]{5}[CDEP]H[A-Z0-9]{6}[0-9]$") && MRNAndGRNFormatValidatorHelper.IsMRNDigitValid(reference).isMRNDigitValid,
			{ Length: RegistrationNumberLength } => Regex.IsMatch(reference, @"^AT[CDEP](02|41|51|91)[0-9]{16}$"),
			_ => false,
		};

		public static readonly ImmutableHashSet<ZString> PreviousProceduresRequiringReference = ImmutableHashSet.Create<ZString>(
			PreviousProcedureList.Codes._ATA,
			PreviousProcedureList.Codes._ESUMA,
			PreviousProcedureList.Codes._GB,
			PreviousProcedureList.Codes._PUEB,
			PreviousProcedureList.Codes._T1,
			PreviousProcedureList.Codes._T2,
			PreviousProcedureList.Codes._TIR,
			PreviousProcedureList.Codes._VO
		);

		public const int MaximumAllowedItemsForExportInvoiceHeaderAndInvoiceLine = 99;
		public const int MaximumAllowedItemsForExportInvoiceHeaderAndInvoiceLineDuringTransitionPeriod = 9;
	}
}
