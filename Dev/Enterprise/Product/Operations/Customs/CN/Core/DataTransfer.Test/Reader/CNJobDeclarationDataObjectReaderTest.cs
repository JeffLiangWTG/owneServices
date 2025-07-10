using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNJobDeclarationDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestFillOrganizationsCore()
		{
			var buyerDocOrgHeader = CreateOrganisation("Buyer 1", "BUY1");
			var buyerOrgHeader = CreateOrganisation("Buyer 2", "BUY2");
			var manufacturer = CreateOrganisation("Manufacturer 1", "MFC1");

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				declaration.JE_ApplicationCode = "BLT";

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
				declarationData.AddOrgAddress(writeManager, buyerDocOrgHeader, "BuyerDocumentaryAddress");
				declarationData.AddOrgAddress(writeManager, manufacturer, "Manufacturer");

				var reader = new CNJobDeclarationDataObjectReader(declarationData, logger, Factory);
				var result = reader.ReadIntoBusinessObject();

				AssertEquals("BuyerDocumentaryAddress", buyerDocOrgHeader.PK, result.BuyerDocAddress.OrganisationPK);
				AssertEquals("Buyer", buyerDocOrgHeader.PK, result.JE_OH_Buyer);
				AssertEquals("Manufacturer", manufacturer.PK, result.JE_OH_Manufacturer);

				declarationData.AddOrgAddress(writeManager, buyerOrgHeader, "Buyer");
				result = reader.ReadIntoBusinessObject();

				AssertEquals("BuyerDocumentaryAddress", buyerDocOrgHeader.PK, result.BuyerDocAddress.OrganisationPK);
				AssertEquals("Buyer", buyerOrgHeader.PK, result.JE_OH_Buyer);
			}
		}

		public void TestFillDeclarant()
		{
			var declarant = CreateOrganisation("Declarant 1", "DEC1");
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				declaration.JE_ApplicationCode = "BLT";

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var declarationData = SetupDeclaration(ZString.Empty, ZString.Empty);
				declarationData.AddOrgAddress(writeManager, declarant, "Declarant");

				var reader = new CNJobDeclarationDataObjectReader(declarationData, logger, Factory);
				var result = reader.ReadIntoBusinessObject();

				AssertNotEquals("Declarant should not be read from XUS", declarant.MainAddress.PK, result.JE_OA_DeclarantAddress);
			}
		}

		static Shipment SetupDeclaration(ZString messageType, ZString messageSubType)
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
	}
}
