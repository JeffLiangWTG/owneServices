namespace Enterprise.DbUpgrader.Data
{
	public interface IFixReferencesAndDuplicates
	{
		void PerformExtraDataManipulationBeforeEnablingConstraints();
	}
}
