using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5.Testing
{
	abstract class NctsMovementHeaderDataObjectWriter<T> : DataTransfer.Testing.NctsHeaderCommonDataObjectWriterTest<T>
		where T : NctsMovementHeaderDataObjectWriter
	{
		protected sealed override void AssertAdditionalData(Shipment shipment)
		{
			AssertNull("shipment.ShipmentType", shipment.ShipmentType);
			AssertAdditionalDataCore(shipment);
		}

		protected abstract void AssertAdditionalDataCore(Shipment shipment);
		protected abstract ZString MovementType { get; }

		protected sealed override void SetupAdditionalData(NctsHeader header) => SetupAdditionalDataCore(header);
		protected abstract void SetupAdditionalDataCore(NctsHeader header);

		static protected void AssertOffice(string message, CustomsReference customsReference, ZString officeCode, ZString officePurpose, ZString officePurposeDesc, ZDateTime submittedDateToCustoms, ZString officeDescription)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("customsReference.Type.Code", NctsUxmlTypeList.Codes.OfficeCode, customsReference.Type.Code);
				AssertEquals("customsReference.Type.Description", NctsUxmlTypeList.Descriptions.OfficeCode, customsReference.Type.Description);
				AssertEquals("customsReference.SubType.Code", officePurpose, customsReference.SubType.Code);
				AssertEquals("customsReference.SubType.Description", officePurposeDesc, customsReference.SubType.Description);
				AssertEquals("customsReference.Reference", officeCode, customsReference.Reference);
				AssertEquals("customsReference.ReferencedEntityDescription", officeDescription, customsReference.ReferencedEntityDescription);
			});
		}

		protected OrgHeader OrgINTHEMSYD => orgINTHEMSYD ?? (orgINTHEMSYD = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(Factory));
		OrgHeader orgINTHEMSYD;

		protected OrgHeader OrgWUFSHIJNB => orgWUFSHIJNB ?? (orgWUFSHIJNB = OrganizationAddressTestHelper.GetOrganizationBO_WUFSHIJNB(Factory));
		OrgHeader orgWUFSHIJNB;

		protected override NctsHeader GetNewHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(MovementType);
			return header;
		}
	}
}
