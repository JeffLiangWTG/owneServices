using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ReportOrder : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string AccountsOrderBeginsWith = "AccountsOrderBeginsWith";
			public const string AccountsOrderEndsWith = "AccountsOrderEndsWith";
			public const string GLAccountFirstReportStartsFrom = "GLAccountFirstReportStartsFrom";
			public const string GLAccountSecondReportStartsFrom = "GLAccountSecondReportStartsFrom";
			public const string Language = "Language";
			public const string CountryCode = "CountryCode";
		}

		#endregion

		public ReportOrder()
		{
		}

		public ReportOrder(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ReportOrder(factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateLanguage();
			ValidateCountryCode();
			ValidateAccountsOrderBeginsWith();
			ValidateGLAccountSecondReportStartsFrom();
		}

		#region Bound Properties

		#region Language

		[MaxLength(7)]
		[List("LanguageList")]
		public ZString Language
		{
			get { return language; }
			set
			{
				CheckMaximumLength(LanguageInfo, value);
				SetNonPersistentPropertyValue(LanguageInfo, ref language, value);
				fAccountDescriptorList = null;
				if (!IsValidationSuspended)
				{
					ValidateLanguage();
				}
				fGLAccountFirstReportStartsFrom = ZGuid.Empty;
				GLAccountFirstReportStartsFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return GetZPropertyInfo(Schema.Language); }
		}

		public void ValidateLanguage()
		{
			LanguageInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(LanguageInfo);
			ListValidation.ErrorIfInvalidCode(LanguageInfo, LanguageList);
			CheckRecordIsUnique();
		}

		ZString language;

		#endregion

		#region  Country  Code

		ZString fCountryCode = ZString.Empty;
		public RefCountryCollection Countries
		{
			get { return countries ?? (countries = new RefCountryCollection(CurrentFactory)); }
		}

		RefCountryCollection countries;

		[MaxLength(2)]
		[List("Countries")]
		public ZString CountryCode
		{
			get { return fCountryCode; }
			set
			{
				CheckMaximumLength(CountryCodeInfo, value);
				SetNonPersistentPropertyValue(CountryCodeInfo, ref fCountryCode, value);
				fAccountDescriptorList = null;
				if (!IsValidationSuspended)
				{
					ValidateCountryCode();
				}
				fGLAccountFirstReportStartsFrom = ZGuid.Empty;
				GLAccountFirstReportStartsFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CountryCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CountryCode); }
		}

		void ValidateCountryCode()
		{
			CountryCodeInfo.ClearAllNotifications();
			if (fCountryCode.Length > 0)
			{
				ListValidation.ErrorIfInvalidCode(CountryCodeInfo);
			}
			CheckRecordIsUnique();
		}

		void CheckRecordIsUnique()
		{
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				foreach (ReportOrder item in collection)
				{
					item.ClearRowNotifications();
					if (item != this &&
						item.Language == Language && item.CountryCode == CountryCode)
					{
						ZString duplicateMessage = Res.GetString("2274CFA3-F043-40D2-B018-23716B3BE9FF", "Duplicate Country/Region Code and Language.");
						AddRowError(duplicateMessage);
						item.AddRowError(duplicateMessage);
						break;
					}
				}
			}
		}

		#endregion

		#region Accounts Order Begins With

		[MaxLength(15)]
		[List("AccountOrderTypeList")]
		public ZString AccountsOrderBeginsWith
		{
			get { return accountsOrderBeginsWith; }
			set
			{
				CheckMaximumLength(AccountsOrderBeginsWithInfo, value);
				SetNonPersistentPropertyValue(AccountsOrderBeginsWithInfo, ref accountsOrderBeginsWith, value);
				if (!IsValidationSuspended)
				{
					ValidateAccountsOrderBeginsWith();
				}
				AccountsOrderEndsWithInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AccountsOrderBeginsWithInfo
		{
			get { return GetZPropertyInfo(Schema.AccountsOrderBeginsWith); }
		}

		public void ValidateAccountsOrderBeginsWith()
		{
			AccountsOrderBeginsWithInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(AccountsOrderBeginsWithInfo);
			ListValidation.ErrorIfInvalidCode(AccountsOrderBeginsWithInfo, AccountOrderTypeList);
		}

		ZString accountsOrderBeginsWith;

		#endregion

		#region Accounts Order Ends With

		[MaxLength(15)]
		[List("AccountOrderTypeList")]
		public ZString AccountsOrderEndsWith
		{
			get
			{
				ZString result = ZString.Empty;

				if (AccountsOrderBeginsWith == AccountOrderTypeList[0].Code)
				{
					result = AccountOrderTypeList[1].Code;
				}
				else if (AccountsOrderBeginsWith == AccountOrderTypeList[1].Code)
				{
					result = AccountOrderTypeList[0].Code;
				}

				return result;
			}
		}

		public ZPropertyInfo AccountsOrderEndsWithInfo
		{
			get { return GetZPropertyInfo(Schema.AccountsOrderEndsWith); }
		}

		#endregion

		#region GL Account First Report Starts From

		[List("AccountDescriptorList")]
		public ZGuid GLAccountFirstReportStartsFrom
		{
			get
			{
				if (fGLAccountFirstReportStartsFrom.IsEmpty)
				{
					var filter = new ZQuery(AccountDescriptorList.CompleteFilter);
					Type accountType = typeof(AccGLAccountDescriptor);

					filter.OrderBy = AccGLAccountDescriptorSchema.AJ_LocalAccountNumber.Name + " ASC";
					BusinessObject firstAccount = CurrentFactory.LoadTop1(accountType, filter);
					fGLAccountFirstReportStartsFrom = firstAccount != null ? firstAccount.PK : ZGuid.Empty;
				}
				return fGLAccountFirstReportStartsFrom;
			}
		}

		ZGuid fGLAccountFirstReportStartsFrom;

		public ZPropertyInfo GLAccountFirstReportStartsFromInfo
		{
			get { return GetZPropertyInfo(Schema.GLAccountFirstReportStartsFrom); }
		}

		#endregion

		#region GL Account Second Report Starts From

		[List("AccountDescriptorList")]
		public ZGuid GLAccountSecondReportStartsFrom
		{
			get { return fGLAccountSecondReportStartsFrom; }
			set
			{
				SetNonPersistentPropertyValue(GLAccountSecondReportStartsFromInfo, ref fGLAccountSecondReportStartsFrom, value);
				if (!IsValidationSuspended)
				{
					ValidateGLAccountSecondReportStartsFrom();
				}
			}
		}

		public ZPropertyInfo GLAccountSecondReportStartsFromInfo
		{
			get { return GetZPropertyInfo(Schema.GLAccountSecondReportStartsFrom); }
		}

		public void ValidateGLAccountSecondReportStartsFrom()
		{
			GLAccountSecondReportStartsFromInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(GLAccountSecondReportStartsFromInfo);

#if DEBUG
			if (!DoNotPerformListValidationOnGLAccountSecondReportStartsFrom)
#endif
			{
				ListValidation.ErrorIfInvalidPK(GLAccountSecondReportStartsFromInfo, AccountDescriptorList);
			}

			if (!GLAccountSecondReportStartsFrom.IsEmpty &&
				GLAccountSecondReportStartsFrom == GLAccountFirstReportStartsFrom)
			{
				if (AccountsOrderBeginsWith == "ProfitAndLoss")
				{
					GLAccountSecondReportStartsFromInfo.AddError(Res.GetString("B18C1FE6-4D32-489E-9910-8D249806392C",
@"The account you have selected is the first multi language account mapping, when multi language accounts are sorted in ascending order.
This account will be used when running the multi language ‘Profit and Loss’ report.
You should select the account that should be used as the starting point when running the multi language ‘Balance Sheet’ report.
You cannot select the first multi language account mapping in this field."));
				}
				else if (AccountsOrderBeginsWith == "BalanceSheet")
				{
					GLAccountSecondReportStartsFromInfo.AddError(Res.GetString("F55E00C7-692D-4B39-BFD6-682E5A12FB0C",
@"The account you have selected is the first multi language account mapping, when multi language accounts are sorted in ascending order.
This account will be used when running the multi language ‘Balance Sheet’ report.
You should select the account that should be used as the starting point when running the multi language ‘Profit and Loss’ report.
You cannot select the first multi language account mapping in this field."));
				}
			}
		}

		ZGuid fGLAccountSecondReportStartsFrom;

#if DEBUG
		internal bool DoNotPerformListValidationOnGLAccountSecondReportStartsFrom;
#endif

		#endregion

		#endregion

		#region Lookups

		#region Language List

		public CodeDescriptionPairList LanguageList
		{
			get
			{
				if (fLanguageList == null)
				{
					fLanguageList = new CodeDescriptionPairList(OLookUpEditType.GLLanguage);
				}
				return fLanguageList;
			}
		}

		CodeDescriptionPairList fLanguageList;

		#endregion

		#region Account Order Type List

		public CodeDescriptionPairList AccountOrderTypeList
		{
			get
			{
				if (fAccountOrderTypeList == null)
				{
					fAccountOrderTypeList = new CodeDescriptionPairList(OLookUpEditType.AccountOrderType);
				}

				return fAccountOrderTypeList;
			}
		}

		CodeDescriptionPairList fAccountOrderTypeList;

		#endregion

		#region Account Descriptor List

		public BusinessObjectCollection AccountDescriptorList
		{
			get
			{
				if (fAccountDescriptorList == null)
				{
					ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, AccountTypeComboBoxConstants.Header);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, Language);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, CountryCode);
					fAccountDescriptorList = new AccGLAccountDescriptorCollection(CurrentFactory, filter);
					fAccountDescriptorList.Load();
				}

				return fAccountDescriptorList;
			}
		}

		BusinessObjectCollection fAccountDescriptorList;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Language, Language);
			writer.WriteElementString(Schema.CountryCode, CountryCode);
			writer.WriteElementString(Schema.AccountsOrderBeginsWith, AccountsOrderBeginsWith);
			writer.WriteElementString(Schema.GLAccountSecondReportStartsFrom, GLAccountSecondReportStartsFrom.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Language = reader.ReadElementString(Schema.Language);
			CountryCode = reader.ReadElementString(Schema.CountryCode);
			AccountsOrderBeginsWith = reader.ReadElementString(Schema.AccountsOrderBeginsWith);
			GLAccountSecondReportStartsFrom = new ZGuid(reader.ReadElementString(Schema.GLAccountSecondReportStartsFrom));
		}

		#endregion
	}
}
