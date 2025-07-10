using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ImportSADNumber : EU.EMCS.Business.ImportSADNumber
	{
		public ImportSADNumber(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new ImportSADNumberValidation(this);

		public new ImportSADNumberValidation Validation => (ImportSADNumberValidation)base.Validation;
	}
}
