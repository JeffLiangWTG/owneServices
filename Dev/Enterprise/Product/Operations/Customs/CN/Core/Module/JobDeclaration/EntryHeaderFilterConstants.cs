namespace Enterprise.Customs.CN.Module;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = " A Filter Description should not be localized as it is used as the key for persistence, MultilingualDescription is used for display.")]
public static class EntryHeaderFilterConstants
{
	public const string DeclarationUnifiedNumber = "Declaration Unified Number";
	public const string DeclarationOfficeOfEntryExit = "Office of Entry/Exit";
	public const string EntryInstructionManualNo = "Manual No.";
	public const string EntryInstructionPackages = "No of Packages";
	public const string CIQNumber = "CIQ Number";
	public const string BillOfLading = "Bill of lading";
	public const string CIQStatus = "CIQ Status";
	public const string DeclarationDate = "Declaration Date";
	public const string CustomsProcedure = "Customs Procedure";
	public const string ArchiveDate = "Archive Date";
	public const string ArchiveUser = "Archive User";
	public const string AuditedDate = "Audited Date";
	public const string AuditedUser = "Audited User";
	public const string ReadyForCompleteDeclaration = "Ready For Complete Declaration";
	public const string ACDANumber = "ACDA Number";
}
