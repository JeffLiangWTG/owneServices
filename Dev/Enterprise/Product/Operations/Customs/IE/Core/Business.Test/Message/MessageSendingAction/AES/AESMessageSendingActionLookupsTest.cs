using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AESMessageSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestSendingActionTypeList()
		{
			declaration.JE_MessageType = string.Empty;
			AssertNoExceptionThrown("No exception for empty JE_MessageType", () => _ = lookups.SendingActionTypeList);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("SendingActionTypeList.Code", "511, 513, 514, 515, X13", lookups.SendingActionTypeList.CodesAsString);
			var expList = lookups.SendingActionTypeList;

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertEquals("SendingActionTypeList.Code", "570, 573, 614", lookups.SendingActionTypeList.CodesAsString);
			var rexList = lookups.SendingActionTypeList;

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("SendingActionTypeList.Code", "613, 614, 615", lookups.SendingActionTypeList.CodesAsString);
			var exsList = lookups.SendingActionTypeList;

			var anotherJob = Factory.New<JobDeclaration>();
			var anotherLookups = new AESMessageSendingAction((CusEntryHeader)anotherJob.ActiveEntryHeaders.AddNew()).Lookups;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertSame("EXP, List should have been cached.", expList, anotherLookups.SendingActionTypeList);
			anotherJob.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertSame("REX, List should have been cached.", rexList, anotherLookups.SendingActionTypeList);
			anotherJob.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertSame("EXS, List should have been cached.", exsList, anotherLookups.SendingActionTypeList);
		}

		public void TestSendingActionTypeListForDisplay()
		{
			declaration.JE_MessageType = string.Empty;
			AssertNoExceptionThrown("No exception for empty JE_MessageType", () => _ = lookups.SendingActionTypeListForDisplay);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var expDisplayList = lookups.SendingActionTypeListForDisplay;
			AssertEquals("EXP - SendingActionTypeListForDisplay.Code", "CC511C, CC513C, CC514C, CC515C, EX513", expDisplayList.CodesAsString);
			AssertList("EXP", lookups.SendingActionTypeList, expDisplayList);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			var rexDisplayList = lookups.SendingActionTypeListForDisplay;
			AssertEquals("SendingActionTypeListForDisplay.Code", "CC570C, CC573C, CC614C", rexDisplayList.CodesAsString);
			AssertList("REX", lookups.SendingActionTypeList, rexDisplayList);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			var exsDisplayList = lookups.SendingActionTypeListForDisplay;
			AssertEquals("SendingActionTypeListForDisplay.Code", "CC613C, CC614C, CC615C", exsDisplayList.CodesAsString);
			AssertList("EXS", lookups.SendingActionTypeList, exsDisplayList);

			var anotherJob = Factory.New<JobDeclaration>();
			var anotherLookups = new AESMessageSendingAction((CusEntryHeader)anotherJob.ActiveEntryHeaders.AddNew()).Lookups;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertSame("EXP, List should have been cached.", expDisplayList, anotherLookups.SendingActionTypeListForDisplay);
			anotherJob.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertSame("REX, List should have been cached.", rexDisplayList, anotherLookups.SendingActionTypeListForDisplay);
			anotherJob.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertSame("EXS, List should have been cached.", exsDisplayList, anotherLookups.SendingActionTypeListForDisplay);
		}

		void AssertList(string messageType, CodeDescriptionPairList list, CodeDescriptionPairList displayList)
		{
			var processingList = new CodeDescriptionPairList(list);
			AssertEquals($"{messageType} - SendingActionTypeListForDisplay should have element as SendingActionTypeList", processingList.Count, displayList.Count);
			displayList.Cast<ICodeDescription>().ForEach(x =>
			{
				var mappingCode = x.PK.ToString();
				AssertNotEquals($"{messageType} - {x.Code} should have PK value", ZString.Empty, mappingCode);
				var matched = processingList[mappingCode];
				AssertNotNull($"{messageType} - {x.Code}'s PK Value '{mappingCode}' should exists in SendingActionTypeList", matched);
				processingList.Remove(matched);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			sendingAction = new AESMessageSendingAction(entryHeader);
			lookups = (AESMessageSendingActionLookups)sendingAction.Lookups;
		}

		JobDeclaration declaration;
		AESMessageSendingAction sendingAction;
		AESMessageSendingActionLookups lookups;
	}
}
