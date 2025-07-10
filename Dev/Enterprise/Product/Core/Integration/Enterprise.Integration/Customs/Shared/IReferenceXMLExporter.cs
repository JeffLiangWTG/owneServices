using System;
using System.Collections.Generic;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IReferenceXMLExporter
			{
				void Export(IEnumerable<Guid> dataRecordPks, string xmlTypeName, string filePath);
				bool UploadFile(IEnumerable<Guid> dataRecordPks, string tableName, string emailContacts);
			}
		}
	}
}
