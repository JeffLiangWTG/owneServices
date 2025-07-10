using System.Collections.Generic;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business.Testing
{
	public class SameRateComparerTest : JobPaymentBasisComparerTest
	{
		protected override PaymentBasis DifferentPaymentBasis
		{
			get
			{
				var quantity = new Quantity(1, QuantityUnit.HB);
				var rateInfo = RateInfo.CreateMIN(450, Constants.CurrencyCodes.Australia);

				return new PaymentBasis(quantity, rateInfo, AdapterType.Shipment, "S00001234");
			}
		}

		protected override IEqualityComparer<JobPaymentBasis> Comparer => JobPaymentBasisComparer.SameRate;
	}
}