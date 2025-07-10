using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DataTransfer.Testing;

[TestedType(typeof(DeclarationDataObjectReader))]
sealed class DeclarationDataObjectReaderTest : DataObjectReaderTest
{
	[ExpectNoExceptions]
	public void TestInvoiceHeaderDataObjectReader()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_MessageSubType = "CUS";
		declaration.JE_ApplicationCode = "BLT";
		var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
		var reader = new DeclarationDataObjectReaderForTest(declarationData, logger, Factory);
		NUnit.Framework.Assert.That(reader.GetInvoiceHeaderDataObjectReader(declaration.JobComInvoiceGroupHeaders[0], new CommercialInvoiceHeader(), declarationData, null), Is.TypeOf<CommercialInvoiceHeaderDataObjectReader>());
	}

	[ExpectNoExceptions]
	public void TestEntryInstructionDataObjectReader()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_MessageSubType = "CUS";
		declaration.JE_ApplicationCode = "BLT";
		var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
		var reader = new DeclarationDataObjectReaderForTest(declarationData, logger, Factory);
		NUnit.Framework.Assert.That(reader.GetCustomsEntryInstructionDataObjectReader(new EntryInstruction(), declaration), Is.TypeOf<CustomsEntryInstructionDataObjectReader>());
	}

	Shipment SetupDeclaration(ZString messageType, ZString messageSubType)
	{
		var dataContext = DataContextFactory.New();
		dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
		dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
		return new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
		{
			DataContext = dataContext,
			MessageType = new CodeDescriptionPair()
			{ Code = messageType },
			MessageSubType = new CodeDescriptionPair()
			{ Code = messageSubType }
		};
	}

	class DeclarationDataObjectReaderForTest : DeclarationDataObjectReader
	{
		internal DeclarationDataObjectReaderForTest(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null) : base(declarationDataObject, logger, factory, shipment)
		{
		}

		public CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> GetInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return base.CreateNewCommercialInvoiceHeaderDataObjectReader(groupHeader, invoiceData, dataObject, landedCostDataReader);
		}

		public Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader GetCustomsEntryInstructionDataObjectReader(EntryInstruction dataObject, JobDeclaration declaration)
		{
			return base.CreateCustomsEntryInstructionDataObjectReader(dataObject, declaration);
		}
	}
}
