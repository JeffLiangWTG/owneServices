using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.MFI.DocWrappers.Testing
{
	[TestedType(typeof(DocMFIPrintStatement))]
	sealed class DocMFIPrintStatementTestCase : DocumentWrapperTestCase
	{
		public void TestClientOverride()
		{
			AssertEquals("Client's DocInvoice Override", typeof(DocMFIPrintStatement), DocStatement.New(printStatement, Factory).GetType());
		}

		public void TestInvoiceLogo()
		{
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(3, 3));
			Image statementLetterhead = new Bitmap(1, 1);
			MFIDataRegistry.Instance.StatementLetterhead = statementLetterhead;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			GlbCompany.CurrentCompany.GC_Code = "AKL";
			var constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "EML");
			statementWrapper.SetTemplateConstants(constants);
			Factory.Save();
			AssertEquals("MFI NZ Specific Statement logo", statementLetterhead.Size, statementWrapper.StatementLogo.Size);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			constants.Clear();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "PRN");
			statementWrapper.SetTemplateConstants(constants);
			Factory.Save();
			AssertEquals("Default logo", null, statementWrapper.StatementLogo);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			constants.Clear();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "FAX");
			statementWrapper.SetTemplateConstants(constants);
			Factory.Save();
			AssertEquals("MFI NZ Specific Statement logo", statementLetterhead.Size, statementWrapper.StatementLogo.Size);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			constants.Clear();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "PRN");
			statementWrapper.SetTemplateConstants(constants);
			Factory.Save();
			AssertEquals("Default logo", SystemDataRegistry.Instance.CompanyLogo.Value.Size, statementWrapper.StatementLogo.Size);
		}

		DocMFIPrintStatement statementWrapper;
		PrintStatement printStatement;
		protected override void SetUp()
		{
			printStatement = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			printStatement.CutOffDate = ZDateTime.Today.AddDays(1);
			statementWrapper = DocMFIPrintStatement.New(printStatement, Factory);
			base.SetUp();
			Factory.Save();
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod() => DocMFIPrintStatement.New(printStatement, Factory);

		public override DocumentWrapper[] GetDocumentWrappers() => new DocumentWrapper[] { statementWrapper };
	}
}
