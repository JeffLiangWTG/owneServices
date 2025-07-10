using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNSWClientSettingValidationTest : TestCaseWithFactory
	{
		public void TestValidateMachineName_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.MachineNameInfo);
		}

		public void TestValidateMachineName_CheckEntered_IfNotAllEmpty()
		{
			bizObj.ArchiveFolder = @"D:\Folders\ArchiveFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.MachineNameInfo);
		}

		public void TestValidateSendFolder_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.SendFolderInfo);
		}

		public void TestValidateSendFolder_CheckEntered_IfNotAllEmpty()
		{
			bizObj.ArchiveFolder = @"D:\Folders\ArchiveFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.SendFolderInfo);
		}

		public void TestValidateSendFolder_UniqueLocalFolder_IfAllEmpty()
		{
			AssertWillNotValidateIsLocalPathAndNotDuplicated(bizObj.SendFolderInfo);
		}

		public void TestValidateSendFolder_UniqueLocalFolder_IfNotAllEmpty()
		{
			bizObj.ArchiveFolder = @"D:\Folders\ArchiveFolder\";
			AssertValidateIsLocalPathAndNotDuplicated(bizObj.SendFolderInfo);
		}

		public void TestValidateReceiveFolder_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.ReceiveFolderInfo);
		}

		public void TestValidateReceiveFolder_CheckEntered_IfNotAllEmpty()
		{
			bizObj.ArchiveFolder = @"D:\Folders\ArchiveFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.ReceiveFolderInfo);
		}

		public void TestValidateReceiveFolder_UniqueLocalFolder_IfAllEmpty()
		{
			AssertWillNotValidateIsLocalPathAndNotDuplicated(bizObj.ReceiveFolderInfo);
		}

		public void TestValidateReceiveFolder_UniqueLocalFolder_IfNotAllEmpty()
		{
			bizObj.ArchiveFolder = @"D:\Folders\ArchiveFolder\";
			AssertValidateIsLocalPathAndNotDuplicated(bizObj.ReceiveFolderInfo);
		}

		public void TestValidateErrorResponseFolder_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.ErrorResponseFolderInfo);
		}

		public void TestValidateErrorResponseFolder_CheckEntered_IfNotAllEmpty()
		{
			bizObj.ArchiveFolder = @"D:\Folders\ArchiveFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.ErrorResponseFolderInfo);
		}

		public void TestValidateErrorResponseFolder_UniqueLocalFolder_IfAllEmpty()
		{
			AssertWillNotValidateIsLocalPathAndNotDuplicated(bizObj.ErrorResponseFolderInfo);
		}

		public void TestValidateErrorResponseFolder_UniqueLocalFolder_IfNotAllEmpty()
		{
			bizObj.ArchiveFolder = @"D:\Folders\ArchiveFolder\";
			AssertValidateIsLocalPathAndNotDuplicated(bizObj.ErrorResponseFolderInfo);
		}

		public void TestValidateArchiveFolder_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.ArchiveFolderInfo);
		}

		public void TestValidateArchiveFolder_CheckEntered_IfNotAllEmpty()
		{
			bizObj.ErrorResponseFolder = @"D:\Folders\ErrorResponseFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.ArchiveFolderInfo);
		}

		public void TestValidateArchiveFolder_UniqueLocalFolder_IfAllEmpty()
		{
			AssertWillNotValidateIsLocalPathAndNotDuplicated(bizObj.ArchiveFolderInfo);
		}

		public void TestValidateArchiveFolder_UniqueLocalFolder_IfNotAllEmpty()
		{
			bizObj.ErrorResponseFolder = @"D:\Folders\ErrorResponseFolder\";
			AssertValidateIsLocalPathAndNotDuplicated(bizObj.ArchiveFolderInfo);
		}

		#region ACDA Folders

		public void TestValidateAcdaSendFolder_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.AcdaSendFolderInfo);
		}

		public void TestValidateAcdaSendFolder_CheckEntered_IfNotAllEmpty()
		{
			bizObj.AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.AcdaSendFolderInfo);
		}

		public void TestValidateAcdaSendFolder_UniqueLocalFolder_IfAllEmpty()
		{
			AssertWillNotValidateIsLocalPathAndNotDuplicated(bizObj.AcdaSendFolderInfo);
		}

		public void TestValidateAcdaSendFolder_UniqueLocalFolder_IfNotAllEmpty()
		{
			bizObj.AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder\";
			AssertValidateIsLocalPathAndNotDuplicated(bizObj.AcdaSendFolderInfo);
		}

		public void TestValidateAcdaReceiveFolder_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.AcdaReceiveFolderInfo);
		}

		public void TestValidateAcdaReceiveFolder_CheckEntered_IfNotAllEmpty()
		{
			bizObj.AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.AcdaReceiveFolderInfo);
		}

		public void TestValidateAcdaReceiveFolder_UniqueLocalFolder_IfAllEmpty()
		{
			AssertWillNotValidateIsLocalPathAndNotDuplicated(bizObj.AcdaReceiveFolderInfo);
		}

		public void TestValidateAcdaReceiveFolder_UniqueLocalFolder_IfNotAllEmpty()
		{
			bizObj.AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder\";
			AssertValidateIsLocalPathAndNotDuplicated(bizObj.AcdaReceiveFolderInfo);
		}

		public void TestValidateAcdaErrorResponseFolder_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.AcdaErrorResponseFolderInfo);
		}

		public void TestValidateAcdaErrorResponseFolder_CheckEntered_IfNotAllEmpty()
		{
			bizObj.AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.AcdaErrorResponseFolderInfo);
		}

		public void TestValidateAcdaErrorResponseFolder_UniqueLocalFolder_IfAllEmpty()
		{
			AssertWillNotValidateIsLocalPathAndNotDuplicated(bizObj.AcdaErrorResponseFolderInfo);
		}

		public void TestValidateAcdaErrorResponseFolder_UniqueLocalFolder_IfNotAllEmpty()
		{
			bizObj.AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder\";
			AssertValidateIsLocalPathAndNotDuplicated(bizObj.AcdaErrorResponseFolderInfo);
		}

		public void TestValidateAcdaArchiveFolder_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.AcdaArchiveFolderInfo);
		}

		public void TestValidateAcdaArchiveFolder_CheckEntered_IfNotAllEmpty()
		{
			bizObj.AcdaErrorResponseFolder = @"D:\Folders\AcdaErrorResponseFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.AcdaArchiveFolderInfo);
		}

		public void TestValidateAcdaArchiveFolder_UniqueLocalFolder_IfAllEmpty()
		{
			AssertWillNotValidateIsLocalPathAndNotDuplicated(bizObj.AcdaArchiveFolderInfo);
		}

		public void TestValidateAcdaArchiveFolder_UniqueLocalFolder_IfNotAllEmpty()
		{
			bizObj.AcdaErrorResponseFolder = @"D:\Folders\AcdaErrorResponseFolder\";
			AssertValidateIsLocalPathAndNotDuplicated(bizObj.AcdaArchiveFolderInfo);
		}

		#endregion

		public void TestValidateRunningIntervalInSeconds_CheckEntered_IfAllEmpty()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(bizObj.RunningIntervalInSecondsInfo);
		}

		public void TestValidateRunningIntervalInSeconds_CheckEntered_IfNotAllEmpty()
		{
			bizObj.ErrorResponseFolder = @"D:\Folders\ErrorResponseFolder\";
			ValidationTestHelper.AssertErrorIfNotEntered(bizObj.RunningIntervalInSecondsInfo);
		}

		public void TestValidateRunningIntervalInSeconds_CannotBeSmallerThan15Seconds_IfAllEmpty()
		{
			AssertEquals(true, !bizObj.RunningIntervalInSecondsInfo.HasError("Running Interval cannot be smaller than 15 seconds."));
		}

		public void TestValidateRunningIntervalInSeconds_CannotBeSmallerThan15Seconds_IfNotAllEmpty()
		{
			const string message = "Running Interval cannot be smaller than 15 seconds.";
			CombineAssertions(() =>
			{
				bizObj.ErrorResponseFolder = @"D:\Folders\ErrorResponseFolder\";
				bizObj.RunningIntervalInSeconds = 13;
				AssertHasError("<15", bizObj.RunningIntervalInSecondsInfo, message);
				bizObj.RunningIntervalInSeconds = 15;
				AssertNoError("=15", bizObj.RunningIntervalInSecondsInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			bizObj = new CNSWClientSetting();
		}
		CNSWClientSetting bizObj;

		void AssertValidateIsLocalPathAndNotDuplicated(ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = (ZString)"Folder";
			AssertHasError("Not valid local path", propertyInfo, "[FOLDER] is not a local folder path.");

			var propertyInfoName = propertyInfo.Name;
			var otherInfo = new[]
			{
				bizObj.SendFolderInfo, bizObj.ReceiveFolderInfo, bizObj.ErrorResponseFolderInfo, bizObj.ArchiveFolderInfo,
				bizObj.AcdaSendFolderInfo, bizObj.AcdaReceiveFolderInfo, bizObj.AcdaErrorResponseFolderInfo, bizObj.AcdaArchiveFolderInfo
			}.First(x => x.Name != propertyInfoName);
			otherInfo.Value = (ZString)@"D:\Folders\Folder1\";
			propertyInfo.Value = (ZString)@"D:\Folders\Folder1\";
			AssertHasError("Duplicate", propertyInfo, "Folder path cannot be duplicated.");
			propertyInfo.Value = (ZString)@"D:\Folders\Folder2\";
			AssertNoError("Unique", bizObj.SendFolderInfo, "Folder path cannot be duplicated.");
		}

		void AssertWillNotValidateIsLocalPathAndNotDuplicated(ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = (ZString)"Folder";
			AssertEquals("Not valid local path", true, !bizObj.RunningIntervalInSecondsInfo.HasError($"[{bizObj.SendFolder}] is not a local folder path."));

			var propertyInfoName = propertyInfo.Name;
			var otherInfo = new[]
			{
				bizObj.SendFolderInfo, bizObj.ReceiveFolderInfo, bizObj.ErrorResponseFolderInfo, bizObj.ArchiveFolderInfo,
				bizObj.AcdaSendFolderInfo, bizObj.AcdaReceiveFolderInfo, bizObj.AcdaErrorResponseFolderInfo, bizObj.AcdaArchiveFolderInfo
			}.First(x => x.Name != propertyInfoName);
			otherInfo.Value = (ZString)@"D:\Folders\Folder1\";
			propertyInfo.Value = (ZString)@"D:\Folders\Folder1\";
			AssertEquals("Duplicated", true, !bizObj.RunningIntervalInSecondsInfo.HasError("Folder path cannot be duplicated."));
		}
	}
}
