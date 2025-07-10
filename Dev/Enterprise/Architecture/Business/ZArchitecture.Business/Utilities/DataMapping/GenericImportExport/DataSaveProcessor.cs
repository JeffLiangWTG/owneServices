using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class DataSaveProcessor : DataTransferProcessor
	{
		public DataSaveProcessor(BusinessObjectFactory factory, string inProgressMessage, string completedMessage)
		{
			this.factory = factory;
			this.inProgressMessage = inProgressMessage;
			this.completedMessage = completedMessage;
		}

		readonly BusinessObjectFactory factory;
		readonly string inProgressMessage;
		readonly string completedMessage;

		const int OnSavingPercentage = 1;
		const int OnSavingCompletePercentage = 100;
		const int SleepTime = 500;

		public override void Import()
		{
			OnSaving(OnSavingPercentage, inProgressMessage);
			factory.Save();
			OnSavingComplete(OnSavingCompletePercentage, completedMessage);
			System.Threading.Thread.Sleep(SleepTime); //to let users can see the progress percentage move to the end before the progress bar disppears
		}

		public override void Rollback()
		{
		}
	}
}
