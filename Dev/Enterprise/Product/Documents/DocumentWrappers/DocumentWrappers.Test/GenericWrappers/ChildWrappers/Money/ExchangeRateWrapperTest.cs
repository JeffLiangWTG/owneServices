using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class ExchangeRateWrapperTest : GenericWrapperTest
	{
		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
ExchangeRate
======================================================================
Name                                    Type
----------------------------------------------------------------------
Currency                                Currency
BuyRate                                 Decimal
SellRate                                Decimal
SellRateAgent                           Decimal
";
			}
		}
	}
}
