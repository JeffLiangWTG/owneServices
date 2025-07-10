using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class GenericChargeFilterControl : ZFilterStripControl
	{
		public GenericChargeFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Overrides

		protected override void UpdateNumberLoadedMessageCore(ZString message, int numberOfRecordsFound, bool shouldShowNumberLoadedMessageBox)
		{
			var msgToShow = message;

			if (numberOfRecordsFound == 0)
			{
				var source = FilterBusinessObject as IWarningMessageProvider;
				if (source != null)
				{
					var additionalMsg = source.GetWarningMessage();
					if ((ZString)null != additionalMsg && !additionalMsg.IsEmpty)
					{
						msgToShow = message + System.Environment.NewLine + source.GetWarningMessage();
					}
				}
			}

#if DEBUG
			PopupMessageCacheForTest = msgToShow;
#endif
			base.UpdateNumberLoadedMessageCore(msgToShow, numberOfRecordsFound, shouldShowNumberLoadedMessageBox);
		}

#if DEBUG
		public ZString PopupMessageCacheForTest;
#endif

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				if (!AccountingMasterFilesUtils.HasGLAccountSelectionAndEntry)
				{
					grid.RemoveFromAvailableColumns("AlternateAccounts");
				}
			}
		}

		#endregion
	}
}
