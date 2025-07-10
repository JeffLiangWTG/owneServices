using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationValueSetStrategyTest : EU.Business.Declaration.Testing.JobDeclarationValueSetStrategyTest
{
	public override void TestJE_MessageTypeChanged()
	{
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		var supplierAddInfo = EUOrgImpAddInfo.Get(supplier, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		supplierAddInfo.Deserialise();
		supplierAddInfo.ZO_Box14UseIndirectRepresentationForExporter = true;

		var importer = Factory.NewWithValidTestData<OrgHeader>();
		var importerAddInfo = EUOrgImpAddInfo.Get(importer, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		importerAddInfo.Deserialise();
		importerAddInfo.ZO_Box14UseIndirectRepresentation = true;
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Supplier = supplier.PK;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("", declaration.JE_DeclarantType);

		declaration.JE_DeclarantType = "";
		declaration.JE_OH_Supplier = ZGuid.Empty;
		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("", declaration.JE_DeclarantType);
	}

	public override void TestDefaultSupplierChanged()
	{
		var supplier = Factory.NewWithValidTestData<OrgHeader>();

		var declaration = Factory.New<JobDeclaration>();

		var addInfo = EUOrgImpAddInfo.Get(supplier, Core.Constants.CountryCodes.Latvia);
		addInfo.Deserialise();
		addInfo.ZO_Box14UseIndirectRepresentationForExporter = true;
		Factory.Save();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_OH_Supplier = supplier.PK;
		AssertEquals("", declaration.JE_DeclarantType);

		declaration.JE_DeclarantType = "";
		addInfo.ZO_Box14UseIndirectRepresentationForExporter = false;
		Factory.Save();
		declaration.JE_OH_Supplier = ZGuid.Empty;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_OH_Supplier = supplier.PK;
		AssertEquals("", declaration.JE_DeclarantType);

		addInfo.ZO_Box14UseIndirectRepresentationForExporter = true;
		Factory.Save();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_OH_Supplier = ZGuid.Empty;
		declaration.JE_OH_Supplier = supplier.PK;
		AssertEquals("", declaration.JE_DeclarantType);
	}

	public void TestDefaultGoodsLocationOfItemsAsCustomsOffice()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_CustomsOffice = "IT017100";

		declaration.JE_LocationQualifier = "FC";
		AssertEquals("When qualifier is not D, goods location is not defaulted to office of presentation", ZString.Empty, declaration.JE_LocationOfGoods);

		declaration.JE_LocationQualifier = "D";
		AssertEquals("When qualifier is D, goods location is defaulted to office of presentation", "IT017100", declaration.JE_LocationOfGoods);
	}

	public void TestGoodsLocationFieldsAreEmptiedOnAuthorisationNumberChange()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_LocationQualifier = "AA";
		declaration.JE_SubLocationOfGoods = "IT";
		declaration.GoodsLocationAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
		declaration.JE_LocationOfGoods = "BB";
		declaration.JE_LocationOtherInformation = "OTH";
		declaration.ZG_AuthorisationNumber = "ARG1234";

		declaration.ZG_AuthorisationNumber = ZString.Empty;

		CombineAssertions("All Goods location fields must be empty", () =>
		{
			AssertEquals(ZString.Empty, declaration.JE_LocationQualifier);
			AssertEquals(ZString.Empty, declaration.JE_SubLocationOfGoods);
			AssertEquals(ZGuid.Empty, declaration.GoodsLocationAddress.OrganisationPK);
			AssertEquals(ZString.Empty, declaration.JE_LocationOfGoods);
			AssertEquals(ZString.Empty, declaration.JE_LocationOtherInformation);
		});
	}

	public void TestAddAeoCertificateToSupportingDocuments_WhenMessageTypeChanges()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO001");

		var importer = Factory.New<OrgHeader>();
		importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO002");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Supplier = supplier.PK;
		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_MessageType = ZString.Empty;
		var invoice = declaration.Invoices.AddNew();
		invoice.SupportingDocuments.RemoveAndDeleteAll();

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		Assert(invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.Y022 && x.CSI_ReferenceNumber == "AEO001"));

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		Assert(invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.Y023 && x.CSI_ReferenceNumber == "AEO002"));
	}

	public void TestAddAeoCertificateToSupportingDocuments_WhenSupplierChanges()
	{
		var supplier1 = Factory.New<OrgHeader>();
		supplier1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO001");

		var supplier2 = Factory.New<OrgHeader>();
		supplier2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO002");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		invoice.SupportingDocuments.RemoveAndDeleteAll();

		declaration.JE_OH_Supplier = supplier1.PK;
		Assert(invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.Y022 && x.CSI_ReferenceNumber == "AEO001"));

		declaration.JE_OH_Supplier = supplier2.PK;
		Assert(invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.Y022 && x.CSI_ReferenceNumber == "AEO002"));
	}

	public void TestAddAeoCertificateToSupportingDocuments_WhenImporterChanges()
	{
		var importer1 = Factory.New<OrgHeader>();
		importer1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO001");

		var importer2 = Factory.New<OrgHeader>();
		importer2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO002");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoice.SupportingDocuments.RemoveAndDeleteAll();

		declaration.JE_OH_Importer = importer1.PK;
		Assert(invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.Y023 && x.CSI_ReferenceNumber == "AEO001"));

		declaration.JE_OH_Importer = importer2.PK;
		Assert(invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.Y023 && x.CSI_ReferenceNumber == "AEO002"));
	}

	public void TestAddAeoCertificateToSupportingDocuments_WhenDeclarantChanges()
	{
		var declarant1 = Factory.New<OrgHeader>();
		declarant1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO001");

		var declarant2 = Factory.New<OrgHeader>();
		declarant2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO002");

		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoice.SupportingDocuments.RemoveAndDeleteAll();

		declaration.JE_OA_DeclarantAddress = declarant1.MainAddress.PK;
		Assert(invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.Y024 && x.CSI_ReferenceNumber == "AEO001"));

		declaration.JE_OA_DeclarantAddress = declarant2.MainAddress.PK;
		Assert(invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.Y024 && x.CSI_ReferenceNumber == "AEO002"));
	}

	public void TestAddAeoCertificateToSupportingDocuments_WhenDeclarantTypeChanges()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO001");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
		var invoice = declaration.Invoices.AddNew();
		invoice.SupportingDocuments.RemoveAndDeleteAll();

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		Assert(invoice.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.Y024 && x.CSI_ReferenceNumber == "AEO001"));
	}

	public void TestZG_CTStatusIDWhenJE_RL_NKOriginChangeEXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.ZG_CTStatusID = "T";

		declaration.JE_RL_NKOrigin = "IT";
		AssertEquals("After setting JE_RL_NKOrigin, ZG_CTStatusID", "T", declaration.ZG_CTStatusID);

		declaration.JE_RL_NKOrigin = "US";
		AssertEquals("After updating JE_RL_NKOrigin, ZG_CTStatusID", "T", declaration.ZG_CTStatusID);

		declaration.JE_RL_NKOrigin = "";
		AssertEquals("After deleting JE_RL_NKOrigin, ZG_CTStatusID", "T", declaration.ZG_CTStatusID);
	}

	public void TestZG_CTStatusIDWhenJE_RL_NKFinalDestinationChangeEXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.ZG_CTStatusID = "T";

		declaration.JE_RL_NKFinalDestination = "IT";
		AssertEquals("After setting JE_RL_NKFinalDestination, ZG_CTStatusID", "T", declaration.ZG_CTStatusID);

		declaration.JE_RL_NKFinalDestination = "US";
		AssertEquals("After updating JE_RL_NKFinalDestination, ZG_CTStatusID", "T", declaration.ZG_CTStatusID);

		declaration.JE_RL_NKFinalDestination = "";
		AssertEquals("After deleting JE_RL_NKFinalDestination, ZG_CTStatusID", "T", declaration.ZG_CTStatusID);
	}
}
