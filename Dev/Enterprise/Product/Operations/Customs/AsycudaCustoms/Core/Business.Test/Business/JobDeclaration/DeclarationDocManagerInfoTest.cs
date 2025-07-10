using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(DeclarationDocManagerInfo))]
	class DeclarationDocManagerInfoTest : Customs.Business.Testing.DeclarationDocManagerInfoTest
	{
		public new void TestRelatedObjectTypesAlwaysShow()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				Assert("should not always show related object edocs", !declaration.DocManagerInfo.RelatedObjectTypesAlwaysShow(null));
				Assert("should always show CusInBondMoveHeader related object edocs", declaration.DocManagerInfo.RelatedObjectTypesAlwaysShow(Factory.New<CusInBondMoveHeader>()));
			});
		}

		public void TestRelatedObjectsCusInBondMoveHeaderRetrieved()
		{
			var jobDeclaration = (JobDeclaration)base.GetPopulatedParentBusinessObject();
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var cusInBondMoveHeader = entryInstruction.CusInBondPermitsHeaders.AddNew();
			Factory.Save();
			var relatedObjects = jobDeclaration.DocManagerInfo.RelatedObjects;
			CombineAssertions(() =>
			{
				Assert("cusInBondMoveHeader added", relatedObjects.Contains(cusInBondMoveHeader));
				AssertNotNull("DocManagerInfo of cusInBondMoveHeader", cusInBondMoveHeader.DocManagerInfo);
			});
		}
	}
}
