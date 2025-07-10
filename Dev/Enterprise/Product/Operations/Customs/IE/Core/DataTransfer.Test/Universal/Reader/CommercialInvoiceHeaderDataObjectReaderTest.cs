using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.DataTransfer.Universal.Testing
{
	sealed class CommercialInvoiceHeaderDataObjectReaderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestIsMainPack()
		{
			var shipment = CreateShipment();
			shipment.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>()
								{
									new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
									{
										AddInfoCollection = new System.Collections.Generic.List<AddInfo>()
										{
											new AddInfo()
											{
												Key = EUAddInfoSchema.Constants.ZG_IsMainPack.Substring(3),
												Value = ZBool.True.ToString()
											}
										}
									}
								}))
						}
			};

			var declaration = GetDeclarationFromShipment(shipment);
			var invoice = declaration.Invoices[0];
			var invoiceLine = invoice.InvoiceLines[0];
			AssertEquals("IsMainPack", ZBool.True, invoiceLine.ZG_IsMainPack);
		}

		Shipment CreateShipment()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			return new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "MB123",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
			};
		}

		JobDeclaration GetDeclarationFromShipment(Shipment shipment)
		{
			return (JobDeclaration)new EU.DataTransfer.Universal.JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject();
		}
	}
}
