using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(JobPaymentBasisViewCollection))]
	public class JobPaymentBasisViewCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestPaymentBasesConversionForDisplay()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "FRT";
			chargeCode.AC_Desc = "Freight";

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = chargeCode.PK;

			var sourceBases = new List<JobPaymentBasis>();

			var flatBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			flatBasis.PBS_FlatRate = 100m;
			flatBasis.PBS_RX_NKRateCurrency = "AUD";
			flatBasis.PBS_ChargeableDescription = "DESC";
			flatBasis.PBS_AdapterID = "SHP01";
			flatBasis.PBS_JR = charge.PK;
			flatBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.FLT);

			sourceBases.Add(flatBasis);

			var perUnitBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			perUnitBasis.PBS_PerUnitRate = 5m;
			perUnitBasis.PBS_RateUnit = "KG";
			perUnitBasis.PBS_ChargeableAmount = 100m;
			perUnitBasis.PBS_ChargeableUnit = "KG";
			perUnitBasis.PBS_RX_NKRateCurrency = "AUD";
			perUnitBasis.PBS_AdapterID = "SHP01";
			perUnitBasis.PBS_RateUnitType = "Weight";
			perUnitBasis.PBS_AdapterType = nameof(AdapterType.Shipment);
			perUnitBasis.PBS_ChargeableUnitType = "Weight";
			perUnitBasis.PBS_JR = charge.PK;
			perUnitBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);

			sourceBases.Add(perUnitBasis);

			var percentageBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			percentageBasis.PBS_ChargeableAmount = 1000;
			percentageBasis.PBS_ChargeableUnit = "AUD";
			percentageBasis.PBS_PerUnitRate = 110m;
			percentageBasis.PBS_RateUnit = "100";
			percentageBasis.PBS_RX_NKRateCurrency = "AUD";
			percentageBasis.PBS_AdapterID = "SHP01";
			percentageBasis.PBS_JR = charge.PK;
			percentageBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.PER);

			sourceBases.Add(percentageBasis);

			var minBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			minBasis.PBS_MinRate = 1000m;
			minBasis.PBS_ChargeableAmount = 1m;
			minBasis.PBS_RX_NKRateCurrency = "AUD";
			minBasis.PBS_AdapterID = "SHP01";
			minBasis.PBS_JR = charge.PK;
			minBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.MIN);

			var maxBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			maxBasis.PBS_MaxRate = 1200m;
			maxBasis.PBS_ChargeableAmount = 1m;
			maxBasis.PBS_RX_NKRateCurrency = "AUD";
			maxBasis.PBS_AdapterID = "SHP01";
			maxBasis.PBS_JR = charge.PK;
			maxBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.MAX);

			sourceBases.Add(minBasis);
			sourceBases.Add(maxBasis);

			var containerBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			containerBasis.PBS_ChargeableAmount = 2m;
			containerBasis.PBS_AdapterID = "SHP01";
			containerBasis.PBS_RX_NKRateCurrency = "AUD";
			containerBasis.PBS_ChargeableDescription = "040401";
			containerBasis.PBS_ChargeableUnit = "20GP";
			containerBasis.PBS_RateUnit = QuantityUnit.CN;
			containerBasis.PBS_PerUnitRate = 50m;
			containerBasis.PBS_JR = charge.PK;
			containerBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);

			sourceBases.Add(containerBasis);

			var collection = new JobPaymentBasisViewCollection(Factory);
			collection.AddRange(sourceBases);

			AssertEquals(6, collection.Count);

			var expectedCollection = new object[]
			{
				new
				{
					Amount = (ZDecimal)100m,
					Quantity = ZString.Empty,
					PBS_ChargeableUnit = ZString.Empty,
					RateValue = (ZDecimal)100m,
					RateUnit = ZString.Empty,
					PBS_RateReference = (ZString)"FLT"
				},
				new
				{
					Amount = (ZDecimal)500m,
					Quantity = (ZString)"100",
					PBS_ChargeableUnit = (ZString)"KG",
					RateValue = (ZDecimal)5m,
					RateUnit = (ZString)"KG",
					PBS_RateReference = (ZString)"UNT"
				},
				new
				{
					Amount = (ZDecimal)1100m,
					Quantity = (ZString)"1000",
					PBS_ChargeableUnit = (ZString)"AUD",
					RateValue = (ZDecimal)110m,
					RateUnit = (ZString)"%",
					PBS_RateReference = (ZString)"PER"
				},
				new
				{
					Amount = (ZDecimal)1000m,
					Quantity = ZString.Empty,
					PBS_ChargeableUnit = ZString.Empty,
					RateValue = (ZDecimal)1000m,
					RateUnit = ZString.Empty,
					PBS_RateReference = (ZString)"MIN"
				},
				new
				{
					Amount = (ZDecimal)1200m,
					Quantity = ZString.Empty,
					PBS_ChargeableUnit = ZString.Empty,
					RateValue = (ZDecimal)1200m,
					RateUnit = ZString.Empty,
					PBS_RateReference = (ZString)"MAX"
				},
				new
				{
					Amount = (ZDecimal)100m,
					Quantity = (ZString)"2",
					PBS_ChargeableUnit = (ZString)"20GP",
					RateValue = (ZDecimal)50m,
					RateUnit = (ZString)"CN",
					PBS_RateReference = (ZString)"UNT"
				},
			};
			var actualCollection = collection.Select(c => new
			{
				c.Amount,
				c.Quantity,
				c.PBS_ChargeableUnit,
				c.RateValue,
				c.RateUnit,
				c.PBS_RateReference
			});

			AssertContainsExactElementsInAnyOrder(expectedCollection, actualCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobPaymentBasisViewCollection(Factory);
		}
	}
}
