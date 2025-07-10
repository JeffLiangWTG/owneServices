using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class DeltaGVATDeferStrategyTest : VATDeferStrategyAbstractTest
	{
		public void TestDefermentAccountNumberShouldBeDeclarantWhenNoDANAgainstClient()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();
			importer.CustomsCodes.RemoveAndDeleteAll();
			var declarant = CreateDeclarant(declaration.CountryCode);
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			declaration.JE_OA_DeclarantAddress = declarant.Addresses[0].PK;

			AssertEquals("GetPaymentMethodSource is the declarant, because no DAN has been captured against the importer", "DANZZZ", declaration.JE_DefermentAccountNumber);
		}

		public void TestDefermentAccountNumberShouldBeDefaultedWhenOrganisationChangedForExport()
		{
			var (declaration, supplier) = CreateDeclarationAndImporter();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;

			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals("GetPaymentMethodSource couldn't be inferred, because the declaration is export but no supplier nor declarant was captured", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("GetPaymentMethodSource is the the supplier because the declaration is export and an supplier was captured", "DANXXX", declaration.JE_DefermentAccountNumber);

			var declarant = CreateDeclarant(declaration.CountryCode);

			declaration.JE_CustomsProfile = "DGI002";
			declaration.JE_OA_DeclarantAddress = declarant.Addresses[0].PK;
			AssertEquals("GetPaymentMethodSource is still the supplier, whatever the Customs Profile", "DANXXX", declaration.JE_DefermentAccountNumber);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals("GetPaymentMethodSource is now the declarant, because no supplier is captured, but a declarant has been set against the declaration", "DANZZZ", declaration.JE_DefermentAccountNumber);
		}

		public override void TestDefermentAccountNumberShouldBeDefaultedWhenOrganisationChanged()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();

			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("GetPaymentMethodSource couldn't be inferred, because the declaration is import but no importer nor declarant was captured", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("GetPaymentMethodSource is the the importer because the declaration is import and an importer was captured", "DANXXX", declaration.JE_DefermentAccountNumber);

			var declarant = CreateDeclarant(declaration.CountryCode);

			declaration.JE_CustomsProfile = "DGI002";
			declaration.JE_OA_DeclarantAddress = declarant.Addresses[0].PK;
			AssertEquals("GetPaymentMethodSource is still the importer, whatever the Customs Profile", "DANXXX", declaration.JE_DefermentAccountNumber);

			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("GetPaymentMethodSource is now the declarant, because no importer is captured, but a declarant has been set against the declaration", "DANZZZ", declaration.JE_DefermentAccountNumber);
		}

		public override void TestDefermentAccountNumberShouldBeDefaultedWhenPaymentMethodChanged()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("No payment method has been set, deferment account number should be empty", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_PaymentMethodInfo.ClearValue();
			declaration.JE_DefermentAccountNumberInfo.ClearValue();
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;

			AssertEquals("Payment method changed, so should JE_DefermentAccountNumber", "DANXXX", declaration.JE_DefermentAccountNumber);
		}
	}
}
