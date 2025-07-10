using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA
{
	public class LIQSubmissionData : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LIQSubmissionData(BusinessObjectFactory factory) : base(factory)
		{
			IsReadOnly = false;
		}

		internal bool IsReadOnly { get; set; }

		int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#region Box1

		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box1_TotalVatBaseReceivables
		{
			get { return box1_TotalVatBaseReceivables; }
			set { SetNonPersistentPropertyValue(Box1_TotalVatBaseReceivablesInfo, ref box1_TotalVatBaseReceivables, value); }
		}

		public ZPropertyInfo Box1_TotalVatBaseReceivablesInfo
		{
			get { return GetZPropertyInfo(nameof(Box1_TotalVatBaseReceivables)); }
		}

		ZDecimal box1_TotalVatBaseReceivables;

		#endregion

		#region Box2

		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box2_TotalVatReceivables
		{
			get { return box2_TotalVatReceivables; }
			set { SetNonPersistentPropertyValue(Box2_TotalVatReceivablesInfo, ref box2_TotalVatReceivables, value); }
		}

		public ZPropertyInfo Box2_TotalVatReceivablesInfo
		{
			get { return GetZPropertyInfo(nameof(Box2_TotalVatReceivables)); }
		}

		ZDecimal box2_TotalVatReceivables;

		#endregion

		#region Box3

		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box3_TotalVatBasePayables
		{
			get { return box3_TotalVatBasePayables; }
			set { SetNonPersistentPropertyValue(Box3_TotalVatBasePayablesInfo, ref box3_TotalVatBasePayables, value); }
		}

		public ZPropertyInfo Box3_TotalVatBasePayablesInfo
		{
			get { return GetZPropertyInfo(nameof(Box3_TotalVatBasePayables)); }
		}

		ZDecimal box3_TotalVatBasePayables;

		#endregion

		#region Box4

		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box4_TotalVatPayablesRecoverable
		{
			get { return box4_TotalVatPayablesRecoverable; }
			set { SetNonPersistentPropertyValue(Box4_TotalVatPayablesRecoverableInfo, ref box4_TotalVatPayablesRecoverable, value); }
		}

		public ZPropertyInfo Box4_TotalVatPayablesRecoverableInfo
		{
			get { return GetZPropertyInfo(nameof(Box4_TotalVatPayablesRecoverable)); }
		}

		ZDecimal box4_TotalVatPayablesRecoverable;

		#endregion

		#region Box5

		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box5_TotalVatPayablesNotRecoverable
		{
			get { return box5_TotalVatPayablesNotRecoverable; }
			set { SetNonPersistentPropertyValue(Box5_TotalVatPayablesNotRecoverableInfo, ref box5_TotalVatPayablesNotRecoverable, value); }
		}

		public ZPropertyInfo Box5_TotalVatPayablesNotRecoverableInfo
		{
			get { return GetZPropertyInfo(nameof(Box5_TotalVatPayablesNotRecoverable)); }
		}

		ZDecimal box5_TotalVatPayablesNotRecoverable;

		#endregion

		#region Box6

		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box6_VatBalanceReceivablesAndPayables => (box2_TotalVatReceivables + box4_TotalVatPayablesRecoverable);

		#endregion

		#region Box7

		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box7_BalancePreviousPeriod
		{
			get { return box7_BalancePreviousPeriod; }
			set { SetNonPersistentPropertyValue(Box7_BalancePreviousPeriodInfo, ref box7_BalancePreviousPeriod, value); }
		}

		public ZPropertyInfo Box7_BalancePreviousPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(Box7_BalancePreviousPeriod)); }
		}

		ZDecimal box7_BalancePreviousPeriod;

		#endregion

		#region Box8

		[ReadOnlyMember(nameof(IsReadOnly))]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Box8_TotalBalance => (Box6_VatBalanceReceivablesAndPayables + box7_BalancePreviousPeriod);

		#endregion

		public void Copy(LIQSubmissionData source)
		{
			Box1_TotalVatBaseReceivables  = source.Box1_TotalVatBaseReceivables ;
			Box2_TotalVatReceivables = source.Box2_TotalVatReceivables;
			Box3_TotalVatBasePayables = source.Box3_TotalVatBasePayables;
			Box4_TotalVatPayablesRecoverable = source.Box4_TotalVatPayablesRecoverable;
			Box5_TotalVatPayablesNotRecoverable = source.Box5_TotalVatPayablesNotRecoverable;
			Box7_BalancePreviousPeriod = source.Box7_BalancePreviousPeriod;
		}
	}
}
