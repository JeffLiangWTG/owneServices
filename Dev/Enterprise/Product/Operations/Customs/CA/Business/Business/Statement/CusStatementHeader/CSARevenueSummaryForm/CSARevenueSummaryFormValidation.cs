using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CSARevenueSummaryFormValidation : CusStatementHeaderValidation
	{
		public CSARevenueSummaryFormValidation(AutoCusStatementHeader parent) : base(parent)
		{
		}

		CusStatementHeader CusStatementHeader => (CusStatementHeader)Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePeriodMonth();
			ValidatePeriodYear();
		}

		protected override void CheckB2_OH_Importer()
		{
			base.CheckB2_OH_Importer();
			MandatoryValidation.CheckEntered(Parent.B2_OH_ImporterInfo);
			var importer = Parent.Importer;
			if (importer != null)
			{
				var businessNumber = importer.GetCABusinessNumber();
				if (businessNumber.IsEmpty)
				{
					Parent.B2_OH_ImporterInfo.AddError(Res.GetString("945237C7-3D54-4ED5-8DCD-05DB77D30469", "This importer doesn't contain Business Number For Importer/Export."));
				}
			}
		}

		protected override void CheckB2_PeriodEndDate()
		{
			base.CheckB2_PeriodEndDate();
			MandatoryValidation.CheckEntered(Parent.B2_PeriodEndDateInfo);
			var endDate = Parent.B2_PeriodEndDate;
			if (endDate.IsValid)
			{
				var day = endDate.Day;
				var endDay = DateTime.DaysInMonth(endDate.Year, endDate.Month);
				if (day != endDay && day != 18)
				{
					Parent.B2_PeriodEndDateInfo.AddError(Res.GetString("FCE4A29C-C72D-45C2-B7C6-F17B04C9A883", "Please choose 18th or end date as the day of this month."));
				}
			}
		}

		protected override void CheckB2_StatementNumber()
		{
			base.CheckB2_StatementNumber();
			var statementNumber = Parent.B2_StatementNumber;
			if (!statementNumber.IsEmpty)
			{
				var query = new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
				query.AddToFilter(CusStatementHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				var loadedHeader = Parent.Factory.LoadTop1<CusStatementHeader>(query);
				if (loadedHeader != null)
				{
					Parent.B2_StatementNumberInfo.AddError(Res.GetString("E907D791-26B3-4C76-A5D2-8498D3E97A88", "There is already exists a CSA Revenue Summary Form with statement number {0}.", statementNumber));
				}
			}
		}

		public void ValidatePeriodMonth()
		{
			ValidateCalculatedProperty(CusStatementHeader.PeriodMonthInfo);
		}

		protected void CheckPeriodMonth()
		{
			var parent = CusStatementHeader;
			var month = parent.PeriodMonth;
			if (!parent.ImporterHasNoAccountingTimeOption && (month < 1 || month > 13))
			{
				parent.PeriodMonthInfo.AddError(Res.GetString("55961968-C30E-4C00-97C2-556DB7CA08B9", "Period Month range from 1 to 12."));
			}
		}

		public void ValidatePeriodYear()
		{
			ValidateCalculatedProperty(CusStatementHeader.PeriodYearInfo);
		}

		protected void CheckPeriodYear()
		{
			var parent = CusStatementHeader;
			var year = parent.PeriodYear;
			if (!parent.ImporterHasNoAccountingTimeOption && (year < 1900 || year > 2079))
			{
				parent.PeriodYearInfo.AddError(Res.GetString("2AF6CD47-0129-437E-973C-2F23F91F9752", "Please enter a valid year"));
			}
		}
	}
}
