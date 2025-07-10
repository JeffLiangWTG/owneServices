using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCOLSLateLodgementReasonUserControl : LongTextControl
	{
		public AUCOLSLateLodgementReasonUserControl()
		{
			InitializeComponent();
			Controls.Remove(LongTextTextBox);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			ReasonDropEdit.SetDataBinding(dataSource, dataMember);
		}

		protected override BusinessObject CurrentItem
		{
			get
			{
				BusinessObject result = null;
				var manager = ReasonDropEdit.DataBindings.Cast<Binding>().FirstOrDefault()?.BindingManagerBase as CurrencyManager;
				if (manager != null && manager.Position != -1)
				{
					result = (BusinessObject)manager.GetCurrent();
				}
				return result;
			}
		}
	}
}
