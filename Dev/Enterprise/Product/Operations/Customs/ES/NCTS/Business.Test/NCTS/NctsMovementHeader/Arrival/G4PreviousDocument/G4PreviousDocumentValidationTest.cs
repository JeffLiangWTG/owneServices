using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class G4PreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_SubType()
		{
			previousDocument.CSI_SubType = ZString.Empty;
			AssertNoNotifications(previousDocument.CSI_SubTypeInfo);

			previousDocument.CSI_SubType = "AH";
			AssertNoNotifications(previousDocument.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_Code()
		{
			previousDocument.CSI_Code = ZString.Empty;
			AssertNoNotifications(previousDocument.CSI_CodeInfo);

			previousDocument.CSI_SubType = "AH";
			AssertNoNotifications(previousDocument.CSI_CodeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			previousDocument = nctsHeader.ArrivalMovementHeader.G4PreviousDocuments.AddNew();
		}
		G4PreviousDocument previousDocument;
	}
}
