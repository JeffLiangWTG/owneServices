using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class SingleLineEntryValidation : AutoSingleLineEntryValidation
	{
		public SingleLineEntryValidation(AutoSingleLineEntry parent)
			: base(parent) { }

		#region Implementation

		public new SingleLineEntry Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (SingleLineEntry)base.Parent; }
		}

		protected override void CheckCurrency()
		{
			base.CheckCurrency();
			ListValidation.ErrorIfInvalidPK(Parent.CurrencyInfo);
		}
		#endregion
	}
}
