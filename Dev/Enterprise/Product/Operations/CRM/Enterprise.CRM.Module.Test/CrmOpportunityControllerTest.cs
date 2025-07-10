using System;
using Enterprise.CRM.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CRM.Module.Test
{
	[TestedType(typeof(CrmOpportunityController))]
	public class CrmOpportunityControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CrmOpportunity;
		}

		public void TestEditForm_OpenGLOW()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/"))
			{
				AssertControllerNotNull();
				WebUrlLauncher.ClearLastUrlLaunched();
				var opp = GetBusinessObjectThatIsInTheDatabase() as CrmOpportunity;
				var iZForm = Controller.ShowEditForm(opp);
				AssertNull(iZForm);
				AssertEquals("EditForm opens GLOW url", "https://localhost/goto/OpportunityEdit?entityPK=" + opp.PK.ToString(), WebUrlLauncher.LastUrlLaunched);
			}
		}

		public override void TestDeleteForm()
		{
			AssertControllerNotNull();
			var bizO = GetBusinessObjectThatIsInTheDatabase();
			var form = Controller.ShowDeleteForm(bizO);
			AssertNull(form);
		}

		public override void TestEditForm()
		{
			AssertControllerNotNull();
			AssertNull(GetEditFormToShow());
		}

		public override void TestNewForm()
		{
			AssertControllerNotNull();
			AssertNull(Controller.ShowNewForm());
		}

		public override void TestViewForm()
		{
			AssertControllerNotNull();
			AssertNull(Controller.ShowViewForm(GetBusinessObjectThatIsInTheDatabase()));
		}
	}
}
