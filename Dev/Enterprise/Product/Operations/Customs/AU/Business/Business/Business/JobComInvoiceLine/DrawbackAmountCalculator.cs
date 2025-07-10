using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class DrawbackAmountCalculator
	{
		public DrawbackAmountCalculator(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}
		readonly JobComInvoiceLine invoiceLine;
		public ZDecimal CalculatedCustomsValue;
		public ZDecimal CalculatedDutyAmount;
		public ZDecimal DutyRate;
		ZDecimal decQty;
		ZDecimal decCustomsValue;
		ZDecimal decDutyAmount;
		ProRataAmounts proRataAmounts;
		IDrawbackEntryLine drawbackImportEntryLine;

		public void Calculate()
		{
			CalculatedCustomsValue = 0m;
			CalculatedDutyAmount = 0m;
			decQty = 0;
			decCustomsValue = 0;
			decDutyAmount = 0;
			proRataAmounts = new ProRataAmounts();
			drawbackImportEntryLine = invoiceLine.DrawbackImportEntryLine;
			int drawbackCusEntryLineCollectionCount = invoiceLine.DrawbackCusEntryLineCollection.Count;
			if (drawbackCusEntryLineCollectionCount > 0)
			{
				foreach (CusEntryLine entryLine in invoiceLine.DrawbackCusEntryLineCollection)
				{
					if (entryLine.DrawbackClaimQuantity <= 0m)
					{
						drawbackCusEntryLineCollectionCount -= 1;
					}
				}
			}
			if (invoiceLine.IsBOMLineExpanded)
			{
				// return zero for lines with expanded BOMs
			}
			else if (drawbackImportEntryLine != null && (invoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.ActualShipment ||
					(invoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment && drawbackCusEntryLineCollectionCount == 0)))
			{
				GetValuesWhenSpecificImportDecSupplied(drawbackImportEntryLine);
			}
			else if (invoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment &&
								drawbackCusEntryLineCollectionCount > 0)
			{
				GetValuesForRepresentativeShipmentAverageCalc();
			}
			else if (invoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment &&
					 invoiceLine.Part != null)
			{
				GetValuesForRepresentativeShipmentBestFit();
			}
			else if (invoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.Imputation)
			{
				GetValuesForImputationMethod();
			}
		}

		void GetValuesForRepresentativeShipmentAverageCalc()
		{
			invoiceLine.AddInfo.ZA_DDN_Hidden = ZString.Empty;
			invoiceLine.AddInfo.ZA_DDL_Hidden = ZInt.Zero;
			DutyRate = -1;
			foreach (CusEntryLine line in invoiceLine.DrawbackCusEntryLineCollection)
			{
				var drawbackAmounts = DrawbackAmountsForLinePart(line, invoiceLine);
				if (drawbackAmounts.Quantity > 0)
				{
					proRataAmounts.Calculate(line.DrawbackClaimQuantity, drawbackAmounts.Quantity, drawbackAmounts.CustomsValue, drawbackAmounts.DutyAmount);
					decQty += line.DrawbackClaimQuantity;
					decCustomsValue += proRataAmounts.CalculatedCustomsValue;
					decDutyAmount += proRataAmounts.CalculatedDutyAmount;
					if (DutyRate != line.CL_DutyPercent)
					{
						if (DutyRate == -1)
						{
							DutyRate = line.CL_DutyPercent;
						}
						else
						{
							DutyRate = 0m;
						}
					}
				}
			}
			proRataAmounts.Calculate(invoiceLine.DrawbackClaimQuantity, decQty, decCustomsValue, decDutyAmount);
			CalculatedCustomsValue = proRataAmounts.CalculatedCustomsValue;
			CalculatedDutyAmount = proRataAmounts.CalculatedDutyAmount;
		}

		void GetValuesForRepresentativeShipmentBestFit()
		{
			var earliestInvoice = invoiceLine.Declaration.Invoices.EarliestInvoice;
			var startDate = earliestInvoice != null && earliestInvoice.JZ_InvoiceDate.IsValid ? earliestInvoice.JZ_InvoiceDate : ZDateTime.Now;
			int daysAgo = CustomsDataRegistry.Instance.MaximumAgeforRepresentativeShipmentCalculationMethod.Value;
			startDate = startDate.AddDays(daysAgo > 0 ? -daysAgo : -365);
			var latestInvoice = invoiceLine.Declaration.Invoices.LatestInvoice;
			var endDate = latestInvoice != null && latestInvoice.JZ_InvoiceDate.IsValid ? latestInvoice.JZ_InvoiceDate : ZDateTime.Now;
			endDate = endDate.AddDays(-1);

			var entryLinesForPartFiler = new ZQuery();
			var cusEntryLineQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			var invoiceLineQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL);
			invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_OP, invoiceLine.Part.PK);
			invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_AddInfo, SQLComparisonOperator.NotContains, "IsPackToBondForLine_Hidden=Y");
			cusEntryLineQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			if (invoiceLine.Declaration.Importer != null)
			{
				jobDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_OH_Importer, invoiceLine.Declaration.Importer.PK);
			}
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.PK, invoiceLine.Declaration.Company.PK);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
			jobDeclarationQuery.AddSubQuery(branchQuery, JoinCondition.And);
			entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
			cusEntryLineQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			entryLinesForPartFiler.AddToFilter(cusEntryLineQuery);

			ZString selectedDDN = ZString.Empty;
			ZShort selectedDDL = ZShort.Zero;
			ZDecimal unitCustomsValue = 0m;
			var collection = new DynamicBusinessObjectCollection(invoiceLine.Factory);
			var filterParameterisedText = entryLinesForPartFiler.ParameterisedText;
			collection.Load(string.Format("select {0} from {1} where ", CusEntryLineSchema.Constants.PK, CusEntryLineSchema.Constants.TableName) + filterParameterisedText.ParameterisedQueryText, filterParameterisedText.Parameters);
			var entryLinePKs = collection.Cast<DynamicBusinessObject>().Select(x => x[CusEntryLineSchema.Constants.PK]).ToList();
			foreach (ZGuid entryLinePK in entryLinePKs)
			{
				var newFactory = new BusinessObjectFactory();
				var entryLine = (IDrawbackEntryLine)newFactory.Load<CusEntryLine>(entryLinePK);
				if (entryLine != null)
				{
					var declarationDate = entryLine.DeclarationDate;
					if (declarationDate.IsValid && declarationDate >= startDate && declarationDate <= endDate)
					{
						var drawbackAmounts = DrawbackAmountsForLinePart(entryLine, invoiceLine);
						var perUnitCustomsValue = drawbackAmounts.Quantity <= 0.0m ? 0.0m : decimal.Round(drawbackAmounts.CustomsValue / drawbackAmounts.Quantity, 5);
						if (!entryLine.EntryNumber.IsEmpty && (selectedDDN.IsEmpty || entryLine.CL_DutyPercent == 0m || perUnitCustomsValue < unitCustomsValue))
						{
							selectedDDN = entryLine.EntryNumber.Left(AUAddInfo.Schema.ZA_DDN_HiddenMaxLength);
							selectedDDL = entryLine.EffectiveLineNumber;
							unitCustomsValue = entryLine.CL_DutyPercent == 0m ? 0m : perUnitCustomsValue;
						}
					}
				}
			}

			if (!selectedDDN.IsEmpty)
			{
				invoiceLine.AddInfo.ZA_DDN_Hidden = selectedDDN;
				invoiceLine.AddInfo.ZA_DDL_Hidden = selectedDDL;
				drawbackImportEntryLine = invoiceLine.DrawbackImportEntryLine;
				GetValuesWhenSpecificImportDecSupplied(drawbackImportEntryLine);
			}
		}

		void GetValuesForImputationMethod()
		{
			if (invoiceLine.AddInfo.AggregatedPST.IsEmpty)
			{
				invoiceLine.AddInfo.ZA_PST = "GEN";
			}

			var dutyWrapperForLine = new CMRDutyWrapperForInvoiceLine(invoiceLine, false);
			dutyWrapperForLine.PercentageOfFOBForCustomsValue = 30m;
			var dutyCalculator = new CMRDutyCalculator(dutyWrapperForLine);
			DutyRate = dutyCalculator.DutyRate;
			CalculatedCustomsValue = dutyWrapperForLine.CustomsValue;
			CalculatedDutyAmount = dutyCalculator.Duty.Amount.Amount;
		}

		void GetValuesWhenSpecificImportDecSupplied(IDrawbackEntryLine drawbackImportEntryLine)
		{
			var drawbackAmounts = DrawbackAmountsForLinePart(drawbackImportEntryLine, invoiceLine);
			DutyRate = drawbackAmounts.DutyRate;
			decQty = drawbackAmounts.Quantity;
			decCustomsValue = drawbackAmounts.CustomsValue;
			decDutyAmount = drawbackAmounts.DutyAmount;
			proRataAmounts.Calculate(invoiceLine.DrawbackClaimQuantity, decQty, decCustomsValue, decDutyAmount);
			CalculatedCustomsValue = proRataAmounts.CalculatedCustomsValue;
			CalculatedDutyAmount = proRataAmounts.CalculatedDutyAmount;
		}

		internal static DrawbackAmounts DrawbackAmountsForLinePart(IDrawbackEntryLine importEntryLine, JobComInvoiceLine invoiceLine)
		{
			var result = new DrawbackAmounts();
			result.DutyRate = importEntryLine.CL_DutyPercent;
			var invoiceLines = invoiceLine.JI_PartNo.IsEmpty ? System.Array.Empty<JobComInvoiceLine>() : importEntryLine.GetInvoiceLines();
			if (invoiceLines.Length > 1)
			{
				foreach (JobComInvoiceLine importInvoiceLine in invoiceLines)
				{
					if (importInvoiceLine.JI_PartNo == invoiceLine.JI_PartNo)
					{
						if (invoiceLine.JI_CustomsUnitQty == importInvoiceLine.JI_CustomsUnitQty && importInvoiceLine.JI_CustomsQuantity > 0)
						{
							result.Quantity += importInvoiceLine.JI_CustomsQuantity;
							result.CustomsValue += importInvoiceLine.CustomsValue.Amount;
							result.DutyAmount += importInvoiceLine.JI_Calc_DutyAmount;
						}
						else if (!importInvoiceLine.JI_InvoiceQuantity.IsEmpty && invoiceLine.UnitConverter.Convertible(importInvoiceLine.JI_InvoiceUQ, invoiceLine.JI_InvoiceUQ))
						{
							result.Quantity += invoiceLine.UnitConverter.Convert(importInvoiceLine.JI_InvoiceQuantity, importInvoiceLine.JI_InvoiceUQ, invoiceLine.JI_InvoiceUQ);
							result.CustomsValue += importInvoiceLine.CustomsValue.Amount;
							result.DutyAmount += importInvoiceLine.JI_Calc_DutyAmount;
						}
					}
				}
			}
			else if (invoiceLine.JI_CustomsUnitQty == importEntryLine.CustomsUnitQty && importEntryLine.Quantity > 0)
			{
				result.Quantity = importEntryLine.Quantity;
				result.CustomsValue = importEntryLine.CL_CustomsValue;
				result.DutyAmount = importEntryLine.DutyAmount;
			}
			else if (!importEntryLine.InvoiceQuantity.IsEmpty && invoiceLine.UnitConverter.Convertible(importEntryLine.InvoiceUQ, invoiceLine.JI_InvoiceUQ))
			{
				result.Quantity = invoiceLine.UnitConverter.Convert(importEntryLine.InvoiceQuantity, importEntryLine.InvoiceUQ, invoiceLine.JI_InvoiceUQ);
				result.CustomsValue = importEntryLine.CL_CustomsValue;
				result.DutyAmount = importEntryLine.DutyAmount;
			}
			result.Quantity = ZArchitecture.Core.Utilities.Round(result.Quantity, 5);
			return result;
		}

		internal static ZDecimal CustomsQuantityFromExportInvoiceLineIfPossible(JobComInvoiceLine exportInvoiceLine, JobComInvoiceLine drawbackInvoiceLine)
		{
			ZDecimal result = 0m;

			if (!exportInvoiceLine.JI_CustomsQuantity.IsEmpty && drawbackInvoiceLine.UnitConverter.Convertible(exportInvoiceLine.JI_CustomsUnitQty, drawbackInvoiceLine.JI_CustomsUnitQty))
			{
				result = drawbackInvoiceLine.UnitConverter.Convert(exportInvoiceLine.JI_CustomsQuantity, exportInvoiceLine.JI_CustomsUnitQty, drawbackInvoiceLine.JI_CustomsUnitQty);
			}
			if (result.IsEmpty && !exportInvoiceLine.JI_InvoiceQuantity.IsEmpty && drawbackInvoiceLine.UnitConverter.Convertible(exportInvoiceLine.JI_InvoiceUQ, drawbackInvoiceLine.JI_CustomsUnitQty))
			{
				result = drawbackInvoiceLine.UnitConverter.Convert(exportInvoiceLine.JI_InvoiceQuantity, exportInvoiceLine.JI_InvoiceUQ, drawbackInvoiceLine.JI_CustomsUnitQty);
			}
			if (result.IsEmpty && !drawbackInvoiceLine.JI_InvoiceQuantity.IsEmpty)
			{
				var drawbackImportEntryLine = drawbackInvoiceLine.DrawbackImportEntryLine;
				if (drawbackImportEntryLine != null && drawbackInvoiceLine.UnitConverter.Convertible(drawbackInvoiceLine.JI_InvoiceUQ, drawbackImportEntryLine.InvoiceUQ))
				{
					result = CustomsQuantityFromEntryLineCustomsQuantityProRataByInvoiceQuantity(drawbackImportEntryLine, drawbackInvoiceLine);
				}
			}
			if (result.IsEmpty && !exportInvoiceLine.JI_NetWeight.IsEmpty && drawbackInvoiceLine.UnitConverter.Convertible(exportInvoiceLine.JI_NetWeightUQ, drawbackInvoiceLine.JI_CustomsUnitQty))
			{
				result = drawbackInvoiceLine.UnitConverter.Convert(exportInvoiceLine.JI_NetWeight, exportInvoiceLine.JI_NetWeightUQ, drawbackInvoiceLine.JI_CustomsUnitQty);
			}

			return ZArchitecture.Core.Utilities.Round(result, 5);
		}

		internal static ZDecimal CustomsQuantityFromEntryLineCustomsQuantityProRataByInvoiceQuantity(IDrawbackEntryLine importEntryLine, JobComInvoiceLine invoiceLine)
		{
			ZDecimal result = ZDecimal.Zero;
			ZDecimal entryInvoiceQuantity = ZDecimal.Zero;
			ZDecimal entryCustomsQuantity = ZDecimal.Zero;
			var invoiceLines = invoiceLine.JI_PartNo.IsEmpty ? System.Array.Empty<JobComInvoiceLine>() : importEntryLine.GetInvoiceLines();
			if (invoiceLines.Length > 1)
			{
				foreach (JobComInvoiceLine line in invoiceLines)
				{
					if (line.JI_PartNo == invoiceLine.JI_PartNo)
					{
						entryInvoiceQuantity += line.JI_InvoiceQuantity;
						entryCustomsQuantity += line.JI_CustomsQuantity;
					}
				}
			}
			else
			{
				entryInvoiceQuantity = importEntryLine.InvoiceQuantity;
				entryCustomsQuantity = importEntryLine.CustomsQuantity;
			}
			if (importEntryLine.InvoiceUQ != invoiceLine.JI_InvoiceUQ && invoiceLine.UnitConverter.Convertible(importEntryLine.InvoiceUQ, invoiceLine.JI_InvoiceUQ))
			{
				entryInvoiceQuantity = invoiceLine.UnitConverter.Convert(entryInvoiceQuantity, importEntryLine.InvoiceUQ, invoiceLine.JI_InvoiceUQ);
			}
			if (entryInvoiceQuantity == invoiceLine.JI_InvoiceQuantity)
			{
				result = entryCustomsQuantity;
			}
			else if (!entryInvoiceQuantity.IsEmpty)
			{
				result = entryCustomsQuantity * invoiceLine.JI_InvoiceQuantity / entryInvoiceQuantity;
			}
			return ZArchitecture.Core.Utilities.Round(result, 5);
		}

		internal static ZDecimal CustomsQuantityFromEntryLineCustomsQuantity(IDrawbackEntryLine importEntryLine, JobComInvoiceLine invoiceLine)
		{
			ZDecimal result = ZDecimal.Zero;
			var invoiceLines = invoiceLine.JI_PartNo.IsEmpty ? System.Array.Empty<JobComInvoiceLine>() : importEntryLine.GetInvoiceLines();
			if (invoiceLines.Length > 1)
			{
				foreach (JobComInvoiceLine line in invoiceLines)
				{
					if (line.JI_PartNo == invoiceLine.JI_PartNo)
					{
						result += line.JI_CustomsQuantity;
					}
				}
			}
			else
			{
				result = importEntryLine.CustomsQuantity;
			}
			return result;
		}

		internal static ZDecimal InvoiceQuantityFromEntryLineInvoiceQuantity(IDrawbackEntryLine importEntryLine, JobComInvoiceLine invoiceLine)
		{
			ZDecimal result = ZDecimal.Zero;
			var invoiceLines = invoiceLine.JI_PartNo.IsEmpty ? System.Array.Empty<JobComInvoiceLine>() : importEntryLine.GetInvoiceLines();
			if (invoiceLines.Length > 1)
			{
				foreach (JobComInvoiceLine line in invoiceLines)
				{
					if (line.JI_PartNo == invoiceLine.JI_PartNo)
					{
						result += line.JI_InvoiceQuantity;
					}
				}
			}
			else
			{
				result = importEntryLine.InvoiceQuantity;
			}
			return result;
		}
	}

	class ProRataAmounts
	{
		public ProRataAmounts()
		{
		}
		public ZDecimal CalculatedCustomsValue;
		public ZDecimal CalculatedDutyAmount;

		public void Calculate(ZDecimal drawbackQty, ZDecimal decQty, ZDecimal decCustomsValue, ZDecimal decDutyAmount)
		{
			CalculatedCustomsValue = 0;
			CalculatedDutyAmount = 0;
			if (decQty == 0)
			{
				CalculatedCustomsValue = decCustomsValue;
				CalculatedDutyAmount = decDutyAmount;
			}
			else if (decQty > 0 && drawbackQty > 0)
			{
				ZDecimal factor = drawbackQty / decQty;
				CalculatedCustomsValue = decimal.Round(decCustomsValue * factor, 2);
				CalculatedDutyAmount = decimal.Round(decDutyAmount * factor, 2);
			}
		}
	}

	struct DrawbackAmounts
	{
		public ZDecimal Quantity;
		public ZDecimal CustomsValue;
		public ZDecimal DutyAmount;
		public ZDecimal DutyRate;
	}
}
