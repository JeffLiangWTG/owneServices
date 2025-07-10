using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using DeclarationApplicationCodeList = Enterprise.Customs.GB.Registry.Business.DeclarationApplicationCodeList;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	public class CdsDeclarationTypeCodeHelperTests : TestCaseWithFactory
	{
		[TestDate(2015, 8, 22)]
		public void TestDeclarationTypeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_MessageType = "IMP";
			declaration.ZG_Gateway = GatewayList.Codes.CDS;
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "H1";
			entryInstruction1.CEI_SubStyle = "A";
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "H2";
			entryInstruction2.CEI_SubStyle = "C";
			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invLine2 = declaration.Invoices[0].InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction1.PK;
			invLine2.JI_CEI = entryInstruction2.PK;

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = declaration.CompanyPK;
			password.Badge = declaration.JE_CustomsProfile;
			password.EORI = declaration.DeclarantTraderId;
			password.StatusMessage = "Status Message";
			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = new ZDate(2015, 9, 1);
			password.GP_IssueDate = new ZDate(2014, 3, 20);
			password.IsTokenForCDS = true;

			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = shutUp;
			var entry1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader));
			entry1.CH_CEI_Instruction = entryInstruction1.PK;
			var entry2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader));
			entry2.CH_CEI_Instruction = entryInstruction2.PK;
			Factory.Save();
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);

			var decWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			var sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(shutUp);

			AssertEquals(1, entry1.Messages.Count);

			AssertEquals(1, entry2.Messages.Count);

			AssertContains("<TypeCode>IMA</TypeCode>", entry1.Messages[0].EM_MessageText);
			AssertContains("<TypeCode>IMC</TypeCode>", entry2.Messages[0].EM_MessageText);

			var decTypeCode1 = CdsDeclarationTypeCodeHelper.GetCdsDeclarationTypeCode(entry1);
			AssertContains(decTypeCode1, entry1.EntryTypeFriendlyName);
			var decTypeCode2 = CdsDeclarationTypeCodeHelper.GetCdsDeclarationTypeCode(entry2);
			AssertContains(decTypeCode2, entry2.EntryTypeFriendlyName);
		}
	}
}
