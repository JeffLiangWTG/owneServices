using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	public class CDSJobComInvoiceLineTest : TestCaseWithFactory
	{
		public void TestLookups()
		{
			var declaration = CreateCDSJobDeclaration(Factory);
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<CDSJobComInvoiceLineLookups>(invLine.Lookups);
		}

		public void TestZG_MethodOfPayment_Defaulting()
		{
			var declaration = CreateCDSJobDeclaration(Factory);
			declaration.JE_PaymentMethod = "A";
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals("E", invLine.ZG_MethodOfPayment);
		}

		public static JobDeclaration CreateCDSJobDeclaration(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			return declaration;
		}

		public void TestPaymentMethodsAndDefermentAccountNumberSet_IsUCCCompliant()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori,
				"1234", Core.Constants.CountryCodes.UnitedKingdom);
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "Declarant";
			declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori,
				"5678", Core.Constants.CountryCodes.UnitedKingdom);
			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.Address1 = "Address";
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OA_DeclarantAddress = declarantAddress.PK;
			dec.JE_ApplicationCode =
				GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var invoice = dec.Invoices.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();

			invLine1.ZG_MethodOfPayment = "D";

			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			dec.JE_DefermentAccountNumber = "1234567";

			CombineAssertions("Payment Method D should not have supporting docs", () =>
			{
				Assert("C506 1..7 should not be on InvLine1",
					!invLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x =>
						x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C505 guaranteenotrequired should not be on InvLine1",
					!invLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x =>
						x.CSI_Code == "C505" && x.CSI_ReferenceNumber == "GBCGUguaranteenotrequired" && x.CSI_Actions == "C" && x.CSI_Availability == "C"));
			});

			invLine1.ZG_MethodOfPayment = "E";

			CombineAssertions("Payment Method E should have supporting docs", () =>
			{
				Assert("C506 1..7 should be on InvLine1",
					invLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x =>
						x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C505 guaranteenotrequired should be on InvLine1",
					invLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x =>
						x.CSI_Code == "C505" && x.CSI_ReferenceNumber == "GBCGUguaranteenotrequired" && x.CSI_Actions == "C" && x.CSI_Availability == "C"));
			});
		}

		public void TestSetAdditionalInfoOnClaimEuSubsidyOrNiGoodsAtRiskOrMovingToROIChanged()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();

			dec.JE_NorthernIrelandMode = Constants.NorthernIrelandModeCodes.N2G;
			dec.JE_ClaimEuSubsidy = true;
			dec.JE_NiGoodsAtRiskOfMovingToROI = true;

			CombineAssertions("Claim EU Subsidy and NiGoodsAtRiskOfMovingToROI are false for N2G", () =>
			{
				Assert("Additional Info NIAID should be absent for N2G",
					!invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIAID"));
				Assert("Additional Info NIREM should be absent for N2G",
					!invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIREM"));
			});

			dec.JE_ClaimEuSubsidy = false;
			dec.JE_NorthernIrelandMode = Constants.NorthernIrelandModeCodes.G2N;
			dec.JE_ClaimEuSubsidy = true;
			dec.JE_NiGoodsAtRiskOfMovingToROI = true;

			CombineAssertions("Claim EU Subsidy and NiGoodsAtRiskOfMovingToROI are true for G2N", () =>
			{
				Assert("Additional Info NIAID should be present for G2N",
					invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIAID"));
				Assert("Additional Info NIREM should be absent for G2N",
					!invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIREM"));
			});

			dec.JE_ClaimEuSubsidy = false;
			dec.JE_NiGoodsAtRiskOfMovingToROI = false;

			CombineAssertions("Claim EU Subsidy and NiGoodsAtRiskOfMovingToROI are false when unticked", () =>
			{
				Assert("Additional Info NIAID should not be present",
					!invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIAID"));
				Assert("Additional Info NIREM should be present",
					invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIREM"));
			});
		}

		public void TestSetAdditionalInfoOnNorthernIrelandModeChanged()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = "CDS";
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();

			var ai1 = invLine.AdditionalInfos.AddNew();
			var ai2 = invLine.AdditionalInfos.AddNew();
			ai1.CSI_Code = "NIDOM";
			ai2.CSI_Code = "NIEXP";

			dec.JE_NorthernIrelandMode = "NII";
			CombineAssertions("NorthernIrelandMode: NII", () =>
			{
				Assert("NIDOM should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIDOM"));
				Assert("NIREM should be present", invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIREM"));
				Assert("NIEXP should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIEXP"));
				Assert("NIIMP should be present", invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIIMP"));
			});

			dec.JE_NorthernIrelandMode = "G2N";
			CombineAssertions("NorthernIrelandMode: G2N", () =>
			{
				Assert("NIDOM should be present", invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIDOM"));
				Assert("NIREM should be present", invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIREM"));
				Assert("NIEXP should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIEXP"));
				Assert("NIIMP should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIIMP"));
			});

			dec.JE_NiGoodsAtRiskOfMovingToROI = true;
			CombineAssertions("NorthernIrelandMode: G2N, but at risk (not REM-aining)", () =>
			{
				Assert("NIDOM should be present", invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIDOM"));
				Assert("NIREM should be absent", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIREM"));
				Assert("NIEXP should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIEXP"));
				Assert("NIIMP should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIIMP"));
			});

			dec.JE_NorthernIrelandMode = "";
			CombineAssertions("NorthernIrelandMode: Blank", () =>
			{
				Assert("NIREM should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIREM"));
				Assert("NIDOM should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIDOM"));
				Assert("NIEXP should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIEXP"));
				Assert("NIIMP should not be present", !invLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIIMP"));
			});
		}

		public void TestAdditionalInfoNorthernIrelandDefault()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var europeanUnionDataGrouping =
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes
					.CustomsDeclarationService);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port,
				"Port Code");
			helper.CreateCusCodeListWithAttribute(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT001", "Test Port 1",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "GVMS", "GVMS Attribute");
			Factory.Save();

			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_IsShippingLine = true;
			shippingLine.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_OH_ShippingLine = shippingLine.PK;

			var invoice = dec.Invoices.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();

			CombineAssertions("Invoice Line Added. ClaimEuSubsidy/NiGoodsAtRiskOfMovingToRoi/NorthernIrelandMode/IsGvmsPort are false", () =>
			{
				Assert("Addition Info NIAID should not be present", !invLine1.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIAID"));
				Assert("Addition Info NIREM should not be present", !invLine1.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIREM"));
				Assert("Addition Info NIIMP should not be present", !invLine1.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIIMP"));
				Assert("Addition Info RRS01 should not be present", !dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01"));
			});

			dec.JE_ClaimEuSubsidy = true;
			dec.JE_NorthernIrelandMode = "NII";
			dec.JE_NiGoodsAtRiskOfMovingToROI = true;
			dec.JE_IsGvmsPort = true;

			var invLine2 = invoice.InvoiceLines.AddNew();

			CombineAssertions("Examples of all defaults set", () =>
			{
				Assert("Additional Info NIAID should be present", invLine2.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIAID"));
				Assert("Additional Info NIREM should be absent", !invLine2.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIREM"));
				Assert("Additional Info NIIMP should be present", invLine2.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "NIIMP"));
				Assert("Additional Info RRS01 should be present on declaration header infos", dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01"));
				Assert("Additional Info Description Name is correct", dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01" && x.CSI_Description == "GB999999999888"));
			});
		}
	}
}
