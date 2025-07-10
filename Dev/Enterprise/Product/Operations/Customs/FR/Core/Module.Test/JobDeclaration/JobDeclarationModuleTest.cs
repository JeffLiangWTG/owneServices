using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	public class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
	{
		protected override bool HasFailedFetchHint(TableHitCount tableSelect)
		{
			var hasFailedFetchHint = base.HasFailedFetchHint(tableSelect);
			if (tableSelect.TableName == EDIMessageSchema.Constants.TableName && tableSelect.Value <= 36)
			{
				hasFailedFetchHint = false;
			}

			return hasFailedFetchHint;
		}

		public void TestActionMenuItemsContainsDeltaIEResponseInterchangeImporter()
		{
			using (var module = new JobDeclarationModule())
			{
				_ = module.FormActionMenu;
				var actionMenuItems = module.ActionsMenuItem.MenuItems;
				var addDeltaIEResponseInterchangeActionMenuItem = actionMenuItems.FindByText(DeltaIEInboundInterchangeImporter.DeltaIEResponseInterchangeAddActionText);
				AssertNotNull("The menu item of delta IE response interchange importer should exist in the actions menu.", addDeltaIEResponseInterchangeActionMenuItem);
			}
		}

		public void TestActionMenuItemsContainsDeltaGResponseInterchangeImporter()
		{
			using (var module = new JobDeclarationModule())
			{
				_ = module.FormActionMenu;
				var actionMenuItems = module.ActionsMenuItem.MenuItems;
				var addDeltaGResponseInterchangeActionMenuItem = actionMenuItems.FindByText(DeltaGInboundInterchangeImporter.DeltaGResponseInterchangeAddActionText);
				AssertNotNull("The menu item of delta G response interchange importer should exist in the actions menu.", addDeltaGResponseInterchangeActionMenuItem);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.France;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		protected override Customs.Business.BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.ECS;
			declaration.JE_CustomsProfile = "123456789";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.JI_Weight = 5;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 10;
			invoiceLine2.JI_Weight = 5;

			var entryinstruction = declaration.CustomsEntryInstructions[0];
			entryinstruction.CEI_DateForDuty = ZDateTime.Today;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_TriggeringPointForValidation = "VAQ";

			var cusEntryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader2.CH_CEI_Instruction = entryinstruction.PK;
			cusEntryHeader2.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES100, new ZDateTime(2022, 10, 23).ToOffset());
			cusEntryHeader2.CH_TriggeringPointForValidation = "VAQ";

			cusEntryHeader.CH_BGMReference = "19212081311";
			cusEntryHeader.CH_ExitedStatus = "XXX";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			cusEntryHeader.CH_BGMReference = "19212081312";
			cusEntryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES100, new ZDateTime(2022, 10, 22).ToOffset());

			var message = factory.New<DeltaCImportFREDIMessage>(); 
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Module.Testing.TestFiles.DeltaCImportINVResponseMessageWithREF.xml");
			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 03, 11, DateTimeKind.Utc);
			cusEntryHeader.Messages.Add(message);

			var cusEntryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;
			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			cusEntryHeader.FRCustomsFallbackNumber = "1234567890";
			cusEntryHeader.DeltaGFallbackStatus = "PPW";
			cusEntryHeader.DeltaGFallbackIssueDate = new ZDateTime(2020, 06, 20);
			declaration.JE_LocationOfGoods = "AUL12345123154";
			factory.Save();

			CusEntryNumber entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryType = CusEntryHeader.Schema.FallbackEntryType;
			entryNum.CE_ParentID = declaration.CustomsEntryHeaders[0].PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNum.CE_EntryStatus = "PPW";
			entryNum.CE_IssueDate = new ZDateTime(2020, 06, 20);
			entryNum.CE_EntryNum = "1234567890";
			factory.Save();

			return declaration;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
