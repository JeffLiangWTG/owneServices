using System.Collections.Generic;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	sealed class NctsIE29DocumentWrapperTests : TestCaseWithFactory
	{
		public void TestFallbackInformation()
		{
			SetUpData();
			var wrapper = NctsIE29DocumentWrapperWithSecurityForTest(Factory);
			AssertEquals(string.Empty, wrapper.FALLBACKINFORMATION);
		}

		public void TestNctsIE29DocumentWrapper()
		{
			SetUpData();
			var wrapper = NctsIE29DocumentWrapperWithoutSecurityForTest(Factory);
			AssertNotNull(wrapper);
			AssertType(typeof(NctsIE29DocumentWrapper), wrapper);
			AssertCommonWrapperProperties(wrapper);
			AssertEquals("Seal001-Seal001", wrapper.BOXDSEALSIDENTITY);
			AssertEquals(string.Empty, wrapper.Lines[0].BOX314SENSITIVE);
			AssertEquals(string.Empty, wrapper.Lines[0].BOX315SENSITIVEQTY);
		}

		public void TestNctsIE29DocumentWrapperWithSecurity()
		{
			SetUpData();
			var wrapper = NctsIE29DocumentWrapperWithSecurityForTest(Factory);
			AssertNotNull(wrapper);
			AssertType(typeof(SecurityNctsIE29DocumentWrapper), wrapper);

			AssertCommonWrapperProperties(wrapper);

			var securityWrapper = wrapper as SecurityNctsIE29DocumentWrapper;
			CombineAssertions(() =>
			{
				AssertEquals("C", securityWrapper.BOXS32OTHERSCI);
				AssertEquals("---", securityWrapper.BOXS12FIRSTARRIVALTIME);
				AssertEquals("Y", securityWrapper.BOXS29TRANSPORTCHARGESMOP);
				AssertEquals("BH_JOBREFERENCE", securityWrapper.BOX7REFERENCENUMBERS);
				AssertEquals(string.Empty, securityWrapper.BOX7UCR);
				AssertEquals("EU_EXIT", securityWrapper.BOX21BORDERTRANSPORTID);
				AssertEquals("GB", securityWrapper.BOX21BORDERTRANSPORTFLAG);
				AssertEquals("3", securityWrapper.BOX25BORDERTRANSPORTMODE);
				AssertEquals("default default", securityWrapper.BOX30LOCATIONOFGOODS);
				AssertEquals("MODENA", securityWrapper.BOXS18PLACEOFUNLOADING);
				AssertEquals("DOVER", securityWrapper.BOXS17PLACEOFLOADING);
				AssertEquals("MONOPOLI007", securityWrapper.BOXS10CONVEYANCE);
				AssertEquals("GB BE LU DE AT IT", securityWrapper.BOXS13ROUTING);
				AssertEquals("S.CONSIGNEE\n23 LE DON STR\nCORLEONE\n123-456 IT", securityWrapper.BOXS6SECURITYCONSIGNEE);
				AssertEquals("IT27THEBOSS42", securityWrapper.BOXS6SECURITYCONSIGNEEEORI);
				AssertEquals("S.CONSIGNOR\n13TH FLOOR, ALEX HOUSE, VICTORIA AV\nSOUTHEND-ON-PEA, ESSEX\nSS77 1AA GB", securityWrapper.BOXS4SECURITYCONSIGNOR);
				AssertEquals("GB954131533222", securityWrapper.BOXS4SECURITYCONSIGNOREORI);
				AssertEquals("CARRIER\n23 LE DON STR\nCORLEONE\n123-456 IT", securityWrapper.BOXS7CARRIER);
				AssertEquals("IT27THEBOSS00", securityWrapper.BOXS7CARRIEREORI);
				AssertEquals(string.Empty, securityWrapper.PRESENTATIONOFGOODSDATETIME);
				AssertEquals(string.Empty, securityWrapper.BOX44AUTHORISATIONS);
				AssertEquals(string.Empty, securityWrapper.BOX50REPRESENTATIVE);
				AssertEquals(string.Empty, securityWrapper.BOX50SIGNATURE);
				AssertEquals(string.Empty, securityWrapper.BOXDSIGNATURE);
				AssertEquals(string.Empty, wrapper.Lines[1].BOX314SENSITIVE);
				AssertEquals(true, securityWrapper.BOXS00SECURITY);
				AssertEquals("Seal001-Seal001", securityWrapper.BOXS28SEALSNUMBER);

				var nctsHeader = ((NctsEdiMessage)securityWrapper.WrappedObject).Header;
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("Seal001-Seal001", wrapper.BOXDSEALSIDENTITY);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals(string.Empty, wrapper.BOXDSEALSIDENTITY);
			});
		}

		public void TestNctsPhase5IE29DocumentWrapperWithSecurityBoxes()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType("D");
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			_ = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var ediMessage = nctsHeader.Messages.AddNew();
			var nctsEdiMessage = Factory.Load<NctsEdiMessage>(ediMessage.PK);

			var responseData = new NctsIE29CusdecResponseData(Factory);
			responseData.ReferenceNumberUCR = "AR1";
			responseData.PresentationOfTheGoodsDateAndTime = new ZDateTime(2023, 08, 03, 05, 30, 00);
			responseData.Authorisations = new List<AuthorisationResponseData>
			{
				new AuthorisationResponseData() { SequenceNumber = "1", ReferenceNumber = "AUTHREFNUM1", Type = "A1" },
				new AuthorisationResponseData() { SequenceNumber = "2", ReferenceNumber = "AUTHREFNUM2", Type = "A2" },
			};
			responseData.Representative = new RepresentativeResponseData()
			{
				IdentificationNumber = "1",
				Status = "MVP"
			};

			var wrapper = new SecurityNctsIE29DocumentWrapper(nctsEdiMessage, Factory, responseData);
			CombineAssertions(() =>
			{
				AssertEquals("AR1", wrapper.BOX7UCR);
				AssertEquals("2023-08-03 05:30:00", wrapper.PRESENTATIONOFGOODSDATETIME);
				AssertEquals("1 - A1 - AUTHREFNUM1\r\n2 - A2 - AUTHREFNUM2", wrapper.BOX44AUTHORISATIONS);
				AssertEquals("1 - MVP", wrapper.BOX50REPRESENTATIVE);
			});
		}

		void AssertCommonWrapperProperties(INctsIE29DocumentWrapper wrapper)
		{
			CombineAssertions(() =>
			{
				AssertEquals("18GB00400000000273", wrapper.MOVEMENTREFERENCENUMBER);
				AssertEquals("18GB00400000000273", wrapper.EMAILSUBJECT);
				AssertEquals("QUIRM ENGINEERING\n125 Psuedopolis Yard\nAnk-Morpork\nSS99 1AA GB", wrapper.BOX2CONSIGNOR);
				AssertEquals("GB602070107000", wrapper.BOX2CONSIGNOREORI);
				AssertEquals("GB000060 (GB - Dover/Folkestone Eurotunnel Freight (Lord Warden Square,  Western Docks))", wrapper.BOXCOFFICEOFDEPARTURE);
				AssertEquals(string.Empty, wrapper.BOXCUNIQUEREFERENCENUMBER);
				AssertEquals(string.Empty, wrapper.RETURNOFFICEADDRESS);
				AssertEquals("IT021100", wrapper.BOX53OFFICEOFDESTINATION);
				AssertEquals("1", wrapper.BOX5ITEMS);
				AssertEquals("1", wrapper.BOX6PACKAGES);
				AssertEquals("DROFL POTTERY\n125 Psuedopolis Yard\nAnk-Morpork\nSS99 1AA GB", wrapper.BOX8CONSIGNEE);
				AssertEquals("GB658120050000", wrapper.BOX8CONSIGNEEEORI);
				AssertEquals("EU_EXIT", wrapper.BOX18DEPARTURETRANSPORTID);
				AssertEquals(string.Empty, wrapper.BOX18DEPARTURETRANSPORTFLAG);
				AssertEquals("1000", wrapper.BOX35GROSSMASS);
				AssertEquals("CITY WATCH SHIPPING\n125 Psuedopolis Yard\nAnk-Morpork\nSS99 1AA GB", wrapper.BOX50PRINCIPAL);
				AssertEquals("GB652420267000", wrapper.BOX50PRINCIPALEORI);
				AssertEquals("GB001260", wrapper.BOX51TRANSITOFFICE1);
				AssertEquals(string.Empty, wrapper.BOX51TRANSITOFFICE2);
				AssertEquals(string.Empty, wrapper.BOX51TRANSITOFFICE3);
				AssertEquals(string.Empty, wrapper.BOX51TRANSITOFFICE4);
				AssertEquals(string.Empty, wrapper.BOX51TRANSITOFFICE5);
				AssertEquals(string.Empty, wrapper.BOX51TRANSITOFFICE6);
				AssertEquals(string.Empty, wrapper.BOX50SIGNATURE);
				AssertEquals(string.Empty, wrapper.BOXDSIGNATURE);
				AssertEquals("default", wrapper.BOX52GUARANTEE);
				AssertEquals(string.Empty, wrapper.BOX52GUARANTEEVALIDITY);
				AssertEquals("3", wrapper.BOX52GUARANTEECODE);
				AssertEquals("1", wrapper.BOXDSEALSAFFIXEDNUMBER);
				AssertContains($"WiseTechGlobal.com - {BrandingFactory.Instance.ProductName} v", wrapper.EDIENTERPRISEVERSION);

				AssertEquals("---", wrapper.Lines[0].BOX2CONSIGNOR);
				AssertEquals("---", wrapper.Lines[0].BOX8CONSIGNEE);
				AssertEquals("T1", wrapper.Lines[0].BOX1REGIME);
				AssertEquals("AU", wrapper.Lines[0].BOX15COUNTRYOFORIGIN);
				AssertEquals("GB", wrapper.Lines[0].BOX17COUNTRYOFDESTINATION);
				AssertEquals("1", wrapper.Lines[0].BOX32ITEM);
				AssertEquals("400200500", wrapper.Lines[0].BOX33COMMODITY);
				AssertEquals("1000", wrapper.Lines[0].BOX35GROSSMASS);
				AssertEquals("T2 - EU_EXIT-T2 - default", wrapper.Lines[0].BOX40DOCUMENTS);
				AssertEquals("SM1; 714-RX4-RX4 Details", wrapper.Lines[0].BOX44);
				AssertEquals("SM1; 714-RX4-RX4 Details", wrapper.Lines[0].BOX441DOCSANDCERTS);
				AssertEquals("Bloomingales", wrapper.Lines[0].BOX311MARKS);
				AssertEquals("1 - Box", wrapper.Lines[0].BOX312NUMBERS);
				AssertEquals(string.Empty, wrapper.Lines[0].BOX313CONTAINERS);
				AssertEquals("FLOWERS", wrapper.Lines[0].BOX314DESCRIPTION);
				AssertEquals("BH_JOBREFERENCE", wrapper.LOCALREFERENCENUMBER);
			});
		}

		public void TestNOTRELEASEDWATERMARK()
		{
			var wrapper = NctsIE29DocumentWrapperWithSecurityForTest(Factory);
			AssertEquals("NOTRELEASEDWATERMARK must be always Empty", string.Empty, wrapper.NOTRELEASEDWATERMARK);
		}

		public void TestBOXDCLEARANCE()
		{
			var wrapper = NctsIE29DocumentWrapperWithSecurityForTest(Factory);
			AssertEquals("BOXDCLEARANCE must be always Empty", string.Empty, wrapper.BOXDCLEARANCE);
		}

		void SetUpData()
		{
			CreateZZRefTestValuesIfNeeded("GB000060", "GB - Dover/Folkestone Eurotunnel Freight (Lord Warden Square,  Western Docks)", Core.Constants.CountryCodes.UnitedKingdom, new ZString[] { "DES", "DEP", "EXP" });
			CreateZZRefTestValuesIfNeeded("GB000074", "GB - Immingham (Custom House, Immingham Dock)", Core.Constants.CountryCodes.UnitedKingdom, new ZString[] { "DES", "ENT", "EXP", "EXT" });

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var nctsCodeType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS;
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			_ = helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			_ = helper.CreateNewOrGetExistingCusCodeType(nctsCodeType, "Supporting Document Of NCTS");
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, nctsCodeType, "380", "Commercial invoice", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BX", "Box", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "Bag", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"CT", "Carton", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"LT", "Lot", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"PK", "Package", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			var previoucDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;
			_ = helper.CreateNewOrGetExistingCusCodeType(previoucDocumentType, "Previous Documents Transit NCTS (BOX40)");
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, previoucDocumentType, "ZZZ", "Other", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			Factory.Save();
		}

		void CreateZZRefTestValuesIfNeeded(ZString officeCode, ZString description, ZString dataGroupingCode, ZString[] officePurpose)
		{
			var factory = new BusinessObjectFactory();
			var helper = new Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
			_ = helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, $"Data Grouping Code {dataGroupingCode}");
			_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			_ = helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, description, officePurpose);
			_ = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			factory.Save();
		}

		internal static INctsIE29DocumentWrapper NctsIE29DocumentWrapperWithoutSecurityForTest(BusinessObjectFactory factory)
		{
			var inboundMessage = NctsEdiMessageDocumentSupporterTest.NctsIE29EdiMessage(factory, withSecurity: false);
			return NctsIE29DocumentWrapper.New(inboundMessage);
		}

		INctsIE29DocumentWrapper NctsIE29DocumentWrapperWithSecurityForTest(BusinessObjectFactory factory)
		{
			var inboundMessage = NctsEdiMessageDocumentSupporterTest.NctsIE29EdiMessage(factory, withSecurity: true);
			return NctsIE29DocumentWrapper.New(inboundMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			oldCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
		}
		ZString oldCountryCode;

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(oldCountryCode);
		}
	}
}
