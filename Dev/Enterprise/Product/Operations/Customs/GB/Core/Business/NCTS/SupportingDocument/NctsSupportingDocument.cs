using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business
{
	public class NctsSupportingDocument : EU.NCTS.Business.NctsSupportingDocument
	{
		public NctsSupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsSupportingDocumentValidation Validation => (NctsSupportingDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewPhase5Validation() => new NctsSupportingDocumentValidation(this);

		protected override CusSupportingInfoValidation GetNewPhase4Validation() => new NctsSupportingDocumentValidation(this);
	}
}
