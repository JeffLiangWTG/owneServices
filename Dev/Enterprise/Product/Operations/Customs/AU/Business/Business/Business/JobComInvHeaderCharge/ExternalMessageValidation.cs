using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExternalMessageValidation : Customs.Business.ExternalMessageValidation
	{
		public ExternalMessageValidation(BusinessObject bizObj)
			: base(bizObj)
		{
		}

		public void ValidateDistributeByForEdifice(ZPropertyInfo distributeByField, bool isImportEdifice, bool isImport)
		{
			var distributeBy = (ZString)distributeByField.Value;
			if (!distributeBy.IsEmpty && distributeBy != Customs.Common.ChargeDistributeByList.Codes.Value)
			{
				if (isImportEdifice)
				{
					distributeByField.AddError(EdificeDistributionByOtherThanValueError);
				}
				else if (isImport)
				{
					distributeByField.AddWarning(DistributionByOtherThanValueWarningForCMR);
				}
			}
		}

		protected override string GetAdviceHowToFixNoValidExchangeRates()
		{
			var result = new ZStringBuilder();
			result.Append(base.GetAdviceHowToFixNoValidExchangeRates());
			result.Append(ManualUpgradeOfReference);
			return result.ToString();
		}

		protected override bool ValidateEachWorkingDayHasAnExchangeRate
		{
			get { return true; }
		}

		#region Strings

		public static string EdificeDistributionByOtherThanValueError
		{
			get { return Res.GetString("d41fe141-a21c-49cf-bea9-3743e99644cc", "Distribution by weight/volume for Edifice jobs cannot be done as Customs valuation is done by VALUE only."); }
		}

		public static string DistributionByOtherThanValueWarningForCMR
		{
			get { return Res.GetString("4019b836-1c63-409b-91cb-5f50006a4752", "Invoices will be normalized to FOB in AUD to avoid Customs Factor for line CV calculations as AU Customs Valuation is done by VALUE only."); }
		}

		public static string ManualUpgradeOfReference
		{
			get { return "\r\n" + Res.GetString("b08bb411-f626-4005-9091-afc664226a0f", "Alternatively, CMR Reference Files can be updated manually by selecting Config > System > Upgrades > CMR Reference Files Updates > Production > Full Update."); }
		}

		#endregion
	}
}
