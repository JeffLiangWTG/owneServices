using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	public class LocalExportJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJZ_OH_Manufacturer()
		{
			var wrongManufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "NAT", "RK1", "");
			var emptyManufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "");
			var manufacturerWithoutUnipassID = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK3", "");
			var manufacturerWithoutUnipassIDCodes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1234567890" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturerWithoutUnipassID, manufacturerWithoutUnipassIDCodes);

			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK4", "");
			var manufacturerCodes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1234567890" },
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "123456789012345" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer, manufacturerCodes);

			invoice.Validation.ValidateJZ_OH_Manufacturer();
			AssertHasMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_OH_Manufacturer = wrongManufacturer.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, "The Manufacturer must not be an individual.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, "There is no Business Registration Number for this organization. Please press F3 here and add a number of type 'GBR' in Config > Registration Numbers/Codes on the Organization form.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

			invoice.JZ_OH_Manufacturer = emptyManufacturer.PK;
			AssertNoMessageErrors(invoice.JZ_OH_ManufacturerInfo);

			declaration.JE_ExportGoodsType = LocalExportGoodsTypeList.Codes.ManufacturingInProcess;
			invoice.Validation.ValidateJZ_OH_Manufacturer();
			AssertNoMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, "The Manufacturer must not be an individual.");
			AssertHasMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, "There is no Business Registration Number for this organization. Please press F3 here and add a number of type 'GBR' in Config > Registration Numbers/Codes on the Organization form.");
			AssertHasMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

			invoice.JZ_OH_Manufacturer = manufacturerWithoutUnipassID.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, "The Manufacturer must not be an individual.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, "There is no Business Registration Number for this organization. Please press F3 here and add a number of type 'GBR' in Config > Registration Numbers/Codes on the Organization form.");
			AssertHasMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

			invoice.JZ_OH_Manufacturer = manufacturer.PK;
			AssertNoMessageErrors(invoice.JZ_OH_ManufacturerInfo);

			declaration.JE_ExportGoodsType = LocalExportGoodsTypeList.Codes.OriginalState;
			invoice.Validation.ValidateJZ_OH_Manufacturer();
			AssertHasMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, MandatoryValidation.DoNotEntered);
			invoice.JZ_OH_Manufacturer = ZGuid.Empty;
			AssertNoMessageErrorContaining(invoice.JZ_OH_ManufacturerInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestJZ_OH_Buyer()
		{
			var wrongBuyer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "NAT", "RK1", "");
			var emptyBuyer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "");
			emptyBuyer.MainAddress.OA_Address1 = "";
			var buyer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK3", "");
			buyer.MainAddress.OA_Address1 = "Test Address";
			var buyerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1234567890" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(buyer, buyerCodes);

			invoice.Validation.ValidateJZ_OH_Buyer();
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			invoice.JZ_OH_Buyer = wrongBuyer.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The Importer must not be an individual.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The main address is missing. Press F3 here and enter the address.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Business Registration Number", Constants.IdentificationType.BusinessRegNo));
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingCompanyNameMessage);
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingRepresentativeMessage);

			invoice.JZ_OH_Buyer = emptyBuyer.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The Importer must not be an individual.");
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The main address is missing. Press F3 here and enter the address.");
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Business Registration Number", Constants.IdentificationType.BusinessRegNo));
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingCompanyNameMessage);
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingRepresentativeMessage);

			invoice.JZ_OH_Buyer = buyer.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The Importer must not be an individual.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The main address is missing. Press F3 here and enter the address.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Business Registration Number", Constants.IdentificationType.BusinessRegNo));
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingCompanyNameMessage);
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingRepresentativeMessage);

			buyer.OH_FullName = "READYKOREA";
			TestOrgDataSetUpHelper.AddOrgContact(buyer, "BuyerName", true);
			invoice.JZ_OH_Buyer = buyer.PK;
			AssertNoMessageErrors(invoice.JZ_OH_BuyerInfo);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			invoice.JZ_OH_Buyer = wrongBuyer.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The Importer must not be an individual.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The main address is missing. Press F3 here and enter the address.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Business Registration Number", Constants.IdentificationType.BusinessRegNo));
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingCompanyNameMessage);
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingRepresentativeMessage);

			invoice.JZ_OH_Buyer = emptyBuyer.PK;
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The Importer must not be an individual.");
			AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, "The main address is missing. Press F3 here and enter the address.");
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Business Registration Number", Constants.IdentificationType.BusinessRegNo));
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingCompanyNameMessage);
			AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, LocalExportJobComInvoiceHeaderValidation.MissingRepresentativeMessage);

			invoice.JZ_OH_Buyer = buyer.PK;
			AssertNoMessageErrors(invoice.JZ_OH_BuyerInfo);
		}

		public void TestTotalGrossWeight()
		{
			invoice.JZ_Weight = -10;
			AssertHasMessageErrorContaining(invoice.JZ_WeightInfo, "Please enter an 'Inv. Gross Weight' greater than 0.");
			invoice.JZ_Weight = 0;
			AssertHasMessageErrorContaining(invoice.JZ_WeightInfo, "Please enter an 'Inv. Gross Weight' greater than 0.");
			invoice.JZ_Weight = 60;
			AssertNoMessageErrors(invoice.JZ_WeightInfo);
		}

		public void TestJZ_NoOfPacks()
		{
			invoice.JZ_NoOfPacks = -1;
			AssertHasMessageErrorContaining(invoice.JZ_NoOfPacksInfo, "Please enter a 'Packages' greater than or equal to 0.");

			invoice.JZ_NoOfPacks = 0;
			AssertNoMessageErrors(invoice.JZ_NoOfPacksInfo);

			invoice.JZ_NoOfPacks = 1;
			AssertNoMessageErrors(invoice.JZ_NoOfPacksInfo);
		}

		public void TestZeroFreightInsuranceNoWarnings()
		{
			AssertEquals(invoice.JZ_Calc_FOBAmount, invoice.JZ_Calc_CIFAmount);
			invoice.Validation.ValidateAll();
			AssertNoWarnings(invoice.JZ_Calc_CIFAmountInfo);
		}

		public void TestRefundApplicantType()
		{
			declaration.JE_ExportGoodsType = "1";

			invoice.JZ_DRWApplicantType = ZString.Empty;
			AssertHasMessageErrorContaining(invoice.JZ_DRWApplicantTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoice.JZ_DRWApplicantTypeInfo, "Drawback Applicant Type must be '1' if Goods Type is 1.");
			invoice.JZ_DRWApplicantType = "9";
			AssertNoMessageErrorContaining(invoice.JZ_DRWApplicantTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoice.JZ_DRWApplicantTypeInfo, ListValidation.InvalidCodeMessageError);
			invoice.JZ_DRWApplicantType = "1";
			AssertNoMessageErrors(invoice.JZ_DRWApplicantTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			invoice = declaration.Invoices.AddNew();
		}
		JobComInvoiceHeader invoice;
		JobDeclaration declaration;
	}
}
