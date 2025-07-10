using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.H7.Business
{
	public sealed class CusGoodsLocation : EU.H7.Business.CusGoodsLocation, Integration.Customs.IEH7.ICusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

		public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		public override ZString DisplayText
		{
			get
			{
				var stringBuilder = new ZStringBuilder();
				stringBuilder.AppendIfNotEmpty(CGL_Qualifier);
				stringBuilder.AppendIfNotEmpty(CGL_Type);
				var latitude = Address.E2_Latitude;
				var longitude = Address.E2_Longitude;
				stringBuilder.AppendIfNotEmpty(latitude != ZDecimal.Zero || longitude != ZDecimal.Zero ? $"{latitude:0.0000000},{longitude:0.0000000}" : string.Empty);
				stringBuilder.AppendIfNotEmpty(Address.E2_GovRegNum);
				stringBuilder.AppendIfNotEmpty(Address.E2_Address1AndE2_Address2);
				stringBuilder.AppendIfNotEmpty(Address.E2_Postcode);
				stringBuilder.AppendIfNotEmpty(CGL_CustomsOffice);
				stringBuilder.AppendIfNotEmpty(CGL_AdditionalIdentifier);
				stringBuilder.AppendIfNotEmpty(Address.E2_City);
				stringBuilder.AppendIfNotEmpty(Address.E2_RN_NKCountryCode);
				var contact = Address.E2_Contact;
				stringBuilder.AppendIfNotEmpty(contact.IsEmpty ? string.Empty : $"Contact {contact}");
				var phone = Address.E2_Phone;
				stringBuilder.AppendIfNotEmpty(phone.IsEmpty ? string.Empty : $"Ph {phone}");
				stringBuilder.AppendIfNotEmpty(Address.E2_Email);

				return stringBuilder.ToStringWithDelimiterBetweenAppends(";");
			}
		}

		public override ZString Unlocode
		{
			get => base.CGL_CustomsOffice;
			set => base.CGL_CustomsOffice = value;
		}

		public override ZPropertyInfo UnlocodeInfo => GetWrappedZPropertyInfo(nameof(Unlocode), x => CGL_CustomsOfficeInfo);
	}
}
