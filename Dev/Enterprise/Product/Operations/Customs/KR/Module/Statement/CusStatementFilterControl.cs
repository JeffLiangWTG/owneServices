
using CargoWise.EntityFramework;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.KR.Module
{
	/// <summary>
	/// Filter control for Statements.
	/// </summary>
	public partial class CusStatementFilterControl : ZFilterStripControl<StatementFilterStrip>
	{
		private readonly System.ComponentModel.Container components;

		public CusStatementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
