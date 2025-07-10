using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineCatchZoneValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var quarantineExDocHeader = invoiceHeader.QuarantineExDocHeader;

			quarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			var catchZones = quarantineExDocHeader.NexDocCatchZones.AddNew();
			catchZones.CY_Data = "test";
			AssertNoMessageErrors(catchZones.CY_DataInfo);

			quarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			catchZones = quarantineExDocHeader.NexDocCatchZones.AddNew();
			catchZones.CY_Data = "test";
			AssertHasMessageError(catchZones.CY_DataInfo, "Origin catch zone may only be present when produce type is fish.");
		}
	}
}
