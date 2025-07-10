using System;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public partial class BoardFiltersForm : ZChildForm
	{
		public BoardFiltersForm()
		{
			InitializeComponent();
		}

		public BoardFiltersForm(IFilterable filterable)
			: base(new BoardFilterBusinessObjectCollection(filterable))
		{
			InitializeComponent();
		}

		public new BoardFilterBusinessObjectCollection BusinessEntity
		{
			get { return (BoardFilterBusinessObjectCollection)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("e71f8ea6-b797-49a5-ae62-701e656a97e2", "Applied Filters"); }
		}

		void ClearAllFiltersButton_Click(object sender, EventArgs e)
		{
			using (new ZWaitCursorChanger(this))
			{
				BusinessEntity.RemoveAndDeleteAll();
			}
			Close();
		}
	}
}
