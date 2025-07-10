using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DCG.Response;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.MessageProcessors;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DCGResponseMessageDataObject : MessageDataObject<TMessage>, IDCGResponseDataProvider
	{
		public DCGResponseMessageDataObject(FREDIMessage message)
			: base(message)
		{
			this.country = message.GetCountryCodeSafe();
		}

		public ZString DateFrom => item?.Debutregul ?? ZString.Empty;

		public ZString DateTo => item?.Finregul ?? ZString.Empty;

		public ZString EntryNum => Entete?.Refdec ?? ZString.Empty;

		public ZString DCGReference => Entete?.Refdos ?? ZString.Empty;

		public ZString TransactionID => ResponseMessage?.EnveloppeMessage?.TransactionId;

		public Collection<TTaxationDetail> Taxes => item?.CumulLiquidation ?? new Collection<TTaxationDetail>();

		public Collection<TRefDsidse> Entries => item?.RefDsidse ?? new Collection<TRefDsidse>();

		public Collection<TReponseErreur> Errors => ReponseDatas?.Erreurs ?? new Collection<TReponseErreur>();

		public Collection<TAnomalie> Anomalies => ReponseDatas?.Anomalies ?? new Collection<TAnomalie>();

		public ZBool HasTaxes => Taxes.Any();

		public ZBool HasErrors => Errors.Any();

		public ZBool HasAnomalies => Anomalies.Any();

		public ZBool HasEntries => Entries.Any();

		public ZString GetMessageInterpretation() => Prettier.GetMessageInterpretation();

		public ZString GetReadableErrorList()
		{
			var stringBuilder = new ZStringBuilder();

			if (HasErrors)
			{
				stringBuilder.Append(ZString.Empty);
				stringBuilder.Append(ZString.Empty);
				stringBuilder.Append(Res.GetString("39702B9B-D20B-4E6B-A221-8C103357E9F7", "Following errors were reported by Customs:"));
				foreach (var error in Errors)
				{
					stringBuilder.Append(string.Format("- {0}/{1}", error.ErreurCode, error.ErreurDescription));
				}
			}

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		public ZString GetReadableAnomalyList()
		{
			var stringBuilder = new ZStringBuilder();

			if (HasAnomalies)
			{
				stringBuilder.Append(ZString.Empty);
				stringBuilder.Append(ZString.Empty);
				stringBuilder.Append(Res.GetString("E0972147-831B-4D90-8D90-74C0F42978BF", "Following anomalies were reported by Customs:"));
				foreach (var anomaly in Anomalies)
				{
					stringBuilder.Append(string.Format("- {0}/{1} ({2})", anomaly.Anomaliecode, anomaly.Anomaliedescr, anomaly.RefDsidse.Refdec));
				}
			}

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		public ZString GetReadableTaxList()
		{
			var stringBuilder = new ZStringBuilder();

			if (HasTaxes)
			{
				stringBuilder.Append(ZString.Empty);
				stringBuilder.Append(ZString.Empty);
				stringBuilder.Append(Res.GetString("3DC2879B-CFB0-4DB8-9C31-F6943BD64147", "Response lists following taxes:"));
				foreach (var tax in Taxes)
				{
					stringBuilder.Append(string.Format("- {0}/{1} : {2} {3}", tax.Codtax, tax.Codtaxeeu, tax.Montanttax, Core.Constants.CurrencyCodes.France));
				}
			}

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		public ZDate PeriodStartDate => GetDateTime(DateFrom);

		public ZDate PeriodEndDate => GetDateTime(DateTo);

		public ZString Direction
		{
			get
			{
				var direction = StatementEntryTypeImpExpList.Codes.Import;
				if (HasEntries)
				{
					var firstReportedEntry = EntryActionHelper.GetEntryHeaderFromEntryNumber(Factory, Entries[0].Refdec, country);
					if (firstReportedEntry != null)
					{
						direction = firstReportedEntry.IsImport ? StatementEntryTypeImpExpList.Codes.Import : StatementEntryTypeImpExpList.Codes.Export;
					}
				}
				return direction;
			}
		}

		public ZString Frequency
		{
			get
			{
				var frequency = StatementPeriodicityList.Codes.Unknown;

				if (HasEntries)
				{
					var firstReportedEntry = EntryActionHelper.GetEntryHeaderFromEntryNumber(Factory, Entries[0].Refdec, country);
					var result = firstReportedEntry?.Declaration?.JE_CustomsProfileRelatedAccount?.CZ_ReportingPeriod;
					switch (result)
					{
						case ReportingPeriodList.Codes.MON:
							frequency = StatementPeriodicityList.Codes.Month;
							break;
						case ReportingPeriodList.Codes.TEN:
							frequency = StatementPeriodicityList.Codes.Decade;
							break;
						case ReportingPeriodList.Codes.DAY:
							frequency = StatementPeriodicityList.Codes.Day;
							break;
					}
				}

				return frequency;
			}
		}

		public ZString MessageStatusDescription => (HasAnomalies || HasErrors) ? StatementStatusList.Descriptions.Incomplete : StatementStatusList.Descriptions.Complete;

		protected override FREDIMessagePrettier CreatePrettier() => new DCGResponsePrettier(this);

		ZDate GetDateTime(ZString date)
		{
			var result = ZDate.Empty;

			System.DateTime output;
			if (System.DateTime.TryParseExact(date, "dd/MM/yyyy", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out output))
			{
				result = (ZDate)output;
			}

			return result;
		}

		ReponseDeclaration ReponseDeclaration => cachedReponseDeclaration ?? (cachedReponseDeclaration = ResponseMessage?.ReponseDeclaration);
		ReponseDeclaration cachedReponseDeclaration;

		TReponseDatas ReponseDatas => cachedReponseDatas ?? (cachedReponseDatas = ReponseDeclaration?.ReponseDatas);
		TReponseDatas cachedReponseDatas;

		TEnteteReponse Entete => cachedEntete ?? (cachedEntete = ReponseDeclaration?.Entete);
		TEnteteReponse cachedEntete;

		Tok item => cachedItem ?? (cachedItem = ReponseDatas?.Ok);
		Tok cachedItem;
		readonly ZString country;
	}
}
