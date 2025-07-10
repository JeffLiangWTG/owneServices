using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncident.SupportIncidentDocumentSupporter))]
	class SupportIncidentDocumentSupporterTest : DocumentEngineCore.DocumentSupport.Testing.DocumentSupporterTest
	{
		public void TestGetContactOrganisation()
		{
			var docSupporter = DocumentSupporter;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = docSupporter.BusinessObject as SupportIncident;
			incident.IM_OH_Client = org.PK;

			Factory.Save();

			var orgContact = docSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalClient, DocumentDirection.ANY);
			AssertNotNull(orgContact);
			AssertEquals(org.PK, orgContact.OrgHeader.PK);

			orgContact = docSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignee, DocumentDirection.ANY);
			AssertNull(orgContact);
		}

		public void TestSupportedDataContext()
		{
			Assert("DataContext.GenericFreightJob is supported", DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob))));
			Assert("Business Object DataContext for SupportIncident is supported", DocumentSupporter.IsDataContextSupported(new DataContextValue(".SupportIncident")));
		}

		public void TestGetDocBusinessObject()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJob, null);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.SupportIncident, DocumentSupporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals("Security checkpoint", EDISecurityCheckpoints.CustomerServiceIncident, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			base.TestRunningDocumentsShouldNotCauseException();
			Assert(true);
		}

		#region Implementation

		SupportIncident.SupportIncidentDocumentSupporter documentSupporter;
		SupportIncident.SupportIncidentDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = documentSupporter = new SupportIncident.SupportIncidentDocumentSupporter((SupportIncident)GetDocumentSupportableBusinessObject())); }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<SupportIncident>();
		}

		#endregion
	}
}
