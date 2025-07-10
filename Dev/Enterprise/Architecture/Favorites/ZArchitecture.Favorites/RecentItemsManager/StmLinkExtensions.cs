using System;
using System.Linq;

namespace Enterprise.ZArchitecture.Favorites
{
	public static class StmLinkExtensions
	{
		public static void RemoveFromCollection(this StmLinkCollection collection, LinkWrapper shortcut)
		{
			collection.UpdateCollection(shortcut, (l, c) => c.Delete(l));
		}

		public static void UpdateCollection(this StmLinkCollection collection, LinkWrapper shortcut, Action<StmLink, StmLinkCollection> action)
		{
			var match = collection.OfType<StmLink>().FirstOrDefault(t => LinkWrapper.AreShortcutsEqual(shortcut, t));
			if (match != null)
			{
				action(match, collection);
			}
		}

		public static void AddToCollection(this StmLinkCollection collection, LinkWrapper shortcut)
		{
			collection.AddNew(shortcut);
			collection.CleanUp();
		}

		public static void InitializeLink(this StmLink link, LinkWrapper wrapper)
		{
			string wrapperRecordDescriptionn = null;
			if (wrapper.RecordDescription != null && wrapper.RecordDescription.Length > link.STL_ItemDescriptionInfo.MaxLength)
			{
				wrapperRecordDescriptionn = wrapper.RecordDescription.Substring(0, link.STL_ItemDescriptionInfo.MaxLength - 3) + "...";
			}

			link.STL_ModuleID = wrapper.ModuleName;
			link.STL_ItemUrl = wrapper.RecordUrl;
			link.STL_ItemDescription = wrapperRecordDescriptionn ?? wrapper.RecordDescription;
			link.STL_ItemPK = wrapper.RecordKey;
		}

		#region Test
#if DEBUG

		public static void AddToCollectionForTest(this StmLinkCollection collection, LinkWrapper shortcut)
		{
			var link = collection.AddNew();
			link.InitializeLink(shortcut);
			collection.SetLinkDefaults(link);
		}

#endif
		#endregion
	}
}
