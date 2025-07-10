using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class InvoiceLineChargeLookups : EU.Business.Declaration.InvoiceLineChargeLookups
	{
		public InvoiceLineChargeLookups(EU.Business.Declaration.InvoiceLineCharge parent) : base(parent)
		{
		}

		protected new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				CodeDescriptionPairList result;
				var invoiceLine = Parent.InvoiceLine;
				if (invoiceLine?.IsImport ?? false)
				{
					var excludeChargeINP = (invoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._E01);
					var excludeChargeOPF = !IsOPFChargeSelectionAllowed(invoiceLine);

					result = Factory.GetCachedValue("DEInvoiceLineChargeLookups.ChargeTypeList_" + excludeChargeOPF + excludeChargeINP, () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddRange(base.ChargeTypeList);
						if (excludeChargeINP)
						{
							list.RemoveCode(ImportChargeCodeList.Codes.INP);
						}
						if (excludeChargeOPF)
						{
							list.RemoveCode(ImportChargeCodeList.Codes.OPF);
						}
						list.RemoveCode(ImportChargeCodeList.Codes.AIR);
						return list;
					});
				}
				else if (invoiceLine?.IsExport ?? false)
				{
					result = Factory.GetCachedValue<ChargeCodeList>();
				}
				else
				{
					result = base.ChargeTypeList;
				}

				return result;
			}
		}

		protected bool IsOPFChargeSelectionAllowed(JobComInvoiceLine invoiceLine)
		{
			bool validOPFCode = false;

			var procedureCode = invoiceLine.JI_Procedure.Left(2);
			var previousProcedureCode = invoiceLine.JI_Procedure.SubstringSafe(2, 2);
			var concession = invoiceLine.Concession;

			if ((procedureCode == CustomsProcedureCodeList.Import.ProcedureCode._61 || procedureCode == CustomsProcedureCodeList.Import.ProcedureCode._63)
				&& ((previousProcedureCode == CustomsProcedureCodeList.Import.PreviousProcedureCode._21 && concession != CustomsProcedureCodeList.Import.Concession._F01 && concession != CustomsProcedureCodeList.Import.Concession._B02 && concession != CustomsProcedureCodeList.Import.Concession._B03)
				|| (previousProcedureCode == CustomsProcedureCodeList.Import.PreviousProcedureCode._22 && concession != CustomsProcedureCodeList.Import.Concession._F01)))
			{
				validOPFCode = true;
			}
			return validOPFCode;
		}
	}
}
