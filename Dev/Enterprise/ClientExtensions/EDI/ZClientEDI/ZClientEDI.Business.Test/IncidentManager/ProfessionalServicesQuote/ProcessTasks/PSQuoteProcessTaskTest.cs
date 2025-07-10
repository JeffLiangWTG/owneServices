using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(PSQuoteProcessTask))]
	public class PSQuoteProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentType()
		{
			CachedTask.P9_ParentID = Factory.New<ProfessionalServicesQuote>().PK;
			AssertEquals(typeof(ProfessionalServicesQuote), CachedTask.Parent.GetType());
		}

		public void TestParentControllerID()
		{
			AssertEquals(ClientControllerRegistration.ProfessionalServicesQuote, CachedTask.ParentControllerID);
		}

		public void TestSubclassOfCRMProcessTask()
		{
			Assert(GetExpectedBusinessObjectType().IsSubclassOf(typeof(CRMProcessTask)));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ProfessionalServicesQuote quote = Factory.New<ProfessionalServicesQuote>();
			return quote.WorkflowItems.AddNew();
		}

		ProcessTask CachedTask
		{
			get { return (ProcessTask)CachedBusinessObject; }
		}
	}
}
