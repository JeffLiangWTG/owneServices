using System.Data;
using System.Reflection;
using CargoWise.Common;

namespace Enterprise.DbUpgrader.Data
{
	public class DocumentsCompleteDataFile : DocumentsDataFile
	{
		public DocumentsCompleteDataFile()
			: base(DataFileRelativePath)
		{
		}
		const string DataFileRelativePath = @"Documents\DocumentsComplete.xml";
		public override string ResourceRelativeName => "DbUpgrader.Data.Documents.DocumentsComplete.xml";

		protected override Assembly ResourceAssembly
		{
			get { return AssemblyLoader.LoadAssembly(ResourceAssemblyName); }
		}

		protected override string ResourceNameHeader
		{
			get { return "ExcelTemplates."; }
		}

		protected override string StmTemplateSelectList()
		{
			return "SO_PK, SO_CannotEditDocumentData, SO_DataContext, SO_ExcelTemplatePath, SO_IsClientSpecific, SO_IsPasswordProtected, SO_IsSystemDefined, SO_IsUserConfigurable, SO_LicenceCode, SO_Name, SO_Template, SO_TemplateRestriction, SO_UDFFieldCache, SO_TemplateType, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser";
		}
		internal string InternalStmTemplateSelectList => StmTemplateSelectList();

		protected override int GetVersionCore(DataSet data)
		{
			return DataProxy.DocumentsVersion.VersionNumber;
		}

		public const string ResourceAssemblyName = "ExcelTemplates";
	}
}
