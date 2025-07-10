using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public partial class ChangeCategoryPage : ZIFramePageWithFilterStripBizO
	{
		#region Constants

		public static MultilingualString OrCategories { get { return ResString.GetMultilingualString("f2597062-9df3-41f7-aa71-ee46360811e2", "Title"); } }
		public static MultilingualString ClearCategory { get { return ResString.GetMultilingualString("534ee19c-ae71-491b-a578-33ea11c0087a", "Clear"); } }
		public static MultilingualString RedCategory { get { return ResString.GetMultilingualString("08a4c01e-fb3b-4be1-bc53-176ad036de6f", "Red"); } }
		public static MultilingualString GreenCategory { get { return ResString.GetMultilingualString("8b4f1134-50e6-4cfc-9c90-7d28ef47d883", "Green"); } }
		public static MultilingualString BlueCategory { get { return ResString.GetMultilingualString("6d042f41-6823-49c5-a898-b8209d8c3545", "Blue"); } }
		public static MultilingualString BrownCategory { get { return ResString.GetMultilingualString("d3270c75-efec-40f6-a4d2-d3a104546af2", "Brown"); } }
		public static MultilingualString GreyCategory { get { return ResString.GetMultilingualString("b3e43f88-41f3-4672-a007-d222d96299b8", "Grey"); } }

		#endregion

		#region Implementation

		Image GetImage(MultilingualString categoryName)
		{
			Image img = new Image();
			img.ImageUrl = categoryName.GetUnresolvedString().ToLower() + "_category.jpg";
			return img;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "css style value should not be translated")]
		Button GetButton(MultilingualString categoryName)
		{
			Button btn = new Button();
			btn.ID = categoryName.GetUnresolvedString();
			if (categoryName.Equals(ClearCategory))
			{
				btn.Text = Res.GetString("c3527cdd-d1b3-45c0-9e75-148656bcebfc", "Clear Category");
				btn.ToolTip = Res.GetString("484fc2fc-4c7c-4f0b-9665-ebfa3818632a", "Clear category for the current filter.");
			}
			else
			{
				btn.Text = Res.GetString("16afc2db-52e6-4e13-8da2-ccad194613d3", "{0} Category", categoryName);
				btn.ToolTip = Res.GetString("0549c24a-b1e8-4e13-8d58-3885dbb9a1bd", "Change category for the current filter to {0}.", categoryName);
			}

			if (categoryName != OrCategories && BizO != null)
			{
				if (BizO.OrCategory == String2OrCategory(categoryName))
				{
					btn.Style["font-weight"] = "bold";
				}
			}

			btn.Click += new EventHandler(Btn_Click);
			return btn;
		}

		FilterOrCategory String2OrCategory(string categoryName)
		{
			FilterOrCategory result = FilterOrCategory.None;

			if (categoryName == RedCategory.GetUnresolvedString())
			{
				result = FilterOrCategory.Red;
			}
			else if (categoryName == GreenCategory.GetUnresolvedString())
			{
				result = FilterOrCategory.Green;
			}
			else if (categoryName == BlueCategory.GetUnresolvedString())
			{
				result = FilterOrCategory.Blue;
			}
			else if (categoryName == BrownCategory.GetUnresolvedString())
			{
				result = FilterOrCategory.Brown;
			}
			else if (categoryName == GreyCategory.GetUnresolvedString())
			{
				result = FilterOrCategory.Grey;
			}

			return result;
		}

		void Btn_Click(object sender, EventArgs e)
		{
			base.HandleOkButtonClick();
			Button btn = sender as Button;
			if (btn != null)
			{
				string categoryName = btn.ID;
				if (categoryName != OrCategories && BizO != null)
				{
					BizO.OrCategory = String2OrCategory(categoryName);
				}
			}
		}

		#endregion

		#region FilterStripBizO

		public override string QueryStringKey
		{
			get { return ChangeCategoryPopup.DataSourceIndexerQueryStringKey; }
		}

		#endregion

		#region FilterStrip

		FilterStrip BizO
		{
			get { return fFilterStrip ?? (fFilterStrip = GetFilterStripFromSession()); }
		}

		FilterStrip fFilterStrip;

		FilterStrip GetFilterStripFromSession()
		{
			var filterStripPK = GetGuidFromParameter(ChangeCategoryPopup.FilterStripPKQueryStringKey);

			return GetFilterStripFromPK(filterStripPK);
		}

		FilterStrip GetFilterStripFromPK(ZGuid filterStripPK)
		{
			if (FilterStripBizO != null)
			{
				foreach (FilterStrip filterStrip in FilterStripBizO.FilterStrips)
				{
					if (filterStrip.PK == filterStripPK)
					{
						return filterStrip;
					}
				}
			}

			return null;
		}

		#endregion

		#region Overrides

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return BizO;
		}

		protected override bool DisableOKCancelButtons
		{
			get { return true; }
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			if (FormControl != null)
			{
				Table table = new Table();
				table.Rows.Add(GetTableRow(OrCategories));
				table.Rows.Add(GetSeparatorRow());
				table.Rows.Add(GetTableRow(ClearCategory));
				table.Rows.Add(GetTableRow(RedCategory));
				table.Rows.Add(GetTableRow(GreenCategory));
				table.Rows.Add(GetTableRow(BlueCategory));
				table.Rows.Add(GetTableRow(BrownCategory));
				table.Rows.Add(GetTableRow(GreyCategory));

				FormControl.Controls.Add(table);
			}
		}

		static TableRow GetSeparatorRow()
		{
			var row = new TableRow() { CssClass = "CategorySeparator" };
			row.Cells.Add(new TableCell() { ColumnSpan = 2 });

			return row;
		}

		TableRow GetTableRow(MultilingualString categoryName)
		{
			TableRow row = new TableRow();
			row.Height = new Unit("22px");
			row.Cells.Add(new TableCell());
			row.Cells.Add(new TableCell());

			if (!categoryName.Equals(ClearCategory))
			{
				row.Cells[0].Controls.Add(GetImage(categoryName));
			}

			if (categoryName.Equals(OrCategories))
			{
				Label lblTitle = new Label();
				lblTitle.Text = Res.GetString("c796d805-71e3-4fdb-a919-073b6bf22df5", "'Or' Filter Categories");
				lblTitle.CssClass = "SubSectionTitle";

				row.VerticalAlign = VerticalAlign.Top;
				row.Cells[1].Controls.Add(lblTitle);
			}
			else
			{
				row.Cells[1].Controls.Add(GetButton(categoryName));
			}

			return row;
		}

		#endregion

		#region OK

		protected override string[] OKFunctionArguments
		{
			get { return null; }
		}

		#endregion

		#region Cancel

		protected override string[] CancelFunctionArguments
		{
			get { return null; }
		}

		#endregion
	}
}
