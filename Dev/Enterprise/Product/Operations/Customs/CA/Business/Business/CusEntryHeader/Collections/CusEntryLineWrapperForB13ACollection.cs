using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CusEntryLineWrapperForB13ACollection : NonPersistentBusinessObjectCollection<CusEntryLineWrapperForB13A>
	{
		#region Overrides of NonPersistentBusinessObjectCollection

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CusEntryLineWrapperForB13A();
		}

		#endregion
	}
}
