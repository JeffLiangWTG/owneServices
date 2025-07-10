using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusStatementLineCharge))]
	sealed class CusStatementLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestChangeMaxLengthAndDecimalPlace()
		{
			var charge = Factory.New<CusStatementHeader>().StatementLines.AddNew().Charges.AddNew();

			AssertEquals(3, charge.B4_ChargeTypeInfo.MaxLength);

			AssertHasCustomAttribute<DecimalPlacesAttribute>(charge.GetType(), CusStatementLineCharge.Schema.B4_ChargeAmount, true, attrib => attrib.DecimalPlaces == 0);
		}
	}
}
