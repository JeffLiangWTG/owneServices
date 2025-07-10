using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class SelectingLinesForm : ZChildForm
	{
		public SelectingLinesForm(ClassificationLineWrapperCollection collection)
			: base(collection)
		{
			this.collection = collection;
			InitializeComponent();
		}
		readonly ClassificationLineWrapperCollection collection;

		void SelectAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (ClassificationLine1Wrapper line in this.collection)
			{
				line.IsSelected = true;
			}
		}
	}
}
