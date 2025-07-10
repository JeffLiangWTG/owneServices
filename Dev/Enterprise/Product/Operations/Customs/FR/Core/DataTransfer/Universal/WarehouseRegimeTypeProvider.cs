using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.FR.DataTransfer.Universal
{
	public class WarehouseRegimeTypeProvider : Customs.DataTransfer.Universal.WarehouseRegimeTypeProvider
	{
		public WarehouseRegimeTypeProvider(Shipment shipment) : base(shipment)
		{
		}

		protected override CustomsRegime GetCustomsRegimeCore()
		{
			CustomsRegime customsRegime;
			if (HasImportationForInwardProcessingProcedure() || HasOutOfInwardProcedure())
			{
				customsRegime = CustomsRegime.InwardProcessing;
			}
			else
			{
				customsRegime = CustomsRegime.BondedWarehouse;
			}

			return customsRegime;
		}

		bool HasImportationForInwardProcessingProcedure()
		{
			return shipment.EntryInstructionCollection.FirstOrDefault() is EntryInstruction entryInstruction && entryInstruction.Style.GetValueOrDefault() == DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing;
		}

		bool HasOutOfInwardProcedure()
		{
			return shipment.CommercialInfo?.CommercialInvoiceCollection
				?.Where(invoice => invoice.CommercialInvoiceLineCollection != null)
				.SelectMany(e => e.CommercialInvoiceLineCollection)
				.Select(e => e?.Procedure)
				.Distinct()
				.Any(e => e is { } procedure && IsOutOfInwardProcessing(procedure)) ?? false;
		}

		BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();
		BusinessObjectFactory factory;

		RefCusProcedure.Loader RefCusLoader => refCusLoader ??= new RefCusProcedure.Loader(Factory);
		RefCusProcedure.Loader refCusLoader;

		bool IsOutOfInwardProcessing(ZString value) => RefCusLoader
			.LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(value.PadRight(7), shipment.MessagingApplicationCode.GetCodeAsUpperCase() == DeclarationApplicationCodeList.Codes.DeltaIE ? Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE : Core.Constants.CountryCodes.France, ZDateTime.Today)?.IsOutOfInwardProcessing() ?? false;
	}
}
