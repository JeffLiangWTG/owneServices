using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMDN_AH;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMDN_CB;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CARMDailyNoticeMessageLineGroupWrapper : ITableValues
	{
		public CARMDailyNoticeMessageLineGroupWrapper(BusinessObjectFactory factory, Zcarmdnoticeah dailyNoticeAH)
			 : this(factory)
		{
			this.dailyNoticeAH = dailyNoticeAH;
			this.detailsAH = dailyNoticeAH.Details;
		}

		readonly Zcarmdnoticeah dailyNoticeAH;
		readonly ZcarmdnoticeahDetails detailsAH;

		public CARMDailyNoticeMessageLineGroupWrapper(BusinessObjectFactory factory, ZcarmdnoticecbDetailsImporter detailsCB)
			 : this(factory)
		{
			this.detailsCB = detailsCB;
		}

		readonly ZcarmdnoticecbDetailsImporter detailsCB;

		CARMDailyNoticeMessageLineGroupWrapper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		#region Properties

		public ZString LegalName
		{
			get
			{
				return GetValue(dailyNoticeAH, detailsAH, detailsCB,
					(x, y) => ZString.Join(" ", new ZString[] { x.Header.Party.NameOrg1, x.Header.Party.NameOrg2, x.Header.Party.NameOrg3, x.Header.Party.NameOrg4 }).TrimEnd(),
					(z) => ZString.Join(" ", new ZString[] { z.Party.NameOrg1, z.Party.NameOrg2, z.Party.NameOrg3, z.Party.NameOrg4 }).TrimEnd());
			}
		}

		public ZString ImporterBusinessNumber => GetValue<ZString>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => x.Header.Party.Account, (z) => z.Party.Account);

		public OrgHeader Importer
		{
			get
			{
				if (importerCached == null)
				{
					importerCached = new CachedProperty<OrgHeader>(factory, () =>
					{
						OrgHeader result = null;
						var importerPK = StatementMessageProcessorHelper.FindImporter(factory, ImporterBusinessNumber, CusStatementHeaderTypes.Codes.Importer);
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

		public ZDecimal PaidAmount => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => x.Summary.Payments, (z) => z.Linetotals.Payments);

		public ZDecimal RefundAmount => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => x.Summary.Disb, (z) => ZDecimal.Zero);

		public ZDecimal TotalDuties => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => y.Linetotals.Duties, (z) => z.Linetotals.Duties);

		public ZDecimal TotalSIMA => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => y.Linetotals.Sima, (z) => z.Linetotals.Sima);

		public ZDecimal TotalExciseTax => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => y.Linetotals.Excise, (z) => z.Linetotals.Excise);

		public ZDecimal TotalExciseDuties => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => y.Linetotals.Exciseduties, (z) => z.Linetotals.Exciseduties);

		public ZDecimal TotalGSTAndPSTAndHST => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => y.Linetotals.Gst + y.Linetotals.Pst + y.Linetotals.Hst, (z) => z.Linetotals.Gst + z.Linetotals.Pst + z.Linetotals.Hst);

		public ZDecimal TotalInterests => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => y.Linetotals.Interest, (z) => z.Linetotals.Interest);

		public ZDecimal TotalOthers => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => y.Linetotals.Others, (z) => z.Linetotals.Others);

		public ZDecimal TotalTotals => GetValue<ZDecimal>(dailyNoticeAH, detailsAH, detailsCB, (x, y) => y.Linetotals.Totals, (z) => z.Linetotals.Totals);

		public IEnumerable<CARMDailyNoticeMessageLineWrapper> LineIteams
		{
			get
			{
				return GetValue(dailyNoticeAH, detailsAH, detailsCB,
					(x, y) => y.Lineitems.Lineitem.OrderBy(a => a.Id).Select(b => new CARMDailyNoticeMessageLineWrapper(factory, b)),
					(z) => z.Lineitems.Lineitem.OrderBy(a => a.Id).Select(b => new CARMDailyNoticeMessageLineWrapper(factory, b)));
			}
		}

		#endregion

		#region ITableValues

		public IEnumerable<object> Values
		{
			get
			{
				var caption = Res.GetString("730CB77D-0F33-4C43-994B-DAA580D4E8C1", "Importer Total");
				yield return new CellWithFormatting(caption, TableInterpretation.Attributes.GetColspanAttribute(4), true);
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

		T GetValue<T>(Zcarmdnoticeah dailyNoticeAH, ZcarmdnoticeahDetails detailsAH, ZcarmdnoticecbDetailsImporter detailsCB, Func<Zcarmdnoticeah, ZcarmdnoticeahDetails, T> valueGetterAH, Func<ZcarmdnoticecbDetailsImporter, T> valueGetterCB)
		{
			if (dailyNoticeAH != null && detailsAH != null)
			{
				return valueGetterAH(dailyNoticeAH, detailsAH);
			}
			else
			{
				return valueGetterCB(detailsCB);
			}
		}
	}
}
