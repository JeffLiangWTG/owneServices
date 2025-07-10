using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TariffValidator
	{
		public TariffValidator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public void Validate(ZPropertyInfo tariffInfo, bool useCustomsTariffList, int minLengthRequired, ZDateTime dutyDate, bool isImport, bool isEmptyAllowed = false)
		{
			var tariffCode = TariffFormatter.Format(tariffInfo.Value.ToString());
			if (!isEmptyAllowed && tariffCode.IsEmpty)
			{
				tariffInfo.AddMessageError(Res.GetString("324cd632-86c0-4346-b35c-8f27bfe73715", "You have not entered a Classification Tariff code."));
			}
			if (!tariffCode.IsEmpty)
			{
				if (minLengthRequired > 0 && tariffCode.Length < minLengthRequired)
				{
					if (minLengthRequired == 10)
					{
						tariffInfo.AddMessageError(Res.GetString("f5017df5-7e9e-497c-8edc-2061fb530209", "Classification Tariff code must be 10 characters long."));
					}
					else
					{
						tariffInfo.AddMessageError(Res.GetString("3a7e9399-969d-437a-84b9-36c7f2cc6346", "Classification Tariff code must be at least {0} characters long.", minLengthRequired));
					}
				}
				else if (useCustomsTariffList)
				{
					if (dutyDate.IsValid && CACClassHeader.Load(factory, dutyDate, tariffCode) == null)
					{
						if (isImport)
						{
							tariffInfo.AddMessageError(Res.GetString("D7BFC06B-5800-49AB-A478-E3884CB86006", "Classification was not found or is not valid for {0}.", dutyDate.ToShortDateString()));
						}
						else
						{
							tariffInfo.AddMessageError(Res.GetString("A7DF98E4-BED1-46E8-91D9-FA50CFF251B5", "Classification was not found or is not valid for {0}.", dutyDate.ToShortDateString()));
						}
					}
				}
				else if (!useCustomsTariffList && !CodeExistsInCustomsTariffList(tariffCode, ZDateTime.Empty))
				{
					tariffInfo.AddMessageError(Res.GetString("74d3cb7c-16ba-48fc-821c-02387e6397c1", "Classification Tariff code {0} not found in the export tariff code list.", tariffCode));
				}
			}
		}

		public void ValidateFourDigitTariff(ZPropertyInfo tariffInfo)
		{
			ZString tariffCode = tariffInfo.Value.ToString();
			if (!tariffCode.IsEmpty)
			{
				if (!Regex.IsMatch(tariffCode, @"^(99|00)\d\d$"))
				{
					tariffInfo.AddMessageError(Res.GetString("ef7db086-b79f-4d1f-9a25-42381d013452", "Tariff code does not match pattern '99XX' or '00XX'"));
				}
				else if (!CodeExistsInCustomsTariffList(tariffCode, ZDateTime.Empty))
				{
					tariffInfo.AddMessageError(Res.GetString("a9297208-879c-4748-b9f7-819f94ebfe54", "Tariff code {0} not found in the customs tariff code list.", tariffCode));
				}
			}
		}

		TariffView.Loader TariffViewLoader
		{
			get
			{
				if (tariffViewLoader == null)
				{
					tariffViewLoader = new TariffView.Loader(factory);
				}
				return tariffViewLoader;
			}
		}
		TariffView.Loader tariffViewLoader;

		bool CodeExistsInCustomsTariffList(ZString tariffCode, ZDateTime dutyDate)
		{
			var result = false;
			if (tariffCode.Length > 10)
			{
				tariffCode = tariffCode.Substring(0, 10);
			}
			else
			{
				tariffCode = tariffCode.PadRight(10, '0');
			}
			result = TariffViewLoader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, tariffCode, dutyDate) != null;
			return result;
		}

		TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter tariffFormatter;
	}
}
