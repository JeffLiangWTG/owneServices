using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.OperationalAction;
using Enterprise.Customs.GUI;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.OperationalActions
{
	public class CARNSQueryOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public CARNSQueryOperationalActionMethodApplicator()
			: base(Res.GetString("F3362AD1-E8AF-43B6-A725-D48C15D10874", "Send Release Status Query operational action"))
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var logNotificationWrapper = new OperationalActionLogAndUserNotificationWrapper(new GUI.MessageInstructionUserNotification(), log, IgnoreAllWarnings, SuppressNotificationPopout);
			var runner = new CARNSQueryOperationalActionRunner(logNotificationWrapper, new SendsMessagesToCustomsGUI());
			runner.PerformFunctionOperationalAction(targets);
		}

		#region User Action Configuration Section

		public ZBool IgnoreAllWarnings
		{
			get { return this.ignoreAllWarnings; }
			set { this.ignoreAllWarnings = value; }
		}
		ZBool ignoreAllWarnings;

		public ZBool SuppressNotificationPopout
		{
			get { return this.suppressNotificationPopOut; }
			set { this.suppressNotificationPopOut = value; }
		}
		ZBool suppressNotificationPopOut;

		#endregion
	}
}
