using System;
using System.Reflection;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ComponentToolboxVisibilityTestCase : ComponentToolboxVisibilityBaseTestCase
	{
		protected override Type[] GetExpectedToolboxVisibleComponentTypes()
		{
			return new Type[]
			{
				typeof(ZBindingSource),
				typeof(ZCalcEdit),
				typeof(ZCalcDropEdit),
				typeof(ZGroupBox),
				typeof(ZDateEdit),
				typeof(ZDateTimeOffsetEdit),
				typeof(ZGeographyEdit),
				typeof(ZTextBox),
				typeof(ZTimeEdit),
				typeof(ZTimeTimeEdit),
				typeof(ZDayAndTimeEdit),
				typeof(ZDropEdit),
				typeof(ZGuidDropEdit),
				typeof(ZGuidSearchEdit),
				typeof(ZLabel),
				typeof(ZHeaderLabel),
				typeof(ZRichTextBox),
				typeof(ZPeriodEdit),
				typeof(ZExchangeRateControl),
				typeof(ZCodeFindBox),
				typeof(ZGuidFindBox),
				typeof(ZGuidSearchEdit),
				typeof(ZFilterCollectionFindBox),
				typeof(ZButton),
				typeof(ZTabControl),
				typeof(ZModuleButtonGrid),
				typeof(ZPanel),
				typeof(ZCollapsiblePanel),
				typeof(ZTreeView),
				typeof(ZCalcFindBox),
				typeof(ZRadioButton),
				typeof(ZDropButtonOnly),
				typeof(ZYearEdit),
				typeof(ZGrid),
				typeof(ZDisplayGrid),
				typeof(ZCheckedListBox),
				typeof(ZCheckedTextBox),
				typeof(ZOverridableTextBox),
				typeof(Internal.ZDateRangeControl),
				typeof(Internal.ZTimeRangeControl),
				typeof(Internal.ZNumberRangeControl),
				typeof(ZAnimationBox),
				typeof(ZMasterBillControl),
				typeof(ZTimeEditEx),
				typeof(ZTimeTimeEditEx),
				typeof(ZTimeZoneFindBox),
				typeof(ZToolBar),
				typeof(ZNumericUpDown),
				typeof(ZCheckBox),
				typeof(ZLinkLabel),
				typeof(ZPreviousNextControl),
				typeof(ZUserControl),
				typeof(ZDynamicControlCreationUserControl),
				typeof(ZSelectedEventPopupFindBox),
				typeof(ZWebBrowser),
				typeof(Enterprise.Core.Forms.ZPostingButtonsUserControl),
				typeof(ZDropEditWithFixedWidth),
				typeof(ZGuidDropEditWithFixedWidth),
				typeof(ZOpenFileDialog),
				typeof(ZSaveFileDialog),
				typeof(ZFolderBrowserDialog),
				typeof(ZCodeFindBoxWithSelectedEvent),
				typeof(ZGuidFindBoxWithSelectedEvent),
				typeof(ZToolStrip),
				typeof(ZTranslatableTextControl),
				typeof(ZFadePanel),
				typeof(ZIntEdit),
				typeof(ZFilteredTreeView),
				typeof(SeparatorUserControl),
				typeof(ZPictureBox),
			};
		}

		protected override Assembly TargetAssembly
		{
			get
			{
				return Assembly.Load("Enterprise.ZArchitecture.GUI");
			}
		}
	}
}
