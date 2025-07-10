using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(G4PreviousDocument))]
	class G4PreviousDocumentTest : CusSupportingInfoTest<G4PreviousDocument>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("G4 Previous Document", previousDocument.HumanReadableName);
		}

		public void TestValidation()
		{
			AssertType<G4PreviousDocumentValidation>("G4 Validation expected", previousDocument.Validation);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("Default CSI_Type", "PRE", previousDocument.CSI_Type);
			AssertEquals("Default CSI_Code", "G4", previousDocument.CSI_Code);
			AssertEquals("Default CSI_ParentTableCode", "BM", previousDocument.CSI_ParentTableCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			previousDocument = nctsHeader.ArrivalMovementHeader.G4PreviousDocuments.AddNew();
		}
		G4PreviousDocument previousDocument;

		protected override IEnumerable<G4PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var previousDocument = nctsHeader.ArrivalMovementHeader.G4PreviousDocuments.AddNew();
			yield return previousDocument;
		}

		protected override BusinessObject GetNewBusinessObject() => previousDocument;
	}
}
