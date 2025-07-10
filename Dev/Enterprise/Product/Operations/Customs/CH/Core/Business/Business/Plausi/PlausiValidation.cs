using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CH.Business;

public class PlausiValidation
{
	protected PlausiValidation()
	{
	}

	public static PlausiValidation New(ICusSupportingInfoParent parent) => New(parent?.JobDeclaration);

	public static PlausiValidation New(JobDeclaration declaration)
	{
		if (declaration == null)
		{
			return new PlausiValidation();
		}

		return declaration.IsImport ? new ImportPlausiValidation()
							: declaration.IsExport ? new ExportPlausiValidation()
							: declaration.IsExportDeclarationActivation ? new ExportDeclarationActivationPlausiValidation()
							: new PlausiValidation();
	}

	public virtual void CheckCH0001(ZPropertyInfo targetPropertyInfo, IMessageSendingDeclaration declaration)
	{
	}

	public virtual void CheckCH0003(JobComInvoiceLine invoiceLine)
	{
		invoiceLine.ClearRowNotificationsContaining(PassarValidationMessages.MessageCH0003);
		if (!invoiceLine.JobDeclaration.IsExportDeclarationActivation &&
			!invoiceLine.PackagesPivot.Any())
		{
			invoiceLine.AddRowMessageError(PassarValidationMessages.MessageCH0003);
		}
	}

