using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public class ResultCountMessage
	{
		public ResultCountMessage(IResultCountHandler resultCountHandler, int maxRowsToLoad, int maxRecommendedRowsToLoad)
		{
			this.resultCountHandler = resultCountHandler;
			this.maxRowsToLoad = maxRowsToLoad;
			this.maxRecommendedRowsToLoad = maxRecommendedRowsToLoad;
		}
		readonly IResultCountHandler resultCountHandler;
		readonly int maxRowsToLoad;
		readonly int maxRecommendedRowsToLoad;

		public void UpdateResultCountMessage(int resultCount, string limitedRunMessage = null)
		{
			if (resultCount > maxRowsToLoad)
			{
				var message = GetTooManyResultsErrorMessage(resultCount);
				resultCountHandler.UpdateNumberLoadedMessage(message, resultCount, true);
			}
			else if (resultCount > maxRecommendedRowsToLoad)
			{
				var message1 = Res.GetString("FilterModule|FoundGRecords", "Found {0:G} records (a large number of results). In future, please fill in more of the search screen", resultCount);
				resultCountHandler.UpdateNumberLoadedMessage(message1, resultCount, false);
			}
			else if (resultCount == 0)
			{
				var message = string.IsNullOrEmpty(limitedRunMessage)
					? Res.GetString("FilterModule|ThereAreNoRecords", "There are no records that match your search.")
					: limitedRunMessage;
				resultCountHandler.UpdateNumberLoadedMessage(message, resultCount, true);
			}
			else if (resultCount == 1)
			{
				var message = Res.GetString("FilterModule|Found1Record", "Found 1 record that matches your search criteria.");
				resultCountHandler.UpdateNumberLoadedMessage(message, resultCount, false);
			}
			else
			{
				var message = Res.GetString("FilterModule|FoundGRecordsThatMatchYourCriteria", "Found {0:G} records that match your search criteria.", resultCount);
				resultCountHandler.UpdateNumberLoadedMessage(message, resultCount, false);
			}
		}

		protected virtual string GetTooManyResultsErrorMessage(int numberResults)
		{
			if (EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult)
			{
				return Res.GetString("FilterModule|TooManyRecordsToDisplay", "Too many records to display ({0:G}). Please fill in more of the search screen and then click 'Find'.", numberResults);
			}
			else
			{
				return Res.GetString("FilterModule|MoreThanGRecordsWereReturned", "This search returns more than the maximum number of records to display.\r\nThe number of search records to display can be defined in the registry up to a maximum value of 15,000 records.\r\n\r\nSee: Registry > Physical Server > Display Grid > Max No. of Records to Show\r\n\r\nThe current value is set to {0:G}.", maxRowsToLoad);
			}
		}
	}
}
