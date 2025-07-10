using System;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(CustomsBrokerageUserControl))]
sealed class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
{
	public void TestMessageVersionChangeTriggersRemoveUserControlOfEachTabPage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageVersion = "XML";

		using (var userControl = new CustomsBrokerageUserControlForRemoveUserControlOfEachTabPageTest())
		{
			AssertEquals("PRE-CONDITION: Before setting declaration", 0, userControl.RemoveUserControlOfEachTabPageHitCount);

			userControl.JobDeclaration = declaration;
			AssertEquals("POST-CONDITION: on declaration set", 1, userControl.RemoveUserControlOfEachTabPageHitCount);

			declaration.MessageVersion = "TXT";
			AssertEquals("POST-CONDITION: on MessageVersion change", 2, userControl.RemoveUserControlOfEachTabPageHitCount);
		}
	}

	protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

	protected override Type ImportSupplierHeaderUserControlType => typeof(ImportSupplierHeaderUserControl);

	protected override Type ExportSupplierHeaderUserControlType => typeof(ExportSupplierHeaderUserControl);

	protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

	protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

	protected override Type MessageUserControlType => typeof(MessageUserControl);

	protected override Type EntryInstructionDetailsUserControlType => typeof(EntryInstructionDetailsUserControl);

	class CustomsBrokerageUserControlForRemoveUserControlOfEachTabPageTest : CustomsBrokerageUserControl
	{
		protected override void RemoveUserControlOfEachTabPage()
		{
			RemoveUserControlOfEachTabPageHitCount++;
		}

		public int RemoveUserControlOfEachTabPageHitCount { get; set; }
	}
}
