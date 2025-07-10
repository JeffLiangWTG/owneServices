using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.DeclarationStatusUpdater
{
	public static class DeclarationEntrySubmittedDateUpdater
	{
		public static void Update(JobDeclaration declaration)
		{
			declaration.JE_EntrySubmittedDate = GetEarliestEntrySubmittedDate(declaration);
		}

		static ZDateTime GetEarliestEntrySubmittedDate(JobDeclaration declaration)
		{
			ZDateTime result = ZDateTime.Empty;
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (!entry.CH_EntrySubmittedDate.IsEmpty)
				{
					if (result.IsEmpty || result > entry.CH_EntrySubmittedDate)
					{
						result = entry.CH_EntrySubmittedDate;
					}
				}
			}
			return result;
		}
	}
}
