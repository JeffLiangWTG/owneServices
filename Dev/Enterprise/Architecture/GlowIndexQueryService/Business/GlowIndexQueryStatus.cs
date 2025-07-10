namespace GlowIndexQueryService.Business
{
	public enum GlowIndexQueryStatus
	{
		Success,
		UserNotLoggedIn,
		GlowDisabled,
		Uninitialised,
		UnknownEntityType,
		UnknownCategoryType,
		UnknownSearchField,
		UnKnownLookupMetadata,
		EntityTypeAndCategoryTypeCoexist,
		BlankSearchTerms,
		UnknownError,
		BadRequest,
		DeadService,
		ServiceUnavailable,
	}
}
