namespace Enterprise.Customs.ES.Business
{
	public enum ResponseStatus : int
	{
		Successful = 200,
		BadRequest = 400,
		Unauthorized = 401,
		InternalServerError = 500,
		Timeout = 504,
		// todo: is there a better HTTP code (just to keep the pattern)?
		Duplicate = 409
	}
}
