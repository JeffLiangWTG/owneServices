
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CMRAqisPremises.Schema.QP_AQISPremisesIdentifier), DescriptionProperty(CMRAqisPremises.Schema.QP_AQISPremisesName)]
	public class CMRAqisPremises : AutoCMRAqisPremises
	{
		public CMRAqisPremises(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRAqisPremises New(BusinessObjectFactory factory)
		{
			return factory.New<CMRAqisPremises>();
		}
	}
}
