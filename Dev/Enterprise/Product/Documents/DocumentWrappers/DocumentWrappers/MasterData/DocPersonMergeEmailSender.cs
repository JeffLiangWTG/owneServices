using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocPersonMergeEmailSender : DocumentWrapper, Integration.DocumentWrappers.IDocPersonMergeEmailSender
	{
		DocPersonMergeEmailSender(PersonMergeEmailSender personMergeEmailSender, BusinessObjectFactory factoryToWrap) : base(personMergeEmailSender, factoryToWrap)
		{
		}

		public static DocPersonMergeEmailSender New(PersonMergeEmailSender personMergeEmailSender, BusinessObjectFactory factoryToWrap)
		{
			return personMergeEmailSender != null ? new DocPersonMergeEmailSender(personMergeEmailSender, factoryToWrap) : null;
		}

		public PersonMergeEmailSender PersonMergeEmailSender
		{
			get { return (PersonMergeEmailSender)WrappedObject; }
		}

		[DocumentField("Person Name")]
		public ZString PersonName => PersonMergeEmailSender?.RetainedPerson?.FullName ?? ZString.Empty;

		[DocumentField("Merged Accounts")]
		public ZString MergedAccounts
		{
			get
			{
				if (PersonMergeEmailSender == null)
				{
					return ZString.Empty;
				}

				var contacts = new List<PersonMergeEmailSender.IContactWrapper>();

				contacts.AddRange(PersonMergeEmailSender.RetainedPerson.ActiveContactsWithEmail);
				foreach (var dissolvedPerson in PersonMergeEmailSender.SuccessfulDissolvedPersonCollection)
				{
					contacts.AddRange(dissolvedPerson.ActiveContactsWithEmail);
				}

				return GetContactsByEmailGroup(contacts);
			}
		}

		[DocumentField("Merged Accounts Table")]
		public ZString MergedAccountsTable
		{
			get
			{
				if (PersonMergeEmailSender == null)
				{
					return ZString.Empty;
				}

				var contacts = new List<PersonMergeEmailSender.IContactWrapper>();

				contacts.AddRange(PersonMergeEmailSender.RetainedPerson.ActiveContactsWithEmail);
				foreach (var dissolvedPerson in PersonMergeEmailSender.SuccessfulDissolvedPersonCollection)
				{
					contacts.AddRange(dissolvedPerson.ActiveContactsWithEmail);
				}

				return GetContactsTableByEmailGroup(contacts);
			}
		}

		[DocumentField("Accounts with Retained Password")]
		public ZString AccountsWithRetainedPassword
		{
			get
			{
				if (PersonMergeEmailSender == null)
				{
					return ZString.Empty;
				}

				var personWithRetainedPassword = PersonMergeEmailSender.RetainedPerson.HasPassword
					? PersonMergeEmailSender.RetainedPerson
					: PersonMergeEmailSender.SuccessfulDissolvedPersonCollection.FirstOrDefault(x =>
						x.HasPassword && PersonMergeEmailSender.PasswordMatchesRetainedPassword(x));

				if (personWithRetainedPassword == null)
				{
					ErrorReporter.ReportOnce("No Password was present when one was expected", FormattableString.Invariant($"Retained Person PK: {PersonMergeEmailSender.RetainedPerson.PK}"));
					return string.Empty;
				}

				if (!personWithRetainedPassword.ActiveContactsWithEmail.Any())
				{
					return personWithRetainedPassword.FullName;
				}

				return GetContactsByEmailGroup(personWithRetainedPassword.ActiveContactsWithEmail);
			}
		}

		[DocumentField("Accounts with Retained Password Table")]
		public ZString AccountsWithRetainedPasswordTable
		{
			get
			{
				if (PersonMergeEmailSender == null)
				{
					return ZString.Empty;
				}

				var personWithRetainedPassword = PersonMergeEmailSender.RetainedPerson.HasPassword
					? PersonMergeEmailSender.RetainedPerson
					: PersonMergeEmailSender.SuccessfulDissolvedPersonCollection.FirstOrDefault(x =>
						x.HasPassword && PersonMergeEmailSender.PasswordMatchesRetainedPassword(x));

				if (personWithRetainedPassword == null)
				{
					ErrorReporter.ReportOnce("No Password was present when one was expected", FormattableString.Invariant($"Retained Person PK: {PersonMergeEmailSender.RetainedPerson.PK}"));
					return string.Empty;
				}

				if (!personWithRetainedPassword.ActiveContactsWithEmail.Any())
				{
					return personWithRetainedPassword.FullName;
				}

				return GetContactsTableByEmailGroup(personWithRetainedPassword.ActiveContactsWithEmail);
			}
		}

		string GetContactsByEmailGroup(IEnumerable<PersonMergeEmailSender.IContactWrapper> contacts)
		{
			var groupedContacts = contacts.GroupBy(x => x.Email);
			var mergedAccounts = new StringBuilder();
			foreach (var emailGrouping in groupedContacts)
			{
				mergedAccounts.Append(FormattableString.Invariant($"<b>{emailGrouping.Key}<b><br>"));
				foreach (var contact in emailGrouping)
				{
					mergedAccounts.Append(FormattableString.Invariant($"{contact.OrgCode} - {contact.OrgName}<br>"));
				}
			}

			return mergedAccounts.ToString(0, Math.Max(0, mergedAccounts.Length - 4));
		}

		string GetContactsTableByEmailGroup(IEnumerable<PersonMergeEmailSender.IContactWrapper> contacts)
		{
			var groupedContacts = contacts.GroupBy(x => x.Email);
			var mergedAccounts = new StringBuilder((NoResString)"<table cellspacing=\"20\">");
			foreach (var emailGrouping in groupedContacts)
			{
				mergedAccounts.Append(FormattableString.Invariant($"<tr><td><b>{emailGrouping.Key}</b></td>"));
				var contactCount = emailGrouping.Count();
				for (int i = 0; i < contactCount; i++)
				{
					var contact = emailGrouping.ElementAt(i);

					if (i > 0)
					{
						mergedAccounts.Append((NoResString)"<tr><td></td>");
					}

					mergedAccounts.Append(FormattableString.Invariant($"<td>{contact.OrgCode}</td><td>{contact.OrgName}</td></tr>"));
				}
			}

			mergedAccounts.Append((NoResString)"</table>");

			return mergedAccounts.ToString();
		}
	}
}
