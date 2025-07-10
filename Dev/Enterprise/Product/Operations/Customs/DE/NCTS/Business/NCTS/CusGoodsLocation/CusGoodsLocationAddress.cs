using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class CusGoodsLocationAddress : EU.NCTS.Business.CusGoodsLocationAddress
	{
		public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGoodsLocationAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

		protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

		[MaxLength(70)]
		public override ZString E2_Contact
		{
			get => base.E2_Contact;
			set => base.E2_Contact = value;
		}

		public override ZPropertyInfo E2_PhoneInfo => GetZPropertyInfo(nameof(E2_Phone));

		[MaxLength(35)]
		public override ZString E2_City
		{
			get => base.E2_City;
			set => base.E2_City = value;
		}
	}
}
