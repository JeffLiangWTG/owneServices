using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ClientSharedComponents;
using Enterprise.ZArchitecture;
using ZClientMFI.Common;

namespace Enterprise.Client.MFI.AutoeDocAllocation
{
	public static class AutoeDocAllocationValidateEnvironment
	{
		public static bool Validate(INotifications notify, BusinessObjectFactory factory)
		{
			bool result = true;

			if (MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory.IsEmpty || !Directory.Exists(MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory))
			{
				var errorMessage = Helper.AccessToFolderErrorMessage(MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory);
				notify.Notify(new ErrorNotification(ErrorType.MissingDataDirectory, "MFI AutoeDoc Allocation source directory registry value is missing or directory is not valid: '" + MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory + "'. Exception details:\r\n" + errorMessage));
				result = false;
			}

			if (MFIDataRegistry.Instance.AutoeDocAllocationHoldDirectory.IsEmpty || !Directory.Exists(MFIDataRegistry.Instance.AutoeDocAllocationHoldDirectory))
			{
				var errorMessage = Helper.AccessToFolderErrorMessage(MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory);
				notify.Notify(new ErrorNotification(ErrorType.MissingDataDirectory, "MFI AutoeDoc Allocation hold directory registry value is missing or directory is not valid: '" + MFIDataRegistry.Instance.AutoeDocAllocationHoldDirectory + "'. Exception details:\r\n" + errorMessage));
				result = false;
			}

			if (!NotificationGroupValidator.IsValid(MFIDataRegistry.Instance.AutoeDocAllocationNotificationGroup, factory))
			{
				notify.Notify(new ErrorNotification(ErrorType.EmailNotifyGroupNotExist, "Auto eDoc Allocation notification group has not been set in registry or is invalid."));
				result = false;
			}

			return result;
		}
	}
}
