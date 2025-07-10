using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChangeTransactionDatesBusinessObject : ChangeTransactionDatesBusinessObjectBase
	{
		public ChangeTransactionDatesBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory)
			: base(factory)
		{
			this.PluginSecurity = pluginSecurity;
		}

		public ChangeTransactionDatesBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory, OperationsJobConfigurationCodes operationsJobConfigurationCodes)
			: this(pluginSecurity, factory)
		{
			OperationsJobConfigurationCodes = operationsJobConfigurationCodes;
			JobType = operationsJobConfigurationCodes.ConsumerTypeCode;
			Direction = operationsJobConfigurationCodes.DirectionCode;
			Mode = operationsJobConfigurationCodes.TransportMode;
			Broker = operationsJobConfigurationCodes.Broker;
		}

		readonly OperationsJobConfigurationCodes OperationsJobConfigurationCodes;
		readonly string JobType;
		readonly string Direction;
		readonly string Mode;
		readonly string Broker;

		public void PopulateValuesFrom(ChangeTransactionDatesBusinessObject source)
		{
			if (!this.InvoiceDateInfo.ReadOnly && !source.InvoiceDateInfo.ReadOnly)
			{
				this.InvoiceDate = source.InvoiceDate;
			}

			if (!this.PostDateInfo.ReadOnly && !source.PostDateInfo.ReadOnly)
			{
				this.PostDate = source.PostDate;
			}
		}

		#region RevenueRecognitionDates

		public const int RevenueRecognitionDatesMaxLength = 255;
		[MaxLength(RevenueRecognitionDatesMaxLength)]
		public ZString RevenueRecognitionDates
		{
			get { return RevenueRecognitionDates_innerValue; }
			set { SetNonPersistentPropertyValue(RevenueRecognitionDatesInfo, ref RevenueRecognitionDates_innerValue, value); }
		}
		ZString RevenueRecognitionDates_innerValue;

		public ZPropertyInfo RevenueRecognitionDatesInfo
		{
			get { return GetZPropertyInfo(nameof(RevenueRecognitionDates)); }
		}

		protected bool RevenueRecognitionDates_ReadOnly
		{
			get { return true; }
		}

		#endregion

		protected
#if DEBUG
 virtual
#endif
 bool InvoiceDate_ReadOnly
		{
			get
			{
				bool overrideTransactionDate = false;

				if (JobType != null && Direction != null && Mode != null && Broker != null)
				{
					InvoiceDateConfigurationHelper helper = new InvoiceDateConfigurationHelper(OperationsJobConfigurationCodes, null);
					InvoiceDateConfiguration config = helper.FindInvoiceDateConfiguration();
					if (config != null)
					{
						overrideTransactionDate = config.Override;
					}
				}
				return !ModifyTransactionDateSecurityIsAllowed || !overrideTransactionDate;
			}
		}

		protected
#if DEBUG
 virtual
#endif
 bool PostDate_ReadOnly
		{
			get
			{
				return !ModifyPostDateSecurityIsAllowed ||
						!AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.OverridePostDate;
			}
		}

		#region Implementation

		readonly SecurityCheckpoint PluginSecurity;

		protected JobInvoicingSecurityHelper SecurityHelper
		{
			get { return SecurityHelper_internalValue ?? (SecurityHelper_internalValue = new JobInvoicingSecurityHelper(PluginSecurity)); }
		}
		JobInvoicingSecurityHelper SecurityHelper_internalValue;

		protected virtual bool ModifyTransactionDateSecurityIsAllowed
		{
			get { return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ModifyTransactionDate); }
		}

		protected virtual bool ModifyPostDateSecurityIsAllowed
		{
			get { return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ModifyPostDate); }
		}

		#endregion
	}
}
