using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
{
	public class WarehouseCustomsLineDetailsProviderTest : TestCaseWithFactory
	{
		public void TestWarehouseCustomsLineDetailsType()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext
				{
					Company = new Company
					{
						Code = Core.Constants.CountryCodes.France
					}
				},
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>
							{
								new CommercialInvoiceLine
								{
									BondedWarehouseQuantity = 1
								}
							}))
					}
				}
			};
			var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(shipment);
			var lineDetails = provider.GetLineDetails();
			AssertType<WarehouseCustomsLineDetails>(lineDetails.Single());
		}
	}
}
