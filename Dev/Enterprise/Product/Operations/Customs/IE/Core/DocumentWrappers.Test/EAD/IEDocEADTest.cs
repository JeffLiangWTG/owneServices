using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.DocumentWrappers.Testing
{
	[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Ireland)]
	class IEDocEADTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHTest
	{
		public void TestBox29ExitOfficeLabel()
		{
			AssertEquals("Office of Exit", Wrapper.Box29ExitOfficeLabel);
		}

		public override void TestEadBarcode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("21IT1S31T0003580T");
			var wrapper = IEDocEAD.New(entryHeader, Factory);
			AssertEquals(ExpectedEadBarCode, wrapper.EadBarcode);
		}

		public void TestBox7ReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_HouseBill = "HBL123321";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "LRN2401000";
			Factory.Save();
			var wrapper = IEDocEAD.New(entryHeader, Factory);

			AssertEquals("LRN2401000; HBL123321", wrapper.Box7ReferenceNumber);

			declaration.JE_OwnerRef = "B0001000";
			AssertEquals("B0001000; LRN2401000; HBL123321", wrapper.Box7ReferenceNumber);

			declaration.JE_HouseBill = "";
			AssertEquals("B0001000; LRN2401000", wrapper.Box7ReferenceNumber);
		}

		public void TestBox14Declarant()
		{
			var declarantOrgHeader = Factory.New<OrgHeader>();
			declarantOrgHeader.OH_FullName = "My Test org for Box14";
			var declarantAddress = declarantOrgHeader.Addresses.AddNew();
			declarantAddress.OA_Address1 = "Address Line from Address against Org for Box14!";
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_DeclarantType = Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			var wrapper = IEDocEAD.New(entryHeader, Factory);
			AssertEquals("Precondition: ShowDeclarantRepresentativeAddress is true", true, wrapper.ShowDeclarantRepresentativeAddress);
			AssertEquals("When EORI is empty", "MY TEST ORG FOR BOX14\nADDRESS LINE FROM ADDRESS AGAINST ORG FOR BOX14!\nIRELAND", wrapper.Box14Declarant);

			var cusCode = declarantOrgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_CustomsRegNo = "EORI001";
			AssertEquals("When EORI has value", "IEEORI001\r\nMY TEST ORG FOR BOX14\nADDRESS LINE FROM ADDRESS AGAINST ORG FOR BOX14!\nIRELAND", wrapper.Box14Declarant);

			declaration.JE_DeclarantType = Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._1Self;
			CombineAssertions("When ShowDeclarantRepresentativeAddress is false", () =>
			{
				AssertEquals("ShowDeclarantRepresentativeAddress", false, wrapper.ShowDeclarantRepresentativeAddress);
				AssertEquals("Box14Declarant", ZString.Empty, wrapper.Box14Declarant);
			});
		}

		public void TestBox14Representative()
		{
			var representativeOrgHeader = Factory.New<OrgHeader>();
			representativeOrgHeader.OH_FullName = "My Test org for Box14";
			var representativeAddress = representativeOrgHeader.Addresses.AddNew();
			representativeAddress.OA_Address1 = "Address Line from Address against Org for Box14!";
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_DeclarantType = Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_Representative = representativeAddress.PK;
			var wrapper = IEDocEAD.New(entryHeader, Factory);
			AssertEquals("Precondition: ShowDeclarantRepresentativeAddress is true", true, wrapper.ShowDeclarantRepresentativeAddress);
			AssertEquals("When EORI is empty", "MY TEST ORG FOR BOX14\nADDRESS LINE FROM ADDRESS AGAINST ORG FOR BOX14!\nIRELAND", wrapper.Box14Representative);

			var cusCode = representativeOrgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_CustomsRegNo = "EORI001";
			AssertEquals("When EORI has value", "IEEORI001\r\nMY TEST ORG FOR BOX14\nADDRESS LINE FROM ADDRESS AGAINST ORG FOR BOX14!\nIRELAND", wrapper.Box14Representative);

			declaration.JE_DeclarantType = Enterprise.Customs.EU.Business.RepresentationTypeList.Codes._1Self;
			CombineAssertions("When ShowDeclarantRepresentativeAddress is false", () =>
			{
				AssertEquals("ShowDeclarantRepresentativeAddress", false, wrapper.ShowDeclarantRepresentativeAddress);
				AssertEquals("Box14Representative", ZString.Empty, wrapper.Box14Representative);
			});
		}

		public void TestBox29ExportOffice()
		{
			var wrapper = Wrapper;
			wrapper.Declaration.JE_CustomsOffice = "IE000001";
			AssertEquals("IE000001", wrapper.Box29ExportOffice);
		}

		public override void TestBox18IdentityOfTransportAtDeparture()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = Enterprise.DocumentWrappers.Customs.EU.DocSADH.New(entryHeader, Factory);

			declaration.JE_TransportIDInland = "RAIL 1234";
			AssertEquals("Box18IdentityOfTransportAtDeparture", "RAIL 1234", wrapper.Box18IdentityOfTransportAtDeparture);
		}

		protected new IEDocEAD Wrapper => (IEDocEAD)base.Wrapper;

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return IEDocEAD.New(entryHeader, Factory);
		}

		protected override ZString CountrySpecificCurrency => Core.Constants.CurrencyCodes.EuropeanUnion;

		protected override ZDateTime ExpectedDOE => new ZDateTime(2008, 7, 1);
	}
}
