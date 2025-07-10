using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ExportCargoMeanagement : IExportCargoMeanagement
	{
		public string ImportCargoManagementNumber { get; set; }

		ZString IExportCargoMeanagement.ImportCargoManagementNumber => ImportCargoManagementNumber;
	}
}
