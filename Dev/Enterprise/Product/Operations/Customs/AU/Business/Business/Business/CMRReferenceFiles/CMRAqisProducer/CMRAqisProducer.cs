
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CMRAqisProducer.Schema.QR_AQISProducerCode), DescriptionProperty(CMRAqisProducer.Schema.QR_AQISProducerName)]
	public class CMRAqisProducer : AutoCMRAqisProducer
	{
		public CMRAqisProducer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRAqisProducer New(BusinessObjectFactory factory)
		{
			return factory.New<CMRAqisProducer>();
		}
	}
}
