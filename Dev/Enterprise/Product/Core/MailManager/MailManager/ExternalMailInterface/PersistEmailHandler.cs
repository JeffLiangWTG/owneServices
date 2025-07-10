namespace Enterprise.MailManager.ExternalMailInterface
{
	public delegate void PersistEmailHandler(string uniqueId, string email, ref bool continueDownloading);
}
