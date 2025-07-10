using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class EventLogHelper
	{
		public StmALog CreateLogForUserLoggedIn(WebUser webUser)
		{
			return CreateLog(Events.Login, webUser, "");
		}

		public StmALog CreateLogForDocumentPrinted(WebUser webUser, string documentName)
		{
			return CreateLog(Events.WebDocumentPrintedDocName, webUser, documentName);
		}

		public StmALog CreateLogForReportPrinted(WebUser webUser, string reportName)
		{
			return CreateLog(Events.WebReportPrintedReportName, webUser, reportName);
		}

		public StmALog CreateLogForModuleChanged(WebUser webUser, string moduleName)
		{
			return CreateLog(Events.WebModuleAccessedModuleName, webUser, moduleName);
		}

		public StmALog CreateLogForAdd(WebUser webUser, BusinessObject addedBizO)
		{
			return CreateLog(Events.AddedARecordToTheSystem, webUser, GetReference(addedBizO));
		}

		public StmALog CreateLogForEdit(WebUser webUser, BusinessObject editedBizO)
		{
			return CreateLog(Events.EditedARecord, webUser, GetReference(editedBizO));
		}

		public StmALog CreateLogForCancel(WebUser webUser, BusinessObject cancelledBizO)
		{
			return CreateLog(Events.SetToInactive, webUser, GetReference(cancelledBizO));
		}

		public StmALog CreateWMREvent(WebUser webUser, string reference)
		{
			return CreateLog(Events.ModifiedOnWeb, webUser, reference);
		}

		public StmALog CreateWMREvent(BusinessObject eventParent, string reference)
		{
			if (eventParent != null)
			{
				var parent = eventParent;
				var stmALog = parent.GetLogs().AddNew(Events.ModifiedOnWeb, reference, ZDateTimeOffset.Now);
				stmALog.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
				parent.Factory.Save();
				return stmALog;
			}
			return null;
		}

		StmALog CreateLog(Event eventType, WebUser webUser, string reference)
		{
			if (EventLoggingIsEnabled && webUser != null && webUser.LoggedInUser != null)
			{
				var parent = (EnterpriseBusinessObject)webUser.LoggedInUser;
				var stmALog = parent.Logs.AddNew(eventType, reference, ZDateTimeOffset.Now);
				stmALog.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
				parent.Factory.Save();
				return stmALog;
			}
			return null;
		}

		bool EventLoggingIsEnabled
		{
			get { return WebDataRegistry.Instance.WebActivityLogging.Value; }
		}

#if DEBUG

		internal
#endif
 ZString GetReference(BusinessObject bizO)
		{
			string reference = string.Empty;

			if (bizO != null)
			{
				reference = bizO.HumanReadableName;
			}

			return reference;
		}
	}
}
