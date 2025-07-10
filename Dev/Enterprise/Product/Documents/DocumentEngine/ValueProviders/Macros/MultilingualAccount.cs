using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class MultilingualAccount : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<MultilingualAccount({languagecode},{countrycode,},{reportcode},{startorend})>",
				ResString.GetMultilingualString("12f8b2f9-1d94-4dbb-8001-db82084c561a",
				@"Returns an account number in the language specified by the language code and country code, for the report specified by the 3-character code (P&L - Profit and Loss, BSH - Balance Sheet). Will return either the starting account number or ending account number depending on whether '{0}' or '{1}' is specified as the third parameter. ",
				"START", "END"),
				new List<(string example, object expectedResult)> {
					("<MultilingualAccount(ZH-CN,CN,P&L,START)>", "5000.00.00"),
					("<MultilingualAccount(EN-US,AU,BSH,END)>", "4999.00.00") });
		}

		#region GetReplacement
		protected override object GetReplacementCore(string macro, Report report)
		{
			string firstParameter = fRegex.Match(macro).Groups[1].Value;
			string secondParameter = fRegex.Match(macro).Groups[2].Value;
			CountryCode = secondParameter;

			string thirdParameter = fRegex.Match(macro).Groups[3].Value;
			string fourthParameter = fRegex.Match(macro).Groups[4].Value;
			bool isSupportedReplacement = ((thirdParameter.Equals("P&L", StringComparison.OrdinalIgnoreCase) || thirdParameter.Equals("BSH", StringComparison.OrdinalIgnoreCase))
										&& (fourthParameter.Equals("START", StringComparison.OrdinalIgnoreCase) || fourthParameter.Equals("END", StringComparison.OrdinalIgnoreCase)));
			Language = firstParameter;
			bool isLocalAccountFrameworkSetup = !SecondReportStartFrom.Equals(Guid.Empty);
			if (isSupportedReplacement && isLocalAccountFrameworkSetup)
			{
				SecondReportStartFromAccountNumber = GetLocalAccount(SecondReportStartFrom);

				if (thirdParameter.Equals("P&L", StringComparison.OrdinalIgnoreCase) || thirdParameter.Equals("BSH", StringComparison.OrdinalIgnoreCase))
				{
					bool isSecondReport = (AccountOrderBeginWith.Equals(AccountOrderType.BalanceSheet) && thirdParameter.Equals("P&L", StringComparison.OrdinalIgnoreCase)) ||
						(AccountOrderBeginWith.Equals(AccountOrderType.ProfitAndLoss) && thirdParameter.Equals("BSH", StringComparison.OrdinalIgnoreCase));

					if (fourthParameter.Equals("START", StringComparison.OrdinalIgnoreCase))
					{
						return GetLocalAccountStart(SecondReportStartFromAccountNumber, isSecondReport);
					}
					return GetLocalAccountEnd(SecondReportStartFromAccountNumber, isSecondReport);
				}
			}
			return ZString.Empty;
		}

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = GetFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		protected virtual BusinessObjectFactory GetFactory()
		{
			return new BusinessObjectFactory();
		}

		protected string fLanguage;
		protected string fSecondReportStartFromAccountNumber;

		protected virtual AccountOrderType AccountOrderBeginWith
		{
			get
			{
				string accountsOrderBeginsWith = ObjectFactory.Get<IAccounting>().ReportOrder_AccountsOrderBeginsWith(Language, CountryCode);

				if (accountsOrderBeginsWith == nameof(AccountOrderType.BalanceSheet))
				{
					return AccountOrderType.BalanceSheet;
				}
				return AccountOrderType.ProfitAndLoss;
			}
		}

		protected virtual Guid SecondReportStartFrom
		{
			get
			{
				ZGuid gLAccountSecondReportStartsFrom = ObjectFactory.Get<IAccounting>().ReportOrder_GLAccountSecondReportStartsFrom(Language, CountryCode);

				if (gLAccountSecondReportStartsFrom != ZGuid.Empty && Factory.Load(typeof(AccGLAccountDescriptor), gLAccountSecondReportStartsFrom) != null)
				{
					return gLAccountSecondReportStartsFrom.ToGuid();
				}
				return Guid.Empty;
			}
		}

		protected ZString CountryCode
		{
			get { return countryCode; }
			set { countryCode = value; }
		}
		ZString countryCode;

		protected string Language
		{
			set
			{
				if (fLanguage != value)
				{
					fLanguage = value;
				}
			}
			get
			{
				return fLanguage;
			}
		}

		protected string SecondReportStartFromAccountNumber
		{
			set
			{
				if (fSecondReportStartFromAccountNumber != value)
				{
					fSecondReportStartFromAccountNumber = value;
				}
			}
			get
			{
				return fSecondReportStartFromAccountNumber;
			}
		}

		protected override void ResetCore()
		{
			fLanguage = string.Empty;
			fSecondReportStartFromAccountNumber = string.Empty;
			countryCode = string.Empty;
		}

		protected string GetLocalAccount(Guid localAccount)
		{
			return ((AccGLAccountDescriptor)Factory.Load(typeof(AccGLAccountDescriptor), localAccount)).AJ_LocalAccountNumber;
		}

		protected string GetLocalAccountStart(string localAccount, bool isSecondReport)
		{
			if (isSecondReport)
			{
				return SecondReportStartFromAccountNumber;
			}

			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, SQLComparisonOperator.Equal, Language);

			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance,
							   CountryCode.Length == 2 ? CountryCode : ZString.Empty);
			filter.MaximumRows = 1;
			filter.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.LessThan, localAccount);
			filter.OrderBy = AccGLAccountDescriptor.Schema.AJ_LocalAccountNumber;

			AccGLAccountDescriptor[] accounts = (AccGLAccountDescriptor[])Factory.Load(typeof(AccGLAccountDescriptor), filter);
			if (accounts.Length == 0)
			{
				throw new ApplicationException("Cannot get start Local Account Number for the first report. Please check Accounting>Framework>Report Order>Report Order Registry setting.");
			}
			return accounts[0].AJ_LocalAccountNumber;
		}

		protected string GetLocalAccountEnd(string localAccount, bool isSecondReport)
		{
			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, SQLComparisonOperator.Equal, Language);
			filter.MaximumRows = 1;

			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance,
							   CountryCode.Length == 2 ? CountryCode : ZString.Empty);

			if (isSecondReport)
			{
				filter.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.GreaterThan, localAccount);
			}
			else
			{
				filter.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.LessThan, localAccount);
			}
			filter.OrderBy = AccGLAccountDescriptor.Schema.AJ_LocalAccountNumber + " " + OrderByClause.Descending;

			AccGLAccountDescriptor[] accounts = (AccGLAccountDescriptor[])Factory.Load(typeof(AccGLAccountDescriptor), filter);
			if (accounts.Length == 0)
			{
				throw new ApplicationException(string.Format("Cannot get end Local Account Number for the {0} report. Please check Accounting>Framework>Report Order>Report Order Registry setting.", isSecondReport ? "second" : "first"));
			}
			return accounts[0].AJ_LocalAccountNumber;
		}
		#endregion

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)multilingual(?:[\s]*)account(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)([^\s]*)(?:[\s]*),(?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)([^\s]+)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
