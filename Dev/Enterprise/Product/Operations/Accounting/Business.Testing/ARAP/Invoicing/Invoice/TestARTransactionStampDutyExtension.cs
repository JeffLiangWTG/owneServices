using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TestARTransactionStampDutyExtension : TestCaseWithFactory
	{
		public void TestStampDutyForARInvoice()
		{
			AssertStampDutyForARCreditNote(typeof(ARInvoice));
		}

		public void TestStampDutyForARCreditNote()
		{
			AssertStampDutyForARCreditNote(typeof(ARCreditNote));
		}

		public void TestStampDutyForARInvoiceEventOnly()
		{
			AssertStampDutyForARCreditNote(typeof(ARInvoice), false);
		}

		public void TestStampDutyForARCreditNoteEventOnly()
		{
			AssertStampDutyForARCreditNote(typeof(ARCreditNote), false);
		}

		void AssertStampDutyForARCreditNote(Type invoiceType, bool remitStampDuty = true)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccTaxRate taxRate1, taxRate2;
				var testObjectCreator = new TestObjectCreator(Factory);

				SetupStampDutyCalculationTaxIDsAndRegistry(testObjectCreator, out taxRate1, out taxRate2);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_RL_NKClosestPort = "ITROM";
				if (!remitStampDuty)
				{
					orgHeader.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.NBO, Core.Constants.CountryCodes.Italy);
				}

				var invoice = testObjectCreator.CreateInvoice(invoiceType, "AR001", testObjectCreator.AUD, 1M, orgHeader);
				var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1M, 50M, 0M, 0M, 50M, 0M, 0M, testObjectCreator.CC3.PK);
				line.AL_AT = taxRate1.PK;

				Assert("Stamp duty should not be applicable as amount does not exceed threshold", !invoice.ShouldAddStampDuty());

				Factory.Save();

				IAmending original = invoice as IAmending;
				IAmending amending = original.GenerateAmendingTransaction(invoice.AH_TransactionType);
				Assert("Precondition: Is amending invoice", amending.IsAmendingTransaction);

				((InvoicingBase)amending).Lines[0].AL_OSExTaxAmount = 100M;
				var amendingInvoice = (InvoicingBase)amending;

				Assert("Stamp duty should be applicable as amount exceeds threshold", amendingInvoice.IsStampDutyApplicable());
				if (remitStampDuty)
				{
					Assert("Stamp duty should be added as amount exceeds threshold", amendingInvoice.ShouldAddStampDuty());
				}

				Factory.Save();

				if (remitStampDuty)
				{
					AssertEquals("Lines count", 2, amendingInvoice.Lines.Count);
					var stampDutyLine = amendingInvoice.Lines[1];
					Assert("Is stamp duty charge line", stampDutyLine.IsStampDutyChargeLine());
					if (invoiceType == typeof(ARInvoice))
					{
						Assert("Ex tax amount is postive for AR invoice", stampDutyLine.AL_OSExTaxAmount > 0);
					}
					else
					{
						Assert("Ex tax amount is negative for AR credit note", stampDutyLine.AL_OSExTaxAmount < 0);
					}
				}
				else
				{
					AssertEquals("No new Lines", 1, amendingInvoice.Lines.Count);
				}

				IAmending amending2 = original.GenerateAmendingTransaction(invoice.AH_TransactionType);
				Assert("Precondition: Is amending invoice", amending2.IsAmendingTransaction);

				((InvoicingBase)amending2).Lines[0].AL_OSExTaxAmount = 100M;
				var amendingInvoice2 = (InvoicingBase)amending2;

				Assert("Stamp duty should not be applicable as a previous amended invoice already has stamp duty event", !amendingInvoice2.IsStampDutyApplicable());
				if (remitStampDuty)
				{
					Assert("Stamp duty should be not be applicable as a previous amended invoice already has stamp duty charge", !amendingInvoice2.ShouldAddStampDuty());
				}

				if (!remitStampDuty)
				{
					orgHeader.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.NBO, Core.Constants.CountryCodes.Italy);
					Assert("Stamp duty should not be applicable as organization has an IT NBO code", !amendingInvoice2.ShouldAddStampDuty());
				}
			}
		}

		void SetupStampDutyCalculationTaxIDsAndRegistry(TestObjectCreator creator, out AccTaxRate taxRate, out AccTaxRate taxRate2)
		{
			taxRate = AccTaxRate.FindExistingTaxRate(Factory, "ART7", AccTaxRate.Types.Rated, Core.Constants.CountryCodes.Italy);
			taxRate2 = AccTaxRate.FindExistingTaxRate(Factory, "ART2", AccTaxRate.Types.Rated, Core.Constants.CountryCodes.Italy);

			var taxRateExempt = AccTaxRate.FindExistingTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, Core.Constants.CountryCodes.Italy);

			Factory.Save();

			var registry = AccountingConfigurationRegistry.Instance;
			var currCompGuid = GlbCompany.CurrentCompany.PK.ToGuid();

			string value = taxRate.PK.ToString() + "," + taxRate2.PK.ToString();
			registry.TaxIDsAttractingStampDuty.SetValue(currCompGuid, Guid.Empty, Guid.Empty, value);
			registry.StampDutyFixedAmount.SetValue(currCompGuid, Guid.Empty, Guid.Empty, 1.81m);
			registry.StampDutyThreshold.SetValue(currCompGuid, Guid.Empty, Guid.Empty, 77.47m);

			var chargeCode = creator.CreateChargeCode("BOLLO", "Stamp Duty", "NON", 1m, taxRateExempt, creator.WHTFREE1);
			registry.StampDutyChargeCode.SetValue(currCompGuid, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			Factory.Save();
		}

		public void TestStampDutyForARCreditNoteWhenStampDutyAmountGreaterThanTotalAmount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccTaxRate taxRate1, taxRate2;
				var testObjectCreator = new TestObjectCreator(Factory);
				SetupStampDutyCalculationTaxIDsAndRegistry(testObjectCreator, out taxRate1, out taxRate2);
				var registry = AccountingConfigurationRegistry.Instance;
				registry.StampDutyFixedAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 100m);
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_RL_NKClosestPort = "ITROM";
				Factory.Save();

				var crd1 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "AR001", testObjectCreator.AUD, 1M, orgHeader);
				var crd1Line = testObjectCreator.CreateInvoiceLine(crd1, testObjectCreator.AUD, 1M, 200M, 0M, 0M, 200M, 0M, 0M, testObjectCreator.CC3.PK);
				crd1Line.AL_AT = taxRate1.PK;
				var crd2 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "AR002", testObjectCreator.AUD, 1M, orgHeader);
				var crd2Line = testObjectCreator.CreateInvoiceLine(crd2, testObjectCreator.AUD, 1M, 80M, 0M, 0M, 80M, 0M, 0M, testObjectCreator.CC3.PK);
				crd2Line.AL_AT = taxRate1.PK;
				Factory.Save();

				InvoicingBaseTest.AssertStampDutyLiabilityEvent(crd1, true);
				AssertEquals("Invoice Line Count", 2, crd1.Lines.Count);
				var stampDutyLine = crd1.Lines[1];
				AssertEquals("Stamp Duty Line Charge Code PK", stampDutyLine.AL_AC, registry.StampDutyChargeCode.Value);
				AssertEquals("Stamp Duty Line Amount", stampDutyLine.AL_LineAmount, 100m);
				AssertEquals("Stamp Duty Line Charge Code", stampDutyLine.ChargeCode.AC_Code, "ZZBOLLO");

				InvoicingBaseTest.AssertStampDutyLiabilityEvent(crd2, true);
				AssertEquals("Invoice Line Count", 1, crd2.Lines.Count);
			}
		}

		public void TestRechargeStampDutyForDifferentAROrgTypes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccTaxRate taxRate1, taxRate2;
				var testObjectCreator = new TestObjectCreator(Factory);
				SetupStampDutyCalculationTaxIDsAndRegistry(testObjectCreator, out taxRate1, out taxRate2);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var invoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", testObjectCreator.AUD, 1M, orgHeader);
				var invoiceLine = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1M, 200M, 0M, 0M, 200M, 0M, 0M, testObjectCreator.CC3.PK);
				invoiceLine.AL_AT = taxRate1.PK;

				var regRecharge = AccountingConfigurationRegistry.Instance.StampDutyRecharge;
				var currCompGuid = GlbCompany.CurrentCompany.PK.ToGuid();

				//ALL
				var stampDutyRecharge = new StampDutyRecharge
				{
					StampDutyRechargeOrganizationType = Core.Constants.StampDutyRechargeOrganizationType.All,
					StampDutyRechargeTransactionType = Core.Constants.StampDutyRechargeTransactionType.All
				};
				regRecharge.SetValue(currCompGuid, Guid.Empty, Guid.Empty, stampDutyRecharge);
				orgHeader.OH_RL_NKClosestPort = "AUSYD";
				Assert("Stamp duty should be applicable", invoice.ShouldAddStampDuty());

				//LOC
				stampDutyRecharge = new StampDutyRecharge
				{
					StampDutyRechargeOrganizationType = Core.Constants.StampDutyRechargeOrganizationType.LocalOrganizations,
					StampDutyRechargeTransactionType = Core.Constants.StampDutyRechargeTransactionType.All
				};
				regRecharge.SetValue(currCompGuid, Guid.Empty, Guid.Empty, stampDutyRecharge);
				orgHeader.OH_RL_NKClosestPort = "AUSYD";
				Assert("Stamp duty should not be applicable", !invoice.ShouldAddStampDuty());
				orgHeader.OH_RL_NKClosestPort = "ITROM";
				Assert("Stamp duty should be applicable", invoice.ShouldAddStampDuty());

				//NIL
				stampDutyRecharge = new StampDutyRecharge
				{
					StampDutyRechargeOrganizationType = Core.Constants.StampDutyRechargeOrganizationType.NotRecharging,
					StampDutyRechargeTransactionType = Core.Constants.StampDutyRechargeTransactionType.All
				};
				regRecharge.SetValue(currCompGuid, Guid.Empty, Guid.Empty, stampDutyRecharge);
				orgHeader.OH_RL_NKClosestPort = "ITROM";
				Assert("Stamp duty should not be applicable", !invoice.ShouldAddStampDuty());
			}
		}

		public void TestRechargeStampDutyForDifferentTransactionTypes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccTaxRate taxRate1, taxRate2;
				var testObjectCreator = new TestObjectCreator(Factory);
				SetupStampDutyCalculationTaxIDsAndRegistry(testObjectCreator, out taxRate1, out taxRate2);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_RL_NKClosestPort = "ITROM";
				Factory.Save();

				var invoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", testObjectCreator.AUD, 1M, orgHeader);
				var invoiceLine = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1M, 200M, 0M, 0M, 200M, 0M, 0M, testObjectCreator.CC3.PK);
				invoiceLine.AL_AT = taxRate1.PK;

				var note = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "AR002", testObjectCreator.AUD, 1M, orgHeader);
				var noteLine = testObjectCreator.CreateInvoiceLine(note, testObjectCreator.AUD, 1M, 200M, 0M, 0M, 200M, 0M, 0M, testObjectCreator.CC3.PK);
				noteLine.AL_AT = taxRate1.PK;

				var regRecharge = AccountingConfigurationRegistry.Instance.StampDutyRecharge;
				var currCompGuid = GlbCompany.CurrentCompany.PK.ToGuid();

				//ALL
				var stampDutyRecharge = new StampDutyRecharge
				{
					StampDutyRechargeOrganizationType = Core.Constants.StampDutyRechargeOrganizationType.All,
					StampDutyRechargeTransactionType = Core.Constants.StampDutyRechargeTransactionType.All
				};
				regRecharge.SetValue(currCompGuid, Guid.Empty, Guid.Empty, stampDutyRecharge);
				Assert("Stamp duty should be applicable", invoice.ShouldAddStampDuty());
				Assert("Stamp duty should be applicable", note.ShouldAddStampDuty());

				//INV
				stampDutyRecharge = new StampDutyRecharge
				{
					StampDutyRechargeOrganizationType = Core.Constants.StampDutyRechargeOrganizationType.All,
					StampDutyRechargeTransactionType = Core.Constants.StampDutyRechargeTransactionType.ARInvoice
				};
				regRecharge.SetValue(currCompGuid, Guid.Empty, Guid.Empty, stampDutyRecharge);
				Assert("Stamp duty should be applicable", invoice.ShouldAddStampDuty());
				Assert("Stamp duty should not be applicable", !note.ShouldAddStampDuty());
			}
		}

		public void TestStampDutyForLocalDescriptionAndNoErrors()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var testObjectCreator = new TestObjectCreator(Factory);

				var taxRate = Factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "ART7")).First();
				taxRate.SetRateNumerator_ForTestOnly(0);
				var value = taxRate.PK.ToString();

				var registry = AccountingConfigurationRegistry.Instance;
				var currCompGuid = GlbCompany.CurrentCompany.PK.ToGuid();

				registry.TaxIDsAttractingStampDuty.SetValue(currCompGuid, Guid.Empty, Guid.Empty, value);
				registry.StampDutyFixedAmount.SetValue(currCompGuid, Guid.Empty, Guid.Empty, 2m);
				registry.StampDutyThreshold.SetValue(currCompGuid, Guid.Empty, Guid.Empty, 77.47m);
				registry.EnableLocalChargeCodeDescriptionDefault.SetValue(currCompGuid, Guid.Empty, Guid.Empty, true);

				var chargeCode = testObjectCreator.CreateChargeCode("BOLLO", "Stamp Duty", "NON", 1m, taxRate, testObjectCreator.WHTFREE1);
				chargeCode.AC_LocalLanguageDescription = "Bollo";
				registry.StampDutyChargeCode.SetValue(currCompGuid, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_RL_NKClosestPort = "ITROM";
				orgHeader.OH_IsDebtor = true;

				Factory.Save();

				var job = testObjectCreator.CreateJob(testObjectCreator.CreateShipment("S00001001"));
				var charge = testObjectCreator.CreateCharge(job, testObjectCreator.FRT, testObjectCreator.FRT.AC_Desc,
						testObjectCreator.EUR, 0M, null,
						testObjectCreator.EUR, 100M, orgHeader);
				charge.JR_AT_SellGSTRate = taxRate.PK;

				Factory.Save();

				var postManager = new InvoicingPostManager(job);
				var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
				Factory.Save();

				AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

				var arInvoices = transactions.GetAllARTransactions();
				var arInvoice = (ARInvoice)arInvoices[0];
				AssertEquals("AR Invoice Line Count", 2, arInvoice.Lines.Count);

				var bolloLine = arInvoice.Lines.Cast<ARInvoiceLine>().FirstOrDefault(l => l.ChargeCode.AC_Code.Contains("BOLLO"));
				AssertNotNull("Should have a bollo line", bolloLine);
				AssertEquals(2m, bolloLine.AL_LineAmount);
				AssertEquals("Should use local language description", "Bollo", bolloLine.AL_Desc);
				Assert("Should not have errors", !bolloLine.HasErrors);
			}
		}
	}
}
