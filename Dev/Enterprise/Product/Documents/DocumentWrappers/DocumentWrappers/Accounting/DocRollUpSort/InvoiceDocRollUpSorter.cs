using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper;
using Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort
{
	class InvoiceDocRollUpSorter : BaseDocRollUpSorter<ZString, MultilingualString, DocARBaseInvoice, DocARInvoiceLineCollection>
	{
		public InvoiceDocRollUpSorter(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docHeader, BusinessObjectFactory factory)
			: base(docHeader, factory)
		{
			DocLineRollUpper = docLineRollUpper;
		}

		DocARInvoiceCommon DocARInvoiceCommon => (DocARInvoiceCommon)DocHeader;

		override protected IComparer GetAlphabeticComparer() => new AlphabeticComparer();

		override protected IComparer GetSequenceSortComparer() => new SequenceSortComparer();

		override protected IComparer GetUserEnteredComparer() => new UserEnteredComparer();

		protected override BaseDocRollUpper<ZString, MultilingualString, DocARInvoiceLineCollection> GetChargeDocRollUpper(DocARBaseInvoice header, BusinessObjectFactory factory, DocARInvoiceLineCollection lines, ZString rollUpStyleId, ZString rollUpGroupId, params ZString[] chargeGroups)
			=> new InvoiceChargeDocRollUpper(DocLineRollUpper, header, factory, lines, rollUpStyleId, rollUpGroupId, chargeGroups);

		protected override BaseDocRollUpper<ZString, MultilingualString, DocARInvoiceLineCollection> GetChargeDocRollUpper(DocARBaseInvoice header, BusinessObjectFactory factory, DocARInvoiceLineCollection lines, ZString rollUpStyleId, Dictionary<ZString, ZString> chargeGroupToGroupIdMap)
			=> new InvoiceChargeDocRollUpper(DocLineRollUpper, header, factory, lines, rollUpStyleId, chargeGroupToGroupIdMap);

		protected override DocARInvoiceLineCollection GetNewLineList()
			=> DocARInvoiceLineCollection.New(DocARInvoiceCommon.InvoicingBase.Factory);

		protected override DocARInvoiceLineCollection GroupAndSortLinesCore(ZString display, ZString style, DocARInvoiceLineCollection lines)
		{
			var result = lines;
			DocARInvoiceLineCollection groupedLines = null;

			switch (display)
			{
				case OrgConstants.GroupOrSubTotalCharges.Code.SubTotal:
					IsSubTotal = true;
					groupedLines = GetLinesWithSubTotal(style);
					break;

				case OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence:
					IsSubTotal = false;
					groupedLines = GetLinesWithSubTotal(style);
					break;
			}

			return groupedLines;
		}

		#region Sub-Total Lines

		DocARInvoiceLineCollection GetLinesWithSubTotal(ZString rollUpCodeFromOrganisation)
		{
			var result = DocARInvoiceLineCollection.New(DocARInvoiceCommon.InvoicingBase.Factory);
			foreach (var group in TopLevelLineGroups)
			{
				result.AddRange(GetLinesWithSubTotal(group, rollUpCodeFromOrganisation));
			}

			return result;
		}

		public IEnumerable<DocARInvoiceLineCollection> TopLevelLineGroups
		{
			get
			{
				if (DocARInvoiceCommon.InvoiceType != DocARBaseInvoice.InvoiceTypeConsol || DocARInvoiceCommon.IsRollUpEntireConsol)
				{
					yield return DocARInvoiceCommon.Lines;
				}
				else
				{
					var groups = new Dictionary<ZString, DocARInvoiceLineCollection>();
					foreach (DocARInvoiceLine line in DocARInvoiceCommon.Lines)
					{
						DocARInvoiceLineCollection group = null;
						if (groups.ContainsKey(line.FKToShipment))
						{
							group = groups[line.FKToShipment];
						}
						else
						{
							group = DocARInvoiceLineCollection.New(DocARInvoiceCommon.InvoicingBase.Factory);
							groups.Add(line.FKToShipment, group);
						}

						group.Add(line);
					}

					foreach (var lines in groups.Values)
					{
						yield return lines;
					}
				}
			}
		}

		DocARInvoiceLineCollection GetLinesWithSubTotal(DocARInvoiceLineCollection lines, ZString rollUpCodeFromOrganistaion)
		{
			var newLines = DocARInvoiceLineCollection.New(Factory);

			switch (rollUpCodeFromOrganistaion)
			{
				case OrgConstants.InvoiceLineGroupings.Code.All:
					break;

				case OrgConstants.InvoiceLineGroupings.Code.CLC:
					newLines.AddRange(GetLinesWithSubTotalByCurrency(lines));
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OFD:
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFD, DocRollUpConstants.RollupAndSubTotalGroups.Origin), ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.Origin));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFD, DocRollUpConstants.RollupAndSubTotalGroups.FreightAndInsurance), ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFD, DocRollUpConstants.RollupAndSubTotalGroups.Destination), ChargeCodeGroupList.Codes.Destination, ChargeCodeGroupList.Codes.Unloading));
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OFO:
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFO, DocRollUpConstants.RollupAndSubTotalGroups.Origin), ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.Origin));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFO, DocRollUpConstants.RollupAndSubTotalGroups.FreightAndInsurance), ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance));
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OFF:
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Origin), ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.Origin));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Freight), ChargeCodeGroupList.Codes.Freight));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Insurance), ChargeCodeGroupList.Codes.Insurance));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Destination), ChargeCodeGroupList.Codes.Destination, ChargeCodeGroupList.Codes.Unloading));
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OFI:
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFI, DocRollUpConstants.RollupAndSubTotalGroups.Origin), ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.Origin));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFI, DocRollUpConstants.RollupAndSubTotalGroups.Freight), ChargeCodeGroupList.Codes.Freight));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OFI, DocRollUpConstants.RollupAndSubTotalGroups.Insurance), ChargeCodeGroupList.Codes.Insurance));
					break;

				case OrgConstants.InvoiceLineGroupings.Code.AEC:
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.AEC, DocRollUpConstants.RollupAndSubTotalGroups.OriginFreightInsuranceAndDestination), ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance, ChargeCodeGroupList.Codes.Unloading, ChargeCodeGroupList.Codes.Destination));
					break;

				case OrgConstants.InvoiceLineGroupings.Code.OandF:
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.OandF, DocRollUpConstants.RollupAndSubTotalGroups.OriginFreightAndInsurance), ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance));
					break;

				case OrgConstants.InvoiceLineGroupings.Code.ORF:
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.ORF, DocRollUpConstants.RollupAndSubTotalGroups.OriginAndFreight), ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Freight));
					break;

				case OrgConstants.InvoiceLineGroupings.Code.FandD:
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.FandD, DocRollUpConstants.RollupAndSubTotalGroups.FreightInsuranceAndDestination), ChargeCodeGroupList.Codes.Destination, ChargeCodeGroupList.Codes.Unloading, ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance));
					break;

				case OrgConstants.InvoiceLineGroupings.Code.CCG:
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Origin), ChargeCodeGroupList.Codes.Origin));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Loading), ChargeCodeGroupList.Codes.Loading));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Freight), ChargeCodeGroupList.Codes.Freight));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Insurance), ChargeCodeGroupList.Codes.Insurance));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Unloading), ChargeCodeGroupList.Codes.Unloading));
					newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Destination), ChargeCodeGroupList.Codes.Destination));
					var chargeCodeGroups = new ChargeCodeGroupList();
					var topCodes = new List<string>() { ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance, ChargeCodeGroupList.Codes.Unloading, ChargeCodeGroupList.Codes.Destination };
					foreach (ICodeDescription chargeCodeGroup in chargeCodeGroups)
					{
						if (!topCodes.Contains(chargeCodeGroup.Code))
						{
							newLines.AddRange(GetLinesWithSubTotal(lines, GetDescriptionByStyleAndGroup(OrgConstants.InvoiceLineGroupings.Code.CCG, chargeCodeGroup.Code), chargeCodeGroup.Code));
						}
					}
					break;

				case OrgConstants.InvoiceLineGroupings.Code.CCD:
					var uniqueChargeCodesInInvoice = GetUniqueChargeCodesInInvoice();
					foreach (var chargeCodePK in uniqueChargeCodesInInvoice)
					{
						var chargeCode = DocChargeCode.New(Factory, chargeCodePK);
						if (chargeCode != null)
						{
							newLines.AddRange(GetLinesWithSubTotal(lines, chargeCode));
						}
						else
						{
							var accGLHeader = Factory.Load<AccGLHeader>(chargeCodePK);
							if (accGLHeader != null)
							{
								newLines.AddRange(GetLinesWithSubTotal(lines, DocGLAccount.New(accGLHeader, Factory)));
							}
						}
					}
					break;
			}

			var tempLines = DocARInvoiceLineCollection.New(Factory);

			foreach (DocARInvoiceLine line in lines)
			{
				if (!line.HasBeenSubTotalled)
				{
					tempLines.Add(line);
				}
			}

			if (IsSubTotal)
			{
				tempLines.Sort((Comparison<DocARInvoiceLine>)DocLineSorter.CompareAlphabetically);
			}
			else
			{
				tempLines.Sort(GetSequenceSortComparer());
			}

			foreach (DocARInvoiceLine line in tempLines)
			{
				newLines.Add(line);
			}

			return newLines;
		}

		List<ZGuid> GetUniqueChargeCodesInInvoice()
		{
			var uniqueChargeCodesInInvoice = new List<ZGuid>();
			foreach (DocARInvoiceLine line in DocARInvoiceCommon.Lines)
			{
				if (line.ChargeCode != null)
				{
					if (!uniqueChargeCodesInInvoice.Contains(((BusinessObject)line.ChargeCode.WrappedObject).PK))
					{
						uniqueChargeCodesInInvoice.Add(((BusinessObject)line.ChargeCode.WrappedObject).PK);
					}
				}
				else if (line.GLAccount != null && !uniqueChargeCodesInInvoice.Contains(((BusinessObject)line.GLAccount.WrappedObject).PK))
				{
					uniqueChargeCodesInInvoice.Add(((BusinessObject)line.GLAccount.WrappedObject).PK);
				}
			}
			return uniqueChargeCodesInInvoice;
		}

		static MultilingualString GetDescriptionByStyleAndGroup(ZString styleId, ZString groupId) => AccountingConfigurationRegistry.Instance.InvoiceRollupAndGroupDescriptionRegistryItem.GetDescription(styleId, groupId);

		DocARInvoiceLineCollection GetLinesWithSubTotal(DocARInvoiceLineCollection lines, DocChargeCode chargeCode)
		{
			var linesForThisChargeCode = DocARInvoiceLineCollection.New(Factory);

			foreach (DocARInvoiceLine line in lines)
			{
				if (!line.IsSubTotalLine &&
						line.ChargeCode != null &&
						line.ChargeCode.Code == chargeCode.Code)
				{
					line.HasBeenSubTotalled = true;
					linesForThisChargeCode.Add(line);
				}
			}

			if (linesForThisChargeCode.Count > 0)
			{
				linesForThisChargeCode.Add(GetSubTotalLine(linesForThisChargeCode, (NoResString)chargeCode.Desc));
				linesForThisChargeCode.Add(GetSpacerLine(linesForThisChargeCode));
			}

			return linesForThisChargeCode;
		}

		DocARInvoiceLineCollection GetLinesWithSubTotal(DocARInvoiceLineCollection lines, DocGLAccount gLAccount)
		{
			var linesForThisGLAccount = DocARInvoiceLineCollection.New(Factory);

			foreach (DocARInvoiceLine line in lines)
			{
				if (!line.IsSubTotalLine &&
						line.GLAccount != null &&
						line.GLAccount.AccountNumber == gLAccount.AccountNumber)
				{
					line.HasBeenSubTotalled = true;
					linesForThisGLAccount.Add(line);
				}
			}

			if (linesForThisGLAccount.Count > 0)
			{
				linesForThisGLAccount.Add(GetSubTotalLine(linesForThisGLAccount, (NoResString)gLAccount.Description));
				linesForThisGLAccount.Add(GetSpacerLine(linesForThisGLAccount));
			}

			return linesForThisGLAccount;
		}

		DocARInvoiceLineCollection GetLinesWithSubTotal(DocARInvoiceLineCollection lines, MultilingualString subtotalLineDescription, params ZString[] chargeCodeGroups)
		{
			var linesForThisChargeGroup = DocARInvoiceLineCollection.New(Factory);

			foreach (DocARInvoiceLine line in lines)
			{
				if (!line.IsSubTotalLine &&
						line.ChargeCode != null &&
						((IList)chargeCodeGroups).Contains(line.ChargeCode.ChargeGroup))
				{
					line.HasBeenSubTotalled = true;
					linesForThisChargeGroup.Add(line);
				}
			}

			if (IsSubTotal)
			{
				linesForThisChargeGroup.Sort((Comparison<DocARInvoiceLine>)DocLineSorter.CompareAlphabetically);
			}
			else
			{
				linesForThisChargeGroup.Sort(GetSequenceSortComparer());
			}

			if (linesForThisChargeGroup.Count > 0)
			{
				linesForThisChargeGroup.Add(GetSubTotalLine(linesForThisChargeGroup, subtotalLineDescription));
				linesForThisChargeGroup.Add(GetSpacerLine(linesForThisChargeGroup));
			}

			return linesForThisChargeGroup;
		}

		DocARInvoiceLineCollection GetLinesWithSubTotalByCurrency(DocARInvoiceLineCollection lines)
		{
			var result = new DocARInvoiceLineCollection(Factory);

			var currencyCodes = GetCurrencyCodeList(lines);

			foreach (var currencyCode in currencyCodes)
			{
				result.AddRange(GetLinesWithSubTotalByCurrency(lines, currencyCode));
			}
			return result;
		}

		List<String> GetCurrencyCodeList(DocARInvoiceLineCollection lines)
		{
			var result = new List<string>();

			foreach (DocARInvoiceLine line in lines)
			{
				if (!result.Contains(line.ChargeCurrency))
				{
					result.Add(line.ChargeCurrency);
				}
			}
			return result;
		}

		DocARInvoiceLineCollection GetLinesWithSubTotalByCurrency(DocARInvoiceLineCollection lines, string currencyCode)
		{
			var result = new DocARInvoiceLineCollection(Factory);
			var subTotalLine = DocARInvoiceLineForRollUp.New(Factory);

			foreach (DocARInvoiceLine line in lines)
			{
				if (line.ChargeCurrency == currencyCode)
				{
					subTotalLine.OSExTaxAmount += line.OSExTaxAmount;
					subTotalLine.GSTVAT += line.GSTVAT;
					subTotalLine.LineAmount += line.LineAmount;
					subTotalLine.OSTaxAmount += line.OSTaxAmount;
					subTotalLine.ChargeOSAmount += line.ChargeOSAmount;
					subTotalLine.ChargeOSAmountForCLC += line.ChargeOSAmountForCLC;

					line.HasBeenSubTotalled = true;
					result.Add(line);
				}
			}

			if (result.Count > 0)
			{
				if (!IsSubTotal)
				{
					result.Sort(GetSequenceSortComparer());
				}

				subTotalLine.Currency = result[0].Currency;
				subTotalLine.ChargeCurrency = currencyCode;
				subTotalLine.SetLineDescription(new ZString(currencyCode));
				subTotalLine.IsSubTotalLine = true;
				subTotalLine.OSTaxDisplay = subTotalLine.FormatOSTaxWithNoCurrencySymbolFormat();
				subTotalLine.TaxAmountDisplay = "";
				subTotalLine.ChargeExchangeRate = result[0].ChargeExchangeRate;
				DocLineRollUpper.PrepareRollUpLineForGrouping(subTotalLine, result);
				result.Add(subTotalLine);
				result.Add(GetSpacerLine(result));
			}

			return result;
		}

		DocARInvoiceLineForRollUp GetSubTotalLine(DocARInvoiceLineCollection lines, MultilingualString subtotalLineDescription)
		{
			var subTotalLine = DocARInvoiceLineForRollUp.New(Factory);

			foreach (DocARInvoiceLine line in lines)
			{
				subTotalLine.OSExTaxAmount += line.OSExTaxAmount;
				subTotalLine.GSTVAT += line.GSTVAT;
				subTotalLine.LineAmount += line.LineAmount;
				subTotalLine.OSTaxAmount += line.OSTaxAmount;
			}

			subTotalLine.SetLineDescription(subtotalLineDescription);
			subTotalLine.IsSubTotalLine = true;
			subTotalLine.OSTaxDisplay = "";
			subTotalLine.TaxAmountDisplay = "";
			DocLineRollUpper.PrepareRollUpLineForGrouping(subTotalLine, lines);

			return subTotalLine;
		}

		DocARInvoiceLineForRollUp GetSpacerLine(DocARInvoiceLineCollection lines)
		{
			var blankLineAfterSubTotal = DocARInvoiceLineForRollUp.New(Factory);
			blankLineAfterSubTotal.IsSpacerLine = true;
			blankLineAfterSubTotal.OSTaxDisplay = "";
			blankLineAfterSubTotal.TaxAmountDisplay = "";
			DocLineRollUpper.PrepareRollUpLineForGrouping(blankLineAfterSubTotal, lines);
			return blankLineAfterSubTotal;
		}

		BaseInvoiceDocLineRollUpper DocLineRollUpper { get; }

		#endregion

		bool IsSubTotal { get; set; }

		protected override DocARInvoiceLineCollection GetLinesForRollUpCore(DocARInvoiceLineCollection lines, ZString rollUpCodeFromOrganistaion)
		{
			DocARInvoiceLineCollection result = null;
			var listOfCharges = new List<ZString>();
			var mapping = new Dictionary<ZString, ZString>();

			var docARInvoiceLineCollection = lines;

			switch (rollUpCodeFromOrganistaion)
			{
				case OrgConstants.InvoiceLineGroupings.Code.CCD:
					if (AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule == AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code)
					{
						var rollupGrouperCCD = new RollupGrouperCCD(DocLineRollUpper, DocARInvoiceCommon, DocARInvoiceCommon.InvoicingBase.Factory, docARInvoiceLineCollection);
						result = rollupGrouperCCD.RollUp();
					}
					else
					{
						var rollupGrouperCCDWithTaxRate = new RollupGrouperCCDWithTaxRate(DocLineRollUpper, DocARInvoiceCommon, DocARInvoiceCommon.InvoicingBase.Factory, docARInvoiceLineCollection);
						result = rollupGrouperCCDWithTaxRate.RollUp();
					}
					break;
				case OrgConstants.InvoiceLineGroupings.Code.CLC:
					var docARInvoice = DocHeader;
					result = new RollupGrouperCLC(DocLineRollUpper, docARInvoice, DocARInvoiceCommon.InvoicingBase.Factory, docARInvoiceLineCollection).RollUp();
					break;
			}

			return result;
		}

		class SequenceSortComparer : IComparer
		{
			public int Compare(object docLine1, object docLine2)
			{
				if ((docLine1 is DocARInvoiceLine && docLine2 is DocARInvoiceLine)
					|| (docLine1 is DocARInvoiceLineForRollUp && docLine2 is DocARInvoiceLineForRollUp))
				{
					return DocLineSorter.CompareBySequence(docLine1 as ISortableDocLine, docLine2 as ISortableDocLine);
				}

				if (docLine1 is DocARInvoiceLine && docLine2 is DocARInvoiceLineForRollUp)
				{
					return 1;
				}

				if (docLine1 is DocARInvoiceLineForRollUp && docLine2 is DocARInvoiceLine)
				{
					return -1;
				}

				return 0;
			}
		}

		class UserEnteredComparer : IComparer
		{
			public int Compare(object docLine1, object docLine2)
			{
				if ((docLine1 is DocARInvoiceLine && docLine2 is DocARInvoiceLine) || (docLine1 is DocARInvoiceLineForRollUp && docLine2 is DocARInvoiceLineForRollUp))
				{
					return DocLineSorter.CompareByUserEntered((ISortableDocLine)docLine1, (ISortableDocLine)docLine2);
				}

				if (docLine1 is DocARInvoiceLine && docLine2 is DocARInvoiceLineForRollUp)
				{
					return 1;
				}

				if (docLine1 is DocARInvoiceLineForRollUp && docLine2 is DocARInvoiceLine)
				{
					return -1;
				}

				return 0;
			}
		}

		class AlphabeticComparer : IComparer
		{
			public int Compare(object docLine1, object docLine2)
				=> DocLineSorter.CompareAlphabetically((ISortableDocLine)docLine1, (ISortableDocLine)docLine2);
		}
	}
}
