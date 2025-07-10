using CargoWise.EntityFramework;

namespace Enterprise.ResourceStrings.Business
{
	public class HelpDataStringCollection : NonPersistentBusinessObjectCollection<HelpDataString>
	{
		public HelpDataStringCollection()
		{ }

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HelpDataString();
		}

		#endregion
	}
}
