using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsPreviousDocument : EU.NCTS.Business.NctsPreviousDocument
	{
		public NctsPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		protected override CusSupportingInfoValidation GetNewPhase5Validation() => new NctsPreviousDocumentPhase5Validation(this);

		protected override CusSupportingInfoValidation GetNewPhase4Validation() => new NctsPreviousDocumentPhase5Validation(this);
	}
}
