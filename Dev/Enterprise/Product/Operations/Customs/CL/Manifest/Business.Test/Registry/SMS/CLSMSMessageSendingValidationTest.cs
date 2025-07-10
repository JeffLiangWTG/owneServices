using CargoWise.EntityFramework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	partial class CLSMSMessageSendingTest
	{
		public void TestAllEmpty()
		{
			BizObj.RunPreSaveValidation();

			AssertNoErrors(BizObj.MachineNameInfo);
			AssertNoErrors(BizObj.ApplicationNodeNameInfo);
			AssertNoErrors(BizObj.ApplicationNodePasswordInfo);
			AssertNoErrors(BizObj.RunningIntervalInSecondsInfo);
			AssertNoErrors(BizObj.SendFolderInfo);
			AssertNoErrors(BizObj.UnknownFolderInfo);
			AssertNoErrors(BizObj.InvalidFolderInfo);
			AssertNoErrors(BizObj.RejectedFolderInfo);
			AssertNoErrors(BizObj.ReceiveFolderInfo);
			AssertNoErrors(BizObj.AcceptedFolderInfo);
		}

		public void TestValidateMachineName()
		{
			BizObj.SendFolder = @"D:\Folders\SendFolder";
			BizObj.Validation.ValidateMachineName();
			AssertHasErrorContaining(BizObj.MachineNameInfo, MandatoryValidation.MustBeEntered);

			BizObj.MachineName = "Machine 1";
			AssertNoErrors(BizObj.MachineNameInfo);
		}

		public void TestValidateApplicationNodePassword()
		{
			BizObj.MachineName = "Machine";
			BizObj.ApplicationNodePassword = string.Empty;
			AssertHasErrorContaining(BizObj.ApplicationNodePasswordInfo, MandatoryValidation.MustBeEntered);

			BizObj.ApplicationNodePassword = "1234";
			AssertNoErrors(BizObj.ApplicationNodePasswordInfo);
		}

		public void TestRunningInterval()
		{
			BizObj.RunningIntervalInSeconds = 14;
			AssertHasError(BizObj.RunningIntervalInSecondsInfo, "Running Interval cannot be smaller than 15 seconds.");

			BizObj.RunningIntervalInSeconds = 16;
			AssertNoErrors(BizObj.RunningIntervalInSecondsInfo);
		}

		public void TestValidateFolders()
		{
			BizObj.MachineName = "Machine 1";
			BizObj.Validation.ValidateSendFolder();
			AssertHasErrorContaining(BizObj.SendFolderInfo, MandatoryValidation.MustBeEntered);
			BizObj.SendFolder = "Folder";
			AssertNoErrorContaining(BizObj.SendFolderInfo, MandatoryValidation.MustBeEntered);
			AssertHasError(BizObj.SendFolderInfo, $"[{BizObj.SendFolder}] is not a local folder path.");

			BizObj.ReceiveFolder = @"D:\Folders\ReceiveFolder\";
			BizObj.SendFolder = @"D:\Folders\ReceiveFolder\";
			AssertHasError(BizObj.SendFolderInfo, "Folder path cannot be duplicated.");

			BizObj.SendFolder = @"D:\Folders\SendFolder\";
			AssertNoErrors(BizObj.SendFolderInfo);

			BizObj.ReceiveFolder = @"D:\Folders\SendFolder\";
			AssertHasError(BizObj.ReceiveFolderInfo, "Folder path cannot be duplicated.");

			BizObj.ReceiveFolderInfo.ClearAllNotifications();
			BizObj.ReceiveFolder = @"D:\Folders\ReceiveFolder\";
			BizObj.UnknownFolder = @"D:\Folders\ReceiveFolder\";
			AssertHasError(BizObj.UnknownFolderInfo, "Folder path cannot be duplicated.");

			BizObj.UnknownFolder = @"D:\Folders\UnknownFolder\";
			AssertNoErrors(BizObj.UnknownFolderInfo);
		}
	}
}
