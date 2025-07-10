using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using Vehicle = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Vehicle;

namespace Enterprise.Customs.CH.DataTransfer.Testing;

partial class CombinedDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
{
	public void TestImportVehicles()
	{
		var dataContext = DataContextFactory.New();
		dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		dataContext.CodesMappedToTarget = true;
		dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
		var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
		{
			DataContext = dataContext,
			WayBillNumber = "MB123",
			WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
			CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
						{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>  new DataObjectList<CommercialInvoiceLine>()
							{
								new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									VehicleCollection = new List<Vehicle>()
									{
										new Vehicle() { VIN = "VIN1", RegistrationNumber = "REGNUM1", Model = "ML1" },
										new Vehicle() { VIN = "VIN2", RegistrationNumber = "REGNUM2", Model = "ML2" },
									}
								}
							}))
						}
			}
		};

		var reader = new DeclarationDataObjectReader(universalShipment, new TestErrorLogger(), Factory);
		var declarationBO = reader.ReadIntoBusinessObject();
		var line = declarationBO.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault();

		var query = new ZQuery(CusVehicleSchema.CVH_ParentID, line.PK);
		var existingVehicles = Factory.Load<Customs.Business.CusVehicle>(query);

		CombineAssertions(() =>
		{
			AssertEquals("# of vehicles", 2, existingVehicles.Length);

			var vehicle = existingVehicles[0];
			AssertEquals("Vehicle 1, VIN", "VIN1", vehicle.CVH_VehicleIdentificationNumber);
			AssertEquals("Vehicle 1, RegistrationNumber", "REGNUM1", vehicle.CVH_RegistrationNumber);
			AssertEquals("Vehicle 1, Model", "ML1", vehicle.CVH_ModelName);
			AssertEquals("Vehicle 2, VIN", "VIN2", existingVehicles[1].CVH_VehicleIdentificationNumber);
		});
	}
}

