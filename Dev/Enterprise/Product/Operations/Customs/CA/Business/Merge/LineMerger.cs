using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		#region GetEntryCreationStrategies

		protected override EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			#region Testing
#if DEBUG
			if (Declaration.UseBaseMergeStrategyForTesting)
			{
				return base.GetEntryCreationStrategies();
			}
#endif
			#endregion

			EntryCreationStrategy[] result;
			var isB3Lodged = Declaration.IsB3Lodged;
			if (Declaration.IsExport)
			{
				result = CACustomsDataRegistry.Instance.SendG7ExportMessages.Value
							? new[] { new ExportMergeStrategy(Declaration, MessageTypeList.Codes.G7Export) }
							: new[] { new ExportMergeStrategy(Declaration, MessageTypeList.Codes.DataLoadingModule) };
			}
			else if (Declaration.IsLVS)
			{
				result = new[] { new ImportMergeStrategy(Declaration, MessageTypeList.GetCADOrB3CMessageType(isB3Lodged)) };
			}
			else if (Declaration.IsImport)
			{
				result = new EntryCreationStrategy[]
							{
								new ImportMergeStrategy(Declaration, MessageTypeList.Codes.EDIRelease),
								new ImportMergeStrategy(Declaration, MessageTypeList.GetCADOrB3CMessageType(isB3Lodged))
							};
			}
			else
			{
				result = base.GetEntryCreationStrategies();
			}
			return result;
		}

		#endregion

		#region OnMerging / OnMerged

		protected override void OnMerging()
		{
			base.OnMerging();
			if (Declaration.IsImport)
			{
				foreach (JobComInvoiceLine line in Declaration.InvoiceLines)
				{
					line.SuspendMarkApportionmentDirtyForDutiesAndTaxes();
				}
			}
		}

		protected override void OnMerged()
		{
			base.OnMerged();
			var declaration = Declaration;
			if (declaration.IsIM2 && declaration.PreviousJob != null)
			{
				MergeEntryLinesForIM2(declaration);
			}
			if (declaration.IsImport)
			{
				foreach (JobComInvoiceLine line in declaration.InvoiceLines)
				{
					line.ResumeMarkApportionmentDirtyForDutiesAndTaxes();
				}
			}
		}

		void MergeEntryLinesForIM2(JobDeclaration declaration)
		{
			RenumberSubHeadersOnCopiedJob(declaration, declaration.PreviousJob);

			var entryNoList = new List<ZShort>();

			foreach (CusEntryLine line in declaration.B3EntryHeader.AllEntryLines)
			{
				if (!entryNoList.Contains(line.CL_LineNumber))
				{
					var invoiceLine = (JobComInvoiceLine)line.InvoiceLines.First();
					var subHeaderNumber = invoiceLine.CA_B3SubHeaderNumber;

					var previousB3LineNo = invoiceLine.CA_PreviousB3LineNo;
					var previousEntryLine = GetPreviousEntryLine(declaration, previousB3LineNo);
					var entryLines = (from CusEntryLine entryLine in declaration.B3EntryHeader.AllEntryLines
									  let invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().First()
									  where invoiceLines.CA_PreviousB3LineNo == previousB3LineNo
									  select entryLine).ToList();

					if (entryLines.Count == 1)
					{
						var effectivLine = entryLines[0];
						effectivLine.CA_B2SubHeader = subHeaderNumber;
						effectivLine.CA_B2LineNo = previousB3LineNo.ToString();
						entryNoList.Add(effectivLine.CL_LineNumber);
					}
					else if (entryLines.Count > 1)
					{
						var currentmaxmatchpoint = 0;
						var currentmaxindex = -1;
						for (int index = 0; index < entryLines.Count; index++)
						{
							var effectivLine = entryLines[index];
							var lineMatchPoint = GetMatchPoint(effectivLine, previousEntryLine);
							var effectiveInvoiceLine = (JobComInvoiceLine)effectivLine.InvoiceLines.First();
							effectivLine.CA_B2SubHeader = effectiveInvoiceLine.CA_B3SubHeaderNumber;

							if (lineMatchPoint > currentmaxmatchpoint)
							{
								effectivLine.CA_B2LineNo = previousB3LineNo.ToString();
								if (currentmaxindex > -1)
								{
									entryLines[currentmaxindex].CA_B2LineNo = new ZString(previousB3LineNo + JobComInvoiceLine.SplitLine);
								}
								currentmaxmatchpoint = lineMatchPoint;
								currentmaxindex = index;
							}
							else
							{
								effectivLine.CA_B2LineNo = new ZString(previousB3LineNo + JobComInvoiceLine.SplitLine);
							}
							entryNoList.Add(effectivLine.CL_LineNumber);
						}
					}
				}
			}
		}

		internal static void RenumberSubHeadersOnCopiedJob(JobDeclaration declaration, JobDeclaration previousJob)
		{
			var previousSubHeaderNumbers = previousJob.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(line => line.CA_B3SubHeaderNumber).Select(line => line.CA_B3SubHeaderNumber).ToArray();
			var highest = previousSubHeaderNumbers.Last();
			var dic = new Dictionary<ZInt, ZInt>();
			var newInvoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();
			var invoiceDic = new Dictionary<ZInt, List<JobComInvoiceLine>>();

			foreach (var invoiceLine in newInvoiceLines)
			{
				if (!invoiceDic.ContainsKey(invoiceLine.CA_B3SubHeaderNumber))
				{
					invoiceDic.Add(invoiceLine.CA_B3SubHeaderNumber, new List<JobComInvoiceLine> { invoiceLine });
				}
				else
				{
					invoiceDic[invoiceLine.CA_B3SubHeaderNumber].Add(invoiceLine);
				}
			}

			foreach (var key in invoiceDic.Keys)
			{
				var matchLines = invoiceDic[key].OrderBy(x => x.CA_PreviousB3LineNo);

				var getMatchedPrevious = false;
				foreach (var matchLine in matchLines.Where(matchLine => !dic.ContainsValue(matchLine.CA_PreviousB3SubHeaderNo)))
				{
					dic.Add(key, matchLine.CA_PreviousB3SubHeaderNo);
					getMatchedPrevious = true;
					break;
				}
				if (!getMatchedPrevious)
				{
					highest++;
					dic.Add(key, highest);
				}
			}

			foreach (var key in dic.Keys)
			{
				if (key != dic[key])
				{
					invoiceDic[key].Where(line => line.CA_B3SubHeaderNumber == key).ForEach(line => line.CA_B3SubHeaderNumber = dic[key]);
				}
			}
		}

		CusEntryLine GetPreviousEntryLine(JobDeclaration declaration, ZInt previousB3LineNo)
		{
			var previousdeclaration = declaration.PreviousJob;
			if (!previousdeclaration.IsIM2)
			{
				return
					previousdeclaration.B3EntryHeader.AllEntryLines.Cast<CusEntryLine>()
						.FirstOrDefault(line => line.CL_LineNumber == previousB3LineNo);
			}
			else
			{
				return
					previousdeclaration.B3EntryHeader.AllEntryLines.Cast<CusEntryLine>()
						.FirstOrDefault(line => line.CA_B2LineNo == previousB3LineNo.ToString());
			}
		}

		ZInt GetMatchPoint(CusEntryLine line, CusEntryLine line2)
		{
			if (line == null || line2 == null)
			{
				return 0;
			}

			var invoiceLine = line.InvoiceLines.Cast<JobComInvoiceLine>().First();
			var invoiceLine2 = line2.InvoiceLines.Cast<JobComInvoiceLine>().First();
			ZInt result = 0;
			var emptyPage = new IM2AdjustmentsDocPage();
			var subHeaderStr1 = emptyPage.GetSubHeaderStr(invoiceLine);
			var subHeaderStr2 = emptyPage.GetSubHeaderStr(invoiceLine2);
			result += subHeaderStr1 == subHeaderStr2 ? 10 : 0;
			result += invoiceLine.JI_FormattedTariff == invoiceLine2.JI_FormattedTariff ? 1 : 0;
			result += invoiceLine.CA_99TariffCode == invoiceLine2.CA_99TariffCode ? 1 : 0;
			result += invoiceLine.CA_AuthorityNumber == invoiceLine2.CA_AuthorityNumber ? 1 : 0;
			result += invoiceLine.CA_ValueForDutyCode == invoiceLine2.CA_ValueForDutyCode ? 1 : 0;
			result += invoiceLine.CA_CalculationMethod == invoiceLine2.CA_CalculationMethod ? 1 : 0;
			result += invoiceLine.CA_TreatmentCode == invoiceLine2.CA_TreatmentCode ? 1 : 0;
			return result;
		}

		#endregion

		#region PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty

		protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();
			if (Declaration.IsImport)
			{
				Declaration.SetB3SubHeaderNumbers();
			}
		}

		internal static ZString GetTRMMergeKey(JobComInvoiceHeader header, JobComInvoiceLine line)
		{
			var sbuilder = new StringBuilder();
			var effectivePlaceOfExp = header.CA_TradeZone.IsEmpty ? line.EffectiveCountryAndStateOfExport : header.CA_TradeZone;

			sbuilder.Append(header.SupplierDocumentaryAddress.HumanReadableName);
			sbuilder.Append(header.SupplierDocumentaryAddress.AddressAsASingleLine);
			sbuilder.Append(header.CA_USPortOfExit);
			sbuilder.Append(header.JZ_ValuationDateOverride);
			sbuilder.Append(header.JZ_RX_NKInvoice_Currency);
			sbuilder.Append(header.CA_TimeLimit);
			sbuilder.Append(header.CA_TimeLimitCode);
			sbuilder.Append(effectivePlaceOfExp);
			sbuilder.Append(line.EffectiveTreatmentCode);
			sbuilder.Append(line.EffectiveCountryAndStateOfOrigin);
			sbuilder.Append(line.JI_RX_NKLinePriceCurr);
			return sbuilder.ToString();
		}

		#endregion

		#region GetLineNumberAssigner

		protected override ILineNumberAssigner GetLineNumberAssigner(Customs.Business.CusEntryHeader entryHeader)
		{
			return Declaration.IsImport && (((CusEntryHeader)entryHeader).IsB3CorCAD)
					? new B3LineNumberAssigner(entryHeader)
					: base.GetLineNumberAssigner(entryHeader);
		}

		#endregion

		#region Calculate Duties

		protected override void CalculateDuties()
		{
			base.CalculateDuties();
			if (Declaration.IsImport && Declaration.ShouldCalculateDutiesOnMerge)
			{
				var entryHeader = Declaration.B3EntryHeader;
				if (entryHeader != null)
				{
					foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
					{
						ResetFeesAmounts(entryLine);
						if (entryLine.InvoiceLines.Count == 1
							|| (Declaration.IsLVS
								&& (Declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation
									|| (entryLine.RandomLine.CA_AuthorityNumber.IsEmpty && !entryLine.RandomLine.IsRemissionLine))))
						{
							foreach (JobComInvoiceLine line in entryLine.InvoiceLines)
							{
								AddDutiesAndTaxes(entryLine, line.CA_CVforCurrConv, line.DutyAndTaxManager);
							}
						}
						else
						{
							var oldCustomsValue = entryLine.CL_CustomsValue;
							var manager = entryLine.PopulateDutiesAndTaxesReturningManager();
							AddDutiesAndTaxes(entryLine, entryLine.GetValueForCurrencyConversion(), manager);
							entryLine.ApportionRoundingAmountsOverLines();

							// Diagnostics for WI00199700
							var data = (IDutyAndTaxData)entryLine;
							var exchangeRate = data.ExchangeRate;
							var customsValue = entryLine.CL_CustomsValue;

							if (data.CalculationMethod != CalculationMethods.Codes.DeliveredDutyPaid
								&& !exchangeRate.IsEmpty && !customsValue.IsEmpty)
							{
								var valueForCurrencyConversion = data.ValueForCurrencyConversion;
								var vFCCMultiplyExchangeRate = Utilities.Round(valueForCurrencyConversion * exchangeRate, 2);
								var cVCFeeTotals = Utilities.Round(entryLine.Fees.GetAmount(EntryChargeTypeList.Codes.CustomsValueInInvoiceCurrency) * exchangeRate, 2);
								var isCalculated = manager.IsCalculated;

								if (customsValue != vFCCMultiplyExchangeRate)
								{
									ErrorReporter.ReportOnce("CustomsValueCalculationError",
									string.Format(CultureInfo.InvariantCulture, @"CL_CustomsValue not equals to ValueForCurrency * ExchangeRate, CL_CustomsValue: {0}, ValueForCurrencyConversion: {1}, ExchangeRate: {2}, Old Customs Value: {3}, Manager calculdated: {4}, ValueForCurrency * ExchangeRate: {5}",
										customsValue, valueForCurrencyConversion, exchangeRate, oldCustomsValue, isCalculated, vFCCMultiplyExchangeRate));
								}

								if (customsValue != cVCFeeTotals)
								{
									ErrorReporter.ReportOnce("CustomsValueCalculationError",
									string.Format(CultureInfo.InvariantCulture, @"CL_CustomsValue not equals to CVC Fee Totals, CL_CustomsValue: {0}, CVC Fee Totals: {1}, Old Customs Value: {2}, Manager calculdated: {3}",
										customsValue, cVCFeeTotals, oldCustomsValue, isCalculated));
								}
							}
						}
						entryLine.RefreshCalculatedFieldsForEntryLines();
					}
				}
			}
		}

		static void AddDutiesAndTaxes(CusEntryLine entryLine, ZDecimal valueForCurrConv, DutyAndTaxManager dutyAndTaxManager)
		{
			entryLine.Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.CustomsValueInInvoiceCurrency).CF_ChargeAmount += valueForCurrConv;
			entryLine.Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.CustomsValueForTax).CF_ChargeAmount += dutyAndTaxManager.NormalValueForTax;
			entryLine.Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.TotalDutyAmount).CF_ChargeAmount += dutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty);
			if (!dutyAndTaxManager.SIMADuties.Any() || IDutyAndTaxDataExtensions.IsSimaAmountPayable(dutyAndTaxManager.SIMADuties.First().C1_ExemptCode))
			{
				entryLine.Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.TotalSIMAAmount).CF_ChargeAmount += dutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.SIMADuty);
			}
			else
			{
				entryLine.Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount).CF_ChargeAmount += dutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.SIMADuty);
			}
			entryLine.Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.TotalExciseTaxAmount).CF_ChargeAmount += dutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.ExciseTax);
			if (entryLine.Declaration.IsGSTDirectPayment)
			{
				entryLine.Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.TotalGSTDirectAmount).CF_ChargeAmount += dutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.GST);
			}
			else
			{
				entryLine.Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.TotalGSTAmount).CF_ChargeAmount += dutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.GST);
			}

			var i = 1;
			foreach (var tax in dutyAndTaxManager.Duties.OrderBy(a => a, new DutyAndTaxComparer()))
			{
				entryLine.Fees.GetOrAddFeeByFeeType(new ZString(EntryChargeTypeList.Codes.TotalDutyAmount).Left(2) + i++).CF_ChargeAmount += tax.C1_Amount;
			}
		}

		static void ResetFeesAmounts(CusEntryLine line)
		{
			foreach (ICodeDescription type in new EntryChargeTypeList())
			{
				line.Fees.GetOrAddFeeByFeeType(type.Code).CF_ChargeAmount = 0;
			}
		}

		#endregion
	}
}
