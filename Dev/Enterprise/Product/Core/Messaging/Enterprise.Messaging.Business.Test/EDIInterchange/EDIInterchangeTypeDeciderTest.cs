using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class EDIInterchangeTypeDeciderTest : TestCaseWithFactory
	{
		public void TestInterchange()
		{
			AssertType(EDIInterchange.ApplicationCodes.CMR, "", ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICMRInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.NEXDOCS, "", ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICMRInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.COLS, "", ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICOLSInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.EXDOC, "", ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IEXDOCInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.Traxon, "", ObjectFactory.GetType<Enterprise.Integration.Customs.HK.ITraxonInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.CAIMP, "", ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.CAEXP, "", ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.CAACI, "", ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.NewZealandCustoms, "", ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.INZCustomsInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.NewZealandMAFeBACCa, "", ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.INZEBACCAInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.USCustomsDIS, "", ObjectFactory.GetType<Enterprise.Integration.Customs.US.DIS.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.CIM, "", ObjectFactory.GetType<Freight.Integration.Forwarding.ICIMEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.SouthAfricanCustoms, "", ObjectFactory.GetType("ZA.IZACustomsInterchange"));
			AssertType(EDIInterchange.ApplicationCodes.ITCustoms, "", ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.BRCustoms, "", ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.ESCustoms, "", ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.TRCustoms, "", ObjectFactory.GetType<Enterprise.Integration.Customs.TR.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.CNCustomsSingleWindow, "", ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.BECustoms, "", ObjectFactory.GetType<Enterprise.Integration.Customs.BE.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.CHCustomsEdec, "", ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.CHCustomsPassar, "", ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput, "", ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.KRCustoms, "", ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.JPCustoms, "", ObjectFactory.GetType<Enterprise.Integration.Customs.JP.IEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.NOCustomsEmma, "", ObjectFactory.GetType<Enterprise.Integration.Customs.NO.IEmmaEDIInterchange>());
			AssertType(EDIInterchange.ApplicationCodes.ILCustoms, "", ObjectFactory.GetType<Enterprise.Integration.Customs.IL.IEDIInterchange>());
		}

		public void TestCAUDMInterchange()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_GB = branch.PK;
			interchange.EI_To = "CACustoms";

			var message = interchange.ContainedMessages.AddNew();
			message.FillWithValidTestData();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_MessageType = "IID";

			Factory.Save();

			var row = ((IBusinessObjectInternals)interchange).Row;

			var typeDecider = ObjectFactory.Get<Enterprise.Integration.Customs.CA.IUDMInterchangeTypeDecider>();
			var expectedType = typeDecider.GetTypeForLoad(row, Factory);

			var interchangeInNewFactory = NewFactory().Load<EDIInterchange>(interchange.PK);
			var actualType = interchangeInNewFactory.GetType();

			AssertEquals("Precondition", "CAUniversalXMLInterchange", expectedType.Name);
			AssertEquals("Should returns the expected type in a new factory.", expectedType, actualType);

			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			Factory.Save();

			actualType = new EDIInterchangeTypeDecider().GetTypeForApplicationCode(EDIInterchange.ApplicationCodes.UniversalDataMessaging, row, NewFactory());
			AssertEquals("Should get the default type as the interchange is not Transmit.", typeof(EDIInterchange), actualType);

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			Factory.Save();

			actualType = new EDIInterchangeTypeDecider().GetTypeForApplicationCode(EDIInterchange.ApplicationCodes.UniversalDataMessaging, row, NewFactory());
			AssertEquals("Should get the default type as the interchange country is not Canada.", typeof(EDIInterchange), actualType);
		}

		public void TestUSCustomsImportInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.USCustomsImport;
			var row = ((IBusinessObjectInternals)interchange).Row;
			AssertEquals(((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.US.ICBPEDIInterchangeTypeDecider>()).GetTypeForLoad(row, Factory), new EDIInterchangeTypeDecider().GetTypeForApplicationCode(EDIInterchange.ApplicationCodes.USCustomsImport, row, Factory));
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.USCustomsExport;
			AssertEquals(((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.US.ICBPEDIInterchangeTypeDecider>()).GetTypeForLoad(row, Factory), new EDIInterchangeTypeDecider().GetTypeForApplicationCode(EDIInterchange.ApplicationCodes.USCustomsExport, row, Factory));
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.AMS;
			AssertEquals(((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.US.ICBPEDIInterchangeTypeDecider>()).GetTypeForLoad(row, Factory), new EDIInterchangeTypeDecider().GetTypeForApplicationCode(EDIInterchange.ApplicationCodes.AMS, row, Factory));
		}

		public void TestIECustomsInterchange()
		{
			var row = ((INeedRow)Factory.New<EDIInterchange>()).Row;
			var ieTypeDecider = (TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.IE.IEDIInterchangeTypeDecider>();
			var commonTypeDecider = new EDIInterchangeTypeDecider();

			var applicationCodeCommon = EDIInterchange.ApplicationCodes.IECustomsCommon;
			row[EDIInterchange.Schema.EI_ApplicationCode] = applicationCodeCommon;
			AssertEquals("IEC, Should return type from IE TypeDecider", ieTypeDecider.GetTypeForLoad(row, Factory), commonTypeDecider.GetTypeForApplicationCode(applicationCodeCommon, row, Factory));

			var applicationCodeEMCS = EDIInterchange.ApplicationCodes.IECustomsEMCS;
			row[EDIInterchange.Schema.EI_ApplicationCode] = applicationCodeEMCS;
			AssertEquals("EMCS, Should return type from IE TypeDecider", ieTypeDecider.GetTypeForLoad(row, Factory), commonTypeDecider.GetTypeForApplicationCode(applicationCodeEMCS, row, Factory));

			var applicationCodeAES = EDIInterchange.ApplicationCodes.IECustomsExport;
			row[EDIInterchange.Schema.EI_ApplicationCode] = applicationCodeAES;
			AssertEquals("AES, Should return type from IE TypeDecider", ieTypeDecider.GetTypeForLoad(row, Factory), commonTypeDecider.GetTypeForApplicationCode(applicationCodeAES, row, Factory));

			var applicationCodeAIS = EDIInterchange.ApplicationCodes.IECustomsImport;
			row[EDIInterchange.Schema.EI_ApplicationCode] = applicationCodeAIS;
			AssertEquals("AIS, Should return type from IE TypeDecider", ieTypeDecider.GetTypeForLoad(row, Factory), commonTypeDecider.GetTypeForApplicationCode(applicationCodeAIS, row, Factory));

			var applicationCodeAISUCC5 = EDIInterchange.ApplicationCodes.IECustomsUCC5Import;
			row[EDIInterchange.Schema.EI_ApplicationCode] = applicationCodeAISUCC5;
			AssertEquals("AIS UCC5, Should return type from IE TypeDecider", ieTypeDecider.GetTypeForLoad(row, Factory), commonTypeDecider.GetTypeForApplicationCode(applicationCodeAISUCC5, row, Factory));

			var applicationCodeNCTS = EDIInterchange.ApplicationCodes.IECustomsNCTS;
			row[EDIInterchange.Schema.EI_ApplicationCode] = applicationCodeNCTS;
			AssertEquals("NCTS, Should return type from IE TypeDecider", ieTypeDecider.GetTypeForLoad(row, Factory), commonTypeDecider.GetTypeForApplicationCode(applicationCodeNCTS, row, Factory));

			var applicationCodeReporting = EDIInterchange.ApplicationCodes.IECustomsAndExcise;
			row[EDIInterchange.Schema.EI_ApplicationCode] = applicationCodeReporting;
			AssertEquals("Reporting, Should return type from IE TypeDecider", ieTypeDecider.GetTypeForLoad(row, Factory), commonTypeDecider.GetTypeForApplicationCode(applicationCodeReporting, row, Factory));
		}

		public void TestEBLInterchange()
		{
			AssertEquals("EBL", ApplicationCodeList.Codes.ElectronicBillOfLadingMessaging);
			AssertEquals("Electronic Bill Of Lading Messaging", ApplicationCodeList.Descriptions.ElectronicBillOfLadingMessaging);
			AssertEquals("EBL", EDIInterchange.ApplicationCodes.ElectronicBillOfLadingMessaging);
		}

		void AssertType(string appCode, string interchangeType, Type expectedType)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeType = interchangeType;
			var type = new EDIInterchangeTypeDecider().GetTypeForApplicationCode(appCode, ((IBusinessObjectInternals)interchange).Row, Factory);
			AssertEquals(expectedType, type);
		}
	}
}
