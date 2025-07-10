namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	class StmTranslationFeedbackToPreserve : PreserveTestValueScripts
	{
		protected override string TargetTableName => "StmTranslationFeedback";
	}

	class StmTranslationFeedbackResourceToPreserve : PreserveTestValueScripts
	{
		protected override string TargetTableName => "StmTranslationFeedbackResource";
	}
}
