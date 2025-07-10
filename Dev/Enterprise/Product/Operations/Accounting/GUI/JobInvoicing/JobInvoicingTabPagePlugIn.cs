using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	/// <summary>
	/// We can not just use ZAutoSizedTabPagePlugIn because the PlugIn management alters the 
	/// order of appearance of PlugIn TabPages based on the type of the TabPage - which pretty
	/// much makes ZAutoSizedTabPagePlugIn useless for general use.
	/// 
	/// ZAutoSizedTabPagePlugIn is last in the order so if InvoicingPlugIn used this type, 
	/// the Billing tab would appear over to the right, next to eDocs and Notes etc. By using 
	/// our own type that Z doesn't known about, we get around this problem - CM.
	/// </summary>

	public partial class JobInvoicingTabPagePlugIn : ZTabPagePlugIn
	{
		public JobInvoicingTabPagePlugIn(ZPlugIn plugIn)
			: base(plugIn)
		{
		}

		protected override bool IsAutoSized
		{
			get { return true; }
		}
	}
}
