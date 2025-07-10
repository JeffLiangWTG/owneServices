using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusEntryInstructionValidation : EU.Business.Declaration.CusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
			IsUCC6 = parent.IsUCC6;
		}

		protected readonly bool IsUCC6;

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		protected override void CheckCEI_Style()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_StyleInfo);
		}

		protected override void CheckCEI_OA_Warehouse()
		{
			base.CheckCEI_OA_Warehouse();

			if (Parent.CEI_OA_Warehouse.IsValid)
			{
				var authorizationUsageCount = Parent.FromWarehouseAuthorizationUsages.Count;

				if (authorizationUsageCount == 0)
				{
					Parent.CEI_OA_WarehouseInfo.AddMessageError(Res.GetString("5E8D7C18-4501-4C55-ACBE-E60DB0E9DEA9", "Please enter an Authorization for the Warehouse From."));
				}
				else if (authorizationUsageCount > 1)
				{
					Parent.CEI_OA_WarehouseInfo.AddMessageError(Res.GetString("5FC95E1D-0AC5-4871-904C-CC3C8DC7F8CF", "More than one Authorizations exist for the Warehouse From."));
				}
			}
			else if (Parent.CEI_OA_Warehouse.IsEmpty && Parent.HasOutOfWarehouseProcedure)
			{
				Parent.CEI_OA_WarehouseInfo.AddMessageError(Res.GetString("318733C1-6717-41B4-8358-064A5A0B1393", "This field is mandatory when Procedure/CPC is an out of warehouse procedure."));
			}
		}

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();

			if (Parent.CEI_OA_Warehouse2.IsValid)
			{
				var authorizationUsageCount = Parent.ToWarehouseAuthorizationUsages.Count;

				if (authorizationUsageCount == 0 && !Parent.HasInvoiceLineWithPreviousProcedure0700)
				{
					Parent.CEI_OA_Warehouse2Info.AddMessageError(Res.GetString("8D1575BE-B1B3-4802-893B-F127E4F0419F", "Please enter an Authorization for the Warehouse To. Authorizations can be entered under Maintain > Customs > Customs Files > Authorizations"));
				}
				else if (authorizationUsageCount > 1)
				{
					Parent.CEI_OA_Warehouse2Info.AddMessageError(Res.GetString("BF4BA32E-8B5E-4748-B5A6-B942A9194A73", "More than one Authorizations exist for the Warehouse To."));
				}
			}
			else if (Parent.CEI_OA_Warehouse2.IsEmpty && Parent.HasIntoWarehouseProcedure)
			{
				Parent.CEI_OA_Warehouse2Info.AddMessageError(Res.GetString("CD674F84-5644-4644-8D7E-1C355264FFD4", "This field is mandatory when Procedure/CPC is an into warehouse procedure."));
			}
		}
	}
}
