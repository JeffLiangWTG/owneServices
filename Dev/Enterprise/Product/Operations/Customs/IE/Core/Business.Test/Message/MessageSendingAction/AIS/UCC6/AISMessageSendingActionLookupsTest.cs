using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AISMessageSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestSendingActionTypeList()
		{
			(_, _, _, var lookups) = GetNewLookups(Factory);
			AssertEquals("SendingActionTypeList.Code", "413, 414, 415, 432, 433", lookups.SendingActionTypeList.CodesAsString);

			var anotherJob = Factory.New<JobDeclaration>();
			var anotherLookups = new AISMessageSendingAction((CusEntryHeader)anotherJob.ActiveEntryHeaders.AddNew()).Lookups;

			AssertSame("List should have been cached.", lookups.SendingActionTypeList, anotherLookups.SendingActionTypeList);
		}

		public void TestSendingActionTypeListForDisplay()
		{
			(_, _, _, var lookups) = GetNewLookups(Factory);
			var list = lookups.SendingActionTypeListForDisplay;
			AssertEquals("SendingActionTypeList.Code", "IM413, IM414, IM415, IM432, IM433", list.CodesAsString);
			var processingList = new CodeDescriptionPairList(lookups.SendingActionTypeList);
			AssertEquals("SendingActionTypeListForDisplay should have element as SendingActionTypeList", processingList.Count, list.Count);
			list.Cast<ICodeDescription>().ForEach(x =>
			{
				var mappingCode = x.PK.ToString();
				AssertNotEquals($"{x.Code} should have PK value", ZString.Empty, mappingCode);
				var matched = processingList[mappingCode];
				AssertNotNull($"{x.Code}'s PK Value '{mappingCode}' should exists in SendingActionTypeList", matched);
				processingList.Remove(matched);
			});

			var anotherJob = Factory.New<JobDeclaration>();
			var anotherLookups = new AISMessageSendingAction((CusEntryHeader)anotherJob.ActiveEntryHeaders.AddNew()).Lookups;

			AssertSame("List should have been cached.", list, anotherLookups.SendingActionTypeListForDisplay);
		}

		static (JobDeclaration declaration, CusEntryHeader entry, AISMessageSendingAction sendingAction, AISMessageSendingActionLookups lookups) GetNewLookups(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var sendingAction = new AISMessageSendingAction(entryHeader);
			var lookups = (AISMessageSendingActionLookups)sendingAction.Lookups;
			return (declaration, entryHeader, sendingAction, lookups);
		}
	}
}
