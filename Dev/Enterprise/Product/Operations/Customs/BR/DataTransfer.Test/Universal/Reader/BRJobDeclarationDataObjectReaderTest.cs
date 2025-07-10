using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	public class BRJobDeclarationDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestInvoiceHeaderDataObjectReader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			declaration.JE_ApplicationCode = "BLT";

			var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
			var reader = new BRJobDeclarationDataObjectReaderForTest(declarationData, logger, Factory);
			AssertType<BRInvoiceHeaderDataObjectReader>(reader.GetInvoiceHeaderDataObjectReader(declaration.JobComInvoiceGroupHeaders[0], new CommercialInvoiceHeader(), declarationData, null));
		}

		Shipment SetupDeclaration(ZString messageType, ZString messageSubType)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			return new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair() { Code = messageType },
				MessageSubType = new CodeDescriptionPair() { Code = messageSubType }
			};
		}

		class BRJobDeclarationDataObjectReaderForTest : BRJobDeclarationDataObjectReader
		{
			internal BRJobDeclarationDataObjectReaderForTest(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null) : base(declarationDataObject, logger, factory, shipment)
			{
			}

			public CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader> GetInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
			{
				return base.CreateNewCommercialInvoiceHeaderDataObjectReader(groupHeader, invoiceData, dataObject, landedCostDataReader);
			}
		}
	}
}
