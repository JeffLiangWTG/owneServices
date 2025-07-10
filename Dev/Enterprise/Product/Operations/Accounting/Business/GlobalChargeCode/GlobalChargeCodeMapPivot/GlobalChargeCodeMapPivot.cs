using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public abstract class GlobalChargeCodeMapPivot : AutoAccGlobalChargeCodeMapPivot
	{
		public GlobalChargeCodeMapPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.LedgerTypes")]
		public override ZString YP_TYPE
		{
			get
			{
				return base.YP_TYPE;
			}
			set
			{
				base.YP_TYPE = value;
			}
		}

		#region YP_Description

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		[MaxLength(80)]
		public ZString YP_Description
		{
			get
			{
				if (ChargeCode != null)
				{
					return ChargeCode.AC_Desc;
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo YP_DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(YP_Description)); }
		}

		#endregion

		#endregion

		public static readonly GlobalChargeCodeMapPivotTypeDecider TypeDecider = new GlobalChargeCodeMapPivotTypeDecider();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GlobalChargeCodeMapPivotFetchStrategy(this);
		}

		#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

		#endif
	}
}

