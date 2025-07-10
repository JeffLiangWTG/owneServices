using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;

namespace Enterprise.Client.Wow
{
	public class WoolworthsJobExchangeRate : Enterprise.Accounting.Business.JobInvoicing.ExchangeRate
	{
		public WoolworthsJobExchangeRate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected ZDecimal GetExchangeRate(ZString currencyNK)
		{
			ZDecimal result = 0;
			if (Job != null)
			{
				JobDeclaration declaration = (JobDeclaration)Factory.Load(typeof(JobDeclaration), this.Job.JH_ParentID);
				if (declaration == null)
				{
					ErrorReporter.ReportOnce(
						"ExpectedADeclarationInGetExchangeRate",
						"Expected a declaration");
				}
				if (declaration.JE_DateOfArrival.IsValid)
				{
					result = (ZDecimal)new Enterprise.ZArchitecture.Environment.ExchangeRate(false, 4, Env.CurrentCompany.PK).GetRate(
						currencyNK, Enterprise.ZArchitecture.Core.ExchangeRateType.Buy, declaration.JE_DateOfArrival.ToDateTime());
				}
			}
			return result;
		}
	}

	#region Implementation
	#endregion

}
