using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class ReimportCountryCodeValidation : Customs.Business.CusCodeDataValidation
	{
		public ReimportCountryCodeValidation(ReimportCountryCode parent)
			: base(parent)
		{
		}

		protected new ReimportCountryCode Parent => (ReimportCountryCode)base.Parent;

		protected override void CheckCY_Code()
		{
			if (Parent.Parent is CusEntryInstruction entryInstruction && entryInstruction.EnabledOutwardProcessing)
			{
				var targetInfo = Parent.CY_CodeInfo;
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
				var countryCode = Parent.CY_Code;
				if (!countryCode.IsEmpty)
				{
					var parentPK = Parent.PK;
					if (entryInstruction.ReimportCountryCodes.Cast<ReimportCountryCode>().Any(x => x.CY_Code == countryCode && x.PK != parentPK))
					{
						targetInfo.AddMessageError(Res.GetString("ed58c6c9-f2ee-4dca-859c-8edd2af92721", "Country/Region Code has already been entered."));
					}
				}

				if (entryInstruction.Style2ndDigitIs2() && countryCode != Core.Constants.CountryCodes.Germany)
				{
					targetInfo.AddMessageError(Res.GetString("9B3F8879-847F-478B-913D-621E719AC482", "The selected Type (Procedure) requires the Reimport Country/Region to be 'DE'."));
				}
			}
		}
	}
}
