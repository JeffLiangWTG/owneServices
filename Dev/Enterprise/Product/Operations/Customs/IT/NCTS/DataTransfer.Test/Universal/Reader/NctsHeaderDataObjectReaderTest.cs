using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.DataTransfer.Testing;

sealed class NctsHeaderDataObjectReaderTest : OrganizationAddressTestHelper
{
	public void TestImportDeclarantTypeAndDeclarant()
	{
		var newFactory = new BusinessObjectFactory();
		var header = newFactory.New<NctsHeader>();
		header.BH_ApplicationCode = "NCT";
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.RepresentationType = EU.Business.RepresentationTypeList.Codes._3Indirect;
		header.DeclarantAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

		var writingManager = new DataWritingManager(new ActionInfo(null, header));
		var writer = new NctsHeaderDataObjectWriter(writingManager);
		var shipment = writer.GetDataObject(header);

		var reader = new NctsHeaderDataObjectReader(shipment, Logger, Factory);
		var headerBO = (NctsHeader)reader.ReadIntoBusinessObject();
		AssertEquals("headerBO.RepresentationType", EU.Business.RepresentationTypeList.Codes._3Indirect, headerBO.RepresentationType);
		AssertEquals("headerBO.DeclarantAddressPK", GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, headerBO.DeclarantAddressPK);
	}
}
