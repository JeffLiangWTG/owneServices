using System.Collections.Generic;

namespace CargoWise.Services.Calendar
{
	public class ReminderRecipient
	{
		public ReminderRecipient(string name, string email)
		{
			this.Name = name;
			this.Email = email;
		}

		public override bool Equals(object obj)
		{
			return Equals((ReminderRecipient)obj);
		}

		public bool Equals(ReminderRecipient other)
		{
			return Email == other.Email;
		}

		public override int GetHashCode()
		{
			return Email != null ? Email.GetHashCode() : 0;
		}

		public readonly string Name;
		public readonly string Email;
	}

	public class ReminderRecipientCollection : List<ReminderRecipient>
	{
		public void Add(string name, string email)
		{
			if (email.Length > 0)
			{
				ReminderRecipient recipient = new ReminderRecipient(name, email);
				if (!Contains(recipient))
				{
					Add(recipient);
				}
			}
		}
	}
}
