using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class TransactionBillingSystemTest : TestCaseWithFactory
	{
		public void TestLoadUsages()
		{
			ZDateTime periodStart = new ZDateTime(2011, 10, 01);
			ClientChargeableUsage chargeableUsage1 = CreateChargeableUsage(organisation1, periodStart, 10);
			ClientChargeableUsage chargeableUsage2 = CreateChargeableUsage(childOrganisation11, periodStart, 11);
			ClientChargeableUsage chargeableUsageLastMonth1 = CreateChargeableUsage(organisation2, periodStart.AddMonths(-1), 20);
			ClientChargeableUsage chargeableUsage3 = CreateChargeableUsage(organisation2, periodStart, 20);
			chargeableUsage3.U1_SubCode = "C3A";
			ClientChargeableUsage chargeableUsage3b = CreateChargeableUsage(organisation2, periodStart, 50);
			chargeableUsage3b.U1_SubCode = "C3B";
			ClientChargeableUsage chargeableUsage3bI = CreateChargeableUsage(organisation2, periodStart, 40);
			chargeableUsage3bI.U1_SubCode = "C3B";
			chargeableUsage3bI.U1_Reference1 = "AU";
			chargeableUsage3bI.U1_Reference2 = "BNE";
			ClientChargeableUsage chargeableUsage3bII = CreateChargeableUsage(organisation2, periodStart, 30);
			chargeableUsage3bII.U1_SubCode = "C3B";
			chargeableUsage3bII.U1_Reference1 = "AU";
			chargeableUsage3bII.U1_Reference2 = "SYD";
			ClientChargeableUsage chargeableUsage4b = CreateChargeableUsage(organisation2, periodStart, 10);
			chargeableUsage4b.U1_SubCode = "C4B";
			chargeableUsage4b.U1_Reference1 = "NL";
			chargeableUsage4b.U1_Reference2 = "ABC";
			chargeableUsage4b.U1_Reference3 = "AIREXP";
			ClientChargeableUsage chargeableUsage4bI = CreateChargeableUsage(organisation2, periodStart, 20);
			chargeableUsage4bI.U1_SubCode = "C4B";
			chargeableUsage4bI.U1_Reference1 = "NL";
			chargeableUsage4bI.U1_Reference2 = "ABC";
			chargeableUsage4bI.U1_Reference3 = "AIRIMP";
			ClientChargeableUsage chargeableUsage4bII = CreateChargeableUsage(organisation2, periodStart, 30);
			chargeableUsage4bII.U1_SubCode = "C4B";
			chargeableUsage4bII.U1_Reference1 = "NL";
			chargeableUsage4bII.U1_Reference2 = "ABC";
			chargeableUsage4bII.U1_Reference3 = "SEAEXP";
			ClientChargeableUsage chargeableWithInactiveOrg = CreateChargeableUsage(organisationInactive, periodStart, 10);

			var chargeableUsage1Test = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, organisation1Test, 333);

			Factory.Save();

			DummyTransactionBillingSystem billingSystem = new DummyTransactionBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2011, 10, 31));

			List<PriceItemUsage> allUsages = new List<PriceItemUsage>();
			foreach (SystemBill bill in billingSystem.LoadSystemBills(context))
			{
				allUsages.AddRange(bill.SystemUsages.Cast<PriceItemUsage>());
			}

			AssertEquals("usages", 5, allUsages.Count);
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1Test });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage2 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsageLastMonth1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3, chargeableUsage3b, chargeableUsage3bI, chargeableUsage3bII, chargeableUsage4b, chargeableUsage4bI, chargeableUsage4bII });

			DummyTransactionBillingSystem billingSystemIncludeSubCode = new DummyTransactionBillingSystem(true);
			allUsages.RemoveAll(x => true);
			foreach (SystemBill bill in billingSystemIncludeSubCode.LoadSystemBills(context))
			{
				allUsages.AddRange(bill.SystemUsages.Cast<PriceItemUsage>());
			}

			AssertEquals("usages", 7, allUsages.Count);
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1Test });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage2 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsageLastMonth1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3b, chargeableUsage3bI, chargeableUsage3bII });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage4b, chargeableUsage4bI, chargeableUsage4bII });

			DummyTransactionBillingSystem billingSystemIncludeReference1And2 = new DummyTransactionBillingSystem(false, true, true);
			allUsages.RemoveAll(x => true);
			foreach (SystemBill bill in billingSystemIncludeReference1And2.LoadSystemBills(context))
			{
				allUsages.AddRange(bill.SystemUsages.Cast<PriceItemUsage>());
			}

			AssertEquals("usages", 8, allUsages.Count);
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1Test });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage2 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsageLastMonth1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3, chargeableUsage3b });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3bI });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3bII });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage4b, chargeableUsage4bI, chargeableUsage4bII });

			DummyTransactionBillingSystem billingSystemIncludeSubCodeAndReference1And2 = new DummyTransactionBillingSystem(true, true, true);
			allUsages.RemoveAll(x => true);
			foreach (SystemBill bill in billingSystemIncludeSubCodeAndReference1And2.LoadSystemBills(context))
			{
				allUsages.AddRange(bill.SystemUsages.Cast<PriceItemUsage>());
			}

			AssertEquals("usages", 9, allUsages.Count);
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1Test });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage2 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsageLastMonth1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3b });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3bI });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3bII });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage4b, chargeableUsage4bI, chargeableUsage4bII });

			DummyTransactionBillingSystem billingSystemIncludeSubCodeAndReference1And2And3 = new DummyTransactionBillingSystem(true, true, true, true);
			allUsages.RemoveAll(x => true);
			foreach (SystemBill bill in billingSystemIncludeSubCodeAndReference1And2And3.LoadSystemBills(context))
			{
				allUsages.AddRange(bill.SystemUsages.Cast<PriceItemUsage>());
			}

			AssertEquals("usages", 11, allUsages.Count);
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage1Test });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage2 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsageLastMonth1 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3 });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3b });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3bI });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage3bII });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage4b });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage4bI });
			AssertUsage(allUsages, new ClientChargeableUsage[] { chargeableUsage4bII });
		}

		ClientChargeableUsage CreateChargeableUsage(EDIOrgHeader organisation, ZDateTime periodStart, ZInt unitCount)
		{
			var licHeader = organisation.LicCompany.LicHeadersForAllDatabases[0];
			return BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, licHeader, unitCount);
		}

		void AssertUsage(IEnumerable<PriceItemUsage> allUsages, ClientChargeableUsage[] chargeableUsages)
		{
			PriceItemUsage systemUsage = allUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsages[0].PK));

			AssertEquals("DUM", systemUsage.SystemCode);
			AssertEquals(chargeableUsages[0].OrganisationPK, systemUsage.OrganisationPK);
			AssertEquals((ZInt)chargeableUsages.Sum(x => x.U1_UnitCount), systemUsage.TransactionCount);
			AssertEquals(chargeableUsages[0].U1_PeriodStart, systemUsage.PeriodStart);

			ZString expectedServerCode = chargeableUsages[0].LicenceDatabase != null ? chargeableUsages[0].LicenceDatabase.LD_ServerCode : ZString.Empty;
			AssertEquals(expectedServerCode, systemUsage.ServerCode);

			foreach (ClientChargeableUsage chargeableUsage in chargeableUsages)
			{
				AssertEquals("system usage contains all related chargeable usages", true, systemUsage.ChargeableUsagePKs.Contains(chargeableUsage.PK));
			}
		}

		#region Implementation

		EDIOrgHeader organisation1;
		EDIOrgHeader organisation2;
		EDIOrgHeader organisation3;
		EDIOrgHeader organisationInactive;
		EDIOrgHeader childOrganisation11;
		LicenceHeader organisation1Test;

		protected override void SetUp()
		{
			base.SetUp();

			organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			childOrganisation11 = BillingTestHelper.CreateDependentOrganisation(organisation1, "DDA");
			organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			organisation3 = BillingTestHelper.CreateOrganisation(Factory, "CCC");
			organisation3.LicCompany.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = organisation2.PK;
			organisation1Test = BillingTestHelper.CreateAnotherDatabase(organisation1.LicCompany.LicHeadersForAllDatabases[0], "TST");
			organisation1Test.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			organisationInactive = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			organisationInactive.OH_IsActive = false;

			DummyBusinessObject dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Guid = organisation1.PK;
			dummy1.Z0_Date = new ZDateTime(2010, 10, 10);
			dummy1.Z0_Number = 16;

			DummyBusinessObject dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Guid = organisation2.PK;
			dummy2.Z0_Date = new ZDateTime(2010, 10, 10);
			dummy2.Z0_Number = 32;

			DummyBusinessObject dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy3.Z0_Guid = organisation3.PK;
			dummy3.Z0_Date = new ZDateTime(2010, 10, 10);
			dummy3.Z0_Number = 64;

			DummyBusinessObject dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy4.Z0_Guid = organisationInactive.PK;
			dummy4.Z0_Date = new ZDateTime(2010, 10, 10);
			dummy4.Z0_Number = 128;

			Factory.Save();
		}

		#region DummyTransactionBillingSystem

		class DummyTransactionBillingSystem : TransactionBillingSystem
		{
			public DummyTransactionBillingSystem(bool includeSubCode = false, bool includeReference1 = false, bool includeReference2 = false, bool includeReference3 = false)
			{
				IncludeSubCodeInSystemUsage = includeSubCode;
				IncludeReference1InSystemUsage = includeReference1;
				IncludeReference2InSystemUsage = includeReference2;
				IncludeReference3InSystemUsage = includeReference3;
			}

			public override string SystemCode
			{
				get { return "DUM"; }
			}

			protected override SystemBill CreateSystemBill()
			{
				return new SystemBill(Context.Factory);
			}

			protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
			{
				var result = new DummyRawUsage(context);
				while (reader.Read())
				{
					Guid organisationPK = (Guid)reader[DummyBusinessObject.Schema.Z0_Guid];
					int amount = (int)reader[DummyBusinessObject.Schema.Z0_Number];

					if (organisationPK == context.OrganisationPK)
					{
						result.RawLines.Add("Amount: " + amount.ToString());
					}
				}

				return result;
			}

			protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
			{
				return new StlRawUsage(context);
			}

			public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
			{
			}

			public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
			{
			}

			protected override string Query_Raw_Usage
			{
				get { return query_Raw_Usage; }
			}

			const string query_Raw_Usage =
@"SELECT Z0_Guid, Z0_Number FROM dbo.DummyBizo
WHERE Z0_Date >= @DateFrom
  AND Z0_Date < @DateTo 
  AND Z0_Guid = @OrgPk
";
		}

		#endregion

		#endregion
	}
}
