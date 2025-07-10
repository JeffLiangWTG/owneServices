namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class AccountingExportTasksCreatorTest : CompanyLevelXMLTasksCreatorTest
	{
		protected override CompanyLevelXMLTasksCreator XMLTasksCreator
		{
			get { return AccXMLTasksCreator; }
		}

		AccountingExportTasksCreator AccXMLTasksCreator
		{
			get { return accXMLTasksCreator ?? (accXMLTasksCreator = new AccountingExportTasksCreator(Buffer, Factory)); }
		}
		AccountingExportTasksCreator accXMLTasksCreator;
	}
}
