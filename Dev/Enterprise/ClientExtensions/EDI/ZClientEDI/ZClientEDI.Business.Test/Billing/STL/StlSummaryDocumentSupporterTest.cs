using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlSummaryDocumentSupporter))]
	public class StlSummaryDocumentSupporterTest : DocumentSupporterTest
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
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			return StlMonthlyUsageTest.CreateMonthlyUsage(lic, new ZDateTime(2015, 9, 1));
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			if (documentCommand.SU_MenuName.StartsWith("STL Billing") ||
				documentCommand.SU_MenuName.StartsWith("ODPL Monthly") ||
				documentCommand.SU_MenuName.StartsWith("Billing"))
			{
				return true;
			}

			return base.ExcludeDocumentCommandTest(documentCommand);
		}

		StlSummaryDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = documentSupporter = new StlSummaryDocumentSupporter((StlMonthlyUsage)GetDocumentSupportableBusinessObject())); }
		}
		StlSummaryDocumentSupporter documentSupporter;

		#endregion
	}
}
