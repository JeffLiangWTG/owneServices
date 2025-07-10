using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.DeclarationStatusUpdater
{
	public static class DeclarationEntryStatusUpdater
	{
		public static void Update(JobDeclaration declaration)
		{
			var entryStatus = ZString.Empty;
			var entryStatuses = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Select(x => x.CH_EntryStatus).Where(x => !x.IsEmpty)
				.Distinct().Take(2).ToArray();
			switch (entryStatuses.Length)
			{
				case 2:
					entryStatus = MessageStatusList.Codes.MultipleStatus;
					break;
				case 1:
					entryStatus = entryStatuses[0];
					break;
			}

			declaration.JE_EntryStatus = entryStatus;
		}
	}
}
