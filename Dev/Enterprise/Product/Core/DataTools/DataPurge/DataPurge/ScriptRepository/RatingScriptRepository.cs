namespace Enterprise.DataPurge
{
	class RatingScriptRepository : ScriptRepository
	{
		protected override string[] PurgeScripts
		{
			get
			{
				return new string[] {
					RatingHeaderScript
				};
			}
		}

		#region Purge Scripts

		#region RatingHeader

		const string RatingHeaderScript = @"
--RatingHeader
DELETE dbo.RateTariffDiscount
DELETE dbo.RateLineItems
DELETE dbo.RateLines
DELETE dbo.RateOneOffContainers
DELETE dbo.RateOneOffPackLine
DELETE dbo.RateOneOffShipment
DELETE dbo.RateAttachment
DELETE dbo.RateEntry
DELETE dbo.RatingHeader";

		#endregion

		#endregion
	}
}
