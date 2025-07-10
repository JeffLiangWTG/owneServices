using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AddInfoJobDeclarationAuthorisationNumberListTest : AddInfoJobDeclarationLookupsTest
{
	public void TestAuthorisationNumberListBasedOnMessageType()
	{
		var supplier = SetupSupplier();
		var importer = SetupImporter();
		var declarant = SetupDeclarant();
		var carrier = SetupCarrier();
		var forwarder = SetupForwarder();
		var cto = SetupCto();
		var depot = SetupDepot();
		var containerYard = SetupContainerYard();
		var controllingAgent = SetupControllingAgent();
		var controllingCustomer = SetupControllingCustomer();
		var externalBroker = SetupExternalBroker();
		var representative = SetupRepresentative();
		var seller = SetupSeller();
		var manufacter = SetupManufacter();

		Declaration.JE_OH_Supplier = supplier.PK;
		Declaration.JE_OH_Importer = importer.PK;
		Declaration.JE_OA_DeclarantAddress = declarant.Addresses.AddNew().PK;
		Declaration.JE_OH_ShippingLine = carrier.PK;
		Declaration.JE_OH_Forwarder = forwarder.PK;
		Declaration.ContainerTerminalOperatorDocAddress.OrganisationPK = cto.PK;
		Declaration.DepotDocAddress.OrganisationPK = depot.PK;
		Declaration.ContainerYardDocAddress.OrganisationPK = containerYard.PK;
		Declaration.JE_OH_ControllingAgent = controllingAgent.PK;
		Declaration.JE_OH_ControllingCustomer = controllingCustomer.PK;
		Declaration.JE_OH_ExternalBroker = externalBroker.PK;
		Declaration.JE_OA_Representative = representative.MainAddress.PK;
		Declaration.JE_OA_SellerAddress = seller.MainAddress.PK;
		Declaration.JE_OA_ManufacturerAddress = manufacter.MainAddress.PK;

		Declaration.JE_MessageType = ZString.Empty;
		AssertLookup("MessageType = empty", Declaration.AddInfoLookups.AuthorisationNumberList, 0, new Dictionary<ZString, ZString>());

		AssertAuthorisationNumberListWhenImport();
		AssertAuthorisationNumberListWhenExport();
		AssertAuthorisationNumberListWhenMiscellaneousCustoms();

		AssertAuthorisationNumberListCached(importer);
	}

	#region Implementation

	void AssertAuthorisationNumberListCached(OrgHeader importer)
	{
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: importer.PK, "NEW AUTH", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertLookup("MessageType = IMP. Result has been cached and new authorisations are not loaded", Declaration.AddInfoLookups.AuthorisationNumberList, ExpectedImpAuth.Count, ExpectedImpAuth);
	}

	void AssertAuthorisationNumberListWhenMiscellaneousCustoms()
	{
		Declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertLookup("MessageType = MSC", Declaration.AddInfoLookups.AuthorisationNumberList, 0, new Dictionary<ZString, ZString>());
	}

	void AssertAuthorisationNumberListWhenExport()
	{
		Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertLookup("MessageType = EXP", Declaration.AddInfoLookups.AuthorisationNumberList, ExpectedExpAndComAuth.Count, ExpectedExpAndComAuth);
	}

	void AssertAuthorisationNumberListWhenImport()
	{
		Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertLookup("MessageType = IMP", Declaration.AddInfoLookups.AuthorisationNumberList, ExpectedImpAuth.Count, ExpectedImpAuth);
	}

	Dictionary<ZString, ZString> ExpectedImpAuth => new Dictionary<ZString, ZString>()
		{
			{ "IMP-ALI-123", "IMPORTER" },
			{ "IMP-CWP-123", "IMPORTER" },
			{ "IMP-CW1-123", "IMPORTER" },
			{ "IMP-CW2-123", "IMPORTER" },
			{ "DEC-ALI-123", "DECLARANT" },
			{ "CAR-ALI-123", "CARRIER" },
			{ "FOR-ALI-123", "FORWARDER" },
			{ "CTO-ALI-123", "CTO" },
			{ "DEP-ALI-123", "DEPOT" },
			{ "CYD-ALI-123", "CNT-YARD" },
			{ "CAG-ALI-123", "CTRL-AGENT" },
			{ "CCU-ALI-123", "CTRL-CSTMR" },
			{ "BRK-ALI-123", "EXTRNL-BRKR" },
			{ "RPS-ALI-123", "RPRSNTTV" },
			{ "SEL-ALI-123", "SELLER" },
			{ "MNF-ALI-123", "MNFCTR" },
			{ "DEC-ALE-123", "DECLARANT" },
			{ "CAR-ALE-123", "CARRIER" },
			{ "FOR-ALE-123", "FORWARDER" },
			{ "CTO-ALE-123", "CTO" },
			{ "DEP-ALE-123", "DEPOT" },
			{ "CYD-ALE-123", "CNT-YARD" },
			{ "CAG-ALE-123", "CTRL-AGENT" },
			{ "CCU-ALE-123", "CTRL-CSTMR" },
			{ "BRK-ALE-123", "EXTRNL-BRKR" },
			{ "RPS-ALE-123", "RPRSNTTV" },
			{ "SEL-ALE-123", "SELLER" },
			{ "MNF-ALE-123", "MNFCTR" }
		};

	Dictionary<ZString, ZString> ExpectedExpAndComAuth => new Dictionary<ZString, ZString>()
		{
			{ "SUP-ALE-123", "SUPPLIER" },
			{ "DEC-ALE-123", "DECLARANT" },
			{ "CAR-ALE-123", "CARRIER" },
			{ "FOR-ALE-123", "FORWARDER" },
			{ "CTO-ALE-123", "CTO" },
			{ "DEP-ALE-123", "DEPOT" },
			{ "CYD-ALE-123", "CNT-YARD" },
			{ "CAG-ALE-123", "CTRL-AGENT" },
			{ "CCU-ALE-123", "CTRL-CSTMR" },
			{ "BRK-ALE-123", "EXTRNL-BRKR" },
			{ "RPS-ALE-123", "RPRSNTTV" },
			{ "SEL-ALE-123", "SELLER" },
			{ "MNF-ALE-123", "MNFCTR" }
		};

	#region Setup Organizations

	OrgHeader SetupManufacter() => SetupOrganization("MNFCTR", "MNF");

	OrgHeader SetupSeller() => SetupOrganization("SELLER", "SEL");

	OrgHeader SetupRepresentative() => SetupOrganization("RPRSNTTV", "RPS");

	OrgHeader SetupExternalBroker() => SetupOrganization("EXTRNL-BRKR", "BRK");

	OrgHeader SetupControllingCustomer() => SetupOrganization("CTRL-CSTMR", "CCU");

	OrgHeader SetupControllingAgent() => SetupOrganization("CTRL-AGENT", "CAG");

	OrgHeader SetupContainerYard() => SetupOrganization("CNT-YARD", "CYD");

	OrgHeader SetupDepot() => SetupOrganization("DEPOT", "DEP");

	OrgHeader SetupCto() => SetupOrganization("CTO", "CTO");

	OrgHeader SetupForwarder() => SetupOrganization("FORWARDER", "FOR");

	OrgHeader SetupCarrier() => SetupOrganization("CARRIER", "CAR");

	OrgHeader SetupDeclarant()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "DECLARANT";
		AddNewAuthHeader(declarant, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, "DEC-ALE-123");
		AddNewAuthHeader(declarant, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, "EXP-DEC-ALE", isExpired: true);
		AddNewAuthHeader(declarant, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, "DEC-ALI-123");
		AddNewAuthHeader(declarant, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, "EXP-DEC-ALI", isExpired: true);
		AddNewAuthHeader(declarant, "UND", "UND-IMP");
		return declarant;
	}

	OrgHeader SetupImporter()
	{
		var importer = Factory.New<OrgHeader>();
		importer.OH_Code = "IMPORTER";

		AddNewAuthHeader(importer, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, "IMP-ALI-123");
		AddNewAuthHeader(importer, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, "EXP-IMP-ALI", isExpired: true);
		AddNewAuthHeader(importer, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "IMP-CWP-123");
		AddNewAuthHeader(importer, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "IMP-CW1-123");
		AddNewAuthHeader(importer, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "IMP-CW2-123");

		return importer;
	}

	OrgHeader SetupSupplier()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.OH_Code = "SUPPLIER";
		AddNewAuthHeader(supplier, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, "SUP-ALE-123");
		AddNewAuthHeader(supplier, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, "EXP-SUP-ALE", isExpired: true);
		AddNewAuthHeader(supplier, "UND", "UND-SUP");
		return supplier;
	}

	#endregion

	OrgHeader SetupOrganization(ZString code, ZString shortCode)
	{
		var organization = Factory.New<OrgHeader>();
		organization.OH_Code = code;
		AddNewAuthHeader(organization, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, $"{shortCode}-{CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport}-123");
		AddNewAuthHeader(organization, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, $"{shortCode}-{CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport}-123");
		return organization;
	}

	void AddNewAuthHeader(OrgHeader orgHeader, ZString type, ZString number, bool isExpired = false)
	{
		var endDate = isExpired ? ZDate.Today.AddDays(-5) : ZDate.Today.AddDays(10);
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type, permitHolder: orgHeader.PK, number, startDate: ZDate.Today.AddDays(-10), endDate);
	}

	void AssertLookup(ZString combineAssertionsMessage, CodeDescriptionPairList lookup, int count, Dictionary<ZString, ZString> expectedCodeDescriptionPairList)
	{
		CombineAssertions(combineAssertionsMessage, () =>
		{
			AssertEquals("Lookup count", count, lookup.Count);
			foreach (var codeDescriptionPair in expectedCodeDescriptionPairList)
			{
				AssertEquals(codeDescriptionPair.Value, lookup.GetDescriptionFromCode(codeDescriptionPair.Key));
			}
		});
	}

	#endregion
}
