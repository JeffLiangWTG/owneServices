namespace Enterprise.CommissionManagement.Business
{
	public class CommissionFinalizerLineItem : AutoCommissionFinalizerLineItem, ISelectableViewCommissionLineProvider
	{
		#region Constructors

		public CommissionFinalizerLineItem(ViewCommissionLine viewCommissionLine)
			: base(viewCommissionLine != null ? viewCommissionLine.Factory : null)
		{
			this.viewCommissionLine = viewCommissionLine;

			if (viewCommissionLine != null)
			{
				RegisterEditableChildObject(viewCommissionLine);
			}
		}

		#endregion

		#region CommissionLine

		public ViewCommissionLine ViewCommissionLine
		{
			get { return viewCommissionLine; }
		}
		readonly ViewCommissionLine viewCommissionLine;

		#endregion
	}
}
