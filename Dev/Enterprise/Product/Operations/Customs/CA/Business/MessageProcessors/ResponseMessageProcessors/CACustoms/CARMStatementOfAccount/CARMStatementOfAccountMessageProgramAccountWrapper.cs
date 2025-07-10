using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMSOA;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Messaging.MessageProcessors;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CARMStatementOfAccountMessageProgramAccountWrapper : ITableValues
	{
		public CARMStatementOfAccountMessageProgramAccountWrapper(BusinessObjectFactory factory, ZcarmsoaDetailsProgramAccount programAccount)
		{
			this.programAccount = programAccount;
		}

		readonly ZcarmsoaDetailsProgramAccount programAccount;

		#region Properties

		public string Acccount => GetValue<ZString>(programAccount, (x) => x.Account[0]);

		public ZDecimal Duties => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Duties);

		public ZDecimal Excise => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Excise);

		public ZDecimal ExciseDuties => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Exciseduties);

		public ZDecimal SIMA => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Sima);

		public ZDecimal GST => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Gst);

		public ZDecimal HST => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Hst);

		public ZDecimal PST => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Pst);

		public ZDecimal Interest => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Interest);

		public ZDecimal Penalties => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Penalties);

		public ZDecimal Payments => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Payments);

		public ZDecimal Others => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Others);

		public ZDecimal Totals => GetValue<ZDecimal>(programAccount, (x) => x.Linetotals.Totals);

		public IEnumerable<CARMStatementOfAccountMessageDaySummaryWrapper> DaySummary
		{
			get
			{
				return GetValue(programAccount,
					(x) => x.DaySummary.OrderBy(a => a.ReleaseDate).Select(b => new CARMStatementOfAccountMessageDaySummaryWrapper(Acccount, b)));
			}
		}

		#endregion

		T GetValue<T>(ZcarmsoaDetailsProgramAccount programAcccount, Func<ZcarmsoaDetailsProgramAccount, T> valueGetter)
		{
			return valueGetter(programAcccount);
		}

		#region ITableValues

		public IEnumerable<object> Values
		{
			get
			{
				var caption = Res.GetString("370194BD-D4B9-4F63-BC41-CA3E32A83744", "Line Total");
				yield return new CellWithFormatting(caption, TableInterpretation.Attributes.GetColspanAttribute(3), true);
				yield return InterpretationHelper.FormatAmount(Duties);
				yield return InterpretationHelper.FormatAmount(Excise);
				yield return InterpretationHelper.FormatAmount(ExciseDuties);
				yield return InterpretationHelper.FormatAmount(SIMA);
				yield return InterpretationHelper.FormatAmount(GST);
				yield return InterpretationHelper.FormatAmount(HST);
				yield return InterpretationHelper.FormatAmount(PST);
				yield return InterpretationHelper.FormatAmount(Interest);
				yield return InterpretationHelper.FormatAmount(Penalties);
				yield return InterpretationHelper.FormatAmount(Payments);
				yield return InterpretationHelper.FormatAmount(Others);
				yield return InterpretationHelper.FormatAmount(Totals);
			}
		}

		#endregion
	}
}
