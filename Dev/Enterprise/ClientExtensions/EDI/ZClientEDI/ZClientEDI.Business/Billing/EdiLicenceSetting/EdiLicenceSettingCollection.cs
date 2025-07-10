using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiLicenceSettingCollection : ActiveBusinessObjectCollection<EdiLicenceSetting>
	{
		public EdiLicenceSettingCollection(LicenceDatabase master)
			: base(master.Factory, master, new ZQuery(), EdiLicenceSettingSchema.LS9_LD)
		{
			Master = master;
		}

		public EdiLicenceSettingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public readonly LicenceDatabase Master;

		#region Implementation

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}
}

