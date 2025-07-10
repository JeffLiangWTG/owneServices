using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Module
{
	public partial class CAQueryMessagesFilterControl : Enterprise.Messaging.Module.EDIMessageFilterControl
	{
		public CAQueryMessagesFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("CAQueryMessagesFilterControl|AE9C2863-DE59-4C3A-A542-798618F1E696", "Batch", "Batch Number", "");
			zTextBoxColumnStyleInfo1.ColumnName = EDIMessage.Schema.BatchNumber;
			zTextBoxColumnStyleInfo1.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo1, 60, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageType, 40);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageSubTypeDescription, 100);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_SendOrReceiveHumanReadable, 55);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_Status, 40);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageDateTime, 95);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_HeldUntilDate, 95);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageSubType, 40);
				FilteredGrid.ReOrderColumns(EDIMessageFilterControlDefaultColumnsSequence);
			}
		}

		string[] EDIMessageFilterControlDefaultColumnsSequence
		{
			get
			{
				if (eDIMessageFilterControlDefaultColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						EDIMessage.Schema.BatchNumber,
						EDIMessage.Schema.EM_MessageType,
						EDIMessage.Schema.EM_MessageSubTypeDescription,
						EDIMessage.Schema.EM_SendOrReceiveHumanReadable,
						EDIMessage.Schema.EM_Status,
						EDIMessage.Schema.EM_MessageDateTime,
						EDIMessage.Schema.EM_InterchangeSender,
						EDIMessage.Schema.EM_SendingUser,
						EDIMessage.Schema.EM_MessageNum,
						EDIMessage.Schema.EM_MessageSubType,
						EDIMessage.Schema.EM_DateTimeInterchangeSent,
						EDIMessage.Schema.EM_InterchangeNumber,
						EDIMessage.Schema.EM_InterchangeStatus,
						EDIMessage.Schema.EM_ApplicationCode,
						EDIMessage.Schema.EM_ApplicationReference,
						EDIMessage.Schema.EM_MessageTextShort
					};
					eDIMessageFilterControlDefaultColumnsSequence = columnList.ToArray();
				}
				return eDIMessageFilterControlDefaultColumnsSequence;
			}
		}
		string[] eDIMessageFilterControlDefaultColumnsSequence;
	}
}
