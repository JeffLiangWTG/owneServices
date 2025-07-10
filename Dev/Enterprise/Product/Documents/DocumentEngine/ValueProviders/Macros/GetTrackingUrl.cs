using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GetTrackingUrl : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			var expectedUrl = TrackingUrlCreator.Instance.CreateUrl(new ZGuid("CBEB9589-A539-4E82-BBDE-33ABD05331A4"), TrackingConstants.BusinessContext.Booking, new ZGuid("6E449683-C509-11CF-AAFA-00AA00B6015C"));

			return new ValueProviderDocumenter("<GetTrackingUrl({recipientcontactpk},{webtrackingobjecttype},{webtrackingobjectpk})>",
				ResString.GetMultilingualString("37b667bb-30c8-41fc-ae86-9a16ff2c460b",
				@"Returns a clickable link (URL) that will allow the consumer of the resulting document to click through to the Web Tracking page for the Tracked Item indicated by the Tracking Object Type and Tracking Object PK specified. 
The link will work on any machine that is connected to the Internet and will pass them through to the appropriate page on your Web Tracking website. 
If you want a link for internal use navigating directly into {0} itself, try using the {1} macro instead.",
				"CargoWise", "GetShowEditFormUrl"),
				new List<(string example, object expectedResult)> { ("<GetTrackingUrl(<Contact.OC_PK>, Booking, <Booking.EB_PK>)>", expectedUrl) });
		}

		#region GetReplacement
		protected override object GetReplacementCore(string macro, Report report)
		{
			string result = "";

			string contactPKAsString = Regex.Match(macro).Groups["RecipientContactPK"].ToString().Trim();
			string businessContextAsString = Regex.Match(macro).Groups["TrackingBusinessContext"].ToString().Trim();
			string businessContextIDAsString = Regex.Match(macro).Groups["TrackingBusinessContextPK"].ToString().Trim();

			try
			{
				if (!string.IsNullOrEmpty(contactPKAsString) && !string.IsNullOrEmpty(businessContextAsString) && !string.IsNullOrEmpty(businessContextIDAsString))
				{
					ZGuid contactPK = GetContactPK(contactPKAsString);
					TrackingConstants.BusinessContext businessContext = GetBusinessContext(businessContextAsString);

					if (IsValidGuid(businessContextIDAsString))
					{
						ZGuid businessContextPK = GetBusinessContextPK(businessContextIDAsString);
						result = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextPK);
					}
					else
					{
						result = TrackingUrlCreator.Instance.CreateUrl(contactPK, businessContext, businessContextIDAsString);
					}
				}
			}
			catch (ArgumentException ex)
			{
				ReportMacroError(report, ex.Message);
			}
			return result;
		}

		ZGuid GetContactPK(string contactPKAsString)
		{
			try
			{
				return new ZGuid(contactPKAsString);
			}
			catch (FormatException ex)
			{
				throw new ArgumentException("Invalid ContactPK '" + contactPKAsString + "'; " + ex.Message, ex);
			}
		}

		TrackingConstants.BusinessContext GetBusinessContext(string businessContextAsString)
		{
			try
			{
				return (TrackingConstants.BusinessContext)Enum.Parse(typeof(TrackingConstants.BusinessContext), businessContextAsString, true);
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException("Invalid TrackingBusinessContext type '" + businessContextAsString + "'; " + ex.Message, ex);
			}
		}

		ZGuid GetBusinessContextPK(string businessContextIDAsString)
		{
			try
			{
				return new ZGuid(businessContextIDAsString);
			}
			catch (FormatException ex)
			{
				throw new ArgumentException("Invalid TrackingBusinessContextPK '" + businessContextIDAsString + "'; " + ex.Message, ex);
			}
		}

		bool IsValidGuid(string guidAsString)
		{
			bool result = false;

			if (!string.IsNullOrEmpty(guidAsString))
			{
				try
				{
					result = new ZGuid(guidAsString).IsValid;
				}
				catch
				{
				}
			}

			return result;
		}

		#endregion

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)GetTrackingUrl(?:[\s]*)\((?:[\s]*)(?<RecipientContactPK>.*)(?:[\s]*),(?:[\s]*)(?<TrackingBusinessContext>[^\s]+)(?:[\s]*),(?:[\s]*)(?<TrackingBusinessContextPK>.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
