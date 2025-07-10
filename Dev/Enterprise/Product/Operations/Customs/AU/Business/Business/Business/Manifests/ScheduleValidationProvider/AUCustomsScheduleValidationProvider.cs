using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class AUCustomsScheduleValidationProvider : IScheduleValidationProvider
	{
		#region IScheduleValidationProvider Members

		public JobVoyageValidation GetExtraVoyageValidation(JobVoyage voyage)
		{
			if (voyage.IsSea && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				return new ExtraVoyageValidation(voyage);
			}

			return null;
		}

		#endregion
	}
}
