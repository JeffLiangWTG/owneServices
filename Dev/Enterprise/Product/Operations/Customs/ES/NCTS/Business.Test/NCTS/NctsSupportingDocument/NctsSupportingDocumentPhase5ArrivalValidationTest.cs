using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsSupportingDocumentPhase5ArrivalValidation))]
	sealed class NctsSupportingDocumentPhase5ArrivalValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ItemNumber()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var supportingDoc = nctsHeader.Bills.AddNew().SupportingDocuments.AddNew();
			supportingDoc.CSI_ItemNumber = 0;
			AssertNoMessageErrors("no mandatory validation", supportingDoc.CSI_ItemNumberInfo);
		}
	}
}
