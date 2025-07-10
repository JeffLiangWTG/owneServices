using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ForeignOperatorsUserControl : ZUserControl
	{
		public ForeignOperatorsUserControl()
		{
			InitializeComponent();
			AddColumns();
			DefaultColumnsInOrder();
		}

		void AddColumns()
		{
			if (BRCustomsDataRegistry.Instance.EnableForeignOperator.Value)
			{
				CreateNewTextBoxColumn(ForeignOperator.Schema.ForeignOperatorName, 130);
				CreateNewGuidFindBoxColumn(ForeignOperator.Schema.CGI_BFR_ForeignOperator, 80, Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.BR.ForeignOperator);
			}
		}

		void CreateNewGuidFindBoxColumn(ZString column, ZInt length, ModuleIdentifier module)
		{
			var zGuidFindBoxColumnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
			zGuidFindBoxColumnStyleInfo.ColumnName = column;
			zGuidFindBoxColumnStyleInfo.ModuleID = module;
			zGuidFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			ForeignOperatorsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo);
		}

		void CreateNewTextBoxColumn(ZString column, ZInt length)
		{
			var zTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo.ColumnName = column;
			zTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zTextBoxColumnStyleInfo.IsVisible = true;
			ForeignOperatorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
		}

		void DefaultColumnsInOrder()
		{
			var columns = new List<string>
			{
				ForeignOperator.Schema.IsKnow,
			};

			if (BRCustomsDataRegistry.Instance.EnableForeignOperator.Value)
			{
				columns.AddRange(new string[]
				{
					ForeignOperator.Schema.CGI_BFR_ForeignOperator,
					ForeignOperator.Schema.ForeignOperatorName,
				});
			}

			columns.AddRange(new string[]
			{
				ForeignOperator.Schema.CountryCode,
				ForeignOperator.Schema.AuthorityCode,
				ForeignOperator.Schema.CGI_CustomsStatusDescription
			});

			var defaultColumns = columns.ToArray();

			ForeignOperatorsGrid.SetAllColumnsVisible(false);
			ForeignOperatorsGrid.SetColumnVisible(true, defaultColumns);
			ForeignOperatorsGrid.ReOrderColumns(defaultColumns);
		}
	}
}
