using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.GB.DataTransfer.Universal;
using Enterprise.Customs.GB.GVMS;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.DataTransfer.Test.Universal
{
	public class GBGVMSAsycudaManifestHeaderDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestReaderDataObject()
		{
			var helper = new GVMSAsycudaManifestDataObjectReaderTestHelper();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("GVMS", "GB", ZDateTime.Today, true))
			{
				var existingHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var existingHeaderPK = existingHeader.PK;
				Factory.SaveForTesting();

				var usAirLocalPort1 = helper.GetAirLocalPort1("GB");
				var sgAirLocalPort1 = helper.GetAirLocalPort1(Core.Constants.CountryCodes.UnitedKingdom);
				var portOfLoading = new UNLOCO() { Code = usAirLocalPort1.RL_Code };
				var portOfDischarge = new UNLOCO() { Code = sgAirLocalPort1.RL_Code };

				var universalShipment = helper.SetupManifestHeader("MAST0006", portOfLoading, portOfDischarge, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(3), existingHeader.AMA_JobReference, GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms);

				var additionalInfoDataObject1 = new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = GvmsItemReference.GvmsItemReferenceType },
					ReferenceNumber = "HI2",
					Type = new CodeDescriptionPair6Char() { Code = GVMSCustomsReference.Codes.AtaCarnet }
				};
				var additionalInfoDataObject2 = new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = GvmsItemReference.GvmsItemReferenceType },
					Type = new CodeDescriptionPair6Char() { Code = GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber }
				};
				var additionalInfoDataObject3 = new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = GvmsItemReference.GvmsItemReferenceType },
					ReferenceNumber = "HI1"
				};
				var additionalInfoDataObject4 = new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = GvmsItemReference.GvmsItemReferenceType },
					Type = new CodeDescriptionPair6Char() { Code = GVMSCustomsReference.Codes.IndirectExportDeclarationEad },
					ReferenceNumber = "TEST EAD"
				};
				universalShipment.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
					{
						additionalInfoDataObject1, additionalInfoDataObject2, additionalInfoDataObject3, additionalInfoDataObject4
					}));

				Factory.SaveForTesting();
				var reader = new GBGVMSAsycudaManifestHeaderDataObjectReader(universalShipment, Logger, Factory);
				var readerHeaderBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var factory = new BusinessObjectFactory();
				var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);
				AssertEquals("Header is updated", existingHeaderPK, headerBO.PK);
			}
		}
	}

	class GVMSAsycudaManifestDataObjectReaderTestHelper : AsycudaManifestDataObjectReaderTestHelper
	{
		protected override DataContextType HeaderDataContextType => DataContextType.GvmsAsycudaManifestHeader;
	}
}

