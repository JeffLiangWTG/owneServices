namespace Enterprise.Customs.CA.Business
{
	using System;
	using CargoWise.EntityFramework;

	partial class AutoDutyAndTax
	{
		#region HasChangesChanged

		public new event EventHandler<HasChangesChangedEventArgs> HasChangesChanged
		{
			add { AddInfo.HasChangesChanged += value; }
			remove { AddInfo.HasChangesChanged -= value; }
		}

		#endregion
	}
}
