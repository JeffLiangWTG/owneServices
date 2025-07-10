using System;
using System.Drawing;
using System.Globalization;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.GUI
{
	static class GenericStatusChangeButtonProvider
	{
		public static GenericStatusChangeButton GetButtonForCode(ZString statusCode, ITaskCardComponentParent parent, int? locationX, int? locationY, string clickBehaviour)
		{
			switch (statusCode)
			{
				case StaticControlTypeList.Codes.CompletedStatusButton:
					return CreateButton(locationX, locationY, parent, ProcessTaskStatusCodeList.Codes.Closed,
						() => Enterprise.BufferManagement.GUI.Properties.Resources.tick_selected,
						() => Enterprise.BufferManagement.GUI.Properties.Resources.tick,
						(t) => t.P9_Status == ProcessTaskStatusCodeList.Codes.Closed, clickBehaviour);
				case StaticControlTypeList.Codes.SuspendStatusButton:
					return CreateButton(locationX, locationY, parent, ProcessTaskStatusCodeList.Codes.Suspended,
						() => Enterprise.BufferManagement.GUI.Properties.Resources.pause_selected,
						() => Enterprise.BufferManagement.GUI.Properties.Resources.pause,
						(t) => t.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended, clickBehaviour);
				case StaticControlTypeList.Codes.WorkingStatusButton:
					return CreateButton(locationX, locationY, parent, ProcessTaskStatusCodeList.Codes.Working,
						() => Enterprise.BufferManagement.GUI.Properties.Resources.play_selected,
						() => Enterprise.BufferManagement.GUI.Properties.Resources.play,
						(t) => t.P9_Status == ProcessTaskStatusCodeList.Codes.Working, clickBehaviour);
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "In order to get a status change button it must be a status from the list. {0} is not supported", statusCode));
			}
		}

		static GenericStatusChangeButton CreateButton(int? locationX, int? locationY, ITaskCardComponentParent parent, ZString statusChangeValue, Func<Bitmap> selectedPictureGetter, Func<Bitmap> unSelectedPictureGetter, Func<ProcessTask, bool> selectedStatusFunc, string clickBehaviour)
		{
			var button = new GenericStatusChangeButton(parent, statusChangeValue, selectedPictureGetter, unSelectedPictureGetter, selectedStatusFunc, clickBehaviour);
			button.Size = ControlDpiScalingHelper.NewScaledSize(23, 23, true);

			if (locationX.HasValue && locationY.HasValue)
			{
				button.Location = ControlDpiScalingHelper.NewScaledPoint(locationX.Value, locationY.Value, true);
			}
			button.BackColor = System.Drawing.SystemColors.Control;

			return button;
		}
	}
}
