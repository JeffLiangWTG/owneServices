using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Module
{
	public partial class K84ReportsFilterControl : Enterprise.Messaging.Module.EDIMessageFilterControl
	{
		public K84ReportsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			var zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("K84ReportsFilterControl|B0B921C1-1E5D-4456-9599-F55EB806B96B", "Statement", "Statement Date", "");
			zDateEditColumnStyleInfo1.ColumnName = EDIMessage.Schema.K84StatementDate;
			zDateEditColumnStyleInfo1.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo1, 80, true);
			zDateEditColumnStyleInfo1.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			var zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("K84ReportsFilterControl|6C020CD6-8ED1-4E3C-9AF6-9D2F568F925D", "Accounting", "Accounting Date", "");
			zDateEditColumnStyleInfo2.ColumnName = EDIMessage.Schema.K84AccountingDate;
			zDateEditColumnStyleInfo2.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo2, 80, true);
			zDateEditColumnStyleInfo2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("K84ReportsFilterControl|680F2D18-17BC-40DA-A384-06D39D296CBD", "Statement BN9");
			zTextBoxColumnStyleInfo1.ColumnName = EDIMessage.Schema.K84StatementBN9;
			zTextBoxColumnStyleInfo1.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo2, 80, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
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
						EDIMessage.Schema.EM_MessageType,
						EDIMessage.Schema.EM_MessageSubType,
						EDIMessage.Schema.EM_MessageSubTypeDescription,
						EDIMessage.Schema.K84StatementDate,
						EDIMessage.Schema.K84AccountingDate,
						EDIMessage.Schema.EM_MessageDateTime,
						EDIMessage.Schema.EM_InterchangeSender,
						EDIMessage.Schema.EM_SendingUser,
						EDIMessage.Schema.EM_Status,
						EDIMessage.Schema.EM_SendOrReceiveHumanReadable,
						EDIMessage.Schema.EM_MessageNum,
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
