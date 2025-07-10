using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(EDIDataRegistry))]
	partial class EDIDataRegistryTest
	{
		public void TestMainRepositories()
		{
			TestRegistryItem(
				ItemSet.MainRepositories,
				"MainRepositories",
				"WiseTech Global Client Extensions/Escrow Source Exporter",
				"Main repositories",
				@"List of repositories to retrieve.
Dependency traversal starts from the root of the repository or from an optional path inside the repository separated by a semicolon.
For example,
""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev""
""https://github.com/WiseTechGlobal/Glow;/DotNet""
",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new[]
				{
					"https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev;/",
					"https://github.com/WiseTechGlobal/Glow;/DotNet",
				});
		}

		public void TestIncidentConfiguration()
		{
			TestRegistryItem(
				ItemSet.EscrowIncidentConfiguration,
				"EscrowIncidentConfiguration",
				"WiseTech Global Client Extensions/Escrow Source Exporter",
				"Incident configuration",
				@"Incident product code, module code and priority code delimited by |.
For example, if the value is AAA|BBB|CR9, the incident being created will have AAA as IM_Product, BBB as IM_Module and CR9 as IM_Priority.
Please use valid codes.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"IST||CR9");
		}

		[TestDate(2025, 1, 1, 10, 0, 0)]
		public void TestIncidentMessage()
		{
			var now = ZDateTime.Now;
			var currentDate = now.ToString("dd/MM/yyyy");
			var currentMonth = now.ToString("MMMM yyyy");

			TestRegistryItem(
				ItemSet.EscrowIncidentMessage,
				"EscrowIncidentMessage",
				"WiseTech Global Client Extensions/Escrow Source Exporter",
				"Incident Message",
				"The contents of DetailNoteText within IncidentCreator.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Memo,
				@$"Source code and binary assets for third-party escrow agent have been prepared.

1. Prepare the document ""Escrow Deposit Lodgement Form"" from the template modifying CargoWiseOne field to the date ""{currentDate}"" and print it.
2. Enable BitLocker To Go on the USB flash drive, using a randomly generated password 20 characters in length.
3. Email the password to chrissyl@assurex-escrow.com.au

   Subject: CargoWise escrow password ""{currentDate}""
   Email Body: <password>

4. Download the content from the ProGet Assets directory using the link https://a.com/assets/EscrowExport/Escrow202409/ and copy it to the USB flash drive.
5. Courier the USB flash drive and accompanying form to the address:

   Assurex Escrow Pty Ltd
   Suite 93, Level 5, 330 Wattle Street, Ultimo NSW 2007, Australia.

   Att:  Chrissy Leahy / Robyn Quigley
   02 9211 0522

6. After sending the package, please email the tracking number to
   maree.isaacs@wisetechglobal.com; geoff.bennett@wisetechglobal.com; mikhail.gerasimov@wisetechglobal.com
   with the email subject ""Escrow {currentMonth}""

Thank you.
");
		}
	}
}
