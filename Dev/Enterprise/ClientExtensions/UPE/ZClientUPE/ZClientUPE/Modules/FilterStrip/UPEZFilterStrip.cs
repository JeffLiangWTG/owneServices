using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module
{
	public class UPEZFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is UPEModuleFilter)
			{
				result = GetTreeFilterControls();
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}

		Control[] GetTreeFilterControls()
		{
			ZLabel statusLabel = new ZLabel();
			statusLabel.AutoSize = true;
			statusLabel.Text = "Status:";
			statusLabel.Size = statusLabel.PreferredSize;
			ControlDpiScalingHelper.SetTop(ref statusLabel, LabelTop, false);
			ControlDpiScalingHelper.SetLeft(ref statusLabel, Label1Start(statusLabel), false);
			statusLabel.TabIndex = 1;

			UPEQueueFilterStripTreeView treeView = new UPEQueueFilterStripTreeView(QueueFilterBizO);
			ControlDpiScalingHelper.SetLeft(ref treeView, FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref treeView, FilterControlBoxWidth, true);
			ControlDpiScalingHelper.SetTop(ref treeView, FilterControlTop, false);
			ControlDpiScalingHelper.SetHeight(ref treeView, 116, true);
			treeView.TabIndex = 2;

			PreferredHeight = treeView.Height + ControlDpiScalingHelper.OnePixel;

			return new Control[] { statusLabel, treeView };
		}

		public IQueueFilterBusinessObject QueueFilterBizO
		{
			get { return (IQueueFilterBusinessObject)ParentFilterControl.FilterBusinessObject; }
		}

		[SuppressFilterStripControlBoundCheck]
		public ZFilterStripControl ParentFilterControl
		{
			get
			{
				Control result = Parent;

				while (result != null && !(result is ZFilterStripControl))
				{
					result = result.Parent;
				}

				return result as ZFilterStripControl;
			}
		}
	}
}