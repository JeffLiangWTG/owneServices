namespace Enterprise.MailManager.ExternalMailInterface
{
	public class UserPasswordAuthConfiguration
	{
		public UserPasswordAuthConfiguration(string userName, string password)
		{
			UserName = userName;
			Password = password;
		}

		public string UserName { get; private set; }
		public string Password { get; private set; }
	}
}
