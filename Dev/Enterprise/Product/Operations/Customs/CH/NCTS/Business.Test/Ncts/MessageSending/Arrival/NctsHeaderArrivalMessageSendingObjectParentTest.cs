using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderArrivalMessageSendingObjectParent))]
class NctsHeaderArrivalMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderArrivalMessageSendingObjectParent(null));
		NctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		AssertExceptionThrown<ArgumentException>("When nctsHeader is not a arrival job", () => new NctsHeaderArrivalMessageSendingObject(NctsHeader));
	}

	public void TestValidation()
	{
		AssertType<NctsHeaderArrivalMessageSendingObjectParentValidation>(SendingObjectParent.Validation);
	}

	public void TestTopLevelBusinessObject()
	{
		AssertSame(NctsHeader, SendingObjectParent.TopLevelBusinessObject);
	}

	public void TestSecurityCheckpointToSendWithMessageError()
	{
		AssertSame(Env.Security.EuNctsSendWithMessageErrors, SendingObjectParent.SecurityCheckpointToSendWithMessageError);
	}

	public void TestGetSendingObjectsCollectionCore() => CombineAssertions(() =>
	{
		(var masterHeader, var header1, var header2) = CreateHeaderWithMultipleMRN();

		var sendingObject = new NctsHeaderArrivalMessageSendingObjectParent(masterHeader);
		AssertEquals("SendingObjectsCollection should contain 2 objects", 2, sendingObject.SendingObjectsCollection.Count);
		AssertEquals("First element should be header1", header1.MovementReferenceNumber, sendingObject.SendingObjectsCollection[0].MRN);
		AssertEquals("Second element should be header2", header2.MovementReferenceNumber, sendingObject.SendingObjectsCollection[1].MRN);

		masterHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnderRecoveryProcedure;
		sendingObject = new NctsHeaderArrivalMessageSendingObjectParent(masterHeader);
		AssertEquals("SendingObjectsCollection should contain masterHeader when CustomsStatus != UAP", 1, sendingObject.SendingObjectsCollection.Count);
		AssertEquals("First element should be header1", masterHeader.ArrivalMovementHeader.BM_PaperlessInbondNum, sendingObject.SendingObjectsCollection[0].LRN);
	});

	public void TestUpdateSendingObjectsBeforeSending()
	{
		(var masterHeader, var header1, var header2) = CreateHeaderWithMultipleMRN();
		var sendingObject = new NctsHeaderArrivalMessageSendingObjectParent(masterHeader);

		header1.ArrivalMovementHeader.BM_UnloadingDate = DateTime.Now;

		var dateOfUnloading = new ZDateTimeOffset(2025, 5, 29);
		sendingObject.DateOfUnloading = dateOfUnloading;
		sendingObject.UpdateSendingObjectsBeforeSending();

		AssertEquals("Header1, BM_UnloadingDate should be updated to: ", dateOfUnloading, header1.ArrivalMovementHeader.BM_UnloadingDate);
	}

	public void TestDateOfUnloading_Caption() => AssertEquals("DateOfUnloading Caption", "Date of unloading", SendingObjectParent.DateOfUnloadingInfo.Description);

	public void TestIsConformed_Caption() => AssertEquals("DateOfUnloading Caption", "Unloaded cargo conforms to declaration", SendingObjectParent.IsConformedInfo.Description);

	protected override BusinessObject GetNewBusinessObject()
	{
		return new NctsHeaderArrivalMessageSendingObjectParent(NctsHeader);
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		return nctsHeader;
	}

	NctsHeaderArrivalMessageSendingObjectParent SendingObjectParent => sendingObjectParent ?? (sendingObjectParent = (NctsHeaderArrivalMessageSendingObjectParent)GetNewBusinessObject());
	NctsHeaderArrivalMessageSendingObjectParent sendingObjectParent;

	(NctsHeader masterHeader, NctsHeader firstHeader, NctsHeader secondHeader) CreateHeaderWithMultipleMRN()
	{
		const string lrn = "22CH123456789012N0";

		var masterHeader = CreateNctsHeader();
		masterHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = lrn;
		masterHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;

		var mrn1 = masterHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		mrn1.CSI_ReferenceNumber = "123456789CH";
		mrn1.CSI_Status = YesNoList.Codes.Yes;

		var mrn2 = masterHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		mrn2.CSI_ReferenceNumber = "987654321CH";
		mrn2.CSI_Status = YesNoList.Codes.Yes;

		var childHeader1 = CreateNctsHeader();
		childHeader1.ArrivalMovementHeader.MultipleMRNIndicator = false;
		childHeader1.ArrivalMovementHeader.BM_PaperlessInbondNum = lrn;
		childHeader1.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		childHeader1.ArrivalMovementHeader.BM_NoChangesToReport = true;
		childHeader1.MovementReferenceNumberSetter(mrn1.CSI_ReferenceNumber);

		var childHeader2 = CreateNctsHeader();
		childHeader2.ArrivalMovementHeader.MultipleMRNIndicator = false;
		childHeader2.ArrivalMovementHeader.BM_PaperlessInbondNum = lrn;
		childHeader2.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		childHeader2.ArrivalMovementHeader.BM_NoChangesToReport = true;
		childHeader2.MovementReferenceNumberSetter(mrn2.CSI_ReferenceNumber);

		masterHeader.ArrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childHeader1.ArrivalMovementHeader);
		masterHeader.ArrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childHeader2.ArrivalMovementHeader);

		return (masterHeader, childHeader1, childHeader2);
	}
}
