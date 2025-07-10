using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiBilledDiscount))]
	public class EdiBilledDiscountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateBilledDiscounts_ODPL()
		{
			var d10 = new DiscountDetailedInfo("D10 : 10%", "D10", 10, 10);
			var d15 = new DiscountDetailedInfo("D15 : 15%", "D15", 15, 15);
			var d30 = new DiscountDetailedInfo("D30 : 30%", "D30", 30, 30);

			var d15_2 = new DiscountDetailedInfo("D15 : 15%", "D15", 15, 15);

			var billedUsage = Factory.NewWithValidTestData<EdiBilledUsage>();
			billedUsage.BU9_TransactionAmountPreDiscount = 2000;
			billedUsage.BU9_TransactionAmountPostDiscount = 600; // =2000 - (2000*0.1) - (2000 * 0.15 * 2) - (2000 * 0.3 )

			EdiBilledDiscount.CreateBilledDiscounts(billedUsage, new[] { d10, d15, d30, d15_2 }, null);

			var billedDiscounts = Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, billedUsage.PK))
				.OrderBy(x => x.BD9_Type).ToArray();

			AssertEquals(3, billedDiscounts.Length);

			AssertEquals("D10", billedDiscounts[0].BD9_Type);
			AssertEquals(ZGuid.Empty, billedDiscounts[0].BD9_PHD_Discount);
			AssertEquals(2000m * 0.10m, billedDiscounts[0].BD9_TransactionAmount);
			AssertEquals(Utilities.Round(2000m * 0.10m * 100m / (2000m), 2), billedDiscounts[0].BD9_Percent);

			AssertEquals("D15", billedDiscounts[1].BD9_Type);
			AssertEquals(ZGuid.Empty, billedDiscounts[1].BD9_PHD_Discount);
			AssertEquals(2000m * 0.15m * 2m, billedDiscounts[1].BD9_TransactionAmount);
			AssertEquals(15m, billedDiscounts[1].BD9_Percent);

			AssertEquals("D30", billedDiscounts[2].BD9_Type);
			AssertEquals(ZGuid.Empty, billedDiscounts[2].BD9_PHD_Discount);
			AssertEquals(2000m * 0.30m, billedDiscounts[2].BD9_TransactionAmount);
			AssertEquals(Utilities.Round(2000m * 0.30m * 100m / (2000m), 2), billedDiscounts[2].BD9_Percent);
		}

		public void TestCreateBilledDiscounts_STL()
		{
			var d10 = CreateIStlDiscount("D10", 10);
			var d15 = CreateIStlDiscount("D15", 15);
			var d30 = CreateIStlDiscount("D30", 30);

			var billedUsage = Factory.NewWithValidTestData<EdiBilledUsage>();
			billedUsage.BU9_TransactionAmountPreDiscount = 2000;
			billedUsage.BU9_TransactionAmountPostDiscount = 900; // 2000 - (2000*0.10) - (2000 * 0.15) - (2000 * 0.30);

			EdiBilledDiscount.CreateBilledDiscounts(billedUsage, new[] { d10, d15, d30 });

			var billedDiscounts = Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, billedUsage.PK))
				.OrderByDescending(x => x.BD9_TransactionAmount).ToArray();

			AssertEquals(3, billedDiscounts.Length);

			AssertEquals("", billedDiscounts[0].BD9_Type);
			AssertEquals(d30.HeaderDiscount.PK, billedDiscounts[0].BD9_PHD_Discount);
			AssertEquals(2000m * 0.30m, billedDiscounts[0].BD9_TransactionAmount);
			AssertEquals(30m, billedDiscounts[0].BD9_Percent);

			AssertEquals("", billedDiscounts[1].BD9_Type);
			AssertEquals(d15.HeaderDiscount.PK, billedDiscounts[1].BD9_PHD_Discount);
			AssertEquals(2000m * 0.15m, billedDiscounts[1].BD9_TransactionAmount);
			AssertEquals(15m, billedDiscounts[1].BD9_Percent);

			AssertEquals("", billedDiscounts[2].BD9_Type);
			AssertEquals(d10.HeaderDiscount.PK, billedDiscounts[2].BD9_PHD_Discount);
			AssertEquals(2000m * 0.10m, billedDiscounts[2].BD9_TransactionAmount);
			AssertEquals(10m, billedDiscounts[2].BD9_Percent);
		}

		public void TestCreateBilledDiscounts_STL_100Percent()
		{
			var d10 = CreateIStlDiscount("D10", 10);
			var d15 = CreateIStlDiscount("D15", 15);
			var d100 = CreateIStlDiscount("D100", 100);
			var d110 = CreateIStlDiscount("D110", 110);

			var billedUsage1 = Factory.NewWithValidTestData<EdiBilledUsage>();
			billedUsage1.BU9_TransactionAmountPreDiscount = 2000;
			billedUsage1.BU9_TransactionAmountPostDiscount = 0;

			var billedUsage2 = Factory.NewWithValidTestData<EdiBilledUsage>();
			billedUsage2.BU9_TransactionAmountPreDiscount = 2000;
			billedUsage2.BU9_TransactionAmountPostDiscount = 0;

			EdiBilledDiscount.CreateBilledDiscounts(billedUsage1, new[] { d10, d15, d100 });
			EdiBilledDiscount.CreateBilledDiscounts(billedUsage2, new[] { d10, d15, d100, d110 });
			{
				var billedDiscounts = Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, billedUsage1.PK))
					.OrderBy(x => x.BD9_TransactionAmount).ToArray();

				AssertEquals(3, billedDiscounts.Length);

				AssertEquals("", billedDiscounts[0].BD9_Type);
				AssertEquals(d10.HeaderDiscount.PK, billedDiscounts[0].BD9_PHD_Discount);
				AssertEquals(10m / (10m + 15m) * (1 - 0.9m * 0.85m) * 2000m, billedDiscounts[0].BD9_TransactionAmount);
				AssertEquals(10m, billedDiscounts[0].BD9_Percent);

				AssertEquals("", billedDiscounts[1].BD9_Type);
				AssertEquals(d15.HeaderDiscount.PK, billedDiscounts[1].BD9_PHD_Discount);
				AssertEquals(15m / (10m + 15m) * (1 - 0.9m * 0.85m) * 2000m, billedDiscounts[1].BD9_TransactionAmount);
				AssertEquals(15m, billedDiscounts[1].BD9_Percent);

				AssertEquals("", billedDiscounts[2].BD9_Type);
				AssertEquals(d100.HeaderDiscount.PK, billedDiscounts[2].BD9_PHD_Discount);
				AssertEquals(2000m - billedDiscounts[0].BD9_TransactionAmount - billedDiscounts[1].BD9_TransactionAmount, billedDiscounts[2].BD9_TransactionAmount);
				AssertEquals(100m, billedDiscounts[2].BD9_Percent);
			}
			{
				var billedDiscounts = Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, billedUsage2.PK))
					.OrderBy(x => x.BD9_Percent).ToArray();
				AssertEquals(4, billedDiscounts.Length);

				AssertEquals("", billedDiscounts[0].BD9_Type);
				AssertEquals(d10.HeaderDiscount.PK, billedDiscounts[0].BD9_PHD_Discount);
				AssertEquals(10m / (10m + 15m) * (1 - 0.9m * 0.85m) * 2000m, billedDiscounts[0].BD9_TransactionAmount);
				AssertEquals(10m, billedDiscounts[0].BD9_Percent);

				AssertEquals("", billedDiscounts[1].BD9_Type);
				AssertEquals(d15.HeaderDiscount.PK, billedDiscounts[1].BD9_PHD_Discount);
				AssertEquals(15m / (10m + 15m) * (1 - 0.9m * 0.85m) * 2000m, billedDiscounts[1].BD9_TransactionAmount);
				AssertEquals(15m, billedDiscounts[1].BD9_Percent);

				AssertEquals("", billedDiscounts[2].BD9_Type);
				AssertEquals(d100.HeaderDiscount.PK, billedDiscounts[2].BD9_PHD_Discount);
				var fullDiscountAmount = 2000m - billedDiscounts[0].BD9_TransactionAmount - billedDiscounts[1].BD9_TransactionAmount;
				AssertEquals(0.5m * fullDiscountAmount, billedDiscounts[2].BD9_TransactionAmount);
				AssertEquals(100m, billedDiscounts[2].BD9_Percent);

				AssertEquals("", billedDiscounts[3].BD9_Type);
				AssertEquals(d110.HeaderDiscount.PK, billedDiscounts[3].BD9_PHD_Discount);
				AssertEquals(0.5m * fullDiscountAmount, billedDiscounts[3].BD9_TransactionAmount);
				AssertEquals(110m, billedDiscounts[3].BD9_Percent);
			}
		}

		public void TestCreateBilledDiscounts_4DecimalPlaces()
		{
			//ODPL
			var d1099 = new DiscountDetailedInfo("D99 : 10.99%", "D99", 10.99m, 10.99m);
			var billedUsage = Factory.NewWithValidTestData<EdiBilledUsage>();
			billedUsage.BU9_TransactionAmountPreDiscount = 22;
			billedUsage.BU9_TransactionAmountPostDiscount = 19.5822;

			EdiBilledDiscount.CreateBilledDiscounts(billedUsage, new[] { d1099 }, null);
			var billedDiscount = Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, billedUsage.PK))
				.OrderBy(x => x.BD9_Type).Single();

			AssertEquals("D99", billedDiscount.BD9_Type);
			AssertEquals(ZGuid.Empty, billedDiscount.BD9_PHD_Discount);
			AssertEquals(2.4178m, billedDiscount.BD9_TransactionAmount);
			AssertEquals(10.99m, billedDiscount.BD9_Percent);

			//STL
			var stl1099 = CreateIStlDiscount("S99", 10.99m);
			billedUsage = Factory.NewWithValidTestData<EdiBilledUsage>();
			billedUsage.BU9_TransactionAmountPreDiscount = 22;
			billedUsage.BU9_TransactionAmountPostDiscount = 19.5822;

			EdiBilledDiscount.CreateBilledDiscounts(billedUsage, new[] { stl1099 });
			billedDiscount = Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, billedUsage.PK))
				.OrderByDescending(x => x.BD9_TransactionAmount).Single();

			AssertEquals("", billedDiscount.BD9_Type);
			AssertEquals(stl1099.HeaderDiscount.PK, billedDiscount.BD9_PHD_Discount);
			AssertEquals(2.4178m, billedDiscount.BD9_TransactionAmount);
			AssertEquals(10.99m, billedDiscount.BD9_Percent);
		}

		IStlDiscount CreateIStlDiscount(string discountName, decimal discountPercent)
		{
			var discount = Factory.New<EdiPriceHeaderDiscount>();
			discount.PHD_Name = discountName;
			discount.PHD_Version = "STL1";
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;

			var setting = Factory.New<DiscountLicenceSetting>();
			setting.LS9_Percent = discountPercent;
			var discountInfo = new DiscountInfo();
			discountInfo.Init(discount, setting, null, null);
			var stlDiscount = new PercentageStlDiscount(discountInfo);

			return stlDiscount;
		}
	}
}
