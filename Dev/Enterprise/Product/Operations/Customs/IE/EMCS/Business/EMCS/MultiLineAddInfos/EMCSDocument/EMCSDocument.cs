using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSDocument : EU.EMCS.Business.EMCSDocument
	{
		public EMCSDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new EMCSDocumentValidation(this);
	}
}
