using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Documents.CMR
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This wrapper will be used in the CMRWayBill Excel template, which will be developed in future workflow")]
	public class CMRWayBillWrapper : ICMRConsignmentNote
	{
		public CMRWayBillWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));

			InitializeLazy();
		}
		readonly JobDeclaration declaration;

		ZString ICMRConsignmentNote.SupplierAddress => fullFormattedSupplierAddress ?? (fullFormattedSupplierAddress = CMRConsignmentNoteFormat.FormatEndOfLine(declaration.SupplierDocumentaryAddress?.Address?.AddressFullFormatted) ?? ZString.Empty);
		string fullFormattedSupplierAddress;

		ZString ICMRConsignmentNote.ImporterAddress => fullFormattedImporterAddress ?? (fullFormattedImporterAddress = CMRConsignmentNoteFormat.FormatEndOfLine(declaration.ImporterDocumentaryAddress?.Address?.AddressFullFormatted) ?? ZString.Empty);
		string fullFormattedImporterAddress;

		ZString ICMRConsignmentNote.InternationalConsignmentNote => internationalConsignmentNote ?? (internationalConsignmentNote = GetInternationalConsignmentNote());
		string internationalConsignmentNote;

		ZString ICMRConsignmentNote.PlaceOfDelivery => placeOfDelivery ?? (placeOfDelivery = GetPlaceOfDelivery());
		string placeOfDelivery;

		ZString ICMRConsignmentNote.CityCountryDateOfGoodsTakingOver => cityCountryDateOfGoodsTakingOver ?? (cityCountryDateOfGoodsTakingOver = GetCityCountryDateOfGoodsTakingOver());
		string cityCountryDateOfGoodsTakingOver;

		ZString ICMRConsignmentNote.CarrierAddress => carrierAddress ?? (carrierAddress = GetCarrierAddress());
		string carrierAddress;

		ZString ICMRConsignmentNote.GoodsAttachedDocuments => goodsAttachedDocuments ?? (goodsAttachedDocuments = GetGoodsAttachedDocuments());
		string goodsAttachedDocuments;

		ZString ICMRConsignmentNote.LineDetailsBox6_7_8_9 => lineDetailsBox6_7_8_9 ?? (lineDetailsBox6_7_8_9 = GetLineDetailsBox6_7_8_9());
		string lineDetailsBox6_7_8_9;

		ZString ICMRConsignmentNote.LineDetailsTariffCodeBox10 => lineDetailsTariffCodeBox10 ?? (lineDetailsTariffCodeBox10 = GetLineDetailsTariffCodeBox10());
		string lineDetailsTariffCodeBox10;

		ZString ICMRConsignmentNote.LineDetailsGrossWeightInKGBox11 => lineDetailsGrossWeightInKGBox11 ?? (lineDetailsGrossWeightInKGBox11 = GetLineDetailsGrossWeightInKGBox11());
		string lineDetailsGrossWeightInKGBox11;

		ZString ICMRConsignmentNote.LineDetailsVolumeInM3Box12 => lineDetailsVolumeInM3Box12 ?? (lineDetailsVolumeInM3Box12 = GetLineDetailsVolumeInM3Box12());
		string lineDetailsVolumeInM3Box12;

		ZString ICMRConsignmentNote.IncotermAndTextBox14 => incotermAndTextBox14 ?? (incotermAndTextBox14 = GetFormattedIncotermAndSeparator(declaration.IncoTerm));
		string incotermAndTextBox14;

		ZString ICMRConsignmentNote.TransportIDBox23 => transportIDBox23 ?? (transportIDBox23 = declaration.ZG_Box18TransportID);
		string transportIDBox23;

		ZString ICMRConsignmentNote.JobNumber => jobNumber ?? (jobNumber = declaration.JobNumber);
		string jobNumber;

		ZString ICMRConsignmentNote.Box14PaymentCarriage => Box14PaymentCarriageCore;
		protected virtual ZString Box14PaymentCarriageCore => Box14PaymentCarriageString;

		ZString ICMRConsignmentNote.Box19SpecialAgreements => Box19SpecialAgreementsCore;
		protected virtual ZString Box19SpecialAgreementsCore => Box19SpecialAgreementsString;

		ZString ICMRConsignmentNote.Box20ToBePaidBy => Box20ToBePaydByCore;
		protected virtual ZString Box20ToBePaydByCore => Box20ToBePaydByString;

		ZString ICMRConsignmentNote.Box15CashOnDelivery => Box15CashOnDeliveryCore;
		protected virtual ZString Box15CashOnDeliveryCore => Box15CashOnDeliveryString;

		ZString ICMRConsignmentNote.Box23TransportAndTrailerID => Box23TransportAndTrailerIDCore;
		protected virtual ZString Box23TransportAndTrailerIDCore => Box23TransportAndTrailerIDString;

		ZString ICMRConsignmentNote.SendersInstructions => ZString.Empty;

		ZString ICMRConsignmentNote.SpecialAgreements => ZString.Empty;

		ZString ICMRConsignmentNote.EstablishedInPlace => ZString.Empty;

		ZString ICMRConsignmentNote.EstablishedInDate => ZString.Empty;

		ZString ICMRConsignmentNote.DangerousGoodsClass => ZString.Empty;

		ZString ICMRConsignmentNote.DangerousGoodsNumber => ZString.Empty;

		ZString ICMRConsignmentNote.DangerousGoodsLetter => ZString.Empty;

		ITextLimitCalculator ICMRConsignmentNote.TextLimitCalculator => textLimitCalculator ?? (textLimitCalculator = new TextLimitCalculator());
		ITextLimitCalculator textLimitCalculator;

		#region Implementation

		ZString GetFormattedIncotermAndSeparator(ZString incoTerm) => incoTerm.IsEmpty ? ZString.Empty : (ZString)$"{incoTerm}{CMRConsignmentNoteConstants.Delimiters.DashBetweenSpaces}";

		ZString GetLineDetailsBox6_7_8_9()
		{
			var builder = new ZStringBuilder();

			foreach (var entryLine in EntryLines)
			{
				var lineBuilder = new ZStringBuilder();
				lineBuilder.AppendIfNotEmpty(new EntryLinePacksMarksAndNumbersBuilder(entryLine).Build());
				lineBuilder.Append(RemoveCarriageReturnsFromDescription(entryLine.EffectiveDescription));

				var cutLineDescription = new ZString(lineBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SemicolonAndSpace)).Left(CMRConsignmentNoteConstants.Length.MaxLineDetailLength);

				builder.Append(cutLineDescription);
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetLineDetailsTariffCodeBox10()
		{
			var builder = new ZStringBuilder();

			foreach (var entryLine in EntryLines)
			{
				builder.Append(entryLine.Tariff);
			}

			var lineDetailsTariffCodeBox10 = ZString.Empty;

			if (!builder.IsEmpty)
			{
				lineDetailsTariffCodeBox10 = builder.ToStringWithNewLineBetweenAppends();
			}

			return CMRConsignmentNoteFormat.TrimTrailingCarriageReturns(lineDetailsTariffCodeBox10);
		}

		ZString GetLineDetailsGrossWeightInKGBox11()
		{
			var builder = new ZStringBuilder();

			foreach (var entryLine in EntryLines)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(entryLine.EffectiveGrossWeight.InKilogramsSafe));
			}

			return CMRConsignmentNoteFormat.GetFormattedColumn(builder);
		}

		ZString GetLineDetailsVolumeInM3Box12()
		{
			var builder = new ZStringBuilder();

			foreach (var entryLine in EntryLines)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(entryLine.EffectiveVolume.InCubicMetres));
			}

			return CMRConsignmentNoteFormat.GetFormattedColumn(builder);
		}

		ZString GetGoodsAttachedDocuments()
		{
			var builder = new ZStringBuilder();

			AddReferenceToBuilder(GetN380References(declaration.SupportingDocuments), builder);

			foreach (var invoice in declaration.Invoices.Cast<JobComInvoiceHeader>())
			{
				AddReferenceToBuilder(GetN380References(invoice.SupportingDocuments), builder);
			}

			foreach (var line in declaration.InvoiceLines.Cast<JobComInvoiceLine>())
			{
				AddReferenceToBuilder(GetN380References(line.SupportingDocuments), builder);
			}

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.CommaAndSpace);
		}

		void AddReferenceToBuilder(IEnumerable<ZString> references, ZStringBuilder builder)
		{
			foreach (var reference in references)
			{
				builder.AppendIfNotEmpty(reference);
			}
		}

		IEnumerable<ZString> GetN380References(SupportingDocumentCollection supportingDocuments)
		{
			return supportingDocuments.Cast<SupportingDocument>()
				.Where(s => s.CSI_Code == SupportingDocumentTypes.N380)
				.Select(d => d.CSI_ReferenceNumber);
		}

		ZString GetCityCountryDateOfGoodsTakingOver()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(declaration.SupplierDocumentaryAddress?.City.ToUpperInvariant());
			builder.AppendIfNotEmpty(declaration.SupplierDocumentaryAddress?.Country?.Code ?? ZString.Empty);
			builder.AppendIfNotEmpty(new ZDate(declaration.JE_DateAtFinalDestination).ToString(CMRConsignmentNoteConstants.Formats.CMRDateFormat));

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetPlaceOfDelivery()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(declaration.ImporterDocumentaryAddress?.Postcode ?? ZString.Empty);
			builder.AppendIfNotEmpty(declaration.ImporterDocumentaryAddress?.City.ToUpperInvariant());
			builder.AppendIfNotEmpty(declaration.ImporterDocumentaryAddress?.Country?.Code ?? ZString.Empty);

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetCarrierAddress()
		{
			var carrier = declaration.ShippingLine;
			var builder = new ZStringBuilder();

			if (carrier != null)
			{
				var mainAddress = carrier?.Addresses.MainAddress;
				builder.AppendIfNotEmpty(mainAddress?.CompanyName.ToUpperInvariant());
				builder.AppendIfNotEmpty(mainAddress?.Address1.ToUpperInvariant());

				var cityAndCountryBuilder = new ZStringBuilder();
				cityAndCountryBuilder.AppendIfNotEmpty(GetCity());
				cityAndCountryBuilder.AppendIfNotEmpty(GetCountry());

				builder.AppendIfNotEmpty(cityAndCountryBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.DashBetweenSpaces));

				return builder.ToStringWithNewLineBetweenAppends();

				ZString GetCity() => mainAddress?.City.ToUpperInvariant() ?? ZString.Empty;

				ZString GetCountry() => mainAddress?.Country?.Description.ToUpperInvariant() ?? ZString.Empty;
			}

			return ZString.Empty;
		}

		ZString RemoveCarriageReturnsFromDescription(ZString description) => description.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");

		IEnumerable<CusEntryLine> EntryLines => lazyEntryLines.Value;

		Lazy<IEnumerable<CusEntryLine>> lazyEntryLines;

		void InitializeLazy()
		{
			lazyEntryLines = new Lazy<IEnumerable<CusEntryLine>>(() => declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(x => x.MergedLines).ToArray());
		}

		ZString GetInternationalConsignmentNote()
		{
			var transportMode = declaration.JE_TransportMode;

			if (transportMode == TransportTypeGenericList.Codes.Air || transportMode == TransportTypeGenericList.Codes.Sea)
			{
				return declaration.JE_MasterBill;
			}

			return declaration.JE_VesselName;
		}

		const string Box14PaymentCarriageString = "14";
		const string Box19SpecialAgreementsString = "19";
		const string Box20ToBePaydByString = "20";
		const string Box15CashOnDeliveryString = "15";
		const string Box23TransportAndTrailerIDString = "23";

		#endregion
	}
}
