using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(SystemUsageDocumentSupporter))]
	public class SystemUsageDocumentSupporterTest : DocumentSupporterTest
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
			return new DummyUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			if (documentCommand.SU_MenuName.StartsWith("Billing") || documentCommand.SU_MenuName.StartsWith("ODPL Monthly"))
			{
				return true;
			}

			return base.ExcludeDocumentCommandTest(documentCommand);
		}

		SystemUsageDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = documentSupporter = new SystemUsageDocumentSupporter(GetDocumentSupportableBusinessObject() as SystemUsage)); }
		}
		SystemUsageDocumentSupporter documentSupporter;

		#endregion
	}
}
