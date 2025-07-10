using System.Linq;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;

namespace Enterprise.Customs.IT.NCTS.DataTransfer.Testing;

sealed class NctsHeaderDataObjectWriterTest : DataObjectWriterTest
{
	public void TestExportDeclarantTypeAndDeclarant()
	{
		nctsHeader.LocalReferenceNumber = "NCTD00050098";
		nctsHeader.RepresentationType = EU.Business.RepresentationTypeList.Codes._2Direct;
		nctsHeader.DeclarantAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
		var shipment = writer.GetDataObject(nctsHeader);
		AssertEquals("shipment.DeclarantType.Code", EU.Business.RepresentationTypeList.Codes._2Direct, shipment.DeclarantType.Code);
		AssertEquals("shipment.DeclarantType.Description", EU.Business.RepresentationTypeList.Descriptions._2Direct, shipment.DeclarantType.Description);

		var declarantOrgAddress = shipment.OrganizationAddressCollection.FirstOrDefault(AddressTypes.Declarant);
		AssertEquals("declarantOrgAddress.OrganizationCode", GlbCompany.CurrentCompany.OrgProxy.OH_Code, declarantOrgAddress.OrganizationCode);
	}

	public void TestGetNewMoveHeaderDataObjectWriter()
	{
		var writerForTest = new NctsHeaderDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, nctsHeader)));
		AssertType<NctsMoveHeaderDataObjectWriter>(writerForTest.GetNewMoveHeaderDataObjectWriterExposed(writerForTest.GetDataObject(nctsHeader)));
	}

	public void TestPopulateAuthorizationNumberCustomsReference()
	{
		nctsHeader.Authorization = "XYZ";
		var dataObject = writer.GetDataObject(nctsHeader);

		var authorizationNumberCusRefList = dataObject.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == "AUT").ToArray();
		AssertEquals("'AUT' CustomsReferenceCollection Length", 1, authorizationNumberCusRefList.Length);

		CombineAssertions(() =>
		{
			var authorizationNumberCusRef = authorizationNumberCusRefList[0];
			AssertEquals("Type.Description", "Authorization Number", authorizationNumberCusRef.Type.Description);
			AssertEquals("Reference", "XYZ", authorizationNumberCusRef.Reference);
		});
	}

	public void TestPopulateCustomsProfileIdentifier()
	{
		nctsHeader.BH_CustomsProfile = "999A";
		var dataObject = writer.GetDataObject(nctsHeader);
		AssertEquals("CustomsProfileIdentifier Type", "Node", dataObject.CustomsProfileIdentifier.Type);
		AssertEquals("CustomsProfileIdentifier Value", "999A", dataObject.CustomsProfileIdentifier.Value);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		writer = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, nctsHeader)));
	}

	NctsHeader nctsHeader;
	NctsHeaderDataObjectWriter writer;

	class NctsHeaderDataObjectWriterForTest : NctsHeaderDataObjectWriter
	{
		public NctsHeaderDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
		{
		}

		public EU.NCTS.DataTransfer.Phase4.NctsMoveHeaderDataObjectWriter GetNewMoveHeaderDataObjectWriterExposed(Shipment shipment) => GetNewMoveHeaderDataObjectWriter(shipment);
	}
}
