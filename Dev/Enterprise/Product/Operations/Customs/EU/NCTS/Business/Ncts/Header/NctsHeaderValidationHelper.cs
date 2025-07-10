using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class NctsHeaderValidationHelper
	{
		#region Mandatory Data

		public static void CheckMandatoryArrivalDestinationTrader(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsArrivalMovementAllowed)
			{
				var docAddress = nctsHeader.DestinationTrader;
				if (nctsHeader.IsPhase5 && (docAddress.IsEmpty || !IsTraderEoriValid(docAddress)))
				{
					if (nctsHeader.Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration
						&& configuration.IsRuleTR0074Active)
					{
						info.AddMessageError(configuration.Messages.TR0074Message);
					}
				}
				else
				{
					if (docAddress.IsEmpty || (!AllAddressFieldsCompleted(docAddress) && !IsTraderEoriValid(docAddress)))
					{
						info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C060, Res.GetString("9B9B379A-EF06-4594-93B2-1B0B47F96593", "Please enter a Destination trader with an EORI or enter full address details. N.B. the EORI is mandatory for native NCTS messaging. ")));
					}
				}
			}
		}

		public static void CheckMandatoryArrivalOffice(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsArrivalMovementAllowed)
			{
				var destinationCustomsOfficeCodeForArrival = nctsHeader.IsPhase5 ? nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival : nctsHeader.DestinationCustomsOfficeCodeForArrival;
				if (destinationCustomsOfficeCodeForArrival.IsEmpty)
				{
					if (nctsHeader.Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration
						&& configuration.IsRuleTR0075Active)
					{
						info.AddMessageError(configuration.Messages.TR0075Message);
					}
				}
			}
		}

		#endregion Mandatory Data

		#region Conditions

		public static void CheckConditionC001(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var countryOfDestination = nctsHeader.MovementHeader?.BM_RL_NKDestinationPort ?? ZString.Empty;
			if (nctsHeader.IsDepartureMovement
				&& !countryOfDestination.IsEmpty
				&& nctsHeader.Consignee.IsEmpty
				&& (IsNctsContractingParty(countryOfDestination)
						|| countryOfDestination == Core.Constants.CountryCodes.Andorra
						|| countryOfDestination == Core.Constants.CountryCodes.SanMarino))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C001, Res.GetString("06B06921-698A-445A-94E5-98916008DA64", "Consignee Trader is required for goods destined for NCTS contracting parties.")));
			}
		}

		static void CheckConditionC030(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement
				&& IsMovementBetweenContractingPartiesThatAreDifferent(nctsHeader)
				&& (IsCountryAndorraOrSanMarino(nctsHeader.DepartureCustomsOfficeCodeCountry) || IsCountryAndorraOrSanMarino(nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture))
				&& !nctsHeader.HasTransitOffice()
				&& !nctsHeader.IsPhase5)
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C030, Res.GetString("7E2CCFC6-DD15-4264-A2E3-17E9D4D48BC0", "Office of Transit is required for this movement.")));
			}
		}

		static bool IsMovementBetweenContractingPartiesThatAreDifferent(NctsHeader nctsHeader)
		{
			var hasDifferentContractingParties = false;
			if (!nctsHeader.DepartureCustomsOfficeCodeCountry.IsEmpty && !nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture.IsEmpty)
			{
				if (IsNctsContractingParty(nctsHeader.DepartureCustomsOfficeCodeCountry) && IsNctsContractingParty(nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture)
					&& nctsHeader.DepartureCustomsOfficeCodeCountry != nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture)
				{
					hasDifferentContractingParties = true;
				}
			}
			return hasDifferentContractingParties;
		}

		public static bool IsInternalCommunityTransitProcedureType(ZString entryType)
		{
			switch (entryType)
			{
				case NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure:
				case NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories:
					return true;
				default:
					return false;
			}
		}

		public static void CheckConditionC050(this NctsHeader nctsHeader, ZPropertyInfo info, JobDocAddress docAddress)
		{
			if (nctsHeader.IsDepartureMovement
				&& (!nctsHeader.IsPhase5 || (nctsHeader.ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider validationdecider && validationdecider.IsRuleC0050Active))
				&& !AllAddressFieldsCompleted(docAddress)
				&& !(IsTraderEoriValid(docAddress) || (IsTirDeclaration(nctsHeader) && IsTraderTirValid(nctsHeader))))
			{
				info.AddMessageError(Res.GetString("C14F41D8-3CA0-4AE6-A998-878DDC58D94C", "[{0}] Please enter a Principal trader with an {1} or enter full address details.", Rules_C_Conditions.Codes.C050, GetApplicableCustomCodesForMessage(nctsHeader)));
			}
		}

		public static void CheckNoOrMultipleGuarantees(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.Configuration.UseGuaranteeGridValidation)
			{
				if (nctsHeader.GuaranteeRefresher.totalGuarantees.Count == 0)
				{
					info.AddMessageError(Res.GetString("9636BA95-F53F-4B13-9DF8-189799E5AB25", "There is no guarantee found for Principal, Declarant or Organization Proxy. Please select a guarantee."));
				}
				else if (nctsHeader.GuaranteeRefresher.totalGuarantees.Count > 1)
				{
					info.AddMessageError(Res.GetString("1C1D2E16-CAD7-4EDA-960E-6A4CD9D62EB0", "There are more than 1 guarantee found for Principal, Declarant or Organization Proxy, they are {0}, {1}... Please select a guarantee.", nctsHeader.GuaranteeRefresher.totalGuarantees[0], nctsHeader.GuaranteeRefresher.totalGuarantees[1]));
				}
			}
		}

		public static void CheckConditionTR0087(this NctsHeader nctsHeader, JobDocAddress docAddress)
		{
			if (nctsHeader.IsPhase5Departure
				&& nctsHeader.ValidationDecider is IRuleTR0087Decider decider && decider.IsActive
				&& docAddress.IsEmpty)
			{
				docAddress.OrganisationPKInfo.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0087Message);
			}
		}

		static string GetApplicableCustomCodesForMessage(NctsHeader nctsHeader)
		{
			const string eori = "EORI";
			var customCodes = IsTirDeclaration(nctsHeader)
				? new[] { eori, OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers }
				: new[] { eori };
			return string.Join(Res.GetString("E17CCD65-A4B4-4442-9CE5-734B1F47A2EA", " or "), customCodes);
		}

		public static void CheckConditionC0505(this NctsHeader nctsHeader, ZPropertyInfo info, JobDocAddress docAddress)
		{
			if (nctsHeader != null
				&& nctsHeader.IsPhase5Departure
				&& nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleC0505Active
				&& docAddress.E2_Postcode.IsEmpty
				&& (docAddress.Country?.RN_PostcodeValidationRule ?? ZString.Empty) == CountryAddressValidationRuleList.Codes.MustBeEntered)
			{
				info.AddMessageError(Res.GetString("D68533B0-48D8-4B01-939D-F106448D48F3", "[C0505] You have not entered a Post Code."));
			}
		}

		static bool AllAddressFieldsCompleted(JobDocAddress docAddress)
		{
			return docAddress != null && docAddress.Address != null && docAddress.Address.Header != null
				&& !docAddress.Address.Header.OH_FullNameTruncated.IsEmpty
				&& !docAddress.E2_Address1.IsEmpty
				&& !docAddress.E2_Postcode.IsEmpty
				&& !docAddress.E2_City.IsEmpty
				&& !docAddress.Address.Header.CountryCode.IsEmpty;
		}

		public static bool IsTraderEoriValid(JobDocAddress traderJobDocAddress)
		{
			var eoriCode = traderJobDocAddress?.Address?.GetEuIdentificationNumber() ?? ZString.Empty;
			return !eoriCode.IsEmpty;
		}

		public static void CheckConditionC111(this NctsHeader nctsHeader, ZPropertyInfo info, JobDocAddress traderJobDocAddress)
		{
			var isSimplifiedNctsProcedure = nctsHeader.MovementHeader.IsSimplifiedNctsProcedure;
			if (nctsHeader.IsDepartureMovement
				&& (isSimplifiedNctsProcedure || IsNormalProcedureInEUWithAEO(nctsHeader, isSimplifiedNctsProcedure, nctsHeader.DepartureCustomsOfficeCodeCountry))
				&& traderJobDocAddress != null && !IsTraderEoriValid(traderJobDocAddress))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C111, Res.GetString("B1ECC3DA-F584-42EF-A675-8041C2804754", "EORI is required for the Principal trader.")));
			}
		}

		public static void CheckConditionC112(this NctsHeader nctsHeader, ZPropertyInfo info, JobDocAddress traderJobDocAddress, string arrivalMsgInfo)
		{
			var simplifiedArrivalNctsProcedure = nctsHeader.ArrivalMovementHeader?.IsSimplifiedNctsProcedure ?? ZBool.False;
			if (nctsHeader.IsArrivalMovementAllowed && (simplifiedArrivalNctsProcedure || IsNormalProcedureInEU(simplifiedArrivalNctsProcedure, nctsHeader.DestinationCustomsOfficeCodeCountryForArrival)) && !IsTraderEoriValid(traderJobDocAddress))
			{
				var errorMsg = ZString.Format(Res.GetString("5C45C489-E989-4B49-BF72-CAEC23864DFC", "Please enter a Destination trader with an EORI. Required for a {0} to an EU member state.  N.B. the EORI is mandatory for native NCTS messaging. "), arrivalMsgInfo);
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C112, errorMsg));
			}
		}

		public static void CheckConditionC187(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			nctsHeader.CheckConditionC187(nctsHeader.SecurityConsignor, () => nctsHeader.MovementHeader.GoodsItems.Select(x => x.SecurityConsignor), info);
		}

		public static void CheckConditionC187(this NctsHeader nctsHeader, JobDocAddress consignorHeaderSecurityTrader, Func<IEnumerable<JobDocAddress>> getConsignorGoodsItemSecurityTradersFunc, ZPropertyInfo info)
		{
			if (!nctsHeader.IsPhase5 && nctsHeader.IsDepartureMovement
				&& HasInvalidSecurityTrader(nctsHeader, consignorHeaderSecurityTrader, getConsignorGoodsItemSecurityTradersFunc()))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C187, Res.GetString("D956A70A-7333-4FBA-87F1-40614558B69E", "Security Consignor Trader must be present at the header or detail level.")));
			}
		}

		public static void CheckConditionC188(this NctsHeader nctsHeader, JobDocAddress headerSecurityTrader, Func<IEnumerable<JobDocAddress>> getGoodsItemSecurityTradersFunc, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement
				&& HasInvalidSecurityTrader(nctsHeader, headerSecurityTrader, getGoodsItemSecurityTradersFunc())
				&& !HasSpecialMentionOfType10600(nctsHeader))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C188, Res.GetString("E233B6F3-F976-4FA6-B8F5-30710EDBBD03", "Security Consignee Trader must be present at the header or detail level.")));
			}
		}

		public static void CheckConditionC188(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			nctsHeader.CheckConditionC188(nctsHeader.SecurityConsignee, () => nctsHeader.MovementHeader.GoodsItems.Select(x => x.SecurityConsignee), info);
		}

		static bool HasInvalidSecurityTrader(this NctsHeader nctsHeader, JobDocAddress headerSecurityTrader, IEnumerable<JobDocAddress> goodsItemSecurityTraders)
		{
			var securityTraderIsEmpty = headerSecurityTrader.IsEmpty;
			var goodsItemIsEmpty = !goodsItemSecurityTraders.Any();

			var securityTraderOnAllGoodsItem = goodsItemSecurityTraders.Any(i => i.IsEmpty);
			var securityTraderEmptyOnAllGoodsItem = goodsItemSecurityTraders.Any(i => !i.IsEmpty);
			return nctsHeader.BH_FTZMove && ((securityTraderIsEmpty && (goodsItemIsEmpty || securityTraderOnAllGoodsItem)) || (!securityTraderIsEmpty && securityTraderEmptyOnAllGoodsItem));
		}

		public static bool IsNormalProcedureInEU(bool simplified, string countryCode)
		{
			return !simplified && (!string.IsNullOrEmpty(countryCode) && IsEuMemberState(countryCode));
		}

		public static bool IsNormalProcedureInEUWithAEO(NctsHeader nctsHeader, bool simplified, string countryCode)
		{
			return IsNormalProcedureInEU(simplified, countryCode)
						&& nctsHeader.MovementHeader.BM_BTAIndicator == SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
		}

		static bool HasSpecialMentionOfType10600(NctsHeader nctsHeader)
		{
			return (from NctsDepartureCargoDesc gi in nctsHeader.MovementHeader.GoodsItems
					from SpecialMention sm in gi.SpecialMentions
					where sm.Statement == "10600"
					select sm).Any();
		}

		public static void CheckConditionC236(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement
				&& nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleC0236Active
				&& !(IsTraderEoriValid(nctsHeader.Principal) || (IsTirDeclaration(nctsHeader) && IsTraderTirValid(nctsHeader)))
				&& HasGuaranteeReferenceNumber(nctsHeader))
			{
				info.AddMessageError(Res.GetString("6A38A73D-53F0-4B66-BEC4-0D923CCC526C", "[{0}] Principal {1} is required if guarantees are used.", Rules_C_Conditions.Codes.C236, GetApplicableCustomCodesForMessage(nctsHeader)));
			}
		}

		static bool HasGuaranteeReferenceNumber(NctsHeader nctsHeader)
			=> (from NctsGuarantee g in nctsHeader.GetEffectiveGuarantees() where !g.PW_BondNumber.IsEmpty select g).Any();

		public static void CheckConditionR0520(this NctsHeader nctsHeader, ZPropertyInfo valuePropertyInfo, ZPropertyInfo messageTargetPropertyInfo)
		{
			if (!nctsHeader.IsPhase5Departure || !nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleR0520Active)
			{
				return;
			}

			if (!valuePropertyInfo.OriginalValue.Equals(valuePropertyInfo.Value) && !nctsHeader.MovementHeader.BM_CustomsStatus.IsEmpty)
			{
				messageTargetPropertyInfo.AddMessageError(Res.GetString("C03FFE70-DB5C-464F-BBDF-42B11D1F28B8", "This field may not be amended to a value that is different to what was originally declared to Customs."));
			}
		}

		public static void CheckConditionR0520(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			CheckConditionR0520(nctsHeader, info, info);
		}

		public static void CheckConditionC572(this NctsHeader nctsHeader, ZPropertyInfo info, JobDocAddress docAddress)
		{
			if (nctsHeader.IsDepartureMovement
				&& nctsHeader.BH_FTZMove
				&& nctsHeader.MovementHeader.BM_BTAIndicator == SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators
				&& IsEuMemberState(nctsHeader.DepartureCustomsOfficeCodeCountry)
				&& !IsTraderEoriValid(docAddress)
				&& docAddress.Organisation != null)
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C572, Res.GetString("A631D845-1243-4F4E-A94C-FDDA2AE99B86", "If field 'circumstance' = 'E' and office of presentation is in EU, then Security Consignor must have an EORI code")));
			}
		}

		public static void CheckConditionC587(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (!nctsHeader.IsPhase5
				&& nctsHeader.IsDepartureMovement
				&& nctsHeader.BH_FTZMove
				&& nctsHeader.MovementHeader.BM_BTAIndicator != SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies
				&& nctsHeader.Itinerary.Count == 0)
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C587, Res.GetString("BC5860AF-9DC8-4E80-B995-A1217AD9BB84", "Itinerary is required for a Safety and Security movement.")));
			}
		}

		public static void CheckRepresentative_R0850_1(this NctsHeader nctsHeader)
		{
			if (nctsHeader.IsPhase5Departure && nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleR0850_1Active)
			{
				var representative = nctsHeader.MovementHeader.Representative;
				var organisation = representative.Organisation;
				if (organisation != null && organisation.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).IsEmpty)
				{
					representative.OrganisationPKInfo.AddMessageError(Res.GetString("A0D3B0F8-9F7A-4F8A-803C-FD203DF4A90E",
					"{0} EORI-Number is required but Representative has no EORI-Number captured in Registration Numbers / Codes.", ValidationRuleCodeConstants.R0850_1.GetRuleCodeMessagePrefix()));
				}
			}
		}

		public static bool IsConditionRP16(NctsHeader nctsHeader)
		{
			return nctsHeader?.MovementHeader is NctsDepartureMovementHeader movementHeader
				&& movementHeader.BM_InBondEntryType is ZString declarationType
				&& !declarationType.IsEmpty && !declarationType.EqualsIgnoringCase(NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration)
				&& movementHeader.CusAuthorizationUsages.Any(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit)
				&& nctsHeader.Configuration.ValidationRuleConfiguration.IsRuleRP16Active;
		}

		public static void CheckConditionC901(this NctsDepartureCargoDesc goodsItem)
		{
			var moveHeader = goodsItem.MoveHeader;
			if (moveHeader != null && moveHeader.IsTIRDeclaration && goodsItem.BY_LineNo == 1 && goodsItem.SupportingDocuments.All(x => x.CSI_Code != TirCarnetDocumentCode))
			{
				goodsItem.AddRowMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C901, Res.GetString("2EE1C78F-FC33-4038-9A43-39C0D8933721", "A TIR declaration requires one Supporting Document of type 952 (TIR Carnet) on the first goods item")));
			}
		}

		public static void CheckConditionC902(this NctsHeader nctsHeader, NctsSupportingDocument sd, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement && nctsHeader.MovementHeader.IsTIRDeclaration && sd.CSI_Code == TirCarnetDocumentCode)
			{
				if (!nctsHeader.MovementHeader.TirCarnetNumber.Equals(sd.CSI_ReferenceNumber))
				{
					info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C902, Res.GetString("C188934A-0FA9-4193-90F4-9C74C0AB1631", "Invalid TIR Carnet Number. A TIR declaration requires a Supporting Document of type 952 (TIR Carnet) on the first goods item with the same code declared in the TIR declaration")));
				}
			}
		}

		public static void CheckConditionC904(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (!nctsHeader.IsPhase5 && nctsHeader.IsDepartureMovement
				&& nctsHeader.MovementHeader.IsTIRDeclaration
				&& !IsTraderTirValid(nctsHeader))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.C904, Res.GetString("5D65D799-BED4-4B96-8CAD-F540FE358526", "Principal Trader must have a TIR Carnet reference. Open the Trader for editing, choose 'config', 'registration numbers' and enter a registration code of type 'TIR'")));
			}
		}

		static bool IsTraderTirValid(NctsHeader nctsHeader)
		{
			var tir = ZString.Empty;
			if (nctsHeader.Principal != null && nctsHeader.Principal.Organisation != null)
			{
				tir = nctsHeader.Principal.Organisation.GetRegoCodeOfThisOrg(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers);
			}
			return !tir.IsEmpty;
		}

		static bool IsTirDeclaration(NctsHeader nctsHeader) => nctsHeader?.MovementHeader?.IsTIRDeclaration ?? false;

		public static void CheckConditionR020(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement
				&& nctsHeader.MovementHeader.BM_InBondEntryType == NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure
				&& !IsEuMemberState(nctsHeader.DepartureCustomsOfficeCodeCountry)
				&& !HasT2PreviousDocTypeWithReference(nctsHeader))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.R020, Res.GetString("300FB514-B91F-4353-BAD8-2C1456D919EC", "Previous Document Reference is missing for a T2 declaration issued in a non-EU country.")));
			}
		}

		static bool HasT2PreviousDocTypeWithReference(NctsHeader nctsHeader)
		{
			var allowedPrevDocs = new ZString[] { "T2", "T2L", "T2F", "T2LF", "T2CIM", "T2TIR", "T2ATA" };
			var prevDocReference = nctsHeader.MovementHeader.GoodsItems
				.SelectMany(i => i.PreviousDocuments)
				.Where(p => allowedPrevDocs.Contains(p.CSI_Code) && !p.CSI_ReferenceNumber.IsEmpty)
				.Select(p => p.CSI_ReferenceNumber).FirstOrDefault();

			return !prevDocReference.IsEmpty;
		}

		static void CheckConditionR904(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement
				&& IsCountryAndorraOrSanMarino(nctsHeader.DepartureCustomsOfficeCodeCountry)
				&& !nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture.IsEmpty
				&& !IsCountryRegimeEC(nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.R904, Res.GetString("998C9BD0-4763-4844-8362-54A809993288", "Destination Office must be in the European Union for departures from Andorra or San Marino.")));
			}
		}

		static void CheckConditionR905(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement
				&& IsNonEuCommonTransitCountry(nctsHeader.DepartureCustomsOfficeCodeCountry)
				&& IsCountryAndorraOrSanMarino(nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.R905, Res.GetString("E424242E-E75F-4412-A968-6E28C165F409", "Office of Destination can not be in Andorra or San Marino for departures from offices in non EU Common Transit countries.")));
			}
		}

		static void CheckConditionR906(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement
				&& nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture == Core.Constants.CountryCodes.Andorra
				&& !nctsHeader.HasTransitOffice(Core.Constants.CountryCodes.Andorra))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.R906, Res.GetString("96A0FBA7-8B9C-4037-BADB-891FDFF802A0", "If Office of Destination is Andorra, Office of Transit must be Andorra.")));
			}
		}

		static void CheckConditionR907(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var transitOffice = OfficeOfTransit(nctsHeader);
			if (nctsHeader.IsDepartureMovement
				&& nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture == Core.Constants.CountryCodes.SanMarino
				&& transitOffice != null
				&& !IsCountryRegimeEC(transitOffice.OfficeCode))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.R907, Res.GetString("7E4E50CC-C2B2-4A20-B388-5B61721B128A", "Office of Destination must be a country in the European Union.")));
			}
		}

		static bool IsCountryRegimeEC(string officeCode)
		{
			return officeCode.Length > 1 && IsCommunityTransitCountry(officeCode.Substring(0, 2));
		}

		static void CheckConditionR908(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var transitOffice = OfficeOfTransit(nctsHeader);
			if (nctsHeader.IsDepartureMovement
				&& IsNonEuCommonTransitCountry(nctsHeader.DepartureCustomsOfficeCodeCountry)
				&& transitOffice != null
				&& IsCountryAndorraOrSanMarino(transitOffice.OfficeCode.SubstringSafe(0, 2)))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.R908, Res.GetString("041555A9-D0E4-42B5-9431-C7730FF09493", "Office of Transit cannot Andorra or San Marino.")));
			}
		}

		public static void CheckConditionR909(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement && nctsHeader.MovementHeader.ValidationDecider is INctsDepartureMovementHeaderValidationDecider validationDecider && validationDecider.IsRuleR0909Active)
			{
				var movementHeader = nctsHeader.MovementHeader;
				if (movementHeader.BM_InBondEntryType != NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino)
				{
					CheckConditionR0909_FromItalyToSanMarino(nctsHeader, info);
				}

				if (movementHeader.BM_InBondEntryType != NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5
					&& movementHeader.BM_InBondEntryType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure
					&& movementHeader.BM_InBondEntryType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories)
				{
					CheckConditionR0909_ToSanMarinoFromACountryOtherThanItaly(nctsHeader, info);
				}
			}
		}

		static void CheckConditionR0909_FromItalyToSanMarino(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.IsDepartureMovement
				&& IsFromItalyToSanMarinoOffice(nctsHeader))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.R909, Res.GetString("2F928C6C-2E1C-4950-8026-3A2DEC0FBA50", "Declaration Type is incompatible with Departure or Destination office.")));
			}
		}

		public static void CheckConditionR0909_ToSanMarinoFromACountryOtherThanItaly(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var departureCustomsOfficeCodeCountry = nctsHeader.IsPhase5 ? nctsHeader.MovementHeader.DepartureCustomsOfficeCodeCountry : nctsHeader.DepartureCustomsOfficeCodeCountry;
			if (IsToSanMarinoOfficeButNotFromItaly(nctsHeader)
				&& EUCommunityCountryCodes().Contains(departureCustomsOfficeCodeCountry))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.R909, Res.GetString("2F928C6C-2E1C-4950-8026-3A2DEC0FBA50", "Declaration Type is incompatible with Departure or Destination office.")));
			}

			ICollection<string> EUCommunityCountryCodes() => nctsHeader.Lookups.EUCommunityCountryCodesList.GetAllCodes();
		}

		public static void CheckConditionE1102(this NctsHeader nctsHeader, ZPropertyInfo info, JobDocAddress docAddress, string traderName)
		{
			if ((nctsHeader.Configuration.ValidationRuleConfiguration?.IsRuleE1102_1Active ?? false) && nctsHeader.IsPhase5Departure && nctsHeader.IsInPhase5TransitionPeriod && docAddress?.Postcode.Length > NctsConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.PostCode)
			{
				info.AddWarning(Res.GetString("97979CF6-F0B3-4A64-81FC-3B6AD32F0574", "{0} Address Postcode is longer than 9 characters, it will be truncated in the message", traderName));
			}
		}

		static bool IsFromItalyToSanMarinoOffice(NctsHeader nctsHeader)
		{
			return nctsHeader.DepartureCustomsOfficeCodeCountry == Core.Constants.CountryCodes.Italy
						&& nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture == Core.Constants.CountryCodes.SanMarino;
		}

		static bool IsToSanMarinoOfficeButNotFromItaly(NctsHeader nctsHeader)
		{
			var departureCustomsOfficeCodeCountry = nctsHeader.IsPhase5 ? nctsHeader.MovementHeader.DepartureCustomsOfficeCodeCountry : nctsHeader.DepartureCustomsOfficeCodeCountry;
			var destinationCustomsOfficeCodeCountryForDeparture = nctsHeader.IsPhase5 ? nctsHeader.MovementHeader.DestinationCustomsOfficeCodeCountryForDeparture : nctsHeader.DestinationCustomsOfficeCodeCountryForDeparture;
			return departureCustomsOfficeCodeCountry != Core.Constants.CountryCodes.Italy
						&& destinationCustomsOfficeCodeCountryForDeparture == Core.Constants.CountryCodes.SanMarino;
		}

		public static void CheckConditionR910(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var transitOffice = OfficeOfTransit(nctsHeader);
			if (nctsHeader.IsDepartureMovement
				&& IsCountryAndorraOrSanMarino(nctsHeader.DepartureCustomsOfficeCodeCountry)
				&& transitOffice != null
				&& !IsCountryRegimeEC(transitOffice.OfficeCode))
			{
				info.AddMessageError(GetRuleExplanation(Rules_C_Conditions.Codes.R910, Res.GetString("2D27D0D6-85AD-4551-91D5-FAE580018B0C", "Office of Transit must be a country in the European Union.")));
			}
		}

		public static ICustomsOffice OfficeOfTransit(NctsHeader nctsHeader) => nctsHeader.TransitCustomsOfficeCodeList?.FirstOrDefault();

		public static void CheckDepartureMovementCustomsOffice(this NctsHeader header, ZPropertyInfo targetInfo)
		{
			Argument.NotNull(header, nameof(header));
			Argument.NotNull(targetInfo, nameof(targetInfo));

			header.CheckConditionC030(targetInfo);
			header.CheckConditionR904(targetInfo);
			header.CheckConditionR905(targetInfo);
			header.CheckConditionR906(targetInfo);
			header.CheckConditionR907(targetInfo);
			header.CheckConditionR908(targetInfo);
			header.CheckConditionR910(targetInfo);
			header.SecurityConsignor.Validation.ValidateOrganisationPK();
			header.Principal.Validation.ValidateOrganisationPK();
			header.MovementHeader.Validation.ValidateBM_InBondEntryType();
		}

		public static void CheckDeclarantIsValid(this NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (nctsHeader.Declarant != null && !AllAddressFieldsCompleted(nctsHeader.Declarant) && !IsTraderEoriValid(nctsHeader.Declarant))
			{
				info.AddMessageError(Res.GetString("7AC2E5C3-6F7F-4044-992F-CB092371F8D9", "Please enter a Declarant trader with an EORI or enter full address details."));
			}
		}

		#endregion

		public static ZString GetRuleExplanation(ZString ruleNumber, ZString explanation)
		{
			Argument.NotNullOrEmpty(ruleNumber, nameof(ruleNumber));
			Argument.NotNullOrEmpty(explanation, nameof(explanation));

			return new ZString(explanation + "(" + ruleNumber + ")");
		}

		public static ZDBOnlyQuery GetDuplicateMRNQuery(NctsHeader header)
		{
			var result = new ZDBOnlyQuery(typeof(NctsHeader));
			result.AddToFilter(CusInBondHeaderSchema.PK, SQLComparisonOperator.NotEqual, header.PK);
			result.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, SQLComparisonOperator.NotEqual, NctsMovementType.Codes.Departure);
			var currentBranchesQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK, CusInBondHeaderSchema.BH_GB);
			currentBranchesQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			result.AddSubQuery(currentBranchesQuery, JoinCondition.And);
			var currentMRNQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, CusInBondHeaderSchema.PK);
			currentMRNQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, header.TableName);
			currentMRNQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, header.ArrivalMrnFromUser);
			currentMRNQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			currentMRNQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, header.CountryCode);
			result.AddSubQuery(currentMRNQuery, JoinCondition.And);
			result.IsNoResultQuery = false;
			result.OrderBy = CusInBondHeaderSchema.Constants.BH_SystemCreateTimeUtc;
			return result;
		}

		public static ImmutableHashSet<string> NctsContractingParties => cachedNctsContractingParties.Value;
		static readonly Lazy<ImmutableHashSet<string>> cachedNctsContractingParties =
			new Lazy<ImmutableHashSet<string>>(() => EuMemberCountries.Union(Core.Constants.CountryCodes.EuCommonTransitCountries).Union(new[] { Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes }).ToImmutableHashSet());

		public static ImmutableHashSet<string> EuMemberCountries => cachedEuMemberCountries.Value;
		static readonly Lazy<ImmutableHashSet<string>> cachedEuMemberCountries = new Lazy<ImmutableHashSet<string>>(
			() => ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>()
				.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers()
				.Where(x => x != Core.Constants.CountryCodes.Switzerland).ToImmutableHashSet());

		internal static ImmutableHashSet<string> CommonTransitCountries_List9 => cachedCommonTransitCountries_List9.Value;
		static readonly Lazy<ImmutableHashSet<string>> cachedCommonTransitCountries_List9 =
			new Lazy<ImmutableHashSet<string>>(() =>
			{
				// Details: This codelist is a Business codelist maintained in CS/RD, and can be found by selecting the countries
				// with country regime different from OTH (Other Country) i.e. EC country or Transit country not within the EC.

				var list9 = new List<string>();
				list9.Add(Core.Constants.CountryCodes.Andorra);
				list9.Add(Core.Constants.CountryCodes.Austria);
				list9.Add(Core.Constants.CountryCodes.AlandIslands);
				list9.Add(Core.Constants.CountryCodes.Belgium);
				list9.Add(Core.Constants.CountryCodes.Bulgaria);
				list9.Add(Core.Constants.CountryCodes.Switzerland);
				list9.Add(Core.Constants.CountryCodes.Cyprus);
				list9.Add(Core.Constants.CountryCodes.CzechRepublic);
				list9.Add(Core.Constants.CountryCodes.Germany);
				list9.Add(Core.Constants.CountryCodes.Denmark);
				list9.Add(Core.Constants.CountryCodes.Estonia);
				list9.Add(Core.Constants.CountryCodes.Spain);
				list9.Add(Core.Constants.CountryCodes.Finland);
				list9.Add(Core.Constants.CountryCodes.France);
				list9.Add(Core.Constants.CountryCodes.UnitedKingdom);
				list9.Add(Core.Constants.CountryCodes.FrenchGuyana);
				list9.Add(Core.Constants.CountryCodes.Guadeloupe);
				list9.Add(Core.Constants.CountryCodes.Greece);
				list9.Add(Core.Constants.CountryCodes.Croatia);
				list9.Add(Core.Constants.CountryCodes.Hungary);
				list9.Add(Core.Constants.CountryCodes.Ireland);
				list9.Add(Core.Constants.CountryCodes.Iceland);
				list9.Add(Core.Constants.CountryCodes.Italy);
				list9.Add(Core.Constants.CountryCodes.Liechtenstein);
				list9.Add(Core.Constants.CountryCodes.Lithuania);
				list9.Add(Core.Constants.CountryCodes.Luxembourg);
				list9.Add(Core.Constants.CountryCodes.Latvia);
				list9.Add(Core.Constants.CountryCodes.Macedonia);
				list9.Add(Core.Constants.CountryCodes.Monaco);
				list9.Add(Core.Constants.CountryCodes.Martinique);
				list9.Add(Core.Constants.CountryCodes.Malta);
				list9.Add(Core.Constants.CountryCodes.Netherlands);
				list9.Add(Core.Constants.CountryCodes.Norway);
				list9.Add(Core.Constants.CountryCodes.Poland);
				list9.Add(Core.Constants.CountryCodes.Portugal);
				list9.Add(Core.Constants.CountryCodes.Reunion);
				list9.Add(Core.Constants.CountryCodes.Romania);
				list9.Add(Core.Constants.CountryCodes.Serbia);
				list9.Add(Core.Constants.CountryCodes.Sweden);
				list9.Add(Core.Constants.CountryCodes.Slovenia);
				list9.Add(Core.Constants.CountryCodes.SvalbardAndJanMayen);
				list9.Add(Core.Constants.CountryCodes.Slovakia);
				list9.Add(Core.Constants.CountryCodes.SanMarino);
				list9.Add(Core.Constants.CountryCodes.Turkey);
				return list9.ToImmutableHashSet();
			});

		internal static ImmutableHashSet<string> CommunityTransitCountries_List10 => cachedCommunityTransitCountries_List10.Value;
		static readonly Lazy<ImmutableHashSet<string>> cachedCommunityTransitCountries_List10 =
			new Lazy<ImmutableHashSet<string>>(() =>
			{
				// Details: Member countries of the EU. This codelist is a Business codelist maintained in CS/RD,
				// and can be found by selecting the countries with country regime equal to EEC (European Economic Community).

				var list10 = new List<string>();
				list10.Add(Core.Constants.CountryCodes.Austria);
				list10.Add(Core.Constants.CountryCodes.Belgium);
				list10.Add(Core.Constants.CountryCodes.Bulgaria);
				list10.Add(Core.Constants.CountryCodes.Cyprus);
				list10.Add(Core.Constants.CountryCodes.CzechRepublic);
				list10.Add(Core.Constants.CountryCodes.Germany);
				list10.Add(Core.Constants.CountryCodes.Denmark);
				list10.Add(Core.Constants.CountryCodes.Estonia);
				list10.Add(Core.Constants.CountryCodes.Spain);
				list10.Add(Core.Constants.CountryCodes.Finland);
				list10.Add(Core.Constants.CountryCodes.France);
				list10.Add(Core.Constants.CountryCodes.UnitedKingdom);
				list10.Add(Core.Constants.CountryCodes.FrenchGuyana);
				list10.Add(Core.Constants.CountryCodes.Guadeloupe);
				list10.Add(Core.Constants.CountryCodes.Greece);
				list10.Add(Core.Constants.CountryCodes.Croatia);
				list10.Add(Core.Constants.CountryCodes.Hungary);
				list10.Add(Core.Constants.CountryCodes.Ireland);
				list10.Add(Core.Constants.CountryCodes.Italy);
				list10.Add(Core.Constants.CountryCodes.Lithuania);
				list10.Add(Core.Constants.CountryCodes.Luxembourg);
				list10.Add(Core.Constants.CountryCodes.Latvia);
				list10.Add(Core.Constants.CountryCodes.Monaco);
				list10.Add(Core.Constants.CountryCodes.Martinique);
				list10.Add(Core.Constants.CountryCodes.Malta);
				list10.Add(Core.Constants.CountryCodes.Netherlands);
				list10.Add(Core.Constants.CountryCodes.Poland);
				list10.Add(Core.Constants.CountryCodes.Portugal);
				list10.Add(Core.Constants.CountryCodes.Reunion);
				list10.Add(Core.Constants.CountryCodes.Romania);
				list10.Add(Core.Constants.CountryCodes.Sweden);
				list10.Add(Core.Constants.CountryCodes.Slovenia);
				list10.Add(Core.Constants.CountryCodes.Slovakia);
				return list10.ToImmutableHashSet();
			});
		internal static ImmutableHashSet<string> NonEuCommonTransitCountries_List139 => cachedNonEuCommonTransitCountries_List139.Value;
		static readonly Lazy<ImmutableHashSet<string>> cachedNonEuCommonTransitCountries_List139 =
			new Lazy<ImmutableHashSet<string>>(() =>
			{
				return Core.Constants.CountryCodes.EuCommonTransitCountries
					.Where(x => x != Core.Constants.CountryCodes.SvalbardAndJanMayen
						&& x != Core.Constants.CountryCodes.Andorra
						&& x != Core.Constants.CountryCodes.SanMarino
						&& x != Core.Constants.CountryCodes.UnitedKingdom).ToImmutableHashSet();
			});

		public static bool IsCommonTransitCountry(string country)
		{
			return CommonTransitCountries_List9.Contains(country);
		}

		public static bool IsCommunityTransitCountry(string country)
		{
			return CommunityTransitCountries_List10.Contains(country);
		}
		public static bool IsNonEuCommonTransitCountry(string country)
		{
			return NonEuCommonTransitCountries_List139.Contains(country);
		}

		public static bool IsEuMemberState(string country)
		{
			return EuMemberCountries.Contains(country);
		}

		public static bool IsNctsContractingParty(ZString country)
		{
			return NctsContractingParties.Contains(country);
		}

		public static bool IsCountryAndorraOrSanMarino(ZString country)
		{
			return (country == Core.Constants.CountryCodes.Andorra || country == Core.Constants.CountryCodes.SanMarino);
		}

		public static bool IsRuleActive(this NctsHeader header, Func<ValidationRuleConfiguration, bool> isActive)
		{
			var configuration = header?.Configuration?.ValidationRuleConfiguration;
			return configuration != null && isActive(configuration);
		}

		public static bool IsRuleActive(this NctsCommonMovementHeader movementHeader, Func<ValidationRuleConfiguration, bool> isActive)
		{
			var configuration = movementHeader?.Header?.Configuration?.ValidationRuleConfiguration;
			return configuration != null && isActive(configuration);
		}

		public const string TirCarnetDocumentCode = "952";

		public static void CheckConditionC0904(this NctsHeader header, JobDocAddress principal)
		{
			var isTirDeclaration = header.MovementHeader?.IsTIRDeclaration ?? false;
			var orgHeader = principal?.Organisation;
			if (orgHeader == null || !header.IsPhase5 || !isTirDeclaration || !header.Configuration.ValidationRuleConfiguration.IsRuleC0904Active)
			{
				return;
			}

			var addressCountryCode = principal.E2_RN_NKCountryCode;
			var expectedCusCodeTypes = new ZString[]
			{
				OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori
			};

			var cusCodes = orgHeader.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(addressCountryCode, expectedCusCodeTypes);
			if (cusCodes == ZString.Empty)
			{
				var messageError = Res.GetString("bb70d006-741d-4682-98db-6aa15884c703", "[C0904] TIR Holder Identification Number or EORI Number is required for Principal Organization.");
				principal.OrganisationPKInfo.AddMessageError(messageError);
			}
		}

		internal static string C0909ValidationMessage => Res.GetString("ABEC0CEB-4746-4264-84FC-BF4C4CDBC3DE", "[C0909] You have not entered Country Of Dispatch. It is required either on Declaration or House Consignment or House Consignment Item.");

		public static string TR0021ValidationMessage => Res.GetString("E948D3BC-09BB-41C1-B391-F58C5AEBCA2D", "[TR0021] Actual consignee or actual office of destination must be entered if query information is filled in. ");
	}
}
