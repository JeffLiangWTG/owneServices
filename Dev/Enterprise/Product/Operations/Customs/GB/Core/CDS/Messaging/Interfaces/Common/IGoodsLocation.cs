using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IGoodsLocation
	{
		ZString Name { get; }
		ZString TypeCode { get; }
		ZString AddressTypeCode { get; }
		ZString CountryCode { get; }
	}

	public class GoodsLocationWrapper : IGoodsLocation
	{
		GoodsLocationWrapper(ZString name, ZString typeCode, ZString addressTypeCode, ZString countryCode)
		{
			this.name = name;
			this.typeCode = typeCode;
			this.addressTypeCode = addressTypeCode;
			this.countryCode = countryCode;
		}

		public static GoodsLocationWrapper New(ZString name, ZString typeCode, ZString addressTypeCode, ZString countryCode)
		{
			return new GoodsLocationWrapper(name, typeCode, addressTypeCode, countryCode);
		}

		ZString IGoodsLocation.Name => name;

		ZString IGoodsLocation.TypeCode => typeCode;

		ZString IGoodsLocation.AddressTypeCode => addressTypeCode;

		ZString IGoodsLocation.CountryCode => countryCode;

		readonly ZString name;
		readonly ZString typeCode;
		readonly ZString addressTypeCode;
		readonly ZString countryCode;

		/*
		<GoodsLocation>
			<ID>WLALONBTW</ID>
			<TypeCode>A</TypeCode>
			<Address>
				<TypeCode>U</TypeCode>
				<CountryCode>GB</CountryCode>
			</Address>
		</GoodsLocation>
		 */
	}
}
