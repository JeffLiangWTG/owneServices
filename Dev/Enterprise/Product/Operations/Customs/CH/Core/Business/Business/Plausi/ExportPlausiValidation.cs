using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class ExportPlausiValidation : BaseExportPlausiValidation
{
	readonly List<string> tobaccoRequiredAdditionalInfoCodes = new List<string>()
	{
		AdditionalInformationTypeCodes.A1401,
		AdditionalInformationTypeCodes.ProductMainGroup,
		AdditionalInformationTypeCodes.ProductSubgroup,
		AdditionalInformationTypeCodes.A1404,
		AdditionalInformationTypeCodes.A1405,
		AdditionalInformationTypeCodes.A1406
	};

	public override void CheckNP70000(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (((string)invoiceLine.JobDeclaration?.JE_GoodsDestination) == Core.Constants.CountryCodes.Switzerland && !invoiceLine.JI_NonTradingGoods)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70000);
		}
	}

	public override void CheckNP70001(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Weight < invoiceLine.JI_NetWeight)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70001);
		}
	}

	#region NP70009

	public override void CheckNP70009_Weight(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		CheckNP70009(targetPropertyInfo, invoiceLine, () => invoiceLine.EntryInstruction.TotalWeightInKG > 5000, PassarValidationMessages.MessageNP70009_Weight);
	}

	public override void CheckNP70009_Price(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		CheckNP70009(targetPropertyInfo, invoiceLine, () => invoiceLine.EntryInstruction.TotalPriceInCHF > 5000, PassarValidationMessages.MessageNP70009_Price);
	}

	void CheckNP70009(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine, Func<bool> exceedsMaxValue, string errorMessage)
	{
		if (invoiceLine?.EntryInstruction is CusEntryInstruction entryInstuction && entryInstuction.IsSimplified && entryInstuction.JobDeclaration.IsGoodsDestinationInNCL0147CountryList && exceedsMaxValue())
		{
			targetPropertyInfo.AddMessageError(errorMessage);
		}
	}

	#endregion NP70009

	public override void CheckNP70020(ZPropertyInfo targetPropertyInfo, AutoCusHouseContPackInvoiceLinePivot packInvLinePivot)
	{
		base.CheckNP70020(targetPropertyInfo, packInvLinePivot, PassarValidationMessages.MessageNP70020);
	}

	public override void CheckNP70021(ZPropertyInfo targetPropertyInfo, CusContainer container)
	{
		if (container.CO_ContainerNumber.IsEmpty && container.CO_Seal.IsEmpty && container.CO_SecondSeal.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70021);
		}
	}

	public override void CheckNP70026(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_Weight == 0 && HasAnyNumOfPacksHigherThanZeroAndPackageIsBulk())
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70026);
		}

		bool HasAnyNumOfPacksHigherThanZeroAndPackageIsBulk() => invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(x => x.CHC_NumberOfPacks > 0 || x.Package.IsBulkOnlyPackaging);
	}

	public override void CheckNP70043(ZPropertyInfo targetPropertyInfo, CusContainer cusContainer)
	{
		var cusContainers = cusContainer.Declaration.CusContainers;
		var seal = (ZString)targetPropertyInfo.Value;
		if (!seal.IsEmpty && (cusContainer.CO_Seal == cusContainer.CO_SecondSeal || cusContainers.Any(c => (c.PK != cusContainer.PK && (c.CO_Seal == seal || c.CO_SecondSeal == seal)))))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70043);
		}
	}

	public override void CheckNP70065(ZPropertyInfo targetPropertyInfo, string postCode, string message)
	{
		if (postCode == SwissCustomsConstants.PostCodes.Code7562 || postCode == SwissCustomsConstants.PostCodes.Code7563)
		{
			targetPropertyInfo.AddMessageError(message);
		}
	}

	public override void CheckNP70066(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_GoodsReturned && (invoiceLine.JI_NonTradingGoods || invoiceLine.EntryInstruction.CEI_Procedure != ProcedureCodesPassar.ExportFromFreeCirculation))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70066);
		}
	}

	public override void CheckNP70097(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		var entryInstruction = invoiceLine.EntryInstruction;
		if (entryInstruction != null)
		{
			if (!entryInstruction.CEI_PartialDelivery && !invoiceLine.JI_CustomsThirdUnitQty.IsEmpty && invoiceLine.JI_CustomsThirdQuantity.IsEmpty)
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70097);
			}
		}
	}

	public override void CheckNP70155(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		var invoiceLine = (JobComInvoiceLine)additionalInformation.Parent;
		if (invoiceLine.IsTobaccoRefundType)
		{
			CheckNP70155_A1402ValidCode(targetPropertyInfo, additionalInformation);
			CheckNP70155_A1403ValidCode(targetPropertyInfo, additionalInformation, invoiceLine);
			CheckNP70155_MandatoryDescription(targetPropertyInfo, additionalInformation);
		}
	}

	void CheckNP70155_A1402ValidCode(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		if (additionalInformation.CSI_Code == AdditionalInformationTypeCodes.ProductMainGroup
			&& !additionalInformation.Lookups.DescriptionCodeList.ContainsCode(additionalInformation.CSI_Description))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70155_A1402ValidCode);
		}
	}

	void CheckNP70155_A1403ValidCode(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation, JobComInvoiceLine invoiceLine)
	{
		if (additionalInformation.CSI_Code == AdditionalInformationTypeCodes.ProductSubgroup
			&& invoiceLine.AdditionalInformations.Where(addInfo => addInfo.CSI_Code == AdditionalInformationTypeCodes.ProductMainGroup && !addInfo.CSI_Description.IsEmpty).Any()
			&& !additionalInformation.Lookups.DescriptionCodeList.ContainsCode(additionalInformation.CSI_Description))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70155_A1403ValidCode);
		}
	}

	void CheckNP70155_MandatoryDescription(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		if (tobaccoRequiredAdditionalInfoCodes.Contains(additionalInformation.CSI_Code)
			&& additionalInformation.CSI_Description.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70155_MandatoryDescription);
		}
	}

	public override void CheckNP70162(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if ((invoiceLine.EntryInstruction?.IsProcessingTransaction ?? false)
			&& invoiceLine.IsOrdinaryProcess
			&& !invoiceLine.Restrictions.Cast<Restriction>().Any(x => !x.CSI_ReferenceNumber.IsEmpty && x.CSI_Code == UniversalReferenceConstants.PermitCodes.Other))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70162);
		}
	}

	public override void CheckNP70168(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		if (additionalInformation.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.VocQuantityInKilograms && !decimal.TryParse(additionalInformation.CSI_Description, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70168_MustBeDecimal);
		}
	}

	public override void CheckNP70168(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine jobComInvoiceLine)
	{
		if (jobComInvoiceLine.JI_RefundType == UniversalReferenceConstants.RefundType.Refund && !jobComInvoiceLine.HasAdditionalInformationA1301)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70168_NotEntered);
		}
	}

	public override void CheckNP70163(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		if (inAndOutwardProcessing.CSI_IssuerType != InAndOutwardProcessingBillingTypesPassar.NonCollection && (inAndOutwardProcessing.Parent?.EntryInstruction?.CEI_Procedure ?? ZString.Empty) == ProcedureCodesPassar.OutwardProcessing)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70163);
		}
	}

	public override void CheckR198AndNP70167(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		CheckR198AndNP70167(targetPropertyInfo, invoiceLine, PassarValidationMessages.MessageNP70167);
	}

	public override void CheckNP70165(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		var entryInstruction = inAndOutwardProcessing.Parent.EntryInstruction;
		if (entryInstruction != null && entryInstruction.CEI_Procedure != UniversalReferenceConstants.ProcedureCodesPassar.OutwardProcessing && inAndOutwardProcessing.CSI_Procedure == InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70165);
		}
	}

	public override void CheckNP70169(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_NonTradingGoods && !invoiceLine.InAndOutwardProcessingRepair && (invoiceLine.EntryInstruction?.IsProcessingTransaction ?? false))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70169);
		}
	}

	public override void CheckNP70172(ZPropertyInfo targetPropertyInfo, IDocAddress address, DocAddressType type)
	{
		if (address != null && !IsCountryCHorLIorDE(((OrgAddress)address)?.Country?.Code ?? string.Empty))
		{
			switch (type)
			{
				case DocAddressType.SupplierDocumentaryAddress:
					targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70172_Supplier);
					break;

				case DocAddressType.ConsignorDocumentaryAddress:
					targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70172_Consignor);
					break;
			}
		}
	}
	public override void CheckNP70175(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.IsTobaccoRefundType)
		{
			if (!tobaccoRequiredAdditionalInfoCodes.All(code => invoiceLine.AdditionalInformations.Where(addInfo => addInfo.CSI_Code == code).Any()))
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70175);
			}
		}
	}

	public override void CheckNP70178(ZPropertyInfo targetPropertyInfo, AutoCusHouseContPackInvoiceLinePivot packInvLinePivot)
	{
		var invoiceLine = packInvLinePivot.Factory.Load<JobComInvoiceLine>(packInvLinePivot.CHC_JI);
		if (IsCurrentNumberOfPacksZeroAndAnyOtherNotZero() || IsCurrentNumberOfPacksNotZeroAndAnyOtherZero())
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70178);
		}

		bool IsCurrentNumberOfPacksZeroAndAnyOtherNotZero() => packInvLinePivot.CHC_NumberOfPacks == 0 && invoiceLine.PackagesPivot.Cast<AutoCusHouseContPackInvoiceLinePivot>().Any(x => x.PK != packInvLinePivot.PK && x.CHC_NumberOfPacks != 0);

		bool IsCurrentNumberOfPacksNotZeroAndAnyOtherZero() => packInvLinePivot.CHC_NumberOfPacks != 0 && invoiceLine.PackagesPivot.Cast<AutoCusHouseContPackInvoiceLinePivot>().Any(x => x.PK != packInvLinePivot.PK && x.CHC_NumberOfPacks == 0);
	}

	public override void CheckNP70195(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_RefundType == UniversalReferenceConstants.RefundType.RequestForAlcohol)
		{
			if (!invoiceLine.AdditionalInformations.Cast<AdditionalInformation>().Any(info => info.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.VolAlcohol))
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70195(UniversalReferenceConstants.AdditionalInformationTypeCodes.VolAlcohol));
			}
			if (!invoiceLine.AdditionalInformations.Cast<AdditionalInformation>().Any(info => info.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.LitresAlcohol))
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70195(UniversalReferenceConstants.AdditionalInformationTypeCodes.LitresAlcohol));
			}
		}
	}

	public override void CheckNP70195(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		if (additionalInformation.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.VolAlcohol && !decimal.TryParse(additionalInformation.CSI_Description, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70195_DescriptionA1101);
		}
		else if (additionalInformation.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.LitresAlcohol && !decimal.TryParse(additionalInformation.CSI_Description, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70195_DescriptionA1102);
		}
	}

	public override void CheckNP70197(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		if (additionalInformation.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.AlcoholOnBeerRefundLiters && !decimal.TryParse(additionalInformation.CSI_Description, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70197_MustBeDecimal);
		}
	}

	public override void CheckNP70197(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine jobComInvoiceLine)
	{
		if (jobComInvoiceLine.JI_RefundType == UniversalReferenceConstants.RefundType.RefundOfAlcoholOnBeer && !jobComInvoiceLine.HasAdditionalInformationA1102)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70197_NotEntered);
		}
	}

	public override void CheckNP70199(RestrictionAdditionalInformation restrictionAdditionalInformation)
	{
		base.CheckNP70199(restrictionAdditionalInformation);

		var restriction = restrictionAdditionalInformation.Parent;
		var codeList = restrictionAdditionalInformation.Lookups.CY_CodeList.GetAllCodes().ToList();

		var definedCodes = restriction.AdditionalInformations.Select(x => x.CY_Code);

		if (!codeList.All(x => definedCodes.Contains(x)))
		{
			restrictionAdditionalInformation.AddRowMessageError(restriction.CSI_ReferenceNumber.IsEmpty ? PassarValidationMessages.MessageNP70199_NoPermitNumber(restrictionAdditionalInformation.Lookups.CY_CodeList.CodesAsString, restriction.CSI_Code) : PassarValidationMessages.MessageNP70199_PermitNumber(restrictionAdditionalInformation.Lookups.CY_CodeList.CodesAsString, restriction.CSI_Code));
		}
	}

	public override void CheckNP70212(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		if (additionalInformation.Parent is CusEntryInstruction
			&& additionalInformation.CSI_Code == AdditionalInformationTypeCodes.PartialShipmentNumber
			&& !int.TryParse(additionalInformation.CSI_Description, out _))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP702012_NotValidInteger);
		}
	}

	public override void CheckNP70212(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
		if ((entryInstruction?.CEI_PartialDelivery ?? false) && !entryInstruction.HasAdditionalInformationV1201)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP702012_NoV1201);
		}
	}

	public override void CheckNP70213_V1201(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		if (additionalInformation.Parent is CusEntryInstruction entryInsruction
			&& additionalInformation.CSI_Code == AdditionalInformationTypeCodes.PartialShipmentNumber
			&& int.TryParse(additionalInformation.CSI_Description, out int result) && result > 1
			&& !entryInsruction.AdditionalInformations.Where(addInfo => !addInfo.CSI_Description.IsEmpty && addInfo.CSI_Code == AdditionalInformationTypeCodes.ReferenceOfTheFristPartShipment).Any())
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70213_V1201);
		}
	}

	public override void CheckNP70213_V1202(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		if (additionalInformation.Parent is CusEntryInstruction
			&& additionalInformation.CSI_Code == AdditionalInformationTypeCodes.ReferenceOfTheFristPartShipment
			&& additionalInformation.CSI_Description.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70213_V1202);
		}
	}

	public override void CheckNP70229(ZPropertyInfo targetPropertyInfo, RestrictionAdditionalInformation restrictionAdditionalInformation)
	{
		var data = restrictionAdditionalInformation.CY_Data;
		if (!data.IsEmpty && !restrictionAdditionalInformation.LinkedCodeType.IsEmpty)
		{
			ListValidation.MessageErrorIfInvalidCode(targetPropertyInfo, PassarValidationMessages.MessageErrorInvalidCode(PassarValidationMessages.NP70229));
		}
	}

	public override void CheckNS30001_PermitExceptionReason(ZPropertyInfo targetPropertyInfo, Restriction restriction)
	{
		var referenceNumberIsEmpty = restriction.CSI_ReferenceNumber.IsEmpty;
		var permitExceptionReasonIsEmpty = restriction.CSI_Description.IsEmpty;

		if (!restriction.IsPermitExceptionReasonAllowed && !permitExceptionReasonIsEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30001_NotApplicablePermitExceptionReason);
		}
		else if (restriction.IsPermitExceptionReasonAllowed && restriction.IsPermitNumberAllowed)
		{
			if (referenceNumberIsEmpty && permitExceptionReasonIsEmpty)
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30001_MandatoryPermitNumberOrPermitExceptionReason);
			}
			else if (!referenceNumberIsEmpty && !permitExceptionReasonIsEmpty)
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30001_RequiredJustOneOfPermitNumberAndPermitExceptionReason);
			}
		}
	}

	#region NS30003

	public override void CheckNS30003_NotEmpty(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction, Func<string> humanReadbleNameProvider = null)
	{
		if (IsSimplified(entryInstruction) && !((ZString)targetPropertyInfo.Value).IsEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30003_NotEmpty(humanReadbleNameProvider?.Invoke() ?? targetPropertyInfo.HumanReadableName));
		}
	}

	public override void CheckNS30003_Ticked(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction, Func<string> humanReadbleNameProvider = null)
	{
		if (IsSimplified(entryInstruction) && (ZBool)targetPropertyInfo.Value)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30003_Ticked(humanReadbleNameProvider?.Invoke() ?? targetPropertyInfo.HumanReadableName));
		}
	}

	public override void CheckNS30003_GreaterThanZero(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction, Func<string> humanReadbleNameProvider = null)
	{
		if (IsSimplified(entryInstruction) && (ZDecimal)targetPropertyInfo.Value > 0)
		{
			targetPropertyInfo.AddWarning(PassarValidationMessages.MessageNS30003_GreaterThanZero(humanReadbleNameProvider?.Invoke() ?? targetPropertyInfo.HumanReadableName));
		}
	}

	public override void CheckNS30003_TransportMode(ZPropertyInfo targetPropertyInfo, IMessageSendingDeclaration declaration)
	{
		if (declaration.WrappedDeclaration.IsOwnPropulsion && declaration.WrappedDeclaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().Any(x => x.IsSimplified))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30003_TransportMode);
		}
	}

	public override void CheckNS30003_NotAllowed(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction, Func<string> humanReadbleNameProvider = null, Func<bool> isEmpty = null)
	{
		if (IsSimplified(entryInstruction) && !(isEmpty?.Invoke() ?? targetPropertyInfo.Value.IsEmpty))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30003_NotAllowed(humanReadbleNameProvider?.Invoke() ?? targetPropertyInfo.HumanReadableName));
		}
	}

	public override void CheckNS30003_CusSupportingInfo(CusSupportingInfo supportingInfo, string message)
	{
		base.CheckNS30003_CusSupportingInfo(supportingInfo, message);
		if (supportingInfo.Parent is JobComInvoiceLine invoiceLine && IsSimplified(invoiceLine.EntryInstruction))
		{
			supportingInfo.AddRowMessageError(message);
		}
	}

	public override void CheckNS30003_NotSent(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
		if (!targetPropertyInfo.Value.IsEmpty && declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(i => i.IsSimplified))
		{
			targetPropertyInfo.AddWarning(PassarValidationMessages.MessageNS30003_NotSent);
		}
	}

	public override void CheckNS30003_Tariff(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
		if (IsSimplified(entryInstruction) && !((ZString)targetPropertyInfo.Value).IsEmpty)
		{
			targetPropertyInfo.AddWarning(PassarValidationMessages.MessageNS30003_Tariff);
		}
	}

	public override void CheckNS30003_NetWeight(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
		if (IsSimplified(entryInstruction) && (ZDecimal)targetPropertyInfo.Value > 0)
		{
			targetPropertyInfo.AddWarning(PassarValidationMessages.MessageNS30003_NetWeight);
		}
	}

	public override void CheckNS30003_TransportEquipment(BaseCusLinkPackage linkPackage, BaseJobComInvoiceLine invoiceLine)
	{
		base.CheckNS30003_TransportEquipment(linkPackage, invoiceLine);
		var entryInstruction = (CusEntryInstruction)invoiceLine.EntryInstruction;

		if (IsSimplified(entryInstruction) && linkPackage.IsLinked)
		{
			linkPackage.AddRowWarning(PassarValidationMessages.MessageNS30003_TransportEquipment);
		}
	}

	public override void CheckNS30003_InwardOutward(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		if (!targetPropertyInfo.Value.IsEmpty && (inAndOutwardProcessing.Parent?.EntryInstruction?.IsSimplified ?? false))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30003_InwardOutward);
		}
	}

	bool IsSimplified(CusEntryInstruction entryInstruction) => (entryInstruction?.IsSimplified ?? false);

	#endregion

	public override void CheckNS30021R132_NumberOfPacks(ZPropertyInfo targetPropertyInfo, InvoiceLinePackagePivot invoiceLinePackagePivot)
	{
		if (invoiceLinePackagePivot.CHC_NumberOfPacks > 0 && invoiceLinePackagePivot.Package.IsBulkOnlyPackaging)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30021R132_PackQtyForLoosePackaging(PassarValidationMessages.NS30021));
		}
	}

	public override void CheckNS30021R132_MarksAndNos(ZPropertyInfo targetPropertyInfo, Package package)
	{
		CheckNS30021R132_MarksAndNos(targetPropertyInfo, package, () => PassarValidationMessages.MessageNS30021_MarksAndNos);
	}

	public override void CheckNS30065(ZPropertyInfo targetPropertyInfo, TransportDocument transportDocument)
	{
		if (transportDocument.CSI_Code == TransportDocumentTypeCodes.MAWB)
		{
			var referenceNumber = transportDocument.CSI_ReferenceNumber;
			if (referenceNumber.Length != 11 || referenceNumber.ContainsAnyLetters)
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30065_InvalidFormat);
			}
			else
			{
				var serialNumber = ZInt.Parse(referenceNumber.Substring(3, 8));
				var validCheckdigit = Math.DivRem(serialNumber, 10, out var checkDigit) % 7;
				if (checkDigit != validCheckdigit)
				{
					targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30065_InvalidCheckdigit(validCheckdigit));
				}
				if (RefAirline.LoadFromAirlinePrefix(transportDocument.Factory, referenceNumber.Left(3)) == null)
				{
					targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30065_InvalidAirline);
				}
			}
		}
	}

	public override void CheckNS30092(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
		if (invoiceLine.JI_NetWeight.IsEmpty)
		{
			var universalTariff = invoiceLine.UniversalTariff;
			if (universalTariff != null && universalTariff.HasAttribute(UniversalReferenceConstants.TariffAttributes.NetMassOptional, UniversalReferenceConstants.TariffAttributes.Values.No))
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30092, targetPropertyInfo.HumanReadableName));
			}
		}
	}

	#region NS300098

	public override void CheckNS30098_Description(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		var procedure = inAndOutwardProcessing.Parent?.EntryInstruction?.CEI_Procedure ?? ZString.Empty;
		if (inAndOutwardProcessing.CSI_Description.IsEmpty)
		{
			if (procedure == ProcedureCodesPassar.OutwardProcessing || (procedure == ProcedureCodesPassar.ExportFromFreeCirculation && inAndOutwardProcessing.Repair))
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30098_DescriptionMandatory);
			}
		}
		else
		{
			if (procedure == ProcedureCodesPassar.ExportFromFreeCirculation && !inAndOutwardProcessing.Repair)
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30098_DescriptionNotApplicable);
			}
		}
	}

	public override void CheckNS30098_Procedures41And50(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing, Func<string> humanReadbleNameProvider)
	{
		if (targetPropertyInfo.Value.IsEmpty && (inAndOutwardProcessing.Parent?.EntryInstruction?.IsProcessingTransaction ?? false))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30098_MandatoryForProcedures41And50(humanReadbleNameProvider()));
		}
	}

	#endregion

	public override void CheckNS30104(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
		if (additionalInformation.Parent is JobComInvoiceLine invoiceLine
			&& invoiceLine.AdditionalInformations.Cast<AdditionalInformation>().Any(y => y.PK != additionalInformation.PK && y.CSI_Code == additionalInformation.CSI_Code))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30104);
		}
	}

	public override void CheckNS30108(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
		if (declaration.IsGoodsDestinationInNCL0147CountryList && !declaration.JE_SpecificCircumstanceIndicator.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30108_SpecificCircumstanceIndicator);
		}
	}

	public override void CheckNS30103(ZPropertyInfo targetPropertyInfo, Restriction restriction)
	{
		var isReferenceNumberEmpty = restriction.CSI_ReferenceNumber.IsEmpty;
		var isPermitOwnerEmpty = restriction.PermitOwnerDocAddress.E2_OA_Address.IsEmpty;
		if (!isReferenceNumberEmpty && isPermitOwnerEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30103, restriction.PermitOwnerDocAddress.AddressCaption));
		}
		else if (isReferenceNumberEmpty && !isPermitOwnerEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30103_PermitOwnerNotAllowed);
		}
	}

	public override void CheckNS30108(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
		var declaration = entryInstruction?.JobDeclaration;
		var isMOPEmpty = entryInstruction.CEI_TransportChargesMethodOfPayment.IsEmpty;
		var isInNCL0147CountryList = declaration.IsGoodsDestinationInNCL0147CountryList;
		if (isInNCL0147CountryList && !isMOPEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30108_EuSecurityZoneMethodOfPayment);
		}

		var consignorAEO = declaration.SupplierDocumentaryAddress?.Organisation.GetAEONumber() ?? ZString.Empty;
		var declarantAEO = declaration.DeclarantAddress?.Header.GetAEONumber() ?? ZString.Empty;
		if (!isInNCL0147CountryList && (consignorAEO.IsEmpty || declarantAEO.IsEmpty) && isMOPEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30108_TransportMethodOfPayment);
		}
	}

	public override void CheckNS30110(ZPropertyInfo targetPropertyInfo, RestrictionAdditionalInformation restrictionAdditionalInformation)
	{
		var isCodeEmpty = restrictionAdditionalInformation.CY_Code.IsEmpty;
		if (!isCodeEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo, messagePrefix: "[NS30110] ");
		}

		if (isCodeEmpty && !restrictionAdditionalInformation.CY_Data.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30110);
		}
	}

	public override void CheckNS30116(Restriction restriction)
	{
		restriction.ClearRowNotificationsContaining(PassarValidationMessages.MessageNS30116);

		if (!restriction.CSI_Code.IsEmpty && restriction.IsAdditionalInformation && restriction.AdditionalInformations.Cast<RestrictionAdditionalInformation>().All(x => x.CY_Code.IsEmpty))
		{
			restriction.AddRowMessageError(PassarValidationMessages.MessageNS30116);
		}
	}

	public override void CheckNS30120(ZPropertyInfo targetPropertyInfo, Restriction restriction)
	{
		if (!Regex.IsMatch(restriction.CSI_ReferenceNumber, @"^[a-zA-Z0-9-./]*$"))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30120);
		}
	}

	#region NS30130

	public override void CheckNS30130(ZPropertyInfo targetPropertyInfo, JobComInvoiceHeader invoiceHeader)
	{
		CheckNS30130_OrdinaryDeclaration(targetPropertyInfo, invoiceHeader);
		CheckNS30130_SimplifiedDeclaration(targetPropertyInfo, invoiceHeader);
	}

	void CheckNS30130_OrdinaryDeclaration(ZPropertyInfo targetPropertyInfo, JobComInvoiceHeader invoiceHeader)
	{
		if (invoiceHeader.JobDeclaration.JE_UCR.IsEmpty
			&& invoiceHeader.JZ_UCR.IsEmpty
			&& !invoiceHeader.JobDeclaration.HasTransportDocuments
			&& invoiceHeader.HasOrdinaryInvoiceLines)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30130_OrdinaryDeclaration);
		}
	}

	void CheckNS30130_SimplifiedDeclaration(ZPropertyInfo targetPropertyInfo, JobComInvoiceHeader invoiceHeader)
	{
		if (!invoiceHeader.JZ_UCR.IsEmpty
			&& invoiceHeader.HasSimplifiedInvoiceLines)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30130_SimplifiedDeclaration);
		}
	}

	#endregion NS30130

	public override void CheckNS30098_CustomsOffice(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		var isOrdinary = inAndOutwardProcessing.CSI_Procedure == InAndOutwardProcessingProcessTypesEdec.DueProcedure;
		var isCustomsOfficeEmpty = targetPropertyInfo.Value.IsEmpty;
		if (isCustomsOfficeEmpty && isOrdinary && (inAndOutwardProcessing.Parent?.EntryInstruction?.IsProcessingTransaction ?? false))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30098_CustomsOfficeMandatory);
		}

		if ((((inAndOutwardProcessing.Parent?.EntryInstruction?.IsProcessingTransaction ?? false)
			&& !isOrdinary)
			|| ((inAndOutwardProcessing.Parent?.EntryInstruction?.CEI_Procedure ?? ZString.Empty) == UniversalReferenceConstants.ProcedureCodesPassar.ExportFromFreeCirculation))
			&& !isCustomsOfficeEmpty)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30098_CustomsOfficeNotApplicable);
		}
	}

	public override void CheckNS30098_Procedure20(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
		if (!targetPropertyInfo.Value.IsEmpty &&
			(inAndOutwardProcessing.Parent?.EntryInstruction?.CEI_Procedure ?? ZString.Empty) == UniversalReferenceConstants.ProcedureCodesPassar.ExportFromFreeCirculation)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30098_NotApplicableForProcedure20(targetPropertyInfo.HumanReadableName));
		}
	}
	public override void CheckNS30001_CSI_ReferenceNumber(ZPropertyInfo targetPropertyInfo, Restriction restriction)
	{
		if (restriction.IsPermitNumberAllowed)
		{
			if (restriction.IsPermitExceptionReasonAllowed)
			{
				if (targetPropertyInfo.Value.IsEmpty && restriction.CSI_Description.IsEmpty)
				{
					targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30001_MandatoryPermitNumberOrPermitExceptionReason);
				}
				else if (!targetPropertyInfo.Value.IsEmpty && !restriction.CSI_Description.IsEmpty)
				{
					targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30001_NotApplicablePermitNumberNotBoth);
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo, messagePrefix: $"{PassarValidationMessages.NS30001} ");
			}
		}
		else
		{
			if (!restriction.CSI_ReferenceNumber.IsEmpty)
			{
				targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30001_NotApplicablePermitNumber);
			}
		}
	}

	public override void CheckNS30181(ZPropertyInfo targetPropertyInfo, InvoiceLinePackagePivot invoiceLinePackagePivot)
	{
		if (invoiceLinePackagePivot.CHC_NumberOfPacks == 0 && invoiceLinePackagePivot.Package.IsBreakBulkPackaging)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30181_BreakBulkPackQtyNotZero);
		}
	}
}
