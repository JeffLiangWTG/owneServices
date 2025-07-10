using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObject))]
	public class JobDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClearAmendReasonIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = "ACC";
			entry.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			entry.ZG_AmendmentReasonCode = "DEF";
			entry.CH_CustomsMessageRemarks = "Amendment";
			entry.CH_BGMReference = "1GB945390992000-B00001002";

			Factory.Save();
			var testItem = new JobDeclarationMessageSendingObject(entry);
			AssertEquals("DEF", testItem.ChangeAcknowledgementIndicator);
			AssertEquals("Amendment", testItem.VOCReason);

			testItem.MessageType = "NEW";
			AssertEquals(ZString.Empty, testItem.ChangeAcknowledgementIndicator);
			AssertEquals(ZString.Empty, testItem.VOCReason);
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = "ACC";
			entry.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			entry.ZG_AmendmentReasonCode = "DEF";
			entry.CH_CustomsMessageRemarks = "Amendment";
			entry.CH_BGMReference = "1GB945390992000-B00001002";
			entry.LRN = "HYEDUKCMT0000000000006";

			Factory.Save();
			var testItem = new JobDeclarationMessageSendingObject(entry);

			AssertEquals("AMD", testItem.MessageType);
			AssertEquals("ACC", testItem.EntryStatus);
			AssertEquals("ABC", testItem.MovementReferenceNumber);
			AssertEquals("DEF", testItem.ChangeAcknowledgementIndicator);
			AssertEquals("Amendment", testItem.VOCReason);
			AssertEquals("HYEDUKCMT0000000000006", testItem.LocalReferenceNumber);
			AssertEquals("1GB945390992000-B00001002", testItem.BGMReference);
		}

		public void TestMessageTypesList()
		{
			#region CDS Imports:

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = "CDW";
			entry.CH_MessageType = "IMP";
			entry.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			entry.ZG_AmendmentReasonCode = "DEF";
			entry.CH_CustomsMessageRemarks = "Amendment";
			entry.CH_BGMReference = "1GB945390992000-B00001002";

			var testItem = new JobDeclarationMessageSendingObject(entry);

			AssertEquals(6, testItem.MessageTypesList.Count);
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.NewDeclaration));
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.AmendDeclaration));
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.CancelDeclaration));
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.NilAmendment));
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.FecChallenge));
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.ArrivalNotification));

			#endregion CDS Imports.

			#region CDS Exports:

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "EXP";
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = "CDW";
			entry.CH_MessageType = "EXP";
			entry.MovementReferenceNumberSetter("XYZ", ZDateTime.BrettsBirthday);
			entry.ZG_AmendmentReasonCode = "DEF";
			entry.CH_CustomsMessageRemarks = "Amendment";
			entry.CH_BGMReference = "1GB945390992001-B00001003";

			testItem = new JobDeclarationMessageSendingObject(entry);

			AssertEquals(9, testItem.MessageTypesList.Count);
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.NewDeclaration));
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.AmendDeclaration));
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.CancelDeclaration));
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.FecChallenge));
			Assert(testItem.MessageTypesList.ContainsCode(CDSEDIMessageTypeList.Codes.MasterQueryDeclaration));
			AssertEquals("A nil amendment to confirm as credible values that were previously challenged or to confirm as correct an entry which has previously had an amendment request rejected. This request will 'point to' the first item's gross mass.", testItem.MessageTypesList.GetDescriptionFromCode(CDSEDIMessageTypeList.Codes.FecChallenge));
			Assert(testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.Associate));
			Assert(testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.Disassociate));
			Assert(testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.Close));
			Assert(testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.QueryDeclaration));
			Assert(!testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation));
			Assert(!testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.DepartureFromLocation));
			Assert(!testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation));

			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting
				{
					BadgeCode = "ABC",
					RL_PortCode = "GBLBA",
					Direction = "IMP",
					CSPCode = "CCSUK",
					MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk,
					ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
				}
			}))
			{
				using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
				{
					new CredentialsSetting
					{
						BadgeCode = "ABC",
						PIMA = "CUKFFW98000",
						Printer = "CUKFFW98000ZPE",
						Company = "Role",
						Username = "Username",
						Password = "Password",
						IsMaritimeLoader = true
					}
				}))
				{
					declaration.JE_CustomsProfile = "ABC";
					testItem = new JobDeclarationMessageSendingObject(entry);
					AssertEquals(9, testItem.MessageTypesList.Count);
					Assert(!testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation));
					Assert(!testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.DepartureFromLocation));
					Assert(!testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation));

					var badges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
					var badgeCcsuk = badges.AddNew();
					badgeCcsuk.BadgeCode = "XDC";
					badgeCcsuk.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
					badgeCcsuk.RL_PortCode = "GBLBA";
					badgeCcsuk.Direction = "EXP";
					badgeCcsuk.ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
					GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
					var credential = CredentialsSetting.GetCredentialsForBadge(badgeCcsuk.BadgeCode, GlbBranch.CurrentBranch.PK.ToGuid());
					AssertNull(credential);
					var cred = new CredentialsSetting();
					cred.BadgeCode = "XDC";
					cred.Company = "XDC";
					cred.Printer = "CUKFFW98000ZPE" + badgeCcsuk.BadgeCode;
					cred.PIMA = "CUKFFW98000";
					cred.Company = "Role";
					cred.Username = "Username";
					cred.Password = "Password";
					cred.IsMaritimeLoader = true;
					var creds = new CredentialsSettingCollection();
					creds.Add(cred);
					GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);

					declaration.JE_CustomsProfile = "XDC";
					using (GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						testItem = new JobDeclarationMessageSendingObject(entry);
						var isDEPOrLoaderAllowedByRegistry = GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.Value;
						AssertEquals(12, testItem.MessageTypesList.Count);
						Assert(testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation));
						Assert(testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.DepartureFromLocation));
						Assert(testItem.MessageTypesList.ContainsCode(GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation));
					}
				}
			}
			#endregion CDS Exports.
		}

		public void TestShouldSendDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var testItem = new JobDeclarationMessageSendingObject(entry);
			Assert("Single Entry - ShouldSend must be true", testItem.ShouldSend);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			testItem = new JobDeclarationMessageSendingObject(entry);
			var testItem2 = new JobDeclarationMessageSendingObject(entry2);
			Assert("Multiple Entries - No MRN so ShouldSend must be true", testItem.ShouldSend);
			Assert("Multiple Entries - No MRN so ShouldSend must be true", testItem2.ShouldSend);

			entry.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			entry.CH_EntryStatus = "ACC";
			entry2.CH_EntryStatus = "";

			testItem = new JobDeclarationMessageSendingObject(entry);
			testItem2 = new JobDeclarationMessageSendingObject(entry2);
			Assert("Multiple Entries - Has MRN and is Pre-lodged so ShouldSend must be false", !testItem.ShouldSend);
			Assert("Multiple Entries - No MRN so ShouldSend must be true", testItem2.ShouldSend);

			entry2.CH_EntryStatus = "ACC";
			testItem = new JobDeclarationMessageSendingObject(entry);
			testItem2 = new JobDeclarationMessageSendingObject(entry2);
			Assert("Multiple Entries - Is Pre-lodged but not cleared so ShouldSend must be true", testItem.ShouldSend);
			Assert("Multiple Entries -  No MRN so ShouldSend must be true", testItem2.ShouldSend);

			entry2.MovementReferenceNumberSetter("DEF", ZDateTime.BrettsBirthday);
			entry2.CH_EntryStatus = "CLR";
			testItem = new JobDeclarationMessageSendingObject(entry);
			testItem2 = new JobDeclarationMessageSendingObject(entry2);
			Assert("Multiple Entries - Is Pre-lodged but not cleared so ShouldSend must be true", testItem.ShouldSend);
			Assert("Multiple Entries - Is Pre-lodged and cleared so ShouldSend must be false", !testItem2.ShouldSend);

			entry.CH_EntryStatus = "TAX";
			entry2.CH_EntryStatus = "";
			testItem = new JobDeclarationMessageSendingObject(entry);
			Assert("Multiple Entries - Is not Pre-lodged and tax has been calculated so ShouldSend must be false", !testItem.ShouldSend);
		}

		public void TestMessageTypeDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var testItem = new JobDeclarationMessageSendingObject(entry);
			AssertEquals("No MRN- MessageType should be NEW", CDSEDIMessageTypeList.Codes.NewDeclaration, testItem.MessageType);

			entry.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			testItem = new JobDeclarationMessageSendingObject(entry);
			AssertEquals("Has MRN, not pre-lodged - MessageType should be NEW", CDSEDIMessageTypeList.Codes.NewDeclaration, testItem.MessageType);

			entry.CH_EntryStatus = "ACC";
			testItem = new JobDeclarationMessageSendingObject(entry);
			AssertEquals("Has MRN, pre-lodged, not clear - MessageType should be AMEND", CDSEDIMessageTypeList.Codes.AmendDeclaration, testItem.MessageType);

			entry.CH_EntryStatus = "CLR";
			testItem = new JobDeclarationMessageSendingObject(entry);
			AssertEquals("Has MRN, pre-lodged, clear - MessageType should be CANCEL", CDSEDIMessageTypeList.Codes.CancelDeclaration, testItem.MessageType);
		}

		public void TestAmendmentReasonCodeDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var testItem = new JobDeclarationMessageSendingObject(entry);
			Assert(testItem.ChangeAcknowledgementIndicator.IsEmpty);

			testItem.MessageType = CDSEDIMessageTypeList.Codes.NilAmendment;
			AssertEquals(AmendmentCancellationReasonCode.Codes.A_Nil, testItem.ChangeAcknowledgementIndicator);

			testItem.ChangeAcknowledgementIndicator = "";
			testItem.MessageType = CDSEDIMessageTypeList.Codes.FecChallenge;
			AssertEquals(AmendmentCancellationReasonCode.Codes.A_Nil, testItem.ChangeAcknowledgementIndicator);

			testItem.ChangeAcknowledgementIndicator = "";
			testItem.MessageType = CDSEDIMessageTypeList.Codes.ArrivalNotification;
			AssertEquals(AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice, testItem.ChangeAcknowledgementIndicator);
		}

		public void TestAmendmentReasonCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var testItem = new JobDeclarationMessageSendingObject(entry);

			testItem.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			var listForNewDeclaration = testItem.AmendmentReasonCodesList;

			testItem.MessageType = CDSEDIMessageTypeList.Codes.NilAmendment;
			var listForNilAmendment = testItem.AmendmentReasonCodesList;

			Assert("Amendment Reason codes list for Nil Amendment has a different number of elements.", listForNewDeclaration.Count != listForNilAmendment.Count);
			Assert("Amendment Reason codes list for Nil Amendment is a different list object.", listForNewDeclaration != listForNilAmendment);
		}

		public void TestCanParticipateEnhancedValidation()
		{
			var entry = Factory.New<CusEntryHeader>();
			var sendingObject = new JobDeclarationMessageSendingObject(entry);
			var messageTypesThatCannotParticipate = new[]
			{
				CDSEDIMessageTypeList.Codes.CancelDeclaration,
				CDSEDIMessageTypeList.Codes.FecChallenge,
				CDSEDIMessageTypeList.Codes.NilAmendment,
				CDSEDIMessageTypeList.Codes.ArrivalNotification,
				CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest,
				CDSEDIMessageTypeList.Codes.MasterQueryDeclaration,
				GbCusDecMessageFunctionsList.Codes.Associate,
				GbCusDecMessageFunctionsList.Codes.Disassociate,
				GbCusDecMessageFunctionsList.Codes.Close,
				GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation,
				GbCusDecMessageFunctionsList.Codes.DepartureFromLocation,
				GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation
			};
			var allMessageTypeList = new CDSEDIMessageTypeList().GetAllCodes().Concat(new GbCusDecMessageFunctionsList().GetAllCodes());
			foreach (var messageType in allMessageTypeList)
			{
				sendingObject.MessageType = messageType;
				var canParticipate = !messageType.In(messageTypesThatCannotParticipate);
				AssertEquals(canParticipate, sendingObject.CanParticipateEnhancedValidation);
			}
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new JobDeclarationMessageSendingObject(entryHeader);
		}

		#endregion
	}
}
