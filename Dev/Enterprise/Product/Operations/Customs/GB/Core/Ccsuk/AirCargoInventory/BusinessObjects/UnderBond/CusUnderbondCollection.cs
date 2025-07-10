using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusUnderbondCollection<T> : DependentBusinessObjectCollection<T, CusHAWB> where T : CusUnderbond
	{
		public CusUnderbondCollection(CusHAWB parent)
			: base(parent)
		{
			this.parentHawb = parent;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(new ZQuery(CusUnderbondSchema.C4_MovementReason, RemovalTypeAttributeHelper.RemovalTypeCode));
			return result;
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(T);
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			return base.AddNewCore(typeof(T));
		}

		RemovalTypeAttribute RemovalTypeAttributeHelper
		{
			get { return typeAttribute ?? (typeAttribute = RemovalTypeAttribute.Get(typeof(T))); }
		}
		RemovalTypeAttribute typeAttribute;

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusUnderbondSchema.C4_ParentID; }
		}

		protected override bool AllowNewCore
		{
			get
			{
				if (!base.AllowNewCore)
				{
					return false;
				}

				if (parentHawb.CS_IsMasterHouse && !parentHawb.MAWB.IsBasic)
				{
					return false;  // not allowed for consols
				}

				if ((parentHawb.CS_IsMasterHouse && parentHawb.MAWB.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_CanCreateUnderbond))
					|| (!parentHawb.CS_IsMasterHouse && parentHawb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_CanCreateUnderbond)))
				{
					// If parent Awb has splits then allow removals... for those splits.

					var prearrival = (parentHawb == null) ? ZBool.True : parentHawb.IsPrearrival;
					if (prearrival)
					{   //HMRC accreditation rule 6
						return false;
					}

					if (parentHawb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc)
					{
						return false;  // ERTS accreditation - cannot make new underbond under P5 record has been uploaded to CCSUK (send FRC)
					}

					if (RemovalTypeAttributeHelper.RemovalTypeCode == InterShedRemoval.RemovalCode)
					{   // HMRC accreditation rule 3h.
						return (LicenceAndPimaHelper.IsFullShed(parentHawb) || LicenceAndPimaHelper.IsFallbackShed(parentHawb));
					}
					else if (parentHawb != null && parentHawb.IsProfileAShed)
					{
						return false; // IAR, TSR, FBK:  only agents are allowed to make these remoavals. 
					}
					return true;  // no reasin why not
				}
				return false;  // the permission helper said no
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				if (!base.AllowRemoveCore)
				{
					return false;
				}

				return parentHawb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.Delete);
			}
		}

		protected CusHAWB parentHawb;
	}
}
