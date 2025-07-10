using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using SendDeltaG1 = CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders
{
	public class MessageBuilderHelper
	{
		public T LoadTaxationDetails<T>(Interfaces.Common.ILiquidationItem liquidationItem, bool import)
		{
			var result = default(T);

			var tax = liquidationItem?.TaxDetail?.Tax;
			if (tax != null)
			{
				if (import)
				{
					var myTaxationDetail = new SendDeltaG1.Import.TTaxationDetail()
					{
						Codtax = tax.TaxCode,
						Typtax = tax.TaxType,
						Quotax = RemoveTrailingZeros(tax.TaxRate, 3),
						Asstax = tax.TaxAssessed.Round(0).ToZLong(),
						Montanttax = tax.TaxAmount.Round(0).ToZLong(),
						Codeport = tax.ChargePaymentOrDestinationID,
						StatutLiquidation = tax.TaxMethodOfPayment,
						UniSpe = LoadUniSpe<SendDeltaG1.Import.TUniSpe>(liquidationItem.TaxDetail.SuppUnit)
					};
					result = (T)Convert.ChangeType(myTaxationDetail, typeof(T), CultureInfo.InvariantCulture);
				}
				else
				{
					var myTaxationDetail = new SendDeltaG1.Export.TTaxationDetail()
					{
						Codtax = tax.TaxCode,
						Typtax = tax.TaxType,
						Quotax = RemoveTrailingZeros(tax.TaxRate, 3),
						Asstax = tax.TaxAssessed.Round(0).ToZLong(),
						Montanttax = tax.TaxAmount.Round(0).ToZLong(),
						Codeport = tax.ChargePaymentOrDestinationID,
						StatutLiquidation = tax.TaxMethodOfPayment,
						UniSpe = LoadUniSpe<SendDeltaG1.Export.TUniSpe>(liquidationItem.TaxDetail.SuppUnit)
					};
					result = (T)Convert.ChangeType(myTaxationDetail, typeof(T), CultureInfo.InvariantCulture);
				}
			}

			return result;
		}

		public decimal RemoveTrailingZeros(decimal value, int rounding)
		{
			ZDecimal result = Utilities.Round(value, rounding);
			result = Convert.ToDecimal(result.ToString("0.############################", CultureInfo.CurrentCulture), CultureInfo.CurrentCulture);
			return result;
		}

		public Collection<string> LoadAdditionnalCodeList(IEnumerable<Interfaces.Common.ITariffAdditionalCode> tariffAdditionalList)
		{
			var result = new Collection<string>();

			if (tariffAdditionalList != null)
			{
				foreach (var tariffAdditional in tariffAdditionalList)
				{
					result.Add(
						tariffAdditional?.Code.SubstringSafe(0,
							MessageBuilderBaseExtension.NumberOfCharactersAddCode) ?? string.Empty);
				}
			}

			return result;
		}

		public T LoadUniSpe<T>(Interfaces.Common.ISupplementaryUnit suppUnit) where T : ITUniSpe, new()
		{
			if (suppUnit == null || string.IsNullOrEmpty(suppUnit.Code) || suppUnit.Qty == decimal.Zero)
			{
				return default;
			}

			var uniSpe = new T();
			uniSpe.Unispe = suppUnit.Code;
			uniSpe.Nbrunispe = suppUnit.Qty;
			uniSpe.Qualifunispe = suppUnit.Qualif;

			return uniSpe;
		}

		public T LoadTrader<T>(Interfaces.Common.IOrganisation organisation, bool allowFallbakToVatNumberInsteadofEoriForTIN = true) where T : ITTrader, new()
		{
			if (organisation == null)
			{
				return default;
			}

			var itemTrader = new T();
			var tin = allowFallbakToVatNumberInsteadofEoriForTIN ? organisation.OrganisationNumber : organisation.OrganisationNumberEoriOnly;

			itemTrader.Tin = tin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom) ? ZString.Empty : tin.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText);
			itemTrader.Nomoperateur = organisation.FullName.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText);
			itemTrader.Rueoperateur = organisation.Address.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText);
			itemTrader.Paysoperateur = organisation.CountryCode.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText);
			itemTrader.Codepostaloperateur = organisation.PostCode.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText);
			itemTrader.Villeoperateur = organisation.City.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText);

			return itemTrader;
		}

		public Collection<T> LoadMenSpecTexteCodeList<T>(IEnumerable<Interfaces.Common.ITariffAdditionalCode> tariffAdditionalList) where T : ITMenspectexte, new()
		{
			var result = new Collection<T>();

			if (tariffAdditionalList != null)
			{
				foreach (var tariffAdditional in tariffAdditionalList)
				{
					var menSpecTexte = new T();
					menSpecTexte.Menspec =
						tariffAdditional.Code.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersAddCode);
					menSpecTexte.Menspectexte = tariffAdditional.Description;
					result.Add(menSpecTexte);
				}
			}

			return result;
		}

		#region will be used in WI00356351

		public static string GetYesOrNo(ZBool flag)
		{
			return flag ? "Y" : "N";
		}

		public static string GetOptionalPositiveInteger(ZInt number)
		{
			if (number > 0)
			{
				return Utilities.FormatNumberFromZInt(number, 0);
			}
			else
			{
				return null;
			}
		}

		public static string GetOptionalDecimal(ZDecimal input, int numberOfDecimalPlaces)
		{
			if (input.IsEmpty)
			{
				return null;
			}
			else
			{
				return Utilities.FormatNumber(input, numberOfDecimalPlaces);
			}
		}

		public static string GetOptionalString(ZString? input)
		{
			return !input.HasValue || input.Value.IsEmpty ? MessageBuilderBaseExtension.StringNull : input.ToString();
		}

		public static string GetRequiredString(ZString? input)
		{
			return !input.HasValue || input.Value.IsEmpty ? string.Empty : input.ToString();
		}

		public static string GetMandatoryString(ZString? input, ZString fieldName, EU.Business.ErrorCollector errorCollector)
		{
			if (!input.HasValue || input.Value.IsEmpty)
			{
				errorCollector.AddError(MessageMandatoryWithName(fieldName), new EU.Business.ErrorInfo("31", MessageMandatory));
				return string.Empty;
			}
			else
			{
				return input.ToString();
			}
		}
		#endregion

		public static string MessageMandatoryWithName(string name) => Res.GetString("905D4553-6277-4DE7-8EF6-F2AD40105818", "{0} is mandatory", name);
		public static string MessageTvaCodeNotFound => Res.GetString("8C322921-9F95-4AB7-BE45-CB7B1FC72235", "No TVA code found for the tax representative organization");
		public static string MessageNoInvoiceLineDefine => Res.GetString("C8D8C3DC-27E2-41FD-B033-0DBA17FACB03", "No invoice lines are defined");
		public static string MessageGrossWeightGreaterThanZero => Res.GetString("E3290F91-C1FC-4DC3-BFB4-7A7C6C892297", "Gross weight must be greater than zero");
		public static string MessageCustomsQuantityGreaterThanZero => Res.GetString("70D75356-84C6-4F7D-AA84-5252DBC52688", "Customs Quantity[38] must be greater than zero");
		public static string MessageValuationMethodMandatory => Res.GetString("39622ADA-6295-4146-88A0-7607EB8FC0DB", "Valuation method is mandatory");
		public static string MessageMandatory => Res.GetString("45308335-4EEE-4EF5-8E64-2DF76A444F73", "Mandatory");
		public static string MessageSupportingDocNeedDate(string refDoc) => Res.GetString("72084DBF-4615-4395-A10F-2F5DC27FBEC0", "Supporting doc : {0} must have a date", refDoc);
		public static string PartnerIdNotConfigured => Res.GetString("1C5CBDF1-78D2-419F-9EE6-8E9B8555DCA7", "Declarant/Representative ID is not configured. Config it in Organization > Config > France tab.");
		public static string RepresentativeIdNotConfigured(string deltaMode, string agreementType) => Res.GetString("1CA62CED-F64C-4D86-9347-B656860E92D0", "Representative ID is not configured. Config it in Organization > Config > France tab, with Delta Mode ({0}) and Agreement Type ({1}).", deltaMode, agreementType);
		public static string CBRNotConfigured => Res.GetString("267C0B78-EB2E-482B-BD47-0651C5C90647", "The brokerage code number (CBR) is not configured. Config it in Organization > Config > Registration Numbers / Codes tab");

		public const int CW_MarksAndNosMaxLengthForMessage = 42;
	}
}
