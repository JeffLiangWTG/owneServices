using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceHeaderImport))]
	public class PriceHeaderImportTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSchemaMaxLength()
		{
			AssertEquals(OrgHeader.Schema.OH_CodeMaxLength, PriceHeaderImport.Schema.OrgCodeMaxLength);
			AssertEquals(ClientLicencePriceHeader.Schema.L6_RX_NKCurrencyMaxLength, PriceHeaderImport.Schema.CurrencyMaxLength);
			AssertEquals(ClientLicencePriceHeader.Schema.L6_PricelistVersionMaxLength, PriceHeaderImport.Schema.PricelistVersionMaxLength);
		}
	}
}
