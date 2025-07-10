using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	public class ConsolCostSelectorTest : TestCaseWithFactory
	{
		#region Consol Cost Selection Tests

		/*
		 * 3 Consol Costs with same charge Code. But creditor, currency and amount of these Costs will be changed to create one of the following scenario.
		 * S = Same
		 * D = Different.
		 * 
		 *		  CASE #	Creditor		Currency		Amount
		 *		--------------------------------------------------
					1			S				S				S
					2			S				S				D
					3			S				D				S
					4			S				D				D
					5			D				S				S
					6			D				S				D
					7			D				D				S
					8			D				D				D
		 */

		public void TestConsolCostSelectionCase1()
		{
			foreach (bool bringForwardAgainsCreditor in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(
					EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bringForwardAgainsCreditor))
				{
					var bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("Selector will select randomly one cost from three consol costs", true, new[] { cost1, cost2, cost3 }.Contains(bestmatchedCost));

					SetValues(ObjectCreator.Creditor2.PK, "USD", 400M, 1.0M, "CHG", cost1, cost2, cost3);
					bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					if (bringForwardAgainsCreditor)
					{
						AssertNull("No Matching cost should be found", bestmatchedCost);
					}
					else
					{
						AssertEquals("Selector will select randomly one cost from three consol costs", true, new[] { cost1, cost2, cost3 }.Contains(bestmatchedCost));
					}

					SetValues(ObjectCreator.Creditor1.PK, "USD", 300M, 1.0M, "CHG", cost1, cost2, cost3, costToMatchWith);
					bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("Selector will select randomly one cost from three consol costs", true, new[] { cost1, cost2, cost3 }.Contains(bestmatchedCost));

					SetValues(ZGuid.Empty, "USD", 300M, 1.0M, "CHG", cost1, cost2, cost3);
					SetValues(ZGuid.Empty, "USD", 300M, 1.0M, "CHG", costToMatchWith);
					bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("Selector will select randomly one cost from three consol costs", true, new[] { cost1, cost2, cost3 }.Contains(bestmatchedCost));
				}
			}
		}

		public void TestConsolCostSelectionCase2()
		{
			foreach (bool bringForwardAgainsCreditor in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(
					EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bringForwardAgainsCreditor))
				{
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost2, costToMatchWith);
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200.05M, 1.0M, "CHG", cost1);
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 190M, 1.0M, "CHG", cost3);
					var bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("cost2 should be selected", cost2.PK, bestmatchedCost.PK);

					SetValues(ObjectCreator.Creditor1.PK, "AUD", -200M, 1.0M, "CHG", cost2);
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost3);
					bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("Cost3 should be selected", cost3.PK, bestmatchedCost.PK);
				}
			}
		}

		public void TestConsolCostSelectionCase3()
		{
			foreach (bool bringForwardAgainsCreditor in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(
					EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bringForwardAgainsCreditor))
				{
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost2, costToMatchWith);
					SetValues(ObjectCreator.Creditor1.PK, "USD", 200M, 1.0M, "CHG", cost1);
					SetValues(ObjectCreator.Creditor1.PK, "EUR", 200M, 1.0M, "CHG", cost3);
					var bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("cost2 should be selected", cost2.PK, bestmatchedCost.PK);

					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost1);
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost2);
					SetValues(ObjectCreator.Creditor1.PK, "USD", 200M, 1.0M, "CHG", cost3);
					bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("Either Cost1 or Cost2 should be selected", true,
						new ZGuid[] { cost1.PK, cost2.PK }.Contains(bestmatchedCost.PK));
				}
			}
		}

		public void TestConsolCostSelectionCase4()
		{
			foreach (bool bringForwardAgainsCreditor in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(
					EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bringForwardAgainsCreditor))
				{
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost2, costToMatchWith);
					SetValues(ObjectCreator.Creditor1.PK, "USD", 200M, 1.0M, "CHG", cost1);
					SetValues(ObjectCreator.Creditor1.PK, "EUR", 190M, 1.0M, "CHG", cost3);
					var bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("cost2 should be selected", cost2.PK, bestmatchedCost.PK);
				}
			}
		}

		public void TestConsolCostSelectionCase5()
		{
			foreach (bool bringForwardAgainsCreditor in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(
					EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bringForwardAgainsCreditor))
				{
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost2, costToMatchWith);
					SetValues(ObjectCreator.Creditor2.PK, "AUD", 200M, 1.0M, "CHG", cost1);
					SetValues(Guid.Empty, "AUD", 200M, 1.0M, "CHG", cost3);
					var bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("cost2 should be selected", cost2.PK, bestmatchedCost.PK);
				}
			}
		}

		public void TestConsolCostSelectionCase6()
		{
			foreach (bool bringForwardAgainsCreditor in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(
					EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bringForwardAgainsCreditor))
				{
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost2, costToMatchWith);
					SetValues(ObjectCreator.Creditor2.PK, "AUD", 190M, 1.0M, "CHG", cost1);
					SetValues(Guid.Empty, "AUD", 780M, 1.0M, "CHG", cost3);
					var bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("cost2 should be selected", cost2.PK, bestmatchedCost.PK);
				}
			}
		}

		public void TestConsolCostSelectionCase7()
		{
			foreach (bool bringForwardAgainsCreditor in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(
					EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bringForwardAgainsCreditor))
				{
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost2, costToMatchWith);
					SetValues(ObjectCreator.Creditor2.PK, "EUR", 200M, 1.0M, "CHG", cost1);
					SetValues(Guid.Empty, "USD", 200M, 1.0M, "CHG", cost3);
					var bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("cost2 should be selected", cost2.PK, bestmatchedCost.PK);
				}
			}
		}

		public void TestConsolCostSelectionCase8()
		{
			foreach (bool bringForwardAgainsCreditor in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(
					EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bringForwardAgainsCreditor))
				{
					SetValues(ObjectCreator.Creditor1.PK, "AUD", 200M, 1.0M, "CHG", cost2, costToMatchWith);
					SetValues(ObjectCreator.Creditor2.PK, "EUR", 190M, 1.0M, "CHG", cost1);
					SetValues(Guid.Empty, "USD", 180M, 1.0M, "CHG", cost3);
					var bestmatchedCost = ConsolCostSelector.GetBestMatch(costToMatchWith, new[] { cost1, cost2, cost3 });
					AssertEquals("cost2 should be selected", cost2.PK, bestmatchedCost.PK);
				}
			}
		}

		#endregion

		#region Apportion Method Selection Tests
		/*
		 
			S = Same
			D = Different

			Scneario #	|	Creditor	|	Currency	|	Amount	|	Comment
			--------------------------------------------------------------------------------
				1				S				S			 S			All crediotrs are Empty
				1.1				S				S			 S			All crediotrs are Non-Empty
				2				S				S			 D			Amounts are different
				2.1				S				S			 D			Amounts are different in sign only
				3				S				D			 S
				4				S				D			 D			
				5				D				S			 S			Creditors are Different. Mixutre of Different Valid and Empty Creditor
				6				D				S			 D
				7				D				D			 S
				8				D				D			 D


		 */

		public void TestApportionMethodSelection_Case1()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 250M, ApportionMethod = "MAN" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 250M, ApportionMethod = "CHG" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 250M, ApportionMethod = "SHP" };

			var expectedAppMethod = appMethodInfo1.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "AUD", 350M)?.ApportionMethod;
			AssertEquals("Apportion Method: As all candidate apportion method info have the same value, system will pick the first one", expectedAppMethod, actualAppMethod);

			expectedAppMethod = appMethodInfo1.ApportionMethod;
			actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "USD", 350M)?.ApportionMethod;
			AssertEquals("Apportion Method: As all candidate apportion method info have the same value, system will pick the first one", expectedAppMethod, actualAppMethod);
		}

		public void TestApportionMethodSelection_Case1_1()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor1.PK, Currency = "AUD", Amount = 250M, ApportionMethod = "MAN" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor1.PK, Currency = "AUD", Amount = 250M, ApportionMethod = "CHG" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor1.PK, Currency = "AUD", Amount = 250M, ApportionMethod = "SHP" };

			var expectedAppMethod = appMethodInfo1.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "AUD", 350M)?.ApportionMethod;
			AssertEquals("Apportion Method: As all candidate apportion method info have the same value, system will pick the first one", expectedAppMethod, actualAppMethod);

			appMethodInfo1.CreditorPK = ObjectCreator.Creditor2.PK;
			appMethodInfo2.CreditorPK = ObjectCreator.Creditor2.PK;
			appMethodInfo3.CreditorPK = ObjectCreator.Creditor2.PK;

			foreach (var regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue))
				{
					expectedAppMethod = regValue ? "" : "MAN";
					actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "USD", 350M)?.ApportionMethod ?? ZString.Empty;
					AssertEquals("Apportion Method: As all candidate apportion method info have a creditor that doesn't match with the provided creditor, system will pick / not pick any app method based on the value of BringForwardAgainstCreditor registry", expectedAppMethod, actualAppMethod);
				}
			}
		}

		public void TestApportionMethodSelection_Case2()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 250M, ApportionMethod = "MAN" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 150M, ApportionMethod = "CHG" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 50M, ApportionMethod = "SHP" };

			var expectedAppMethod = appMethodInfo2.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "AUD", 130M)?.ApportionMethod;
			AssertEquals("Apportion Method", expectedAppMethod, actualAppMethod);
		}

		public void TestApportionMethodSelection_Case2_1()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = -250M, ApportionMethod = "MAN" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 150M, ApportionMethod = "CHG" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 250M, ApportionMethod = "SHP" };

			var expectedAppMethod = appMethodInfo3.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "AUD", 250M)?.ApportionMethod;
			AssertEquals("Apportion Method", expectedAppMethod, actualAppMethod);
		}

		public void TestApportionMethodSelection_Case3()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 130M, ApportionMethod = "MAN" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "EUR", Amount = 130M, ApportionMethod = "CHG" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "USD", Amount = 130M, ApportionMethod = "SHP" };

			var expectedAppMethod = appMethodInfo2.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "EUR", 130M)?.ApportionMethod;
			AssertEquals("Apportion Method", expectedAppMethod, actualAppMethod);
		}

		public void TestApportionMethodSelection_Case4()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 250M, ApportionMethod = "MAN" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "EUR", Amount = 250M, ApportionMethod = "CHG" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "EUR", Amount = 180M, ApportionMethod = "SHP" };

			var expectedAppMethod = appMethodInfo3.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "EUR", 130M)?.ApportionMethod;
			AssertEquals("Apportion Method", expectedAppMethod, actualAppMethod);
		}

		public void TestApportionMethodSelection_Case5()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor1.PK, Currency = "AUD", Amount = 250M, ApportionMethod = "SHP" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor2.PK, Currency = "AUD", Amount = 250M, ApportionMethod = "CHG" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 250M, ApportionMethod = "MAN" };

			var expectedAppMethod = appMethodInfo2.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor2.PK, "AUD", 130M)?.ApportionMethod;
			AssertEquals("Apportion Method", expectedAppMethod, actualAppMethod);
		}

		public void TestApportionMethodSelection_Case6()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 250M, ApportionMethod = "SHP" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor2.PK, Currency = "AUD", Amount = 250M, ApportionMethod = "CHG" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 180M, ApportionMethod = "MAN" };

			var expectedAppMethod = appMethodInfo3.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "AUD", 130M)?.ApportionMethod;
			AssertEquals("Apportion Method", expectedAppMethod, actualAppMethod);
		}

		public void TestApportionMethodSelection_Case7()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "USD", Amount = 180M, ApportionMethod = "SHP" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor2.PK, Currency = "EUR", Amount = 180M, ApportionMethod = "CHG" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 180M, ApportionMethod = "MAN" };

			var expectedAppMethod = appMethodInfo3.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3 }, ObjectCreator.Creditor1.PK, "AUD", 130M)?.ApportionMethod;
			AssertEquals("Apportion Method", expectedAppMethod, actualAppMethod);
		}

		public void TestApportionMethodSelection_Case8()
		{
			var appMethodInfo1 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor1.PK, Currency = "USD", Amount = 150M, ApportionMethod = "MAN" };
			var appMethodInfo2 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor2.PK, Currency = "EUR", Amount = 150M, ApportionMethod = "SHP" };
			var appMethodInfo3 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ZGuid.Empty, Currency = "AUD", Amount = 180M, ApportionMethod = "REV" };
			var appMethodInfo4 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor1.PK, Currency = "USD", Amount = 180M, ApportionMethod = "GWT" };
			var appMethodInfo5 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor1.PK, Currency = "AUD", Amount = 250M, ApportionMethod = "CNT" };
			var appMethodInfo6 = new ConsolCostSelector.ApportionMethodSelectionInfo() { CreditorPK = ObjectCreator.Creditor1.PK, Currency = "AUD", Amount = 150M, ApportionMethod = "OPT" };

			var expectedAppMethod = appMethodInfo6.ApportionMethod;
			var actualAppMethod = ConsolCostSelector.GetBestMatchedApportionMethod(new[] { appMethodInfo1, appMethodInfo2, appMethodInfo3, appMethodInfo4, appMethodInfo5, appMethodInfo6 }, ObjectCreator.Creditor1.PK, "AUD", 130M)?.ApportionMethod;
			AssertEquals("Apportion Method", expectedAppMethod, actualAppMethod);
		}

		#endregion

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			consol = ObjectCreator.CreateConsol();
			ObjectCreator.CreateShipment("S00001", consol);
			ObjectCreator.CreateShipment("S00002", consol);

			cost1 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, null);
			cost2 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, null);
			cost3 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, null);
			costToMatchWith = ObjectCreator.CreateConsolCost(consol, objectCreator.CC1, null);

			Factory.Save();
		}

		void SetValues(ZGuid creditor, ZString currency, ZDecimal amount, ZDecimal exchangeRate, ZString apportionMethod,
			params JobConsolCost[] costs)
		{
			foreach (JobConsolCost cost in costs)
			{
				cost.E6_ApportionmentMethod = apportionMethod;
				cost.E6_OH_Creditor = creditor;
				cost.E6_RX_NKCurrency = currency;
				cost.E6_ExchangeRate = exchangeRate;
				cost.E6_LocalCostAmount = amount;
			}
		}

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (objectCreator == null)
				{
					objectCreator = new TestObjectCreator(Factory);
				}
				return objectCreator;
			}
		}

		TestObjectCreator objectCreator;

		JobConsolCost cost1, cost2, cost3, costToMatchWith;

		ForwardingConsol consol;

		#endregion
	}
}
