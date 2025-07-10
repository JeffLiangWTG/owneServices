using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecAcceptanceMessageProcessor : BaseResponseMessageProcessor<IAcceptanceResponseDetail>
{
	public EdecAcceptanceMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("0978270E-A1A2-4642-A931-93CB3486B522", "Acceptance Message Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.Import, MessageTypeCodeList.Codes.Export };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Accepted };

	protected override ZString GetEntryHeaderReference(IAcceptanceResponseDetail customsResponse) => customsResponse.TraderDeclarationNumber;

	protected override void ProcessResponseMessage(CHEDIMessage message, IAcceptanceResponseDetail customsResponse)
	{
		if (customsResponse != null && message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			var mrnEntryNum = $"{customsResponse.CustomsDeclarationNumber}.{customsResponse.CustomsDeclarationVersion}";
			if (entryHeader.GetLatestDeclarationVersion() < customsResponse.CustomsDeclarationVersion)
			{
				var issueDate = customsResponse.IssueDate.DateTime;
				var accessCode = customsResponse.AccessCode;

				entryHeader.CH_Status = CHLogicalStatusList.Codes.Accepted;
				entryHeader.CH_EntryStatus = customsResponse.Status;
				entryHeader.CH_EntryReleaseDate = customsResponse.IsGoodsDeclarationRelease ? issueDate : entryHeader.CH_EntryReleaseDate;
				entryHeader.CH_PhaseStatus = entryHeader.CH_PhaseStatus == PassarDeclarationPhaseList.Codes.Amendment ? PassarDeclarationPhaseList.Codes.Declaration : entryHeader.CH_PhaseStatus;
				entryHeader.EntryInstruction.CEI_DateForDuty = issueDate;

				if (!string.IsNullOrEmpty(mrnEntryNum))
				{
					var oldValue = entryHeader.MovementReferenceNumber;
					entryHeader.MovementReferenceNumberSetter(mrnEntryNum, issueDate, customsResponse.EntryStatus);
					LogInformationIfValueUpdated(Res.GetString("16b82b2b-8e5a-49b6-a9bb-ba96be06e44d", "Movement Reference Number"), oldValue, mrnEntryNum, message.EM_MessageNum);
				}

				if (!string.IsNullOrEmpty(accessCode))
				{
					var oldValue = entryHeader.AccessCode;
					entryHeader.AccessCodeSetter(accessCode, issueDate);
					LogInformationIfValueUpdated(Res.GetString("73717f00-ff27-40f7-b1b1-58a1606b1f60", "Access Code"), oldValue, accessCode, message.EM_MessageNum);
				}

				UpdateCusEntryHeaderCharges(customsResponse, entryHeader);
				UpdateEntryLineFees(customsResponse, entryHeader);
			}

			var reference = entryHeader.CreateClearanceEventReference(mrnEntryNum, customsResponse.Status);
			entryHeader.Logs.AddNew(entryHeader.Declaration.CustomsClearedEventType, reference, customsResponse.IssueDate);
		}
	}

	void UpdateCusEntryHeaderCharges(IAcceptanceResponseDetail customsResponse, CusEntryHeader entryHeader)
	{
		entryHeader.ConfirmedCharges.RemoveAndDeleteAll();

		if (customsResponse.Duty != null)
		{
			PopulateCusEntryHeaderCharges(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, customsResponse.Duty);
		}

		if (customsResponse.VAT != null)
		{
			PopulateCusEntryHeaderCharges(Core.Constants.Customs.CusEntryFeeTypes.VAT, customsResponse.VAT);
		}

		PopulateChargesWithTaxesAndFees(customsResponse.AdditionalTaxes);

		PopulateChargesWithTaxesAndFees(customsResponse.Fees);

		void PopulateChargesWithTaxesAndFees(IEnumerable<IEdecResponseFee> additionalTaxesOrFees)
		{
			if (additionalTaxesOrFees != null)
			{
				foreach (var additionalTaxOrFee in additionalTaxesOrFees)
				{
					PopulateCusEntryHeaderCharges(additionalTaxOrFee.Type, additionalTaxOrFee.Amount);
				}
			}
		}

		void PopulateCusEntryHeaderCharges(ZString chargeType, decimal? amount)
		{
			var newCharge = entryHeader.Charges.AddNew();
			newCharge.C1_Source = CusEntryHeaderChargesSourceCodeList.Codes.CUS;
			newCharge.C1_ChargeType = chargeType;
			newCharge.C1_ChargeAmount = (ZDecimal)amount;
		}
	}

	void UpdateEntryLineFees(IAcceptanceResponseDetail customsResponse, CusEntryHeader entryHeader)
	{
		foreach (var goodsItem in customsResponse.GoodsItems)
		{
			var entryLine = GetEntryLine(entryHeader, goodsItem.TraderItemID);

			if (entryLine != null)
			{
				entryLine.ConfirmedFees.RemoveAndDeleteAll();

				PopulateCusEntryLineFees(entryLine, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, goodsItem.ValuationDetailDuty, goodsItem.ValuationRate);
				PopulateCusEntryLineFees(entryLine, Core.Constants.Customs.CusEntryFeeTypes.VAT, goodsItem.ValuationDetailVAT);

				goodsItem.AdditionalTaxDetails?.GroupBy(g => g.Type).Select(c => new { GroupedTariffType = c.Key, SumAmount = c.Sum(s => s.Amount) }).ForEach(additionalTaxDetail =>
				{
					PopulateCusEntryLineFees(entryLine, additionalTaxDetail.GroupedTariffType, additionalTaxDetail.SumAmount);
				});

				goodsItem.FeeDetails?.ForEach(fee =>
				{
					PopulateCusEntryLineFees(entryLine, fee.Type, fee.Amount);
				});
			}
		}

		CusEntryLine GetEntryLine(CusEntryHeader entryHeader, string traderItemID)
		{
			return short.TryParse(traderItemID, out var traderItemIdShort)
						? entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(l => l.CL_LineNumber == traderItemIdShort)
						: null;
		}

		void PopulateCusEntryLineFees(CusEntryLine entryLine, ZString chargeType, ZDecimal chargeAmount, ZDecimal chargeRate = new ZDecimal())
		{
			var newDuty = entryLine.Fees.AddNew();
			newDuty.CF_Source = CusEntryHeaderChargesSourceCodeList.Codes.CUS;
			newDuty.CF_ChargeType = chargeType;
			newDuty.CF_ChargeAmount = chargeAmount;
			newDuty.CF_Rate = chargeRate;
		}
	}
}
