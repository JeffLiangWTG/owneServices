using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CUSCONHeaderProvider))]
	sealed class CUSCONHeaderProviderTest : ImportHeaderProviderAbstractTest<CUSCONHeaderProvider>
	{
		public void TestConstructor_EntryHeaderNull()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSCONHeaderProvider(null));
		}

		public void TestConstructor_EntryInstructionNull()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSCONHeaderProvider(Factory.New<CusEntryHeader>()));
		}

		public void TestTemporaryReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", string.Empty, Provider.TemporaryReferenceNumber);

				mrnEntryNumber.CE_EntryNum = "ATC401573750920204851";
				AssertEquals("Has value", "ATC401573750920204851", Provider.TemporaryReferenceNumber);
			});
		}

		public void TestGoodsLocation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", string.Empty, Provider.GoodsLocation);

				declaration.JE_LocationOfGoods = "Mainz - Finthen";
				AssertEquals("Has value", "Mainz - Finthen", Provider.GoodsLocation);
			});
		}

		public void TestPresentationConfirmer_ReturnImportPartyIDProvider()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_Representative = orgAddress.PK;
			AssertType(typeof(ImportPartyIDProvider), Provider.PresentationConfirmer);
		}

		public void TestPresentationConfirmer_RepresentationTypeDIR()
		{
			orgAddress.OA_Address1 = "Address 1";
			TestHelper.CreateCL010CoutryList(Factory);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_Representative = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("NoRepresentative", null, Provider.PresentationConfirmer);

				declaration.JE_OA_Representative = orgAddress.PK;
				AssertType<ImportPartyIDProvider>("Type", Provider.PresentationConfirmer);
				AssertEquals("EORInumber", "GREOR1", Provider.PresentationConfirmer.EoriNumber);
				AssertEquals("EORIbranch", "EBS1", Provider.PresentationConfirmer.EoriBranchSuffix);
			});
		}

		public void TestPresentationConfirmer_RepresentationTypeSEL()
		{
			orgAddress.OA_Address1 = "Address 1";
			TestHelper.CreateCL010CoutryList(Factory);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			CombineAssertions(() =>
			{
				AssertEquals("Default: EORInumber", ZString.Empty, Provider.PresentationConfirmer.EoriNumber);
				AssertNull("Default: EORIbranch", Provider.PresentationConfirmer.EoriBranchSuffix);

				declaration.JE_OA_DeclarantAddress = orgAddress.PK;
				AssertEquals("EORInumber", "GREOR1", Provider.PresentationConfirmer.EoriNumber);
				AssertEquals("EORIbranch", "EBS1", Provider.PresentationConfirmer.EoriBranchSuffix);
			});
		}

		public void TestPresentationConfirmer_RepresentationTypeIND()
		{
			orgAddress.OA_Address1 = "Address 1";
			TestHelper.CreateCL010CoutryList(Factory);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			CombineAssertions(() =>
			{
				AssertEquals("Default: EORInumber", ZString.Empty, Provider.PresentationConfirmer.EoriNumber);
				AssertNull("Default: EORIbranch", Provider.PresentationConfirmer.EoriBranchSuffix);

				declaration.JE_OA_DeclarantAddress = orgAddress.PK;
				AssertEquals("EORInumber", "GREOR1", Provider.PresentationConfirmer.EoriNumber);
				AssertEquals("EORIbranch", "EBS1", Provider.PresentationConfirmer.EoriBranchSuffix);
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

		public void TestArrivalTransportMeansIdentity_FIX()
		{
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;
			declaration.ZG_Box18TransportID = "AA-BB123";
			AssertNull(Provider.ArrivalTransportMeansIdentity);
		}

		public void TestArrivalTransportMeansIdentity_Other()
		{
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			declaration.ZG_Box18TransportID = "AA-BB123";
			AssertEquals("AA-BB123", Provider.ArrivalTransportMeansIdentity);
		}

		public void TestPreviousAdministrativeReferenceType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", string.Empty, Provider.PreviousAdministrativeReferenceType);

				entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				AssertEquals("Not empty", "ATNEU", Provider.PreviousAdministrativeReferenceType);
			});
		}

		public void TestPreviousAdministrativeReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null", Provider.PreviousAdministrativeReferenceNumber);

				var previousDocument = entryInstruction.PreviousDocuments.AddNew();
				previousDocument.CSI_ReferenceNumber = "REF123456";
				AssertEquals("Not empty", "REF123456", Provider.PreviousAdministrativeReferenceNumber);
			});
		}

		public void TestSummaryDeclaration()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", Provider.SummaryDeclaration);

				entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				AssertNotNull("Not NULL", Provider.SummaryDeclaration);
			});
		}

		public void TestCustomsWarehouse()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", Provider.CustomsWarehouse);

				entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				AssertNotNull("Not NULL", Provider.CustomsWarehouse);
			});
		}

		public void TestInwardProcessing()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", Provider.InwardProcessing);

				entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				AssertNotNull("Not NULL", Provider.InwardProcessing);
			});
		}

		protected override CUSCONHeaderProvider GetProvider() => new CUSCONHeaderProvider(entryHeader);

		protected override void SetUp()
		{
			base.SetUp();
			mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			orgAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
		}
		CusEntryNumber mrnEntryNumber;
		OrgAddress orgAddress;

		new ICUSCONHeader Provider => base.Provider;
	}
}
