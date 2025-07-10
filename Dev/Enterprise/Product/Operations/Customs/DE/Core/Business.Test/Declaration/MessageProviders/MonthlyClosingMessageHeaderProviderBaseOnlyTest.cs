using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(MonthlyClosingMessageHeaderProviderForTest))]
	sealed class MonthlyClosingMessageHeaderProviderBaseOnlyTest : MonthlyClosingMessageHeaderProviderTest<MonthlyClosingMessageHeaderProviderForTest>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new MonthlyClosingMessageHeaderProviderForTest(null));
		}

		public void TestHeader()
		{
			AssertType<MonthlyClosingDecHeaderProviderBaseForTest>(Provider.Header);
		}

		public void TestMessageGroup()
		{
			AssertEquals("ABC", Provider.MessageGroup);
		}

		public void TestInterchangeRecipientID()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.InterchangeRecipientID);
				declaration.CRD_CustomsOffice = "DE003301";
				AssertEquals("DE003301", Provider.InterchangeRecipientID);
			});
		}

		public void TestInterchangeSender()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var representative = EORIHelperTest.GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			representative.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			declaration.CRD_OA_RepresentativeAddress = representative.PK;

			CombineAssertions(() =>
			{
				AssertEquals("InterchangeSender.EoriNumber", "DEEOR1", Provider.InterchangeSender.EoriNumber);
				AssertEquals("InterchangeSender.EoriBranchSuffix", "EBS1", Provider.InterchangeSender.EoriBranchSuffix);
			});
		}

		public void TestAuthorisationNumber()
		{
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var representative = EORIHelperTest.GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			representative.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			declaration.CRD_OA_RepresentativeAddress = representative.PK;

			AssertEquals("AuthorisationNumber", "9877700310012345123456000", Provider.AuthorisationNumber);
		}

		[TestDate(2022, 5, 3, 16, 35, 22)]
		public void TestPreparationDateAndTimeCET()
		{
			AssertEquals(new DateTime(2022, 5, 3, 18, 35, 00), Provider.PreparationDateAndTimeCET.DateAndTime);
		}

		protected override MonthlyClosingMessageHeaderProviderForTest GetProvider() => new MonthlyClosingMessageHeaderProviderForTest(declaration);

		protected override void SetUp()
		{
			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();
		}

		new IImportMessageHeader Provider => base.Provider;
	}

	sealed class MonthlyClosingMessageHeaderProviderForTest : MonthlyClosingMessageHeaderProvider
	{
		public MonthlyClosingMessageHeaderProviderForTest(CusReconDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}

		public override string MessageGroup => "ABC";

		public override IImportHeader Header => header ?? (header = new MonthlyClosingDecHeaderProviderBaseForTest(declaration, "47"));

		IImportHeader header;
		readonly CusReconDeclaration declaration;
	}
}
