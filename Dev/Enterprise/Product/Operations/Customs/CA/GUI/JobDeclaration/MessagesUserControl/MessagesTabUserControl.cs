using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class MessagesTabUserControl : Customs.GUI.BaseMessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
			SetupMessageColumns();
		}

		void MessagesGrid_AfterBind(object sender, EventArgs e)
		{
			MessagesGrid.ListManager.PositionChanged += MessagesGridListManager_PositionChanged;
			MessageTextEDITabPage.TabVisible = false;
			MessageDetailsEDITabPage.TabVisible = false;
			MessagesGridListManager_PositionChanged(null, null);
		}

		internal void MessagesGridListManager_PositionChanged(object sender, EventArgs e)
		{
			EDIMessage message = null;
			var listManager = MessagesGrid.ListManager;
			if (listManager != null)
			{
				message = (EDIMessage)listManager.GetCurrent();
				if (message != null && message.IsDeleted)
				{
					message = null;
				}
			}

			if (currentMessage != message)
			{
				currentMessage = message;
				if (currentMessage != null && (!currentMessage.RawMessage.IsEmpty || !currentMessage.RawMessageInterpretation.IsEmpty))
				{
					MessageTextEDITabPage.TabVisible = true;
					MessageDetailsEDITabPage.TabVisible = true;
				}
				else
				{
					MessageTextEDITabPage.TabVisible = false;
					MessageDetailsEDITabPage.TabVisible = false;
				}
			}
		}
		protected EDIMessage currentMessage;

		void SetupMessageColumns()
		{
			MessagesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("MessagesTabUserControl|1AF7439B-DB55-4BDB-8D7E-1571AA4F83BF", "Batch No.", "Batch Number", ""),
				ColumnName = EDIMessage.Schema.BatchNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
			});

			MessagesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("MessagesTabUserControl|471B557E-2701-45EA-B732-C46278C5547F", "Message Schedule"),
				ColumnName = EDIMessage.Schema.MessageScheduleDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(245)
			});

			MessagesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("MessagesTabUserControl|0486E611-4778-4826-B0C9-63EC85D7B304", "Sent With Errors"),
				ColumnName = EDIMessage.Schema.EM_SendWithMessageErrorsFormatted,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105)
			});

			var intStatusColumn = MessagesGrid.GetColumnStyle(EDIMessage.Schema.EM_InterchangeStatus);
			intStatusColumn.IsVisible = true;

			MessagesGrid.ReOrderColumns(MessagesGridSortOrder);
		}

		string[] MessagesGridSortOrder
		{
			get
			{
				if (messagesGridSortOrder == null)
				{
					messagesGridSortOrder = new[]
					{
						EDIMessage.Schema.MessageScheduleDescription,
						EDIMessage.Schema.BatchNumber,
						EDIMessage.Schema.EM_ReceiveTransmit,
						EDIMessage.Schema.EM_MessageType,
						EDIMessage.Schema.EM_MessageSubType,
						EDIMessage.Schema.EM_Status,
						EDIMessage.Schema.EM_User,
						EDIMessage.Schema.EM_MessageDateTime,
						EDIMessage.Schema.EM_SystemCreateTimeUtc,
						EDIMessage.Schema.EM_DateTimeInterchangeSent,
						EDIMessage.Schema.EM_MessageNum,
						EDIMessage.Schema.EM_InterchangeNumber,
						EDIMessage.Schema.EM_InterchangeStatus,
						EDIMessage.Schema.EM_ApplicationReference,
						EDIMessage.Schema.EM_SendWithMessageErrorsFormatted
					};
				}
				return messagesGridSortOrder;
			}
		}
		string[] messagesGridSortOrder;
	}
}
