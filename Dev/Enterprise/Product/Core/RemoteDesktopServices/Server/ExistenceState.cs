namespace Enterprise.RemoteDesktopServices
{
	enum ExistenceState
	{
		ExistingNormally,
		ExistingButNotFile,
		ExistingButNotDirectory,
		ExistingButNoPermission,
		UnauthorizedError,
		ParentDirectoryNotExisting,
		NotExisting,
		InvalidCharacterInPath,
#if NETCOREAPP
		PlatformNotSupported,
#endif
	}
}
