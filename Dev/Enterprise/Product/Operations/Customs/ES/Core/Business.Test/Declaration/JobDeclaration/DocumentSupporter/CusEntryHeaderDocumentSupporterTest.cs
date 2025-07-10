using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	class CusEntryHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetSupportedDataContexts()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var documentSupporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;
			var expectedSupportedDataContext = new DataContext[] { DataContext.ESSADH, DataContext.SADH };
			CombineAssertions("Expected Supported DataContexts", () =>
			{
				foreach (var dataContext in expectedSupportedDataContext)
				{
					Assert($"{dataContext} should be supported", documentSupporter.IsDataContextSupported(new DataContextValueForTesting(dataContext)));
				}
			});
		}

		public void TestGetDocumentTitlesForPivotES()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "C10";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "Reference";
			var docC10Header = ESDocC10Header.New(entryHeader, declaration);
			var docSupporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;

			CombineAssertions(() =>
			{
				var result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, docC10Header, pivot);
				AssertEquals("When parentDocumentName is correct", "C10 - Reference", result.Title);

				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, declaration, pivot);
				AssertNull("When parentBusinessObject is not entryHeader or ESDocC10Header", result);

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, docC10Header, pivot);
				AssertEquals("When parentBusinessObject is ESDocC10Header and it has mrn", "C10 - MRNCode", result.Title);

				result = docSupporter.GetDocumentTitlesForPivot("SADH C88", entryHeader, pivot);
				AssertEquals("When parentBusinessObject is entryHeader and it has mrn", "SADH C88 - MRNCode", result.Title);
			});
		}

		public void TestGetDocumentTitlesForPivotES_Shipment()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "C10";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "Reference";
			var docC10Header = ESDocC10Header.New(entryHeader, declaration);
			var docSupporter = shipment.DocumentSupporter;

			CombineAssertions(() =>
			{
				var result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, docC10Header, pivot);
				AssertEquals("When parentDocumentName is correct", "C10 - Reference", result.Title);

				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, declaration, pivot);
				AssertNull("When parentBusinessObject is not entryHeader or ESDocC10Header", result);

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				result = docSupporter.GetDocumentTitlesForPivot(menuItem.SU_MenuName, docC10Header, pivot);
				AssertEquals("When parentBusinessObject is ESDocC10Header and it has mrn", "C10 - MRNCode", result.Title);

				result = docSupporter.GetDocumentTitlesForPivot("SADH C88", entryHeader, pivot);
				AssertEquals("When parentBusinessObject is entryHeader and it has mrn", "SADH C88 - MRNCode", result.Title);
			});
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var landedCostHeader = (BusinessObject)Factory.New<LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = declaration.TablePrefix;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;

			return entryHeader;
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;
				maxHits.Add("CusSupportingInfo", 2);
				return maxHits;
			}
		}
	}
}
