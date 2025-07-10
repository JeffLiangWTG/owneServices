using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayouts))]
	sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
	{
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
				yield return (MiscOptionsControlBag.Instance.NACCSCredentialGuidDropEdit, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.PaymentOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.PaymentPartyDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.PaymentDeadlineExtensionDropEdit, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, ControlWidthClass.Auto);
			}
		}

		public void TestPaymentOptionsVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Not Visible", false, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentOptionsSeparatorUserControl, declaration));
				AssertEquals("Not Visible", false, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentPartyDropEdit, declaration));
				AssertEquals("Not Visible", false, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentDeadlineExtensionDropEdit, declaration));
				AssertEquals("Not Visible", false, Layout.IsVisible(CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, declaration));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Visible", true, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentOptionsSeparatorUserControl, declaration));
				AssertEquals("Visible", true, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentPartyDropEdit, declaration));
				AssertEquals("Visible", true, Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentDeadlineExtensionDropEdit, declaration));
				AssertEquals("Visible", true, Layout.IsVisible(CommonMiscOptionsControlBag.Instance.DefermentAccountNumberTextBox, declaration));
			});
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonMiscOptionsLayoutBuilder<JobDeclaration>();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;

		PanelLayout Layout => layout ?? (layout = new MiscOptionsLayouts().Layout);

		PanelLayout layout;
	}
}
