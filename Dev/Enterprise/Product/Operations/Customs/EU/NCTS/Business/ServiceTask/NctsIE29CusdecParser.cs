using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class NctsIE29CusdecParser : INctsIE29CusdecParser
	{
		public NctsIE29CusdecParser()
		{
		}

		public virtual NctsIE29CusdecResponseData Parse()
		{
			return new NctsIE29CusdecResponseData(Factory); // Please implement this in your country, see FR or GB for an example
		}

		protected NctsIE29CusdecResponseData ie29ResponseData;

		public NctsEdiMessage EdiMessage { set; get; }
		public BusinessObjectFactory Factory { set; get; }
	}
}
