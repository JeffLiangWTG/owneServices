#if DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMTestHelper
	{
		IBMSystem CreateSystem(BusinessObjectFactory factory, params string[] workflowTypes);
		IBMSystem CreateSystemAndRelatedWorkflowType(BusinessObjectFactory factory, string workflowType, bool isActive);
		IBMComponent CreateBucket(IBMSystem system, string name = "bucket", int sequence = 0, int offsetMinutes = 0);
		IBMComponent CreateBuffer(IBMSystem system, string name = "buffer", int timespanMinutes = 96 * 60, byte loadLimitPercent = 50, int sequence = 0);
		IBMComponentLink LinkComponents(IBMComponent from, IBMComponent to, byte sequence = 0, bool? isReleaseGate = null);
		void MarkAsReleaseGroupWithinSystem(IBMSystem system, IGlbGroup group);

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		IProcessJobHeader CreateJobHeader<T>(BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone = true);
		IProcessJobHeader GetJobHeaderForParent(IWorkflowProviderCore parent, BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone = true);
		IProcessHeader CreateWorkflow(IProcessJobHeader jobHeader, string name, Guid? releaseGroupPK = null, DateTime? penetrationResetDateTime = null);
		IProcessHeader CreateWorkflow(IProcessTaskTemplate template, string description = "Zoot! Review.");
		IProcessHeader CreateWorkflow(BusinessObjectFactory factory, string completionStatement, IBMComponent currentComponent = null, DateTime? releaseDateTime = null, Guid? releaseGroupPK = null, bool autoAssignTasks = false);
		IProcessHeaderLink CreateDependencyLink(IProcessTaskTemplate template, IProcessHeader headerFrom = null, IProcessHeader headerTo = null);
		IProcessHeaderLink CreateParentChildLink(IProcessTaskTemplate template, IProcessHeader headerFrom = null, IProcessHeader headerTo = null);
		IProcessHeaderLink CreateLink(IProcessHeader headerFrom, IProcessHeader headerTo, string linkType = "DEP");
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		IProcessTask CreateTask(IProcessHeader workflow, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", string taskStatus = "ASN", int? sequence = null, string description = "", bool createStaffIfNotExist = false, IGlbCapability capability = null, IGlbGroup group = null);
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		IProcessTask CreateTask(BusinessObject job, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", string taskStatus = "ASN", int? sequence = null, string description = "");
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		IProcessTask CreateTask(IProcessTaskTemplate template, IProcessHeader workflow, string resourceCode = "", int lowEstMinutes = 60, string description = "Isopropanol", decimal estVariationFactor = 2m);
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		IProcessTask CreateTaskWithCompany(IProcessHeader workflow, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", string taskStatus = "ASN", int? sequence = null, string description = "", IGlbCompany company = null);
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		IProcessHeader CreateWorkflowAndTask(BusinessObjectFactory factory, string completionStatement, IBMComponent currentComponent = null, DateTime? releaseDateTime = null, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", int estVariationFactor = 1, int? sequence = null, string description = "", string taskStatus = "ASN", Guid? capabilityPK = null, bool createStaffIfNotExist = false);

		IProcessHeader CreateQualityIteration(IProcessTask iterateFromTask, IProcessTask containmentBarrierTask, string iterationWorkflowDescription = "", string resourceUnderReviewStaffCode = null, string iterationReasonCode = null, bool shouldCreateWorkflowForIteration = true);

		ITagDefinition CreateTagDefinition(BusinessObjectFactory factory, string code, string description = "", bool isExclusive = false, string usageScope = null, string scope = null, bool isActive = true);
		ITagMagnitude CreateTagMagnitude(ITagDefinition definition, string code, string description = "", int nudge = 0, bool isActive = true);

		IBMControlCustomisation CreateControlCustomisation(BusinessObjectFactory factory, string type = "DET", int width = 100, int height = 100, string backgroundColor = "Hot Pink");
		IBMControlCustomisationLink CreateControlCustomisationLink(BusinessObjectFactory factory, ICustomisedLayoutSupportable parent, IBMControlCustomisation controlLayout);
		IBMControlCustomisationLink CreateControlCustomisationLink(BusinessObjectFactory factory, IGlbGroup group, IBMControlCustomisation controlLayout);

		IProcessTaskTemplate CreateWorkflowTemplate(BusinessObjectFactory factory, string processType, string subType1 = null, string subType2 = null, string subType3 = null, string subType4 = null, string subType5 = null, string name = null, string description = null, bool isPartial = false, bool isUniversal = false);

		IBMReleaseSequence CreateReleaseSequence(BusinessObjectFactory factory, Guid releaseGroup, string name = "Skywalker Saga", bool isActive = true, Guid? capability = null, int? nudge = null);
		IBMReleaseSequenceItem CreateReleaseSequenceItem(IBMReleaseSequence sequence, IProcessHeader workflow, int position = 1, int value = 0, int investment = 0, string note = "If this is a consular ship, where is the ambassador?");

		void EnableBMSInRegistry();
		void DisableBMSInRegistry();
		void SetWorkflowManagementModeInRegistry(string mode);

		void SetAlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates(bool value);

		void AssertIsPrerequisite(IProcessHeader fromHeader, IProcessHeader toHeader);
		void AssertIsNotPrerequisite(IProcessHeader fromHeader, IProcessHeader toHeader);
		void AssertIsParent(IProcessHeader childHeader, IProcessHeader parentHeader);

		IDisposable TemporarilyDisableTableCachingInUberFactory(IReadOnlyCollection<string> tablesToDisableCaching);
	}
}

#endif
