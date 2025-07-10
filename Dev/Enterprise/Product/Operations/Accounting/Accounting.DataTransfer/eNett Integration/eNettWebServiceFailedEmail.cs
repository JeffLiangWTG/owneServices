using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	public partial class eNettWebServiceFailedEmail : AccountingEmailDef
	{
		public eNettWebServiceFailedEmail(string methodName,
			DateTime date,
			string errorCode,
			string errorMessage,
			bool isAutoFactorySave,
			BusinessObjectFactory factory = null)
		{
			if (factory == null && !isAutoFactorySave)
			{
				throw new ArgumentException($"It is invalid to set '{nameof(isAutoFactorySave)}' false when no external Factory has been passed to '{nameof(factory)}' as it will lead to losing unsaved data");
			}
			if (isAutoFactorySave && factory != null)
			{
				throw new ArgumentException($"'{nameof(factory)}' must be null when '{nameof(isAutoFactorySave)}' is true as it will be ignored.");
			}
			this.MethodName = methodName;
			this.Date = date;
			this.ErrorCode = errorCode;
			this.ErrorMessage = errorMessage;
			IsAutoFactorySave = isAutoFactorySave;
			Factory = factory;
		}

#if DEBUG
		public bool IsAutoFactorySave_ForTest => IsAutoFactorySave;
		public BusinessObjectFactory Factory_ForTest => Factory;
#endif

		#region Implementation

		bool IsAutoFactorySave { get; }
		BusinessObjectFactory Factory { get; }
		protected string MethodName { get; private set; }
		protected DateTime Date { get; private set; }
		protected string ErrorCode { get; private set; }
		protected string ErrorMessage { get; private set; }

		#region Overrides 

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.ENettNotificationsGroup; }
		}

		protected sealed override void SendCore()
		{
			if (IsAutoFactorySave)
			{
				base.SendCore();
			}
			else
			{
				Env.OutgoingMailManager.Create(Factory, this, Recipient.Value, GroupSourceLocator.GetFromRegistryItem(Recipient));
			}
		}

		protected override string GetBody()
		{
			return GetContent();
		}

		protected override string GetSubject()
		{
			return Res.GetString("56d35cd9-cf65-49a3-903c-7015c7d9cf5b", "Call to web method {0} failed", MethodName);
		}

		#endregion

		#region Helper functions

		string GetContent()
		{
			return Res.GetString("6b37561e-8501-49ee-9edc-a97683d9a529", @"Call to web method {0} failed.
Date: {1}
Code: {2}
Message: {3}", MethodName, Date, ErrorCode, ErrorMessage);
		}

		#endregion
		#endregion
	}
}
