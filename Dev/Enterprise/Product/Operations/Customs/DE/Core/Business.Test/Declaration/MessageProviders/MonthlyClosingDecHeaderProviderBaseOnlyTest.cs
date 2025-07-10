using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(MonthlyClosingDecHeaderProvider))]
	class MonthlyClosingDecHeaderProviderBaseOnlyTest : MonthlyClosingDecHeaderProviderAbstractTest<MonthlyClosingDecHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new MonthlyClosingDecHeaderProviderBaseForTest(null, MonthlyClosingMessageRoleList.Codes.FinalMessage));
		}

		public void TestMessageRole()
		{
			AssertEquals("47", Provider.MessageRole);
		}

		public void TestDeclarationKind_AAV()
		{
			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AAV;
			AssertEquals("Z", Provider.DeclarationKind);
		}

		public void TestDeclarationKind_AZ()
		{
			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AZ;
			AssertEquals("Z", Provider.DeclarationKind);
		}

		public void TestDeclarationKind_AZL()
		{
			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AZL;
			AssertEquals("Z", Provider.DeclarationKind);
		}

		public void TestDeclarationKind_VAV()
		{
			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VAV;
			AssertEquals("Y", Provider.DeclarationKind);
		}

		public void TestDeclarationKind_VZA()
		{
			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VZA;
			AssertEquals("Y", Provider.DeclarationKind);
		}

		public void TestDeclarationKind_VZL()
		{
			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VZL;
			AssertEquals("Y", Provider.DeclarationKind);
		}

		public void TestDeclarationKind_Other()
		{
			declaration.CRD_DeclarationType = CusReconDeclarationTypeList.Codes.IWP;
			AssertEquals(string.Empty, Provider.DeclarationKind);
		}

		public void TestReferenceNumber_2() => AssertReferenceNumber(MonthlyClosingMessageRoleList.Codes.AmendmentMessage, true);

		public void TestReferenceNumber_22() => AssertReferenceNumber(MonthlyClosingMessageRoleList.Codes.FinalizationMessage, true);

		public void TestReferenceNumber_36() => AssertReferenceNumber(MonthlyClosingMessageRoleList.Codes.ModificationMessage, true);

		public void TestReferenceNumber_47() => AssertReferenceNumber(MonthlyClosingMessageRoleList.Codes.FinalMessage, false);

		public void TestReferenceNumber_9() => AssertReferenceNumber(MonthlyClosingMessageRoleList.Codes.FirstPartialMessage, false);

		public void TestLocalReferenceNumber()
		{
			declaration.CRD_JobReferenceNumber = "JOBREFERENCENUMBER";
			AssertEquals("JOBREFERENCENUMBER", Provider.LocalReferenceNumber);
		}

		public void TestStartAccountingPeriodDate()
		{
			declaration.CRD_PeriodFrom = new ZDate(2022, 1, 1);
			AssertEquals(new DateTime(2022, 1, 1), Provider.StartAccountingPeriodDate);
		}

		public void TestEndAccountingPeriodDate()
		{
			declaration.CRD_PeriodTo = new ZDate(2022, 1, 31);
			AssertEquals(new DateTime(2022, 1, 31), Provider.EndAccountingPeriodDate);
		}

		public void TestDeclarantIsConsigneeFlag_Y()
		{
			declaration.CRD_OA_DeclarantAddress = Factory.New<OrgAddress>().PK;
			declaration.CRD_OA_ImporterAddress = Factory.New<OrgAddress>().PK;
			declaration.IsDeclarantImporter = true;
			AssertEquals(true, Provider.DeclarantIsConsigneeFlag);
		}

		public void TestDeclarantIsConsigneeFlag_N()
		{
			declaration.CRD_OA_DeclarantAddress = Factory.New<OrgAddress>().PK;
			declaration.CRD_OA_ImporterAddress = Factory.New<OrgAddress>().PK;
			declaration.IsDeclarantImporter = false;
			AssertEquals(false, Provider.DeclarantIsConsigneeFlag);
		}

		public void TestLocalClearanceProcedure()
		{
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			header.CPH_Number = "AUTHNUMBER";
			declaration.CRD_CPH_ReconClearanceAuthorisation = header.PK;

			AssertEquals("AUTHNUMBER", Provider.LocalClearanceProcedure);
		}

		public void TestProcedureAuthorization()
		{
			var entry = declaration.CusReconEntries.AddNew();
			var entryHeader = Factory.New<Declaration.CusEntryHeader>();
			entry.CRE_CH_OriginalEntry = entryHeader.PK;
			var entryInstruction = Factory.New<Declaration.CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var usage = entryInstruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = "EUS";
			usage.AGC_Number = "ENDUSEAUTHNUMBER";

			AssertEquals("ENDUSEAUTHNUMBER", Provider.ProcedureAuthorization);
		}

		public void TestCurrencyCode()
		{
			AssertEquals("EUR", Provider.CurrencyCode);
		}

		public void TestRepresentativeRelationshipFlag_SEL()
		{
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertEquals("0", Provider.RepresentativeRelationshipFlag);
		}

		public void TestRepresentativeRelationshipFlag_DIR()
		{
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("1", Provider.RepresentativeRelationshipFlag);
		}

		public void TestRepresentativeRelationshipFlag_IND()
		{
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertEquals("2", Provider.RepresentativeRelationshipFlag);
		}

		public void TestDeclarationPlace()
		{
			AssertEquals("Brisbane", Provider.DeclarationPlace);
		}

		public void TestDeclarant()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var importerAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				var declarantAddress = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS1");
				declaration.CRD_OA_ImporterAddress = importerAddress.PK;
				declaration.CRD_OA_DeclarantAddress = declarantAddress.PK;
				AssertNotNull("Populated", Provider.Declarant);
				AssertEquals("Declarant's EORI", "GREOR2", Provider.Declarant.Identification.EoriNumber);
			});
		}

		public void TestRepresentative_NotPopulated()
		{
			var representativeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.CRD_OA_RepresentativeAddress = representativeAddress.PK;
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertNull(Provider.Representative);
		}

		public void TestRepresentative()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var representativeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				declaration.CRD_OA_RepresentativeAddress = representativeAddress.PK;
				declaration.CRD_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertNotNull("Populated", Provider.Representative);
				AssertEquals("Represantative's EORI", "GREOR1", Provider.Representative.EoriNumber);
			});
		}

		public void TestPrincipal_NotPopulated()
		{
			var principalAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.CRD_OA_BuyingAgentAddress = principalAddress.PK;
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertNull(Provider.Principal);
		}

		public void TestPrincipal()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var principalAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				declaration.CRD_OA_BuyingAgentAddress = principalAddress.PK;
				declaration.CRD_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertNotNull("Populated", Provider.Principal);
				AssertEquals("Principal's EORI", "GREOR1", Provider.Principal.Identification.EoriNumber);
			});
		}

		public void TestContactPerson()
		{
			CombineAssertions(() =>
			{
				using (Factory.SetTemporaryCurrentUser(fullName: "Bob Baumeister"))
				{
					AssertNotNull("Populated", Provider.ContactPerson);
					AssertEquals("Current User's name", "Bob Baumeister", Provider.ContactPerson.PersonName);
				}
			});
		}

		public void TestIsModificationMessage_9() => AssertIsModificationMessage(MonthlyClosingMessageRoleList.Codes.FirstPartialMessage, false);

		public void TestIsModificationMessage_2() => AssertIsModificationMessage(MonthlyClosingMessageRoleList.Codes.AmendmentMessage, false);

		public void TestIsModificationMessage_47() => AssertIsModificationMessage(MonthlyClosingMessageRoleList.Codes.FinalMessage, false);

		public void TestIsModificationMessage_22() => AssertIsModificationMessage(MonthlyClosingMessageRoleList.Codes.FinalizationMessage, false);

		public void TestIsModificationMessage_36() => AssertIsModificationMessage(MonthlyClosingMessageRoleList.Codes.ModificationMessage, true);

		void AssertIsModificationMessage(string messageRole, bool expectedIsModificationMessage)
		{
			var provider = new MonthlyClosingDecHeaderProviderBaseForTest(declaration, messageRole);
			AssertEquals($"message role {messageRole}, IsModificationMessage should be {expectedIsModificationMessage}", expectedIsModificationMessage, provider.IsModificationMessage);
		}

		protected override MonthlyClosingDecHeaderProvider GetProvider() => new MonthlyClosingDecHeaderProviderBaseForTest(declaration, "47");

		void AssertReferenceNumber(string messageRole, bool shouldBeFilled)
		{
			var provider = new MonthlyClosingDecHeaderProviderBaseForTest(declaration, messageRole);
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "MRN12345";
			AssertEquals(shouldBeFilled, provider.ReferenceNumber == "MRN12345");
		}
	}

	class MonthlyClosingDecHeaderProviderBaseForTest : MonthlyClosingDecHeaderProvider
	{
		public MonthlyClosingDecHeaderProviderBaseForTest(CusReconDeclaration declaration, string messageRole)
			: base(declaration, messageRole)
		{
		}

		internal new bool IsModificationMessage => base.IsModificationMessage;
	}
}
