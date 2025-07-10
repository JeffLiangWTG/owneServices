using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(TrainingZoneRate))]
	public class TrainingZoneRateTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			TrainingZoneRate result = new TrainingZoneRate();
			result.ZonePK = zoneForTest.PK;
			result.RateAmount = 989;
			result.CurrencyCode = "USD";
			return result;
		}

		protected virtual TrainingZoneRate GetNewTrainingZoneRate()
		{
			TrainingZoneRateCollection collection = new TrainingZoneRateCollection();
			return collection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewTrainingZoneRate();
		}

		protected override void SetUp()
		{
			base.SetUp();

			zoneForTest = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneForTest.FZ_ZoneType = EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training;
			Factory.Save();
			statisticsCollectorSuspender = PerformanceStatisticsCollector.SuspendStatisticsCollector();
		}

		protected override void TearDown()
		{
			base.TearDown();
			statisticsCollectorSuspender?.Dispose();
		}

		IDisposable statisticsCollectorSuspender;
		RefZoneHeader zoneForTest;

		#endregion

		#region Properties

		public void TestZone()
		{
			TrainingZoneRate rate = GetNewTrainingZoneRate();
			rate.ZonePK = zoneForTest.PK;

			AssertEquals(zoneForTest.PK, rate.ZonePK);
			AssertEquals(zoneForTest.PK, rate.Zone.PK);
		}

		public void TestRateAmount()
		{
			TrainingZoneRate rate = GetNewTrainingZoneRate();
			rate.RateAmount = (ZDecimal)56m;
			AssertEquals((ZDecimal)56m, rate.RateAmount);
		}

		public void TestCurrency()
		{
			TrainingZoneRate rate = GetNewTrainingZoneRate();
			rate.CurrencyCode = "GBP";
			AssertEquals("GBP", rate.CurrencyCode);
			AssertEquals("GBP", rate.Currency.RX_Code);
			AssertEquals(RefCurrencySchema.RX_Code.MaxLength, rate.CurrencyCodeInfo.MaxLength);
		}

		#endregion

		#region Lookups

		public void TestLookups()
		{
			TrainingZoneRate rate = GetNewTrainingZoneRate();

			AssertNotNull(rate.Lookups);
			AssertEquals(typeof(TrainingZoneRateLookups), rate.Lookups.GetType());
		}

		#endregion

		#region Validation

		public void TestValidation()
		{
			TrainingZoneRate rate = GetNewTrainingZoneRate();

			AssertNotNull(rate.Validation);
			AssertEquals(typeof(TrainingZoneRateValidation), rate.Validation.GetType());
		}

		#endregion

		#region Get clone and equality

		public void TestEquals()
		{
			TrainingZoneRate rate1 = (TrainingZoneRate)GetBusinessObjectToSerialise();
			TrainingZoneRate rate2 = (TrainingZoneRate)GetBusinessObjectToSerialise();
			AssertEquals(true, rate1.Equals(rate2));
			AssertEquals(rate1, rate2);

			rate2.ZonePK = Factory.New<EDIRefZoneHeader>().PK;
			AssertEquals(false, rate1.Equals(rate2));
			AssertNotEquals(rate1, rate2);
			rate2.ZonePK = rate1.ZonePK;

			rate2.RateAmount = (ZDecimal)123m;
			AssertEquals(false, rate1.Equals(rate2));
			AssertNotEquals(rate1, rate2);
			rate2.RateAmount = rate1.RateAmount;

			rate2.CurrencyCode = "GBP";
			AssertEquals(false, rate1.Equals(rate2));
			AssertNotEquals(rate1, rate2);
			rate2.CurrencyCode = rate1.CurrencyCode;

			rate2.ZonePK = Factory.New<EDIRefZoneHeader>().PK;
			rate2.RateAmount = (ZDecimal)123;
			rate2.CurrencyCode = "SOS";
			AssertEquals(false, rate1.Equals(rate2));
			AssertNotEquals(rate1, rate2);

			AssertEquals(false, rate1.Equals(zoneForTest));
			AssertNotEquals(rate1, zoneForTest);
			AssertEquals(false, rate1.Equals(null));
			AssertNotEquals(rate1, null);
		}

		public void TestCloneTrainingZoneRate()
		{
			TrainingZoneRate rate1 = (TrainingZoneRate)GetBusinessObjectToSerialise();
			TrainingZoneRate rate2 = (TrainingZoneRate)rate1.Clone(null, null);
			AssertEquals((ZDecimal)989, rate2.RateAmount);
			AssertEquals(zoneForTest.PK, rate2.ZonePK);
			AssertEquals("USD", rate2.CurrencyCode);
		}

		public void TestGetHashCode()
		{
			TrainingZoneRate rate1 = (TrainingZoneRate)GetBusinessObjectToSerialise();
			TrainingZoneRate rate2 = (TrainingZoneRate)GetBusinessObjectToSerialise();
			AssertEquals(rate1.GetHashCode(), rate2.GetHashCode());

			rate2.ZonePK = Factory.New<EDIRefZoneHeader>().PK;
			AssertNotEquals(rate1.GetHashCode(), rate2.GetHashCode());
			rate2.ZonePK = rate1.ZonePK;

			rate2.RateAmount = (ZDecimal)123m;
			AssertNotEquals(rate1.GetHashCode(), rate2.GetHashCode());
			rate2.RateAmount = rate1.RateAmount;

			rate2.CurrencyCode = "GBP";
			AssertNotEquals(rate1.GetHashCode(), rate2.GetHashCode());
			rate2.CurrencyCode = rate1.CurrencyCode;

			rate2.ZonePK = Factory.New<EDIRefZoneHeader>().PK;
			rate2.RateAmount = (ZDecimal)123;
			rate2.CurrencyCode = "SOS";
			AssertNotEquals(rate1.GetHashCode(), rate2.GetHashCode());
		}

		#endregion

		public void TestSetParentCollection()
		{
			TrainingZoneRateCollection collection = new TrainingZoneRateCollection();
			TrainingZoneRate rate1 = new TrainingZoneRate();
			rate1.SetParentCollection(collection);
			AssertEquals(collection, rate1.ParentCollection);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Should only have one parent collection")]
		public void TestSetParentCollection_ThrowInvalidOperationException()
		{
			TrainingZoneRate rate1 = new TrainingZoneRate();
			rate1.SetParentCollection(new TrainingZoneRateCollection());
			rate1.SetParentCollection(new TrainingZoneRateCollection());
		}
	}
}
