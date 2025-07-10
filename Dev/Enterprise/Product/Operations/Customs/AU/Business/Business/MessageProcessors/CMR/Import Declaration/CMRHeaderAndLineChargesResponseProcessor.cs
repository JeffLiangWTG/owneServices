using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;

#pragma warning disable IDE0001 // Prevent simplification to base class
using ChargeTypes = Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes;
#pragma warning restore IDE0001 // Prevent simplification to base class

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRHeaderAndLineChargesResponseProcessor : CMRHeaderChargesResponseProcessor
	{
		public CMRHeaderAndLineChargesResponseProcessor(LoggingInformation logger, ZString messageCode, ZString messageName)
			: base(logger, messageCode, messageName)
		{
		}

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();

			if (result && cUSRES != null && entryHeader != null)
			{
				SetLineAmounts(entryHeader);
				ApportionDutyDeferredAcrossConsolidatedEntryMembers(entryHeader);
				CalculateTotalPayable(entryHeader);
			}

			return result;
		}

		void SetLineAmounts(CusEntryHeader entryHeader)
		{
			if (cUSRES.Group6.Count > 0)
			{
				var consolidatedEntryLines = consolidatedDeclaration?.JobDeclarations.Cast<JobDeclaration>().Select(x => x.EntryHeader).SelectMany(x => x.AllEntryLines).ToArray();

				foreach (SegmentGroup11 group11 in cUSRES.Group6[0].Group11)
				{
					ZInt lineNumber = ZInt.Parse(group11.CST[0].GoodsItemNumber);
					CusEntryLine entryLine;
					if (consolidatedEntryLines != null)
					{
						entryLine = consolidatedEntryLines.FirstOrDefault(x => x.ZA_AggregateEntryLineNumber == lineNumber);
					}
					else
					{
						entryLine = entryHeader.MergedLines.FindByLineNumber(lineNumber);
					}

					if (entryLine != null)
					{
						ZDecimal customsValue = 0m;//40
						ZDecimal duty = 0m;//55
						ZDecimal countervailingDuty = 0m;//122
						ZDecimal dumpingDuty = 0m;//125
						ZDecimal wET = 0m;//149
						ZDecimal gSTDeferred = 0m;//210
						ZDecimal gSTPayable = 0m;//369
						ZDecimal lCT = 0m;//371
						ZDecimal warehouseUnitValue = 0m;//146
						ZDecimal tILV = 0m;//68
						ZDecimal securityConcession = 0m;//292
						ZDecimal securityLiability = 0m;//Z01

						foreach (SegmentGroup12 group12 in group11.Group12)
						{
							if (group12.MOA[0].MonetaryAmount.MonetaryAmountTypeCodeQualifier != null)
							{
								switch (group12.MOA[0].MonetaryAmount.MonetaryAmountTypeCodeQualifier.ToString())
								{
									case "40":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out customsValue);
										break;
									case "55":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out duty);
										break;
									case "122":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out countervailingDuty);
										break;
									case "125":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out dumpingDuty);
										break;
									case "149":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out wET);
										break;
									case "210":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out gSTDeferred);
										break;
									case "369":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out gSTPayable);
										break;
									case "371":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out lCT);
										break;
									case "146":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out warehouseUnitValue);
										break;
									case "68":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out tILV);
										break;
									case "292":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out securityConcession);
										break;
									case "Z01":
										ZDecimal.TryParse(group12.MOA[0].MonetaryAmount.MonetaryAmountValue, out securityLiability);
										break;
								}
							}

							if (group11.FTX[0].TextSubjectCodeQualifier == TextSubjectCodeQualifierList.RateAdditionalInformation)
							{
								Common.DutyResult dutyResult = DutyRateParser.ParseAndGetResult(entryLine.Factory, group11.FTX[0].TextLiteral.FreeTextValue1);
								entryLine.CL_DutyPercent = dutyResult.Percent;
								entryLine.CL_FlatAmount = dutyResult.FlatRateAmount;
								entryLine.CL_FlatAmountUQ = dutyResult.FlatRateUQ;
							}
						}

						if (GISLineBreakDownIndicatorPresent(group11))
						{
							using (entryLine.GetValidationSuspender())
							{
								entryLine.CL_CustomsValue = customsValue;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.DutyAmount).CF_ChargeAmount = duty;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.CountervailingDuty).CF_ChargeAmount = countervailingDuty;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.DumpingDuty).CF_ChargeAmount = dumpingDuty;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.WetAmount).CF_ChargeAmount = wET;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTDeferred).CF_ChargeAmount = gSTDeferred;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTAmount).CF_ChargeAmount = gSTPayable;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.LCTAmount).CF_ChargeAmount = lCT;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.TotalDutyTaxForLine).CF_ChargeAmount = duty + countervailingDuty + dumpingDuty + wET + gSTPayable + lCT;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.SecurityConcession).CF_ChargeAmount = securityConcession;
								entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypes.SecurityLiability).CF_ChargeAmount = securityLiability;
								entryLine.UpdateTILV(tILV.ToString() + "AUD");
								entryLine.CL_WarehouseUnitValue = warehouseUnitValue;
							}

							#pragma warning disable IDE0001 // Prevent simplification to base class
							using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(entryLine.Declaration))
							#pragma warning restore IDE0001 // Prevent simplification to base class
							{
								foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
								{
									using (invoiceLine.GetValidationSuspender())
									{
										invoiceLine.AddInfo.ZA_WUV = warehouseUnitValue;
									}
								}
							}
						}
					}
				}
			}
		}

		protected virtual bool GISLineBreakDownIndicatorPresent(SegmentGroup11 group11)
		{
			return group11.Group12.Count > 0;
		}

		void ApportionDutyDeferredAcrossConsolidatedEntryMembers(CusEntryHeader leadEntryHeader)
		{
			if (consolidatedDeclaration != null)
			{
				var logBuilder = new ZStringBuilder(consolidatedDeclaration.CRD_JobReferenceNumber + "\r\n");

				var dutyDeferredAmount = leadEntryHeader.Charges.GetAmount(ChargeTypes.DutyDeferredAmount);
				var shouldApportion = !dutyDeferredAmount.IsEmpty && (!totalActualDuty.HasValue || totalActualDuty.Value > 0.0m);
				var totalApportioned = ZDecimal.Zero;

				foreach (JobDeclaration memberDec in consolidatedDeclaration.JobDeclarations)
				{
					logBuilder.AppendLine(memberDec.JE_DeclarationReference);

					var apportionedDutyDeferred = ZDecimal.Zero;
					var entryHeader = memberDec.EntryHeader;
					var isLeadDeclaration = memberDec.PK == consolidatedDeclaration.CRD_JE_LeadDeclaration;

					if (shouldApportion)
					{
						if (isLeadDeclaration)
						{
							var aQISProcessingCharge = leadEntryHeader.Charges.GetAmount(ChargeTypes.AQISProcessingCharge);
							var declarationProcessingCharge = leadEntryHeader.Charges.GetAmount(ChargeTypes.DeclarationProcessingCharge);
							var totalWoodLevy = leadEntryHeader.Charges.GetAmount(ChargeTypes.Woodlevy);
							var declarationfees = aQISProcessingCharge + declarationProcessingCharge + totalWoodLevy;

							logBuilder.AppendLine($"Is Lead. Deferred Header Fees ${declarationfees}");
							apportionedDutyDeferred += declarationfees;
						}

						foreach (CusEntryLine entryLine in entryHeader.MergedLines)
						{
							var deferrableFees = entryLine.DeferrableFeesAndCharges;
							var isEEG = entryLine.IsExciseEquivalentGoods;
							if (!isEEG)
							{
								deferrableFees += entryLine.Fees.GetAmount(ChargeTypes.DutyAmount);
							}
							apportionedDutyDeferred += deferrableFees;

							logBuilder.AppendLine($"Line {entryLine.ZA_AggregateEntryLineNumber}. Deferred ${deferrableFees}{(isEEG ? " (is EEG)" : "")}");
						}
					}

					entryHeader.Charges.SetAmount(ChargeTypes.DutyDeferredAmount, apportionedDutyDeferred);
					totalApportioned += apportionedDutyDeferred;
					logBuilder.AppendLine($"Total Apportioned: ${apportionedDutyDeferred}\r\n");

					if (!isLeadDeclaration)
					{
						CalculateTotalPayable(entryHeader);
					}
				}

				if (shouldApportion && totalApportioned != dutyDeferredAmount)
				{
					logBuilder.Prepend($"DutyDeferred Apportionment Error. Expected {dutyDeferredAmount}  Was {totalApportioned}\r\n\r\n");
					Logger.LogError(logBuilder.ToString());
				}
				else
				{
					Logger.Log(logBuilder.ToString());
				}
			}
		}

		void CalculateTotalPayable(CusEntryHeader entryHeader)
		{
			ZDecimal result = 0m;
			var isWeeklySettlement = entryHeader.Declaration?.SettlementTypeSelected ?? false;

			foreach (CusEntryHeaderCharges charge in entryHeader.Charges)
			{
				if (charge.IsPayableToCustomsForHeader)
				{
					var isProcessingChargeWithWeeklySettlement = isWeeklySettlement && (charge.C1_ChargeType == CusEntryChargeTypeList.Codes.DeclarationProcessingCharge || charge.C1_ChargeType == CusEntryChargeTypeList.Codes.EntryFee);
					if (!isProcessingChargeWithWeeklySettlement)
					{
						result += charge.C1_ChargeAmount;
					}
				}
			}

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				foreach (CusEntryLineFee lineFee in entryLine.Fees)
				{
					if (lineFee.IsPayableToCustomsForLine && !lineFee.CF_IsLandedCostOnly)
					{
						result += lineFee.CF_ChargeAmount;
					}
				}
			}

			entryHeader.CH_TotalPaid = result - entryHeader.TotalDeferredDutyFromCustoms;
		}
	}
}
