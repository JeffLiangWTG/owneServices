using CargoWise.Types;

namespace Enterprise.ExcelTemplates.Integration
{
	public interface IStmTemplate
	{
		ZString SO_Name { get; }
		ZString SO_ExcelTemplatePath { get; }
		ZString SO_DataContext { get; }
		ZBlob SO_Template { get; }

		ZBool SO_CannotEditDocumentData { get; }
		ZBool SO_IsClientSpecific { get; }
		ZBool SO_IsPasswordProtected { get; }
		ZBool SO_IsSystemDefined { get; }

		ZString SO_TemplateRestriction { get; }
		ZBlob SO_UDFFieldCache { get; }
	}
}
