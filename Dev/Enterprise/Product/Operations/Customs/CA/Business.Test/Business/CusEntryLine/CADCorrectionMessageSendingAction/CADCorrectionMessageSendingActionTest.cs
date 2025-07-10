using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CADCorrectionMessageSendingAction))]
	sealed class CADCorrectionMessageSendingActionTest : Customs.Business.Testing.CusSupportingInfoTest<CADCorrectionMessageSendingAction>
	{
		public void TestParentIsLine()
		{
			Assert("sendingAction parent is CADCorrectionMessageSendingActionWrapper", sendingAction.Parent is CADCorrectionMessageSendingActionWrapper);
		}

		public void TestCaptions()
		{
			AssertCaption(nameof(CADCorrectionMessageSendingAction.EntryLineSequence), "Entry Line Number", mediumCaption: "Entry LNO", shortCaption: "LNO");
			AssertCaption(nameof(CADCorrectionMessageSendingAction.InvoiceSequence), "Invoice Seq #", shortCaption: "Inv. Seq #");
			AssertCaption(nameof(CADCorrectionMessageSendingAction.InvoiceLineSequence), "Inv. Line #", shortCaption: "LNO");
		}

		void AssertCaption(string propertyName, string caption, string mediumCaption = null, string shortCaption = null)
		{
			Predicate<ResourceStringDataAttribute> predicate = (x) => x.Caption == caption && x.MediumCaption == mediumCaption && x.ShortCaption == shortCaption;
			AssertHasCustomAttribute(typeof(CADCorrectionMessageSendingAction), propertyName, false, predicate);
		}

		public void TestEntryLineSequenceMatchesEntryLineWithIncorrectEntryLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;

			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine1 = cadEntry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = cadEntry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = cadEntry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			var entryLine4 = cadEntry.MergedLines.AddNew();
			entryLine4.CL_LineNumber = 4;
			var entryLine5 = cadEntry.MergedLines.AddNew();
			entryLine5.CL_LineNumber = 5;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JobComInvoiceLines.AddNew().JI_CL = entryLine1.PK;
			invoice1.JobComInvoiceLines.AddNew().JI_CL = entryLine3.PK;
			invoice1.JobComInvoiceLines.AddNew().JI_CL = entryLine2.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JobComInvoiceLines.AddNew().JI_CL = entryLine4.PK;
			invoice2.JobComInvoiceLines.AddNew().JI_CL = entryLine5.PK;

			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			var action = wrapper.SendingActions.AddNew();

			action.EntryLineSequence = 1;
			AssertEquals(1, (int)action.InvoiceSequence);
			AssertEquals(1, (int)action.InvoiceLineSequence);
			AssertEquals(entryLine1.PK, action.MatchedEntryLine.PK);

			action.EntryLineSequence = 2;
			AssertEquals(1, (int)action.InvoiceSequence);
			AssertEquals(2, (int)action.InvoiceLineSequence);
			AssertEquals(entryLine3.PK, action.MatchedEntryLine.PK);

			action.EntryLineSequence = 3;
			AssertEquals(1, (int)action.InvoiceSequence);
			AssertEquals(3, (int)action.InvoiceLineSequence);
			AssertEquals(entryLine2.PK, action.MatchedEntryLine.PK);

			action.EntryLineSequence = 4;
			AssertEquals(2, (int)action.InvoiceSequence);
			AssertEquals(1, (int)action.InvoiceLineSequence);
			AssertEquals(entryLine4.PK, action.MatchedEntryLine.PK);

			action.EntryLineSequence = 5;
			AssertEquals(2, (int)action.InvoiceSequence);
			AssertEquals(2, (int)action.InvoiceLineSequence);
			AssertEquals(entryLine5.PK, action.MatchedEntryLine.PK);
		}

		public void TestCSI_Code()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength", 3, sendingAction.CSI_CodeInfo.MaxLength);
				AssertEquals("Caption", "Reason Code", sendingAction.CSI_CodeInfo.Description);
			});
		}

		public void TestCSI_SubType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength", 1, sendingAction.CSI_SubTypeInfo.MaxLength);
				AssertEquals("Caption", "Appeals Program Code", sendingAction.CSI_SubTypeInfo.Description);
			});
		}

		public void TestCSI_Description()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength", 255, sendingAction.CSI_DescriptionInfo.MaxLength);
				AssertEquals("Caption", "Supporting Remarks", sendingAction.CSI_DescriptionInfo.Description);
			});
		}

		public void TestCADCorrectionMessageSendingActionHumanReadableName()
		{
			AssertEquals("CAD Correction Message Sending Action", Factory.New<CADCorrectionMessageSendingAction>().HumanReadableName);
		}

		public void TestParentCorrectlySet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.JobComInvoiceLines.AddNew();

			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_CommoditySequence = 1;
			entryLine.CL_GoodsShipmentSequence = 1;
			invoiceline.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Factory.Save();
			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			var sendingAction = wrapper.SendingActions.AddNew();
			sendingAction.EntryLineSequence = 3;

			AssertEquals("CSI_ParentID", CADCorrectionMessageSendingAction.FakeEntryLinePK, sendingAction.CSI_ParentID);

			sendingAction.EntryLineSequence = 1;
			sendingAction.CSI_DataModel = "CA";

			AssertEquals("CSI_ParentTableCode", CusEntryLineSchema.Constants.Prefix, sendingAction.CSI_ParentTableCode);
			AssertEquals("CSI_ParentID", entryLine.PK, sendingAction.CSI_ParentID);

			var sendingAction2 = wrapper.SendingActions.AddNew();
			sendingAction2.EntryLineSequence = 10;
			sendingAction2.CSI_DataModel = "CA";
			Factory.Save();

			AssertEquals(false, new BusinessObjectFactory().ExistsInDatabase(CusSupportingInfoSchema.Constants.TableName, new ZQuery(CusSupportingInfoSchema.PK, sendingAction2.PK)));
		}

		protected override IEnumerable<CADCorrectionMessageSendingAction> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			line.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			factory.Save();

			var sendingAction = factory.New<CADCorrectionMessageSendingAction>();
			sendingAction.CSI_Type = Customs.Common.CA.CusSupportingInfoTypeList.Codes.CadCorrectionMessageSendingAction;
			sendingAction.CSI_ParentID = entryLine.PK;
			sendingAction.CSI_ParentTableCode = CusEntryLineSchema.Constants.Prefix;
			sendingAction.CSI_DataModel = "CA";
			yield return sendingAction;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return sendingAction;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_CommoditySequence = 1;
			entryLine.CL_GoodsShipmentSequence = 1;
			line.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			sendingAction = wrapper.SendingActions.AddNew();
			sendingAction.EntryLineSequence = 1;
		}
		CADCorrectionMessageSendingAction sendingAction;
	}
}
