using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public static class CNSWClientSettingChecker
	{
		public static ZString CheckForDeclarationMessageSending(JobDeclaration parentDeclaration)
		{
			var result = CheckForMessageSending(parentDeclaration, out var setting);
			if (result.IsEmpty && setting.SendFolder.IsEmpty)
			{
				result = SendFolderIsEmptyMessage;
			}
			return result;
		}

		public static ZString CheckForAcdAgrMessageSending(JobDeclaration jobDeclaration)
		{
			var result = CheckForMessageSending(jobDeclaration, out var setting);
			if (result.IsEmpty && setting.AcdaSendFolder.IsEmpty)
			{
				result = AcdaSendFolderIsEmptyMessage;
			}
			return result;
		}

		static ZString CheckForMessageSending(JobDeclaration declaration, out CNSWClientSetting setting)
		{
			Argument.NotNull(declaration, nameof(declaration));
			var result = ZString.Empty;
			setting = CNCustomsDataRegistry.Instance.CNSWClientSetting.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);
			if (setting == null || !setting.EHubClientRegistered)
			{
				result = EHubClientNotRegisteredMessage;
			}

			return result;
		}

		public static string EHubClientNotRegisteredMessage => Res.GetString("82ee0b8b-4f96-479f-99d0-5e47b37f7265", "Single Window Client Application Settings should be entered for submitting the messages to CN Customs. Please go to Registry -> {0} and ensure the status of the eHub ID is valid.", CNSWClientSettingPath);

		internal static string SendFolderIsEmptyMessage => Res.GetString("8BD2431B-D9DA-4BD0-AF93-C2C967F265C2", "The Send Folder of Customs Declaration cannot be empty. Please set it in Registry -> {0}.", CNSWClientSettingPath);

		internal static string AcdaSendFolderIsEmptyMessage => Res.GetString("5F65DAF6-DAEB-4D26-98B9-0556765A34D4", "The Send Folder of Agreement of Customs Declaration Agent cannot be empty. Please set it in Registry -> {0}.", CNSWClientSettingPath);

		static string CNSWClientSettingPath => CNCustomsDataRegistry.Instance.CNSWClientSetting.HumanReadableRegistryPath();
	}
}
