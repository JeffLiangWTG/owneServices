using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class TransportMeansCollection : CusCodeDataCollection<TransportMeans>
	{
		public TransportMeansCollection(JobDeclaration declaration)
			: base(declaration, CusCodeDataTypeList.Codes.TransportMean)
		{
		}
	}
}
