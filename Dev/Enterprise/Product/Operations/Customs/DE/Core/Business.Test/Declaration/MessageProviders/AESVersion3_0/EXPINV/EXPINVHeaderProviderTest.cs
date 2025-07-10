using System;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPINVHeaderProvider))]
	class EXPINVHeaderProviderTest : AESHeaderProviderAbstractTest<EXPINVHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPINVHeaderProvider(null));
		}

		public void TestLocalReferenceNumber()
		{
			entryHeader.LocalReferenceNumber = "REF12345";
			CombineAssertions(() =>
			{
				AssertEquals("MRN empty", "REF12345", Provider.LocalReferenceNumber);

				AddMRN();
				AssertEquals("MRN not empty", String.Empty, Provider.LocalReferenceNumber);
			});
		}

		public void TestInvalidationReason()
		{
			action.Annotation = "Cancel reason.";
			AssertEquals("CancellationReason", "Cancel reason.", Provider.InvalidationReason);
		}

		public void TestExporter()
		{
			AssertNull("According to the specification (D.1.0), Exporter is a Locked data group and will not be used until a new version is released.", Provider.Exporter);
		}

		public void TestDeclarant_PartyConstellationNoRepresentation()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			declaration.JE_OA_DeclarantAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1").PK;
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1100;
			using (Factory.SetTemporaryCurrentUser("Sachbearbeiter", "Bob Baumeister", "06131474747", "bob.baumeister@samplefreight.de"))
			{
				CombineAssertions("Populated without ContactPerson as no representation", () =>
				{
					AssertEquals("EoriNumber", "GREOR1", Provider.Declarant.EoriNumber);
					AssertEquals("EoriBranchSuffix", "EBS1", Provider.Declarant.EoriBranchSuffix);
					AssertNull("No ContactPerson", Provider.Declarant.ContactPerson);
				});
			}
		}

		public void TestDeclarant_PartyConstellationDirectRepresentation()
		{
			declaration.JE_OA_DeclarantAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1").PK;
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0011;
			AddMRN();
			using (Factory.SetTemporaryCurrentUser("Sachbearbeiter", "Bob Baumeister", "06131474747", "bob.baumeister@samplefreight.de"))
			{
				AssertNull("Only Representative should be mapped when ZG_PartyConstellation == '**1*', not Declarant", Provider.Declarant);
			}
		}

		public void TestDeclarant_MissingEORINumber()
		{
			declaration.JE_OA_DeclarantAddress = Factory.New<OrgHeader>().MainAddress.PK;
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1100;
			AssertNull("Declarant has no EORINumber", Provider.Declarant);
		}

		public void TestDeclarant_InvalidPartyConstellation()
		{
			declaration.JE_OA_DeclarantAddress = Factory.GetOrgHeaderWithEori("TESTORG", "12345", Core.Constants.CountryCodes.Greece).PK;
			entryInstruction.ZG_PartyConstellation = "0020";
			AssertNull("Invalid party constellation should return a null Declarant.", Provider.Declarant);
		}

		public void TestDeclarant_PartyConstellationDirectRepresentationButMissingMRN()
		{
			declaration.JE_OA_DeclarantAddress = Factory.GetOrgHeaderWithEori("TESTORG", "12345", Core.Constants.CountryCodes.Greece).PK;
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
			AssertNull("Missinng MRN for PartyConstellation **1*", Provider.Declarant);
		}

		protected override EXPINVHeaderProvider GetProvider() => new EXPINVHeaderProvider(action);

		protected override void SetUp()
		{
			base.SetUp();
			action = new ExportEntryMessageSendingAction(entryHeader);
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		}
		ExportEntryMessageSendingAction action;
		CusEntryInstruction entryInstruction;

		new IEXPINVHeader Provider => base.Provider;

		void AddMRN()
		{
			var entryNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			entryNumber.CE_EntryNum = "Test";
		}
	}
}
