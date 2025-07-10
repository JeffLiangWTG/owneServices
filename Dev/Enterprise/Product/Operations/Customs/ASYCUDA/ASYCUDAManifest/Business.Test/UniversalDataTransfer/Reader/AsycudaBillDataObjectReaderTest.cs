using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer.Testing
{
	public class AsycudaBillDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestImportingAsycudaBillExportGeneralManifestData_Bangladesh()
		{
			var billObject = SetupShipment();
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Bangladesh, Factory.BOFactory);
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Bangladesh;
			header.AMA_Nature = "IMP";

			var billBO = new AsycudaBillDataObjectReader(billObject, new TestErrorLogger(), Factory, header, readerHelper, false).ReadIntoBusinessObject() as AsycudaBill;
			Factory.SaveForTesting();

			AssertEGMFileds("EGM not show for Import", billBO, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);

			header.AMA_Nature = "EXP";
			billBO = new AsycudaBillDataObjectReader(billObject, new TestErrorLogger(), Factory, header, readerHelper, false).ReadIntoBusinessObject() as AsycudaBill;
			Factory.SaveForTesting();
			AssertEGMFileds("EGM only show for Export and Bangladesh", billBO, "100", "EN0001", "C", new ZDateTime(2023, 5, 23));
		}

		public void TestImportingAsycudaBillExportGeneralManifestData_OtherCountry()
		{
			var billObject = SetupShipment();
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Namibia, Factory.BOFactory);
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Namibia;
			header.AMA_Nature = "EXP";
			var billBO = new AsycudaBillDataObjectReader(billObject, new TestErrorLogger(), Factory, header, readerHelper, false).ReadIntoBusinessObject() as AsycudaBill;
			Factory.SaveForTesting();
			AssertEGMFileds("EGM not show for country other than Bangladesh", billBO, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
		}

		Shipment SetupShipment()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.AsycudaBill, null);
			var billObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "W123",
				WayBillType = new WayBillType { Code = "HWB" },
			};
			billObject.SetEntryNumberCollection(() => new List<EntryNumber>()
			{
				new EntryNumber()
				{
					Type =
						new EntryType()
						{
							Code = Constants.CustomsEntryType.SAD
						},
					Number = "EN0001",
					Category = "C",
					EntryLineReference = "100",
					IssueDate = new ZDateTime(2023, 5, 23)
				}
			});

			return billObject;
		}

		void AssertEGMFileds(string message, AsycudaBill bill, ZString code, ZString number, ZString serial, ZDateTime date)
		{
			var factory = new BusinessObjectFactory();
			var billBO = factory.Load<AsycudaBill>(bill.PK);
			AssertEquals(message + "billBO.SADOfficeCode", code, billBO.SADOfficeCode);
			AssertEquals(message + "billBO.SADRegistrationNumber", number, billBO.SADRegistrationNumber);
			AssertEquals(message + "billBO.SADRegistrationSerial", serial, billBO.SADRegistrationSerial);
			AssertEquals(message + "billBO.SADOfficeCode", date, billBO.SADRegistrationDate);
		}
	}
}
