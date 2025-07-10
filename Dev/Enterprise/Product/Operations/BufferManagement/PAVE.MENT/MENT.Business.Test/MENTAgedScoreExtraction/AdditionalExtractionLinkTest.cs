using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(AdditionalExtractionLink))]
	class AdditionalExtractionLinkTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDoesNotStoreQueryPK()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Clyde");
			var query = extraction.RelatedQuery;

			var anotherExtraction = MENTTestHelper.CreateExtraction(Factory, "Billings", query);

			var link = extraction.AdditionalExtractions.AddNew();

			AssertEquals(ZGuid.Empty, link.QueryPK);
			AssertEquals(ZGuid.Empty, link.ExtractionPK);

			link.QueryPK = query.PK;

			AssertEquals(query.PK, link.QueryPK);
			AssertEquals(2, link.Extractions.Count);

			link.ExtractionPK = anotherExtraction.PK;
			AssertEquals(query.PK, link.QueryPK);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedExtraction = newFactory.Load<MENTAgedScoreExtraction>(extraction.PK);

			var loadedLink = (AdditionalExtractionLink)loadedExtraction.AdditionalExtractions.First();
			AssertEquals(loadedExtraction.RelatedQuery.PK, loadedLink.QueryPK);
			AssertEquals(anotherExtraction.PK, loadedLink.ExtractionPK);
		}

		public void TestReturnExtractions()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Clyde");
			var query = extraction.RelatedQuery;

			var anotherExtraction = MENTTestHelper.CreateExtraction(Factory, "Billings", query);

			var link = extraction.AdditionalExtractions.AddNew();
			link.ExtractionPK = anotherExtraction.PK;
			link.QueryPK = query.PK;

			AssertEquals(2, link.Extractions.Count);

			link.ExtractionPK = ZGuid.Empty;
			link.QueryPK = ZGuid.Empty;

			AssertEquals(0, link.Extractions.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AdditionalExtractionLink(Factory, MENTTestHelper.CreateExtraction(Factory, "Pruple"));
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "ExtractionPK";
			}
		}

		#endregion
	}
}
