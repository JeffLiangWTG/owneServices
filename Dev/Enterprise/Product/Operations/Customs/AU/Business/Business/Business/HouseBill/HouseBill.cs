using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[SystemDefinedValues]
	public class Bill : TypeSafeBill, Integration.Customs.AU.IBill
	{
		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new class Schema : TypeSafeBill.Schema
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public const string CU_fPartShipConsignmentReference = "CU_fPartShipConsignmentReference";
			public const int CU_fPartShipConsignmentReferenceMaxLength = 35;
		}

		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static Bill New(BusinessObjectFactory factory)
		{
			return (Bill)factory.
				// Split for find/replace
				New(typeof(Bill));
		}

		protected override Customs.Business.CusDecHouseBillLookups GetNewLookups()
		{
			return new BillLookups(this);
		}

		protected override Customs.Business.CusDecHouseBillValidation GetNewValidation()
		{
			return Declaration != null && Declaration.IsExWarehouse ? new CusDecHouseBillValidationForExWarehouse(this) : new BillValidation(this);
		}

		public override ZGuid CU_JE
		{
			get { return base.CU_JE; }
			set
			{
				bool hasChanged = base.CU_JE != value;
				base.CU_JE = value;
				if (hasChanged && !IsCopying && Declaration != null)
				{
					Declaration.Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CU_BillType
		{
			get { return base.CU_BillType; }
			set
			{
				base.CU_BillType = value;

				if (!IsHouseBill)
				{
					CU_fPartShipConsignmentReference = ZString.Empty;
				}
			}
		}

		public override ZString CU_BillNum
		{
			get { return base.CU_BillNum; }
			set
			{
				var isChanged = base.CU_BillNum != value;
				base.CU_BillNum = value;
				if (isChanged && !IsCopying && !IsRefreshingByDataRefreshBus)
				{
					SetConsignmentReferenceIfRequired();
				}
			}
		}

		void SetConsignmentReferenceIfRequired()
		{
			if (AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.Value && IsHouseBill && !CU_BillNum.IsEmpty)
			{
				CU_fPartShipConsignmentReference = CusHAWBBase.ExistingConRefFor(CU_BillNum, Factory).Left(CU_fPartShipConsignmentReferenceInfo.MaxLength);
			}
		}

		[ReadOnlyMember(nameof(CU_fPartShipConsignmentReference_ReadOnly))]
		[MaxLength(Schema.CU_fPartShipConsignmentReferenceMaxLength)]
		public ZString CU_fPartShipConsignmentReference
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.CU_fPartShipConsignmentReference); }
			set
			{
				CheckMaximumLength(CU_fPartShipConsignmentReferenceInfo, value);
				this.SetSystemDefinedValue(Schema.CU_fPartShipConsignmentReference, value);
				CU_fPartShipConsignmentReferenceInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCU_fPartShipConsignmentReference();
				}
			}
		}

		public ZPropertyInfo CU_fPartShipConsignmentReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.CU_fPartShipConsignmentReference); }
		}

		bool CU_fPartShipConsignmentReference_ReadOnly
		{
			get { return !IsHouseBill; }
		}
	}
}
