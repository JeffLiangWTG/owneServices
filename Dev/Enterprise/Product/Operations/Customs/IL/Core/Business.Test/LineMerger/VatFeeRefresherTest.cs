using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IL.Business.Testing
{
	class VatFeeRefresherTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			AssertExceptionThrown<ArgumentNullException>(() => VatFeeRefresher<CusEntryLineFee, CusEntryLine>.New(null));
			AssertNoExceptionThrown(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				VatFeeRefresher<CusEntryLineFee, CusEntryLine>.New(entryLine.Fees);
			});
		}

		public void TestSystemAddedVatFeeCalculationSuspender()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				var vatRefresher = entryLine.Fees.VatRefresher as VatFeeRefresher<CusEntryLineFee, CusEntryLine>;

				AssertNotNull("entryLine.Fees.VatRefresher as VatFeeRefresher", vatRefresher);
				Assert("System added VAT fee calculation is not suspended", !vatRefresher.IsSystemAddedVatFeeCalculationSuspended);

				using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
				{
					Assert("System added VAT fee calculation is suspended", vatRefresher.IsSystemAddedVatFeeCalculationSuspended);
				}

				Assert("System added VAT fee calculation is not suspended", !vatRefresher.IsSystemAddedVatFeeCalculationSuspended);
			}
		}

		public void TestFactoryMethodDecidesTypeToInstantiateBasedOnUseUniversalFeeCalculationConfiguration()
		{
			var vatRefresherWithUniversalFeeCalculation = (VatCalculationTestHelper.SetUpEntryLine(Factory).Fees).VatRefresher;
			AssertNotNull("VatRefresher (Using Universal Fee Calculation)", vatRefresherWithUniversalFeeCalculation);
			AssertType<VatFeeRefresher<CusEntryLineFee, CusEntryLine>>("vatRefresherWithUniversalFeeCalculation", vatRefresherWithUniversalFeeCalculation);

			using (TemporarilySetUseUniversalFeeCalculationConfiguration(false))
			{
				var vatRefresherWithoutUniversalFeeCalculation = (VatCalculationTestHelper.SetUpEntryLine(Factory).Fees).VatRefresher;
				AssertNotNull("VatRefresher (Not Using Universal Fee Calculation)", vatRefresherWithoutUniversalFeeCalculation);
				AssertEquals("VatRefresher Type (Not Using Universal Fee Calculation)", "DummyVatRefresher", vatRefresherWithoutUniversalFeeCalculation.GetType().Name);
			}
		}

		public void TestDummyVatRefresher()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(false))
			{
				var entryLine = VatCalculationTestHelper.SetUpEntryLine(Factory);
				var vatRefresher = entryLine.Fees.VatRefresher;
				AssertEquals("VatRefresher Type", "DummyVatRefresher", vatRefresher?.GetType().Name);

				VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "", false);
				AssertEquals("[PRE-CONDITION] Entry Line fee count", 1, entryLine.Fees.Count);

				using (var suspensionDisposer = vatRefresher.SuspendSystemAddedVatFeeRecalculation())
				{
					AssertNotNull("DummyVatRefresher.SuspendSystemAddedVatFeeRecalculation()", suspensionDisposer);
					AssertEquals("DummyVatRefresher.SuspendSystemAddedVatFeeRecalculation() = DisposableAction.NoAction?", true, object.ReferenceEquals(suspensionDisposer, DisposableAction.NoAction));
				}

				AssertNoExceptionThrown(() => vatRefresher.Unhook());
			}
		}

		public void TestUpdateSystemAddedVatFeeIfAllowed()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(true))
			{
				VatCalculationTestHelper.SetUpAndSaveRefData(Factory);

				var entryLine = VatCalculationTestHelper.SetUpEntryLine(Factory);

				CusEntryLineFee systemDefinedVatFee;
				CusEntryLineFee systemDefinedDtyFee;

				using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
				{
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "ADD", true);
					systemDefinedDtyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 25m, 1212m, 456m, "", false);
					systemDefinedVatFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 22m, 1m, 1m, "", false);
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 33m, 123m, 543m, "ADD", true);
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, "%", 1m, 50m, 123.32m, "ADD", false);
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, "%", 1m, 50m, 123.32m, "ADD", false);
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, "110", "%", 1m, 50m, 123.32m, "ADD", false);
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, "110", "%", 1m, 50m, 123.32m, "ADD", true);
				}

				CombineAssertions("PRE-Condition", () =>
				{
					AssertNotNull(systemDefinedVatFee);
					AssertEquals("CF_Rate", 22m, systemDefinedVatFee.CF_Rate);
					AssertEquals("CF_BaseValue", 1m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 1m, systemDefinedVatFee.CF_ChargeAmount);
				});

				systemDefinedVatFee.CF_ChargeAmount = 0m;

				CombineAssertions("Changing system added VAT fee doesn't trigger its recalculation ", () =>
				{
					AssertEquals("CF_Rate", 22m, systemDefinedVatFee.CF_Rate);
					AssertEquals("CF_BaseValue", 1m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 0m, systemDefinedVatFee.CF_ChargeAmount);
				});

				systemDefinedDtyFee.CF_ChargeAmount = 0m;
				systemDefinedDtyFee.CF_ChargeAmount = 456m;

				CombineAssertions("Updated VAT", () =>
				{
					AssertNotNull(systemDefinedVatFee);
					AssertEquals("CF_Rate", 22m, systemDefinedVatFee.CF_Rate);
					AssertEquals("CF_BaseValue", 907.35m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 199.6170m, systemDefinedVatFee.CF_ChargeAmount);
				});
			}
		}

		public void TestUpdateSystemAddedVatFeeIfAllowedWithoutExistingSystemAddedVat()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(true))
			{
				VatCalculationTestHelper.SetUpAndSaveRefData(Factory);

				var entryLine = VatCalculationTestHelper.SetUpEntryLine(Factory);

				CusEntryLineFee systemDefinedDtyFee;

				using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
				{
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "ADD", true);
					systemDefinedDtyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 25m, 1212m, 456m, "", false);
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 33m, 123m, 543m, "ADD", true);
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, "%", 1m, 50m, 123.32m, "ADD", false);
				}

				AssertNull("PRE-Condition", GetSystemDefinedVatFee(entryLine));

				AssertNoExceptionThrown("Triggering VAT recalculation", () =>
				{
					systemDefinedDtyFee.CF_ChargeAmount = 0m;
					systemDefinedDtyFee.CF_ChargeAmount = 456m;
				});

				AssertNull("POST-Condition", GetSystemDefinedVatFee(entryLine));
			}
		}

		public void TestUpdateSystemAddedVatFeeIfAllowedWithoutUniversalFeeCalculation()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(false))
			{
				VatCalculationTestHelper.SetUpAndSaveRefData(Factory);

				var entryLine = VatCalculationTestHelper.SetUpEntryLine(Factory);
				AssertNotNull("VatRefresher", entryLine.Fees.VatRefresher);

				CusEntryLineFee systemDefinedVatFee;
				CusEntryLineFee systemDefinedDtyFee;

				systemDefinedVatFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 22m, 1m, 1m, "", false);
				systemDefinedDtyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 25m, 1212m, 456m, "", false);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "ADD", true);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 33m, 123m, 543m, "ADD", true);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, "%", 1m, 50m, 123.32m, "ADD", false);

				CombineAssertions("No changes when fees are added", () =>
				{
					AssertNotNull(systemDefinedVatFee);
					AssertEquals("CF_Rate", 22m, systemDefinedVatFee.CF_Rate);
					AssertEquals("CF_BaseValue", 1m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 1m, systemDefinedVatFee.CF_ChargeAmount);
				});

				systemDefinedDtyFee.CF_ChargeAmount += 10m;

				CombineAssertions("No changes when a fee is modified", () =>
				{
					AssertNotNull(systemDefinedVatFee);
					AssertEquals("CF_Rate", 22m, systemDefinedVatFee.CF_Rate);
					AssertEquals("CF_BaseValue", 1m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 1m, systemDefinedVatFee.CF_ChargeAmount);
				});
			}
		}

		public void TestSystemVatFeeReCalculated_OnItemRemoved()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(true))
			{
				var ordVat = VatCalculationTestHelper.SetUpAndSaveRefData(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryHeader.PK;
				invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;

				CusEntryLineFee systemDefinedDtyFee;
				CusEntryLineFee systemDefinedVatFee;
				CusEntryLineFee userDefinedDtyFee;

				using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
				{
					systemDefinedDtyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "", false);
					userDefinedDtyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 22m, 2222m, 220m, "ADD", false);
					systemDefinedVatFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 11m, 320m, 35.2m, "", false);
				}

				CombineAssertions("PRE-Condition", () =>
				{
					AssertEquals("CF_BaseValue", 320m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 35.2m, systemDefinedVatFee.CF_ChargeAmount);
				});

				entryLine.Fees.RemoveAndDelete(userDefinedDtyFee);

				CombineAssertions("System added VAT has been updated", () =>
				{
					AssertEquals("CF_BaseValue", 135.2m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 14.872m, systemDefinedVatFee.CF_ChargeAmount);
				});
			}
		}

		public void TestSystemVatFeeReCalculated_OnCusEntryLineFeeChange()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(true))
			{
				VatCalculationTestHelper.SetUpAndSaveRefData(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();

				CusEntryLineFee systemDefinedDtyFee;
				CusEntryLineFee systemDefinedVatFee;
				CusEntryLineFee userDefinedVatFee;
				CusEntryLineFee userDefinedDtyFee;

				using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
				{
					systemDefinedDtyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "", false);
					systemDefinedVatFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 11m, 100m, 11m, "", false);
				}

				userDefinedVatFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 5, 50m, 2.5m, "ADD", false);
				CombineAssertions("No changes on system added VAT", () =>
				{
					AssertEquals("CF_BaseValue", 114.84m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 12.6324m, systemDefinedVatFee.CF_ChargeAmount);
				});

				userDefinedDtyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 22m, 2222m, 220m, "ADD", false);
				CombineAssertions("A new DTY has been added and system added VAT has been updated", () =>
				{
					AssertEquals("CF_BaseValue", 335.17m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 36.8687m, systemDefinedVatFee.CF_ChargeAmount);
				});

				systemDefinedDtyFee.CF_ChargeAmount = 0m;
				CombineAssertions("An existing DTY has been edited and system added VAT has been updated", () =>
				{
					AssertEquals("CF_BaseValue", 259.37m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 28.5307m, systemDefinedVatFee.CF_ChargeAmount);
				});

				systemDefinedDtyFee.CF_ChargeAmount = 9999m;
				systemDefinedDtyFee.CF_IsLandedCostOnly = true;
				CombineAssertions("An existing DTY has been edited but system added VAT has not been updated", () =>
				{
					AssertEquals("CF_BaseValue", 1350m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 148.5000m, systemDefinedVatFee.CF_ChargeAmount);
				});
			}
		}

		public void TestSystemAddedVatFeeNotCalculatedWhenInterfacedDeclaration()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(true))
			{
				var ordVat = VatCalculationTestHelper.SetUpAndSaveRefData(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryHeader.PK;
				invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;

				CusEntryLineFee systemDefinedDtyFee;
				CusEntryLineFee systemDefinedVatFee;

				using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
				{
					systemDefinedDtyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "", false);
					systemDefinedVatFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 11m, 100m, 11m, "", false);
				}

				CombineAssertions("PRE-Condition", () =>
				{
					Assert(declaration.IsDeclarationIntegrated);
					AssertEquals("CF_BaseValue", 100m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 11m, systemDefinedVatFee.CF_ChargeAmount);
				});

				systemDefinedDtyFee.CF_ChargeAmount = 0m;
				CombineAssertions("POST-Condition: no changes", () =>
				{
					AssertEquals("CF_BaseValue", 100m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 11m, systemDefinedVatFee.CF_ChargeAmount);
				});
			}
		}

		/// <summary>
		/// VAT Refresher should be instantiated allong with Fee Collection.
		/// So in order to dynamically change existing VAT fee when other fees change, events must be hooked upon instantiation of the entry line fee collection.
		/// </summary>
		public void TestEntryFeeCollectionVatRefreshingEventsAreHookedWithoutExplicitlyReferencingVatRefresher()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(true))
			{
				VatCalculationTestHelper.SetUpAndSaveRefData(Factory);

				var entryLine = VatCalculationTestHelper.SetUpEntryLine(Factory);

				VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage, 22m, 1m, 1m, "", false);
				AssertEquals("[PRE-CONDITION] Fee Count", 1, entryLine.Fees.Count);

				CombineAssertions("[PRE-CONDITION] Pre-Existing VAT Fee", () =>
				{
					var fee = entryLine.Fees[0];
					AssertEquals("CF_ChargeType", Constants.EntryLineFee.VATFeeTypeCode, fee.CF_ChargeType);
					AssertEquals("CF_Rate", 22m, fee.CF_Rate);
					AssertEquals("CF_BaseValue", 1m, fee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 1m, fee.CF_ChargeAmount);
				});

				VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 25m, 1212m, 456m, ILRateOverrideReasonList.Codes.Additional, false);
				var dtyFee = entryLine.Fees.Cast<CusEntryLineFee>().SingleOrDefault(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
				var vatFee = GetSystemDefinedVatFee(entryLine);

				CombineAssertions("New DTY and Updated VAT fees", () =>
				{
					AssertEquals("Fee Count", 2, entryLine.Fees.Count);
					AssertNotNull("Duty Fee", dtyFee);
					AssertNotNull("VAT Fee", vatFee);
				});

				CombineAssertions("Updated VAT fee", () =>
				{
					AssertEquals("CF_Rate", 22m, vatFee.CF_Rate);
					AssertEquals("CF_BaseValue", 456.05m, vatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 100.3310m, vatFee.CF_ChargeAmount);
					AssertEquals("Base DTY Fee => CF_ChargeAmount", 456m, dtyFee.CF_ChargeAmount);
				});
			}
		}

		public void TestSystemAddedVatFeeNotCalculatedWhenEntryLineHasNoDeclaration()
		{
			using (TemporarilySetUseUniversalFeeCalculationConfiguration(true))
			{
				var ordVat = VatCalculationTestHelper.SetUpAndSaveRefData(Factory);

				var entryLine = Factory.New<CusEntryLine>();
				var invoiceLine = entryLine.InvoiceLines.AddNew();
				invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
				AssertNotNull("VatRefresher", entryLine.Fees.VatRefresher);

				CusEntryLineFee systemDefinedDtyFee;
				CusEntryLineFee systemDefinedVatFee;

				systemDefinedVatFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 11m, 100m, 11m, "", false);
				systemDefinedDtyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "", false);

				CombineAssertions("No changes when a fee is added", () =>
				{
					AssertNull(entryLine.Declaration);
					AssertEquals("CF_BaseValue", 100m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 11m, systemDefinedVatFee.CF_ChargeAmount);
				});

				systemDefinedDtyFee.CF_ChargeAmount = 0m;

				CombineAssertions("No changes when a fee is modified", () =>
				{
					AssertEquals("CF_BaseValue", 100m, systemDefinedVatFee.CF_BaseValue);
					AssertEquals("CF_ChargeAmount", 11m, systemDefinedVatFee.CF_ChargeAmount);
				});
			}
		}

		IDisposable TemporarilySetUseUniversalFeeCalculationConfiguration(bool useUniversalFeeCalculation)
		{
			var originalProvider = DeclarationConfigurationProvider.Provider;
			var mock = new Mock<IDeclarationConfigurationProvider>();
			mock.Setup(i => i.UseUniversalFeeCalculation).Returns(useUniversalFeeCalculation);
			DeclarationConfigurationProvider.Provider = mock.Object;

			return new DisposableAction(() =>
			{
				DeclarationConfigurationProvider.Provider = originalProvider;
			});
		}

		CusEntryLineFee GetSystemDefinedVatFee(CusEntryLine entryLine) => entryLine.Fees.Cast<CusEntryLineFee>().SingleOrDefault(x => x.CF_ChargeType == Constants.EntryLineFee.VATFeeTypeCode && x.CF_RateOverrideReasonCode.IsEmpty);
	}
}
