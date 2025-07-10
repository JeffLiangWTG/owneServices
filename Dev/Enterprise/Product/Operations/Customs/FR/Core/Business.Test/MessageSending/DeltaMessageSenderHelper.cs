using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class DeltaMessageSenderHelper : TestCaseWithFactory
	{
		public Declaration.CusEntryHeader CreateEntryDeclarationForTest(bool isDeltaC)
		{
			var declaration = Factory.New<JobDeclaration>();
			var deltaMode = isDeltaC ? OrgCusAccountDeltaGTypeList.Codes.G1 : OrgCusAccountDeltaGTypeList.Codes.G2;
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MasterBill = "UnitTest";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var declarantAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = OrgCusCode.FranceCodeTypes.Siret;
			orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DG1", ZString.Empty, ZString.Empty, "AB03FCC5");
			orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DG2", ZString.Empty, ZString.Empty, "AB03FCC5");

			declarantAddress.OA_OH = orgHeader.PK;
			declarantAddress.OA_Address1 = "Eugene Leroy Street ";
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(orgHeader);
			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;

			Factory.Save();
			#region Organisation Registration number : SRT

			var orgCusCodeSrt = Factory.New<OrgCusCode>();
			orgCusCodeSrt.OK_OA_PremisesAddress = declarantAddress.PK;
			orgCusCodeSrt.OK_CodeType = OrgCusCode.FranceCodeTypes.Siret;
			orgCusCodeSrt.OK_CustomsRegNo = "FR33159700500064";
			orgCusCodeSrt.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			orgCusCodeSrt.OK_OH = declarantAddress.OA_OH;

			orgHeader.CustomsCodes.Add(orgCusCodeSrt);

			#endregion

			#region Organisation Registration number : CBR

			var orgCusCodeCbr = Factory.New<OrgCusCode>();
			orgCusCodeCbr.OK_OA_PremisesAddress = declarantAddress.PK;
			orgCusCodeCbr.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
			orgCusCodeCbr.OK_CustomsRegNo = "FR33159700500064";
			orgCusCodeCbr.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			orgCusCodeCbr.OK_OH = declarantAddress.OA_OH;

			orgHeader.CustomsCodes.Add(orgCusCodeCbr);

			#endregion

			var cei = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.FillWithValidTestData();

			var customOffice = declaration.CustomsOffices.AddNew();
			customOffice.CY_Code = "ENT";
			customOffice.CY_Type = "EUO";
			customOffice.CY_Data = "FR000130";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "FRPAR";
			declaration.JE_OH_Importer = importer.PK;
			var countryCodeImporter = Factory.NewWithValidTestData<RefCountry>();

			declaration.Importer.FillWithValidTestData();
			declaration.Importer.MainAddress.OA_PostCode = "24130";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "AUSYD";
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_DeltaMode = deltaMode;

			var shutUp = new SendsMessagesToCustomsShutterUpperer();

			var mergeResult = declaration.DoMerge(shutUp);

			Assert("Merge failed", mergeResult);

			Factory.Save();

			return declaration.CustomsEntryHeaders[0];
		}
	}
}
