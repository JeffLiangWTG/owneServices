using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceBillingDiscountCollection))]
	internal class ClientLicenceBillingDiscountCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientLicenceBillingDiscountCollection>
	{
		#region Calculate Discounts

		public void TestCalculateTransactionalDiscount()
		{
			ClientLicenceBillingDiscount minimumFeeDiscount = Collection.AddNew();
			minimumFeeDiscount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			minimumFeeDiscount.L5_BreakAmount = 150m;
			minimumFeeDiscount.L5_Units = 100;

			ClientLicenceBillingDiscount volumeDiscount1 = Collection.AddNew();
			volumeDiscount1.L5_Type = BillingConstants.DiscountType.Volume;
			volumeDiscount1.L5_BreakAmount = 50m;
			volumeDiscount1.L5_Discount = 10m;

			ClientLicenceBillingDiscount volumeDiscount2 = Collection.AddNew();
			volumeDiscount2.L5_Type = BillingConstants.DiscountType.Volume;
			volumeDiscount2.L5_BreakAmount = 100m;
			volumeDiscount2.L5_Discount = 20m;

			ClientLicenceBillingDiscount specialDiscount = Collection.AddNew();
			specialDiscount.L5_Type = BillingConstants.DiscountType.Special;
			specialDiscount.L5_Discount = 5m;

			ClientLicenceBillingDiscount nonTransactionlDiscount1 = Collection.AddNew();
			nonTransactionlDiscount1.L5_Type = BillingConstants.DiscountType.Capped;
			nonTransactionlDiscount1.L5_Discount = 20m;

			ClientLicenceBillingDiscount nonTransactionlDiscount2 = Collection.AddNew();
			nonTransactionlDiscount2.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			nonTransactionlDiscount2.L5_Discount = 20m;

			Discountable discountable = CreateDiscountable(100m);

			DiscountCalculation calculation = Collection.CalculateTransactionalDiscount(discountable, 120, 2m, ZDateTime.Today, ZDateTime.Today.AddMonths(1));
			AssertEquals("Discount applied: flat and volume2", true, calculation.DiscountAmount > 0);
			AssertEquals(3, calculation.DiscountDescriptions.Count());
			AssertEquals("Amount was raised by flat discount", 150m + 20 * 2m, calculation.Amount);
			AssertEquals("Volume2 and special discounts applied", calculation.Amount * 0.2m + calculation.Amount * 0.05m, calculation.DiscountAmount);
			AssertEquals(120, calculation.UnitCount);

			ZString[] discountDescriptions = calculation.DiscountDescriptions.ToArray();
			AssertEquals(true, discountDescriptions[0].Contains("Minimum Fee"));
			AssertEquals(true, discountDescriptions[1].Contains("Volume"));
			AssertEquals(true, discountDescriptions[2].Contains("Special"));
			ZString[] invoiceDescriptions = calculation.InvoiceDescriptions.ToArray();
			AssertEquals("Special Discount of 5%", invoiceDescriptions[0]);
			AssertEquals("Volume Discount of 20%", invoiceDescriptions[1]);
		}

		public void TestCalculateTransactional_DiscountSubCode()
		{
			ClientLicenceBillingDiscount minimumFeeDiscount = Collection.AddNew();
			minimumFeeDiscount.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			minimumFeeDiscount.L5_SubCode = "AD1";
			minimumFeeDiscount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			minimumFeeDiscount.L5_BreakAmount = 150m;
			minimumFeeDiscount.L5_Units = 100;

			ClientLicenceBillingDiscount volumeDiscountA1 = Collection.AddNew();
			volumeDiscountA1.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			volumeDiscountA1.L5_SubCode = "AD1";
			volumeDiscountA1.L5_Type = BillingConstants.DiscountType.Volume;
			volumeDiscountA1.L5_BreakAmount = 50m;
			volumeDiscountA1.L5_Discount = 10m;
			ClientLicenceBillingDiscount volumeDiscountA2 = Collection.AddNew();
			volumeDiscountA2.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			volumeDiscountA2.L5_SubCode = "AD1";
			volumeDiscountA2.L5_Type = BillingConstants.DiscountType.Volume;
			volumeDiscountA2.L5_BreakAmount = 100m;
			volumeDiscountA2.L5_Discount = 20m;

			ClientLicenceBillingDiscount volumeDiscountB1 = Collection.AddNew();
			volumeDiscountB1.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			volumeDiscountB1.L5_SubCode = "AD2";
			volumeDiscountB1.L5_Type = BillingConstants.DiscountType.Volume;
			volumeDiscountB1.L5_BreakAmount = 10m;
			volumeDiscountB1.L5_Discount = 5m;
			ClientLicenceBillingDiscount volumeDiscountB2 = Collection.AddNew();
			volumeDiscountB2.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			volumeDiscountB2.L5_SubCode = "AD2";
			volumeDiscountB2.L5_Type = BillingConstants.DiscountType.Volume;
			volumeDiscountB2.L5_BreakAmount = 20m;
			volumeDiscountB2.L5_Discount = 10m;

			ClientLicenceBillingDiscount specialDiscount = Collection.AddNew();
			specialDiscount.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			specialDiscount.L5_SubCode = "AD2";
			specialDiscount.L5_Type = BillingConstants.DiscountType.Special;
			specialDiscount.L5_Discount = 5m;

			Discountable discountable = new Discountable(BillingConstants.BillingSystem.AirlineMessaging, 100m, "", 0m);

			DiscountCalculation calculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalDiscount(Collection.ToList(), discountable, 120, 2m, ZDateTime.Today, ZDateTime.Today.AddMonths(1), "AD1");
			AssertEquals("Discount applied: flat and volumeA2", true, calculation.DiscountAmount > 0);
			AssertEquals(2, calculation.DiscountDescriptions.Count());
			AssertEquals("Amount was raised by flat discount", 150m + 20 * 2m, calculation.Amount);
			AssertEquals("VolumeA2 discount applied", calculation.Amount * 0.2m, calculation.DiscountAmount);

			ZString[] discountDescriptions = calculation.DiscountDescriptions.ToArray();
			AssertEquals(true, discountDescriptions[0].Contains("Minimum Fee"));
			AssertEquals(true, discountDescriptions[1].Contains("Volume"));
			ZString[] invoiceDescriptions = calculation.InvoiceDescriptions.ToArray();
			AssertEquals("Volume Discount of 20%", invoiceDescriptions[0]);
		}

		public void TestCalculateTransactional_IncrementalVolume()
		{
			ZString type = BillingConstants.DiscountType.IncrementalVolume;
			ClientLicenceBillingDiscount volumeDiscount1 = AddDiscount(type, 100, 10m);
			ClientLicenceBillingDiscount volumeDiscount2 = AddDiscount(type, 500, 40m);
			ClientLicenceBillingDiscount volumeDiscount3 = AddDiscount(type, 10000, 60m);

			Discountable discountable = CreateDiscountable(11000m * 2m);
			DiscountCalculation calculation = Collection.CalculateTransactionalDiscount(discountable, 11000, 2m, new ZDateTime(2011, 8, 1), new ZDateTime(2011, 8, 31));
			AssertEquals("Discount applied", true, calculation.DiscountAmount > 0);
			AssertEquals(3, calculation.DiscountDescriptions.Count());
			AssertEquals("Volume discount applied", (1000 * 0.6m + 9500 * 0.4m + 400 * 0.1m) * 2m, calculation.DiscountAmount);
			ZString[] discountDescriptions = calculation.DiscountDescriptions.ToArray();
			AssertEquals(true, discountDescriptions[0].Contains("Incremental Volume"));

			discountable = CreateDiscountable(10000m * 2m);
			calculation = Collection.CalculateTransactionalDiscount(discountable, 10000, 2m, new ZDateTime(2011, 8, 1), new ZDateTime(2011, 8, 31));
			AssertEquals("Discount applied", true, calculation.DiscountAmount > 0);
			AssertEquals(2, calculation.DiscountDescriptions.Count());
			AssertEquals("Volume discount applied", (9500 * 0.4m + 400 * 0.1m) * 2m, calculation.DiscountAmount);
			discountDescriptions = calculation.DiscountDescriptions.ToArray();
			AssertEquals(true, discountDescriptions[0].Contains("Incremental Volume"));

			discountable = CreateDiscountable(500m * 2m);
			calculation = Collection.CalculateTransactionalDiscount(discountable, 500, 2m, new ZDateTime(2011, 8, 1), new ZDateTime(2011, 8, 31));
			AssertEquals("Discount applied", true, calculation.DiscountAmount > 0);
			AssertEquals(1, calculation.DiscountDescriptions.Count());
			AssertEquals("Volume discount applied", (400 * 0.1m) * 2m, calculation.DiscountAmount);
			discountDescriptions = calculation.DiscountDescriptions.ToArray();
			AssertEquals(true, discountDescriptions[0].Contains("Incremental Volume"));

			discountable = CreateDiscountable(100 * 2m);
			calculation = Collection.CalculateTransactionalDiscount(discountable, 100, 2m, new ZDateTime(2011, 8, 1), new ZDateTime(2011, 8, 31));
			AssertEquals("Discount not applied", 0m, calculation.DiscountAmount);

			ClientLicenceBillingDiscount commitmentDiscount = Collection.AddNew();
			commitmentDiscount.L5_Type = BillingConstants.DiscountType.Commitment;
			commitmentDiscount.L5_Discount = 5;
			commitmentDiscount.L5_Units = 20000;

			discountable = CreateDiscountable(100 * 2m);
			calculation = Collection.CalculateTransactionalDiscount(discountable, 100, 2m, new ZDateTime(2011, 8, 1), new ZDateTime(2011, 8, 31));
			AssertEquals("Discount applied", true, calculation.DiscountAmount > 0);
			AssertEquals(4, calculation.DiscountDescriptions.Count());
			ZDecimal volumeDiscountAmount = (10000 * 0.6m + 9500 * 0.4m + 400 * 0.1m) * 2m;
			ZDecimal commitmentDiscountAmount = ((20000 * 2m) - volumeDiscountAmount) * 0.05m;
			AssertEquals("discount amount", volumeDiscountAmount + commitmentDiscountAmount, calculation.DiscountAmount);
			discountDescriptions = calculation.DiscountDescriptions.ToArray();
			AssertEquals(true, discountDescriptions[0].Contains("Incremental Volume Discount: 12,000.00"));
			AssertEquals(true, discountDescriptions[1].Contains("Incremental Volume Discount: 7,600.00"));
			AssertEquals(true, discountDescriptions[2].Contains("Incremental Volume Discount: 80.00"));
			AssertEquals(true, discountDescriptions[3].Contains("Commitment Discount: "));
			ZString[] invoiceDescriptions = calculation.InvoiceDescriptions.ToArray();
			AssertEquals("Commitment Discount of 5%", invoiceDescriptions[0]);
			AssertEquals("Incremental Volume Discount", invoiceDescriptions[1]);
		}

		public void TestCalculateTransactionalSurcharge()
		{
			ClientLicenceBillingDiscount minimumFeeDiscount = Collection.AddNew();
			minimumFeeDiscount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			minimumFeeDiscount.L5_BreakAmount = 150m;
			minimumFeeDiscount.L5_Units = 100;

			ClientLicenceBillingDiscount surcharge = Collection.AddNew();
			surcharge.L5_Type = BillingConstants.DiscountType.Surcharge;
			surcharge.L5_Discount = -10m;
			surcharge.L5_Description = "Testing";

			Discountable discountable = CreateDiscountable(100m);

			DiscountCalculation discountCalculation = Collection.CalculateTransactionalDiscount(discountable, 60, 2m, ZDateTime.Today, ZDateTime.Today.AddMonths(1));
			AssertEquals("Amount", 150m, discountCalculation.Amount);

			DiscountCalculation surchargeCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalSurcharge(Collection.ToList(), discountable, discountCalculation.Amount, ZDateTime.Today, ZDateTime.Today.AddMonths(1));

			ZString[] surchargeDescriptions = surchargeCalculation.DiscountDescriptions.ToArray();
			AssertEquals("Testing Surcharge: 15.00 (10% * 150.00)", surchargeDescriptions[0]);
			ZString[] invoiceDescriptions = surchargeCalculation.InvoiceDescriptions.ToArray();
			AssertEquals("Testing Surcharge of 10%", invoiceDescriptions[0]);
		}

		ClientLicenceBillingDiscount AddDiscount(ZString discountType, int units, ZDecimal discount)
		{
			ClientLicenceBillingDiscount result = Collection.AddNew();
			result.L5_Type = discountType;
			result.L5_Units = units;
			result.L5_Discount = discount;
			return result;
		}

		#endregion

		public void TestUpdateCappedDiscount()
		{
			ClientLicenceBillingDiscount cappedDiscount = Collection.AddNew();
			cappedDiscount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			cappedDiscount.L5_Type = BillingConstants.DiscountType.Capped;
			cappedDiscount.L5_BreakAmount = 1000m;
			cappedDiscount.L5_Discount = 20m;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			Collection.UpdateCappedDiscount(Factory, invoice.PK);
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			ClientLicenceBillingDiscount discountReloaded = anotherFactory.Load<ClientLicenceBillingDiscount>(cappedDiscount.PK);
			AssertEquals("Break amount not updated", 1000m, discountReloaded.L5_BreakAmount);

			ZQuery cappedDisountLogQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Capped Discount Used");
			StmALog[] cappedDisountLogs = Organisation.Logs.Find(cappedDisountLogQuery);
			AssertEquals("No capped discount usage logs created", 0, cappedDisountLogs.Length);

			cappedDiscount.Calculate(1000m);
			Collection.UpdateCappedDiscount(Factory, invoice.PK);
			Factory.Save();

			discountReloaded = anotherFactory.Load<ClientLicenceBillingDiscount>(cappedDiscount.PK);
			AssertNotEquals("Break amount re-calculated", 1000m, cappedDiscount.L5_BreakAmount);

			cappedDisountLogs = Organisation.Logs.Find(cappedDisountLogQuery);
			AssertEquals("Capped discount usage log created", 1, cappedDisountLogs.Length);
		}

		public void TestInitDateRange()
		{
			ClientLicenceBillingDiscount discount1 = Collection.AddNew();
			discount1.L5_SystemCode = "AAA";
			discount1.L5_Duration = 12;

			ClientLicenceBillingDiscount discount2 = Collection.AddNew();
			discount2.L5_SystemCode = "AAA";
			discount2.L5_Duration = 36;

			ClientLicenceBillingDiscount discount3 = Collection.AddNew();
			discount3.L5_SystemCode = "AAA";

			ClientLicenceBillingDiscount discount4 = Collection.AddNew();
			discount4.L5_SystemCode = "AAA";
			discount4.L5_StartDate = ZDateTime.Today;

			ClientLicenceBillingDiscount discount5 = Collection.AddNew();
			discount5.L5_SystemCode = "AAA";
			discount5.L5_EndDate = ZDateTime.Today;

			ClientLicenceBillingDiscount discount6 = Collection.AddNew();
			discount6.L5_SystemCode = "BBB";
			discount6.L5_Duration = 12;

			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			Collection.InitDateRange("AAA", factory);
			factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			ClientLicenceBillingDiscount discountReloaded1 = anotherFactory.Load<ClientLicenceBillingDiscount>(discount1.PK);
			AssertNotEquals("Date initialised", ZDateTime.Empty, discountReloaded1.L5_StartDate);
			AssertNotEquals("Date initialised", ZDateTime.Empty, discountReloaded1.L5_EndDate);

			ClientLicenceBillingDiscount discountReloaded2 = anotherFactory.Load<ClientLicenceBillingDiscount>(discount2.PK);
			AssertNotEquals("Date initialised", ZDateTime.Empty, discountReloaded2.L5_StartDate);
			AssertNotEquals("Date initialised", ZDateTime.Empty, discountReloaded2.L5_EndDate);

			ClientLicenceBillingDiscount discountReloaded3 = anotherFactory.Load<ClientLicenceBillingDiscount>(discount3.PK);
			AssertEquals("Date not initialised - no duration", ZDateTime.Empty, discountReloaded3.L5_StartDate);
			AssertEquals("Date not initialised - no duration", ZDateTime.Empty, discountReloaded3.L5_EndDate);

			ClientLicenceBillingDiscount discountReloaded4 = anotherFactory.Load<ClientLicenceBillingDiscount>(discount4.PK);
			AssertEquals("Date not initialised - no duration", ZDateTime.Today, discountReloaded4.L5_StartDate);
			AssertEquals("Date not initialised - no duration", ZDateTime.Empty, discountReloaded4.L5_EndDate);

			ClientLicenceBillingDiscount discountReloaded5 = anotherFactory.Load<ClientLicenceBillingDiscount>(discount5.PK);
			AssertEquals("Date not initialised - no duration", ZDateTime.Empty, discountReloaded5.L5_StartDate);
			AssertEquals("Date not initialised - no duration", ZDateTime.Today, discountReloaded5.L5_EndDate);

			ClientLicenceBillingDiscount discountReloaded6 = anotherFactory.Load<ClientLicenceBillingDiscount>(discount6.PK);
			AssertEquals("Date not initialised - different system code", ZDateTime.Empty, discountReloaded6.L5_StartDate);
			AssertEquals("Date not initialised - different system code", ZDateTime.Empty, discountReloaded6.L5_EndDate);
		}

		public void TestRelationshipDefaultsForNewElement()
		{
			ClientLicenceBillingDiscount billingDiscount = Collection.AddNew();
			AssertEquals("Master", LicenceBilling.PK, billingDiscount.L5_L4);
		}

		public void TestAllowNew()
		{
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			AssertEquals(true, ((IBindingList)Collection).AllowNew);

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			AssertEquals(false, ((IBindingList)Collection).AllowNew);
		}

		#region Implementation

		Discountable CreateDiscountable(ZDecimal rawAmount, decimal quantityForVolumeDiscount = 0m)
		{
			return new Discountable("", rawAmount, "", quantityForVolumeDiscount);
		}

		class Discountable : IDiscountable
		{
			public Discountable(ZString systemCode, ZDecimal amount, string moduleCode, decimal quantityForVolumeDiscount = 0m)
			{
				SystemCode = systemCode;
				AmountToDiscount = amount;
				ModuleCode = moduleCode;
				UnitCount = 0;
				LicenceUnitsToDiscount = quantityForVolumeDiscount;
			}

			public ZString SystemCode { get; set; }

			public ZDecimal AmountToDiscount { get; set; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
			public ZDecimal AmountToDiscountForModule(string moduleCode, out string moduleName)
			{
				moduleName = "";
				if (moduleCode == ModuleCode)
				{
					return AmountToDiscount;
				}

				return 0m;
			}

			public int UnitCount { get; set; }
			public string ModuleCode { get; set; }
			public ZDecimal LicenceUnitsToDiscount { get; set; }
		}

		EDIOrgHeader Organisation;
		ClientLicenceBilling LicenceBilling;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ClientLicenceBillingDiscountCollection);
		}

		protected override ClientLicenceBillingDiscountCollection GetCollectionToTest()
		{
			Organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			LicenceBilling = Factory.New<ClientLicenceBilling>();
			LicenceBilling.L4_LC = Organisation.LicCompany.PK;
			return new ClientLicenceBillingDiscountCollection(LicenceBilling);
		}

		#endregion
	}
}
