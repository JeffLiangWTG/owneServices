using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUDSBJobDeclarationIAccIntegrationDataProviderTest : TestCaseWithFactory
	{
		public void TestDisbursementChargeCodes()
		{
			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "Test2";
			var chargeCodeWithDate = new ChargeCodeWithDate();
			chargeCodeWithDate.ChargeCode = chargeCode2.PK;
			chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Now.AddDays(-1);

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();

			var wholeEntryList = new Registry.Business.Customs.AU.EntryChargeTypeList();

			var provider = new AUDSBJobDeclarationIAccIntegrationDataProvider(declaration) as IAccIntegrationDataProvider;
			AssertEquals(1, provider.DisbursementChargeCodes.Length);
			AssertCollectionNotContains(chargeCode2.PK, provider.DisbursementChargeCodes);

			chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Now.AddDays(1);
			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);
			provider = new AUDSBJobDeclarationIAccIntegrationDataProvider(declaration);
			AssertEquals(2, provider.DisbursementChargeCodes.Length);
			AssertCollectionContains(chargeCode2.PK, provider.DisbursementChargeCodes);
		}

		public void TestInvDataProviders()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;

			var provider = new AUDSBJobDeclarationIAccIntegrationDataProvider(declaration, false) as IAccIntegrationDataProvider;
			AssertEquals(1, provider.InvDataProviders.Length);
			AssertType<AUDSBEntryHeaderIAccInvoiceDataProvider>(provider.InvDataProviders[0]);

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			provider = new AUDSBJobDeclarationIAccIntegrationDataProvider(declaration, false);
			AssertEquals(2, provider.InvDataProviders.Length);
		}
	}
}
