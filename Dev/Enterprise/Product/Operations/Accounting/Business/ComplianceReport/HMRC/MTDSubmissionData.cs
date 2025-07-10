using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	public class MTDSubmissionData : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MTDSubmissionData(BusinessObjectFactory factory) : base(factory)
		{
			IsReadOnly = false;
		}

		internal bool IsReadOnly { get; set; }

		int LocalDecimals => 2;

		#region Box1
		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box1_VATDue
		{
			get { return box1_VATDue; }
			set { SetNonPersistentPropertyValue(Box1_VATDueInfo, ref box1_VATDue, value); }
		}

		public ZPropertyInfo Box1_VATDueInfo
		{
			get { return GetZPropertyInfo(nameof(Box1_VATDue)); }
		}

		ZDecimal box1_VATDue;
		#endregion

		#region Box2
		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box2_VATDueReverseChg
		{
			get { return box2_VATDueReverseChg; }
			set { SetNonPersistentPropertyValue(Box2_VATDueReverseChgInfo, ref box2_VATDueReverseChg, value); }
		}

		public ZPropertyInfo Box2_VATDueReverseChgInfo
		{
			get { return GetZPropertyInfo(nameof(Box2_VATDueReverseChg)); }
		}

		ZDecimal box2_VATDueReverseChg;
		#endregion

		#region Box3
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box3_TotalVATDue => (box1_VATDue + box2_VATDueReverseChg);
		#endregion

		#region Box4
		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box4_VATReclaimed
		{
			get { return box4_VATReclaimed; }
			set { SetNonPersistentPropertyValue(Box4_VATReclaimedInfo, ref box4_VATReclaimed, value); }
		}

		public ZPropertyInfo Box4_VATReclaimedInfo
		{
			get { return GetZPropertyInfo(nameof(Box4_VATReclaimed)); }
		}

		ZDecimal box4_VATReclaimed;
		#endregion

		#region Box5
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box5_NetVAT => Math.Abs(Box3_TotalVATDue - box4_VATReclaimed);

		public ZPropertyInfo Box5_NetVATInfo
		{
			get { return GetZPropertyInfo(nameof(Box5_NetVAT)); }
		}

		internal void UpdateBox5_NetVATNotification()
		{
			if (Box3_TotalVATDue < box4_VATReclaimed)
			{
				AddBox5_NetVATNotification(Res.GetString("6758d0ca-2f03-4e56-9545-578f125a606b", "VAT is recoverable from HMRC"));
			}
			else if (Box3_TotalVATDue > box4_VATReclaimed)
			{
				AddBox5_NetVATNotification(Res.GetString("f9a525f5-144b-4781-b821-b988328f7a25", "VAT is payable to HMRC"));
			}
			else if (Box5_NetVATInfo.HasNotifications())
			{
				Box5_NetVATInfo.ClearAllNotifications();
			}
		}

		void AddBox5_NetVATNotification(string message)
		{
			if (!Box5_NetVATInfo.HasWarning(message))
			{
				if (Box5_NetVATInfo.HasNotifications())
				{
					Box5_NetVATInfo.ClearAllNotifications();
				}
				Box5_NetVATInfo.AddWarning(message);
			}
		}
		#endregion

		#region Box6
		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box6_TotalSalesExVAT
		{
			get { return box6_TotalSalesExVAT; }
			set { SetNonPersistentPropertyValue(Box6_TotalSalesExVATInfo, ref box6_TotalSalesExVAT, value); }
		}

		public ZPropertyInfo Box6_TotalSalesExVATInfo
		{
			get { return GetZPropertyInfo(nameof(Box6_TotalSalesExVAT)); }
		}

		ZDecimal box6_TotalSalesExVAT;
		#endregion

		#region Box7
		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box7_TotalPurchaseExVAT
		{
			get { return box7_TotalPurchaseExVAT; }
			set { SetNonPersistentPropertyValue(Box7_TotalPurchaseExVATInfo, ref box7_TotalPurchaseExVAT, value); }
		}

		public ZPropertyInfo Box7_TotalPurchaseExVATInfo
		{
			get { return GetZPropertyInfo(nameof(Box7_TotalPurchaseExVAT)); }
		}

		ZDecimal box7_TotalPurchaseExVAT;
		#endregion

		#region Box8
		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box8_GoodsSalesECMembersExVAT
		{
			get { return box8_GoodsSalesECMembersExVAT; }
			set { SetNonPersistentPropertyValue(Box8_GoodsSalesECMembersExVATInfo, ref box8_GoodsSalesECMembersExVAT, value); }
		}

		public ZPropertyInfo Box8_GoodsSalesECMembersExVATInfo
		{
			get { return GetZPropertyInfo(nameof(Box8_GoodsSalesECMembersExVAT)); }
		}

		ZDecimal box8_GoodsSalesECMembersExVAT;
		#endregion

		#region Box9
		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box9_GoodsPurchaseECMembersExVAT
		{
			get { return box9_GoodsPurchaseECMembersExVAT; }
			set { SetNonPersistentPropertyValue(Box9_GoodsPurchaseECMembersExVATInfo, ref box9_GoodsPurchaseECMembersExVAT, value); }
		}

		public ZPropertyInfo Box9_GoodsPurchaseECMembersExVATInfo
		{
			get { return GetZPropertyInfo(nameof(Box9_GoodsPurchaseECMembersExVAT)); }
		}

		ZDecimal box9_GoodsPurchaseECMembersExVAT;
		#endregion

		public void Copy(MTDSubmissionData source)
		{
			Box1_VATDue = source.Box1_VATDue;
			Box2_VATDueReverseChg = source.Box2_VATDueReverseChg;
			Box4_VATReclaimed = source.Box4_VATReclaimed;
			Box6_TotalSalesExVAT = source.Box6_TotalSalesExVAT;
			Box7_TotalPurchaseExVAT = source.Box7_TotalPurchaseExVAT;
			Box8_GoodsSalesECMembersExVAT = source.Box8_GoodsSalesECMembersExVAT;
			Box9_GoodsPurchaseECMembersExVAT = source.Box9_GoodsPurchaseECMembersExVAT;
		}
	}
}
