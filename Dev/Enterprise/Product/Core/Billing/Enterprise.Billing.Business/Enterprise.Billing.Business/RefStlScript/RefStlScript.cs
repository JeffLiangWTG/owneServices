using System;
using System.Data;
using CargoWise.Billing.Collectors;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Billing.Business
{
	public sealed class RefStlScript : AutoRefStlScript, IRefStlScript
	{
		public RefStlScript(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public string ActiveOn => STL_ActiveOn;
		public string FeatureCode => STL_FeatureCode;
		public string FeatureName => STL_FeatureName;
		public string DataGranularity => STL_DataGranularity;
		public string TransactionDateUtc => STL_TransactionDateUtc;
		public string GuidReference => STL_GuidReference;
		public string TransactionCount => STL_TransactionCount;
		public string FromClause
		{
			get
			{
				try
				{
					return STL_FromClause;
				}
				catch (ZBlobReadException exception)
				{
					throw new RefDataException("Temporary exception caused by failover operation, will self-recover in next try", exception, false);
				}
			}
		}
		public string RoleName => STL_RoleName;
		public string ModuleName => STL_ModuleName;
		public string FunctionName => STL_FunctionName;
		public string CompanyCode => STL_CompanyCode;
		public string BranchCode => STL_BranchCode;
		public string BillingReference1 => STL_BillingReference1;
		public string BillingReference2 => STL_BillingReference2;
		public string BillingReference3 => STL_BillingReference3;
		public string BillingReference4 => STL_BillingReference4;
		public string WhereClause
		{
			get
			{
				try
				{
					return STL_WhereClause;
				}
				catch (ZBlobReadException exception)
				{
					throw new RefDataException("Temporary exception caused by failover operation, will self-recover in next try", exception, false);
				}
			}
		}
		public string CreatingUserCode => STL_CreatingUserCode;
		public string AdditionalRefs => STL_AdditionalRefs;
		public string PreparationScript
		{
			get
			{
				try
				{
					return STL_PreparationScript;
				}
				catch (ZBlobReadException exception)
				{
					throw new RefDataException("Temporary exception caused by failover operation, will self-recover in next try", exception, false);
				}
			}
		}
		public bool WithOptionRecompile => STL_WithOptionRecompile;
		public bool UsedInBilling => STL_UsedInBilling;
		public string MinCW1Version => STL_MinCW1Version;
		public string MaxCW1Version => STL_MaxCW1Version;
		public string DateType => STL_DateType;
		public DateTime CollectionStartDateUtc => STL_CollectionStartDateUtc.IsValid ? STL_CollectionStartDateUtc.ToDateTime() : DateTime.MinValue;
	}
}
