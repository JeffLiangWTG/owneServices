using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact.D99B.Messages.CUSRES;
#pragma warning disable IDE0001 // Prevent simplification to base class
using ChargeTypes = Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes;
#pragma warning restore IDE0001 // Prevent simplification to base class

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRHeaderChargesResponseProcessor : BaseImportDeclarationMessageProcessor
	{
		protected CMRHeaderChargesResponseProcessor(LoggingInformation logger, ZString messageCode, ZString messageName)
			: base(logger, messageCode, messageName)
		{
		}

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();

			if (result && entryHeader != null && cUSRES != null)
			{
				SetHeaderAmounts(entryHeader);
			}

			return result;
		}

		protected ZDecimal? totalActualDuty; //55: Total Actual Duty.  Needed by CMRHeaderAndLineChargesResponseProcessor because the Header is processed before the Lines.

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected void SetHeaderAmounts(CusEntryHeader entryHeader)
		{
			if (cUSRES.Group5.Count > 0)
			{
				ZDecimal aQISContainerCharge = ZDecimal.Zero;//35:Container Cost
				ZDecimal aQISProcessingCharge = ZDecimal.Zero;//26:ChargesCollectFee
				ZDecimal declarationProcessingCharge = ZDecimal.Zero;//23:ChargeAmount
				ZDecimal totalOtherCharges = ZDecimal.Zero;//304:OtherCharges
				ZDecimal totalPayableAdmin = ZDecimal.Zero;//7
				ZDecimal totalWoodLevy = ZDecimal.Zero;//58:FeeAmount
				ZDecimal totalTILV = ZDecimal.Zero;//68:TILV
				ZDecimal dutyDeferredAmount = ZDecimal.Zero;//346 Total Deferred Amount

				bool hasAQISProcessingChargeReported = false;
				bool hasDeclarationProcessingChargeReported = false;

				foreach (SegmentGroup5 group5 in cUSRES.Group5)
				{
					var monetaryAmount = group5.MOA[0].MonetaryAmount;
					var qualifier = monetaryAmount.MonetaryAmountTypeCodeQualifier?.ToString();

					if (qualifier != null)
					{
						switch (qualifier)
						{
							case "35":
								ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out aQISContainerCharge);
								break;
							case "26":
								ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out aQISProcessingCharge);
								hasAQISProcessingChargeReported = true;
								break;
							case "23":
								ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out declarationProcessingCharge);
								hasDeclarationProcessingChargeReported = true;
								break;
							case "304":
								ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out totalOtherCharges);
								break;
							case "7":
								ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out totalPayableAdmin);
								break;
							case "55":
								if (ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out var qualifier55Value))
								{
									totalActualDuty = qualifier55Value;
								}
								break;
							case "58":
								ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out totalWoodLevy);
								break;
							case "68":
								ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out totalTILV);
								break;
							case "346":
								ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out dutyDeferredAmount);
								break;
						}
					}
				}

				entryHeader.Charges[ChargeTypes.Woodlevy].C1_ChargeAmount = totalWoodLevy;
				entryHeader.Charges[ChargeTypes.AQISContainerCharges].C1_ChargeAmount = aQISContainerCharge;
				entryHeader.Charges[ChargeTypes.OtherCharges].C1_ChargeAmount = totalOtherCharges;
				entryHeader.Charges[ChargeTypes.TotalPayableAdmin].C1_ChargeAmount = totalPayableAdmin;
				entryHeader.Charges[ChargeTypes.DutyDeferredAmount].C1_ChargeAmount = dutyDeferredAmount;

				if (hasAQISProcessingChargeReported || IsAccepted)
				{
					entryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount = aQISProcessingCharge;
				}
				if (hasDeclarationProcessingChargeReported || IsAccepted)
				{
					entryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount = declarationProcessingCharge;
				}

				var auTILV = totalTILV.ToString() + "AUD";
				if (consolidatedDeclaration != null)
				{
					consolidatedDeclaration.UpdateTILV(auTILV);
				}
				else
				{
					entryHeader.UpdateTILV(auTILV);
				}
			}
		}

		bool IsAccepted => (outgoingMessage?.IsAnOriginal ?? false);
	}
}
