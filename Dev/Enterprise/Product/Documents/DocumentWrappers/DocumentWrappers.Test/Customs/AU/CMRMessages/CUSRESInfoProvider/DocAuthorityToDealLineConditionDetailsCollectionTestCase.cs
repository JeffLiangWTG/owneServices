using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocAuthorityToDealLineConditionDetailsCollection))]
	sealed class DocAuthorityToDealLineConditionDetailsCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<DocAuthorityToDealLineConditionDetailsCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			SegmentGroup13 group13 = new CUSRESMessage().Group6.InstantiateAChildAndAddItToChildrenCollection().Group13.InstantiateAChildAndAddItToChildrenCollection();
			AuthorityToDealLineConditionDetails line = new AuthorityToDealLineConditionDetails(group13);
			return DocAuthorityToDealLineConditionDetails.New(line, Factory);
		}

		protected override DocAuthorityToDealLineConditionDetailsCollection GetCollectionToTest()
		{
			return new DocAuthorityToDealLineConditionDetailsCollection(Factory);
		}
	}
}
