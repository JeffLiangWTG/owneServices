using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetailsWithEntryInstruction : Customs.DataTransfer.Universal.WarehouseCustomsLineDetailsWithEntryInstruction
	{
		public WarehouseCustomsLineDetailsWithEntryInstruction(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail)
			: base(factory, invoiceLine, fallbackDetail)
		{
		}

		protected override string CountryCode => GlbCompany.CurrentCompany.Country.Code;

		protected override List<AddInfo> GetThirdQuantityAsAddInfosForWarehousing() => new List<AddInfo>();

		protected override bool IsOutward
		{
			get
			{
				if (isOutward == null)
				{
					var procedure = InvoiceLine.Procedure.GetValueOrDefault();
					var procedureCode = procedure.Left(2).Trim();
					var ppc = procedure.SubstringSafe(2, 2).Trim();
					if (ppc.IsEmpty)
					{
						ppc = "00";
					}

					var procedureGroup = CustomsProcedureCode;

					isOutward = !procedureGroup.IsEmpty && (new RefCusProcedure.Loader(factory).LoadTop1FromCodeAndCountry(procedureCode, ppc, CountryCode, ZDateTime.Today, procedureGroup)?.IsOutOfWarehouse() ?? ZBool.False);
				}
				return isOutward.Value;
			}
		}
		bool? isOutward;
	}
}
