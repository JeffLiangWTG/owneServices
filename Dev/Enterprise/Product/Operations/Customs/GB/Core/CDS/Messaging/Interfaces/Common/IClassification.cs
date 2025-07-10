using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IClassification
	{
		ZString ID { get; }
		ZString TypeCode { get; }
	}

	public class ClassificationWrapper : IClassification
	{
		ClassificationWrapper(ZString id, ZString typeCode)
		{
			this.id = id;
			this.typeCode = typeCode;
		}

		public static ClassificationWrapper New(ZString id, ZString typeCode)
		{
			return new ClassificationWrapper(id, typeCode);
		}

		ZString IClassification.ID => id;

		ZString IClassification.TypeCode => typeCode;

		readonly ZString id;
		readonly ZString typeCode;
	}
}
