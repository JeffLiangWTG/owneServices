using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Filter control for GLJournal.
	/// </summary>
	public partial class GLJournalFilterControl : ZFilterStripControl
	{
		public GLJournalFilterControl()
		{
			InitializeComponent();
		}

		public GLJournalFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
