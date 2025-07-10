using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class AdditionalInfo : EU.ExitControl.Business.AdditionalInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);

		protected override Customs.Business.CusSupportingInfoLookups GetNewLookups() => new AdditionalInfoLookups(this);
	}
}
