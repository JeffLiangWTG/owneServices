using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZVolumeTest : TestCase
	{
		public void TestConstructor()
		{
			ZVolume volume = new ZVolume(1m, "M3");
			AssertEquals(1m, volume.Amount);
			AssertEquals("M3", volume.Unit);
		}

		public void TestConvertInvalidUnit()
		{
			ZVolume volume = new ZVolume(1m, "FOOBLE");
			AssertEquals(0m, volume.InCubicMetres);
		}

		public void TestEmpty()
		{
			ZVolume emptyVolume = ZVolume.Empty;
			AssertEquals(0m, emptyVolume.Amount);
			AssertEquals("M3", emptyVolume.Unit);
			Assert(emptyVolume.IsEmpty);
		}

		public void TestOperatorOverLoadAddSameUnit()
		{
			ZVolume volume1 = new ZVolume(1.1m, "M3");
			ZVolume volume2 = new ZVolume(2.2m, "M3");
			ZVolume volume3 = volume1 + volume2;
			AssertEquals(3.3m, volume3.Amount);
			AssertEquals("M3", volume3.Unit);
		}

		public void TestOperatorOverLoadSubtractSameUnit()
		{
			ZVolume volume1 = new ZVolume(2.2m, "M3");
			ZVolume volume2 = new ZVolume(1.0m, "M3");
			ZVolume volume3 = volume1 - volume2;
			AssertEquals(1.2m, volume3.Amount);
			AssertEquals("M3", volume3.Unit);
		}

		public void TestOperatorOverLoadAddDifferentUnit()
		{
			ZVolume volume1 = new ZVolume(1, "M3");
			ZVolume volume2 = new ZVolume(1, "CF");
			ZVolume volume3 = volume1 + volume2;
			AssertEquals(1.028317m, volume3.Amount);
			AssertEquals("M3", volume3.Unit);
		}

		public void TestOperatorOverLoadSubtractDifferentUnit()
		{
			ZVolume volume1 = new ZVolume(100, "M3");
			ZVolume volume2 = new ZVolume(1, "CF");
			ZVolume volume3 = volume1 - volume2;
			AssertEquals(99.971683m, volume3.Amount);
			AssertEquals("M3", volume3.Unit);
		}

		public void TestOperatorOverLoadMultiplyCubicMetres()
		{
			ZVolume multipliedVolume = new ZVolume(1000, "M3") * 5;
			AssertEquals(5000m, multipliedVolume.Amount);
			AssertEquals("M3", multipliedVolume.Unit);
		}

		public void TestOperatorOverLoadMultiplyCubicFeet()
		{
			ZVolume multipliedVolume = new ZVolume(1000, "CF") * 5;
			AssertEquals(5000m, multipliedVolume.Amount);
			AssertEquals("CF", multipliedVolume.Unit);
		}

		public void TestOperatorOverLoadDivideCubicMetres()
		{
			ZVolume dividedVolume = new ZVolume(1000, "M3") / 5;
			AssertEquals(200m, dividedVolume.Amount);
			AssertEquals("M3", dividedVolume.Unit);
		}

		public void TestOperatorOverLoadDivideCubicFeet()
		{
			ZVolume dividedVolume = new ZVolume(1000, "CF") / 5;
			AssertEquals(200m, dividedVolume.Amount);
			AssertEquals("CF", dividedVolume.Unit);
		}

		public void TestOperatorOverLoadGreaterThanSameUnit()
		{
			Assert(new ZVolume(1.5m, "M3") > new ZVolume(1.4m, "M3"));
			AssertEquals(false, new ZVolume(1.4m, "M3") > new ZVolume(1.5m, "M3"));
		}

		public void TestOperatorOverLoadGreaterThanOrEqualToSameUnit()
		{
			Assert(new ZVolume(1.5m, "M3") >= new ZVolume(1.5m, "M3"));
			Assert(new ZVolume(1.5m, "M3") >= new ZVolume(1.4m, "M3"));
			AssertEquals(false, new ZVolume(1.4m, "M3") >= new ZVolume(1.5m, "M3"));
		}

		public void TestOperatorOverLoadLessThanSameUnit()
		{
			Assert(new ZVolume(1.4m, "M3") < new ZVolume(1.5m, "M3"));
			AssertEquals(false, new ZVolume(1.5m, "M3") < new ZVolume(1.4m, "M3"));
		}

		public void TestOperatorOverLoadLessThanOrEqualToSameUnit()
		{
			Assert(new ZVolume(1.5m, "M3") <= new ZVolume(1.5m, "M3"));
			Assert(new ZVolume(1.4m, "M3") <= new ZVolume(1.5m, "M3"));
			AssertEquals(false, new ZVolume(1.5m, "M3") <= new ZVolume(1.4m, "M3"));
		}

		public void TestOperatorOverLoadEqualToSameUnit()
		{
			Assert(new ZVolume(1.5m, "M3") == new ZVolume(1.5m, "M3"));
			AssertEquals(false, new ZVolume(1.4m, "M3") == new ZVolume(1.5m, "M3"));
		}

		public void TestOperatorOverLoadNotEqualToSameUnit()
		{
			Assert(new ZVolume(1.4m, "M3") != new ZVolume(1.5m, "M3"));
			AssertEquals(false, new ZVolume(1.5m, "M3") != new ZVolume(1.5m, "M3"));
		}

		public void TestOperatorOverLoadGreaterThanDifferentUnit()
		{
			Assert(new ZVolume(1500m, "L") > new ZVolume(1.4m, "M3"));
			AssertEquals(false, new ZVolume(1400m, "L") > new ZVolume(1.5m, "M3"));
		}

		public void TestOperatorOverLoadGreaterThanOrEqualToDifferentUnit()
		{
			Assert(new ZVolume(1500m, "L") >= new ZVolume(1.5m, "M3"));
			Assert(new ZVolume(1500m, "L") >= new ZVolume(1.4m, "M3"));
			AssertEquals(false, new ZVolume(1400m, "L") >= new ZVolume(1.5m, "M3"));
		}

		public void TestOperatorOverLoadLessThanDifferentUnit()
		{
			Assert(new ZVolume(1400m, "L") < new ZVolume(1.5m, "M3"));
			AssertEquals(false, new ZVolume(1500m, "L") < new ZVolume(1.4m, "M3"));
		}

		public void TestOperatorOverLoadLessThanOrEqualToDifferentUnit()
		{
			Assert(new ZVolume(1500m, "L") <= new ZVolume(1.5m, "M3"));
			Assert(new ZVolume(1400m, "L") <= new ZVolume(1.5m, "M3"));
			AssertEquals(false, new ZVolume(1500m, "L") <= new ZVolume(1.4m, "M3"));
		}

		public void TestOperatorOverLoadEqualToDifferentUnit()
		{
			Assert(new ZVolume(1500m, "L") == new ZVolume(1.5m, "M3"));
			AssertEquals(false, new ZVolume(1400m, "L") == new ZVolume(1.5m, "M3"));
		}

		public void TestOperatorOverLoadNotEqualToDifferentUnit()
		{
			Assert(new ZVolume(1400m, "L") != new ZVolume(1.5m, "M3"));
			AssertEquals(false, new ZVolume(1500m, "L") != new ZVolume(1.5m, "M3"));
		}

		public void TestIsValid()
		{
			ZVolume volume = new ZVolume(0m, "");
			AssertEquals(false, volume.IsValid);

			volume = new ZVolume(0m, "XXX");
			AssertEquals(false, volume.IsValid);

			foreach (string code in Constants.Volume.Codes)
			{
				volume = new ZVolume(0m, code);
				AssertEquals(true, volume.IsValid);
			}
		}

		public void TestConvertTo()
		{
			AssertEquals(35.314667m, new ZVolume(1, "M3").ConvertTo("CF"));
		}

		public void TestInCubicMetres()
		{
			AssertEquals(0.028317m, new ZVolume(1, "CF").InCubicMetres);
		}

		public void TestToStringOnEmpty()
		{
			AssertEquals("0 M3", ZVolume.Empty.ToString());
		}

		public void TestToString()
		{
			AssertEquals("6.25 L", new ZVolume(6.25m, "L").ToString());
		}

		public void TestIsEmpty()
		{
			Assert(new ZVolume(0, "M3").IsEmpty);
			Assert(new ZVolume(0, "CF").IsEmpty);
			AssertEquals(false, new ZVolume(1, "M3").IsEmpty);
			AssertEquals(false, new ZVolume(1, "CF").IsEmpty);
			AssertEquals(false, new ZVolume(-1, "M3").IsEmpty);
			AssertEquals(false, new ZVolume(-1, "CF").IsEmpty);
		}

		public void TestInvalidVolume()
		{
			AssertEquals(ZDecimal.Zero, ZVolume.Invalid.Amount);
			AssertEquals(ZString.Empty, ZVolume.Invalid.Unit);
			AssertEquals(false, ZVolume.Invalid.IsValid);
		}

		public void TestInvalidUnit()
		{
			ZVolume volume = new ZVolume(6m, "TWEEBLES");
			AssertEquals(6m, volume.Amount);
			AssertEquals("TWEEBLES", volume.Unit);
			AssertEquals(false, volume.IsValid);

			foreach (string volumeUnit in Constants.Volume.Codes)
			{
				volume = new ZVolume(1m, volumeUnit);
				AssertEquals(true, volume.IsValid);
			}
		}

		public void TestEquals()
		{
			ZVolume volume1M3 = new ZVolume(1, "M3");
			ZVolume volume1000L = new ZVolume(1000, "L");
			ZVolume volume1001L = new ZVolume(1001, "L");

			AssertEquals(true, volume1M3.Equals(volume1000L));
			AssertEquals(true, volume1000L.Equals(volume1M3));
			AssertEquals(false, volume1M3.Equals(volume1001L));
		}

		public void TestGetHashCode()
		{
			ZVolume volume1 = new ZVolume(1000, "L");
			ZVolume volume2 = new ZVolume(1, "M3");
			Assert("PreCondition: Volume1.Equals(Volume2)", volume1.Equals(volume2));
			AssertEquals(volume2.GetHashCode(), volume1.GetHashCode());
		}
	}
}
