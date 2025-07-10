using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonGoodsShipmentWrapper : IAESCommonGoodsShipment
	{
		public AESCommonGoodsShipmentWrapper(CusEntryHeader entryHeader, bool isComplementaryCWithMRN, bool isComlpX = false)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
			entryInstruction = entryHeader.EntryInstruction;
			this.isComlpX = isComlpX;
			this.isComplementaryCWithMRN = isComplementaryCWithMRN;
		}
		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
		protected readonly CusEntryInstruction entryInstruction;
		protected readonly bool isComplementaryCWithMRN;
		readonly bool isComlpX;

		public IWarehouseCommon Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					var presentationOffice = declaration.GetCustomsOfficeFromList(EuOfficeCodesTypes.Codes.OfficeOfPresentation);
					var previousProceduresOutOfWarehouse = entryHeader.MergedLines.Any(x => x.InvoiceLines.Any(y => ((JobComInvoiceLine)y).CusProcedure?.IsOutOfWarehouse() ?? false));
					if (previousProceduresOutOfWarehouse && (isComlpX || isComplementaryCWithMRN || !entryInstruction.IsSubStyleBOrC) && !presentationOffice.IsEmpty && entryInstruction.CusAuthorizationUsages.Any(x => CommonWrappersHelper.WarehouseTypeListContainsCode(x.AGC_Code)))
					{
						warehouse = new WarehouseCommonWrapper(entryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().FirstOrDefault(x => CommonWrappersHelper.WarehouseTypeListContainsCode(x.AGC_Code)));
					}
				}
				return warehouse;
			}
		}
		WarehouseCommonWrapper warehouse;
	}
}
