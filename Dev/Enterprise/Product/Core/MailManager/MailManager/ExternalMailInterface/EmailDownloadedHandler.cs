namespace Enterprise.MailManager.ExternalMailInterface
{
	public delegate void EmailDownloadedHandler(string uniqueId, ref string email, ref bool continueDownloading);
}
