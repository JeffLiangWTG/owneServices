using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayouts))]
	sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
	{
		public void TestBankAccountGuidFindBoxVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				Assert("BankAccountGuidFindBox visible for Import", Layout.IsVisible(MiscOptionsControlBag.Instance.BankAccountGuidFindBox, declaration));
				Assert("PaymentPartyDropEdit visible for Import", Layout.IsVisible(CommonMiscOptionsControlBag.Instance.PaymentPartyDropEdit, declaration));
				Assert("PaymentSeparatorUserControl visible for Import", Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				Assert("BankAccountGuidFindBox NOT visible for Export", !Layout.IsVisible(MiscOptionsControlBag.Instance.BankAccountGuidFindBox, declaration));
				Assert("PaymentPartyDropEdit NOT visible for Export", !Layout.IsVisible(CommonMiscOptionsControlBag.Instance.PaymentPartyDropEdit, declaration));
				Assert("PaymentSeparatorUserControl NOT visible for Export", !Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				Assert("BankAccountGuidFindBox visible for Import Siscomex", Layout.IsVisible(MiscOptionsControlBag.Instance.BankAccountGuidFindBox, declaration));
				Assert("PaymentPartyDropEdit visible for Import Siscomex", Layout.IsVisible(CommonMiscOptionsControlBag.Instance.PaymentPartyDropEdit, declaration));
				Assert("PaymentSeparatorUserControl visible for Import Siscomex", Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				Assert("BankAccountGuidFindBox NOT visible for Import License", !Layout.IsVisible(MiscOptionsControlBag.Instance.BankAccountGuidFindBox, declaration));
				Assert("PaymentPartyDropEdit NOT visible for Import License", !Layout.IsVisible(CommonMiscOptionsControlBag.Instance.PaymentPartyDropEdit, declaration));
				Assert("PaymentSeparatorUserControl NOT visible for Import License", !Layout.IsVisible(MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, declaration));
			});
		}

		public void TestMergeByDropEditCaption_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
			AssertEquals("Merge By", DataBoundResourceStrings.GetDataForProperty(declaration.JE_MergeByInfo).Caption);
		}

		public void TestMergeByDropEditCaption_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			AssertEquals("Merge By", DataBoundResourceStrings.GetDataForProperty(declaration.JE_MergeByInfo).Caption);
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
				yield return (CommonMiscOptionsControlBag.Instance.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.PaymentSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (CommonMiscOptionsControlBag.Instance.PaymentPartyDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.BankAccountGuidFindBox, ControlWidthClass.Auto);
			}
		}

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder();

		PanelLayout layout;
	}
}
