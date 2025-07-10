using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherDataSource : DynamicBusinessObject, IObsoleteValidation
	{
		public VoucherDataSource(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Charge Code

		public ZString ChargeCode
		{
			get
			{
				return new ZString(((IBusinessObjectInternals)this).Row[ChargeCodeInfo.Name]);
			}
		}

		public ZPropertyInfo ChargeCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				return GetZPropertyInfo(nameof(ChargeCode));
			}
		}

		public ZString ChargeCodeDesc
		{
			get
			{
				return new ZString(((IBusinessObjectInternals)this).Row[ChargeCodeDescInfo.Name]);
			}
		}

		public ZPropertyInfo ChargeCodeDescInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				return GetZPropertyInfo(nameof(ChargeCodeDesc));
			}
		}

		public ZString ChargeCodeLocalLangDesc
		{
			get
			{
				return new ZString(((IBusinessObjectInternals)this).Row[ChargeCodeLocalLangDescInfo.Name]);
			}
		}

		public ZPropertyInfo ChargeCodeLocalLangDescInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				return GetZPropertyInfo(nameof(ChargeCodeLocalLangDesc));
			}
		}

		#endregion

		public ZString BranchCode
		{
			get
			{
				return new ZString(((IBusinessObjectInternals)this).Row[BranchCodeInfo.Name]);
			}
		}

		public ZPropertyInfo BranchCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				return GetZPropertyInfo(nameof(BranchCode));
			}
		}

		public ZString DepartmentCode
		{
			get
			{
				return new ZString(((IBusinessObjectInternals)this).Row[DepartmentCodeInfo.Name]);
			}
		}

		public ZPropertyInfo DepartmentCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				return GetZPropertyInfo(nameof(DepartmentCode));
			}
		}

		public ZGuid GLHeader
		{
			get { return new ZGuid(((IBusinessObjectInternals)this).Row[GLHeaderInfo.Name]); }
		}

		public ZPropertyInfo GLHeaderInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(GLHeader)); }
		}

		public ZString TransactionType
		{
			get { return new ZString(((IBusinessObjectInternals)this).Row[TransactionTypeInfo.Name]); }
		}

		public ZPropertyInfo TransactionTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TransactionType)); }
		}

		public ZDecimal Amount
		{
			get { return new ZDecimal(((IBusinessObjectInternals)this).Row[AmountInfo.Name]); }
		}

		public ZPropertyInfo AmountInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Amount)); }
		}

		public ZDecimal OSAmount
		{
			get
			{
				return new ZDecimal(((IBusinessObjectInternals)this).Row[OSAmountInfo.Name]);
			}
		}

		public ZPropertyInfo OSAmountInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				return GetZPropertyInfo(nameof(OSAmount));
			}
		}

		public ZString VoucherNumber
		{
			get { return new ZString(((IBusinessObjectInternals)this).Row[VoucherNumberInfo.Name]); }
		}

		public ZPropertyInfo VoucherNumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(VoucherNumber)); }
		}

		public override void Delete()
		{
			ErrorReporter.ReportOnce(GetType().ToString(), "Delete is not supported");
		}
	}
}
