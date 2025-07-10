using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsEuOfficeCodeRuleG0034Validation
	{
		public NctsEuOfficeCodeRuleG0034Validation(NctsEuOfficeCode customsOffice)
		{
			this.customsOffice = Argument.NotNull(customsOffice, nameof(customsOffice));
			var header = customsOffice.EffectiveHeader;
			nctsHeader = Argument.NotNull(header, nameof(header));
			factory = customsOffice.Factory;
		}

		internal void ValidateRuleG0034()
		{
			if (!(customsOffice.ValidationDecider is INctsEuOfficeCodeDeparturePhase5ValidationDecider departurePhase5ValidationDecider && departurePhase5ValidationDecider.IsRuleG0034Active)
				|| customsOffice.CY_Data.IsEmpty
				|| customsOffice.CY_Code != OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination)
			{
				return;
			}

			if (!IsCustomsOfficeAppropriate())
			{
				customsOffice.CY_DataInfo.AddMessageError(Res.GetString("AC851040-3F3C-45BB-9170-245DF3743C09", "[G0034] Office of Destination is not appropriate"));
			}
		}

		bool IsCustomsOfficeAppropriate()
		{
			return !IsT2DeclarationOrN830GoodsItemPrevDocInTransitionPeriodOrN830HouseConsignmentPrevDocNotInTransitionPeriod()
				|| IsCountryCodeInCL112OrInCL010AndCL172AndCL294();
		}

		bool IsT2DeclarationOrN830GoodsItemPrevDocInTransitionPeriodOrN830HouseConsignmentPrevDocNotInTransitionPeriod()
		{
			var isInTransitionPeriod = nctsHeader.IsInPhase5TransitionPeriod;

			return nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader &&
				(movementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.T2
				|| (isInTransitionPeriod && HasAnyN830InPreviousDocumentFromGoodsItem())
				|| (!isInTransitionPeriod && HasAnyN830InCommonPreviousDocumentFromHouseConsignment())
				);
		}

		bool IsCountryCodeInCL112OrInCL010AndCL172AndCL294()
		{
			var customsOfficeCountryCode = customsOffice.OfficeCountryCode;
			var customsOfficeCode = customsOffice.CY_Data;

			return customsOffice.IsInCL112CountryList ||
				(customsOffice.IsInCL010CountryList
				&& factory.GetCL172CustomsOfficeDestinationCodes().ContainsCode(customsOfficeCode)
				&& factory.GetCL294CustomsOfficeExitCodes().ContainsCode(customsOfficeCode));
		}

		bool HasAnyN830InPreviousDocumentFromGoodsItem()
		{
			return nctsHeader.Bills
				.Any(b => b.GoodsItems.Cast<NctsDepartureCargoDesc>()
					.Any(gi => HasN830(gi.PreviousDocuments)));
		}

		bool HasAnyN830InCommonPreviousDocumentFromHouseConsignment()
		{
			return nctsHeader.Bills
				.Any(b => HasN830(b.PreviousDocuments));
		}

		bool HasN830(IEnumerable<PreviousDocument> docs) => docs.Any(d => d.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830);

		readonly NctsEuOfficeCode customsOffice;
		readonly NctsHeader nctsHeader;
		readonly BusinessObjectFactory factory;
	}
}
