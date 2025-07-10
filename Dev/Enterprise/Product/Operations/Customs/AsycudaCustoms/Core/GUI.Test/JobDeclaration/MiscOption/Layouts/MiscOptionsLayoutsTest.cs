using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayouts))]
	sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
	{
		public void TestPaidByDropEditVisibility()
		{
			AssertControlVisibility<ZDropEdit>(DeclarationApplicationCodeList.Codes.Interfaced, "PaidByDropEdit", true);
			AssertControlVisibility<ZDropEdit>(DeclarationApplicationCodeList.Codes.Builtin, "PaidByDropEdit", true);
		}

		public void TestPaymentAccountNumberTextBoxVisibility()
		{
			AssertControlVisibility<ZTextBox>(DeclarationApplicationCodeList.Codes.Interfaced, "PaymentAccountNumberTextBox", true);
			AssertControlVisibility<ZTextBox>(DeclarationApplicationCodeList.Codes.Builtin, "PaymentAccountNumberTextBox", true);
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonMiscOptionsControlBag.Instance.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (CommonMiscOptionsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.PaymentPartyDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.PaymentAccountNumberTextBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.AsycudaRelatedDeclarationsUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonMiscOptionsLayoutBuilder<JobDeclaration>();

		void AssertControlVisibility<T>(ZString applicationCode, ZString controlName, bool isVisible) where T : Control
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = applicationCode;

			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = new CustomsBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();

				brokerageUserControl.JobDeclaration = declaration;
				var miscTab = brokerageUserControl.MiscOptionsTabPage;
				brokerageUserControl.MainTabControl.SelectedTab = miscTab;

				var dynamicMiscPanel = miscTab.FindSingle<DynamicLayoutPanel>("DynamicMiscOptionsPanel");
				var miscControl = dynamicMiscPanel.FindSingle<T>(controlName);
				AssertEquals($"{controlName}'s visibility", isVisible, miscControl.Visible);
			}
		}
	}
}
