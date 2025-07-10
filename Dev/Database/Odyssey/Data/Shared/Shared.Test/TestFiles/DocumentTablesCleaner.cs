using CargoWise.Data;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public static class DocumentTablesCleaner
	{
		public static void Clean()
		{
			const string deleteStatement = @"
				DELETE FROM dbo.ProcessTaskNotification;
				DELETE FROM dbo.ProcessTasks;
				DELETE FROM dbo.RateAttachment;
				DELETE FROM dbo.RateAttachmentSet;
				DELETE FROM dbo.OrgDocument;
				DELETE FROM dbo.StmMenuDocumentConfigItem;
				DELETE FROM dbo.StmMenuDocumentConfig;
				DELETE FROM dbo.StmMenuEDocs;
				DELETE FROM dbo.StmMenuTemplatePivot;
				DELETE FROM dbo.StmTemplate;
				DELETE FROM dbo.StmMenuMenuPivot;
				DELETE FROM dbo.StmMenuItem;
				DELETE FROM dbo.RefDocType;
				";

			using (DbCommand command = Db.Connection.Command(deleteStatement))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}
