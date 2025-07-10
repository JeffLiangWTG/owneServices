using System.Data;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Data
{
	public class ClientDocumentsDataFile : DocumentsDataFile
	{
		public ClientDocumentsDataFile(string clientFilePath)
			: base(clientFilePath)
		{
		}

		protected override string IsClientSpecificFlag
		{
			get { return "Y"; }
		}

		protected override DataSet LoadDataSet()
		{
			return LoadDataFromFile(FileRelativePath);
		}

		protected override void CreateVersionRegistryItem()
		{
			VersionRegistryItem = new DatabaseVersionRegistryItem("ClientDocumentVersion");
		}

		protected override string ResourceNameHeader
		{
			get { return ""; }
		}

		protected override string StmTemplateSelectList()
		{
			return "SO_PK, SO_CannotEditDocumentData, SO_DataContext, SO_ExcelTemplatePath, SO_IsClientSpecific, SO_IsPasswordProtected, SO_IsSystemDefined, SO_IsUserConfigurable, SO_LicenceCode, SO_Name, SO_Template, SO_TemplateRestriction, SO_UDFFieldCache, SO_TemplateType, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser";
		}
	}
}
