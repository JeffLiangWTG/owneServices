using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.DeclarationStatusUpdater
{
	public static class DeclarationMessageStatusUpdater
	{
		public static void Update(JobDeclaration declaration)
		{
			switch (declaration.ActiveEntryHeaders.Count)
			{
				case 0:
					declaration.JE_MessageStatus = ZString.Empty;
					break;
				case 1:
					declaration.JE_MessageStatus = declaration.ActiveEntryHeaders[0].CH_Status.ToString();
					break;
				default:
					declaration.JE_MessageStatus = MessageStatusList.Codes.MultipleStatus;
					break;
			}
		}
	}
}
