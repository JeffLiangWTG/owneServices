using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DeclarationMessageSendingObjectLookups))]
sealed class ExportDeclarationMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMessageTypeList() => CombineAssertions(() =>
	{
		SendingObject.Header.Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		SendingObject.Header.CH_EntryStatus = ZString.Empty;
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NE015", SendingObject.Lookups.MessageTypeList.CodesAsString);

		SendingObject.Header.CH_EntryStatus = Common.Shared.EntryStatusList.Codes.Clear;
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NE013, NE014, NC123", SendingObject.Lookups.MessageTypeList.CodesAsString);

		SendingObject.Header.CH_EntryStatus = Common.Shared.EntryStatusList.Codes.Cancelled;
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", ZString.Empty, SendingObject.Lookups.MessageTypeList.CodesAsString);

		SendingObject.Header.CH_EntryStatus = "Tst";
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NE013, NE014", SendingObject.Lookups.MessageTypeList.CodesAsString);

		SendingObject.Header.MovementReferenceNumberSetter("1");

		SendingObject.Header.CH_EntryStatus = AdditionalCHEntryStatusList.Codes.CustomsAssessmentDecision;
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NE069, NC016", SendingObject.Lookups.MessageTypeList.CodesAsString);

		SendingObject.Header.CH_EntryStatus = Common.Shared.EntryStatusList.Codes.Clear;
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NE013, NE014, NC123, NC016", SendingObject.Lookups.MessageTypeList.CodesAsString);

		SendingObject.Header.CH_EntryStatus = Common.Shared.EntryStatusList.Codes.Cancelled;
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NC016", SendingObject.Lookups.MessageTypeList.CodesAsString);

		SendingObject.Header.CH_EntryStatus = "Tst";
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NE013, NE014, NC016", SendingObject.Lookups.MessageTypeList.CodesAsString);

		SendingObject.Header.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		SendingObject.Header.CH_EntryStatus = ZString.Empty;

		SendingObject.Header.Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', JE_MessageSubType='{SendingObject.Header.Declaration.JE_MessageSubType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NC123", SendingObject.Lookups.MessageTypeList.CodesAsString);
		AssertExportDeclarationActivationMessageList();

		SendingObject.Header.Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		SendingObject.Header.CH_EntryStatus = ZString.Empty;
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', JE_MessageSubType='{SendingObject.Header.Declaration.JE_MessageSubType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NE130", SendingObject.Lookups.MessageTypeList.CodesAsString);
		AssertExportDeclarationActivationMessageList();

		SendingObject.Header.Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		SendingObject.Header.CH_EntryStatus = SwissCustomsConstants.CustomsStatusCodes.SubmittedToTaxud;
		AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', JE_MessageSubType='{SendingObject.Header.Declaration.JE_MessageSubType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", "NE130", SendingObject.Lookups.MessageTypeList.CodesAsString);
		AssertExportDeclarationActivationMessageList();

		void AssertExportDeclarationActivationMessageList()
		{
			SendingObject.Header.CH_EntryStatus = Common.Shared.EntryStatusList.Codes.Clear;
			AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', JE_MessageSubType='{SendingObject.Header.Declaration.JE_MessageSubType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", ZString.Empty, SendingObject.Lookups.MessageTypeList.CodesAsString);

			SendingObject.Header.CH_EntryStatus = Common.Shared.EntryStatusList.Codes.Cancelled;
			AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', JE_MessageSubType='{SendingObject.Header.Declaration.JE_MessageSubType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", ZString.Empty, SendingObject.Lookups.MessageTypeList.CodesAsString);

			SendingObject.Header.CH_EntryStatus = "Tst";
			AssertEquals($"JE_MessageType='{SendingObject.Header.Declaration.JE_MessageType}', JE_MessageSubType='{SendingObject.Header.Declaration.JE_MessageSubType}', CH_EntryStatus='{SendingObject.Header.CH_EntryStatus}', MRN='{SendingObject.Header.MovementReferenceNumber}'", ZString.Empty, SendingObject.Lookups.MessageTypeList.CodesAsString);
		}
	});

	public void TestCorrectionReasonList() => CombineAssertions(() =>
	{
		var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
		refDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		CreateCodeList(UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1053, "53A", "53B");
		CreateCodeList(UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1054, "54A", "54B");
		CreateCodeList(UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1057, "57A", "57B");
		Factory.Save();

		AssertEquals($"MessageType='{SendingObject.MessageType}'", 0, SendingObject.Lookups.CorrectionReasonList.Count);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NE013;
		AssertEquals($"MessageType='{SendingObject.MessageType}'", "53A, 53B", SendingObject.Lookups.CorrectionReasonList.CodesAsString);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NE015;
		AssertEquals($"MessageType='{SendingObject.MessageType}'", 0, SendingObject.Lookups.CorrectionReasonList.Count);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NE014;
		AssertEquals($"MessageType='{SendingObject.MessageType}'", "54A, 54B", SendingObject.Lookups.CorrectionReasonList.CodesAsString);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NE069;
		AssertEquals($"MessageType='{SendingObject.MessageType}'", "57A, 57B", SendingObject.Lookups.CorrectionReasonList.CodesAsString);

		void CreateCodeList(string codeList, params string[] values)
		{
			refDataHelper.CreateNewOrGetExistingCusCodeType(codeList, codeList, Core.Constants.CountryCodes.Switzerland);
			foreach (var value in values)
			{
				refDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, codeList, value, value, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			}
		}
	});

	public void TestNextProcedureList() => CombineAssertions(() =>
	{
		SendingObject.Header.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		RefCusCodeTestHelper.CreateNextProcedureList(Factory);

		var nextProcedureList = SendingObject.Lookups.NextProcedureList;
		AssertEquals("List Codes", "1, 2, 3, 4", SendingObject.Lookups.NextProcedureList.CodesAsString);
		AssertSame("cached", nextProcedureList, SendingObject.Lookups.NextProcedureList);
	});

	ExportDeclarationMessageSendingObject SendingObject => sendingObject ??= CreateMessageSendingObject();
	ExportDeclarationMessageSendingObject sendingObject;

	ExportDeclarationMessageSendingObject CreateMessageSendingObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var header = declaration.Invoices.AddNew();
		var invoiceLine = header.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		DoMerge(declaration);
		var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
		var sendingObjectParent = new ExportDeclarationMessageSendingObjectParent(declaration);
		return new ExportDeclarationMessageSendingObject(sendingObjectParent, entryHeader);
	}

	static void DoMerge(BaseJobDeclaration declaration)
	{
		var sendsMessagesToCustomsShutterUpperer = new SendsMessagesToCustomsShutterUpperer(throwExceptionOnInvalidOperation: false);
		sendsMessagesToCustomsShutterUpperer.AnswerToContinueWithAction = true;
		declaration.DoMerge(sendsMessagesToCustomsShutterUpperer);
	}
}
