using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.OperationalAction;
using Enterprise.Customs.GUI;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.OperationalActions
{
	public class CAB3OperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		#region Schema

		public static class Schema
		{
			public const string DefaultScheduleActionCode = "DefaultScheduleActionCode";
		}

		#endregion

		public CAB3OperationalActionMethodApplicator()
			: base(Res.GetString("3A19AB10-835D-4974-BA96-759DE3AB2E7C", "Send CAD Message operational action"))
		{
			this.DefaultScheduleActionCode = OperationalActionLogAndUserNotificationWrapper.DefaultScheduleActionNone;
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var logNotificationWrapper = new OperationalActionLogAndUserNotificationWrapper(new GUI.MessageInstructionUserNotification(), log, IgnoreAllWarnings, AutoRecalculateDutyAndTax, SuppressNotificationPopout);
			var runner = new CAB3OperationalActionRunner(logNotificationWrapper, new SendsMessagesToCustomsGUI(), MergeEntries);
			runner.PerformFunctionOperationalAction(targets);
		}

		#region User Action Configuration Section

		public ZBool IgnoreAllWarnings
		{
			get { return this.ignoreAllWarnings; }
			set { this.ignoreAllWarnings = value; }
		}
		ZBool ignoreAllWarnings;

		public ZBool AutoRecalculateDutyAndTax
		{
			get { return this.autoRecalculateDutyAndTax; }
			set { this.autoRecalculateDutyAndTax = value; }
		}
		ZBool autoRecalculateDutyAndTax;

		public ZBool SuppressNotificationPopout
		{
			get { return this.suppressNotificationPopOut; }
			set { this.suppressNotificationPopOut = value; }
		}
		ZBool suppressNotificationPopOut;

		public ZBool MergeEntries
		{
			get { return this.mergeEntries; }
			set { this.mergeEntries = value; }
		}
		ZBool mergeEntries;

		[List(nameof(DefaultScheduleActionCodeList))]
		public ZString DefaultScheduleActionCode
		{
			get { return defaultScheduleActionCode; }
			set
			{
				SetNonPersistentPropertyValue(DefaultScheduleActionCodeInfo, ref defaultScheduleActionCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDefaultScheduleActionCode();
				}
			}
		}
		ZString defaultScheduleActionCode;

		public virtual ZPropertyInfo DefaultScheduleActionCodeInfo
		{
			get { return this.GetZPropertyInfo(Schema.DefaultScheduleActionCode); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public CAB3OperationalActionMethodApplicatorValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual CAB3OperationalActionMethodApplicatorValidation GetNewValidation()
		{
			return new CAB3OperationalActionMethodApplicatorValidation(this);
		}

		#endregion

		#region Loopup

		public ICodeDescriptionPairList DefaultScheduleActionCodeList
		{
			get { return delayInstructionList ?? (delayInstructionList = GetDefaultScheduleActionCodeList()); }
		}

		ICodeDescriptionPairList GetDefaultScheduleActionCodeList()
		{
			var result = new DeferredB3SendActionList();
			result.Insert(0, new CodeDescriptionPair(OperationalActionLogAndUserNotificationWrapper.DefaultScheduleActionCancel, Res.GetString("BCCE6FD6-22C2-4298-A342-39DFEF1ED5A8", "Auto cancel sending")));
			result.Insert(0, new CodeDescriptionPair(OperationalActionLogAndUserNotificationWrapper.DefaultScheduleActionNone, Res.GetString("9A9EAAC1-12C0-4F34-A3C6-C81A01AA80F4", "Prompt for message schedule")));
			return result;
		}
		ICodeDescriptionPairList delayInstructionList;

		#endregion

	}
}
