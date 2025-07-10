using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.WIPAccrual;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals
{
	public class WIPAccrualPRBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public WIPAccrualPRBusinessObject(BaseWIPAccrual wIP, Xsd.WipOrAccrualPostOrReverse postOrReverse) : base(wIP.Factory)
		{
			this.WIP = wIP;
			this.PostOrReverse = postOrReverse;
		}

		public Xsd.WipOrAccrualPostOrReverse PostedOrReverseStatus
		{
			get { return PostOrReverse; }
		}

		public readonly BaseWIPAccrual WIP;
		readonly Xsd.WipOrAccrualPostOrReverse PostOrReverse;
	}
}
