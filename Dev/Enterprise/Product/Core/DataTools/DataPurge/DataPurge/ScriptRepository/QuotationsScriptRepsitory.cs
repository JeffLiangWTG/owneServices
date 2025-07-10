namespace Enterprise.DataPurge
{
	class QuotationsScriptRepsitory : ScriptRepository
	{
		protected override string[] PurgeScripts
		{
			get
			{
				return new string[] {
					QuotationsRatingHeaderScript,
					QuoteNumberFountainScript,
				};
			}
		}

		#region Purge Scripts

		#region Quotations (RatingHeader)

		const string QuotationsRatingHeaderScript = @"
-- Quotations (RatingHeader)
DECLARE @RatingQuotations TABLE (RatingPk uniqueidentifier)
INSERT @RatingQuotations SELECT TH_PK FROM dbo.RatingHeader WHERE TH_RateType = 'QTE'
DELETE dbo.RateTariffDiscount WHERE TD_TH in (SELECT RatingPk FROM @RatingQuotations)
DELETE dbo.RateLineItems WHERE TM_TL in (SELECT TL_PK FROM dbo.RateLines WHERE TL_TI in (SELECT TI_PK FROM dbo.RateEntry WHERE TI_TH in (SELECT RatingPk FROM @RatingQuotations)))
DELETE dbo.RateLines WHERE TL_TI in (SELECT TI_PK FROM dbo.RateEntry WHERE TI_TH in (SELECT RatingPk FROM @RatingQuotations))
DELETE dbo.RateOneOffContainers WHERE TC_TT in (SELECT TT_PK FROM dbo.RateOneOffShipment WHERE TT_TH in (SELECT RatingPk FROM @RatingQuotations))
DELETE dbo.RateOneOffPackLine WHERE TPL_TT_RateOneOffShipment in (SELECT TT_PK FROM dbo.RateOneOffShipment WHERE TT_TH in (SELECT RatingPk FROM @RatingQuotations))
DELETE dbo.RateOneOffShipment WHERE TT_TH in (SELECT RatingPk FROM @RatingQuotations)
DELETE dbo.RateAttachment WHERE TA_TH in (SELECT RatingPk FROM @RatingQuotations)
DELETE dbo.RateEntry WHERE TI_TH in (SELECT RatingPk FROM @RatingQuotations)
DELETE dbo.RatingHeader WHERE TH_PK in (SELECT RatingPk FROM @RatingQuotations)
";

		#endregion

		#region Quote Number Fountain

		const string QuoteNumberFountainScript = @"
-- Quote Number Fountain (StmNums)
DECLARE @QuoteNumberFountain_ToBeDeleted varchar(256); SET @QuoteNumberFountain_ToBeDeleted = 'QuoteNumber'
DELETE dbo.StmNums WHERE SN_Name = @QuoteNumberFountain_ToBeDeleted
";

		#endregion

		#endregion
	}
}
