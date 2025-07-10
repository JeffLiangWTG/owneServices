using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SADMessageSerializationTest : TestCaseWithFactory
{
	protected override void SetUp()
	{
		base.SetUp();
		SetUpRegistry();
		SetUpRefData();
		SetUpDeclaration();
	}

	void SetUpRegistry()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "DEC";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "AAAA")
			.AppendAccountDetail("AAAA-DEC", "DEC")
			.Build();
	}

	void SetUpDeclaration()
	{
		var refUNLOCO = Factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "FIN";
		refUNLOCO.RL_RN_NKCountryCode = "IT";
		var refCountryStates = Factory.New<RefCountryStates>();
		refCountryStates.RW_Code = "AP";
		refUNLOCO.RL_RW = refCountryStates.PK;

		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = MessageType;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_CustomsProfile = "AAAA-DEC";
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_RL_NKFinalDestination = "FIN";
		declaration.ZG_Box18TransportNationality = "KR";
		declaration.ZG_Box18TransportID = "RX2839A";
		declaration.JE_ContainerMode = "FCL";
		declaration.JE_TransportMode = "AIR";
		declaration.JE_VesselName = "AE";
		declaration.JE_VoyageFlightNo = "4343";
		declaration.JE_TransportModeInland = "OWN";
		declaration.ZG_AuthorisationNumber = "123456D";

		entryInstruction.CEI_DateForDuty = new ZDate(2020, 01, 01);
		entryInstruction.CEI_Procedure = "40";
		entryInstruction.CEI_SubStyle = "A";

		invoice.JZ_IncoTerm = "DAF";
		invoice.ZG_AgreedPlaceCode = "1";
		invoice.JZ_IncoTermPlace = "PLACE";
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		invoice.JZ_InvoiceAmount = 12500.12m;

		invoiceLine.JI_Tariff = "9301200000";
		invoiceLine.JI_PrimaryPreference = "100";
		invoiceLine.ZG_StatisticalValue = 1200m;
		invoiceLine.JI_ConcessionOrder = "123456";
		invoiceLine.JI_LinePrice = 1009m;
		invoiceLine.JI_Procedure = "4000";
		invoiceLine.JI_CustomsQuantity = 100m;
		invoiceLine.JI_CustomsUnitQty = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		var additionalCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		additionalCode.CY_Code = "S001";
		invoiceLine.JI_CountryOfOrigin = "US";
		var invoiceLinePreviousDocument = invoice.PreviousDocuments.AddNew();
		invoiceLinePreviousDocument.CSI_Procedure = "2";
		jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
	}

	void SetUpRefData()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(company.Country.Code, "A", "71", "00", "", "into warehouse procedure", "IMP", group: "IFD", intoWarehouse: true);
		helper.CreateRefCusProcedure(company.Country.Code, "A", "40", "00", "", "non into warehouse procedure", "IMP", group: "IFD", intoWarehouse: false);
	}

	protected abstract ZString MessageType { get; }

	protected JobDeclaration declaration;
	protected CusEntryInstruction entryInstruction;
	protected JobComInvoiceHeader invoice;
	protected JobComInvoiceLine invoiceLine;
	protected JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent;
}
