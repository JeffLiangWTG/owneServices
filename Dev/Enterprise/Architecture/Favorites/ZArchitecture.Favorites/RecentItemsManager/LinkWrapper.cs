using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Favorites
{
	public class LinkWrapper : ILinkWrapper
	{
		public LinkWrapper()
		{
		}

		public LinkWrapper(string moduleName, Guid recordKey, string recordUrl, string recordDescription)
		{
			ModuleName = moduleName;
			RecordKey = recordKey;
			RecordUrl = recordUrl;
			RecordDescription = recordDescription;
		}

		public LinkWrapper(string moduleName)
			: this(moduleName, Guid.Empty, null, null)
		{
		}

		public LinkWrapper(StmLink shortcut)
		{
			ModuleName = shortcut.STL_ModuleID;

			if (!shortcut.STL_ItemUrl.IsEmpty)
			{
				RecordUrl = shortcut.STL_ItemUrl;
			}

			if (!shortcut.STL_ItemDescription.IsEmpty)
			{
				RecordDescription = shortcut.STL_ItemDescription;
			}

			RecordKey = shortcut.STL_ItemPK.IsEmpty ? Guid.Empty : shortcut.STL_ItemPK.ToGuid();
		}

		public string ModuleName { get; private set; }
		public Guid RecordKey { get; private set; }
		public string RecordUrl { get; private set; }
		public string RecordDescription { get; private set; }

		public string UniqueKey
		{
			get { return RecordKey == Guid.Empty ? ModuleName : string.Format("{0}{1}", ModuleName, RecordKey); }
		}

		public bool IsModule
		{
			get { return RecordKey == Guid.Empty; }
		}

		public static bool AreShortcutsEqual(LinkWrapper wrapper, StmLink shortcut)
		{
			var anotherWrapper = new LinkWrapper(shortcut);

			return anotherWrapper.UniqueKey == wrapper.UniqueKey;
		}
	}
}
