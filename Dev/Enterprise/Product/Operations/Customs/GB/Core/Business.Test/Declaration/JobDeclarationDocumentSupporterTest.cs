using System;
using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	class JobDeclarationDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDeclarationGetsRightDocumentSupporter()
		{
			AssertNotNull("declaration.DocumentSupporter should return GB JobDeclarationDocumentSupporter", declaration.DocumentSupporter as JobDeclarationDocumentSupporter);
		}

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals(false, docSupporter.ShowReasonForNotPrinting(DataContext.Dummy, null));
		}

		public void TestDataContextSupported()
		{
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.GbTaxEstimator)));
		}

		public void TestGbTaxEstimatorDataContext()
		{
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.GbTaxEstimator)));

			declaration.CustomsEntryHeaders.AddNew();
			var taxEstimatorWrappers = docSupporter.GetDocumentWrappers(DataContext.GbTaxEstimator, null);
			AssertEquals(1, taxEstimatorWrappers.Length);
			AssertEquals("Enterprise.Customs.GB.DocumentWrappers.DocTaxEstimatorWrapper", taxEstimatorWrappers[0].GetType().FullName);
		}

		public void TestDocSADHContext()
		{
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.SADH)));

			declaration.CustomsEntryHeaders.AddNew();
			var docSADH = docSupporter.GetDocumentWrappers(DataContext.SADH, null);
			AssertEquals(1, docSADH.Length);
			AssertEquals("Enterprise.Customs.GB.DocumentWrappers.DocSADH", docSADH[0].GetType().FullName);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var inv = declaration.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var cl = ceh.MergedLines.AddNew();
			invLine.JI_CL = cl.PK;
			return declaration;
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			// As advised by Kalos, because of this crap: http://crikey.wtg.zone/UserTestResultsForm.aspx?UserTestPK=6806a384-7078-4ce8-9c68-73c9109de40b
			return documentCommand.SU_MenuName.Contains("Cartage Advice", StringComparison.InvariantCultureIgnoreCase)
				|| documentCommand.SU_MenuName.Contains("Request for Service", StringComparison.InvariantCultureIgnoreCase)
				|| documentCommand.SU_MenuName.Contains("Authorization for Service", StringComparison.InvariantCultureIgnoreCase);
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
				maxHits["OrgAddress"] = 2;
				maxHits["RefDatabase_RefDataGrouping"] = 2;
				maxHits["ZZRefCusCodeListCombined"] = 2; // Temporary solution

				return maxHits;
			}
		}

		JobDeclaration declaration;
		JobDeclarationDocumentSupporter docSupporter;

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			docSupporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			base.SetUp();
		}
	}
}
