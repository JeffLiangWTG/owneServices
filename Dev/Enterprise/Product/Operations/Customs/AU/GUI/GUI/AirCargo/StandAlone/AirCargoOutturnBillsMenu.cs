using CargoWise.Common;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirCargoOutturnBillsMenu : CMRMessageManagementMenu
	{
		#region Construction

		protected AirCargoOutturnBillsMenu(CusUnderbondMessageManager manager) : base(manager)
		{
			this.Text = "&Air Cargo";
		}

		protected delegate AirCargoOutturnBillsMenu NewDelegate(CusUnderbondMessageManager manager);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public static AirCargoOutturnBillsMenu New(CusUnderbondMessageManager manager)
		{
			AirCargoOutturnBillsMenu result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden == null)
			{
				result = new AirCargoOutturnBillsMenu(manager);
			}
			else
			{
				result = overridden(manager);
			}
			return result;
		}

		#endregion

		#region Messenger

		protected CusUnderbondMessageManager Manager
		{
			get { return (CusUnderbondMessageManager)manager; }
		}

		#endregion
	}
}
