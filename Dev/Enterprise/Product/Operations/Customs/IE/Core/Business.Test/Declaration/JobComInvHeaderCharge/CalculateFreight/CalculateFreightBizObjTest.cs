using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(CalculateFreightBizObj))]
	class CalculateFreightBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCalculate_BACharge()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			var calculateFreightBizObj = EU.Business.Declaration.CalculateFreightBizObj.New<CalculateFreightBizObj>(invoice.Charges, declaration);
			calculateFreightBizObj.Amount = 1000;
			calculateFreightBizObj.Percentage = 51;
			calculateFreightBizObj.Currency = "EUR";

			calculateFreightBizObj.Calculate();

			CombineAssertions("BA charge", () =>
			{
				var baCharge = invoice.Charges.Cast<JobComInvCharge>().FirstOrDefault(charge => charge.J7_ChargeType == AISChargeCodeList.Codes.BA);
				AssertNotNull("BA charge should have been created.", baCharge);
				AssertEquals("Amount", 490m, baCharge.J7_Amount);
			});
		}

		public void TestCalculate_BACharge_GroupCharge()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var calculateFreightBizObj = EU.Business.Declaration.CalculateFreightBizObj.New<CalculateFreightBizObj>(declaration.TopGroupInvoice.Charges, declaration);
			calculateFreightBizObj.Amount = 1000;
			calculateFreightBizObj.Percentage = 51;
			calculateFreightBizObj.Currency = "EUR";

			calculateFreightBizObj.Calculate();

			var baCharge = declaration.TopGroupInvoice.Charges.Cast<JobComInvCharge>().FirstOrDefault(charge => charge.J7_ChargeType == AISChargeCodeList.Codes.BA);

			CombineAssertions("Group BA charge", () =>
			{
				AssertNull("No BA charge for Group Charges without EXW/FCA/FAS/FOB InvHeader.", baCharge);

				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
				calculateFreightBizObj = EU.Business.Declaration.CalculateFreightBizObj.New<CalculateFreightBizObj>(declaration.TopGroupInvoice.Charges, declaration);
				calculateFreightBizObj.Amount = 1000;
				calculateFreightBizObj.Percentage = 51;
				calculateFreightBizObj.Currency = "EUR";
				calculateFreightBizObj.Calculate();
				baCharge = declaration.TopGroupInvoice.Charges.Cast<JobComInvCharge>().FirstOrDefault(charge => charge.J7_ChargeType == AISChargeCodeList.Codes.BA);
				AssertNotNull("BA charge for Group Charges with at least 1 EXW/FCA/FAS/FOB InvHeader.", baCharge);
				AssertEquals("Amount", 490m, baCharge.J7_Amount);
			});
		}

		public void TestCalculate_BACharge_NonEXW_FCA_FAS_FOB()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			var calculateFreightBizObj = EU.Business.Declaration.CalculateFreightBizObj.New<CalculateFreightBizObj>(invoice.Charges, declaration);
			calculateFreightBizObj.Amount = 1000;
			calculateFreightBizObj.Percentage = 51;
			calculateFreightBizObj.Currency = "EUR";

			calculateFreightBizObj.Calculate();

			var baCharge = invoice.Charges.Cast<JobComInvCharge>().FirstOrDefault(charge => charge.J7_ChargeType == AISChargeCodeList.Codes.BA);
			AssertNull("Not set BA charge when INCOTerm not one of EXW/FCA/FAS/FOB.", baCharge);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return EU.Business.Declaration.CalculateFreightBizObj.New<CalculateFreightBizObj>(invoice.Charges, declaration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
	}
}
