using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DE.DataTransfer.Universal
{
	public class WarehouseRegimeTypeProvider : Customs.DataTransfer.Universal.WarehouseRegimeTypeProvider
	{
		public WarehouseRegimeTypeProvider(Shipment shipment) : base(shipment)
		{
		}

		protected override CustomsRegime GetCustomsRegimeCore()
		{
			CustomsRegime customsRegime;
			if (shipment.EntryInstructionCollection?.FirstOrDefault() is EntryInstruction entryInstruction && entryInstruction.Procedure.GetValueOrDefault() == ImportMainProcedureCodeList.Codes._51
				|| HasSubGroupThatHasInvoiceWithOutOfInwardProcedure51(shipment.CommercialInfo)
				|| HasInvoiceWithOutOfInwardProcedure(shipment.CommercialInfo))
			{
				customsRegime = CustomsRegime.InwardProcessing;
			}
			else
			{
				customsRegime = CustomsRegime.BondedWarehouse;
			}

			return customsRegime;
		}

		BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();
		BusinessObjectFactory factory;

		RefCusProcedure.Loader RefCusLoader => refCusLoader ??= new RefCusProcedure.Loader(Factory);
		RefCusProcedure.Loader refCusLoader;

		bool HasInvoiceWithOutOfInwardProcedure(CommercialInfo info) =>
			info?.CommercialInvoiceCollection
				?.Where(invoice => invoice.CommercialInvoiceLineCollection != null)
				.SelectMany(e => e.CommercialInvoiceLineCollection)
				.Select(e => e?.Procedure)
				.Distinct()
				.Any(e => e is { } procedure && IsOutOfInwardProcessing(procedure)) ?? false;

		bool HasSubGroupThatHasInvoiceWithOutOfInwardProcedure51(CommercialInfo info) =>
			info?.SubGroupCollection?.Any(HasInvoiceWithOutOfInwardProcedure) ?? false;

		bool IsOutOfInwardProcessing(ZString value) => RefCusLoader
			.LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(value.PadRight(7),
				Enterprise.Core.Constants.CountryCodes.Germany, ZDateTime.Today)?.IsOutOfInwardProcessing() ?? false;
	}
}
