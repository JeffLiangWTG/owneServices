using System;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.Customs.GB.H7.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Messaging.Testing
{
	[TestedType(typeof(CDSH7QueryMRNSendingObject))]
	sealed class CDSH7QueryMRNSummarySendingObjectTest : CDSH7QuerySendingObjectTestBase
	{
		public void TestQueryType_ThrowExceptionGivenQueryTypeIsInvalid()
		{
			var exception = AssertExceptionThrown<ArgumentException>(() => new CDSH7QueryMRNSendingObject(bill, H7QueryTypeList.Codes.DUCR));
			AssertEquals("Only MRN query types are supported.", exception.Message);
		}

		protected override ZString expectedEntryNumberType => CDSDISQueryHelper.Constants.EntryNumberTypes.MRN;

		protected override ZString expectedEntryNumber => "MRN0001";

		protected override ZString expectedNotificationType => CDSDISQueryHelper.Constants.NotificationTypes.Status;

		protected override BaseCDSQuerySendingObject GetNewMessageSendingObject()
		{
			bill.MovementReferenceNumber = "MRN0001";
			return new CDSH7QueryMRNSendingObject(bill, H7QueryTypeList.Codes.MRNSummary);
		}
	}

	[TestedType(typeof(CDSH7QueryMRNSendingObject))]
	sealed class CDSH7QueryMRNSnapshotSendingObjectTest : CDSH7QuerySendingObjectTestBase
	{
		protected override ZString expectedEntryNumberType => CDSDISQueryHelper.Constants.EntryNumberTypes.MRN;

		protected override ZString expectedEntryNumber => "MRN0001";

		protected override ZString expectedNotificationType => CDSDISQueryHelper.Constants.NotificationTypes.Full;

		protected override BaseCDSQuerySendingObject GetNewMessageSendingObject()
		{
			bill.MovementReferenceNumber = "MRN0001";
			return new CDSH7QueryMRNSendingObject(bill, H7QueryTypeList.Codes.MRNSnapshot);
		}
	}

	[TestedType(typeof(CDSH7QueryDUCRSendingObject))]
	sealed class CDSH7QueryDUCRSendingObjectTest : CDSH7QuerySendingObjectTestBase
	{
		protected override ZString expectedEntryNumberType => CDSDISQueryHelper.Constants.EntryNumberTypes.DUCR;

		protected override ZString expectedEntryNumber => "23456";

		protected override ZString expectedNotificationType => CDSDISQueryHelper.Constants.NotificationTypes.Status;

		protected override BaseCDSQuerySendingObject GetNewMessageSendingObject()
		{
			var document1 = bill.PreviousDocuments.AddNew();
			document1.CSI_Code = PreviousDocumentCodeListCDS.Codes.AdministrativeAccompanyingDocument;
			document1.CSI_ReferenceNumber = "12345";
			var document2 = bill.PreviousDocuments.AddNew();
			document2.CSI_Code = PreviousDocumentCodeListCDS.Codes.DeclarationUniqueConsignmentReferenceDucr;
			document2.CSI_ReferenceNumber = "23456";
			return new CDSH7QueryDUCRSendingObject(bill);
		}
	}

	[TestedType(typeof(CDSH7QueryUCRSendingObject))]
	sealed class CDSH7QueryUCRSendingObjectTest : CDSH7QuerySendingObjectTestBase
	{
		protected override ZString expectedEntryNumberType => CDSDISQueryHelper.Constants.EntryNumberTypes.UCR;

		protected override ZString expectedEntryNumber => "UCR001";

		protected override ZString expectedNotificationType => CDSDISQueryHelper.Constants.NotificationTypes.Status;

		protected override BaseCDSQuerySendingObject GetNewMessageSendingObject()
		{
			bill.ABL_UCRNumber = "UCR001";
			return new CDSH7QueryUCRSendingObject(bill);
		}
	}
}
