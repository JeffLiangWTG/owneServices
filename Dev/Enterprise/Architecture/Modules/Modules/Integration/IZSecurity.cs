using System;
using System.Collections.Generic;
using CargoWise.Async;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Core.Environment
{
	#region Security State Enumeration

	public enum SecurityState
	{
		Granted,
		Denied,
		Implicit
	}

	#endregion

	public interface IZSecurity
	{
		IThreadSentry ThreadSentry { get; }
		void EnsureCurrentThreadIsOwner();

		bool CachingEnabled { get; set; }
		SecurityState IsGroupAllowed(ISecurityCheckpoint checkpoint);
		bool IsGroupExplicitlyAllowed(ISecurityCheckpoint checkpoint);
		SecurityState IsStaffAllowed(ISecurityCheckpoint checkpoint);

		bool IsSecurityAllowedForAllBranches(ISecurityCheckpoint checkpoint);

		IEnumerable<ISecurityCheckpoint> AllLoadedCheckPoints { get; }
		bool AddCheckPoint(CheckpointLookupKey key, ISecurityCheckpoint checkpoint);

#if DEBUG
		Dictionary<CheckpointLookupKey, ISecurityCheckpoint> CheckPointLookUpTable_ForTest { get; }
		void ResetInvoicingPluginProvider();
#endif

		IZGlbSecurityCollection GlbSecurityCollection { get; }

		Guid UserPK { get; set; }
		Guid BranchPK { get; set; }
		Guid DepartmentPK { get; set; }
		Guid CompanyPK { get; set; }
		Guid GroupPK { get; set; }

		IDisposable SuspendLoadProviders();
		void PrepareForSearching();

		ISecurityCheckpoint FindOrCreateAccessModuleCheckPoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateExportCheckPoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateImportCheckPoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateExportNativeXmlCheckPoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateImportNativeXmlCheckPoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateDocumentsCheckpoint(ModuleIdentifier moduleID, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateDocumentCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateDocumentOverrideCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateVisualizerFormsCheckpoint(ModuleIdentifier moduleID, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateVisualizerFormCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateVisualizerFormModifyCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateVisualizerFormDeliveryCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateVisualizerFormSendMessageCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleID, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateChangeGroupSecurityCheckpoint(Guid groupPK);
		ISecurityCheckpoint FindOrCreateChangeStaffSecurityCheckpoint(Guid staffPK);
		ISecurityCheckpoint FindOrCreateGroupOwnerSecurityCheckpoint(Guid groupPK);

		ISecurityCheckpoint FindOrCreateConversationCheckpoint(ISecurityCheckpoint parent, ModuleIdentifier moduleId);
		ISecurityCheckpoint FindOrCreateConversationCheckpoint(ISecurityCheckpoint parent, ModuleIdentifier moduleId, string childName);

		ISecurityCheckpoint FindOrCreateReportCheckpoint(Guid stmMenuItemPK, MultilingualString stmMenuItemName, ModuleIdentifier moduleId, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateReportCustomizeCheckpoint(ModuleIdentifier moduleId, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateReportScheduleCheckpoint(ModuleIdentifier moduleId, ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateReportRunCheckpoint(ModuleIdentifier moduleId, ISecurityCheckpoint parent);

		ISecurityCheckpoint FindOrCreateUniversalCopyCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateUniversalCopyRunCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateUniversalCopyCEDPrivateCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateUniversalCopyEditPublishCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateUniversalCopyDeletePublishCheckpoint(ISecurityCheckpoint parent);

		ISecurityCheckpoint FindOrCreateCopyCheckpoint(ISecurityCheckpoint parent);

		ISecurityCheckpoint FindOrCreateOperationalActionsCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateOperationalActionsCustomiseCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateOperationalActionsAllowRunOnAllMatchingRecordsCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateOperationalActionsRunCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateOperationalActionsRunSpecificCheckpoint(MultilingualString name, ISecurityCheckpoint parent);

		ISecurityCheckpoint FindOrCreateTemplateRecordCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateTemplateRecordAddCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateTemplateRecordEditCheckpoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateTemplateRecordDeleteCheckpoint(ISecurityCheckpoint parent);

		ISecurityCheckpoint FindOrCreateISACHKCheckPoint(ISecurityCheckpoint parent);
		ISecurityCheckpoint FindOrCreateISACHKSendWithMessageErrorsCheckpoint(ISecurityCheckpoint parent);

		ISecurityCheckpoint FindOrCreateWorkflowCheckpoint(ISecurityCheckpoint workflowProviderCheckpoint);
		ISecurityCheckpoint FindOrCreateWorkflowItemCheckpoint(ISecurityCheckpoint workflowProviderCheckpoint, string workflowItemCheckpointCode);
		ISecurityCheckpoint FindOrCreateWorkflowSubItemCheckpoint(ISecurityCheckpoint workflowProviderCheckpoint, string workflowItemCheckpointCode);

		ISecurityCheckpoint FindOrCreateSendEmailCheckpoint(ISecurityCheckpoint parent);

		void ResetData(IZGlbSecurityCollection glbSecurityCollection, object currentUserPK, Guid branchPK, Guid departmentPK, Guid companyPK);
		void ResetData(IZGlbSecurityCollection glbSecurityCollection, object currentUserPK, Guid branchPK, Guid departmentPK, Guid companyPK, bool reloadStaffOrGroupObject);
		void LockDownUserCompanyDepartmentBranch();
		void ShowError(ISecurityCheckpoint checkpoint);
		void ShowError(ISecurityCheckpoint[] checkpoints);

		void LoadRegistrySecurityCheckPoints();
		ISecurityCheckpoint[] GetAllRegistryCheckPoint();
		ISecurityCheckpoint GetRegistryCheckPoint(string identifier, string displayName);

		void LoadPrintQueueSecurityCheckPoints();
		ISecurityCheckpoint[] GetAllPrintQueueCheckPoint();
		ISecurityCheckpoint GetPrintQueueCheckPoint(Guid printQueuePK, string displayName);

		void LoadDocumentTypeSecurityCheckPoints();
		ISecurityCheckpoint[] GetAllDocumentTypeUploadCheckPoint();
		ISecurityCheckpoint[] GetAllDocumentTypeViewCheckPoint();
		ISecurityCheckpoint[] GetAllDocumentTypeCutCheckPoint();
		ISecurityCheckpoint[] GetAllDocumentTypePermanentDeleteCheckPoint();
		ISecurityCheckpoint GetDocumentTypeUploadCheckPoint(string rT_DocType);
		ISecurityCheckpoint GetDocumentTypeViewCheckPoint(string rT_DocType);
		ISecurityCheckpoint GetDocumentTypeCutCheckPoint(string rT_DocType);
		ISecurityCheckpoint GetDocumentTypePermanentDeleteCheckPoint(string rT_DocType);

		void ReloadPrintCheckpoints();
		void ReloadDocumentCheckpoints();

		ISecurityCheckpoint FindCheckPoint(CheckpointLookupKey key);
		ISecurityCheckpoint FindCheckPoint(string key);

		void AddJobInvoicingPlugIn(ISecurityCheckpoint checkpoint, CheckpointBuilderModel model);
		void BuildJobInvoicingPluginCheckpoints(ISecurityCheckpoint checkpoint);
		void SetJobInvoicingPluginBuilder(Func<SecurityCheckpointBuilder> makeBuilder);

		void SetLazyCheckpointPlugin(Func<CheckpointLookupKey, ISecurityCheckpoint> getCheckpoint);
		void SetInitAllLazyCheckpoints(Action initAllLazyCheckpoints);

		object GetGroupRightsWithImplicitRights(ISecurityCheckpoint checkpoint, IZGlbSecurityCollection securityCollection = null);
		MultilingualString GetErrorMessageForNotAllowed(ISecurityCheckpoint checkpoint);
		MultilingualString GetErrorMessageForNotAllowedInGLOW(ISecurityCheckpoint checkpoint);
		MultilingualString GetErrorMessageForNotAllowed(ISecurityCheckpoint[] checkpoints);
	}
}
