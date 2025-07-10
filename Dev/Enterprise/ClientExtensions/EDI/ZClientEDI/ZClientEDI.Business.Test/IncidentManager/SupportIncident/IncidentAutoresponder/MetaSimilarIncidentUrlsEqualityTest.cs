using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;
using ZClientEDI.Business.IncidentManager.SupportIncident.IncidentAutoresponder;

namespace Enterprise.Client.EDI.IncidentManager.Test
{
	class MetaSimilarIncidentUrlsEqualityTest : TestCase
	{
		public void TestMetaSimilarIncidentUrlsEquality()
		{
			// Arrange
			var urls = new List<string> { "http://example.org" };

			var similarIncidentUrls_Null = null as MetaSimilarIncidentUrls;

			var similarIncidentUrls_EmptyUrls = new MetaSimilarIncidentUrls(ZGuid.Empty, 0.75m);
			similarIncidentUrls_EmptyUrls.Urls = new List<string>();

			var similarIncidentUrls1 = new MetaSimilarIncidentUrls(ZGuid.Empty, 0.5m);
			similarIncidentUrls1.Urls = urls;

			var similarIncidentUrls2 = new MetaSimilarIncidentUrls(ZGuid.Empty, 0.5m);
			similarIncidentUrls2.Urls = urls;
			// Act
			// Assert
			Assert(similarIncidentUrls_EmptyUrls.Equals(similarIncidentUrls_EmptyUrls));
			Assert(!similarIncidentUrls_EmptyUrls.Equals(similarIncidentUrls_Null));
			Assert(!similarIncidentUrls_EmptyUrls.Equals(similarIncidentUrls1));
			Assert(similarIncidentUrls1.Equals(similarIncidentUrls2));
		}
	}
}