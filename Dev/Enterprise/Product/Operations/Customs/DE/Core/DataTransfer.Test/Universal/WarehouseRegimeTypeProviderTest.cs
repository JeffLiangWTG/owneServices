using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing
{
	sealed class WarehouseRegimeTypeProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomsRegime_InwardProcessing()
		{
			var provider = GetProvider(ImportMainProcedureCodeList.Codes._51);
			var customsRegime = provider.GetCustomsRegime();

			AssertEquals(CustomsRegime.InwardProcessing, customsRegime);
		}

		public void TestGetCustomsRegime_OutOfInwardProcessing()
		{
			CreateOutOfInwardCusProcedure(Factory);
			var shipment = GetShipment(ImportMainProcedureCodeList.Codes._40);

			var header = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			header.SetCommercialInvoiceLineCollection(() => new() { new() { Procedure = "4051", }, });

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

		public void TestGetCustomsRegime_OutOfInwardProcessing_SubGroup()
		{
			CreateOutOfInwardCusProcedure(Factory);
			var shipment = GetShipment(ImportMainProcedureCodeList.Codes._40);

			var header = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			header.SetCommercialInvoiceLineCollection(() => new() { new() { Procedure = "4051", }, });

			shipment.CommercialInfo = new CommercialInfo()
			{
				SubGroupCollection = new()
				{
					new CommercialInfo()
					{
						CommercialInvoiceCollection = new ()
						{
							header,
						},
					}
				}
			};

			var provider = new WarehouseRegimeTypeProvider(shipment);

			var customsRegime = provider.GetCustomsRegime();

			AssertEquals(CustomsRegime.InwardProcessing, customsRegime);
		}

		public void TestGetCustomsRegime_BondedWarehouse()
		{
			var provider = GetProvider("22");
			var customsRegime = provider.GetCustomsRegime();

			AssertEquals(CustomsRegime.BondedWarehouse, customsRegime);
		}

		public void TestGetCustomsRegime_CommercialInvoiceLineCollectionIsNull()
		{
			CreateOutOfInwardCusProcedure(Factory);
			var shipment = GetShipment(ImportMainProcedureCodeList.Codes._40);

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

		WarehouseRegimeTypeProvider GetProvider(ZString entryInstructionProcedure)
		{
			return new WarehouseRegimeTypeProvider(GetShipment(entryInstructionProcedure));
		}

		Shipment GetShipment(ZString? entryInstructionProcedure)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
			};
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>() { new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) { Procedure = entryInstructionProcedure } });
			return shipment;
		}

		public static RefCusProcedure CreateOutOfInwardCusProcedure(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "IM", "40", "51", "   ", "4051", "IMP", "");
			procedure.ZZ6_IntoInwardProcessing = "N";
			procedure.ZZ6_OutOfInwardProcessing = "Y";
			factory.Save();
			return procedure;
		}
	}
}
