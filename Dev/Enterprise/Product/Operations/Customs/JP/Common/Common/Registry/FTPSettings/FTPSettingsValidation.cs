using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Common
{
	public class FTPSettingsValidation : ZValidation
	{
		public FTPSettingsValidation(FTPSettings parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly FTPSettings parent;

		public override void ValidateAll()
		{
			ValidateInFolder();
			ValidateOutFolder();
			ValidatePassword();
			ValidateServer();
			ValidateUserName();
		}

		public override Type AutoValidationType => typeof(FTPSettingsValidation);

		public void ValidateInFolder()
		{
			ValidateCalculatedProperty(parent.InFolderInfo);
		}

		protected void CheckInFolder()
		{
			var targetInfo = parent.InFolderInfo;
			if (!targetInfo.ReadOnly && string.IsNullOrWhiteSpace(parent.InFolder))
			{
				targetInfo.AddError(Res.GetString("CF6E5355-F43C-43B5-8471-C8DF4EA0DD01", "You have not entered {0}, and prevent users from saving.", targetInfo.HumanReadableName));
			}
		}

		public void ValidateOutFolder()
		{
			ValidateCalculatedProperty(parent.OutFolderInfo);
		}

		protected void CheckOutFolder()
		{
			var targetInfo = parent.OutFolderInfo;
			if (!targetInfo.ReadOnly && string.IsNullOrWhiteSpace(parent.OutFolder))
			{
				targetInfo.AddError(Res.GetString("CF6E5355-F43C-43B5-8471-C8DF4EA0DD01", "You have not entered {0}, and prevent users from saving.", targetInfo.HumanReadableName));
			}
		}

		public void ValidatePassword()
		{
			ValidateCalculatedProperty(parent.PasswordInfo);
		}

		protected void CheckPassword()
		{
			var targetInfo = parent.PasswordInfo;
			if (!targetInfo.ReadOnly && string.IsNullOrWhiteSpace(parent.Password))
			{
				targetInfo.AddError(Res.GetString("CF6E5355-F43C-43B5-8471-C8DF4EA0DD01", "You have not entered {0}, and prevent users from saving.", targetInfo.HumanReadableName));
			}
		}

		public void ValidateServer()
		{
			ValidateCalculatedProperty(parent.ServerInfo);
		}

		protected void CheckServer()
		{
			var targetInfo = parent.ServerInfo;
			if (!targetInfo.ReadOnly && string.IsNullOrWhiteSpace(parent.Server))
			{
				targetInfo.AddError(Res.GetString("CF6E5355-F43C-43B5-8471-C8DF4EA0DD01", "You have not entered {0}, and prevent users from saving.", targetInfo.HumanReadableName));
			}
		}

		public void ValidateUserName()
		{
			ValidateCalculatedProperty(parent.UserNameInfo);
		}

		protected void CheckUserName()
		{
			var targetInfo = parent.UserNameInfo;
			if (!targetInfo.ReadOnly && string.IsNullOrWhiteSpace(parent.UserName))
			{
				targetInfo.AddError(Res.GetString("CF6E5355-F43C-43B5-8471-C8DF4EA0DD01", "You have not entered {0}, and prevent users from saving.", targetInfo.HumanReadableName));
			}
		}
	}
}
