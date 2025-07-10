using System;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(CustomsBrokerageUserControl))]
sealed class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
{
	protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

	protected override Type ImportSupplierHeaderUserControlType => typeof(ImportSupplierHeaderUserControl);

	protected override Type ExportSupplierHeaderUserControlType => typeof(ExportSupplierHeaderUserControl);

	protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

	protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

	protected override Type MessageUserControlType => typeof(EntryMessageUserControl);

	protected override Type EntryInstructionDetailsUserControlType => typeof(EntryInstructionDetailsUserControl);

	protected override Type ContainersUserControlType => typeof(ContainerUserControl);

	public void TestContainerUserControl()
	{
		{
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			control.MainTabControl.SelectedTab = control.ContainerTabPage;
			AssertEquals(typeof(ContainerUserControl), control.ContainerUserControl.GetType());
		}
	}
}
