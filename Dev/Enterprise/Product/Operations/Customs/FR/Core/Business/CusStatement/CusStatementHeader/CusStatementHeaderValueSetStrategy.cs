using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementHeaderValueSetStrategy : IValueSetStrategy
	{
		public CusStatementHeaderValueSetStrategy(CusStatementHeader statement)
		{
			Statement = statement;
		}

		protected readonly CusStatementHeader Statement;

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case CusStatementHeader.Schema.B2_BranchDesignation:
					DefaultB2_EntryFilerCode();
					break;
				case CusStatementHeader.Schema.B2_OH_Importer:
					DefaultB2_StatementType();
					DefaultB2_EntryFilerCode();
					DefaultB2_ImporterCustomsID();
					DefaultB2_CheckNo();
					break;
				case CusStatementHeader.Schema.B2_StatementType:
					DefaultB2_PeriodStartDate();
					DefaultB2_PeriodEndDate();
					break;
				case CusStatementHeader.Schema.B2_PeriodStartDate:
					DefaultB2_PeriodEndDate();
					break;
				case CusStatementHeader.Schema.B2_PaymentType:
					ClearB2_CheckNoIfNecessary();
					break;
			}
		}

		void DefaultB2_StatementType()
		{
			if (!Statement.B2_StatementTypeInfo.ReadOnly)
			{
				var reportingPeriod = Statement.RelatedCusAccount?.CZ_ReportingPeriod ?? ZString.Empty;
				Statement.B2_StatementType = MapReportingPeriodToStatementPeriodicity(reportingPeriod);
			}
		}

		void DefaultB2_EntryFilerCode()
		{
			if (!Statement.B2_EntryFilerCodeInfo.ReadOnly)
			{
				Statement.B2_EntryFilerCode = Statement.RelatedCusAccount?.CZ_Account ?? ZString.Empty;
			}
		}

		void DefaultB2_ImporterCustomsID()
		{
			if (!Statement.B2_ImporterCustomsIDInfo.ReadOnly)
			{
				var cbrCode = GetCusCode(OrgCusCode.CodeTypes.BrokerageRegistration);
				Statement.B2_ImporterCustomsID = cbrCode.Left(CusStatementHeader.Schema.B2_ImporterCustomsIDMaxLength);
			}
		}

		void DefaultB2_CheckNo()
		{
			if (!Statement.B2_CheckNoInfo.ReadOnly)
			{
				var danCode = GetCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber);
				Statement.B2_CheckNo = danCode.Left(CusStatementHeader.Schema.B2_CheckNoMaxLength);
			}
		}

		void DefaultB2_PeriodStartDate()
		{
			if (!Statement.B2_PeriodStartDateInfo.ReadOnly)
			{
				var previousStartDate = GetPreviousStartDate();
				if (previousStartDate.IsValid)
				{
					Statement.B2_PeriodStartDate = previousStartDate;
				}
			}
		}

		void DefaultB2_PeriodEndDate()
		{
			if (!Statement.B2_PeriodEndDateInfo.ReadOnly)
			{
				var previousEndDate = GetPreviousEndDate();
				if (previousEndDate.IsValid)
				{
					Statement.B2_PeriodEndDate = previousEndDate;
				}
			}
		}

		void ClearB2_CheckNoIfNecessary()
		{
			if (Statement.B2_PaymentType != MethodOfPaymentList.Codes.M && Statement.B2_PaymentType != MethodOfPaymentList.Codes.R)
			{
				Statement.B2_CheckNo = ZString.Empty;
			}
		}

		ZString MapReportingPeriodToStatementPeriodicity(ZString reportingPeriod)
		{
			switch (reportingPeriod)
			{
				case ReportingPeriodList.Codes.DAY:
					return StatementPeriodicityList.Codes.Day;
				case ReportingPeriodList.Codes.TEN:
					return StatementPeriodicityList.Codes.Decade;
				case ReportingPeriodList.Codes.MON:
					return StatementPeriodicityList.Codes.Month;
				default:
					return ZString.Empty;
			}
		}

		ZString GetCusCode(ZString codeType)
		{
			return Statement.Importer?.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.France) ?? ZString.Empty;
		}

		ZDate GetPreviousStartDate()
		{
			var periodicity = Statement.B2_StatementType;
			var today = ZDate.Today;
			switch (periodicity)
			{
				case StatementPeriodicityList.Codes.Day:
					return ZDate.Empty;
				case StatementPeriodicityList.Codes.Decade:
					switch (today.Day)
					{
						case int day when (day < 11):
							return new ZDate(today.Year, today.Month, 21).AddMonths(-1);
						case int day when (day < 21):
							return new ZDate(today.Year, today.Month, 1);
						default:
							return new ZDate(today.Year, today.Month, 11);
					}
				case StatementPeriodicityList.Codes.Month:
					return new ZDate(today.Year, today.Month, 1).AddMonths(-1);
			}
			return ZDate.Empty;
		}

		ZDate GetPreviousEndDate()
		{
			var periodicity = Statement.B2_StatementType;
			var startDate = Statement.B2_PeriodStartDate;
			if (startDate.IsValid)
			{
				switch (periodicity)
				{
					case StatementPeriodicityList.Codes.Day:
						return startDate;
					case StatementPeriodicityList.Codes.Decade:
						switch (startDate.Day)
						{
							case int day when (day < 11):
								return new ZDate(startDate.Year, startDate.Month, 10);
							case int day when (day < 21):
								return new ZDate(startDate.Year, startDate.Month, 20);
							default:
								return new ZDate(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);
						}
					case StatementPeriodicityList.Codes.Month:
						return new ZDate(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);
				}
			}
			return ZDate.Empty;
		}
	}
}
