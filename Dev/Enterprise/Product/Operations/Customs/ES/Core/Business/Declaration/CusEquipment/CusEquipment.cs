using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEquipment : EU.Business.Declaration.CusEquipment, Integration.Customs.ES.ICusEquipment
	{
		public CusEquipment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			Seals.CountChanged -= Seals_CountChanged;
			Seals.CountChanged += Seals_CountChanged;
		}

		public new CusEquipmentValidation Validation => (CusEquipmentValidation)base.Validation;

		protected override Customs.Business.CusEquipmentValidation GetNewValidation() => new CusEquipmentValidation(this);

		public override void Delete()
		{
			Seals.CountChanged -= Seals_CountChanged;

			base.Delete();
		}

		void Seals_CountChanged(object sender, EventArgs e)
		{
			if (!IsDeleted)
			{
				Validation.ValidateCEQ_IdentificationNumber();
			}
		}
	}
}
