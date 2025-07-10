using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.AU.Declaration.Business.CusEntryHeader;
using SendsMessagesToCustomsShutterUpperer = Enterprise.Customs.AU.Declaration.Business.SendsMessagesToCustomsShutterUpperer;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobDeclaration))]
	public class WoolworthsJobDeclaration_DocumentSupportTest : WowDocumentSupportTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			WoolworthsJobDeclaration declaration = Factory.New<WoolworthsJobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_DeclarationReference = "B00148999";
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			EDIMessage aTDMessage = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = Customs.AU.Declaration.Business.Testing.CusEntryHeaderTest.ATDMessageText;
			entryHeader.Messages.Add(Customs.AU.Declaration.Business.Testing.CusEntryHeaderDocumentSupportTest.CreateCMRPAYRECMessage(Factory));
			BusinessObject landedCostHeader = (BusinessObject)Factory.New<Enterprise.Integration.LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = "JE";
			BusinessObject landedCostHistory = (BusinessObject)Factory.New<Enterprise.Integration.LandedCosting.ILandedCostHistory>();
			landedCostHistory[LandedCostHistorySchema.LH_LT.Name] = landedCostHeader.PK;
			landedCostHistory[LandedCostHistorySchema.LH_ParentID.Name] = invoiceLine.PK;
			landedCostHistory[LandedCostHistorySchema.LH_ParentTableCode.Name] = JobComInvoiceLineSchema.Constants.Prefix;
			declaration.DocsAndCartage.Services.AddNew();
			return declaration;
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			if (documentCommand.SU_MenuName.StartsWith("Bill of Entry") || documentCommand.SU_MenuName.StartsWith("Exchange Control") || documentCommand.SU_MenuName.StartsWith("Customs and Excise Declaration") || documentCommand.SU_MenuName.StartsWith("Post Consolidation") || documentCommand.SU_MenuName.StartsWith("Pre Consolidation") || documentCommand.SU_MenuName.Contains("Cartage Advice") || documentCommand.SU_MenuName.Contains("DocBuilder Invoice"))
			{
				return ZBool.True;
			}

			return base.ExcludeDocumentCommandTest(documentCommand);
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = new Dictionary<string, int>();
				maxHits["CusEntryCPDec"] = 3;
				maxHits["CusEntryLine"] = 2;
				maxHits["GenPivot"] = 2;
				maxHits["JobComInvoiceHeader"] = 3;
				maxHits["JobComInvHeaderCharge"] = 2;
				maxHits["RefDbCmrAU_CMRLodgementQuestion"] = 2;
				maxHits["RefDbCmrAU_CMRTariffRatePeriodSnapshot"] = 2;
				maxHits["StmALog"] = 2;
				maxHits["CusEntryHeader"] = 2;
				return maxHits;
			}
		}
	}
}
