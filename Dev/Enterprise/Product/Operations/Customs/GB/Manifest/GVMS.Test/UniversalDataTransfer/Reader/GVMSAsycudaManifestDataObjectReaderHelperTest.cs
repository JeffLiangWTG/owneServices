using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.GB.GVMS.UniversalDataTransfer.Testing
{
	class GVMASsycudaManifestDataObjectReaderHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportAsycudaManifestHeaderFields()
		{
			ASYCUDA.Business.Testing.ZZDataTestHelper.SetupZZ(new BusinessObjectFactory(), Core.Constants.CountryCodes.UnitedKingdom);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var usAirLocalPort1 = help.GetAirLocalPort1("GB");
			var sgAirLocalPort1 = help.GetAirLocalPort1(Core.Constants.CountryCodes.UnitedKingdom);
			var usFirstArrival = new UNLOCO() { Code = usAirLocalPort1.RL_Code };
			var sgFirstArrival = new UNLOCO() { Code = sgAirLocalPort1.RL_Code };
			var headerDataObject = help.SetupManifestHeader("MAST0006", usFirstArrival, sgFirstArrival, new ZDateTime(2018, 2, 10), new ZDateTime(2018, 2, 1), "", GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms);

			var headerEntryHeader = help.SetupCountryHeaderEntryHeader(Core.Constants.CountryCodes.UnitedKingdom, 1);
			var headerEntryInstruction = help.SetupCountryHeaderEntryInstruction(1, usFirstArrival, "OTT1", Core.Constants.CountryCodes.UnitedKingdom, GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms, "NT1");

			headerEntryInstruction.AddInfoCollection.Add(new AddInfo() { Key = AsycudaManifestHeader.Schema.EmptyVehicle, Value = "CON" });
			headerEntryInstruction.AddInfoCollection.Add(new AddInfo() { Key = AsycudaManifestHeader.Schema.RouteId, Value = "35" });
			headerEntryInstruction.AddInfoCollection.Add(new AddInfo() { Key = AsycudaManifestHeader.Schema.InspectionLocations, Value = "GB" });
			headerEntryInstruction.AddInfoCollection.Add(new AddInfo() { Key = AsycudaManifestHeader.Schema.IsUnaccompanied, Value = "Y" });

			headerDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			headerDataObject.EntryHeaderCollection.Add(headerEntryHeader);
			headerDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			headerDataObject.EntryInstructionCollection.Add(headerEntryInstruction);
			headerDataObject.SetLocationOfGoodsCollection(() => new List<LocationOfGoods>());
			headerDataObject.LocationOfGoodsCollection.Add(new LocationOfGoods
			{
				Type = LocationOfGoodsType.Inspection,
				SubType = new CodeDescriptionPair2Char() { Code = "1" },
				AdditionalIdentifier = "L0029A"
			});
			headerDataObject.LocationOfGoodsCollection.Add(new LocationOfGoods
			{
				Type = LocationOfGoodsType.Inspection,
				SubType = new CodeDescriptionPair2Char() { Code = "2" },
				AdditionalIdentifier = "L0030A"
			});

			var declarationCusSupportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)header;
			var declarationCusSupportingInfoTypes = declarationCusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes();

			var additionalInfo = (CusSupportingInfo)Factory.New(typeof(GvmsCustomsReference));
			additionalInfo.CSI_ParentID = header.PK;
			additionalInfo.CSI_ParentTableCode = header.TablePrefix;
			additionalInfo.CSI_ReferenceNumber = "BYE";

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
				ReferenceNumber = "HI1",
				Type = new CodeDescriptionPair6Char() { Code = GVMSCustomsReference.Codes.AtaCarnet }
			};
			var additionalInfoDataObject4 = new CustomsSupportingInformation()
			{
				Category = new CodeDescriptionPair() { Code = GvmsItemReference.GvmsItemReferenceType },
				Type = new CodeDescriptionPair6Char() { Code = GVMSCustomsReference.Codes.IndirectExportDeclarationEad },
				ReferenceNumber = "TEST EAD"
			};
			headerDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
				{
					additionalInfoDataObject1, additionalInfoDataObject2, additionalInfoDataObject3, additionalInfoDataObject4
				}));

			var reader = new ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectReader(headerDataObject, new UniversalDataBuss.Core.Testing.TestErrorLogger(), Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var headerBo = (AsycudaManifestHeader)readerHeaderBO;
			AssertNotNull(headerBo);

			AssertEquals("EmptyVehicle", "CON", headerBo.EmptyVehicle);
			AssertEquals("RouteId", "35", headerBo.RouteId);
			AssertEquals("IsUnaccompanied", true, headerBo.IsUnaccompanied);
			AssertEquals("GvmsCustomsReferenceCollection", 3, headerBo.GvmsCustomsReferenceCollection.Count);
			AssertEquals("GvmsTransitReferenceCollection", 1, headerBo.GvmsTransitReferenceCollection.Count);
			AssertEquals("GvmsEidrAndOralReferenceCollection", 0, headerBo.GvmsEidrAndOralReferenceCollection.Count);
			AssertEquals("InspectionLocations", 2, headerBo.InspectionLocations.Count);
		}
	}
}
