using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class OrgCarrierAccountWrapper : GenericWrapper
	{
		public OrgCarrierAccountWrapper(OrgCarrierAccount account, BusinessObjectFactory factory)
			: base(account, factory)
		{
			OrgCarrierAccountBO = account;
		}

		public ZString MerchantID
		{
			get { return OrgCarrierAccountBO != null ? OrgCarrierAccountBO.OAN_MerchantNumber : ZString.Empty; }
		}

		public ZString MerchantLocationID
		{
			get { return OrgCarrierAccountBO != null ? OrgCarrierAccountBO.OAN_DepotID : ZString.Empty; }
		}

		public ZString AccountNumber
		{
			get { return OrgCarrierAccountBO != null ? OrgCarrierAccountBO.OAN_AccountNumber : ZString.Empty; }
		}

		#region Implementation

		OrgCarrierAccount OrgCarrierAccountBO { get; }

		#endregion
	}
}
