using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	sealed class UniversalExtensionsTest : TestCaseWithFactory
	{
		public void TestGetInvoiceLineAddInfosApplicableForInwardWarehousing()
		{
			var list = Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing();
			AssertEquals("List should be cached to the factory", list, Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing());
			var checkedList = new List<string>(list);
			var missingFields = new List<string>();
			foreach (var expectedField in ExpectedList.Select(x => x.Substring(3)))
			{
				if (checkedList.Contains(expectedField))
				{
					checkedList.Remove(expectedField);
				}
				else
				{
					missingFields.Add(expectedField);
				}
			}
			var failedMessage = new ZStringBuilder();
			if (missingFields.Count > 0)
			{
				failedMessage.Append("The following expected fields are missing from the list; if they are no longer valid then please remove them from the expected list:");
				missingFields.ForEach(x => failedMessage.Append(x));
				failedMessage.AppendLine();
			}
			if (checkedList.Count > 0)
			{
				failedMessage.Append("The following new fields are not in the expected list; if they are valid then please add them to the expected list and also if they are numeric fields that need to be apportioned correctly then add them to Enterprise.Customs.US.Busines.InventorySelectionHeader.FieldsNeedToApplyRatio:");
				checkedList.ForEach(x => failedMessage.Append(x));
			}
			Assert(failedMessage.ToStringWithNewLineBetweenAppends(), failedMessage.IsEmpty);
		}

		static string[] ExpectedList
		{
			get
			{
				return new string[]
				{
					JobComInvoiceLine.Schema.CA_99TariffCode,
					JobComInvoiceLine.Schema.CA_ADJCode,
					JobComInvoiceLine.Schema.CA_ADJValue,
					JobComInvoiceLine.Schema.CA_AirsCode,
					JobComInvoiceLine.Schema.CA_AMMVPerUnit,
					JobComInvoiceLine.Schema.CA_AMMVPercentage,
					JobComInvoiceLine.Schema.CA_ApplyLuxuryTax,
					JobComInvoiceLine.Schema.CA_AuthorityNumber,
					JobComInvoiceLine.Schema.CA_B3SubHeaderNumber,
					JobComInvoiceLine.Schema.CA_CalculationMethod,
					JobComInvoiceLine.Schema.CA_CasualImportCommodity,
					JobComInvoiceLine.Schema.CA_CasualImportDestinationProvince,
					JobComInvoiceLine.Schema.CA_CFIACountryOfSource,
					JobComInvoiceLine.Schema.CA_CFIARegionOfSource,
					JobComInvoiceLine.Schema.CA_CFIAStateOfSource,
					JobComInvoiceLine.Schema.CA_CFIAUSStateOfOrigin,
					JobComInvoiceLine.Schema.CA_CompliantCompletion,
					JobComInvoiceLine.Schema.CA_CompliantImportDate,
					JobComInvoiceLine.Schema.CA_ConveyanceIdentificationNumber,
					JobComInvoiceLine.Schema.CA_DestinationProvince,
					JobComInvoiceLine.Schema.CA_EndUse,
					JobComInvoiceLine.Schema.CA_ExpiryDate,
					JobComInvoiceLine.Schema.CA_IIDRegion,
					JobComInvoiceLine.Schema.CA_ImportReasonCode,
					JobComInvoiceLine.Schema.CA_IsAccountForLine,
					JobComInvoiceLine.Schema.CA_IsAutoDummyHSCodeCasualImportLine,
					JobComInvoiceLine.Schema.CA_IsCasualImport,
					JobComInvoiceLine.Schema.CA_IsExempt,
					JobComInvoiceLine.Schema.CA_IsSeeded,
					JobComInvoiceLine.Schema.CA_MiscID,
					JobComInvoiceLine.Schema.CA_Model,
					JobComInvoiceLine.Schema.CA_ModelNumber,
					JobComInvoiceLine.Schema.CA_PageNumber,
					JobComInvoiceLine.Schema.CA_PageRelativeLineNumber,
					JobComInvoiceLine.Schema.CA_ProductionDate,
					JobComInvoiceLine.Schema.CA_RemissionType,
					JobComInvoiceLine.Schema.CA_RequirementID,
					JobComInvoiceLine.Schema.CA_RequirementVer,
					JobComInvoiceLine.Schema.CA_RN_NKCFIAOrigin,
					JobComInvoiceLine.Schema.CA_RN_NKExport,
					JobComInvoiceLine.Schema.CA_TIIN,
					JobComInvoiceLine.Schema.CA_TreatmentCode,
					JobComInvoiceLine.Schema.CA_TRSNumber,
					JobComInvoiceLine.Schema.CA_TypeSize,
					JobComInvoiceLine.Schema.CA_USStateOfExport,
					JobComInvoiceLine.Schema.CA_ValueForTax,
					JobComInvoiceLine.Schema.CA_ValueForDutyCode,
					// ---- PGA ----
					JobComInvoiceLine.Schema.CA_CFIAInd,
					JobComInvoiceLine.Schema.CA_CNSCInd,
					JobComInvoiceLine.Schema.CA_DFOInd,
					JobComInvoiceLine.Schema.CA_ECCCInd,
					JobComInvoiceLine.Schema.CA_GACInd,
					JobComInvoiceLine.Schema.CA_HCInd,
					JobComInvoiceLine.Schema.CA_NRCanInd,
					JobComInvoiceLine.Schema.CA_PHACInd,
					JobComInvoiceLine.Schema.CA_TCInd,
					JobComInvoiceLine.Schema.CA_ManufactureDate,
					JobComInvoiceLine.Schema.CA_TradeName,
					JobComInvoiceLine.Schema.CA_PackagingQuantity,
					JobComInvoiceLine.Schema.CA_RN_NKSource,
					JobComInvoiceLine.Schema.CA_StateOfSource,
					JobComInvoiceLine.Schema.CA_ModelYear,
					JobComInvoiceLine.Schema.CA_VINNumber,

					// ---- Aggregated Fields for Report ----
					JobComInvoiceLine.Schema.CA_CPTAmount,
					JobComInvoiceLine.Schema.CA_CPTExemptCode,
					JobComInvoiceLine.Schema.CA_CPTRateDescription,
					JobComInvoiceLine.Schema.CA_CTAAmount,
					JobComInvoiceLine.Schema.CA_CTAExemptCode,
					JobComInvoiceLine.Schema.CA_CTARateDescription,
					JobComInvoiceLine.Schema.CA_DTYAmount,
					JobComInvoiceLine.Schema.CA_DTYExemptCode,
					JobComInvoiceLine.Schema.CA_DTYRateDescription,
					JobComInvoiceLine.Schema.CA_EXSAmount,
					JobComInvoiceLine.Schema.CA_EXSExemptCode,
					JobComInvoiceLine.Schema.CA_EXSRateDescription,
					JobComInvoiceLine.Schema.CA_GSTAmount,
					JobComInvoiceLine.Schema.CA_GSTExemptCode,
					JobComInvoiceLine.Schema.CA_GSTRateDescription,
					JobComInvoiceLine.Schema.CA_SIMAmount,
					JobComInvoiceLine.Schema.CA_SIMExemptCode,
					JobComInvoiceLine.Schema.CA_SIMRateDescription
				};
			}
		}
	}
}
