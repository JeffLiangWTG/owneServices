using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsSupportingDocument : EU.NCTS.Business.NctsSupportingDocument
	{
		public NctsSupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusSupportingInfoValidation GetNewPhase5Validation() => IsPhase5Departure ? new NctsSupportingDocumentDepartureValidation(this) : new CusSupportingInfoDisabledValidation(this);

		protected override CusSupportingInfoValidation GetNewPhase4Validation() => new CusSupportingInfoDisabledValidation(this);
	}
}
