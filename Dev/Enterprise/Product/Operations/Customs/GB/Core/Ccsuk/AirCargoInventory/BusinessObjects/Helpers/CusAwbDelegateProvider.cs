
namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusAwbDelegateProvider
	{
		public CusAwbDelegateProvider(GetCusAwbDelegate getCusAwbDelegate)
			: base()
		{
			this.getCusAwbDelegate = getCusAwbDelegate;
		}

		public delegate ICcsukCusAwb GetCusAwbDelegate();

		public ICcsukCusAwb Awb
		{
			get { return getCusAwbDelegate == null ? null : getCusAwbDelegate(); }
		}
		readonly GetCusAwbDelegate getCusAwbDelegate;
	}
}
