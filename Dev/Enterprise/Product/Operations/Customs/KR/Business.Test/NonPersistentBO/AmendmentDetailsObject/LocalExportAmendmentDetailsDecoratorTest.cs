using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExportAmendmentDetailsDecoratorTest : TestCaseWithFactory
	{
		public void TestLocalExportAmendmentDetailsByCusEntrySnapShotData()
		{
			entry = Get5DREntryHeader();
			AssertEquals("This snapshot was created when sending 5DP.", 1, entry.Snapshots.Count);
			var amendedDetails = new LocalExportAmendmentDetailsCollection(entry)[0];
			AssertionLocalExportAmendmentDetailsData(amendedDetails, "5DP Company", "5DP Address1 5DP Address2", "5DP CompanyRepresentative");

			entry = Get5DSEntryHeader();
			AssertEquals("This snapshot was created when sending 5DQ.", 1, entry.Snapshots.Count);
			amendedDetails = new LocalExportAmendmentDetailsCollection(entry)[0];
			AssertionLocalExportAmendmentDetailsData(amendedDetails, "5DQ Company", "5DQ Address1 5DQ Address2", "5DQ CompanyRepresentative");
		}

		void AssertionLocalExportAmendmentDetailsData(LocalExportAmendmentDetails amendmentDetails, ZString supplierCompanyName, ZString supplierAddress, ZString supplierCompanyRepresentative)
		{
			AssertEquals(supplierAddress, amendmentDetails.Supplier.AddressDetails);
			AssertEquals(supplierCompanyName, amendmentDetails.Supplier.CompanyName);
			AssertEquals(supplierCompanyRepresentative, amendmentDetails.Supplier.RepresentativeName);
		}

		CusEntryHeader Get5DREntryHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5DR);
			var messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();

			entry.CusEntryNumber.CE_EntryNum = "6N00220000051X";
			entry.CH_VersionID = 1;
			entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == 1).Delete();
			entry.ResetIsCustomsValueCalculated();
			var declaration = Factory.Load<JobDeclaration>(entry.Declaration.PK);
			var amendmentMessageSendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DR);
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			new GOVCBR5DS5DRAmendmentSender(messageSendingObjects, ElectronicDocumentTypeList.Codes._5DR, Factory).Send();
			Factory.Save();

			return entry;
		}

		CusEntryHeader Get5DSEntryHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5DQ;
			var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5DS);
			var messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();

			entry.CusEntryNumber.CE_EntryNum = "6N00220000052X";
			entry.CH_VersionID = 2;
			entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == 1).Delete();
			entry.ResetIsCustomsValueCalculated();
			var declaration = Factory.Load<JobDeclaration>(entry.Declaration.PK);
			var amendmentMessageSendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DS);
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			new GOVCBR5DS5DRAmendmentSender(messageSendingObjects, ElectronicDocumentTypeList.Codes._5DS, Factory).Send();
			Factory.Save();

			return entry;
		}

		CusEntryHeader entry;
	}
}
