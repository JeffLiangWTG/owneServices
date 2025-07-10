using System;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class CusMiscRequestHeaderViewUserControl : ZUserControl
	{
		public CusMiscRequestHeaderViewUserControl()
		{
			InitializeComponent();
		}

		public new CusMiscRequestHeader CurrentDataItem => base.CurrentDataItem as CusMiscRequestHeader;
		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetDynamicHeaderDetailsPanelLayout();
		}

		void SetDynamicHeaderDetailsPanelLayout()
		{
			if (CurrentDataItem != null)
			{
				switch (CurrentDataItem.CMR_MessageType)
				{
					case ElectronicDocumentTypeList.Codes._5AC:
						DynamicHeaderDetailsPanel.UpdateLayout(new ExtendedHoursRequestViewLayout());
						DynamicRequestLinePanel.UpdateLayout(new MiscRequestRelatedEntries5ACLayout());
						RemoveGridColumnsEntryType();
						break;
					case ElectronicDocumentTypeList.Codes._5GW:
						DynamicHeaderDetailsPanel.UpdateLayout(new ExtendedHoursRequestViewLayout());
						DynamicRequestLinePanel.UpdateLayout(new MiscRequestRelatedEntries5GWLayout());
						break;
					case ElectronicDocumentTypeList.Codes._5SG:
						DynamicHeaderDetailsPanel.UpdateLayout(new FinalPriceReportViewLayout());
						DynamicRequestLinePanel.UpdateLayout(new MiscRequestRelatedEntries5SGLayout());
						RemoveGridColumnsEntryType();
						break;
					default:
						break;
				}
				ReOrderColumns();
			}
		}
		void RemoveGridColumnsEntryType()
		{
			CusMiscRequestLinesBoundGrid.GetColumnStyle(nameof(CusMiscRequestLine.EntryType)).IsUnavailable = true;
		}

		void ReOrderColumns()
		{
			CusMiscRequestLinesBoundGrid.ReOrderColumnsAndChangeVisibility(headerColumns);
		}

		readonly string[] headerColumns =
		{
				nameof(CusMiscRequestLine.EntryType),
				nameof(CusMiscRequestLine.FormattedEntryNumber),
				nameof(CusMiscRequestLine.CML_Remarks),
		};
	}
}
