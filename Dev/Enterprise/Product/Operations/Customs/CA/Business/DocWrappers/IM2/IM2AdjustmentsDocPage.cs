using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;

namespace Enterprise.Customs.CA.Business
{
	public class IM2AdjustmentsDocPage : AdjustmentsDocPage
	{
		public IM2AdjustmentsDocPage() : base()
		{
		}

		protected override AdjustmentsDocPage CreateNewPage()
		{
			return new IM2AdjustmentsDocPage();
		}

		protected override AdjustmentsDocLine CreateNewLine(bool isEmpty = false)
		{
			return new IM2AdjustmentsDocLine(isEmpty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override IEnumerable<AdjustmentsDocPage> GetPages(JobComInvoiceHeader subHeader, JobDeclaration declaration = null)
		{
			var splitLineStr = JobComInvoiceLine.SplitLine;
			var previousDeclaration = declaration.PreviousJob;
			if (previousDeclaration == null || declaration.B3EntryHeader == null)
			{
				return new List<AdjustmentsDocPage>();
			}

			var subHeaderPageDic = new Dictionary<ZInt, List<AccountAndClaimPair>>();
			var onlyPlaceModifyPageDic = new Dictionary<ZInt, List<AccountAndClaimPair>>();
			var newSubHeaderPageDic = new Dictionary<ZInt, Dictionary<ZString, List<AccountAndClaimPair>>>();

			var previousEntriesLines = previousDeclaration.B3EntryHeader.AllEntryLines.Cast<CusEntryLine>();

			var subHeaderDic = GetSubHeaderDic(previousEntriesLines);
			var previousEntryLines = previousEntriesLines.Where(line => !line.CA_B2LineNo.EndsWith(splitLineStr, StringComparison.OrdinalIgnoreCase)).OrderBy(line => line.CL_LineNumber);
			var newEntryLines = declaration.B3EntryHeader.AllEntryLines;

			foreach (var previousEntryLine in previousEntryLines)
			{
				var matchKey = previousDeclaration.IsIM2 ? previousEntryLine.CA_B2LineNo.ToString() : previousEntryLine.CL_LineNumber.ToString();
				var previousRandomLine = previousEntryLine.RandomLine;

				if (previousRandomLine is JobComInvoiceLine previousInvoiceLine)
				{
					var newMatchedEntryLines = newEntryLines.Where(line => line.CA_B2LineNo == matchKey || line.CA_B2LineNo == (matchKey + splitLineStr));
					var subHeaderNo = previousDeclaration.IsIM2 ? previousEntryLine.CA_B2SubHeader : previousInvoiceLine.CA_B3SubHeaderNumber;
					if (newMatchedEntryLines.Any())
					{
						var accountAdded = false;
						foreach (var matchedEntryLine in newMatchedEntryLines)
						{
							var newRelevantLine = matchedEntryLine.RandomLine;
							if (newRelevantLine is JobComInvoiceLine invoiceLine)
							{
								if (MatchSubHeader(subHeaderDic, invoiceLine))
								{
									if (matchedEntryLine.CA_B2LineNo.EndsWith(splitLineStr, StringComparison.OrdinalIgnoreCase))
									{
										var entryLine = accountAdded ? null : newMatchedEntryLines.FirstOrDefault(line => line.CA_B2LineNo == matchKey.Replace(splitLineStr, "") && MatchSubHeader(subHeaderDic, (JobComInvoiceLine)line.InvoiceLines.FirstOrDefault()));
										AddAdditionalPairsForSplitLines(ref accountAdded, subHeaderPageDic, subHeaderNo, previousEntryLine, entryLine);
										AddToDictionary(subHeaderPageDic, subHeaderNo, null, matchedEntryLine);
									}
									else
									{
										if (!accountAdded && (!MatchEntryLine(previousEntryLine, matchedEntryLine) || !MatchEntryValue(previousEntryLine, matchedEntryLine)))
										{
											AddToDictionary(subHeaderPageDic, subHeaderNo, previousEntryLine, matchedEntryLine);
											accountAdded = true;
										}
									}
								}
								else
								{
									var subHeaderStr = GetSubHeaderStr(invoiceLine);
									if (GetSubHeaderStr(previousInvoiceLine, false) == GetSubHeaderStr(invoiceLine, false) &&
										(previousInvoiceLine.EffectiveCountryAndStateOfOrigin != invoiceLine.EffectiveCountryAndStateOfOrigin ||
										 previousInvoiceLine.EffectiveTreatmentCode != invoiceLine.EffectiveTreatmentCode ||
										 previousInvoiceLine.EffectiveCountryAndStateOfExport != invoiceLine.EffectiveCountryAndStateOfExport) &&
										newMatchedEntryLines.All(x => x.CA_B2LineNo != matchKey.Replace(splitLineStr, "") + splitLineStr) &&
										(newSubHeaderPageDic.ContainsKey(subHeaderNo) && !newSubHeaderPageDic[subHeaderNo].ContainsKey(subHeaderStr) ||
										 !newSubHeaderPageDic.ContainsKey(subHeaderNo)))
									{
										AddToDictionary(subHeaderPageDic, subHeaderNo, null, null);
										AddToDictionary(onlyPlaceModifyPageDic, subHeaderNo, previousEntryLine, matchedEntryLine);
									}
									else if ((!subHeaderDic.ContainsKey(subHeaderStr) || (subHeaderDic.ContainsKey(subHeaderStr) && subHeaderDic[subHeaderStr] != invoiceLine.CA_PreviousB3SubHeaderNo))
									&& !newSubHeaderPageDic.Any(x => x.Value.ContainsKey(subHeaderStr)))
									{
										AddToDictionary(subHeaderPageDic, subHeaderNo, null, null);
										AddToDictionary(onlyPlaceModifyPageDic, subHeaderNo, previousEntryLine, matchedEntryLine);
									}
									else
									{
										if (matchedEntryLine.CA_B2LineNo.EndsWith(splitLineStr, StringComparison.OrdinalIgnoreCase))
										{
											var entryLine = accountAdded
												? null
												: newEntryLines.FirstOrDefault(
													line =>
														line.CA_B2LineNo == matchKey.Replace(splitLineStr, "") &&
														MatchSubHeader(subHeaderDic, line.RandomLine));
											AddAdditionalPairsForSplitLines(ref accountAdded, subHeaderPageDic, subHeaderNo, previousEntryLine, entryLine);
											AddPairsForPerSubHeader(invoiceLine, subHeaderDic, subHeaderPageDic, newSubHeaderPageDic, subHeaderNo,
												matchedEntryLine);
										}
										else
										{
											AddAdditionalPairsForSplitLines(ref accountAdded, subHeaderPageDic, subHeaderNo, previousEntryLine, null);
											AddPairsForPerSubHeader(invoiceLine, subHeaderDic, subHeaderPageDic, newSubHeaderPageDic, subHeaderNo,
												matchedEntryLine);
										}
									}
								}
							}
						}
					}
					else
					{
						AddToDictionary(subHeaderPageDic, subHeaderNo, previousEntryLine, null);
					}
				}
			}
			return GetDocPages(declaration, previousEntryLines, subHeaderPageDic, onlyPlaceModifyPageDic, newSubHeaderPageDic);
		}

		void AddPairsForPerSubHeader(JobComInvoiceLine invoiceLine, Dictionary<string, int> subHeaderDic, Dictionary<ZInt, List<AccountAndClaimPair>> subHeaderPageDic, Dictionary<ZInt, Dictionary<ZString, List<AccountAndClaimPair>>> newSubHeaderPageDic, ZInt subHeaderNo, CusEntryLine matchedEntryLine)
		{
			var newSubHeaderStr = GetSubHeaderStr(invoiceLine);
			if (subHeaderDic.ContainsKey(newSubHeaderStr))
			{
				AddToDictionary(subHeaderPageDic, subHeaderDic[newSubHeaderStr], null, matchedEntryLine);
			}
			else
			{
				AddToNewDictionary(newSubHeaderPageDic, subHeaderNo, newSubHeaderStr, matchedEntryLine);
			}
		}

		void AddAdditionalPairsForSplitLines(ref bool accountAdded, Dictionary<ZInt, List<AccountAndClaimPair>> subHeaderPageDic, ZInt subHeaderNo, CusEntryLine previousEntryLine, CusEntryLine entryLine)
		{
			if (!accountAdded)
			{
				AddToDictionary(subHeaderPageDic, subHeaderNo, previousEntryLine, entryLine);
				accountAdded = true;
			}
		}

		List<AdjustmentsDocPage> GetDocPages(JobDeclaration declaration, IOrderedEnumerable<CusEntryLine> previousEntryLines, Dictionary<ZInt, List<AccountAndClaimPair>> subHeaderPageDic, Dictionary<ZInt, List<AccountAndClaimPair>> onlyPlaceModifyPageDic, Dictionary<ZInt, Dictionary<ZString, List<AccountAndClaimPair>>> newSubHeaderPageDic)
		{
			var docPages = new List<AdjustmentsDocPage>();
			var newInvoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();

			foreach (var key in subHeaderPageDic.Keys)
			{
				if (onlyPlaceModifyPageDic.TryGetValue(key, out var accountAndClaimPairs) && subHeaderPageDic[key].Count == 0)
				{
					var entryLine = newInvoiceLines.FirstOrDefault(x => x.CA_PreviousB3SubHeaderNo == key)?.B3EntryLine;
					if (entryLine == null)
					{
						var originalLineNo = accountAndClaimPairs.FirstOrDefault()?.ClaimLine.OriginalLineNo ?? ZString.Empty;
						entryLine = declaration.B3EntryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CA_B2LineNo == originalLineNo);
					}

					docPages.AddRange(GetDocPages(accountAndClaimPairs, key, entryLine, false));
				}
				else
				{
					var entryLine = previousEntryLines.FirstOrDefault(x => ((IClassificationLine1)x).B3SubHeaderNumber == key);
					docPages.AddRange(GetDocPages(subHeaderPageDic[key], key, entryLine, false));
				}

				if (newSubHeaderPageDic.ContainsKey(key))
				{
					var dic = newSubHeaderPageDic[key];
					foreach (var subkey in dic.Keys)
					{
						var invoiceLine = newInvoiceLines.First(x => GetSubHeaderStr(x) == subkey);
						docPages.AddRange(GetDocPages(dic[subkey], key, invoiceLine.B3EntryLine, true));
					}
				}
			}
			return docPages;
		}

		internal Dictionary<string, int> GetSubHeaderDic(IEnumerable<IClassificationLine1> previousEntryLines)
		{
			var groupedEntry = previousEntryLines.GroupBy(x => x.B3SubHeaderNumber).OrderBy(x => x.Key);
			var result = new Dictionary<string, int>();

			foreach (var group in groupedEntry)
			{
				var subHeaderStr = GetSubHeaderStr(group.FirstOrDefault()?.RelevantLine as JobComInvoiceLine);
				if (!result.ContainsKey(subHeaderStr))
				{
					result.Add(subHeaderStr, group.Key);
				}
			}
			return result;
		}

		internal string GetSubHeaderStr(JobComInvoiceLine invoice, bool includePlace = true)
		{
			var header = invoice.InvoiceHeader;
			var mergeBy = header?.JobDeclaration?.CA_MergeBy ?? ZString.Empty;
			var sbuilder = new StringBuilder();
			if (mergeBy == B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices)
			{
				sbuilder.Append(LineMerger.GetTRMMergeKey(header, invoice));
			}
			else
			{
				if (includePlace)
				{
					sbuilder.Append(invoice.EffectiveCountryAndStateOfOrigin);
					sbuilder.Append(invoice.EffectiveCountryAndStateOfExport);
				}
				sbuilder.Append(invoice.EffectiveTreatmentCode);
				if (header != null)
				{
					sbuilder.Append(header.JZ_ValuationDateOverride);
					sbuilder.Append(header.JZ_RX_NKInvoice_Currency);
					sbuilder.Append(header.CA_TimeLimit);
					sbuilder.Append(header.CA_TimeLimitCode);
					sbuilder.Append(header.JZ_InvoiceNumber);
				}
			}
			return sbuilder.ToString();
		}

		void AddToDictionary(Dictionary<ZInt, List<AccountAndClaimPair>> dic, ZInt subHeaderNumber, CusEntryLine previousEntryLineLine, CusEntryLine newEntryLine)
		{
			//As Claimed Split Line should not has As Accounted Line if we already has an As Accounted Line with the same OriginalLineNo.
			var b2LineNo = newEntryLine?.CA_B2LineNo ?? ZString.Empty;
			if (b2LineNo.EndsWith(JobComInvoiceLine.SplitLine))
			{
				var b2LineNoWithoutSuffix = b2LineNo.Substring(0, b2LineNo.Length - JobComInvoiceLine.SplitLine.Length);
				if (dic.Any(x => x.Value.Any(y => y.AccountLine.OriginalLineNo == b2LineNoWithoutSuffix)))
				{
					previousEntryLineLine = null;
				}
			}

			var pairList = GetAccountAndClaimPairList(previousEntryLineLine, newEntryLine);
			if (dic.ContainsKey(subHeaderNumber))
			{
				dic[subHeaderNumber].AddRange(pairList);
			}
			else
			{
				dic.Add(subHeaderNumber, pairList);
			}
		}

		void AddToNewDictionary(Dictionary<ZInt, Dictionary<ZString, List<AccountAndClaimPair>>> dic, ZInt subHeaderNumber, ZString subHeaderZString, CusEntryLine newEntryLine)
		{
			var pairList = GetAccountAndClaimPairList(null, newEntryLine);
			if (dic.ContainsKey(subHeaderNumber))
			{
				var subDic = dic[subHeaderNumber];
				if (subDic.ContainsKey(subHeaderZString))
				{
					subDic[subHeaderZString].AddRange(pairList);
				}
				else
				{
					subDic.Add(subHeaderZString, pairList);
				}
			}
			else
			{
				var subdic = new Dictionary<ZString, List<AccountAndClaimPair>> { { subHeaderZString, pairList } };
				dic.Add(subHeaderNumber, subdic);
			}
		}

		List<IM2AdjustmentsDocPage> GetDocPages(List<AccountAndClaimPair> pairList, ZInt subHeaderNumber, CusEntryLine entryLine, bool isNewSubHeader)
		{
			var result = new List<IM2AdjustmentsDocPage>();
			var emptyLine = CreateNewLine() as IM2AdjustmentsDocLine;
			var emptyPair = new AccountAndClaimPair(emptyLine, emptyLine);
			if (pairList.Any())
			{
				var firstPair = pairList.Count > 0 ? pairList[0] : emptyPair;
				var secondPair = pairList.Count > 1 ? pairList[1] : emptyPair;
				result.Add(GetFirstPage(entryLine, firstPair, secondPair, subHeaderNumber, isNewSubHeader));

				int i = 2;
				while (i < pairList.Count)
				{
					firstPair = pairList[i];
					secondPair = pairList.Count > i + 1 ? pairList[i + 1] : emptyPair;
					result.Add(GetSubSequentPage(null, firstPair, secondPair) as IM2AdjustmentsDocPage);
					i += 2;
				}
				pairList.Clear();
			}
			return result;
		}

		List<AccountAndClaimPair> GetAccountAndClaimPairList(CusEntryLine accountEntryLine, CusEntryLine claimEntryLine)
		{
			var pairList = new List<AccountAndClaimPair>();
			var emptyLine = CreateNewLine() as IM2AdjustmentsDocLine;
			var docAccountLines = emptyLine.GetLinesOrderedByDuty(accountEntryLine, false).ToList();
			var claimLine = emptyLine.GetLinesOrderedByDuty(claimEntryLine, true).ToList();
			int p = 0;
			while (p < docAccountLines.Count || p < claimLine.Count)
			{
				var docAccountLine = p < docAccountLines.Count ? docAccountLines[p] : emptyLine;
				var docClaimLine = p < claimLine.Count ? claimLine[p] : emptyLine;
				p++;
				pairList.Add(new AccountAndClaimPair(docAccountLine, docClaimLine));
			}
			return pairList;
		}

		bool MatchSubHeader(Dictionary<string, int> dic, JobComInvoiceLine line)
		{
			if (line == null)
			{
				return false;
			}
			var subHeaderStr = GetSubHeaderStr(line);
			return dic.ContainsKey(subHeaderStr) && dic[subHeaderStr] == line.CA_PreviousB3SubHeaderNo;
		}

		bool MatchEntryLine(CusEntryLine previousLine, CusEntryLine newLine)
		{
			var previousInvoiceLine = previousLine.RandomLine;
			var newInvoiceLine = newLine.RandomLine;

			if (previousInvoiceLine == null || newInvoiceLine == null)
			{
				return false;
			}

			var result = GetSubHeaderStr(previousInvoiceLine) == GetSubHeaderStr(newInvoiceLine);

			result &= previousLine.Description == newLine.Description
				&& previousInvoiceLine.CA_AuthorityNumber == newInvoiceLine.CA_AuthorityNumber
				&& previousInvoiceLine.CA_99TariffCode == newInvoiceLine.CA_99TariffCode
				&& previousInvoiceLine.JI_Tariff == newInvoiceLine.JI_Tariff
				&& previousInvoiceLine.JI_CustomsUnitQty == newInvoiceLine.JI_CustomsUnitQty
				&& previousInvoiceLine.JI_CustomsSecondUnitQty == newInvoiceLine.JI_CustomsSecondUnitQty
				&& previousInvoiceLine.JI_CustomsThirdUnitQty == newInvoiceLine.JI_CustomsThirdUnitQty
				&& previousInvoiceLine.EffectiveValueForDutyCode == newInvoiceLine.EffectiveValueForDutyCode;

			if (result)
			{
				var previousSima = previousInvoiceLine.DutyAndTaxManager.SIMADuties.FirstOrDefault();
				var newSima = newInvoiceLine.DutyAndTaxManager.SIMADuties.FirstOrDefault();

				if (previousSima != null && newSima != null)
				{
					result &= previousSima.C1_ExemptCode == newSima.C1_ExemptCode;
				}
				else
				{
					result &= previousSima == null && newSima == null;
				}

				result &= GetTaxCompareString(previousInvoiceLine) == GetTaxCompareString(newInvoiceLine);
			}
			return result;
		}

		ZString GetTaxCompareString(JobComInvoiceLine invoiceLine)
		{
			var previousStringBuilder = new ZStringBuilder();
			foreach (var tax in invoiceLine.DutiesAndTaxes.Where(a => a.IsTax).OrderBy(a => a, new DutyAndTaxComparer()))
			{
				previousStringBuilder.Append(tax.C1_Code);
				previousStringBuilder.Append(tax.C1_ExemptCode);
			}
			return previousStringBuilder.ToString();
		}

		bool MatchEntryValue(CusEntryLine previousLine, CusEntryLine newLine)
		{
			var previousFees = previousLine.Fees;
			var newFees = newLine.Fees;

			var result = previousLine.TotalLinePriceInLocalCurrency == newLine.TotalLinePriceInLocalCurrency
				   && previousLine.CustomsQuantity == newLine.CustomsQuantity
				   && previousLine.CustomsUnitQty == newLine.CustomsUnitQty
				   && previousLine.CL_CustomsValue == newLine.CL_CustomsValue
				   && previousLine.DutyAmount == newLine.DutyAmount
				   && previousLine.CL_DutyPercent == newLine.CL_DutyPercent
				   && previousFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalDutyAmount) == newFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalDutyAmount)
				   && previousFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalSIMAAmount) == newFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalSIMAAmount)
				   && previousFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount) == newFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount)
				   && previousFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalExciseTaxAmount) == newFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalExciseTaxAmount);

			if (result)
			{
				var b3GST = previousFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalGSTAmount);
				if (b3GST.IsEmpty)
				{
					b3GST = previousFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalGSTDirectAmount);
				}
				var b2GST = newFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalGSTAmount);
				if (b2GST.IsEmpty)
				{
					b2GST = newFees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalGSTDirectAmount);
				}
				result = b3GST == b2GST;
			}

			return result;
		}

		IM2AdjustmentsDocPage GetFirstPage(CusEntryLine cusEntryLine, AccountAndClaimPair pair1, AccountAndClaimPair pair2, ZInt subHeaderNumber, bool isNewSubHeader)
		{
			var result = new IM2AdjustmentsDocPage();
			var invoiceLine = (JobComInvoiceLine)cusEntryLine?.InvoiceLines.FirstOrDefault();
			if (invoiceLine != null)
			{
				var invoiceHeader = invoiceLine.InvoiceHeader;
				result.SubHeaderNo = isNewSubHeader ? "NS" : subHeaderNumber.ToString();
				result.CountryOfOrigin = invoiceLine.EffectiveCountryAndStateOfOrigin;
				result.PlaceOfExport = invoiceLine.EffectiveCountryAndStateOfExport;
				result.TariffTreatment = invoiceLine.CA_TreatmentCode;

				if (invoiceHeader != null)
				{
					var directShipmentDate = invoiceHeader.JZ_ValuationDateOverride;
					if (directShipmentDate.IsValid)
					{
						result.DirectShipmentMonth = directShipmentDate.ToString("MM", CultureInfo.InvariantCulture);
						result.DirectShipmentDay = directShipmentDate.ToString("dd", CultureInfo.InvariantCulture);
						result.DirectShipmentYear = directShipmentDate.ToString("yyyy", CultureInfo.InvariantCulture);
					}
					result.CurrencyCode = invoiceHeader.JZ_RX_NKInvoice_Currency;
					result.TimeLimit = invoiceHeader.CA_TimeLimit.IsEmpty ? string.Empty : invoiceHeader.CA_TimeLimit.ToString();
					result.TimeCode = invoiceHeader.CA_TimeLimit.IsEmpty ? ZString.Empty : invoiceHeader.CA_TimeLimitCode;
				}
				if (pair1 != null)
				{
					result.AsAccountForDocLine1 = pair1.AccountLine;
					result.AsClaimForDocLine1 = pair1.ClaimLine;
				}
				if (pair2 != null)
				{
					result.AsAccountForDocLine2 = pair2.AccountLine;
					result.AsClaimForDocLine2 = pair2.ClaimLine;
				}
			}
			return result;
		}
	}
}
