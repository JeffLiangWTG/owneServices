using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Core
{
	public interface ISecurityProxy : IZSecurity
	{
		ISecurityCheckpoint AutoRefreshModuleGrids { get; }
		ISecurityCheckpoint EditAllGlobalColourSchemes { get; }
		ISecurityCheckpoint None { get; }
		ISecurityCheckpoint Notes { get; }
		ISecurityCheckpoint NotesDelete { get; }
		ISecurityCheckpoint NotesEdit { get; }
		ISecurityCheckpoint NotesNew { get; }
		ISecurityCheckpoint NotesNewCustomNote { get; }
		ISecurityCheckpoint NotesNewWithAllMDFForOrg { get; }
		ISecurityCheckpoint NotesNewWithAllCompanyForOrg { get; }
		ISecurityCheckpoint NotesViewAllCompany { get; }
		ISecurityCheckpoint NotesModifyAllCompany { get; }
		ISecurityCheckpoint Organisation { get; }
		ISecurityCheckpoint OrgDetailsNewIsTemporaryOrg { get; }
		ISecurityCheckpoint PublishGlobalFilterLayouts { get; }
		ISecurityCheckpoint PublishUserDefinedFilters { get; }
		ISecurityCheckpoint EditUserDefinedFilters { get; }
		ISecurityCheckpoint PublishGlobalGridColorSchemes { get; }
		ISecurityCheckpoint PublishGlobalUniversalCopyTemplates { get; }
		ISecurityCheckpoint DocumentsReports { get; }
		ISecurityCheckpoint TranslationFeedback { get; }
		ISecurityCheckpoint UseSqlFilterStrip { get; }
		ISecurityCheckpoint WorkflowTaskTemplatesReapply { get; }
		ISecurityCheckpoint WorkflowTaskTemplatesNew { get; }
		ISecurityCheckpoint ContainsInNumbersAndReferences { get; }
	}
}
