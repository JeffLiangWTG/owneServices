using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	public class JobDeclarationDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var supporter = new JobDeclarationDocumentSupporter(declaration);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Air, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Rail, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Road, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Sea, supporter.TransportMode);
		}

		public void TestGetDocumentTitlesForPivot()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "SADH C88";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "Reference";
			var docSupporter = declaration.DocumentSupporter;

			CombineAssertions(() =>
			{
				var result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, entryHeader, pivot);
				AssertEquals("When parentDocumentName is the expected one", "SADH C88 - Reference", result.Title);

				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, declaration, pivot);
				AssertNull("When parentBusinessObject is not entryHeader", result);

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, entryHeader, pivot);
				AssertEquals("When parentBusinessObject is entryHeader and it has mrn", "SADH C88 - MRNCode", result.Title);
			});
		}

		public void TestGetDocumentTitlesForPivot_Shipment()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "SADH C88";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "Reference";
			var docSupporter = shipment.DocumentSupporter;

			CombineAssertions(() =>
			{
				var result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, entryHeader, pivot);
				AssertEquals("When parentDocumentName is the expected one", "SADH C88 - Reference", result.Title);

				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, declaration, pivot);
				AssertNull("When parentBusinessObject is not entryHeader", result);

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, entryHeader, pivot);
				AssertEquals("When parentBusinessObject is entryHeader and it has mrn", "SADH C88 - MRNCode", result.Title);
			});
		}

		public void TestSupportedChildBusinessContexts()
		{
			var declaration = Factory.New<JobDeclaration>();
			var docSupporter = declaration.DocumentSupporter;

			AssertEquals("Has CusExitHeader", expected: true, docSupporter.SupportedChildBusinessContexts.Contains(CargoWise.Definitions.BusinessContext.CusExitHeader));
		}

		public void TestGetChildCollection_CusExitHeader() => CombineAssertions(() =>
		{
			var menu = Factory.New<StmMenuItem>();

			var declaration = Factory.New<JobDeclaration>();
			var docSupporter = declaration.DocumentSupporter;

			AssertEquals(0, docSupporter.GetChildCollection(menu, CargoWise.Definitions.BusinessContext.CusExitHeader, null).Length);

			ExitControlTestHelper.CreateCusExitHeader(declaration);

			AssertEquals(1, docSupporter.GetChildCollection(menu, CargoWise.Definitions.BusinessContext.CusExitHeader, null).Length);
		});

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			var ceh = dec.CustomsEntryHeaders.AddNew();
			var cl = ceh.MergedLines.AddNew();
			invLine.JI_CL = cl.PK;
			return dec;
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			// As advised by Kalos, because of this crap: http://crikey.wtg.zone/UserTestResultsForm.aspx?UserTestPK=6806a384-7078-4ce8-9c68-73c9109de40b
			return documentCommand.SU_MenuName.Contains("Cartage Advice", System.StringComparison.InvariantCultureIgnoreCase)
				|| documentCommand.SU_MenuName.Contains("Request for Service", System.StringComparison.InvariantCultureIgnoreCase)
				|| documentCommand.SU_MenuName.Contains("Authorization for Service", System.StringComparison.InvariantCultureIgnoreCase);
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = new Dictionary<string, int>();

				maxHits["CusEntryLine"] = 2;
				maxHits["CusEntryNum"] = 2;
				maxHits["CusHouseContPackInvoiceLinePivot"] = 2;
				maxHits["GenPivot"] = 2;
				maxHits["JobComInvoiceHeader"] = 2;
				maxHits["JobDocAddress"] = 2;
				maxHits["JobHeader"] = 2;
				maxHits["OrgAddress"] = 3;

				return maxHits;
			}
		}
	}
}
