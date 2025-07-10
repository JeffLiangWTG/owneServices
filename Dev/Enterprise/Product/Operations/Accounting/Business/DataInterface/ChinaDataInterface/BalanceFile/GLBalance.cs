using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class GLBalance : DynamicBusinessObject, IFileOutput, IObsoleteValidation
	{
		public GLBalance(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void Delete()
		{
			ErrorReporter.ReportOnce(GetType().ToString() + "Delete", "Delete is not supported");
		}

		#region Properties

		#region BalanceAmount

		public virtual ZDecimal BalanceAmount
		{
			get
			{
				if (fBalanceAmount.IsEmpty)
				{
					fBalanceAmount = BalanceAmountInternal;
				}
				return fBalanceAmount;
			}
			set
			{
				SetNonPersistentPropertyValue(BalanceAmountInfo, ref fBalanceAmount, value);
			}
		}

		ZDecimal fBalanceAmount;

		public ZPropertyInfo BalanceAmountInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(BalanceAmount)); }
		}

		#endregion

		#region BalanceAmountInternal

		public virtual ZDecimal BalanceAmountInternal
		{
			get
			{
				object result = ((IBusinessObjectInternals)this).Row[BalanceAmountInternalInfo.Name];
				return (result == DBNull.Value) ? ZDecimal.Zero : new ZDecimal(result);
			}
		}

		public ZPropertyInfo BalanceAmountInternalInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(BalanceAmountInternal)); }
		}

		#endregion

		#region GLAccountNumber

		public virtual ZString GLAccountNumber
		{
			get { return new ZString(((IBusinessObjectInternals)this).Row[GLAccountNumberInfo.Name]); }
		}

		public ZPropertyInfo GLAccountNumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(GLAccountNumber)); }
		}

		#endregion

		#region AccountType

		public virtual ZString AccountType
		{
			get { return new ZString(((IBusinessObjectInternals)this).Row[AccountTypeInfo.Name]); }
		}

		public ZPropertyInfo AccountTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(AccountType)); }
		}

		#endregion

		#region LocalDebitCredit

		public virtual ZString LocalDebitCredit
		{
			get { return new ZString(((IBusinessObjectInternals)this).Row[LocalDebitCreditInfo.Name]); }
		}

		public ZPropertyInfo LocalDebitCreditInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(LocalDebitCredit)); }
		}

		#endregion

		#endregion

		#region IFileOutput Members

		public void Write(System.IO.StreamWriter writer)
		{
			string balanceAmountOutput = BalanceAmount.IsEmpty ? "0.00" : BalanceAmount.ToString(2);
			string output = DataInterfaceConstant.Quote + DataInterfaceUtils.RemoveTrailingZero(GLAccountNumber) + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab + balanceAmountOutput;
			writer.WriteLine(output);
		}

		#endregion
	}
}
