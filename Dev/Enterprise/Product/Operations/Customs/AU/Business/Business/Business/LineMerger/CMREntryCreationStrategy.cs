using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMREntryCreationStrategy : EntryCreationStrategyBase
	{
		public CMREntryCreationStrategy(LineMerger lineMerger)
			: base(lineMerger)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override Customs.Business.MergeKey GetKeyForHeaderCore(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)baseInvoiceLine.InvoiceHeader;

			Customs.Business.MergeKey result = base.GetKeyForHeaderCore(baseInvoiceLine);
			result.Add(invoiceHeader.AddInfo.ZA_EFD);
			result.Add(invoiceHeader.AddInfo.ZA_VAN);

			result.Add(GetWarehousePartOfKey((JobComInvoiceLine)baseInvoiceLine));

			return result;
		}

		bool? hasLineGoingIntoBondAndLineNotGoingIntoBond;
		ZString GetWarehousePartOfKey(JobComInvoiceLine invoiceLine)
		{
			// this is a bit ugly - waiting on Brett's change for entry line recycling which handles empty fields in the merge key
			ZString result = invoiceLine.WarehouseCCP;
			if (!invoiceLine.IsGoingIntoBondedWarehouse)
			{
				if (!hasLineGoingIntoBondAndLineNotGoingIntoBond.HasValue)
				{
					hasLineGoingIntoBondAndLineNotGoingIntoBond = invoiceLine.Declaration.HasLineGoingIntoBondAndLineNotGoingIntoBond;
				}
				if (hasLineGoingIntoBondAndLineNotGoingIntoBond.Value)
				{
					result = invoiceLine.Declaration.FirstWarehouseCCP;
				}
			}
			return result;
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var result = base.GetKeyForLine(baseInvoiceLine);
			result.Add(invoiceLine.AddInfo.AggregatedZA_VALB_Hidden);
			result.Add(invoiceLine.AddInfo.ZA_IsNonAQISAEPLine_Hidden);
			result.Add(new ZBool(invoiceLine.AddInfo.ZA_IsPackToBondForLine_Hidden == "Y"));
			result.Add(new ZString(invoiceLine.AddInfo.ZA_DTY > 0 || invoiceLine.AddInfo.ZA_SendZeroDutyOverride_Hidden ? "DTY" : ""));
			result.Add(new ZString(invoiceLine.AddInfo.ZA_STD > 0 ? "STD" : ""));

			result.Add(invoiceLine.InvoiceHeader != null && invoiceLine.InvoiceHeader.Supplier != null ? invoiceLine.InvoiceHeader.Supplier.OH_Code : ZString.Empty);

			result.Add(GetLuxuryCarTaxIndicatorString(invoiceLine));
			result.Add(GetAQISPackageTypeString(invoiceLine));
			result.Add(GetAQISLineContainerNumbersString(invoiceLine));
			result.Add(GetAQISDocumentTypeAndNumberString(invoiceLine));
			result.Add(GetAQISCommodityCodesString(invoiceLine));
			result.Add(GetAQISEntityIdsString(invoiceLine));
			result.Add(GetAQISPermitNumbersString(invoiceLine));
			result.Add(GetAQISPremisesIdString(invoiceLine));
			result.Add(GetAQISProcessingTypeString(invoiceLine));
			result.Add(GetAQISProducerCodeString(invoiceLine));
			result.Add(invoiceLine.WUV);
			result.Add(new ZString(invoiceLine.AddInfo.EffectiveTILVString.IsEmpty ? string.Empty : "TILV"));
			result.Add(GetDefaultCPDecAnswersString(invoiceLine));
			result.Add(GetLineSupplierCustomsClientIDString(invoiceLine));
			if (Declaration.IsWHSUniversalXMLActive && Declaration.IsImport && Declaration.SupportsBondedWarehousing)
			{
				var includeWarehouseRelatedData = Declaration.IsExWarehouse || invoiceLine.IsGoingIntoBondedWarehouse;
				var partPK = includeWarehouseRelatedData ? invoiceLine.JI_OP : ZGuid.Empty;
				result.Add(partPK);
				result.Add(includeWarehouseRelatedData && partPK.IsValid ? GetInvoiceQtyToCustomsQtyRatio(invoiceLine) : ZDecimal.Zero);
			}

			if (Declaration.IsImport)
			{
				var isPackToBondForLine = invoiceLine.JI_IsPackToBondForLine;
				var icsPermits = invoiceLine.ICSPermits;
				result.Add(isPackToBondForLine ? ZString.Empty : invoiceLine.AddInfo.ZA_WAR);
				result.Add(isPackToBondForLine ? ZString.Empty : new ZString(icsPermits.Count.ToString(CultureInfo.InvariantCulture) + "_" + icsPermits.GetSortedPermitNumbers()));
			}

			if (Declaration.IsEXPDeclaration)
			{
				result.Add(invoiceLine.AddInfo.ZA_AssayAU_Hidden);
				result.Add(invoiceLine.AddInfo.ZA_AssayNI_Hidden);
				result.Add(invoiceLine.AddInfo.ZA_AssaySN_Hidden);
				result.Add(invoiceLine.AddInfo.ZA_AssayPT_Hidden);
				result.Add(invoiceLine.AddInfo.ZA_AssayPB_Hidden);
				result.Add(invoiceLine.AddInfo.ZA_AssayCU_Hidden);
				result.Add(invoiceLine.AddInfo.ZA_AssayAG_Hidden);
				result.Add(invoiceLine.AddInfo.ZA_AssayWO_Hidden);
				result.Add(invoiceLine.AddInfo.ZA_AssayZN_Hidden);
				result.Add(invoiceLine.JI_AUState);
				result.Add(invoiceLine.AddInfo.ZA_PermitNumbers_Hidden);
				result.Add(invoiceLine.JI_TempImportNum);
			}

			return result;
		}

		ZDecimal GetInvoiceQtyToCustomsQtyRatio(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.JI_CustomsQuantity.IsEmpty ? ZDecimal.Zero : new ZDecimal(invoiceLine.JI_InvoiceQuantity / invoiceLine.JI_CustomsQuantity);
		}

		ZString GetLineSupplierCustomsClientIDString(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.Supplier?.GetCustomsClientID() ?? ZString.Empty;
		}

		#region Default CP Dec answers

		ZString GetDefaultCPDecAnswersString(JobComInvoiceLine invoiceLine)
		{
			var result = new List<string>();
			foreach (CMRCusEntryCPDec question in invoiceLine.QuestionsWithEffectiveDefaultAnswers)
			{
				if (question.EffectiveDefaultAnswer == CMRCusEntryCPDec.Answers.Ambiguous)
				{
					return invoiceLine.PK.ToStringKey();
				}

				result.Add(question.ON_CPDecNum.ToString() + question.EffectiveDefaultAnswer);
			}
			return GetSortedFieldString(result);
		}

		#endregion

		#region Luxury Car Tax Indicator

		ZString GetLuxuryCarTaxIndicatorString(JobComInvoiceLine invoiceLine)
		{
			string result = "";

			if (invoiceLine.AddInfo.ZA_LCTE.Trim().ToUpper() == "Y")
			{
				if (invoiceLine.InvoiceHeader != null)
				{
					result += "InvNo=" + invoiceLine.InvoiceHeader.JZ_InvoiceNumber.Trim();
				}

				result += "LineNo=" + invoiceLine.JI_LineNo;
			}

			return result;
		}

		#endregion

		#region AQIS Container Line

		ZString GetAQISLineContainerNumbersString(JobComInvoiceLine invoiceLine)
		{
			var aQISLineContainerNumbers = new List<string>();

			foreach (CusContainerInvoiceLinePivot pivot in invoiceLine.ContainersPivot)
			{
				var container = pivot.Container;
				aQISLineContainerNumbers.Add(container != null ? container.CO_ContainerNumber.Trim() : ZString.Empty);
			}

			return GetSortedFieldString(aQISLineContainerNumbers);
		}

		#endregion

		#region AQIS Package Type

		ZString GetAQISPackageTypeString(JobComInvoiceLine invoiceLine)
		{
			var aQISPackageTypes = new List<string>();

			foreach (AQISPackage currentPackage in invoiceLine.AQISPackages)
			{
				aQISPackageTypes.Add(currentPackage.Type);
			}

			return GetSortedFieldString(aQISPackageTypes);
		}

		#endregion

		#region AQIS Document Type And Number

		ZString GetAQISDocumentTypeAndNumberString(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine == null)
			{
				throw new InvalidOperationException("InvoiceLine should not be null in GetAQISDocumentTypeAndNumberString.");
			}

			AQISDocumentCollection aggreatedAQISDocuments = invoiceLine.AggreatedAQISDocuments
				?? throw new InvalidOperationException("Invoice line's aggreatedAQISDocuments should not be null in GetAQISDocumentTypeAndNumberString.");
			var aQISDocumentTypeAndNumber = new List<string>();
			foreach (AQISDocument currentDocument in aggreatedAQISDocuments)
			{
				aQISDocumentTypeAndNumber.Add(currentDocument.Type.Trim() + " " + currentDocument.Number.Trim());
			}

			return GetSortedFieldString(aQISDocumentTypeAndNumber);
		}

		#endregion

		#region AQIS Commodity Codes

		ZString GetAQISCommodityCodesString(JobComInvoiceLine invoiceLine)
		{
			var aQISCommodityCodes = new List<string>();

			foreach (AQISCommodityCode currentCommodityCode in invoiceLine.AggreatedAQISCommodityCodes)
			{
				aQISCommodityCodes.Add(currentCommodityCode.Code.ToString().Trim());
			}

			return GetSortedFieldString(aQISCommodityCodes);
		}

		#endregion

		#region AQIS Entity Ids

		ZString GetAQISEntityIdsString(JobComInvoiceLine invoiceLine)
		{
			var aQISEntityIds = new List<string>();

			foreach (AQISEntityId currentEntityId in invoiceLine.AggreatedAQISEntityIds)
			{
				aQISEntityIds.Add(currentEntityId.Code.ToString().Trim());
			}

			return GetSortedFieldString(aQISEntityIds);
		}

		#endregion

		#region AQIS Permit Numbers

		ZString GetAQISPermitNumbersString(JobComInvoiceLine invoiceLine)
		{
			var aQISPermitNumbers = new List<string>();

			foreach (AQISPermitId currentPermitId in invoiceLine.AggreatedAQISPermitIds)
			{
				aQISPermitNumbers.Add(currentPermitId.Code.ToString().Trim());
			}

			return GetSortedFieldString(aQISPermitNumbers);
		}

		#endregion

		#region AQIS Premises Ids

		ZString GetAQISPremisesIdString(JobComInvoiceLine invoiceLine)
		{
			var aQISPremisesIds = new List<string>();

			foreach (AQISPremisesIdAndProcessingType currentPremisesId in invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes)
			{
				aQISPremisesIds.Add(currentPremisesId.PremisesId.ToString().Trim());
			}

			return GetSortedFieldString(aQISPremisesIds);
		}

		#endregion

		#region AQIS Processing Type

		ZString GetAQISProcessingTypeString(JobComInvoiceLine invoiceLine)
		{
			var aQISProcessingTypes = new List<string>();

			foreach (AQISPremisesIdAndProcessingType currentProcessingType in invoiceLine.AggreatedAQISPremisesIdAndProcessingTypes)
			{
				aQISProcessingTypes.Add(currentProcessingType.ProcessingType.ToString().Trim());
			}

			return GetSortedFieldString(aQISProcessingTypes);
		}

		#endregion

		#region AQIS Producer Code

		ZString GetAQISProducerCodeString(JobComInvoiceLine invoiceLine)
		{
			var aQISProducerCodes = new List<string>();

			foreach (AQISProducerCode currentProducerCode in invoiceLine.AggreatedAQISProducerCodes)
			{
				aQISProducerCodes.Add(currentProducerCode.Code.ToString().Trim());
			}

			return GetSortedFieldString(aQISProducerCodes);
		}

		#endregion

		string GetSortedFieldString(List<string> unSortedListOfValues)
		{
			ZStringBuilder result = new ZStringBuilder();
			unSortedListOfValues.Sort(StringComparer.OrdinalIgnoreCase);

			foreach (string currentString in unSortedListOfValues)
			{
				result.Append(currentString + " ");
			}

			return result.ToString().TrimEnd();
		}
	}
}
