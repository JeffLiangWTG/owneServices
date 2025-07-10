using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Business.MessagesWrappers.ECS;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class ECSEnveloppeMessageWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestIE507EnveloppeMessageWrapperConstructor()
		{
			Activator.CreateInstance(typeof(IE507EnveloppeMessageWrapper), new object[] { null });
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestIE618EnveloppeMessageWrapperConstructor()
		{
			Activator.CreateInstance(typeof(IE618EnveloppeMessageWrapper), new object[] { null });
		}

		public void TestIE507EnveloppeMessageWrapperProperties()
		{
			var key = GlbCompany.CurrentCompany.LicenceKeyIdentifier;

			var wrapper1 = new IE507EnveloppeMessageWrapper(exitDetail);

			AssertEquals("MessageIE507", wrapper1.SchemaID);
			AssertEquals("01012012", wrapper1.SchemaVersion);
			AssertEquals("39433691100042", wrapper1.PartnerId);
			AssertEquals(key + "+ECS+<<INTERCHANGENUMBERPLACEHOLDER>>", wrapper1.TransactionId);
			AssertEquals((short)2, wrapper1.NumSeq);
		}

		public void TestIE618EnveloppeMessageWrapperProperties()
		{
			var key = GlbCompany.CurrentCompany.LicenceKeyIdentifier;

			var wrapper1 = new IE618EnveloppeMessageWrapper(exitDetail);

			AssertEquals("MessageIE618", wrapper1.SchemaID);
			AssertEquals("01012012", wrapper1.SchemaVersion);
			AssertEquals("39433691100042", wrapper1.PartnerId);
			AssertEquals(key + "+ECS+<<INTERCHANGENUMBERPLACEHOLDER>>", wrapper1.TransactionId);
			AssertEquals((short)2, wrapper1.NumSeq);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = SetupDeclarant().MainAddress.PK;
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = "JE";
			exitHeader.CEH_ParentID = declaration.PK;
			exitHeader.CEH_ReferenceNumber = "123";
			exitDetail = Factory.New<CusExitDetail>();
			exitDetail.CED_CEH = exitHeader.PK;
			exitDetail.CED_Status = "TTT";

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.FRCustomsMessage;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_To = "TEST";
			interchange.EI_From = "FROM";
			interchange.EI_InterchangeNum = "26";

			helper = new ECSMessageTestHelper();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			ediMessage1 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.DEP, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("39433691100042"));
			ediMessage2 = helper.CreateTestEDIMessage(Factory, branch.PK, ApplicationCodeList.Codes.FRCustomsMessage, ReceiveTransmitList.Codes.Transmit, MessageTypeList.Codes.ECS, MessageSubTypeList.Codes.ARR, EDIMessageStatusList.Codes.Queued, helper.CresteEMMessageText("39433691100043"));
			exitDetail.Messages.Add(ediMessage1);
			ediMessage1.EM_LinkedObject = exitDetail;
			ediMessage1.EM_EI = interchange.PK;
			exitDetail.Messages.Add(ediMessage2);
			ediMessage2.EM_LinkedObject = exitDetail;
		}

		OrgHeader SetupDeclarant()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var declarantAddress = orgHeader.MainAddress;
			orgHeader.OH_Code = OrgCusCode.FranceCodeTypes.Siret;

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.FranceCodeTypes.Siret, "39433691100042", Core.Constants.CountryCodes.France);
			var collectionFrance = orgHeader.DeltaAgreementNumberCollection;
			var deltaAgreement = collectionFrance.AddNew();
			deltaAgreement.CZ_Code = OrgCusAccountCodeList.Codes.ECS;
			deltaAgreement.CZ_Account = "00001909";
			deltaAgreement.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;

			return orgHeader;
		}

		ECSMessageTestHelper helper;
		CusExitDetail exitDetail;
		TestEDIMessage ediMessage1, ediMessage2;
	}
}
