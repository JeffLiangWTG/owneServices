using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.DocumentWrappers.Statement.Testing;

sealed class DocStatementTest : DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var header = Factory.New<CusStatementHeader>();
		header.B2_StatementType = StatementPeriodicityList.Codes.Day;
		return DocStatement.New(header, Factory);
	}

	public void TestProperties()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.OH_FullName = "importer name";
		orgHeader.MainAddress.OA_Address1 = "addresse importer";
		orgHeader.MainAddress.OA_Address2 = "addresse importer address2";
		orgHeader.MainAddress.OA_PostCode = "233333";
		orgHeader.MainAddress.OA_City = "Paris";

		orgHeader.OH_IsConsignee = true;

		var orgCusAccount = orgHeader.DeltaAgreementNumberCollection.AddNew();
		orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
		orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
		orgCusAccount.CZ_OH = orgHeader.PK;
		orgCusAccount.CZ_Account = "DGI002";
		orgCusAccount.CZ_Issuer = "issuer";
		orgCusAccount.CZ_RepresentativeID = "B92F8A8B";
		orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;

		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Enterprise.Core.Constants.CountryCodes.France);
		orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);

		var dec1 = Factory.New<JobDeclaration>();
		dec1.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		dec1.JE_OH_Importer = orgHeader.PK;
		dec1.JE_DeclarationReference = "0738/19";
		var entry1 = dec1.CustomsEntryHeaders.AddNew();
		var entryNumber1 = CusEntryNumber.New(entry1, dec1.JE_MessageType, Core.Constants.CountryCodes.France);
		entryNumber1.CE_EntryNum = "1906142283";
		Factory.Save();

		var resourceRetriever = new EmbeddedResourceRetriever();
		var importMessageText = resourceRetriever.GetString("Enterprise.Customs.FR.DocumentWrappers.Testing.TestFiles.DCGResponseFromCustoms.xml");
		var message = Factory.New<DCGResponseFREDIMessage>();
		message.EM_MessageType = MessageTypeList.Codes.DCG;
		message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
		message.EM_MessageText = importMessageText;
		var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;

		var header = CusStatementHeader.LoadOrCreateDCGStatementHeader(Factory, GlbCompany.CurrentCompany.PK, dataProvider, true);

		header.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
		header.B2_OH_Importer = orgHeader.PK;
		header.B2_ImporterCustomsID = "importerCustomsID";
		header.B2_EntryFilerCode = "DGI002";
		header.B2_CheckNo = "CheckNo";

		header.EntryNumber = "1906142283";

		var chargedetail = header.ChargesDetail;

		var charge = chargedetail.Charges.AddNew();
		charge.B4_ChargeAmount = 10m;
		charge.B4_ChargeType = "A325";
		charge.B4_MethodOfPayment = "1";
		var charge2 = chargedetail.Charges.AddNew();
		charge2.B4_ChargeAmount = 20m;
		charge2.B4_ChargeType = "A375";
		charge2.B4_MethodOfPayment = "2";
		var charge3 = chargedetail.Charges.AddNew();
		charge3.B4_ChargeAmount = 40m;
		charge3.B4_ChargeType = "A445";
		charge3.B4_MethodOfPayment = "3";
		var charge4 = chargedetail.Charges.AddNew();
		charge4.B4_ChargeAmount = 40m;
		charge4.B4_ChargeType = "A445";
		charge4.B4_MethodOfPayment = "6";

		var statementEntry1 = header.Entries.AddNew();
		statementEntry1.B3_EntryNum = "b3impentry";

		var statementEntry2 = header.Entries.AddNew();
		statementEntry2.B3_EntryNum = "b3expentry";

		var wrapper = DocStatement.New(header, Factory);

		CombineAssertions(() =>
		{
			AssertEquals("Direction", StatementEntryTypeList.Descriptions.Import, wrapper.Direction);
			AssertEquals("PrintingPlace", GlbBranch.CurrentBranch.HomePort.Description, wrapper.PrintingPlace);
			AssertEquals("PrintingDate", ZDate.Today.ToString("dd/MM/yyyy"), wrapper.PrintingDate);
			AssertEquals("ImporterID", "FR12345678900001", wrapper.ImporterID);
			AssertEquals("ImporterAddress", @"importer name
addresse importer
addresse importer address2
233333
Paris", wrapper.ImporterAddress);
			AssertEquals("RepresentativeID", "importerCustomsID", wrapper.RepresentativeID);
			AssertEquals("CustomsOffice", orgCusAccount.CZ_Issuer, wrapper.CustomsOffice);
			AssertEquals("AgreementNumber", "DGI002", wrapper.AgreementNumber);
			AssertEquals("EntryNumber", "1906142283", wrapper.EntryNumber);
			AssertEquals("ValidationDate", message.EM_MessageDateTime.ToString("dd/mm/yyyy"), "");  //wrapper.ValidationDate
			AssertEquals("PeriodStartDate", "01/07/2019", wrapper.PeriodStartDate);
			AssertEquals("PeriodEndDate", "31/07/2019", wrapper.PeriodEndDate);
			AssertEquals("EntriesSummary", "b3impentry - b3expentry", wrapper.EntriesSummary);
			AssertEquals("DefermentApprovalNumber", "CheckNo", wrapper.DefermentApprovalNumber);
			AssertType<DocStatementChargesCollection>(wrapper.Charges);

			var statementChargeWrapper = (DocStatementCharge)wrapper.Charges.ElementAt(0);
			AssertEquals("Amount", 10m, statementChargeWrapper.Amount);
			AssertEquals("TotalChargesAmount", 30m, wrapper.TotalChargesAmount);
		});
	}
}
