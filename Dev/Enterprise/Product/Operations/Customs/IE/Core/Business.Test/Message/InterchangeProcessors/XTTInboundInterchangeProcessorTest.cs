using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using BaseEDIInterchange = Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.IE.Business.Testing
{
	class XTTInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		class XTTInboundInterchangeProcessorTestClass : XTTInboundInterchangeProcessor
		{
			protected override string[] ApplicationCodes => new[] { BaseEDIInterchange.ApplicationCodes.IECustomsCommon, BaseEDIInterchange.ApplicationCodes.IECustomsExport };

			protected override ZQuery GetInterchangeTypeFilter() => new ZQuery(EDIInterchangeSchema.EI_InterchangeType, "X12");

			protected override IInboundMessageCreator GetMessageCreator(BaseEDIInterchange interchange)
			{
				interchange.EI_HeaderText = "Processed";
				return new InboundMessageCreatorNoCreation();
			}
		}

		public void TestIsInterchangeNotDeleted()
		{
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var incomingInterchange1 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, BaseEDIInterchange.ApplicationCodes.IECustomsCommon, "X12", "<GREETING>HELO</GREETING>", sessionGUID: ZGuid.NewZGuid(), branchPK: ieBranch.PK);
			var incomingInterchange2 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, BaseEDIInterchange.ApplicationCodes.IECustomsCommon, "X12", "<GREETING>HELO</GREETING>", sessionGUID: ZGuid.NewZGuid(), branchPK: ieBranch.PK);
			incomingInterchange2.EI_Status = EDIInterchange.Status.Error;
			var incomingInterchange3 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, BaseEDIInterchange.ApplicationCodes.IECustomsCommon, "X12", "<GREETING>HELO</GREETING>", sessionGUID: ZGuid.NewZGuid(), branchPK: ieBranch.PK);
			Factory.Save();
			CombineAssertions(() =>
			{
				incomingInterchange1.Delete();
				var processor = new XTTInboundInterchangeProcessorTestClass();
				AssertEquals("Deleted", false, processor.IsInterchangeNotDeleted(incomingInterchange1));
				AssertEquals("Error", false, processor.IsInterchangeNotDeleted(incomingInterchange2));
				AssertEquals("Normal", true, processor.IsInterchangeNotDeleted(incomingInterchange3));
			});
		}

		public void TestCorrectMatch()
		{
			(_, GlbBranch branch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var nonIEIncomingInterchange1 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, BaseEDIInterchange.ApplicationCodes.TRCustoms, "X12", "<GREETING>HELO</GREETING>", sessionGUID: ZGuid.NewZGuid(), branchPK: branch1.PK);
			var differentTypeIncomingInterchange2 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, BaseEDIInterchange.ApplicationCodes.IECustomsCommon, "X13", "<GREETING>HELO</GREETING>", sessionGUID: ZGuid.NewZGuid(), branchPK: branch1.PK);
			var notXTTIncomingInterchange3 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, BaseEDIInterchange.ApplicationCodes.IECustomsCommon, "X12", "<GREETING>HELO</GREETING>", sessionGUID: ZGuid.NewZGuid(), branchPK: branch1.PK);
			notXTTIncomingInterchange3.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			var incomingInterchange5 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, BaseEDIInterchange.ApplicationCodes.IECustomsCommon, "X12", "<GREETING>HELO</GREETING>", sessionGUID: ZGuid.NewZGuid(), branchPK: branch1.PK);
			var messageAcknowledgementInterchange6 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, BaseEDIInterchange.ApplicationCodes.IECustomsExport, "X12", "<GREETING>HELO</GREETING>", sessionGUID: ZGuid.NewZGuid(), branchPK: branch1.PK);
			Factory.Save();
			CombineAssertions(() =>
			{
				new XTTInboundInterchangeProcessorTestClass().ExecuteBatch(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				AssertEDIInterchange(anotherFactory.Load<BaseEDIInterchange>(nonIEIncomingInterchange1.PK), EDIInterchange.Status.Queued, ZString.Empty);
				AssertEDIInterchange(anotherFactory.Load<BaseEDIInterchange>(differentTypeIncomingInterchange2.PK), EDIInterchange.Status.Queued, ZString.Empty);
				AssertEDIInterchange(anotherFactory.Load<BaseEDIInterchange>(notXTTIncomingInterchange3.PK), EDIInterchange.Status.Queued, ZString.Empty);
				AssertEDIInterchange(anotherFactory.Load<BaseEDIInterchange>(incomingInterchange5.PK), EDIInterchange.Status.Received, "Processed");
				AssertEDIInterchange(anotherFactory.Load<BaseEDIInterchange>(messageAcknowledgementInterchange6.PK), EDIInterchange.Status.Received, "Processed");
			});
		}

		void AssertEDIInterchange(BaseEDIInterchange interchange, ZString status, ZString headerText)
		{
			var reference = interchange.EI_InterchangeNum;
			AssertEquals(reference + " - interchange.EI_Status", status, interchange.EI_Status);
			AssertEquals(reference + " - interchange.EI_HeaderText", headerText, interchange.EI_HeaderText);
		}
	}
}
