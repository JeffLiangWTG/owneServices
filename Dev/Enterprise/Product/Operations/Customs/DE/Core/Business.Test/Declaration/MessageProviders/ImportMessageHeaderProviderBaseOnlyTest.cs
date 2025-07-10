using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ImportMessageHeaderProviderBaseForTest))]
	sealed class ImportMessageHeaderProviderBaseOnlyTest : ImportMessageHeaderProviderTest<ImportMessageHeaderProviderBaseForTest>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ImportMessageHeaderProviderBaseForTest(null));
		}

		public void TestHeader()
		{
			AssertType<ImportHeaderProviderForTest>(Provider.Header);
		}

		public void TestMessageGroup()
		{
			AssertEquals("ZBE", Provider.MessageGroup);
		}

		public void TestInterchangeRecipientID()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.InterchangeRecipientID);
				declaration.JE_CustomsOffice = "CO001";
				AssertEquals("ExportCustomsOffice", "CO001", Provider.InterchangeRecipientID);
			});
		}

		public void TestInterchangeSender()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var representative = EORIHelperTest.GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			representative.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			declaration.JE_OA_Representative = representative.PK;

			CombineAssertions(() =>
			{
				AssertEquals("InterchangeSender.EoriNumber", "DEEOR1", Provider.InterchangeSender.EoriNumber);
				AssertEquals("InterchangeSender.EoriBranchSuffix", "EBS1", Provider.InterchangeSender.EoriBranchSuffix);
			});
		}

		public void TestAuthorisationNumber()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var representative = EORIHelperTest.GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			representative.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			declaration.JE_OA_Representative = representative.PK;

			AssertEquals("AuthorisationNumber", "9877700310012345123456000", Provider.AuthorisationNumber);
		}

		[TestDate(2022, 5, 3, 16, 35, 22)]
		public void TestPreparationDateAndTimeCET()
		{
			AssertEquals(new DateTime(2022, 5, 3, 18, 35, 00), Provider.PreparationDateAndTimeCET.DateAndTime);
		}

		protected override ImportMessageHeaderProviderBaseForTest GetProvider() => new ImportMessageHeaderProviderBaseForTest(entryHeader);

		new IImportMessageHeader Provider => base.Provider;
	}

	class ImportMessageHeaderProviderBaseForTest : ImportMessageHeaderProvider
	{
		public ImportMessageHeaderProviderBaseForTest(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public override IImportHeader Header => header ?? (header = new ImportHeaderProviderForTest(EntryHeader));

		public override string MessageGroup => ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;

		IImportHeader header;
	}
}
