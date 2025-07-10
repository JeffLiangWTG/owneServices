using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class VehicleNumberValidation : CusCodeDataValidation
	{
		public VehicleNumberValidation(VehicleNumber parent)
			: base(parent)
		{
		}

		new VehicleNumber Parent => (VehicleNumber)base.Parent;

		protected override void CheckCY_Data()
		{
			var invoiceLine = Parent.Parent as JobComInvoiceLine;
			if (invoiceLine != null && invoiceLine.IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);

				if (!invoiceLine.Declaration.IsDeclarationProcedureTypeB)
				{
					Parent.CY_DataInfo.AddMessageError(Res.GetString("2312C387-9AEF-4DDB-A8BF-8E58270A7AD1", "If 'Second Hand Vehicles' is inserted, 'Declaration Type' must be 'B'."));
				}
			}
		}
	}
}
