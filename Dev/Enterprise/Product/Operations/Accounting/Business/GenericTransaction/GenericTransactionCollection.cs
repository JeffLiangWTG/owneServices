using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business.Internal;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.GenericTransaction
{
	public class GenericTransactionCollection : BusinessObjectCollection<GenericTransaction>
	{
		public GenericTransactionCollection(BusinessObjectFactory factory)
			: this(factory, true)
		{
		}

		public GenericTransactionCollection(BusinessObjectFactory factory, bool observeMaximumRows)
			: base(factory)
		{
			this.ObserveMaximumRows = observeMaximumRows;
		}

		#region Overrides

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new NonPersistentBusinessObjectFindBoxListProvider(this); }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This collection does not support AddNew operation.");
		}

		public override int GetEstimatedLoadCount(ZQuery completeFilter)
		{
			ZInt countToReturn = 0;
			if (FilterHelper != null)
			{
				string sQL = string.Format("SELECT COUNT(*) AS Count FROM ({0}) m", FilterHelper.ParameterisedText);
				DynamicBusinessObjectCollection countCollection = new DynamicBusinessObjectCollection(Factory);
				countCollection.Load(sQL, FilterHelper.Filter.Params);
				countToReturn = (ZInt)countCollection[0]["Count"];
			}

			return countToReturn;
		}

		public override void Load(ZQuery filter)
		{
			FilterHelper = new GenericTransactionFilterHelper(filter, ObserveMaximumRows);
			LoadWrapper();
		}

		public override void Load()
		{
			if (FilterHelper != null)
			{
				LoadUsingFilterHelper();
			}
		}

		#endregion

		public void LoadTop1(ZQuery filter)
		{
			FilterHelper = new GenericTransactionFilterHelper(filter, true);
			FilterHelper.MaximumRows = 1;
			LoadWrapper();
		}

		#region Implementation

		GenericTransactionFilterHelper FilterHelper;
		readonly bool ObserveMaximumRows;

		void LoadWrapper()
		{
			try
			{
				Load();
			}
			finally
			{
				FilterHelper.MaximumRows = null;
			}
		}

		#region LoadUsingFilterHelper

		public static GenericTransaction[] LoadUsingFilterHelper(BusinessObjectFactory factory, GenericTransactionFilterHelper filterHelper)
		{
			var result = new List<GenericTransaction>();
			var sQL = string.Format(CultureInfo.InvariantCulture, "{0} OPTION (RECOMPILE)", filterHelper.ParameterisedText);
			var dynBizOs = new DynamicBusinessObjectCollection(factory);
			dynBizOs.Load(sQL, filterHelper.Filter.Params);
			foreach (DynamicBusinessObject dynBizO in dynBizOs)
			{
				var newBizO = new GenericTransaction(factory);
				PopulateNewBizO(newBizO, dynBizO);
				result.Add(newBizO);
			}
			return result.ToArray();
		}

		void LoadUsingFilterHelper()
		{
			RemoveAll();
			AddRange(LoadUsingFilterHelper(Factory, FilterHelper));
		}

		#endregion

		#region PopulateNewBizO

		static void PopulateNewBizO(GenericTransaction newBizO, DynamicBusinessObject dynBizO)
		{
			using (newBizO.GetValidationSuspender())
			{
				if (dynBizO[GenericTransaction.Schema.PK] is ZGuid)
				{
					newBizO.VT_FK = (ZGuid)dynBizO[GenericTransaction.Schema.PK];
				}
				if (dynBizO[GenericTransaction.Schema.VT_IsHeader] is ZString)
				{
					newBizO.VT_IsHeader = (ZString)dynBizO[GenericTransaction.Schema.VT_IsHeader] == ZBool.True.ToString();
				}
				if (dynBizO[GenericTransaction.Schema.VT_Ledger] is ZString)
				{
					newBizO.VT_Ledger = (ZString)dynBizO[GenericTransaction.Schema.VT_Ledger];
				}
				if (dynBizO[GenericTransaction.Schema.VT_Type] is ZString)
				{
					newBizO.VT_Type = (ZString)dynBizO[GenericTransaction.Schema.VT_Type];
				}
				if (dynBizO[GenericTransaction.Schema.VT_OH] is ZGuid)
				{
					newBizO.VT_OH = (ZGuid)dynBizO[GenericTransaction.Schema.VT_OH];
				}
				if (dynBizO[GenericTransaction.Schema.VT_PostDate] is ZDateTime)
				{
					newBizO.VT_PostDate = (ZDateTime)dynBizO[GenericTransaction.Schema.VT_PostDate];
				}
				if (dynBizO[GenericTransaction.Schema.VT_InvoiceDate] is ZDateTime)
				{
					newBizO.VT_InvoiceDate = (ZDateTime)dynBizO[GenericTransaction.Schema.VT_InvoiceDate];
				}
				if (dynBizO[GenericTransaction.Schema.VT_Period] is ZInt)
				{
					newBizO.VT_Period = (ZInt)dynBizO[GenericTransaction.Schema.VT_Period];
				}
				if (dynBizO[GenericTransaction.Schema.VT_GLAccount] is ZString)
				{
					newBizO.VT_GLAccount = (ZString)dynBizO[GenericTransaction.Schema.VT_GLAccount];
				}
				if (dynBizO[GenericTransaction.Schema.VT_GLAccountDesc] is ZString)
				{
					newBizO.VT_GLAccountDesc = (ZString)dynBizO[GenericTransaction.Schema.VT_GLAccountDesc];
				}
				if (dynBizO[GenericTransaction.Schema.VT_2ndGLAccount] is ZString)
				{
					newBizO.VT_2ndGLAccount = (ZString)dynBizO[GenericTransaction.Schema.VT_2ndGLAccount];
				}
				if (dynBizO[GenericTransaction.Schema.VT_2ndGLAccountDesc] is ZString)
				{
					newBizO.VT_2ndGLAccountDesc = (ZString)dynBizO[GenericTransaction.Schema.VT_2ndGLAccountDesc];
				}
				if (dynBizO[GenericTransaction.Schema.VT_GSTGLAccount] is ZString)
				{
					newBizO.VT_GSTGLAccount = (ZString)dynBizO[GenericTransaction.Schema.VT_GSTGLAccount];
				}
				if (dynBizO[GenericTransaction.Schema.VT_GSTGLAccountDesc] is ZString)
				{
					newBizO.VT_GSTGLAccountDesc = (ZString)dynBizO[GenericTransaction.Schema.VT_GSTGLAccountDesc];
				}
				if (dynBizO[GenericTransaction.Schema.VT_DueDate] is ZDateTime)
				{
					newBizO.VT_DueDate = (ZDateTime)dynBizO[GenericTransaction.Schema.VT_DueDate];
				}
				if (dynBizO[GenericTransaction.Schema.VT_TransactionNo] is ZString)
				{
					newBizO.VT_TransactionNo = (ZString)dynBizO[GenericTransaction.Schema.VT_TransactionNo];
				}
				if (dynBizO[GenericTransaction.Schema.VT_TransactionDesc] is ZString)
				{
					newBizO.VT_TransactionDesc = (ZString)dynBizO[GenericTransaction.Schema.VT_TransactionDesc];
				}
				if (dynBizO[GenericTransaction.Schema.VT_JH] is ZGuid)
				{
					newBizO.VT_JH = (ZGuid)dynBizO[GenericTransaction.Schema.VT_JH];
				}
				if (dynBizO[GenericTransaction.Schema.VT_ChargeCode] is ZString)
				{
					newBizO.VT_ChargeCode = (ZString)dynBizO[GenericTransaction.Schema.VT_ChargeCode];
				}
				if (dynBizO[GenericTransaction.Schema.VT_Amount] is ZDecimal)
				{
					newBizO.VT_Amount = (ZDecimal)dynBizO[GenericTransaction.Schema.VT_Amount];
				}
				if (dynBizO[GenericTransaction.Schema.VT_GST] is ZDecimal)
				{
					newBizO.VT_GST = (ZDecimal)dynBizO[GenericTransaction.Schema.VT_GST];
				}
				if (dynBizO[GenericTransaction.Schema.VT_Total] is ZDecimal)
				{
					newBizO.VT_Total = (ZDecimal)dynBizO[GenericTransaction.Schema.VT_Total];
				}
				if (dynBizO[GenericTransaction.Schema.VT_GC] is ZGuid)
				{
					newBizO.VT_GC = (ZGuid)dynBizO[GenericTransaction.Schema.VT_GC];
				}
				if (dynBizO[GenericTransaction.Schema.VT_GB] is ZGuid)
				{
					newBizO.VT_GB = (ZGuid)dynBizO[GenericTransaction.Schema.VT_GB];
				}
				if (dynBizO[GenericTransaction.Schema.VT_GE] is ZGuid)
				{
					newBizO.VT_GE = (ZGuid)dynBizO[GenericTransaction.Schema.VT_GE];
				}
				if (dynBizO[GenericTransaction.Schema.VT_ReversePeriod] is ZInt)
				{
					newBizO.VT_ReversePeriod = (ZInt)dynBizO[GenericTransaction.Schema.VT_ReversePeriod];
				}
				if (dynBizO[GenericTransaction.Schema.VT_VoucherNo] is ZString)
				{
					newBizO.VT_VoucherNo = (ZString)dynBizO[GenericTransaction.Schema.VT_VoucherNo];
				}

				bool canCalculateOSAmounts = true;
				if (dynBizO[GenericTransaction.Schema.VT_OSTotal] is ZDecimal)
				{
					newBizO.VT_OSTotal = (ZDecimal)dynBizO[GenericTransaction.Schema.VT_OSTotal];
				}
				else
				{
					canCalculateOSAmounts = false;
				}
				if (dynBizO[GenericTransaction.Schema.VT_RX_NKCurrency] is ZString)
				{
					newBizO.VT_RX_NKCurrency = (ZString)dynBizO[GenericTransaction.Schema.VT_RX_NKCurrency];
				}
				else
				{
					canCalculateOSAmounts = false;
				}
				if (dynBizO[GenericTransaction.Schema.VT_ExchangeRate] is ZDecimal)
				{
					newBizO.VT_ExchangeRate = (ZDecimal)dynBizO[GenericTransaction.Schema.VT_ExchangeRate];
				}
				else
				{
					canCalculateOSAmounts = false;
				}

				if (canCalculateOSAmounts)
				{
					newBizO.VT_OSAmount = (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(newBizO.VT_Amount, newBizO.VT_ExchangeRate, newBizO.VT_RX_NKCurrency);
					newBizO.VT_OSGST = (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(newBizO.VT_GST, newBizO.VT_ExchangeRate, newBizO.VT_RX_NKCurrency);
				}

				if (dynBizO[GenericTransaction.Schema.VT_SystemCreateUser] is ZString)
				{
					newBizO.VT_SystemCreateUser = (ZString)dynBizO[GenericTransaction.Schema.VT_SystemCreateUser];
				}
			}

			if (dynBizO[GenericTransaction.Schema.VT_SubAccounts] is ZString)
			{
				newBizO.VT_SubAccounts = (ZString)dynBizO[GenericTransaction.Schema.VT_SubAccounts];
			}

			if (dynBizO[GenericTransaction.Schema.VT_AGForSubAccount] is ZGuid)
			{
				newBizO.VT_AGForSubAccount = (ZGuid)dynBizO[GenericTransaction.Schema.VT_AGForSubAccount];
			}

			if (dynBizO[GenericTransaction.Schema.VT_SubAccountParentPK] is ZGuid)
			{
				newBizO.VT_SubAccountParentPK = (ZGuid)dynBizO[GenericTransaction.Schema.VT_SubAccountParentPK];
			}
		}

		#endregion
	}
}
#endregion
