using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CN.Business;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CustomsCusContainersUserControlTest : TestCaseWithFactory
	{
		public void TestCusContainersBoundGridColumns()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_TransportMode = "RAI";
			jobDeclaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.LoadContainerTabPage();
				var control = (CustomsCusContainersUserControl)brokerageControl.ContainerUserControl;
				AssertNotNull(control.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.CO_ContainerNumber));
				AssertNotNull(control.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.CO_RC));
				AssertNotNull(control.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.ContainerCode));
				AssertNotNull(control.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.ContainerCodeDescription));
				AssertNotNull(control.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.CO_FCL_LCL_AIR));
				AssertNotNull(control.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.CO_Seal));
				AssertNotNull(control.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.CO_SecondSeal));
			}
		}
	}
}
