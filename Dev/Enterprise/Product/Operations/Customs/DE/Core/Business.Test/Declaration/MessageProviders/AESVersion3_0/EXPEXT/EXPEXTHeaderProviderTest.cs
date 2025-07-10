using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPEXTHeaderProvider))]
	sealed class EXPEXTHeaderProviderTest : AESHeaderProviderAbstractTest<EXPEXTHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPEXTHeaderProvider(null));
		}

		public void TestExitDate()
		{
			action.ExitDate = ZDate.BrettsBirthday;
			AssertEquals(ZDate.BrettsBirthday, Provider.ExitDate);
		}

		public void TestIntendedExitDate()
		{
			action.ExitDate = ZDate.BrettsBirthday;
			CombineAssertions(() =>
			{
				action.ExitType = ExportExitTypeList.Codes._2;
				AssertEquals("Exit Type 2", ZDate.BrettsBirthday, Provider.IntendedExitDate);
				action.ExitType = ExportExitTypeList.Codes._4;
				AssertNull("Exit Type 4", Provider.IntendedExitDate);
			});
		}

		public void TestAnnotation()
		{
			action.Annotation = ZString.Replicate('L', 512);
			AssertEquals(action.Annotation, Provider.Annotation);
		}

		public void TestExitType()
		{
			action.ExitType = ExportExitTypeList.Codes._2;
			AssertEquals(ExportExitTypeList.Codes._2, Provider.ExitType);
		}

		public void TestActualExitCustomsOffice()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			action.ExitCustomsOffice = "DE003302";
			CombineAssertions(() =>
			{
				action.ExitType = ExportExitTypeList.Codes._2;
				AssertEquals("Exit Type 2", ZString.Empty, Provider.ActualExitCustomsOffice);
				action.ExitType = ExportExitTypeList.Codes._4;
				AssertEquals("Exit Type 4", "DE003302", Provider.ActualExitCustomsOffice);
			});
		}

		public void TestDeclarant()
		{
			using (Factory.SetTemporaryCurrentUser("Sachbearbeiter", "Bob Baumeister", "06131474747", "bob.baumeister@samplefreight.de"))
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var declarant = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				declaration.JE_OA_DeclarantAddress = declarant.PK;
				CombineAssertions(() =>
				{
					AssertEquals("EoriNumber", "GREOR1", Provider.Declarant.EoriNumber);
					AssertEquals("EoriBranchSuffix", "EBS1", Provider.Declarant.EoriBranchSuffix);
					AssertEquals("Contact: Name", "Bob Baumeister", Provider.Declarant.ContactPerson.PersonName);
					AssertEquals("Contact: Phone", "06131474747", Provider.Declarant.ContactPerson.PhoneNumber);
					AssertEquals("Contact: EMail", "bob.baumeister@samplefreight.de", Provider.Declarant.ContactPerson.MailAddress);
				});
			}
		}

		public void TestDeclarant_NoEORI()
		{
			var declarant = GetOrgWithoutEORNumberAndEORIBranch();
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			AssertEquals("Declarant is null", null, Provider.Declarant);
		}

		public void TestExitCarrier_ExitType2()
		{
			declaration.CarrierEUBorderDocAddress.E2_OA_Address = ExitCarrier.PK;
			action.ExitType = ExportExitTypeList.Codes._2;
			AssertNull("ExitCarrier when ExitType='2'", Provider.ExitCarrier);
		}

		public void TestExitCarrier_ExitType4()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			declaration.CarrierEUBorderDocAddress.E2_OA_Address = ExitCarrier.PK;
			action.ExitType = ExportExitTypeList.Codes._4;
			AssertPartyWithoutContactPerson(Provider.ExitCarrier
				, "GREOR1"
				, "EBS1"
				, "MAX MUSTERMANN"
				, "TESTSTRASSE 1"
				, "MAINZ"
				, "55126"
				, Core.Constants.CountryCodes.Germany);
		}

		public void TestAlternativeEvidences_ExitType2()
		{
			PrepareAlternativeEvidences();
			action.ExitType = ExportExitTypeList.Codes._2;
			AssertEquals("AlternativeEvidences not mapped if ExitType='2'", false, Provider.AlternativeEvidences.Any());
		}

		public void TestAlternativeEvidences_ExitType4()
		{
			PrepareAlternativeEvidences();
			action.ExitType = ExportExitTypeList.Codes._4;
			CombineAssertions(() =>
			{
				AssertEquals("AlternativeEvidences mapped if ExitType='4'", 3, Provider.AlternativeEvidences.Count);
				AssertArrayEqualsByElements("TransportDocuments with type '14'", new[] { "A014", "B014" }, Provider.AlternativeEvidences.Single(x => x.Type == AlternativeEvidenceTypeList.Codes._14).TransportDocuments.Select(x => x.Type).ToArray());
				AssertArrayEqualsByElements("TransportDocuments with type '15'", new[] { "A015" }, Provider.AlternativeEvidences.Single(x => x.Type == AlternativeEvidenceTypeList.Codes._15).TransportDocuments.Select(x => x.Type).ToArray());
				AssertArrayEqualsByElements("TransportDocuments with type '16'", new[] { "A016" }, Provider.AlternativeEvidences.Single(x => x.Type == AlternativeEvidenceTypeList.Codes._16).TransportDocuments.Select(x => x.Type).ToArray());
			});
		}

		public void TestDeclarantSpecified()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				AssertEquals("XX0X", true, Provider.DeclarantSpecified);
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				AssertEquals("Not XX0X", false, Provider.DeclarantSpecified);

				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				var additionInfo1 = invoice1.AdditionalInfos.AddNew();
				additionInfo1.CSI_Code = "X0002";
				additionInfo1.CSI_SubType = "NBA";
				AssertEquals("Not XX0X and no INF document", false, Provider.DeclarantSpecified);
				var additionInfo2 = invoice1.AdditionalInfos.AddNew();
				additionInfo2.CSI_Code = "X0001";
				additionInfo2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				AssertEquals("Not XX0X and has INF document", true, Provider.DeclarantSpecified);
			});
		}

		public void TestDeclarantContactPersonSpecified()
		{
			CombineAssertions(() =>
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				AssertEquals("Not XX1X", false, Provider.DeclarantContactPersonSpecified);
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				AssertEquals("XX1X", true, Provider.DeclarantContactPersonSpecified);
			});
		}

		protected override IEnumerable<Expression<Func<EXPEXTHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Declarant;
			yield return x => x.Representative;
			yield return x => x.ExitCarrier;
			yield return x => x.AlternativeEvidences;
		}

		protected override void SetUp()
		{
			base.SetUp();
			action = new ExportEntryMessageSendingAction(entryHeader);

			entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();
		}
		ExportEntryMessageSendingAction action;
		CusEntryInstruction entryInstruction;

		new IEXPEXTHeader Provider => base.Provider;

		protected override EXPEXTHeaderProvider GetProvider() => new EXPEXTHeaderProvider(action);

		OrgAddress GetTestOrgAddress(string eoriNumber, string ebsNumber, string name, string address, string city, string postcode, string country)
		{
			var testOrgAddress = GetOrgWithEORNumberAndEORIBranch(eoriNumber, ebsNumber);
			testOrgAddress.Header.OH_FullName = name;
			testOrgAddress.Address1 = address;
			testOrgAddress.City = city;
			testOrgAddress.Postcode = postcode;
			testOrgAddress.OA_RN_NKCountryCode = country;
			return testOrgAddress;
		}

		OrgAddress ExitCarrier => GetTestOrgAddress("EOR1", "EBS1", "MAX MUSTERMANN", "TESTSTRASSE 1", "MAINZ", "55126", Core.Constants.CountryCodes.Germany);

		void AddAlternativeEvidence(ZString type, ZString docType)
		{
			var alternativeEvidence = action.AlternativeEvidences.AddNew();
			alternativeEvidence.EvidenceType = type;
			alternativeEvidence.DocType = docType;
		}

		void PrepareAlternativeEvidences()
		{
			AddAlternativeEvidence(AlternativeEvidenceTypeList.Codes._14, "A014");
			AddAlternativeEvidence(AlternativeEvidenceTypeList.Codes._14, "B014");
			AddAlternativeEvidence(AlternativeEvidenceTypeList.Codes._15, "A015");
			AddAlternativeEvidence(AlternativeEvidenceTypeList.Codes._16, "A016");
		}
	}
}
