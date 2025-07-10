namespace Enterprise.Customs.KR.Business
{
	public static class Export5ASItemExtensionMethods
	{
		public static bool IsHeaderItem(this Export5ASItem item)
		{
			return item.EntryLineNo == Export5ASHeaderCreator.EmptyEntryLineNo;
		}
	}
}
