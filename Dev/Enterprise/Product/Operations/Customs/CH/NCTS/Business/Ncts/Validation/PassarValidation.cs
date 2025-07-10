using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using FuncsHelper = Enterprise.Customs.CH.Business.FuncsHelper;

namespace Enterprise.Customs.CH.NCTS.Business;

public class PassarValidation : CH.Business.PassarValidation
{
	public static void CheckNS30034(ZPropertyInfo propertyInfo, NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		if (sendingObject.IsNT141 && !sendingObject.ReasonText.IsEmpty && sendingObject.ActualDestinationCustomsOffice.IsEmpty && sendingObject.ActualConsignee.IsEmpty)
		{
			propertyInfo.AddError(PassarValidationMessages.MessageNS30034);
		}
	}

	public static void CheckNS30035(ZPropertyInfo propertyInfo, NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		if (sendingObject.IsNT141 && !sendingObject.TC11DeliveryDate.IsEmpty && propertyInfo.Value.IsEmpty)
		{
			propertyInfo.AddError(PassarValidationMessages.MessageNS30035(propertyInfo.HumanReadableName));
		}
	}

	public static void CheckNS30068(ZPropertyInfo propertyInfo, NctsDepartureMovementHeader movement)
	{
		var bills = movement.Header.Bills;
		if (bills.Count == 0 && movement.RelatedExportEntryHeaders.Count == 0 && !movement.Header.IsLinkedExport)
		{
			propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30068);
		}
	}

	public static void CheckNS30092(ZPropertyInfo propertyInfo, NctsDepartureCargoDesc cargoDesc)
	{
		if (!cargoDesc.BY_HarmonisedTariff.IsEmpty && cargoDesc.BY_NetWeight.IsEmpty)
		{
			var quantityCode = cargoDesc.UniversalTariff?.GetAttribute(CH.Business.UniversalReferenceConstants.TariffAttributes.NetMassOptional)?.ZZ3_Value ?? ZString.Empty;
			if (quantityCode == CH.Business.UniversalReferenceConstants.TariffAttributes.Values.No)
			{
				propertyInfo.AddMessageError(PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30092, propertyInfo.HumanReadableName));
			}
		}
	}

	public static void CheckNS30122(ZPropertyInfo propertyInfo, NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		if (sendingObject.IsNT141 && sendingObject.ReasonCode == CH.Business.UniversalReferenceConstants.PassarReasonCodes.Duplication && sendingObject.DoubleEntryMRN.IsEmpty)
		{
			propertyInfo.AddError(PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30122, propertyInfo.HumanReadableName));
		}
	}

	public static void CheckNS30093(ZPropertyInfo propertyInfo, NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		if ((sendingObject.MessageType == PassarMessageTypeList.Codes.NT013 || sendingObject.MessageType == PassarMessageTypeList.Codes.NT513) && sendingObject.ReasonCode == CH.Business.UniversalReferenceConstants.PassarReasonCodes.Others && sendingObject.ReasonText.IsEmpty)
		{
			propertyInfo.AddError(PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30093, propertyInfo.HumanReadableName));
		}
	}

	public static void CheckNS30094(ZPropertyInfo propertyInfo, NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		if (sendingObject.IsNT014 && sendingObject.ReasonCode == CH.Business.UniversalReferenceConstants.PassarReasonCodes.Others && sendingObject.ReasonText.IsEmpty)
		{
			propertyInfo.AddError(PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30094, propertyInfo.HumanReadableName));
		}
	}

	public static void CheckNS30162(ZPropertyInfo propertyInfo, NctsDepartureCargoDesc cargoDesc)
	{
		if (!FuncsHelper.IsCHNT015V4Active
			&& cargoDesc.BY_RN_NKCountryOfDispatch.IsEmpty
			&& cargoDesc.Bill.B0_RN_NKCountryOfExport.IsEmpty
			&& cargoDesc.MoveHeader.BM_RN_NKCountryOfDispatch.IsEmpty)
		{
			propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30162);
		}
	}

	public static void CheckNS30163(ZPropertyInfo propertyInfo, NctsDepartureMovementHeader movement)
	{
		if (FuncsHelper.IsCHNT515V4Active)
		{
			CheckHouseConsignmentCardinality(propertyInfo, movement, PassarValidationMessages.NS30163, SwissCustomsConstants.Limits.HouseConsignmentCardinalityLimitV4);
		}
	}

	public static void CheckNP70176(ZPropertyInfo propertyInfo, NctsDepartureMovementHeader movement)
	{
		if (!FuncsHelper.IsCHNT515V4Active)
		{
			CheckHouseConsignmentCardinality(propertyInfo, movement, PassarValidationMessages.NP70176, SwissCustomsConstants.Limits.HouseConsignmentCardinalityLimitV5);
		}
	}

	public static void CheckNP70025(ZPropertyInfo propertyInfo, NctsArrivalMovementHeader arrivalMovementHeader)
	{
		if (arrivalMovementHeader.BM_UnloadingDate.IsInTheFuture())
		{
			propertyInfo.AddMessageError(PassarValidationMessages.MessageNP70025);
		}
	}

	public static void CheckNP70041(ZPropertyInfo propertyInfo, NctsEuOfficeCode officeCode)
	{
		var currentOfficeCountryCode = officeCode.OfficeCountryCode;
		var movementHeader = officeCode.MovementHeader;
		if (!movementHeader.IsNationalTransitSwitzerland
			&& !currentOfficeCountryCode.IsEmpty
			&& officeCode.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination)
		{
			if (officeCode.IsInCL112CountryList && !movementHeader.HasTransitOffice(currentOfficeCountryCode))
			{
				propertyInfo.AddMessageError(PassarValidationMessages.MessageNP70041);
			}
		}
	}

	public static void CheckNP70231(ZPropertyInfo propertyInfo, NctsEuOfficeCode officeCode)
	{
		var movementHeader = officeCode.MovementHeader;
		if (!movementHeader.IsNationalTransitSwitzerland
			&& !officeCode.OfficeCountryCode.IsEmpty
			&& officeCode.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination)
		{
			if (officeCode.IsInCL010CountryList
				&& !movementHeader.TransitCustomsOfficeCodeList
				.Cast<NctsEuOfficeCode>()
				.Any(x => x.IsInCL010CountryList))
			{
				propertyInfo.AddMessageError(PassarValidationMessages.MessageNP70231);
			}
		}
	}

	public static void CheckNP70123(ZPropertyInfo propertyInfo, NctsDepartureMovementHeader movement)
	{
		if (movement.IsNationalTransitSwitzerland && movement.BM_ExportTimeLimit > 10)
		{
			propertyInfo.AddMessageError(PassarValidationMessages.MessageNP70123);
		}
	}

	public static void CheckNP70205(ZPropertyInfo propertyInfo, NctsCommonCargoDesc cargoDesc)
	{
		if (!cargoDesc.BY_HarmonisedTariff.IsEmpty && NctsLookupsHelper.LoadTariffForTransit(cargoDesc.Factory, cargoDesc.BY_HarmonisedTariff, cargoDesc.ValuationDate) == null)
		{
			propertyInfo.AddMessageError(PassarValidationMessages.MessageNP70205);
		}
	}

	public static void CheckNP70237(ZPropertyInfo propertyInfo, NctsArrivalCargoDesc cargoDesc)
	{
		if (cargoDesc.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF && !cargoDesc.IsDIFWithDifferencesIncludingPackages)
		{
			if (cargoDesc.IsAllPackageDEC)
			{
				cargoDesc.BY_UnloadedStateInfo.AddMessageError(PassarValidationMessages.MessageNP70237);
			}
			else if (cargoDesc.IsAnyPackageDIF)
			{
				cargoDesc.BY_UnloadedStateInfo.AddMessageError(PassarValidationMessages.MessageNP70237_WithPackages);
			}
		}
	}

	public static void CheckNP70254(NctsBill bill)
	{
		if (bill.IsPhase5Departure)
		{
			if (bill.HasRequiredPreviousDocument)
			{
				bill.RemoveRowMessageError(PassarValidationMessages.MessageNP70254);
			}
			else
			{
				bill.AddRowMessageError(PassarValidationMessages.MessageNP70254);
			}
		}
	}

	public static void CheckNP70278(ZPropertyInfo propertyInfo, NctsDepartureMovementHeader movement)
	{
		if (!CH.Business.FuncsHelper.IsCHNT015V4Active && movement.Header.GoodsItemsCount + movement.RelatedExportMergedLinesCount > SwissCustomsConstants.Limits.ConsignmentItemsCardinalityLimit)
		{
			propertyInfo.AddMessageError(PassarValidationMessages.MessageNP70278);
		}
	}

	public static void CheckNP70180(ZPropertyInfo propertyInfo, NctsHeader nctsHeader)
	{
		var movementHeader = nctsHeader.ArrivalMovementHeader;
		if (collectionsAreEmpty() && movementHeader.BM_CustomsStatus.IsEmpty)
		{
			propertyInfo.AddMessageError(PassarValidationMessages.MessageNP70180);
		}
		bool collectionsAreEmpty() => movementHeader.MovementReferenceNumbers.Count == 0 && movementHeader.SupernumeraryGoods.Count == 0 && movementHeader.AdditionalTransitOperations.Count == 0;
	}

	public static void CheckNS30018(ZPropertyInfo propertyInfo, NctsBill bill)
	{
		var header = bill.Header;
		if (!CH.Business.FuncsHelper.IsCHNT015V4Active && header.IsDepartureMovement && header.MovementHeader.BM_InBondEntryType != NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland)
		{
			var isHeaderConsigneeEmpty = header.Consignee.IsEmpty;
			var isBillConsigneeEmpty = bill.Consignee.IsEmpty;

			if (!isHeaderConsigneeEmpty && !isBillConsigneeEmpty)
			{
				propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30018_1);
			}
			else if (isHeaderConsigneeEmpty && isBillConsigneeEmpty)
			{
				var ctcCountries = UniversalLookupsHelper.GetCountryCodesCTC(bill.Factory);

				if (ctcCountries.ContainsCode(header.MovementHeader.BM_RL_NKDestinationPort))
				{
					propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30018_2);
				}
				else if (ctcCountries.ContainsCode(bill.B0_RN_NKCountryOfDestination))
				{
					propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30018_3);
				}
				else if (bill.GoodsItems.Any(y => ctcCountries.ContainsCode(y.BY_RN_NKCountryOfDestination)))
				{
					propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30018_3);
				}
			}
		}
	}
	
	public static void CheckNS30132(ZPropertyInfo propertyInfo, NctsBill bill)
	{
		var header = bill.Header;
		if (header.IsDepartureMovement && header.MovementHeader.BM_InBondEntryType == NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland)
		{
			var isHeaderConsigneeEmnpty = header.Consignee.IsEmpty;
			var isBillConsigneeEmnpty = bill.Consignee.IsEmpty;
			if (!isHeaderConsigneeEmnpty && !isBillConsigneeEmnpty)
			{
				propertyInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.C0001_3aMessage);
			}
			else if (isHeaderConsigneeEmnpty && isBillConsigneeEmnpty)
			{
				propertyInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.C0001_3bMessage);
			}
		}
	}

	public static void CheckNZ50023(ZPropertyInfo propertyInfo, MovementReferenceNumberSupportingInfo movementReferenceNumber)
	{
		var parentCollection = movementReferenceNumber.Parent.MovementReferenceNumbers.Cast<MovementReferenceNumberSupportingInfo>();

		if (parentCollection.Any(m => m != movementReferenceNumber && m.CSI_ReferenceNumber == movementReferenceNumber.CSI_ReferenceNumber))
		{
			propertyInfo.AddError(PassarValidationMessages.NZ50023());
		}
	}

	public static void CheckNZ50023(NctsArrivalMovementHeader arrivalMovementHeader)
	{
		var factory = arrivalMovementHeader.Factory;
		var referenceNumbers = arrivalMovementHeader.MovementReferenceNumbers.Select(m => m.CSI_ReferenceNumber).Where(x => !x.IsEmpty).ToArray();

		var duplicateMrnsQuery = MovementReferenceNumberSupportingInfo.Loader.GetLoadMovementReferenceNumbersQuery(referenceNumbers);
		duplicateMrnsQuery.AddToFilter(CusSupportingInfoSchema.PK, SQLComparisonOperator.NotEqual, arrivalMovementHeader.MovementReferenceNumbers.Select(x => x.PK));
		var duplicateMrns = factory.Load<MovementReferenceNumberSupportingInfo>(duplicateMrnsQuery);

		var receivedJobs = new NctsHeader.Loader(factory).FindByMovementReferenceNumbers(NctsMovementType.Codes.Arrival,
			referenceNumbers.Except(duplicateMrns.Select(x => x.CSI_ReferenceNumber)).ToArray());

		arrivalMovementHeader.MovementReferenceNumbers.Cast<MovementReferenceNumberSupportingInfo>().ForEach(mrn =>
		{
			mrn.RemoveRowError(nameof(PassarValidationMessages.NZ50023), true);

			if (duplicateMrns.FirstOrDefault(x => x.CSI_ReferenceNumber == mrn.CSI_ReferenceNumber) is MovementReferenceNumberSupportingInfo duplicateMRN)
			{
				mrn.AddRowError(PassarValidationMessages.NZ50023(duplicateMRN.Parent.Header.BH_JobReference));
			}
			else if (receivedJobs.FirstOrDefault(x => x.MovementReferenceNumber == mrn.CSI_ReferenceNumber) is NctsHeader nctsHeader)
			{
				mrn.AddRowError(PassarValidationMessages.NZ50023(nctsHeader.BH_JobReference));
			}
		});
	}

	public static void CheckNS30128(ZPropertyInfo propertyInfo, JobDocAddress principal)
	{
		var organization = principal.Organisation;
		if (!principal.E2_AddressOverride && organization != null)
		{
			var country = principal.E2_RN_NKCountryCode;
			switch (country)
			{
				case CountryCodes.Switzerland:
					if (organization.GetIdentificationNumberForCH().IsEmpty)
					{
						propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30128_CH);
					}
					break;
				case CountryCodes.Norway:
					if (organization.GetCustomsRegNo(OrgCusCode.CodeTypes.OrganizationNumber, country).IsEmpty)
					{
						propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30128_NO);
					}
					break;
				case CountryCodes.Turkey:
					if (organization.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, country).IsEmpty)
					{
						propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30128_TR);
					}
					break;
				default:
					if ((country == CountryCodes.UnitedKingdom || organization.Factory.IsMemberOfEU(country))
						&& organization.GetIdentificationNumberForEU().IsEmpty)
					{
						propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30128_EUGB);
					}
					break;
			}
		}
	}

	static void CheckHouseConsignmentCardinality(ZPropertyInfo propertyInfo, NctsDepartureMovementHeader movement, string ruleCode, int cardinality)
	{
		if (movement.Header.IsNationalTransitSwitzerland
			&& movement.Header.Bills.Count + movement.Header.PreviousDocumentsEXPOCount + movement.RelatedExportEntryHeaders.Count > cardinality)
		{
			propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30163NP70176(ruleCode, cardinality));
		}
	}

	public static void CheckNS30046NP70279(ZPropertyInfo propertyInfo, NctsDepartureCargoDesc cargoDesc, bool isBY_CommercialReferenceNumberMandatory)
	{
		var header = (NctsHeader)cargoDesc.Header;
		var isInPhase5TransitionPeriod = header?.IsInPhase5TransitionPeriod ?? false;
		if (!isInPhase5TransitionPeriod && isBY_CommercialReferenceNumberMandatory && cargoDesc.BY_CommercialReferenceNumber.IsEmpty)
		{
			if (header.MovementHeader.RelatedExportEntryHeaders.Count > 0 || header.IsLinkedExport)
			{
				propertyInfo.AddWarning(PassarValidationMessages.MessageNP70279);
			}
			else
			{
				propertyInfo.AddMessageError(PassarValidationMessages.MessageNS30046);
			}
		}
	}

	public static void CheckNZ50016(ZPropertyInfo propertyInfo, NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		if (sendingObject.IsNT014
			&& sendingObject.MovementHeader.BM_CustomsStatus != NCTS5DepartureCustomsStatusList.Codes.MrnAllocated
			&& sendingObject.MovementHeader.BM_Phase == DeparturePhaseList.Codes.Activation
			&& sendingObject.ReasonCode.IsEmpty)
		{
			MandatoryValidation.CheckEntered(propertyInfo, errorNotificationPrefix: PassarValidationMessages.NZ50016.GetRuleCodeMessagePrefix(true));
		}
	}
}
