using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(OrganisationBillDocumentSupporter))]
	public class OrganisationBillDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocumentSupporterOverrides()
		{
			AssertEquals("BusinessContext", BusinessContext.CargoWiseBilling, DocumentSupporter.BusinessContext);
			AssertEquals("DataContext", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.CargoWiseBilling))));
			AssertEquals(Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return new OrganisationBill(Factory, Env.CurrentBranch.PK, Factory.New<OrgHeader>().PK, "", ZDateTime.Now);
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			if (documentCommand.SU_MenuName.StartsWith("Billing") || documentCommand.SU_MenuName.StartsWith("ODPL Monthly"))
			{
				return true;
			}

			return base.ExcludeDocumentCommandTest(documentCommand);
		}

		OrganisationBillDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = documentSupporter = new OrganisationBillDocumentSupporter((OrganisationBill)GetDocumentSupportableBusinessObject())); }
		}
		OrganisationBillDocumentSupporter documentSupporter;

		#endregion
	}
}
