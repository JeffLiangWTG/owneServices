using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IDecAdditionalDocument
	{
		ZString CategoryCode { get; }
		ZString TypeCode { get; }
		ZString ID { get; }
		ZDateTime SystemCreateTime { get; }
	}

	class DecAdditionalDocumentWrapper : IDecAdditionalDocument
	{
		readonly ZString categoryCode;
		readonly ZString typeCode;
		readonly ZString id;
		readonly ZDateTime systemCreateTime;

		DecAdditionalDocumentWrapper(ZString categoryCode, ZString typeCode, ZString id)
		{
			this.categoryCode = categoryCode;
			this.typeCode = typeCode;
			this.id = id;
			this.systemCreateTime = ZDateTime.UtcNow;
		}

		public static DecAdditionalDocumentWrapper New(ZString categoryCode, ZString typeCode, ZString id)
		{
			return new DecAdditionalDocumentWrapper(categoryCode, typeCode, id);
		}

		ZString IDecAdditionalDocument.CategoryCode => categoryCode;

		ZString IDecAdditionalDocument.TypeCode => typeCode;

		ZString IDecAdditionalDocument.ID => id;

		ZDateTime IDecAdditionalDocument.SystemCreateTime => systemCreateTime;
	}
}
