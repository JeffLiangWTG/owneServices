using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Module.Testing
{
	public static class ModuleTestHelper
	{
		public static void AssertMessageSent(ZString entryName, CusEntryHeader entryHeader, bool isATC)
		{
			var messages = entryHeader.Messages;
			Assertion.AssertEquals("1 EDIMessage was created for " + entryName, 1, messages.Count);
			var message = messages[0];
			Assertion.AssertEquals("Sent message for " + entryName + " has correct EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
			Assertion.AssertEquals("Sent message for " + entryName + " has correct EM_MessageType", "IQU", message.EM_MessageType);
			Assertion.AssertEquals("Sent message for " + entryName + " has correct EM_MessageSubType", "GUA", message.EM_MessageSubType);

			if (isATC)
			{
				Assertion.AssertContains("Sent IQU message for " + entryName + " is for canary islands", "DatosEnATC", message.EM_MessageText);
			}
			else
			{
				Assertion.AssertNotContains("Sent IQU message for " + entryName + " is not for canary islands", "DatosEnATC", message.EM_MessageText);
			}
		}

		public static JobDeclaration GetDeclarationWith2Entries(BusinessObjectFactory factory, string messageType = Common.Shared.SharedJobMessageTypeList.Codes.Import, string entryInstructionSubStyle1 = EntrySubStyleList.Codes.A, string entryInstructionSubStyle2 = EntrySubStyleList.Codes.B, string entryInstructionStyle1 = "", string entryInstructionStyle2 = "")
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = "BLT";

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = entryInstructionSubStyle1;
			entryInstruction1.CEI_Style = entryInstructionStyle1;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = entryInstructionSubStyle2;
			entryInstruction2.CEI_Style = entryInstructionStyle2;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}

		public static void AddGuaranteeToDeclaration(JobDeclaration declaration, ZGuid entryInstructionPK, ZString guaranteeCode)
			=> GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstructionPK, guaranteeCode));

		public static void CreateGuaranteeHeaderDetail(BusinessObjectFactory factory, ZString guaranteeCode, bool addOBLTransaction = true, bool withOldEndDate = false)
		{
			var holder = factory.NewWithValidTestData<OrgHeader>();

			var guaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = guaranteeCode;
			guaranteeHeader.CPH_OH_PermitHolder = holder.PK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.IMP;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

			if (addOBLTransaction)
			{
				AddOBLTransaction(guaranteeHeader, 1000m);
			}

			if (withOldEndDate)
			{
				guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(-10);
			}
		}

		public static void AddOBLTransaction(CusGuaranteeHeader guaranteeHeader, ZDecimal value)
		{
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "OPENING";
			transaction.CPL_TranValue = value;
			transaction.CPL_Comment = "OPENING";
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
		}
	}
}
