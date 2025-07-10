using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	class DeltaIEJobDeclarationMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMotivationForInvalidationList()
		{
			SetUpCusCodeList_INVMO(Factory);
			var motivationForInvalidationListInstance = GetMessageSendingObjectForTest(Factory).Lookups.MotivationForInvalidationList;

			AssertEquals("MotivationForInvalidationList should contain data for codeType: INVMO and grouping DIE", "INV01, INV02, INV13", motivationForInvalidationListInstance.CodesAsString);
		}

		public void TestMotivationForRectificationList()
		{
			SetUpCusCodeList_RECMO(Factory);

			AssertMotivationForRectificationList("MotivationForRectificationList should contain valid data for codeType: RECMO and grouping DIE.", ZString.Empty, ZString.Empty, ZString.Empty, new string[] { FRConstants.RectificationMotivationTypes.PrelodgeRectification, "REC09", "REC10", "REC12", "REC13", "REC15", "REC16", "REC17", FRConstants.RectificationMotivationTypes.MissingDocuments, "REC19" });
			AssertMotivationForRectificationList("MotivationForRectificationList should only contain MODIF when entry status is REG.", DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered, ZString.Empty, ZString.Empty, new string[] { FRConstants.RectificationMotivationTypes.PrelodgeRectification });
			AssertMotivationForRectificationList("MotivationForRectificationList should not contain REC18 when entry status is REL, entry instruction substyle is C and entry instruction procedure is 71.", DeltaIEImportCusEntryStatusList.Codes.Released, EntrySubstyleCodePairList.Codes.C, FRConstants.ProcedureTypes.IntoBondedWarehouseProcedure, new string[] { FRConstants.RectificationMotivationTypes.PrelodgeRectification, "REC09", "REC10", "REC12", "REC13", "REC15", "REC16", "REC17", "REC19" });
			AssertMotivationForRectificationList("MotivationForRectificationList should not contain REC18 when entry status is REL, entry instruction substyle is F and entry instruction procedure is 71.", DeltaIEImportCusEntryStatusList.Codes.Released, EntrySubstyleCodePairList.Codes.F, FRConstants.ProcedureTypes.IntoBondedWarehouseProcedure, new string[] { FRConstants.RectificationMotivationTypes.PrelodgeRectification, "REC09", "REC10", "REC12", "REC13", "REC15", "REC16", "REC17", "REC19" });
			AssertMotivationForRectificationList("MotivationForRectificationList should contain REC18 when entry instruction substyle is empty even if entry status is REL and entry instruction procedure is 71.", DeltaIEImportCusEntryStatusList.Codes.Released, ZString.Empty, FRConstants.ProcedureTypes.IntoBondedWarehouseProcedure, new string[] { FRConstants.RectificationMotivationTypes.PrelodgeRectification, "REC09", "REC10", "REC12", "REC13", "REC15", "REC16", "REC17", FRConstants.RectificationMotivationTypes.MissingDocuments, "REC19" });
			AssertMotivationForRectificationList("MotivationForRectificationList should contain REC18 when entry status is not REL, even if entry substyle is C and entry instruction procedure is 71.", ZString.Empty, EntrySubstyleCodePairList.Codes.C, FRConstants.ProcedureTypes.IntoBondedWarehouseProcedure, new string[] { FRConstants.RectificationMotivationTypes.PrelodgeRectification, "REC09", "REC10", "REC12", "REC13", "REC15", "REC16", "REC17", FRConstants.RectificationMotivationTypes.MissingDocuments, "REC19" });
			AssertMotivationForRectificationList("MotivationForRectificationList should contain REC18 when entry instruction procedure is not 71, even if entry status is REL and entry instruction substyle is C.", DeltaIEImportCusEntryStatusList.Codes.Released, EntrySubstyleCodePairList.Codes.C, ZString.Empty, new string[] { FRConstants.RectificationMotivationTypes.PrelodgeRectification, "REC09", "REC10", "REC12", "REC13", "REC15", "REC16", "REC17", FRConstants.RectificationMotivationTypes.MissingDocuments, "REC19" });
		}

		void AssertMotivationForRectificationList(ZString comment, ZString entryStatus, ZString instructionSubStyle, ZString instructionProcedure, string[] expectedMotivationList)
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_SubStyle = instructionSubStyle;
			instruction.CEI_Procedure = instructionProcedure;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.CH_EntryStatus = entryStatus;

			var deltaIEMessageSendingObject = new DeltaIEJobDeclarationMessageSendingObject(entry);
			AssertContainsExactElementsInAnyOrder(comment, expectedMotivationList, deltaIEMessageSendingObject.Lookups.MotivationForRectificationList.GetAllCodes());
		}

		public void TestMotivationList()
		{
			DeltaIEJobDeclarationMessageSendingObjectLookupsTest.SetUpCusCodeList_INVMO(Factory);
			DeltaIEJobDeclarationMessageSendingObjectLookupsTest.SetUpCusCodeList_RECMO(Factory);
			var testMessageSendingObject = GetMessageSendingObjectForTest(Factory);
			CombineAssertions(() =>
			{
				testMessageSendingObject.MessageType = DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;
				AssertEquals("MotivationList should be MotivationForRectificationList for IE413.", testMessageSendingObject.Lookups.MotivationForRectificationList, testMessageSendingObject.Lookups.MotivationList);

				testMessageSendingObject.MessageType = DeltaIESendMessageSubTypeList.Codes.Invalidation;
				AssertEquals("MotivationList should be MotivationForInvalidationList for IE414.", testMessageSendingObject.Lookups.MotivationForInvalidationList, testMessageSendingObject.Lookups.MotivationList);
			});
		}

		public void TestMessageSubTypeList()
		{
			var sendingObject = GetMessageSendingObjectForTest(Factory);
			AssertContainsExactElementsInExactOrder(new[] { DeltaIESendMessageSubTypeList.Codes.ImportDeclaration }, sendingObject.Lookups.MessageSubTypeList.GetAllCodes());

			sendingObject.Header.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;
			AssertContainsExactElementsInExactOrder(new[] { DeltaIESendMessageSubTypeList.Codes.PresentationNotification, DeltaIESendMessageSubTypeList.Codes.AmendmentRequest, DeltaIESendMessageSubTypeList.Codes.Invalidation }, sendingObject.Lookups.MessageSubTypeList.GetAllCodes());

			sendingObject.Header.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Amending;
			AssertContainsExactElementsInExactOrder(new[] { DeltaIESendMessageSubTypeList.Codes.AmendmentRequest }, sendingObject.Lookups.MessageSubTypeList.GetAllCodes());

			sendingObject.Header.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationRejected;
			AssertContainsExactElementsInExactOrder(new[] { DeltaIESendMessageSubTypeList.Codes.ImportDeclaration, DeltaIESendMessageSubTypeList.Codes.AmendmentRequest }, sendingObject.Lookups.MessageSubTypeList.GetAllCodes());
		}

		internal static DeltaIEJobDeclarationMessageSendingObject GetMessageSendingObjectForTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var deltaIEMessageSending = new DeltaIEJobDeclarationMessageSendingObject(entry);
			return deltaIEMessageSending;
		}

		internal static void SetUpCusCodeList_INVMO(BusinessObjectFactory factory)
		{
			var refCusCodeListType = UniversalReferenceConstants.RefCusCodeListTypes.Codes.MotivationForInvalidationRequest;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType(refCusCodeListType, "Motivation for Invalidation Request");
			helper.CreateCusCodeList("DIE", refCusCodeListType, "INV01", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "INV02", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "INV13", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("FR", refCusCodeListType, "INV03", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "INV04", ZDateTime.Now.AddDays(-10), ZDateTime.Now.AddDays(-5));
			factory.Save();
		}

		internal static void SetUpCusCodeList_RECMO(BusinessObjectFactory factory)
		{
			var refCusCodeListType = UniversalReferenceConstants.RefCusCodeListTypes.Codes.MotivationForRectificationRequest;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType(refCusCodeListType, "Motivation for Rectification Request");
			helper.CreateCusCodeList("FR", refCusCodeListType, "REC01", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC02", ZDateTime.Now.AddDays(-10), ZDateTime.Now.AddDays(-5));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC09", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC10", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC12", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC13", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC15", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC16", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC17", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC18", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "REC19", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("DIE", refCusCodeListType, "MODIF", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			factory.Save();
		}
	}
}
