using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class VATDeferStrategyTest : EU.Business.Declaration.Testing.VATDeferStrategyTest
{
	public void TestDefaultDefermentAccountNumber_WhenDeclarationIsNotUcc6()
	{
		var importer = Factory.New<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_PaymentMethod = "C";
			AssertEquals("It should not default JE_DefermentAccountNumber from Importer's DAN/DAT since both numbers are not populated.", string.Empty, declaration.JE_DefermentAccountNumber);

			var danCusCode = importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "DAN", "IT");
			declaration.JE_PaymentMethod = "B";
			AssertEquals("It should default JE_DefermentAccountNumber from Importer's DAN when JE_PaymentMethod changed.", "DAN", declaration.JE_DefermentAccountNumber);

			declaration.JE_PaymentMethod = "";
			AssertEquals("It should empty JE_DefermentAccountNumber when empty JE_PaymentMethod", "", declaration.JE_DefermentAccountNumber);

			importer.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "DAT", "IT");
			declaration.JE_PaymentMethod = "C";
			AssertEquals("It should not default JE_DefermentAccountNumber from Importer's DAN/DAT since both numbers are populated.", string.Empty, declaration.JE_DefermentAccountNumber);

			importer.CustomsCodes.RemoveAndDelete(danCusCode);
			declaration.JE_PaymentMethod = "D";
			AssertEquals("It should default JE_DefermentAccountNumber from Importer's DAT when JE_PaymentMethod changed.", "DAT", declaration.JE_DefermentAccountNumber);
		}
	}

	public void TestDefaultDefermentAccountNumber_WhenDeclarationIsUcc6()
	{
		var declarant1 = CreateDeclarant();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.DeferredPayment, declarant1.PK, "111");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.DeferredPayment, declarant1.PK, "222");

		var declarant2 = CreateDeclarant();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.DeferredPayment, declarant2.PK, "333");

		var declaration = Factory.New<JobDeclaration>();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions(() =>
			{
				declaration.JE_OA_DeclarantAddress = declarant1.MainAddress.PK;
				declaration.JE_DefermentAccountNumber = "1232322";
				declaration.JE_PaymentMethod = Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions;
				AssertEquals("When more than one deferment account number is available, JE_DefermentAccountNumber is emptied and not defaulted", "", declaration.JE_DefermentAccountNumber);

				declaration.JE_OA_DeclarantAddress = declarant2.MainAddress.PK;
				declaration.JE_PaymentMethod = Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions;
				AssertEquals("When only one deferment account number is available, JE_DefermentAccountNumber is defaulted", "333", declaration.JE_DefermentAccountNumber);
			});
		}
	}

	protected override string ExpectedDefermentAccountNumberForInvalidMethodOfPayment => "";
}
