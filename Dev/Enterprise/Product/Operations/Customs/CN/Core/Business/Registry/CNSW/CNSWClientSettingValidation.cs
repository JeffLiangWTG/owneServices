using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CNSWClientSettingValidation
	{
		public CNSWClientSettingValidation(CNSWClientSetting parent)
		{
			this.parent = parent;
		}

		readonly CNSWClientSetting parent;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regex Pattern")]
		const string LocalFolderPathRegex = @"^[a-zA-Z]:\\(?:[^\\/:*?"" <>|\r\n]+\\)*[^\\/:*?""<>|\r\n]*$";

		static string FolderCannotBeDuplicatedMsg => Res.GetString("BACDCDB7-4F03-4958-A2A1-2D0634DFC462", "Folder path cannot be duplicated.");

		static string GetIsNotALocalPathMsg(ZString path) => Res.GetString("7E308EDE-AA19-47D3-BF11-0D3865A97DAD", "[{0}] is not a local folder path.", path);

		public void ValidateMachineName()
		{
			var targetInfo = parent.MachineNameInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
		}

		public void ValidateSendFolder()
		{
			var targetInfo = parent.SendFolderInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		public void ValidateReceiveFolder()
		{
			var targetInfo = parent.ReceiveFolderInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		public void ValidateErrorResponseFolder()
		{
			var targetInfo = parent.ErrorResponseFolderInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		public void ValidateArchiveFolder()
		{
			var targetInfo = parent.ArchiveFolderInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		#region ACDA Folders

		public void ValidateAcdaSendFolder()
		{
			var targetInfo = parent.AcdaSendFolderInfo;
			targetInfo.ClearAllNotifications();

			if (HasAnAcdaPath)
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		public void ValidateAcdaReceiveFolder()
		{
			var targetInfo = parent.AcdaReceiveFolderInfo;
			targetInfo.ClearAllNotifications();

			if (HasAnAcdaPath)
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		public void ValidateAcdaErrorResponseFolder()
		{
			var targetInfo = parent.AcdaErrorResponseFolderInfo;
			targetInfo.ClearAllNotifications();

			if (HasAnAcdaPath)
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		public void ValidateAcdaArchiveFolder()
		{
			var targetInfo = parent.AcdaArchiveFolderInfo;
			targetInfo.ClearAllNotifications();

			if (HasAnAcdaPath)
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		bool HasAnAcdaPath => AcdaPathInfos.Any(info => !info.Value.IsEmpty);

		IEnumerable<ZPropertyInfo> AcdaPathInfos => fAcdaPathInfos ?? (fAcdaPathInfos = new List<ZPropertyInfo> { parent.AcdaSendFolderInfo, parent.AcdaReceiveFolderInfo, parent.AcdaErrorResponseFolderInfo, parent.AcdaArchiveFolderInfo });
		IEnumerable<ZPropertyInfo> fAcdaPathInfos;

		#endregion

		void ValidateIsLocalPathAndNotDuplicated(ZPropertyInfo targetInfo)
		{
			var targetValue = (ZString)targetInfo.Value;

			if (!targetValue.IsEmpty)
			{
				if (!Regex.IsMatch(targetValue, LocalFolderPathRegex))
				{
					targetInfo.AddError(GetIsNotALocalPathMsg(targetValue));
				}
				else
				{
					CheckPathDuplicated(targetInfo);
				}
			}
		}

		public void ValidateRunningIntervalInSeconds()
		{
			var targetInfo = parent.RunningIntervalInSecondsInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
				if (parent.RunningIntervalInSeconds < 15)
				{
					targetInfo.AddError(Res.GetString("2B163F6F-926F-4788-A61D-746A6FBB408A", "Running Interval cannot be smaller than 15 seconds."));
				}
			}
		}

		IEnumerable<ZPropertyInfo> fAllPathInfos;
		IEnumerable<ZPropertyInfo> AllPathInfos => fAllPathInfos ?? (fAllPathInfos = new List<ZPropertyInfo> { parent.SendFolderInfo, parent.ReceiveFolderInfo, parent.ErrorResponseFolderInfo, parent.ArchiveFolderInfo });

		void CheckPathDuplicated(ZPropertyInfo info)
		{
			var allPathInfos = AllPathInfos.Concat(AcdaPathInfos);
			var duplicatingInfo = allPathInfos.FirstOrDefault(otherInfo => otherInfo != info && otherInfo.Value.Equals(info.Value));
			if (duplicatingInfo != null)
			{
				info.AddError(FolderCannotBeDuplicatedMsg);
			}
		}

		bool AllEmpty()
		{
			return AllPathInfos.All(info => info.Value.IsEmpty) && (parent.RunningIntervalInSeconds.IsEmpty || parent.RunningIntervalInSeconds == 60) && parent.MachineName.IsEmpty;
		}
	}
}
