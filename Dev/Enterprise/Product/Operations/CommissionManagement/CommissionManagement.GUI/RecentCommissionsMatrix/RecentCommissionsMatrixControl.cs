using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class RecentCommissionsMatrixControl : ZUserControl
	{
		public RecentCommissionsMatrixControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var recentCommissionsMatrix = DataSource as RecentCommissionsMatrix;
			if (recentCommissionsMatrix != null)
			{
				var referenceDate = recentCommissionsMatrix.ReferenceDate;
				RecentCommissionsMatrixGrid.SetColumnCaption(RecentCommissionsRow.Schema.TotalPreviousMonth, GetMonthCaption(referenceDate.AddMonths(-1)));
				RecentCommissionsMatrixGrid.SetColumnCaption(RecentCommissionsRow.Schema.Total2MonthsAgo, GetMonthCaption(referenceDate.AddMonths(-2)));
				RecentCommissionsMatrixGrid.SetColumnCaption(RecentCommissionsRow.Schema.Total3MonthsAgo, GetMonthCaption(referenceDate.AddMonths(-3)));
			}
		}

		static string GetMonthCaption(ZDateTime date)
		{
			var monthList = new string[]
			{
				string.Empty,
				Res.GetString("d11cd27f-83ae-4d30-92c7-e27c9c61e6bc", "January"),
				Res.GetString("a35ae97f-efd8-4c66-948d-8cac5d9472d7", "February"),
				Res.GetString("c11bdb60-fcce-4166-a2af-4ad4a4dd3145", "March"),
				Res.GetString("52a5c1d5-911e-4c00-9ee6-bcd31d868b51", "April"),
				Res.GetString("1fba2707-3f23-4292-8adc-ef830f5474a5", "May"),
				Res.GetString("5c766b4a-b5f9-4a42-8f77-c34ee1aa3d83", "June"),
				Res.GetString("1df97070-4e6a-4257-95d0-538557e52013", "July"),
				Res.GetString("23ac4b34-741c-4c95-8ed0-e7e09fac6210", "August"),
				Res.GetString("cc4f88bd-c15f-47e4-9ead-e9ab5c4993e3", "September"),
				Res.GetString("f7b908d4-4cd6-4490-a56f-9409865641c4", "October"),
				Res.GetString("7524036c-107d-4189-a233-d0311b0c8dad", "November"),
				Res.GetString("3bc06dc0-eac9-41ff-9261-371dbf15d0ad", "December"),
			};

			return monthList[date.Month];
		}
	}
}
