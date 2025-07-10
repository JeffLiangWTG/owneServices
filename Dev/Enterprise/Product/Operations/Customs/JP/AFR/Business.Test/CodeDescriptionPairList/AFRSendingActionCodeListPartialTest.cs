using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class AFRSendingActionCodeListPartialTest : TestCaseWithFactory
	{
		public void TestGetBillActionCodeList()
		{
			foreach (var amendmentActionCode in new[] { ActionCode.AmendingAdd, ActionCode.AmendingDelete, ActionCode.AmendingUpdate })
			{
				var amendmentList = AFRSendingActionCodeList.GetActionCodeList(Factory, amendmentActionCode);
				AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, amendmentActionCode), amendmentList);
				AssertEquals(3, amendmentList.Count);
				AssertEquals(AFRSendingActionCodeList.Codes.Add, AFRSendingActionCodeList.Descriptions.Add, amendmentList.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Add));
				AssertEquals(AFRSendingActionCodeList.Codes.Delete, AFRSendingActionCodeList.Descriptions.Delete, amendmentList.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Delete));
				AssertEquals(AFRSendingActionCodeList.Codes.Update, AFRSendingActionCodeList.Descriptions.Update, amendmentList.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Update));
			}

			var actionCode = ActionCode.CorrectMasterInformation;
			var list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(2, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Delete, AFRSendingActionCodeList.Descriptions.Delete, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Delete));
			AssertEquals(AFRSendingActionCodeList.Codes.Update, AFRSendingActionCodeList.Descriptions.Update, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Update));

			actionCode = ActionCode.ChangeDepartureTimeAfterATD;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(2, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Delete, AFRSendingActionCodeList.Descriptions.Delete, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Delete));
			AssertEquals(AFRSendingActionCodeList.Codes.Update, AFRSendingActionCodeList.Descriptions.Update, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Update));

			actionCode = ActionCode.CorrectVesselInformationByAmendment;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(1, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Add, AFRSendingActionCodeList.Descriptions.Add, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Add));

			actionCode = ActionCode.CorrectVesselInformationByRegistration;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(1, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Register, AFRSendingActionCodeList.Descriptions.Register, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Register));

			actionCode = ActionCode.RegisterCompletionByAmendment;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(4, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Register, AFRSendingActionCodeList.Descriptions.Register, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Register));
			AssertEquals(AFRSendingActionCodeList.Codes.Add, AFRSendingActionCodeList.Descriptions.Add, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Add));
			AssertEquals(AFRSendingActionCodeList.Codes.Delete, AFRSendingActionCodeList.Descriptions.Delete, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Delete));
			AssertEquals(AFRSendingActionCodeList.Codes.Update, AFRSendingActionCodeList.Descriptions.Update, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Update));

			actionCode = ActionCode.RegisterCompletionByRegistration;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(4, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Register, AFRSendingActionCodeList.Descriptions.Register, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Register));
			AssertEquals(AFRSendingActionCodeList.Codes.Add, AFRSendingActionCodeList.Descriptions.Add, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Add));
			AssertEquals(AFRSendingActionCodeList.Codes.Delete, AFRSendingActionCodeList.Descriptions.Delete, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Delete));
			AssertEquals(AFRSendingActionCodeList.Codes.Update, AFRSendingActionCodeList.Descriptions.Update, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Update));

			actionCode = ActionCode.Registering;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(4, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Register, AFRSendingActionCodeList.Descriptions.Register, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Register));
			AssertEquals(AFRSendingActionCodeList.Codes.Add, AFRSendingActionCodeList.Descriptions.Add, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Add));
			AssertEquals(AFRSendingActionCodeList.Codes.Delete, AFRSendingActionCodeList.Descriptions.Delete, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Delete));
			AssertEquals(AFRSendingActionCodeList.Codes.Update, AFRSendingActionCodeList.Descriptions.Update, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Update));

			actionCode = ActionCode.NewBill;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(2, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Register, AFRSendingActionCodeList.Descriptions.Register, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Register));
			AssertEquals(AFRSendingActionCodeList.Codes.Add, AFRSendingActionCodeList.Descriptions.Add, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Add));

			actionCode = ActionCode.ChangeDepartureTimeAfterATD;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(2, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Delete, AFRSendingActionCodeList.Descriptions.Delete, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Delete));
			AssertEquals(AFRSendingActionCodeList.Codes.Update, AFRSendingActionCodeList.Descriptions.Update, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Update));

			actionCode = ActionCode.ReRegisterMasterAfterATD;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(2, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Register, AFRSendingActionCodeList.Descriptions.Register, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Register));
			AssertEquals(AFRSendingActionCodeList.Codes.Add, AFRSendingActionCodeList.Descriptions.Add, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Add));

			actionCode = ActionCode.ReRegisterMasterBeforeATD;
			list = AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode);
			AssertEquals("Should Be Cached", AFRSendingActionCodeList.GetActionCodeList(Factory, actionCode), list);
			AssertEquals(2, list.Count);
			AssertEquals(AFRSendingActionCodeList.Codes.Register, AFRSendingActionCodeList.Descriptions.Register, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Register));
			AssertEquals(AFRSendingActionCodeList.Codes.Add, AFRSendingActionCodeList.Descriptions.Add, list.GetDescriptionFromCode(AFRSendingActionCodeList.Codes.Add));
		}
	}
}
