using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARM_TYPES;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMDN_AH;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMDN_CB;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CARMDailyNoticeMessageWrapper : ITableInterpretation
	{
		public CARMDailyNoticeMessageWrapper(CARMDailyNoticeMessage message)
		{
			if (message.EM_MessageSubType == CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter)
			{
				dailyNoticeAH = XmlObjectSerializer.Deserialize<Zcarmdnoticeah>(message.EM_MessageText);
			}
			else
			{
				dailyNoticeCB = XmlObjectSerializer.Deserialize<Zcarmdnoticecb>(message.EM_MessageText);
			}
			factory = message.Factory;
		}

		readonly BusinessObjectFactory factory;
		readonly Zcarmdnoticeah dailyNoticeAH;
		readonly Zcarmdnoticecb dailyNoticeCB;

		#region Properties

		public ZString FileName => GetValue<ZString>(dailyNoticeAH, dailyNoticeCB, (x) => x.FileName, (y) => y.FileName);

		public ZString FileSeq => GetValue<ZString>(dailyNoticeAH, dailyNoticeCB, (x) => x.FileSplit, (y) => y.FileSplit);

		public ZString StatementType => GetValue<ZString>(dailyNoticeAH, dailyNoticeCB, (x) => CusStatementHeaderTypes.Codes.Importer, (y) => CusStatementHeaderTypes.Codes.Broker);

		public ZString MessageSubTypeDescription => GetValue<ZString>(dailyNoticeAH, dailyNoticeCB, (x) => CARMDailyNoticeMessageSubTypeList.Descriptions.CARMDNoticeImporter, (y) => CARMDailyNoticeMessageSubTypeList.Descriptions.CARMDNoticeBroker);

		public ZString LegalName
		{
			get
			{
				return GetValue<ZString>(dailyNoticeAH, dailyNoticeCB,
					(x) => ZString.Join(" ", new ZString[] { x.Header.Party.NameOrg1, x.Header.Party.NameOrg2, x.Header.Party.NameOrg3, x.Header.Party.NameOrg4 }).TrimEnd(),
					(y) => y.Header.Party.OpName);
			}
		}

		public ZString ImporterBusinessNumber => GetValue<ZString>(dailyNoticeAH, dailyNoticeCB, (x) => x.Header.Party.Account, (y) => BN9);

		public ZString RMNumber => GetValue(dailyNoticeAH, dailyNoticeCB, (x) => ImporterBusinessNumber.SubstringSafe(11, 4), (y) => ZString.Empty);

		public ZString BN9 => GetValue<ZString>(dailyNoticeAH, dailyNoticeCB, (x) => x.Header.Party.Bn9, (y) => y.Header.Party.Bn9);

		public ZDateTime StatementDate => GetValue<ZDateTime>(dailyNoticeAH, dailyNoticeCB, (x) => x.Header.DnDate, (y) => y.Header.DnDate);

		public ZDecimal StatementAmount => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.ImpDec, (y) => ZDecimal.Zero);

		public ZDecimal PaidAmount => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.Payments, (y) => ZDecimal.Zero);

		public ZDecimal RefundAmount => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.Disb, (y) => ZDecimal.Zero);

		public ZDateTime? DueDate => GetValue(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.RevDist.PaymentDueDate, (y) => new ZDateTime?());

		public ZString MessageEN
		{
			get
			{
				return GetValue<ZString>(dailyNoticeAH, dailyNoticeCB,
					(x) => (x.Notes.FirstOrDefault(z => z.Language == MessageLanguageTypeMessageLanguage.En)?.Value ?? ZString.Empty),
					(y) => (y.Notes.FirstOrDefault(z => z.Language == MessageLanguageTypeMessageLanguage.En)?.Value ?? ZString.Empty));
			}
		}

		public ZString MessageFR
		{
			get
			{
				return GetValue<ZString>(dailyNoticeAH, dailyNoticeCB,
					(x) => (x.Notes.FirstOrDefault(z => z.Language == MessageLanguageTypeMessageLanguage.Fr)?.Value ?? ZString.Empty),
					(y) => (y.Notes.FirstOrDefault(z => z.Language == MessageLanguageTypeMessageLanguage.Fr)?.Value ?? ZString.Empty));
			}
		}

		public IEnumerable<CARMDailyNoticeMessageLineGroupWrapper> LineGroups
		{
			get
			{
				return GetValue(dailyNoticeAH, dailyNoticeCB,
					(x) => new[] { new CARMDailyNoticeMessageLineGroupWrapper(factory, x) },
					(y) => y.Details.Importer.OrderBy(a => a.Id).Select(b => new CARMDailyNoticeMessageLineGroupWrapper(factory, b)));
			}
		}

		[ColumnName(0, "Duties")]
		public ZDecimal TotalDuties => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.RevDist.Duties, (y) => LineGroups.Sum(z => z.TotalDuties));

		[ColumnName(1, "SIMA")]
		public ZDecimal TotalSIMA => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.RevDist.Sima, (y) => LineGroups.Sum(z => z.TotalSIMA));

		[ColumnName(2, "Excise Tax")]
		public ZDecimal TotalExciseTax => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.RevDist.Excise, (y) => LineGroups.Sum(z => z.TotalExciseTax));

		[ColumnName(3, "Excise Duties")]
		public ZDecimal TotalExciseDuties => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.RevDist.Exciseduties, (y) => LineGroups.Sum(z => z.TotalExciseDuties));

		[ColumnName(4, "GST/PST/HST")]
		public ZDecimal TotalGSTAndPSTAndHST => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.RevDist.Gst + x.Summary.RevDist.Pst + x.Summary.RevDist.Hst, (y) => LineGroups.Sum(z => z.TotalGSTAndPSTAndHST));

		[ColumnName(5, "Interests")]
		public ZDecimal TotalInterests => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.RevDist.Interest, (y) => LineGroups.Sum(z => z.TotalInterests));

		[ColumnName(6, "Others")]
		public ZDecimal TotalOthers => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.RevDist.Others, (y) => LineGroups.Sum(z => z.TotalOthers));

		[ColumnName(7, "Totals")]
		public ZDecimal TotalTotals => GetValue<ZDecimal>(dailyNoticeAH, dailyNoticeCB, (x) => x.Summary.RevDist.Totals, (y) => LineGroups.Sum(z => z.TotalTotals));

		public OrgHeader Importer
		{
			get
			{
				if (importerCached == null)
				{
					importerCached = new CachedProperty<OrgHeader>(factory, () =>
					{
						OrgHeader result = null;
						var importerPK = StatementMessageProcessorHelper.FindImporter(factory, ImporterBusinessNumber, StatementType);
						if (!importerPK.IsEmpty)
						{
							result = factory.Load<OrgHeader>(importerPK);
						}
						return result;
					});
				}
				return importerCached.Value;
			}
		}
		CachedProperty<OrgHeader> importerCached;

		public bool IsBroker => StatementType == CusStatementHeaderTypes.Codes.Broker;

		#endregion

		#region ITableInterpretation

		string ITableInterpretation.Caption => Res.GetString("1F426A40-C299-4C81-BF8A-B6F0F04D0D96", "Report Grand Total");

		IEnumerable<string> ITableInterpretation.Titles => PropertyNameProvider.GetColumnTitles<CARMDailyNoticeMessageWrapper>();

		IEnumerable<object> ITableValues.Values
		{
			get
			{
				yield return InterpretationHelper.FormatAmount(TotalDuties);
				yield return InterpretationHelper.FormatAmount(TotalSIMA);
				yield return InterpretationHelper.FormatAmount(TotalExciseTax);
				yield return InterpretationHelper.FormatAmount(TotalExciseDuties);
				yield return InterpretationHelper.FormatAmount(TotalGSTAndPSTAndHST);
				yield return InterpretationHelper.FormatAmount(TotalInterests);
				yield return InterpretationHelper.FormatAmount(TotalOthers);
				yield return InterpretationHelper.FormatAmount(TotalTotals);
			}
		}

		#endregion

		T GetValue<T>(Zcarmdnoticeah dailyNoticeAH, Zcarmdnoticecb dailyNoticeCB, Func<Zcarmdnoticeah, T> valueGetterAH, Func<Zcarmdnoticecb, T> valueGetterCB)
		{
			if (dailyNoticeAH != null)
			{
				return valueGetterAH(dailyNoticeAH);
			}
			else
			{
				return valueGetterCB(dailyNoticeCB);
			}
		}
	}
}
