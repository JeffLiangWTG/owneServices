using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconSnapshot : Customs.Business.CusReconSnapshot, Integration.Customs.KR.ICusReconSnapshot
	{
		public CusReconSnapshot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ImportEntryOrEntryLineSerializable ImportEntryOrEntryLine
		{
			get
			{
				if (importEntryOrEntryLine == null)
				{
					if (!CRS_SnapshotXml.IsEmpty)
					{
						using (var textReader = GetCRS_SnapshotXmlReader())
						{
							importEntryOrEntryLine = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryOrEntryLineSerializable>(textReader);
						}
					}
				}
				return importEntryOrEntryLine;
			}
		}
		ImportEntryOrEntryLineSerializable importEntryOrEntryLine;
	}
}
