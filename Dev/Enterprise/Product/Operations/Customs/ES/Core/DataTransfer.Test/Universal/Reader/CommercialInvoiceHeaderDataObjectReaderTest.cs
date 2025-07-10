using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using CusVehicle = Enterprise.Customs.Business.CusVehicle;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using Vehicle = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Vehicle;

namespace Enterprise.Customs.ES.DataTransfer.Universal.Testing
{
	class CommercialInvoiceHeaderDataObjectReaderTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestFillInvoiceLineJI_StateOrRegionOfOriginFromAddInfo()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceHeaderDataObject.InvoiceNumber = "INVABC123";
			invoiceHeaderDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoiceHeaderDataObject.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.AddInfoCollection = new List<UniversalAddInfo>();
			var addInfo = new UniversalAddInfo()
			{
				Key = "ProvinceOfOrigin",
				Value = "28"
			};
			invoiceLine.AddInfoCollection.Add(addInfo);

			var helper = new UniversalDataObjectReaderHelper(Factory, CurrentCompany.GC_RN_NKCountryCode, CurrentCompany.GC_RN_NKCountryCode);

			var reader = new CommercialInvoiceHeaderDataObjectReader(invoiceHeaderDataObject, logger, helper, declaration.TopGroupInvoice);
			var invoiceBO = reader.ReadIntoBusinessObject();
			var invoiceLineBO = invoiceBO.JobComInvoiceLines[0];

			AssertEquals("JI_StateOrRegionOfOrigin Exists", "28", invoiceLineBO.JI_StateOrRegionOfOrigin);
		}

		public void TestImportVehicles()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
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
										new Vehicle() { VIN = "VIN1", Brand = "BRAND1", Model = "MODEL1" },
										new Vehicle() { VIN = "VIN2", Brand = "BRAND2", Model = "MODEL2" },
									}
								}
							}))
						}
					}
				};

				var reader = new JobDeclarationDataObjectReader(universalShipment, new TestErrorLogger(), Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var line = declarationBO.InvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault();

				var query = new ZQuery(CusVehicleSchema.CVH_ParentID, line.PK);
				var existingVehicles = Factory.Load<CusVehicle>(query);

				CombineAssertions(() =>
				{
					AssertEquals("# of vehicles", 2, existingVehicles.Length);

					var vehicle = existingVehicles[0];
					AssertEquals("Vehicle 1, VIN", "VIN1", vehicle.CVH_VehicleIdentificationNumber);
					AssertEquals("Vehicle 1, Brand", "BRAND1", vehicle.CVH_BrandName);
					AssertEquals("Vehicle 1, Model", "MODEL1", vehicle.CVH_ModelName);
					AssertEquals("Vehicle 2, VIN", "VIN2", existingVehicles[1].CVH_VehicleIdentificationNumber);
				});
			}
		}
	}
}
