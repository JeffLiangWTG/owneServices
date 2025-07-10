using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class CusContainerUserControl : BaseCustomsCusContainersWithTrackingUserControl
	{
		public CusContainerUserControl()
		{
			var statusControl = new ContainerStatusDropEditColumnStyleInfo();
			statusControl.Caption = "Status";
			statusControl.ColumnName = CusContainer.Schema.CO_MessageStatus;
			CusContainersBoundGrid.ColumnStyles.Add(statusControl);
		}

		internal class ContainerStatusDropEditColumnStyle : ZDropEditColumnStyle
		{
			readonly CodeDescriptionPairList containerStatus = new Business.Declaration.ContainerStatusCodesList();

			public ContainerStatusDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo) : base(columnInfo)
			{
			}

			protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum) => GetContainerStatus(source.List[rowNum]);

			protected override string FormatValueObjectCore(object source, object propertyValue) => GetContainerStatus(source);

			string GetContainerStatus(object source)
			{
				if (source is CusContainer container)
				{
					return containerStatus.GetDescriptionFromCode(container.CO_MessageStatus) ?? string.Empty;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		internal class ContainerStatusDropEditColumnStyleInfo : ZDropEditColumnStyleInfo
		{
			public override Type ColumnStyleType => typeof(ContainerStatusDropEditColumnStyle);
		}
	}
}
