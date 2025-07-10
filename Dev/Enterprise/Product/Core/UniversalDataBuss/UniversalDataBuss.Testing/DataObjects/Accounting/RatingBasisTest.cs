using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(RatingBasis))]
	class RatingBasisTest : DataObjectTestCase<RatingBasis>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(RatingBasis.OriginKey), JobPaymentBasisSchema.PBS_AdapterID.MaxLength },
				{ nameof(RatingBasis.OriginType), JobPaymentBasisSchema.PBS_AdapterType.MaxLength },
				{ nameof(RatingBasis.OriginAdditionalReference), JobPaymentBasisSchema.PBS_ChargeableDescription.MaxLength },
				{ nameof(RatingBasis.RateReference), JobPaymentBasisSchema.PBS_RateReference.MaxLength },
			};
		}

		public void TestCurrencyDoesntHaveMandatoryAttribute()
		{
			var currencyPropertyType = typeof(RatingBasis).GetProperty(nameof(RatingBasis.Currency)).PropertyType;
			var currencyMandatoryAttribute = currencyPropertyType.GetAttribute<MandatoryAttribute>();

			AssertEquals(null, currencyMandatoryAttribute);
		}
	}
}
