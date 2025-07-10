namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class SnapshotForEHubNativeOrg
	{
		public string CustomerNumber { get; set; }
		public string CustomerName { get; set; }
		public string ContactName { get; set; }
		public string Phone { get; set; }
		public string Fax { get; set; }
		public string EmailAddress { get; set; }
		public string FEIN { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Country { get; set; }
		public string Zip { get; set; }
		public bool CompanyDataChanged { get; set; }

		public bool AreEqual(SnapshotForEHubNativeOrg databaseSnapshot)
		{
			return databaseSnapshot != null
					&& CustomerNumber == databaseSnapshot.CustomerNumber
					&& CustomerName == databaseSnapshot.CustomerName
					&& ContactName == databaseSnapshot.ContactName
					&& Phone == databaseSnapshot.Phone
					&& Fax == databaseSnapshot.Fax
					&& EmailAddress == databaseSnapshot.EmailAddress
					&& FEIN == databaseSnapshot.FEIN
					&& Address1 == databaseSnapshot.Address1
					&& Address2 == databaseSnapshot.Address2
					&& City == databaseSnapshot.City
					&& State == databaseSnapshot.State
					&& Country == databaseSnapshot.Country
					&& Zip == databaseSnapshot.Zip
					&& !databaseSnapshot.CompanyDataChanged;
		}
	}
}
