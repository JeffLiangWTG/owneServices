using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JCJournalController))]
	public class JCJournalControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JCCostingJournal;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.New<JCJournalHeader>();
			Factory.Save();
			return bizO;
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
		}

		public override void TestEditForm()
		{
			AssertNull("EditForm should be null", Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()));
		}

		public override void TestDeleteForm()
		{
			AssertNull("DeleteForm should be null", Controller.ShowDeleteForm(GetBusinessObjectThatIsInTheDatabase()));
		}
	}
}
