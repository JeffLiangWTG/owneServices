using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class TestSearchControl : SearchControl
	{
		public TestSearchControl()
		{
			SetupInternals();
		}

		void SetupInternals()
		{
			SearchResultsDataGrid = new ZDataGrid();
			SearchResultsDataGrid.ID = "SearchResultsDataGrid";
			SearchResultsDataGrid.AllowPaging = true;
			SearchResultsDataGrid.CssClass = CssConstants.ResultsTable;
			SearchResultsDataGrid.AutoGenerateColumns = false;
			SearchResultsDataGrid.Width = new Unit("100%");
			SearchResultsDataGrid.PagerStyle.Mode = PagerMode.NumericPages;
			SearchResultsDataGrid.ItemStyle.CssClass = CssConstants.DetailsCell;
			SearchResultsDataGrid.HeaderStyle.CssClass = CssConstants.DetailsHeader;
			SearchResultsDataGrid.AlternatingItemStyle.CssClass = CssConstants.DetailsAlternatingCell;
			SearchResultsDataGrid.SelectedItemStyle.CssClass = CssConstants.DetailsSelectedCell;

			Buttons = new PlaceHolder();

			GridLayoutControl = new ZGridLayoutControl();

			FilterControl = new HtmlGenericControl("DIV");
			FilterControl.ID = "FilterControl";
			FilterControl.Attributes[nameof(HtmlTextWriterAttribute.Class)] = CssConstants.ContentSection;

			ResultsGridDiv = new HtmlGenericControl("DIV");
			ResultsGridDiv.ID = "ResultsGridDiv";
			ResultsGridDiv.Style.Add("OVERFLOW", "auto");
			ResultsGridDiv.Style.Add("WIDTH", "100%");
			ResultsGridDiv.Controls.Add(SearchResultsDataGrid);

			Controls.Add(FilterControl);
			Controls.Add(Buttons);
			Controls.Add(GridLayoutControl);
			Controls.Add(ResultsGridDiv);
		}

		public void OnLoad()
		{
			SetupGrid();
			OnLoad(EventArgs.Empty);
		}

		public void SetupGridExposed()
		{
			SetupGrid();
		}

		public void SetModuleForTest(ZFilterGridModule module)
		{
			this.fModule = module;
		}

		public void Initialise()
		{
			base.OnInit(EventArgs.Empty);
		}

		public new HtmlGenericControl ResultsGridDiv
		{
			get { return base.ResultsGridDiv; }
			set { base.ResultsGridDiv = value; }
		}

		public void SearchResultsDataGrid_PageIndexChanged(int pageIndex)
		{
			base.SearchResultsDataGrid_PageIndexChanged(SearchResultsDataGrid, new DataGridPageChangedEventArgs(SearchResultsDataGrid, pageIndex));
		}

		protected override void LoadFilterControl(ZFilterGridModule module)
		{
			BaseUserControl filterUserControl = Activator.CreateInstance(module.FilterControlType) as BaseUserControl;
			FilterControl.Controls.AddAt(0, filterUserControl);
		}

		public new StateBag ViewState
		{
			get { return base.ViewState; }
		}

		protected override void Find()
		{
			base.Find();

			FindButtonClicked = true;
		}
		public bool FindButtonClicked;
	}
}
