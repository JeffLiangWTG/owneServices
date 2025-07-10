using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUASPJobDeclarationIAccIntegrationDataProviderTest : TestCaseWithFactory
	{
		public void TestDisbursementChargeCodes()
		{
			var chargeCodeWithDate1 = new ChargeCodeWithDate();
			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "Test1";
			chargeCodeWithDate1.ChargeCode = chargeCode1.PK;

			var chargeCodeWithDate2 = new ChargeCodeWithDate();
			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "Test2";
			chargeCodeWithDate2.ChargeCode = chargeCode2.PK;

			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCodeWithDate1);
			using (RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode2.PK.ToGuid()))
			{
				var provider = new AUASPJobDeclarationIAccIntegrationDataProvider(declaration, ZGuid.BrettsGuid) as IAccIntegrationDataProvider;
				AssertEquals(1, provider.DisbursementChargeCodes.Length);
				AssertEquals(chargeCode1.PK, provider.DisbursementChargeCodes.FirstOrDefault());
			}
		}

		public void TestInvDataProviders()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var provider = new AUASPJobDeclarationIAccIntegrationDataProvider(declaration, entryHeader.PK, false) as IAccIntegrationDataProvider;
			AssertEquals(1, provider.InvDataProviders.Length);
			AssertType<AUASPEntryHeaderIAccInvoiceDataProvider>(provider.InvDataProviders.FirstOrDefault());

			provider = new AUASPJobDeclarationIAccIntegrationDataProvider(declaration, ZGuid.BrettsGuid, false);
			AssertEquals(0, provider.InvDataProviders.Length);
		}
	}
}
