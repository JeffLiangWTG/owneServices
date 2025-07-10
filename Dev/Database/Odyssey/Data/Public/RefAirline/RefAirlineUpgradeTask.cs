namespace Enterprise.DbUpgrader.Data
{
	/// <summary>
	/// Summary description for RefAirlineUpgradeTask.
	/// </summary>
	public class RefAirlineUpgradeTask : EmbeddedUpgradeTask
	{
		public RefAirlineUpgradeTask() : base(new RefAirlineDataFile())
		{
		}

		/// <summary>
		/// We are no longer updating the data in this table.
		/// So only perform the upgrade if the table is empty.
		/// Reason, from Richard White:
		/// "The airlines list from IATA is total crap and not at all up to date.
		/// The last time we purchased it was 12 years out of date��
		/// At that time we decided to allow customers to perform their own updates."
		/// </summary>
		public override bool IsRequired
		{
			get { return ResourceFile.VersionInDatabase == 0; }
		}

		protected override void DoDelete(System.Data.DataRow targetRow, ref int targetIndex)
		{
			targetIndex++;
		}
	}
}
