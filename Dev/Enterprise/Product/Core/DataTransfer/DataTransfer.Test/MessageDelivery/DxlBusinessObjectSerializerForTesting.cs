using System.IO;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.MessageDelivery.Testing
{
	sealed class DxlBusinessObjectSerializerForTesting : DxlBusinessObjectSerializer
	{
		public DxlBusinessObjectSerializerForTesting(BusinessObject bizObjToSerialize)
			: base(null, null)
		{
		}

		internal Stream ConvertXmlToDxl(MemoryStream xmlStream, EDICommunicationsMode mode) => ConvertXmlToDxlStream(xmlStream, mode, "1");
	}
}
