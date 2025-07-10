using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Business
{
	public partial class Bill : Customs.Business.Bill, Integration.Customs.JP.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString CU_BillType
		{
			get => base.CU_BillType;
			set
			{
				if (value != base.CU_BillType)
				{
					base.CU_BillType = value;

					if (!IsCopying && !IsValidationSuspended && Declaration != null)
					{
						Declaration.Bills.ForEach(x => x.MarkAsNeedingValidation());
					}
				}
			}
		}
	}
}
