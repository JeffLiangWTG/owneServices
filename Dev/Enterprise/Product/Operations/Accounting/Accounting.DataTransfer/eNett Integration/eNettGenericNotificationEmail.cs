using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	public partial class eNettGenericNotificationEmail : AccountingEmailDef
	{
		public eNettGenericNotificationEmail(string message)
		{
			this.Message = message;
			ContentType = EmailContentTypes.HTML;
		}
		protected string Message { get; private set; }

		#region Overrides

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.ENettNotificationsGroup; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded HTML")]
		protected override string GetBody()
		{
			return string.Format(@"<PRE>
{0}
Message: {1}
</PRE>", GetSubject(), Message);
		}

		protected override string GetSubject()
		{
			return Res.GetString("6854bdfe-349f-4276-8970-845a649eb5af", "An error occurred during processing transaction(s) received from eNett.");
		}

		#endregion
	}
}
