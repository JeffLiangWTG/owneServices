using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObject))]
sealed class NctsHeaderMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestReadOnlyProperties()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MessageSubType is readonly", true, testItem.MessageSubTypeInfo.ReadOnly);
			AssertEquals("CustomsStatus is readonly", true, testItem.CustomsStatusInfo.ReadOnly);
		});
	}

	public void TestPropertiesSetFromHeader()
	{
		header.MovementHeader.BM_CustomsStatus = "CAN";
		header.EffectiveMessageStatus = "ACC";
		header.MovementHeader.BM_PaperlessInbondNum = "refNum";

		CombineAssertions(() =>
		{
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("CustomsStatus", "CAN", testItem.CustomsStatus);
			AssertEquals("MessageStatus", "ACC", testItem.MessageStatus);
			AssertEquals("LRN", "refNum", testItem.LRN);
		});
	}

	public void TestPropertiesSetFromHeader_LRN()
	{
		header.MovementHeader.BM_PaperlessInbondNum = "refNum";

		CombineAssertions(() =>
		{
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("LRN set from departureMovementHeader when departure", "refNum", testItem.LRN);

			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			header.ArrivalMovementHeader.BM_PaperlessInbondNum = "refNum2";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("LRN set from arrivalMovementHeader when arrival", "refNum2", testItem.LRN);

			header.ESNctsHeader.CEN_TNNArrival = true;
			var nctsHeaderTNN = Factory.New<NctsHeader>();
			nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			var tnnMovement = nctsHeaderTNN.MovementHeader;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;
			tnnMovement.BM_PaperlessInbondNum = "refNum3";
			header.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("LRN set from tnn's departureMovementHeader when TNN", "refNum3", testItem.LRN);

			nctsHeaderTNN.MovementHeader.BM_CustomsStatus = "MRN";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("LRN set from arrivalMovementHeader when arrival (there is a TNN declaration but SendingType is not TNN)", "refNum2", testItem.LRN);
		});
	}

	public void TestGetDefaultMessageTypeArrival_Phase4()
	{
		header.BH_HeaderType = NctsMovementType.Codes.Arrival;
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		CombineAssertions(() =>
		{
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is AVI when messageFunction is null", DeclarationMessageTypeList.Codes.NctsArrivalNotification, testItem.MessageType);

			header.CombinedMessage = true;
			testItem = new NctsHeaderMessageSendingObject(header, messageFunction: new NctsMessageFunctionSet.ArrivalNotificationMessage());
			AssertEquals("MessageType is AVO when messageFunction is ArrivalNotificationMessage and CombinedMessage is true", DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs, testItem.MessageType);

			header.CombinedMessage = false;
			testItem = new NctsHeaderMessageSendingObject(header, messageFunction: new NctsMessageFunctionSet.ArrivalNotificationMessage());
			AssertEquals("MessageType is AVI when messageFunction is ArrivalNotificationMessage and CombinedMessage is false", DeclarationMessageTypeList.Codes.NctsArrivalNotification, testItem.MessageType);

			testItem = new NctsHeaderMessageSendingObject(header, messageFunction: new NctsMessageFunctionSet.UnloadingRemarksMessage());
			AssertEquals("MessageType is OBS when messageFunction is UnloadingRemarksMessage", DeclarationMessageTypeList.Codes.NctsUnloadingRemarks, testItem.MessageType);

			testItem = new NctsHeaderMessageSendingObject(header, messageFunction: new NctsMessageFunctionSet.CombinedArrivalAndDepartureMessage());
			AssertEquals("MessageType is TNA when messageFunction is CombinedArrivalAndDepartureMessage and CombinedMessage is false", DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi, testItem.MessageType);

			header.CombinedMessage = true;
			testItem = new NctsHeaderMessageSendingObject(header, messageFunction: new NctsMessageFunctionSet.CombinedArrivalAndDepartureMessage());
			AssertEquals("MessageType is TAO when messageFunction is CombinedArrivalAndDepartureMessage and CombinedMessage is true", DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb, testItem.MessageType);

			testItem = new NctsHeaderMessageSendingObject(header, messageFunction: new NctsMessageFunctionSet.ArrivalNotificationRejectionMessage());
			AssertEquals("MessageType is AVI when messageFunction is not ArrivalNotificationMessage or UnloadingRemarksMessage or CombinedArrivalAndDepartureMessage", DeclarationMessageTypeList.Codes.NctsArrivalNotification, testItem.MessageType);
		});
	}

	public void TestGetDefaultMessageTypeArrival_Phase5()
	{
		header.BH_HeaderType = NctsMovementType.Codes.Arrival;
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		CombineAssertions(() =>
		{
			var tnnDepartureMovement = AttachTNNDepartureMovement(header);
			tnnDepartureMovement.Header.MovementHeader.BM_CustomsStatus = ZString.Empty;
			header.ArrivalMovementHeader.BM_CustomsStatus = "";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is TNN when CustomsStatus is empty and there is linked TNN Departure", DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration, testItem.MessageType);

			DetachTNNDepartureMovement(header);
			header.ArrivalMovementHeader.BM_CustomsStatus = "";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is ARV when CustomsStatus is empty", DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, testItem.MessageType);

			header.ArrivalMovementHeader.BM_CustomsStatus = "UAP";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is ADG when CustomsStatus is UAP", DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods, testItem.MessageType);

			header.ArrivalMovementHeader.BM_CustomsStatus = "ACS";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is empty when CustomsStatus not in ['',UAP]", ZString.Empty, testItem.MessageType);
		});
	}

	public void TestGetDefaultMessageTypeDeparture_Phase4()
	{
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		CombineAssertions(() =>
		{
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is DEP when header is not TIR", DeclarationMessageTypeList.Codes.NctsDeparture, testItem.MessageType);

			header.MovementHeader.BM_InBondEntryType = "TIR";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is TIR when header is TIR", DeclarationMessageTypeList.Codes.NctsTir, testItem.MessageType);
		});
	}

	public void TestGetDefaultMessageTypeDeparture_Phase5()
	{
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		CombineAssertions(() =>
		{
			header.MovementHeader.BM_CustomsStatus = "";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is DPT when CustomsStatus is empty", DeclarationMessageTypeList.Codes.Ncts5Departure, testItem.MessageType);

			header.MovementHeader.BM_CustomsStatus = "PRE";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is DPN when CustomsStatus is PRE", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, testItem.MessageType);

			header.MovementHeader.BM_CustomsStatus = "CO1";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is DPA when CustomsStatus is CO1", DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, testItem.MessageType);

			header.MovementHeader.BM_CustomsStatus = "DGP";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is DPC when CustomsStatus is DGP", DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation, testItem.MessageType);

			header.MovementHeader.BM_CustomsStatus = "ACS";
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("MessageType is empty when CustomsStatus not in ['',PRE,CO1,DGP]", ZString.Empty, testItem.MessageType);
		});
	}

	public void TestRequestDispatchList()
	{
		CombineAssertions(() =>
		{
			var requestDispatchList = testItem.RequestDispatchList;
			AssertEquals("RequestDispatchList is CodeDescriptionPairList", typeof(YesNoList), requestDispatchList.GetType());
			AssertContainsExactElementsInAnyOrder("RequestDispatchList contains the correct values", new ZString[] { "Y", "N" }, requestDispatchList.GetAllCodesZString());
		});
	}

	public void TestRequestDispatchReadOnly()
	{
		CombineAssertions(() =>
		{
			AssertEquals("RequestDispatch is readonly when there are no annexes to be sent", false, testItem.RequestDispatchInfo.ReadOnly);

			var eDoc1 = header.DocManagerInfo.AddFileOrDocument(new byte[1], "doc1.txt", "CIV");
			var pivot1 = header.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
			Factory.Save();
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("RequestDispatch is readonly when there is only one annex to be sent", true, testItem.RequestDispatchInfo.ReadOnly);

			var eDoc2 = header.DocManagerInfo.AddFileOrDocument(new byte[1], "doc2.txt", "MSC");
			var pivot2 = header.EDocPivotCollection.AddNew();
			pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;
			Factory.Save();
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("RequestDispatch is readonly when there are multiple annexes to be sent", false, testItem.RequestDispatchInfo.ReadOnly);

			var message = SetEDIMessageAndGenPivot(pivot1);
			Factory.Save();
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("RequestDispatch is readonly when there is only one annex to be sent (one annex has SNT message)", true, testItem.RequestDispatchInfo.ReadOnly);

			message.EM_Status = EDIMessage.Status.Rejected;
			Factory.Save();
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("RequestDispatch is readonly when there are multiple annexes to be sent (one annex has REJ message)", false, testItem.RequestDispatchInfo.ReadOnly);

			message.EM_Status = EDIMessage.Status.Received;
			Factory.Save();
			testItem = new NctsHeaderMessageSendingObject(header);
			AssertEquals("RequestDispatch is readonly when there is only one annex to be sent (one annex has RCV message)", true, testItem.RequestDispatchInfo.ReadOnly);
		});

		ESEDIMessage SetEDIMessageAndGenPivot(NctsCusStorageDocPivot docPivot)
		{
			var message = Factory.New<ESEDIMessage>();
			header.Messages.Add(message);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			var messagePivot = Factory.New<GenPivot>();
			messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot.XX_Relation1ID = docPivot.PK;
			messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
			messagePivot.XX_Relation2ID = message.PK;
			messagePivot.XX_Relation2TableCode = message.TablePrefix;

			return message;
		}
	}

	public void TestDefaultValues()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MessageSubType", "ORG", testItem.MessageSubType);
			AssertEquals("RequestDispatch", "Y", testItem.RequestDispatch);
		});
	}

	public void TestMessageTypeReadOnly()
	{
		CombineAssertions("MessageType is editable for NCTS5 Departure or Arrival when there are multiple message types available", () =>
		{
			AssertMessageTypeReadOnly(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS4, true);

			var tnnMovement = AttachTNNDepartureMovement(header);

			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			header.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
			header.ESNctsHeader.CEN_TNNArrival = true;
			tnnMovement.BM_CustomsStatus = ZString.Empty;
			AssertMessageTypeReadOnly(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS5, false);

			header.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
			header.ESNctsHeader.CEN_TNNArrival = true;
			tnnMovement.BM_CustomsStatus = "MRN";
			AssertMessageTypeReadOnly(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS5, true);

			header.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
			header.ESNctsHeader.CEN_TNNArrival = false;
			AssertMessageTypeReadOnly(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS5, true);

			DetachTNNDepartureMovement(header);

			header.ArrivalMovementHeader.BM_CustomsStatus = "UAP";
			AssertMessageTypeReadOnly(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS5, true);

			AssertMessageTypeReadOnly(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS4, true);

			header.MovementHeader.BM_CustomsStatus = ZString.Empty;
			header.MovementHeader.BM_AdditionalDeclarationType = ZString.Empty;
			AssertMessageTypeReadOnly(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS5, true);

			header.MovementHeader.BM_CustomsStatus = "CO1";
			AssertMessageTypeReadOnly(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS5, true);

			header.MovementHeader.BM_CustomsStatus = ZString.Empty;
			header.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			AssertMessageTypeReadOnly(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS5, true);

			header.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			AssertMessageTypeReadOnly(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS5, true);
		});

		void AssertMessageTypeReadOnly(string movementType, string phase, bool expectedMessageTypeReadOnly)
		{
			header.BH_HeaderType = movementType;
			header.BH_ApplicationCode = phase;
			var customsStatus = movementType switch
			{
				NctsMovementType.Codes.Departure => header.MovementHeader?.BM_CustomsStatus ?? ZString.Empty,
				NctsMovementType.Codes.Arrival => header.ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty,
				_ => ZString.Empty
			};
			var availableMessageTypeCount = testItem.MessageTypeList.Count;
			AssertEquals($"MessageType ReadOnly for {phase} {movementType} in status '{customsStatus}' ({availableMessageTypeCount} available MessageType codes)", expectedMessageTypeReadOnly, testItem.MessageTypeInfo.ReadOnly);
		}
	}

	public void TestMessageTypeDefaultValues() => CombineAssertions(() =>
	{
		AssertNcts5Departure(DeclarationMessageTypeList.Codes.Ncts5Departure, NctsTypeOfAdditionalDeclarationList.Codes.A);
		AssertNcts5Departure(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, ZString.Empty, "CO1");
		AssertNcts5Departure(DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, NctsTypeOfAdditionalDeclarationList.Codes.D);
		AssertNcts5Departure(DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, ZString.Empty, "PRE", false);
		AssertNcts5Departure(ZString.Empty, ZString.Empty);
		AssertNcts5Departure(DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, NctsTypeOfAdditionalDeclarationList.Codes.A, "PRE", false);

		void AssertNcts5Departure(string expectedMessageType, string addDeclType, string customsStatus = "", bool expectedReadOnly = true)
		{
			header.MovementHeader.BM_CustomsStatus = customsStatus;
			header.MovementHeader.BM_AdditionalDeclarationType = addDeclType;
			testItem = new NctsHeaderMessageSendingObject(header);

			AssertEquals($"Expected MessageType = '{expectedMessageType}' when Add. Decl. Type = '{addDeclType}' and Customs Status = '{customsStatus}'", expectedMessageType, testItem.MessageType);
			AssertEquals($"Expected MessageType {expectedMessageType} readonly when Add. Decl. Type = '{addDeclType}' and Customs Status = '{customsStatus}'", expectedReadOnly, testItem.MessageTypeInfo.ReadOnly);
		}
	});

	public void TestMessageTypeList()
	{
		var defaultList = new DeclarationMessageTypeList().GetAllCodesZString();

		CombineAssertions(() =>
		{
			AssertNcts4Departure();
			AssertNcts4Arrival();
			AssertNcts5Departure();
			AssertNcts5Arrival();
		});

		void AssertNcts4Departure()
		{
			AssertAvailableMessageTypes(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS4,
				customsStatus: string.Empty,
				expectedMessageTypes: defaultList);
		}

		void AssertNcts4Arrival()
		{
			AssertAvailableMessageTypes(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS4,
				customsStatus: string.Empty,
				expectedMessageTypes: defaultList);
		}

		void AssertNcts5Departure()
		{
			AssertAvailableMessageTypes(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS5,
				customsStatus: string.Empty,
				expectedMessageTypes: ["DPT", "DPD"]);

			AssertAvailableMessageTypes(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS5,
				customsStatus: "PRE",
				expectedMessageTypes: ["DPN", "DPM", "DPC"]);

			AssertAvailableMessageTypes(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS5,
				customsStatus: "CO1",
				expectedMessageTypes: ["DPA"]);

			AssertAvailableMessageTypes(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS5,
				customsStatus: "DGP",
				expectedMessageTypes: ["DPC"]);

			AssertAvailableMessageTypes(NctsMovementType.Codes.Departure, CusInBondApplicationCodeList.Codes.NCTS5,
				customsStatus: "XYZ",
				expectedMessageTypes: []);
		}

		void AssertNcts5Arrival()
		{
			AttachTNNDepartureMovement(header);
			AssertAvailableMessageTypes(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS5,
				customsStatus: string.Empty,
				expectedMessageTypes: ["TNN", "ARV"]);

			DetachTNNDepartureMovement(header);
			AssertAvailableMessageTypes(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS5,
				customsStatus: string.Empty,
				expectedMessageTypes: ["ARV"]);

			AssertAvailableMessageTypes(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS5,
				customsStatus: "UAP",
				expectedMessageTypes: ["ADG"]);

			AssertAvailableMessageTypes(NctsMovementType.Codes.Arrival, CusInBondApplicationCodeList.Codes.NCTS5,
				customsStatus: "ACS",
				expectedMessageTypes: []);
		}

		void AssertAvailableMessageTypes(string movementType, string phase, string customsStatus, params ZString[] expectedMessageTypes)
		{
			header.BH_HeaderType = movementType;
			header.BH_ApplicationCode = phase;

			if (movementType == NctsMovementType.Codes.Departure && header.MovementHeader is not null)
			{
				header.MovementHeader.BM_CustomsStatus = customsStatus;
			}
			else if (movementType == NctsMovementType.Codes.Arrival && header.ArrivalMovementHeader is not null)
			{
				header.ArrivalMovementHeader.BM_CustomsStatus = customsStatus;
			}
			else
			{
				Fail("Customs Status is not applicable. Check the test setup.");
			}

			if (expectedMessageTypes.Length > 0)
			{
				AssertContainsExactElementsInAnyOrder(
					$"MessageTypeList for {phase} {movementType} with CustomsStatus '{customsStatus}'",
					expectedMessageTypes,
					testItem.MessageTypeList.GetAllCodesZString());
			}
			else
			{
				AssertEquals($"MessageTypeList empty for {phase} {movementType} with CustomsStatus '{customsStatus}'",
					0, testItem.MessageTypeList.Count);
			}
		}
	}

	NctsDepartureMovementHeader AttachTNNDepartureMovement(NctsHeader arrivalHeader)
	{
		var nctsHeaderTNN = Factory.New<NctsHeader>();
		nctsHeaderTNN.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
		nctsHeaderTNN.MovementReferenceEntryNumber.CE_EntryNum = "25ES00999912345678";

		var tnnMovement = nctsHeaderTNN.MovementHeader;
		tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;
		tnnMovement.BM_MessageStatus = LogicalStatusList.Codes.Sent;

		arrivalHeader.ESNctsHeader.CEN_TNNArrival = true;
		arrivalHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

		return tnnMovement;
	}

	void DetachTNNDepartureMovement(NctsHeader arrivalHeader)
	{
		arrivalHeader.ESNctsHeader.CEN_TNNArrival = false;
		arrivalHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = ZGuid.Empty;
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var header = Factory.NewWithValidTestData<NctsHeader>();
		return new NctsHeaderMessageSendingObject(header);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewWithValidTestData<NctsHeader>();
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		Factory.Save();

		testItem = new NctsHeaderMessageSendingObject(header);
	}
	NctsHeaderMessageSendingObject testItem;
	NctsHeader header;
}