	public void CheckCH0004(ZPropertyInfo targetPropertyInfo)
	{
		MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo, messagePrefix: $"[{ValidationMessages.Plausi.CH0004}] ");
	}

	public virtual void CheckCH0005(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
	}

	public virtual void CheckNP70066(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR121(ZPropertyInfo targetPropertyInfo, IDocAddress address, string nameOfAddress = null)
	{
	}

	public virtual void CheckR123(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR124(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR127(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR130(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR133a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR133b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR156(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR134abc(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR135(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR144abc(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR146(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
	}

	public virtual void CheckR147(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
	}

	public virtual void CheckR148(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
	}

	public virtual void CheckR158(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine parent)
	{
	}

	public virtual void CheckR159(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR160(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR165(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR166c(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
	}

	public virtual void CheckR166c(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR167c(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
	}

	public virtual void CheckR176(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR177(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR168(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
	}

	public virtual void CheckR170ab(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR205(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckR173(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR174(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR181(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR183a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR183b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR191(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR193(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR198AndNP70167(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	protected void CheckR198AndNP70167(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine, string messageError)
	{
		if (!invoiceLine.JI_NonTradingGoods && invoiceLine.InAndOutwardProcessingRepair)
		{
			targetPropertyInfo.AddMessageError(messageError);
		}
	}

	public virtual void CheckR208(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckR219(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR224(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR206(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckR227a(ZPropertyInfo targetProperty, SupportingDocument supportingDocument)
	{
	}

	public virtual void CheckR249a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR249b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR249c(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
	}

	public virtual void CheckR249d(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR249e(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR256(ZPropertyInfo targetProperty, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR288(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
	}

	public virtual void CheckR261IssuerType(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckR261NonTradingGoods(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR262(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
	}

	public virtual void CheckR267(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR268a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR268b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR271(ZPropertyInfo targetPropertyInfo)
	{
	}

	public virtual void CheckR275(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR276(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR277(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR325(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR290(ZPropertyInfo targetProperty, ICusSupportingInfoParent parent)
	{
	}

	public virtual void CheckR290(ZPropertyInfo targetProperty, SupportingDocument supportingDocument)
	{
	}

	public virtual void CheckR313(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
	}

	public virtual void CheckR314(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
	}

	public virtual void CheckR315(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
	}

	public virtual void CheckR316(ZPropertyInfo targetPropertyInfo, Permit permit)
	{
	}

	public virtual void CheckR330(CusLineTariffDetail lineTariffDetail)
	{
	}

	public virtual void CheckR326R327(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
	}

	public virtual void CheckR334(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
	}

	public virtual void CheckR336(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR337(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
	}

	public virtual void CheckR339(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail tariffDetail)
	{
	}

	public virtual void CheckR348(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
	}

	public virtual void CheckR359(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR361(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNP70001(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNP70172(ZPropertyInfo targetPropertyInfo, IDocAddress address, DocAddressType type)
	{
	}

	public virtual void CheckR292(ZPropertyInfo targetPropertyInfo, DeclarationMessageSendingObject sendingObject)
	{
	}

	public virtual void CheckE001_Reference(ZPropertyInfo targetPropertyInfo, SupportingDocument supportingDocument)
	{
	}

	public virtual void CheckE001_DateOfIssue(ZPropertyInfo targetPropertyInfo, SupportingDocument supportingDocument)
	{
	}

	public virtual void CheckR285(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNS30021R132_MarksAndNos(ZPropertyInfo targetPropertyInfo, Package package)
	{
	}

	protected void CheckNS30021R132_MarksAndNos(ZPropertyInfo targetPropertyInfo, Package package, Func<string> validationMessage)
	{
		if (package.CW_MarksAndNos.IsEmpty && !package.IsLoosePackaging)
		{
			targetPropertyInfo.AddMessageError(validationMessage());
		}
	}

	public virtual void CheckR162R201(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR175R179(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	protected static bool IsCountryCHorLI(ZString countryCode) => countryCode == Core.Constants.CountryCodes.Switzerland || countryCode == Core.Constants.CountryCodes.Liechtenstein;

	protected static bool IsCountryCHorLIorDE(ZString countryCode) => IsCountryCHorLI(countryCode) || countryCode == Core.Constants.CountryCodes.Germany;

	public virtual void CheckR190(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckR229(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckR247(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR353(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR358(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckR356(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR182(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}
	public virtual void CheckR141(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR142(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR145a(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR145b(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR137(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
	}

	public virtual void CheckR138(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
	}

	public virtual void CheckR149a(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
	}

	public virtual void CheckR220a(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
	}

	public virtual void CheckR149b(ZPropertyInfo targetPropertyInfo, CusLineTariffDetail cusLineTariffDetail)
	{
	}

	public virtual void CheckNP70000(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invLine)
	{
	}

	public virtual void CheckNP70009_Weight(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invLine)
	{
	}

	public virtual void CheckNP70009_Price(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invLine)
	{
	}

	public virtual void CheckNP70020(ZPropertyInfo targetPropertyInfo, AutoCusHouseContPackInvoiceLinePivot packInvLinePivot)
	{
	}

	protected void CheckNP70020(ZPropertyInfo targetPropertyInfo, AutoCusHouseContPackInvoiceLinePivot packInvLinePivot, string messageError)
	{
		var package = packInvLinePivot.Factory.Load<Package>(packInvLinePivot.CHC_CW);
		var invoiceLine = packInvLinePivot.Factory.Load<JobComInvoiceLine>(packInvLinePivot.CHC_JI);

		if (packInvLinePivot.CHC_NumberOfPacks.IsEmpty && !packInvLinePivot.CHC_JI.IsEmpty && !package.IsLoosePackaging)
		{
			if (invoiceLine != null && AllPackQtyOnSameInstructionAreZero(invoiceLine))
			{
				packInvLinePivot.CHC_NumberOfPacksInfo.AddMessageError(messageError);
			}
		}

		bool AllPackQtyOnSameInstructionAreZero(JobComInvoiceLine invoiceLine)
		{
			var invoiceLinesOnTheSameEntryInstruction = invoiceLine.EntryInstruction?.InvoiceLines;
			return invoiceLinesOnTheSameEntryInstruction != null && !package.InvoiceLinePivotCollection.OfType<InvoiceLinePackagePivot>()
				.Where(p => invoiceLinesOnTheSameEntryInstruction.Contains(p.InvoiceLine))
				.Any(p => p.CHC_NumberOfPacks > 0);
		}
	}

	public virtual void CheckNP70021(ZPropertyInfo targetPropertyInfo, CusContainer container)
	{
	}

	public virtual void CheckR128(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNS30092(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNP70026(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNP70043(ZPropertyInfo targetPropertyInfo, CusContainer cusContainer)
	{
	}

	public virtual void CheckNP70065(ZPropertyInfo targetPropertyInfo, string postCode, string message)
	{
	}

	public virtual void CheckNP70127(ZPropertyInfo targetPropertyInfo, OrgHeader header)
	{
	}
	public virtual void CheckNP70165(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckNP70169(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNS30001_PermitExceptionReason(ZPropertyInfo targetPropertyInfo, Restriction restriction)
	{
	}

	public virtual void CheckNS30003_NotEmpty(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction, Func<string> humanReadbleNameProvider = null)
	{
	}

	public virtual void CheckNS30003_Tariff(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
	}

	public virtual void CheckNS30003_Ticked(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction, Func<string> humanReadbleNameProvider = null)
	{
	}

	public virtual void CheckNS30003_GreaterThanZero(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction, Func<string> humanReadbleNameProvider = null)
	{
	}

	public virtual void CheckNS30003_TransportMode(ZPropertyInfo targetPropertyInfo, IMessageSendingDeclaration declaration)
	{
	}

	public virtual void CheckNS30003_NetWeight(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
	}

	public virtual void CheckNS30003_CusSupportingInfo(CusSupportingInfo supportingInfo, string message)
	{
		supportingInfo.ClearRowNotificationsContaining(message);
	}

	public virtual void CheckNS30003_NotAllowed(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction, Func<string> humanReadbleNameProvider = null, Func<bool> isEmpty = null)
	{
	}

	public virtual void CheckNS30003_NotSent(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
	}

	public virtual void CheckNS30003_TransportEquipment(BaseCusLinkPackage linkPackage, BaseJobComInvoiceLine invoiceLine)
	{
		linkPackage.ClearRowNotifications();
	}

	public virtual void CheckNS30003_InwardOutward(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckNS30021R132_NumberOfPacks(ZPropertyInfo targetPropertyInfo, InvoiceLinePackagePivot invoiceLinePackagePivot)
	{
	}

	public virtual void CheckNP70163(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckNS30065(ZPropertyInfo targetPropertyInfo, TransportDocument transportDocument)
	{
	}

	public virtual void CheckNP70168(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
	}

	public virtual void CheckNP70168(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine jobComInvoiceLine)
	{
	}

	public virtual void CheckNP70178(ZPropertyInfo targetPropertyInfo, AutoCusHouseContPackInvoiceLinePivot packInvLinePivot)
	{
	}

	public virtual void CheckNS30108(ZPropertyInfo targetPropertyInfo, CusEntryInstruction addInfoEntryInstruction)
	{
	}

	public virtual void CheckNS30108(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
	}

	public virtual void CheckNS30103(ZPropertyInfo targetPropertyInfo, Restriction restriction)
	{
	}

	public virtual void CheckNS30110(ZPropertyInfo targetPropertyInfo, RestrictionAdditionalInformation restrictionAdditionalInformation)
	{
	}

	public virtual void CheckNP70097(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNP70199(RestrictionAdditionalInformation restrictionAdditionalInformation)
	{
		restrictionAdditionalInformation.ClearRowNotificationsContaining(PassarValidationMessages.NP70199);
	}

	public virtual void CheckNS30104(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
	}

	public virtual void CheckNS30116(Restriction restriction)
	{
	}

	public virtual void CheckNS30117(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
	}

	public virtual void CheckNS30120(ZPropertyInfo targetPropertyInfo, Restriction restriction)
	{
	}

	public virtual void CheckNS30130(ZPropertyInfo targetPropertyInfo, JobComInvoiceHeader invoiceHeader)
	{
	}

	public virtual void CheckNS30098_Description(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckNS30098_Procedures41And50(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing, Func<string> humanReadbleNameProvider)
	{
	}

	public virtual void CheckNS30098_CustomsOffice(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckNS30098_Procedure20(ZPropertyInfo targetPropertyInfo, InAndOutwardProcessing inAndOutwardProcessing)
	{
	}

	public virtual void CheckNP70162(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNP70212(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
	}

	public virtual void CheckNP70212(ZPropertyInfo targetPropertyInfo, CusEntryInstruction entryInstruction)
	{
	}

	public virtual void CheckNP70197(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
	}

	public virtual void CheckNP70197(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine jobComInvoiceLine)
	{
	}

	public virtual void CheckNP70213_V1201(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
	}

	public virtual void CheckNP70213_V1202(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
	}

	public virtual void CheckNP70195(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNP70195(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
	}

	public virtual void CheckNP70229(ZPropertyInfo targetPropertyInfo, RestrictionAdditionalInformation restrictionAdditionalInformation)
	{
	}

	public virtual void CheckNS30001_CSI_ReferenceNumber(ZPropertyInfo targetPropertyInfo, Restriction restriction)
	{
	}

	public virtual void CheckNP70175(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckNP70155(ZPropertyInfo targetPropertyInfo, AdditionalInformation additionalInformation)
	{
	}

	public virtual void CheckR338acd(ZPropertyInfo targetPropertyInfo, JobComInvoiceLine invoiceLine)
	{
	}

	public virtual void CheckR257(Tobacco tobacco)
	{
	}

	public virtual void CheckR331abcd(ZPropertyInfo targetPropertyInfo, Tobacco tobacco)
	{
	}

	public virtual void CheckNS30181(ZPropertyInfo targetPropertyInfo, InvoiceLinePackagePivot invoiceLinePackagePivot)
	{
	}
}
