using System.Collections.Generic;
using System.Linq;
using static Enterprise.MailManager.ExternalMailInterface.Testing.SimpleSmtpServer;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	internal class EmailContainer
	{
		readonly Dictionary<int, SimpleEmail> emailDictionary = new ();

		public void Add(SimpleEmail email)
		{
			var maxUid = emailDictionary.Count == 0 ? 0 : emailDictionary.Keys.Max();
			emailDictionary.Add(maxUid + 1, email);
		}

		public Dictionary<int, SimpleEmail> GetEmailDictionary()
		{
			return emailDictionary;
		}

		public void ClearAll()
		{
			emailDictionary.Clear();
		}

		public (int count, int total) GetEmailCount()
		{
			return (emailDictionary.Count, emailDictionary.Values.Sum(email => email.Data.Length));
		}

		public SimpleEmail GetOneEmail(int uid, out bool isError)
		{
			SimpleEmail email ;
			isError = !emailDictionary.TryGetValue(uid, out email);
			return isError ? new SimpleEmail() : email;
		}

		public void DeleteEmail(int uid, out bool isError)
		{
			isError = !emailDictionary.Remove(uid);
		}
	}
}
