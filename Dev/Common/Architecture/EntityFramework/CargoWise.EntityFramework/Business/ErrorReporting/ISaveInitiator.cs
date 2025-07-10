namespace CargoWise.EntityFramework
{
	public interface ISaveInitiator
	{
		bool SaveExceptionCaughtAlready { get; set; }
		IBusiness BusinessEntityForValidation { get; }
		void ShowErrorsDialog();
	}
}
