using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Au = Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.Wow
{
	public class WowMessageSendingValidationTest : TestCaseWithFactory
	{
		public void TestNoTniNoContinue()
		{
			AssertEquals("TotalTAndI.Amount", 0m, Declaration.CustomsEntryHeaders[0].TotalTAndI.Amount);
			Notifier.AnswerToContinueWithAction = false;
			AssertEquals(false, Validation.CheckBusinessObjectLevelValidation(Notifier));
			Assert(Notifier.ContinueWithActionMessage.Contains(WowMessageSendingValidation.MessageNoTNIEntered));
		}

		public void TestNoTniWithContinue()
		{
			AssertEquals("TotalTAndI.Amount", 0m, Declaration.CustomsEntryHeaders[0].TotalTAndI.Amount);
			Notifier.AnswerToContinueWithAction = true;
			AssertEquals(true, Validation.CheckBusinessObjectLevelValidation(Notifier));
			Assert(!Notifier.ContinueWithActionMessage.Contains(WowMessageSendingValidation.MessageNoTNIEntered)); // Base "continue with send" message overrides local one.
		}

		public void TestNoMessageErrorsOrErrors()
		{
			MessageSendingValidation validation = MessageSendingValidation.New(Factory.New<DummyBusinessObject>(), null);
			AssertEquals(true, validation.CheckBusinessObjectLevelValidation(Notifier));
			Assert(string.IsNullOrEmpty(Notifier.InvalidOperationText));
			Assert(string.IsNullOrEmpty(Notifier.ContinueWithActionMessage));
			Assert(Notifier.ContinueWithActionMessage == null); // A ContinueWithActionMessage never called.
		}

		public void TestValidTniNoMessage()
		{
			BaseInvoiceCharge freight = Declaration.Invoices[0].Charges.AddNew();
			freight.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			freight.J7_Amount = 120.00m;
			BaseInvoiceCharge insurance = Declaration.Invoices[0].Charges.AddNew();
			insurance.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance;
			insurance.J7_Amount = 30.00m;
			Declaration.ResumeApportionment();
			AssertEquals("TotalTAndI.Amount", 150.00m, Declaration.CustomsEntryHeaders[0].TotalTAndI.Amount);
			AssertEquals(true, Validation.CheckBusinessObjectLevelValidation(Notifier));
			Assert(string.IsNullOrEmpty(Notifier.InvalidOperationText));
			Assert(!Notifier.ContinueWithActionMessage.Contains(WowMessageSendingValidation.MessageNoTNIEntered));
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = Au.JobMessageTypeList.Codes.Import;
					BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_InvoiceAmount = 1000.00m;
					invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
					BaseInvoiceCharge packing = invoiceHeader.Charges.AddNew();
					packing.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.PackingCost;
					packing.J7_Amount = 70.00m;
					BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceLine1.JI_LinePrice = 500.00m;
					BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceLine2.JI_LinePrice = 500.00m;
					Au.SendsMessagesToCustomsShutterUpperer mergeResult = new Au.SendsMessagesToCustomsShutterUpperer(false);
					mergeResult.AnswerToContinueWithAction = true;
					declaration.DoMerge(mergeResult);
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		WowMessageSendingValidation Validation
		{
			get
			{
				return validation ?? (validation = WowMessageSendingValidation.New(Declaration, null));
			}
		}

		WowMessageSendingValidation validation;
		Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer Notifier
		{
			get
			{
				return notifier ?? (notifier = new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			}
		}

		Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer notifier;
	}
}
