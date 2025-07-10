using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceHeaderLinkImport))]
	public class PriceHeaderLinkImportTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSchemaMaxLength()
		{
			AssertEquals(OrgHeader.Schema.OH_CodeMaxLength, PriceHeaderLinkImport.Schema.OrgCodeMaxLength);
			AssertEquals(ClientLicencePriceHeader.Schema.L6_RX_NKCurrencyMaxLength, PriceHeaderLinkImport.Schema.CurrencyMaxLength);
			AssertEquals(LicenceDatabase.Schema.LD_ServerCodeMaxLength, PriceHeaderLinkImport.Schema.ServerCodeMaxLength);
			AssertEquals(ClientLicencePriceHeader.Schema.L6_PricelistVersionMaxLength, PriceHeaderLinkImport.Schema.PricelistVersionMaxLength);
		}
	}
}
