using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCompetitorsByType))]
	public class DocCompetitorsByTypeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocCompetitorsByType_TypeCodeAndDescription()
		{
			var docCompetitorsByType1 = new DocCompetitorsByType();
			var docCompetitorsByType2 = new DocCompetitorsByType();
			docCompetitorsByType1.Type = CompetitorTypeList.Codes.Forwarding;
			docCompetitorsByType2.Type = "TST";

			Factory.Save();

			AssertEquals("Type is set", CompetitorTypeList.Codes.Forwarding, docCompetitorsByType1.Type);
			AssertEquals("Type is set", "TST", docCompetitorsByType2.Type);
			AssertEquals("Types in registry have full description", "CMF - Forwarding", docCompetitorsByType1.TypeCodeAndDescription);
			AssertEquals("Types not in registry have only code", "TST", docCompetitorsByType2.TypeCodeAndDescription);
		}

		public void TestDocCompetitorsByType_Competitors()
		{
			var docCompetitorsByType = new DocCompetitorsByType();
			AssertNotNull(docCompetitorsByType.Competitors);
			AssertType(typeof(DocCompetitorCollection), docCompetitorsByType.Competitors);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocCompetitorsByType();
		}

		#endregion
	}
}
