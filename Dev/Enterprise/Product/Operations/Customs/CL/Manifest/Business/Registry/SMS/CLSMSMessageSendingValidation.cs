using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLSMSMessageSendingValidation
	{
		public CLSMSMessageSendingValidation(CLSMSMessageSending parent)
		{
			this.parent = parent;
		}
		readonly CLSMSMessageSending parent;

		internal void ValidateAll()
		{
			ValidateMachineName();
			ValidateApplicationNodePassword();
			ValidateRunningIntervalInSeconds();
			ValidateSendFolder();
			ValidateUnknownFolder();
			ValidateInvalidFolder();
			ValidateRejectedFolder();
			ValidateReceiveFolder();
			ValidateAcceptedFolder();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regex Pattern")]
		const string LocalFolderPathRegex = @"^[a-zA-Z]:\\(?:[^\\/:*?"" <>|\r\n]+\\)*[^\\/:*?""<>|\r\n]*$";

		string FolderCannotBeDuplicatedMsg => Res.GetString("E7997048-4DD2-413C-8AFF-EECEE69B07CF", "Folder path cannot be duplicated.");

		string GetIsNotALocalPathMsg(ZString path) => Res.GetString("27A096BC-8439-4231-BB6E-0674D85F1136", "[{0}] is not a local folder path.", path);

		public void ValidateMachineName()
		{
			var targetInfo = parent.MachineNameInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
		}

		public void ValidateApplicationNodePassword()
		{
			var targetInfo = parent.ApplicationNodePasswordInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
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
					targetInfo.AddError(Res.GetString("51D12A1D-3B29-4083-AFCF-3D61ED70A417", "Running Interval cannot be smaller than 15 seconds."));
				}
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

		public void ValidateUnknownFolder()
		{
			var targetInfo = parent.UnknownFolderInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		public void ValidateInvalidFolder()
		{
			var targetInfo = parent.InvalidFolderInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

		public void ValidateRejectedFolder()
		{
			var targetInfo = parent.RejectedFolderInfo;
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

		public void ValidateAcceptedFolder()
		{
			var targetInfo = parent.AcceptedFolderInfo;
			targetInfo.ClearAllNotifications();

			if (!AllEmpty())
			{
				MandatoryValidation.CheckEntered(targetInfo);
				ValidateIsLocalPathAndNotDuplicated(targetInfo);
			}
		}

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

		IEnumerable<ZPropertyInfo> fAllPathInfos;
		IEnumerable<ZPropertyInfo> AllPathInfos => fAllPathInfos ?? (fAllPathInfos = new List<ZPropertyInfo> { parent.SendFolderInfo, parent.UnknownFolderInfo, parent.InvalidFolderInfo, parent.RejectedFolderInfo, parent.ReceiveFolderInfo, parent.AcceptedFolderInfo });

		void CheckPathDuplicated(ZPropertyInfo info)
		{
			var duplicatingInfo = AllPathInfos.FirstOrDefault(otherInfo => otherInfo != info && otherInfo.Value.Equals(info.Value));
			if (duplicatingInfo != null)
			{
				if (duplicatingInfo.HasError(FolderCannotBeDuplicatedMsg))
				{
					duplicatingInfo.ClearAllNotifications();
				}
				info.AddError(FolderCannotBeDuplicatedMsg);
			}
		}

		bool AllEmpty()
		{
			return AllPathInfos.All(info => info.Value.IsEmpty) && (parent.RunningIntervalInSeconds.IsEmpty || parent.RunningIntervalInSeconds == 60) && parent.MachineName.IsEmpty;
		}
	}
}
