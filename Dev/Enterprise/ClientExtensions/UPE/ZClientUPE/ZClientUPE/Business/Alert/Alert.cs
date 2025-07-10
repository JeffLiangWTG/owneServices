using System.Collections.Specialized;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class Alert : NonPersistentBusinessObject, IObsoleteValidation
	{
		public Alert(StringCollection alertsList, BusinessObjectFactory factory)
			: base(factory)
		{
			this.AlertsList = new StringBuilder(alertsList.Count);
			foreach (string s in alertsList)
			{
				this.AlertsList.Append(s);
				this.AlertsList.Append("\n");
			}
		}

		#region Alerts

		[CargoWise.ComponentModel.MaxLength(8000)]
		public ZString Alerts
		{
			get { return AlertsList.ToString(); }
		}

		public ZPropertyInfo AlertsInfo
		{
			get { return GetZPropertyInfo(nameof(Alerts)); }
		}

		#endregion

		readonly StringBuilder AlertsList;
	}
}
