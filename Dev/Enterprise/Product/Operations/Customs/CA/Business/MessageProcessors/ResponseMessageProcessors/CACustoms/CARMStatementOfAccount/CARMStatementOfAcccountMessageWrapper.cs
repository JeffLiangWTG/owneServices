using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARM_TYPES;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMSOA;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CARMStatementOfAcccountMessageWrapper : ITableInterpretation
	{
		public CARMStatementOfAcccountMessageWrapper(CARMStatementOfAccountMessage message)
		{
			soa = XmlObjectSerializer.Deserialize<Zcarmsoa>(message.EM_MessageText);
			factory = message.Factory;
		}

		readonly BusinessObjectFactory factory;
		readonly Zcarmsoa soa;

		#region Properties

		public ZString FileName => GetValue<ZString>(soa, (x) => x.FileName);

		public ZString FileSeq => GetValue<ZString>(soa, (x) => x.FileSplit);

		public ZcarmsoaFileType FileType => soa.FileType;

		public ZString MessageSubTypeDescription => MessageTypeList.Descriptions.CARMStatementOfAccount;

		public ZDateTime PeriodStartDate => GetValue<ZDateTime>(soa, (x) => x.Header.PerStart);

		public ZDateTime PeriodEndDate => GetValue<ZDateTime>(soa, (x) => x.Header.PerEnd);

		public ZDateTime StatementDate => GetValue<ZDateTime>(soa, (x) => x.Header.SoaDate);

		public ZDateTime DueDate => GetValue(soa, (x) => x.Header.PayDue);

		public ZDecimal StatementAmount => GetValue<ZDecimal>(soa, (x) => x.Header.GrandTot);

		public ZString ImporterBusinessNumber => GetValue<ZString>(soa, (x) => x.Header.Party.Bn9);

		public ZString MessageEN
		{
			get
			{
				return GetValue<ZString>(soa,
					(x) => x.Notes.FirstOrDefault(z => z.Language == MessageLanguageTypeMessageLanguage.En)?.Value ?? ZString.Empty);
			}
		}

		public ZString MessageFR
		{
			get
			{
				return GetValue<ZString>(soa,
					(x) => x.Notes.FirstOrDefault(z => z.Language == MessageLanguageTypeMessageLanguage.Fr)?.Value ?? ZString.Empty);
			}
		}

		public IEnumerable<CARMStatementOfAccountMessageProgramAccountWrapper> ProgramAccount
		{
			get
			{
				return GetValue(soa,
					(x) => x.Details.Select(b => new CARMStatementOfAccountMessageProgramAccountWrapper(factory, b)));
			}
		}

		public ZDecimal PreviousStatementBalance => GetValue<ZDecimal>(soa, (x) => x.Summary.LastTotA);

		public ZDecimal CorrectionsToPreviousStatementBalance => GetValue<ZDecimal>(soa, (x) => x.Summary.CorrLastB);

		public ZDecimal PaymentsReceivedAfterPreviousSOA => GetValue<ZDecimal>(soa, (x) => x.Summary.PayLastC);

		public ZDecimal Disburesements => GetValue<ZDecimal>(soa, (x) => x.Summary.DisbD);

		public ZDecimal InterestAndPenaltiesSumTotal => GetValue<ZDecimal>(soa, (x) => x.Summary.InterestE);

		public ZDecimal CurrentPeriodCharges => GetValue<ZDecimal>(soa, (x) => x.Summary.DebitF);

		public ZDecimal CurrentPeriodCredit => GetValue<ZDecimal>(soa, (x) => x.Summary.CreditG);

		public ZDecimal CurrentStatementBalance => GetValue<ZDecimal>(soa, (x) => x.Summary.TotpayH);

		[ColumnName(0, "Duties")]
		public ZDecimal Duties => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Duties);

		[ColumnName(1, "Excise")]
		public ZDecimal Excise => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Excise);

		[ColumnName(2, "Excise Duties")]
		public ZDecimal ExciseDuties => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Exciseduties);

		[ColumnName(3, "SIMA")]
		public ZDecimal SIMA => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Sima);

		[ColumnName(4, "GST")]
		public ZDecimal GST => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Gst);

		[ColumnName(5, "HST")]
		public ZDecimal HST => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Hst);

		[ColumnName(6, "PST")]
		public ZDecimal PST => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Pst);

		[ColumnName(7, "Interest")]
		public ZDecimal Interest => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Interest);

		[ColumnName(8, "Penalties")]
		public ZDecimal Penalties => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Penalties);

		[ColumnName(9, "Payments")]
		public ZDecimal Payments => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Payments);

		[ColumnName(10, "Others")]
		public ZDecimal Others => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Others);

		[ColumnName(11, "Totals")]
		public ZDecimal Totals => GetValue<ZDecimal>(soa, (x) => x.Summary.RevDist.Totals);

		#endregion

		T GetValue<T>(Zcarmsoa statementOfAccount, Func<Zcarmsoa, T> valueGetter)
		{
			return valueGetter(statementOfAccount);
		}

		#region ITableInterpretation

		string ITableInterpretation.Caption => ZString.Empty;

		IEnumerable<string> ITableInterpretation.Titles => PropertyNameProvider.GetColumnTitles<CARMStatementOfAcccountMessageWrapper>();

		IEnumerable<object> ITableValues.Values
		{
			get
			{
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
