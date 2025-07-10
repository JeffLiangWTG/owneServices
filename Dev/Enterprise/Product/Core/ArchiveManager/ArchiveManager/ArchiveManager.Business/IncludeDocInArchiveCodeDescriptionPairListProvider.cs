using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Business
{
	class IncludeDocInArchiveCodeDescriptionPairListProvider : IIncludeDocInArchiveCodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public CodeDescriptionPairList CodeDescriptionPairList
		{
			get
			{
				if (includeDocInArchiveList == null)
				{
					includeDocInArchiveList = new CodeDescriptionPairList();
					includeDocInArchiveList.AddPair(ArchiveConstants.IncludeDocInArchiveCodes.No, Res.GetString("83a5ce0c-d0c0-4913-bf19-fa92a427edef", "Not included in archiving"));
					includeDocInArchiveList.AddPair(ArchiveConstants.IncludeDocInArchiveCodes.Yes, Res.GetString("166CA562-AD1A-4A9C-8666-234034F6CA14", "Included in archiving"));
				}

				return includeDocInArchiveList;
			}
		}

		CodeDescriptionPairList includeDocInArchiveList;

		#endregion
	}
}
