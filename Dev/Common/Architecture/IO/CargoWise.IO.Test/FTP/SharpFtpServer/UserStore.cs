using System.Collections.Generic;
using System.Linq;

namespace CargoWise.IO.Testing.SharpFtpServer
{
	public class UserStore
	{
		readonly List<User> _users = new List<User>();

		public List<User> Users { get { return _users; } }

		public User Validate(string username, string password)
		{
			User user = (from u in _users where u.Username == username && u.Password == password select u).SingleOrDefault();

			return user;
		}
	}
}
