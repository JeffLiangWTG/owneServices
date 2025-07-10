using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public abstract class MessageSendingFormBuilder
	{
		public ZTextBoxColumnStyleInfo[] GetColumnStyles()
		{
			var result = new List<ZTextBoxColumnStyleInfo>();
			var sendColumn = GetSendColumnStyle();
			if (sendColumn != null)
			{
				result.Add(sendColumn);
			}
			result.AddRange(GetAdditionalColumnStyles());
			return result.ToArray();
		}

		public virtual ZTextBoxColumnStyleInfo GetSendColumnStyle()
		{
			var result = new ZCheckBoxColumnStyleInfo() {
					ColumnName = nameof(JobDeclarationMessageSendingObject.ShouldSend),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
					IsMandatory = true,
			};
			return result;
		}

		protected abstract ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles();

		public abstract ZUserControl GetUserControl();
		public abstract ResourceStringData GetUserControlGroupBoxCaption();
		public abstract ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingObjectsGrid);

		public virtual int[] GetFormSize() => new int[] { 700, 460 };

		public virtual void ChangeValidationErrorBindingIfNeeded(KBindingSource bindingSource, ZTextBox validationErrorsTextBox) { }

		protected const int Panel1MinSizeForNoUserControl = 120;
		protected const int Panel1MinSizeForGridUserControl = 268;
		protected const int Panel1MinSizeForMultiUserControls = 380;

		public virtual int Panel1MinSize => Panel1MinSizeForNoUserControl;
		public virtual int Panel2MinSize => 0;

		protected ZTextBoxColumnStyleInfo[] GetEntryNumberColumnStyle()
		{
			var result = new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobDeclarationMessageSendingObject.FormattedEntryNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				}
			};
			return result;
		}

		protected ZTextBoxColumnStyleInfo[] GetCommonAmendmentColumnStyleInfo()
		{
			var result = new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentVersion,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					IsMandatory = true,
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentTypeDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsMandatory = true,
					IsReadOnly = true
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentReason,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsMandatory = true,
				}
			};
			return result;
		}
	}
}
