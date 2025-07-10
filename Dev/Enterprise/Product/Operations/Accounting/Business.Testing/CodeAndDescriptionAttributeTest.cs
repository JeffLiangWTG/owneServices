using System;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	class CodeAndDescriptionAttributeTest : TestCase
	{
		public void TestBizosCodeAndDescriptionAttributeOfAccounttingBizos()
		{
			CodeAndDescriptionAttributeCheck(typeof(AccComplianceDocumentHeader).Assembly, WhiteListOfBizos);
		}

		public void TestBizosCodeAndDescriptionAttributeOfAccounttingTaxFrameworkBizos()
		{
			CodeAndDescriptionAttributeCheck(typeof(TaxFramework.Business.AccTaxTransaction).Assembly, WhiteListOfTaxFrameBizos);
		}

		public void TestWhiteListInAlphabeticalOrder()
		{
			CombineAssertions("Please keep white list in alphabetical order, thank you!", () =>
			{
				AssertContainsExactElementsInExactOrder("WhiteListOfBizos", WhiteListOfBizos.OrderBy(x => x), WhiteListOfBizos);
				AssertContainsExactElementsInExactOrder("WhiteListOfTaxFrameBizos", WhiteListOfTaxFrameBizos.OrderBy(x => x), WhiteListOfTaxFrameBizos);
			});
		}

		void CodeAndDescriptionAttributeCheck(System.Reflection.Assembly assemblyDll, string[] whiteList)
		{
			var bizoTypes = assemblyDll.GetTypes().Where(t => BizoType.IsAssignableFrom(t));
			var releventBizos = bizoTypes
					.Where(t => !NonPeristBizoType.IsAssignableFrom(t))
					.Where(t => !FilterType.IsAssignableFrom(t))
					.Where(t => !DynamicBizoType.IsAssignableFrom(t))
					.Where(t => !t.Name.StartsWith("Auto"))
					.Where(t => !t.Name.StartsWith("Dummy"))
					.Where(t => !t.Name.EndsWith("Pivot"))
					.Where(t => !t.Name.EndsWith("Line"))
					.Where(t => !t.Name.EndsWith("Test"))
					.Where(t => !t.IsAbstract)
					.Where(t => !whiteList.Contains(t.Name))
					.Where(t => t.IsPublic);

			var bizosWithoutCodeProperty = releventBizos
					.Where(t => !t.GetCustomAttributes(true).Any(at => at.GetType() == CodePropertyType))
					.ToList();
			var bizosWithoutDescProperty = releventBizos
					.Where(t => !t.GetCustomAttributes(true).Any(at => at.GetType() == DescPropertyType))
					.ToList();

			CombineAssertions(@"All user visible business objects should nominate a [CodeProperty] which uniquely identifies the object. You may use a calculated property for a composite identifier.
				If your object is not user visible or has no natural identifier, you may add the object to the whitelist. Note that if your object omits [CodeProperty], and appears in a FindBox, you will get a runtime error - see NoCodePropertyException.",
				() =>
				{
					AssertEquals("bizosWithoutCodeProperty", false, bizosWithoutCodeProperty.Any());
					AssertEquals("bizosWithoutDescProperty", false, bizosWithoutDescProperty.Any());
				}
			);
		}

		Type BizoType => typeof(CargoWise.EntityFramework.BusinessObject);
		Type NonPeristBizoType => typeof(CargoWise.EntityFramework.NonPersistentBusinessObject);
		Type FilterType => typeof(ZArchitecture.Business.FilterBusinessObject);
		Type CodePropertyType => typeof(CargoWise.EntityFramework.CodePropertyAttribute);
		Type DescPropertyType => typeof(CargoWise.EntityFramework.DescriptionPropertyAttribute);
		Type DynamicBizoType => typeof(CargoWise.EntityFramework.DynamicBusinessObject);

		// Please keep this list in alphabetical order, thank you!!!
		string[] WhiteListOfBizos => new string[] {
			"AccApportionmentTemplateLines",
			"AccBillingHeader",
			"AccBillingItem",
			"AccCashBasisVAT",
			"AccConsolidationBatch",
			"AccConsolidationMember",
			"AccEInvoicingBatch",
			"AccEPaymentDeal",
			"AccEPaymentQuote",
			"Accrual",
			"AccTaxReturn",
			"AccTaxReturnColumn",
			"AccTransactionHeaderAuthorisationRecord",
			"AccTransactionHeaderNettingLink",
			"AccTransLinePay",
			"ApportionSplitCharge",
			"ARCreditNoteApprovalRequest",
			"CashAdvanceRequestHeader",
			"Charge",
			"DsbJobCloseBatch",
			"EPaymentBeneficiaryRequest",
			"EPaymentDeal",
			"EPaymentQuote",
			"ExchangeRate",
			"FijiAccTransactionHeaderAuthorisationRecord",
			"GenericTransaction",
			"GLBudget",
			"GlobalChargeCodeMapPivotIntercompany",
			"GlobalChargeCodeMapPivotOrganization",
			"IndiaAccTransactionHeaderAuthorisationRecord",
			"InvoicingBaseConsolCostForImporting",
			"JobChargePostingQueue",
			"JobChargeRevRecognition",
			"JobConsolCost",
			"JobConsolCostAttrib",
			"JournalSubAccount",
			"KoreaSouthAccTransactionHeaderAuthorisationRecord",
			"NettingCalculation",
			"NettingFXOffer",
			"NettingOrganisation",
			"NettingPayableLineReference",
			"NettingPayableTransaction",
			"NettingPayableTransactionRef",
			"NettingReceivableLineReference",
			"NettingReceivableTransaction",
			"NettingReceivableTransactionRef",
			"NettingSystemExchangeRate",
			"PaymentApprovalItem",
			"Period",
			"PtrsAllPaymentsReport",
			"PtrsAllPaymentsReport2024",
			"PtrsReport",
			"PtrsReport2024",
			"SamoaAccTransactionHeaderAuthorisationRecord",
			"ShipmentConsolAndMasterBillNumbers",
			"Statement",
			"TparReport",
			"TransactionLineSubAccount",
			"TransactionMatchLink",
			"TransactionPendingAllocationApprovalRequest",
			"ViewMatchGroup",
			"WIP"
		};

		// Please keep this list in alphabetical order, thank you!!!
		string[] WhiteListOfTaxFrameBizos => new string[]
		{
			"AccTaxGLMovement",
			"AccTaxTransaction"
		};
	}
}
