using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	public class UniversalCopyMultiControlColumnStyleInfo : ZMultiControlColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(UniversalCopyMultiControlColumnStyle);

		public GetPropertyListModuleIdDelegate GetPropertyListModuleIdMethod { get; set; }
	}

	public class UniversalCopyMultiControlColumnStyle : ZMultiControlColumnStyle
	{
		public UniversalCopyMultiControlColumnStyle(UniversalCopyMultiControlColumnStyleInfo columnInfo)
			: this(() => new UniversalCopyMultiCombinationControl(), columnInfo)
		{
		}

		public UniversalCopyMultiControlColumnStyle(Func<UniversalCopyMultiCombinationControl> combinationControl, UniversalCopyMultiControlColumnStyleInfo columnInfo)
			: base(combinationControl, columnInfo)
		{
			this.columnInfo = columnInfo;
		}
		readonly UniversalCopyMultiControlColumnStyleInfo columnInfo;

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var combinationControl = (UniversalCopyMultiCombinationControl)control;
			combinationControl.GetPropertyListModuleIdMethod = columnInfo.GetPropertyListModuleIdMethod;
		}

		public override void LoadModuleId(BusinessObject bizo, ZListUserControl listUserControl)
		{
			var currentItem = listUserControl.CurrentItem as PropertyCopyTemplateBizo;

			if (EditControl is UniversalCopyMultiCombinationControl control &&
				listUserControl is ZGridGuidFindBox findBox &&
				(currentItem?.ModuleId == null || findBox.ModuleID == null || findBox.ModuleID == ModuleIDs.NotAssigned))
			{
				var module = control.GetModuleID(bizo);
				if (module != null && module != ModuleIDs.NotAssigned)
				{
					findBox.ModuleID = module;
				}
				control.SetFindBoxEnable(findBox);
			}
		}
	}
}
