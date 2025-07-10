using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Types.Testing
{
	sealed class ZAmountTest : TestCase
	{
		public void TestEquals()
		{
			var a = new ZAmount(5m, ZUnit.Weight.KG, "test");

			AssertEquals(5m, a);
			AssertEquals(a, 5m);

			AssertEquals("5 KG", a);
			AssertEquals(a, "5 KG");

			Assert(5m.Equals(a));
			Assert(a.Equals(5m));

			Assert("5 KG".Equals(a));
			Assert(a.Equals("5 KG"));

			Assert(5m == a);
			Assert(a == 5m);

			Assert("5 KG" == a);
			Assert(a == "5 KG");
		}

		public void TestEmpty()
		{
			var amount = new ZAmount(2204, "USD", "test");

			AssertEquals("2204 USD", (ZAmount.Empty + amount).ToString());
			AssertEquals("2204 USD", (amount + ZAmount.Empty).ToString());
			AssertEquals("2204 USD", (amount - ZAmount.Empty).ToString());

			var a4 = amount * ZAmount.Empty;

			Assert(!a4.IsEmpty);
			AssertEquals("0 USD", a4);

			AssertExceptionThrown<InvalidOperationException>("Should not subtract from Empty amount", () =>
			{
				var a = ZAmount.Empty - amount;
			});

			AssertExceptionThrown<DivideByZeroException>(() =>
			{
				var a = amount / ZAmount.Empty;
			});

			AssertEquals(amount, ZAmount.Min(amount, ZAmount.Empty));
			AssertEquals(amount, ZAmount.Max(amount, ZAmount.Empty));
		}

		public void TestComparisionOperators()
		{
			var baseAmount = new ZAmount(30, "KG", "S1");
			var baseAmount2 = new ZAmount(30.00, "KG", "S2");
			var biggerAmount = new ZAmount(50, "KG", "S3");
			var smallerAmount = new ZAmount(1, "KG", "S4");

			AssertEquals(true, baseAmount == baseAmount2);
			AssertEquals(true, baseAmount >= baseAmount2);
			AssertEquals(true, baseAmount <= baseAmount2);

			AssertEquals(false, baseAmount > biggerAmount);
			AssertEquals(false, baseAmount >= biggerAmount);

			AssertEquals(true, baseAmount < biggerAmount);
			AssertEquals(true, baseAmount <= biggerAmount);

			AssertEquals(true, baseAmount > smallerAmount);
			AssertEquals(true, baseAmount >= smallerAmount);

			AssertEquals(false, baseAmount < smallerAmount);
			AssertEquals(false, baseAmount <= smallerAmount);

			var tonne = new ZAmount(1, "T", "S1");
			var lessThanTonne = new ZAmount(2204, "LB", "S1");
			var moreThanTonne = new ZAmount(2205, "LB", "S1");

			var dollars = new ZAmount(55, "USD", "S1");

			Assert(tonne > lessThanTonne);
			Assert(tonne < moreThanTonne);

			AssertExceptionThrown<InvalidOperationException>(() => { var x = tonne > dollars; });
		}

		public void TestPublicPropertiesDontThowExceptionsIfConstructorDidntRun()
		{
			var publicProperties = typeof(ZAmount).GetProperties(BindingFlags.Public | BindingFlags.Instance);

			Assert("Pre-condition", publicProperties.Any());

			foreach (var property in publicProperties)
			{
				var propertyName = property.Name;
				AssertNoExceptionThrown(propertyName, () =>
				{
					object target = default(ZAmount);
					var value = GetPropertyInfo(ref target, propertyName).GetValue(target, null);
					AssertNotNull(value);
				});
			}
		}

		static PropertyInfo GetPropertyInfo(ref object target, string propertyName)
		{
			var propertyPath = propertyName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			var property = target.GetType().GetProperty(propertyPath[0]);

			if (propertyPath.Length == 1)
			{
				return property;
			}

			var subPropertyPath = string.Join(".", propertyPath.Skip(1));
			target = property.GetValue(target, null);

			return GetPropertyInfo(ref target, subPropertyPath);
		}

		public void TestPlusMinus()
		{
			var weight1 = new ZAmount(150, "KG", "S1");
			var weight2 = new ZAmount(50, "KG", "S2");
			var combinedWeight = weight1 + weight2;

			AssertAmount(combinedWeight, "200 KG", "150 KG + 50 KG", "S1\r\nS2");
			AssertAmount(combinedWeight - weight2, "150 KG", "(150 KG + 50 KG) - 50 KG", "S1\r\nS2");
		}

		public void TestMultiply()
		{
			var weight = new ZAmount(50, "KG", "S1");
			var frtRate = new ZAmount(2, "USD/KG", "R1");

			var charge1 = weight * frtRate;
			var charge2 = charge1 + new ZAmount(30, "USD", "R2");

			AssertAmount(charge1 + charge2, "230 USD", "50 KG x 2 USD/KG + (50 KG x 2 USD/KG + 30 USD)", "R1\r\nR2\r\nS1");

			var storageRate = new ZAmount(.37, "AUD/(M3*HR)", "tariff");
			var storageUsed = new ZAmount(1500, "M3*HR", "warehouse meter");

			var bill = storageRate * storageUsed;
			AssertAmount(bill, "555 AUD", "0.37 AUD/(HR*M3) x 1500 (HR*M3)", "tariff\r\nwarehouse meter");

			var speed = new ZAmount(100, "KM/HR", "speedometer");
			var consumption = new ZAmount(.1, "L/KM", "spec");

			var immediateConsumption = speed * consumption;

			AssertAmount(immediateConsumption, "10 L/HR", "100 KM/HR x 0.1 L/KM", "spec\r\nspeedometer");
		}

		public void TestDivision()
		{
			var speed = new ZAmount(100, "KM/HR", "speedometer");
			var consumption = new ZAmount(10, "KM/L", "spec");

			var immediateConsumption = speed / consumption;

			AssertAmount(immediateConsumption, "10 L/HR", "100 KM/HR / 10 KM/L", "spec\r\nspeedometer");
		}

		public void TestDivisionHidesOne()
		{
			var distance = new ZAmount(5, ZUnit.Length.KM, "t1");
			var time = new ZAmount(1, ZUnit.Time.Hour, "t2");

			var speed = distance / time;

			AssertAmount(speed, "5 KM/HR", "5 KM / 1 HR", "t1\r\nt2");
		}

		public void TestMinMax()
		{
			var weight1 = new ZAmount(150, "KG", "S1");
			var weight2 = new ZAmount(50, "KG", "S2");
			var weight3 = new ZAmount(250, "KG", "S3");
			var weight4 = new ZAmount(350, "KG", "S4");

			var min = ZAmount.Min(new[] { weight1, weight2, weight3, weight4 });
			var max = ZAmount.Max(new[] { weight1, weight2, weight3, weight4 });

			AssertAmount(min, "50 KG", "MIN(150 KG, 50 KG, 250 KG, 350 KG)", "S1\r\nS2\r\nS3\r\nS4");
			AssertAmount(max, "350 KG", "MAX(150 KG, 50 KG, 250 KG, 350 KG)", "S1\r\nS2\r\nS3\r\nS4");
		}

		public void TestPercentage()
		{
			var weight = new ZAmount(50, "KG", "S1");
			var frtRate = new ZAmount(2, "USD/KG", "R1");
			var charge = (weight * frtRate).Percentage(75m, "R2").Percentage(188m, "R3");

			AssertAmount(charge, "141 USD", "188% of(75% of(50 KG x 2 USD/KG))", "R1\r\nR2\r\nR3\r\nS1");
		}

		public void TestRound()
		{
			var measure = new ZAmount(34.7854283742, "M3", "S1");
			var rounded = measure.Round(2, "D1");

			AssertAmount(rounded, "34.79 M3", "Round(34.7854 M3, 2)", "D1\r\nS1");
		}

		public void TestSameSystemConvert()
		{
			var weight1 = new ZAmount(50, ZUnit.Weight.KG, "S1");
			var weight2 = new ZAmount(2, ZUnit.Weight.T, "S2");

			var total = weight1 + weight2;

			AssertAmount(total, "2050 KG", "50 KG + 2 T x 1000 KG/T", "S1\r\nS2\r\nT to KG conversion factor");

			var weight = new ZAmount(50, "KG", "S1");
			var frtRate = new ZAmount(2000, "USD/T", "R1");

			var charge = weight * frtRate;

			AssertAmount(charge, "100 USD", "50 KG x 2000 USD/T / 1000 KG/T", "R1\r\nS1\r\nT to KG conversion factor");
		}

		public void TestMetricImperialConvert()
		{
			var weight = new ZAmount(50, "KG", "S1");
			var frtRate = new ZAmount(2, "USD/LB", "R1");

			var charge = weight * frtRate;

			AssertAmount(charge, "220.462 USD", "50 KG x 2 USD/LB / 0.453592 KG/LB", "LB to KG conversion factor\r\nR1\r\nS1");

			var volumeMtr = new ZAmount(50, "M3", "S1");
			var volumeImp = new ZAmount(500, "CF", "S2");

			var total = volumeMtr + volumeImp;

			AssertAmount(total, "64.1584 M3", "50 M3 + 500 CF x 0.0283168 M3/CF", "CF to M3 conversion factor\r\nS1\r\nS2");
		}

		public void TestIsAnyUnit()
		{
			var containerCount = new ZAmount(5, ZUnit.RefContainer.Any, "S1");
			Assert(containerCount.IsAnyUnit);
		}

		public void TestExceptions()
		{
			AssertExceptionThrown(typeof(ArgumentException), "Empty unit", () => new ZAmount(1, " ", "bla"));
			AssertExceptionThrown(typeof(ArgumentException), "Source should not be empty", () => new ZAmount(1, "KG", " "));
			AssertExceptionThrown(typeof(ArgumentException), "Bad unit KG//M3", () => new ZAmount(1, "KG//M3", "bla"));
			AssertExceptionThrown(typeof(ArgumentException), "Bad unit KG**M3", () => new ZAmount(1, "KG**M3", "bla"));

			var a = new ZAmount(1, "KG", "bla");
			var b = new ZAmount(1, "M3", "bla");

			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot convert M3 to KG", () => { var c = a + b; });
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot convert KG to M3", () => { var c = b - a; });

			a = new ZAmount(10, "KG", "bla");
			b = new ZAmount(1, "KG", "bla");

			AssertExceptionThrown(typeof(ArgumentException), "Source should not be empty", () => a.Percentage(10, " "));
			AssertExceptionThrown(typeof(ArgumentException), "Source should not be empty", () => a.Round(10, " "));

			a = new ZAmount(1, "KG", "bla");
			b = new ZAmount(1, "M3", "bla");
			var d = new ZAmount(1, "KG", "bla");
			var e = new ZAmount(1, "KG", "bla");

			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot convert KG to M3", () => ZAmount.Max(a, b, d, e));
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot convert KG to M3", () => ZAmount.Min(a, b, d, e));

			var a1 = ZAmount.Empty;
			var a2 = new ZAmount(2204, "USD", "test");
			AssertExceptionThrown(typeof(InvalidOperationException), "Should not subtract from Empty amount", () => { var x = a1 - a2; });
		}

		void AssertAmount(ZAmount amount, string expectedStr, string expectedFormula, string expectedSource)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ToString()", expectedStr, amount.ToString());
				AssertEquals("source", expectedSource, amount.Source);
				AssertEquals("formula", expectedFormula, amount.ToFormula());
			});
		}
	}
}
