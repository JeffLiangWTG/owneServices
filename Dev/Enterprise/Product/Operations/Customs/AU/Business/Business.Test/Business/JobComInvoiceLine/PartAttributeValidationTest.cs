using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class PartAttributeValidationTest : MasterFiles.Business.Testing.PartAttributeValidationTest
	{
		public void TestCheckVinCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = orgHeader.PK;
			declaration.Importer.OH_IsConsignee = true;
			declaration.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;

			AUOrgSupplierPart part = Factory.New<AUOrgSupplierPart>();
			invoiceLine.SetPartForTesting(part);

			invoiceLine.AddInfo.ZA_VID = "FromAddInfo";
			invoiceLine.JI_PartAttrib1 = "FromPartAttrib1";
			AssertHasMessageError(invoiceLine.JI_PartAttrib1Info, "Only the VIN in Additional Info will be sent in the message.");

			invoiceLine.AddInfo.ZA_VID = "";
			invoiceLine.JI_PartAttrib1 = "FromPartAttrib2";
			AssertNoMessageError(invoiceLine.JI_PartAttrib1Info, "Only the VIN in Additional Info will be sent in the message.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddInfo.ZA_VID = "FromAddInfo";
			invoiceLine.JI_PartAttrib1 = "FromPartAttrib1";
			AssertNoMessageError(invoiceLine.JI_PartAttrib1Info, "Only the VIN in Additional Info will be sent in the message.");

			invoiceLine.AddInfo.ZA_VID = "";
			invoiceLine.JI_PartAttrib1 = "FromPartAttrib2";
			AssertNoMessageError(invoiceLine.JI_PartAttrib1Info, "Only the VIN in Additional Info will be sent in the message.");
		}
	}
}
