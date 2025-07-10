using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBCollection : Customs.Business.CusHAWBDependentCollection
	{
		public CusHAWBCollection(CusMAWB parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public new CusHAWB this[int index]
		{
			get { return (CusHAWB)Elements[index]; }
		}

		public new CusHAWB AddNew()
		{
			return (CusHAWB)base.AddNew();
		}

		public new CusHAWB AddNew(Type bizOType)
		{
			return (CusHAWB)base.AddNew(bizOType);
		}

		public bool HasChildResponsePendingOrPrealerted
		{
			get
			{
				bool result = false;
				foreach (CusHAWB child in this)
				{
					result |= child.CS_IsResponsePending || child.CS_IsPrealerted;
					if (result)
					{
						break;
					}
				}
				return result;
			}
		}

		public bool CanDelete
		{
			get
			{
				bool result = true;
				foreach (CusHAWB child in this)
				{
					result &= child.CanDelete;
					if (!result)
					{
						break;
					}
				}
				return result;
			}
		}

		public int NumberOfPackages
		{
			get { return this.Cast<CusHAWB>().Sum(hawb => hawb.CS_PiecesManifested); }
		}

		public CusHAWB[] GetHouseBillsWithPrealertHeld()
		{
			return this.Cast<CusHAWB>().Where(hawb => hawb.CS_IsPrealertHeldByUser).ToArray();
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return MasterBill.Consol == null; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			CusHAWB houseBill = child as CusHAWB;
			if (houseBill != null)
			{
				houseBill.CS_RX_NKGoodsCurrency = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.LocalCurrencyConstantCode;
				houseBill.CS_CustomsStatus = AirCargoMessage.NewStatus.NotSent;
				houseBill.CS_CustomsMainStatus = AirCargoMessage.NewStatus.NotSent;
				houseBill.CS_RL_NKOrigin = MasterBill.CM_RL_NKLoadPort;
				houseBill.CS_RL_NKDestination = MasterBill.CM_RL_NKDischargePort;
				houseBill.CS_WeightUQ = "KG";
				houseBill.HasChanges = false;
			}
		}

		protected CusMAWB MasterBill
		{
			get { return (CusMAWB)Master; }
		}

		#endregion
	}
}
