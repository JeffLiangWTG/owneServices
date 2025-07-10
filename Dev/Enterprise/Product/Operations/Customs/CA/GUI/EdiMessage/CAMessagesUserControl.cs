using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAMessagesUserControl : Customs.GUI.MessagesUserControl
	{
		public CAMessagesUserControl()
			: base()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				SetupNewColumns();
			}
		}

		void SetupNewColumns()
		{
			this.SuspendLayout();

			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();

			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAReleaseNotificationsFilterControl|5856A4A5-7C8A-4AED-83E6-062F21CF7236", "Tran. #", "Transaction #", "Transaction Number", "");
			zTextBoxColumnStyleInfo1.ColumnName = EDIMessage.Schema.TransactionNumber;
			zTextBoxColumnStyleInfo1.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo1, 100, true);
			MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAReleaseNotificationsFilterControl|C78DCFB5-B0E8-4D8B-A767-8406072FD4B6", "CCN", "Cargo Control Number", "");
			zTextBoxColumnStyleInfo2.ColumnName = EDIMessage.Schema.CargoControlNumber;
			zTextBoxColumnStyleInfo2.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo2, 100, true);
			MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAReleaseNotificationsFilterControl|8336C34E-9DAB-45DF-B5E9-BD5CA0FF7FA9", "Sub-Loc.", "Sub-Location Code", "");
			zTextBoxColumnStyleInfo3.ColumnName = EDIMessage.Schema.SubLocation;
			zTextBoxColumnStyleInfo3.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo3, 50, true);
			MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);

			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAReleaseNotificationsFilterControl|8AA8E3BA-8AF6-4F48-8CE9-E726AF6FEA72", "CBSA Office", "CBSA Office code", "");
			zTextBoxColumnStyleInfo4.ColumnName = EDIMessage.Schema.CBSAOffice;
			zTextBoxColumnStyleInfo4.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo4, 50, true);
			MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);

			MessagesBoundGrid.SetColumnVisible(true, EDIMessage.Schema.EM_InterchangeStatus);
			MessagesBoundGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageType, 40);
			MessagesBoundGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageSubType, 40);
			MessagesBoundGrid.SetColumnWidth(EDIMessage.Schema.EM_ReceiveTransmit, 40);
			MessagesBoundGrid.SetColumnWidth(EDIMessage.Schema.EM_Status, 40);
			MessagesBoundGrid.SetColumnWidth(EDIMessage.Schema.EM_InterchangeStatus, 40);

			MessagesBoundGrid.ReOrderColumns(ReorderedColumnsSequence);
			this.BindingSource.SetBindingMember(this.MessagesBoundGrid, "MessagesForDisplay");
			this.BindingSource.SetBindingMember(this.HtmlInterpretationBox, "MessagesForDisplay.EM_MessageInterpretation");
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "MessagesForDisplay.EM_FormattedMessageText");

			this.ResumeLayout(false);
		}

		string[] ReorderedColumnsSequence
		{
			get
			{
				if (reorderedColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						EDIMessage.Schema.EM_MessageType,
						EDIMessage.Schema.EM_MessageSubType,
						EDIMessage.Schema.EM_ReceiveTransmit,
						EDIMessage.Schema.EM_Status,
						EDIMessage.Schema.EM_InterchangeStatus,
						EDIMessage.Schema.EM_User,
						EDIMessage.Schema.EM_MessageDateTime,
						EDIMessage.Schema.CargoControlNumber,
						EDIMessage.Schema.TransactionNumber,
						EDIMessage.Schema.SubLocation,
						EDIMessage.Schema.CBSAOffice
					};
					reorderedColumnsSequence = columnList.ToArray();
				}
				return reorderedColumnsSequence;
			}
		}
		string[] reorderedColumnsSequence;
	}
}
