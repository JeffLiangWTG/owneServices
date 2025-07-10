namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class LockMechanismConstants
	{
		// Names of application locks in SQL server are limited by 255 characters.
		// We are reserving 64 characters for key and 32 characters for service prefix.
		// The remaining 159 characters can be allocated later as needed.
		public const int MaxPrefixLength = 32;
		public const int MaxKeyLength = 64;
	}
}
