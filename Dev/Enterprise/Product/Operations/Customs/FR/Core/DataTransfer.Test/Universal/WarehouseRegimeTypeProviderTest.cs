using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
{
	sealed class WarehouseRegimeTypeProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomsRegime_InwardProcessing()
		{
			var provider = GetProvider(DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing, Core.Constants.CountryCodes.France);
			var customsRegime = provider.GetCustomsRegime();

			AssertEquals(CustomsRegime.InwardProcessing, customsRegime);
		}

		public void TestGetCustomsRegime_OutOfInwardProcessing()
		{
			CreateOutOfInwardCusProcedure(Factory, Core.Constants.CountryCodes.France);
			var shipment = GetShipment("30", Core.Constants.CountryCodes.France);

			var header = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			header.SetCommercialInvoiceLineCollection(() => new() { new() { Procedure = "3051", }, });

			shipment.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new()
				{
					header,
				},
			};

			var provider = new WarehouseRegimeTypeProvider(shipment);

			var customsRegime = provider.GetCustomsRegime();

			AssertEquals(CustomsRegime.InwardProcessing, customsRegime);
		}

		public void TestGetCustomsRegime_OutOfInwardProcessing_DeltaIE()
		{
			CreateOutOfInwardCusProcedure(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE);
			var shipment = GetShipment("30", DeclarationApplicationCodeList.Codes.DeltaIE);

			var header = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			header.SetCommercialInvoiceLineCollection(() => new() { new() { Procedure = "3051", }, });

			shipment.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new()
				{
					header,
				},
			};

			var provider = new WarehouseRegimeTypeProvider(shipment);

			var customsRegime = provider.GetCustomsRegime();

			AssertEquals(CustomsRegime.InwardProcessing, customsRegime);
		}

		public void TestGetCustomsRegime_CommercialInvoiceLineCollectionIsNull()
		{
			CreateOutOfInwardCusProcedure(Factory, Core.Constants.CountryCodes.France);
			var shipment = GetShipment("10", Core.Constants.CountryCodes.France);

			var header = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			header.SetCommercialInvoiceLineCollection(() => null);

			shipment.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new()
				 {
					 header,
				 },
			};

			var provider = new WarehouseRegimeTypeProvider(shipment);
			var customsRegime = CustomsRegime.InwardProcessing;

			AssertNoExceptionThrown(() =>
			{
				customsRegime = provider.GetCustomsRegime();
			});

			AssertEquals(CustomsRegime.BondedWarehouse, customsRegime);
		}

		public void TestGetCustomsRegime_BondedWarehouse()
		{
			var provider = GetProvider("10", Core.Constants.CountryCodes.France);
			var customsRegime = provider.GetCustomsRegime();

			AssertEquals(CustomsRegime.BondedWarehouse, customsRegime);
		}

		public static RefCusProcedure CreateOutOfInwardCusProcedure(BusinessObjectFactory factory, ZString dataGrouping)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(dataGrouping, "IM", "30", "51", "   ", "3051", "IMP", "");
			procedure.ZZ6_IntoInwardProcessing = "N";
			procedure.ZZ6_OutOfInwardProcessing = "Y";
			factory.Save();
			return procedure;
		}

		WarehouseRegimeTypeProvider GetProvider(ZString entryInstructionStyle, ZString code)
		{
			return new WarehouseRegimeTypeProvider(GetShipment(entryInstructionStyle, code));
		}

		Shipment GetShipment(ZString? entryInstructionStyle, ZString code)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessagingApplicationCode = new CodeDescriptionPair { Code = code, Description = "datagrouping is " + code },
			};
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>() { new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) { Style = entryInstructionStyle } });
			return shipment;
		}
	}
}
