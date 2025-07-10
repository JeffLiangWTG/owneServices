using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.ReportingBook.AlternateGLAccountWithAttribute.Testing
{
	[TestedType(typeof(AlternateGLAccountWithAttributeDataAdapter))]
	sealed class AlternateGLAccountWithAttributeDataAdapterTest : ValueObjectDataAdapterTest<BusinessObjectThatDoesntSave, Xsd.AlternateGLAccounts>
	{
		public void TestSetAlternateChartAndCompany()
		{
			var nonGlobalChart = Creator.CreateAlternateChart("222", "Description", true, false, BalanceSheetStyleCode.EAL);
			Factory.Save();

			XsdAlternateGLAccount.ChartCode = "";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Chart Code cannot be empty.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.ChartCode = "11";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Chart Code '11'.", notifications.AsString);

			notifications.Clear();
			XsdAlternateGLAccount.ChartCode = "111";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);

			AccAlternateGLAccount = Factory.LoadTop1<AccAlternateGLAccount>(new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, Chart.PK));
			AssertNotNull(AccAlternateGLAccount);
			AssertEquals(ZGuid.Empty, AccAlternateGLAccount.CompanyPK);

			XsdAlternateGLAccount.ChartCode = "222";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			AccAlternateGLAccount = Factory.LoadTop1<AccAlternateGLAccount>(new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, nonGlobalChart.PK));
			AssertEquals(GlbCompany.CurrentCompany.PK, AccAlternateGLAccount.CompanyPK);
		}

		public void TestSetAccountNum_IsFixedLength()
		{
			XsdAlternateGLAccount.AccountNum = "111222";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			AccAlternateGLAccount = Factory.LoadTop1<AccAlternateGLAccount>(new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, Chart.PK));
			AssertNotNull(AccAlternateGLAccount);
			AccAlternateGLAccount.Validation.ValidateAGA_AccountNum();
			AssertNoErrors(AccAlternateGLAccount.AGA_AccountNumInfo);

			XsdAlternateGLAccount.ParentAccount = "";
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Alternate;
			XsdAlternateGLAccount.AccountNum = "";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			AssertContains("Please enter an Alternate Account.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountNum = "12";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertContains("The length of Account Number is invalid. Please enter the number with 6 characters.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountNum = "12x111";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertContains("The Account Number does not matched up with the Account Format '999999' which is set in Chart 111.", notifications.AsString);
		}

		public void TestSetAccountNum_IsNotFixedLength()
		{
			Chart.AAC_IsFixedLength = false;
			Factory.Save();

			XsdAlternateGLAccount.AccountNum = "1122";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			AccAlternateGLAccount = Factory.LoadTop1<AccAlternateGLAccount>(new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, Chart.PK));
			AssertNotNull(AccAlternateGLAccount);
			AssertNoErrors(AccAlternateGLAccount.AGA_AccountNumInfo);

			XsdAlternateGLAccount.AccountNum = "11";
			XsdAlternateGLAccount.ParentAccount = "";
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Alternate;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("The length of Account Number is invalid. Please enter the number with 4, or 6 characters.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountNum = "000021";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertContains(string.Empty, notifications.AsString);
		}

		public void TestMoreThanOneNTEGLAccountsCannotBeAllowedToMapToOneAlternateGLAccount()
		{
			var glHeader = Creator.CreateAccGLHeader("3410.13.22", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.Note, Core.Constants.DebitCredit.Credit);
			var glHeader2 = Creator.CreateAccGLHeader("3410.13.23", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.Note, Core.Constants.DebitCredit.Credit);
			Factory.Save();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Note;
			XsdAlternateGLAccount.ParentAccount = glHeader.AG_AccountNum;
			Creator.CreateAlternateGlAccountForCSVImport(Value.AlternateGLAccount, glHeader2.AG_AccountNum, "311010", Core.Constants.AccountType.Note, Chart.AAC_Code, "test", Core.Constants.DebitCredit.Debit,
				AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0, 5);
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			AssertContains("The specified Alternate Account's Number '311010' already existed and cannot be linked to a different Parent Account as NTE Alternate GL Account cannot be mapped from multiple NTE Parent Accounts.\r\nPlease enter a different value.", notifications.AsString);
		}

		public void TestControlAccountCannotBeMappedToAlternateGLAccountWhichIsMappedByMultipleGLAccounts()
		{
			var accounting = ObjectFactory.Get<IAccounting>();
			var list = new List<IRegistryItem>();
			list.AddRange(AccountingMasterFilesUtils.NotAllowedForDissectionControlAccount);
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "311010", Core.Constants.AccountType.BalanceSheetAccount, description: "test", drCR: Core.Constants.DebitCredit.Debit, reportSection: AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, totalLevel: 0, printSequence: 5);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			var glHeader = Creator.CreateAccGLHeader("3330.13.23", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			Factory.Save();
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);

			for (var i = 0; i < list.Count; i++)
			{
				var controlAccount = list[i];
				using (controlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader.PK.ToGuid()))
				{
					notifications.Clear();
					XsdAlternateGLAccount.ParentAccount = glHeader.AG_AccountNum;
					Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
					Assert(notifications.HasErrors);
					AssertEquals("Error: Row No.1 - Account Number '311010' - The specified Alternate Account's Number '311010' already existed in the Alternate Chart '111' and is linked to Parent Account used in the system registries or bank accounts that do not allow dissection. It cannot be mapped to multiple Parent Accounts.\r\nPlease enter a different value.\r\n", notifications.AsString);

					alternateGLAccount.AlternateGLAccountAttributes.Where(x => x.AAA_AG_GLHeader == glHeader.PK).DeleteAll();
					Factory.Save();
				}
			}
		}

		public void TestControlAccountCannotBeMappedToAlternateGLAccountWhichIsMappedByMultipleGLAccounts2()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "311010", Core.Constants.AccountType.BalanceSheetAccount, description: "test", drCR: Core.Constants.DebitCredit.Debit, reportSection: AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, totalLevel: 0, printSequence: 5);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			var glHeader = Creator.CreateAccGLHeader("3330.13.23", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			Factory.Save();
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForDissectionAttributes(It.IsAny<ZGuid>())).Returns(true);
			Validate();

			mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForSeparateNumbering(It.IsAny<ZGuid>())).Returns(true);
			Validate();

			mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForSeparateNumbering(It.IsAny<ZGuid>())).Returns(false);
			mock.Setup(m => m.IsNotAllowedForDissectionAttributes(It.IsAny<ZGuid>())).Returns(false);
			Validate(false);

			void Validate(bool expectError = true)
			{
				using (ObjectFactory.Substitute(mock.Object))
				{
					notifications.Clear();
					XsdAlternateGLAccount.ParentAccount = glHeader.AG_AccountNum;
					Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);

					if (expectError)
					{
						Assert(notifications.HasErrors);
						AssertEquals("Error: Row No.1 - Account Number '311010' - The specified Alternate Account's Number '311010' already existed in the Alternate Chart '111' and is linked to Parent Account used in the system registries or bank accounts that do not allow dissection. It cannot be mapped to multiple Parent Accounts.\r\nPlease enter a different value.\r\n", notifications.AsString);
					}
					else
					{
						Assert(!notifications.HasErrors);
					}	

					alternateGLAccount.AlternateGLAccountAttributes.Where(x => x.AAA_AG_GLHeader == glHeader.PK).DeleteAll();
					Factory.Save();
				}
			}
		}

		public void TestGLAccountCannotMapToAlternateGLAccountWhichLinkedToBankAccount()
		{
			Creator.CreateBankAccount("AAA", "AAA", Creator.GetCurrency("CNY"), GLHeader);
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "311010", Core.Constants.AccountType.BalanceSheetAccount, description: "test", drCR: Core.Constants.DebitCredit.Debit, reportSection: AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, totalLevel: 0, printSequence: 5);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, attribute: "");
			Factory.Save();
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);

			var glHeader = Creator.CreateAccGLHeader("3330.13.23", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			Factory.Save();
			XsdAlternateGLAccount.ParentAccount = glHeader.AG_AccountNum;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			Assert(notifications.HasErrors);
			AssertEquals("Error: Row No.1 - Account Number '311010' - The specified Alternate Account's Number '311010' already existed in the Alternate Chart '111' and is linked to Parent Account used in the system registries or bank accounts that do not allow dissection. It cannot be mapped to multiple Parent Accounts.\r\nPlease enter a different value.\r\n", notifications.AsString);
		}

		public void TestNAVIsAllowedToSetToAttribute()
		{
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);
			XsdAlternateGLAccount.OCGAttrValue = AccountingMasterFilesConstants.NAV.Code;
			XsdAlternateGLAccount.LFEAttrValue = AccountingMasterFilesConstants.NAV.Code;

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);

			AssertAttributeVaueAndOrgID(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, AccountingMasterFilesConstants.NAV.Code, null);
			AssertAttributeVaueAndOrgID(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, AccountingMasterFilesConstants.NAV.Code, null);
		}

		public void TestSetAccountType()
		{
			XsdAlternateGLAccount.AccountType = "";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Type cannot be empty.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountType = "12";
			XsdAlternateGLAccount.AccountNum = "1";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Account Type. A valid account type must be one of the values: BSH, P&L, TTL, HDR, CLN, ALT,NTE,CHT,RUP,GRP.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountType = "BSH";
			XsdAlternateGLAccount.AccountNum = "2";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("Invalid Account Type. A valid account type must be one of the values: BSH, P&L, TTL, HDR, CLN, ALT,NTE,CHT,RUP,GRP.", notifications.AsString);
		}

		public void TestSetAccountName()
		{
			XsdAlternateGLAccount.AccountName = "";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Please enter an Alternate Account Name.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountName = "12";
			XsdAlternateGLAccount.AccountNum = "1";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("Please enter an Alternate Account Name.", notifications.AsString);

			XsdAlternateGLAccount.AccountName = "128 characters 128 characters 128 characters 128 characters 128 characters 128 characters 128 characters 128 characters 128 characters";
			XsdAlternateGLAccount.AccountNum = "2";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertContains("Row No.1 - Account Number '2' - Name '128 characters 128 characters 128 characters 128 characters 128 characters 128 characters 128 characters 128 characters 128 characters' exceeds the maximum length 128 characters.", notifications.AsString);
		}

		public void TestSetDebitCredit()
		{
			XsdAlternateGLAccount.DebitCredit = "";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Debit/Credit cannot be empty.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.DebitCredit = "12";
			XsdAlternateGLAccount.AccountNum = "1";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Debit/Credit. A valid Debit/Credit should be one of the values: DR or CR.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.DebitCredit = "DR";
			XsdAlternateGLAccount.AccountNum = "2";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("Invalid Debit/Credit. A valid Debit/Credit should be one of the values: DR or CR.", notifications.AsString);
		}

		public void TestSetReportSection()
		{
			XsdAlternateGLAccount.ReportSection = "";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Report Section cannot be empty.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.ReportSection = "re";
			XsdAlternateGLAccount.AccountNum = "1";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Report Section. A valid Report Section must be one of the values:TS,OV,AP,OE,AS,LI", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.ReportSection = AccGLHeader.Constants.SectionTypes.Codes.TradingStatement;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("Invalid Report Section. A valid Report Section must be one of the values:TS,OV,AP,OE,AS,LI", notifications.AsString);
		}

		public void TestSetTotalLevel()
		{
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Total;
			XsdAlternateGLAccount.TotalLevel = 0;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Total Level. Value must be 1 to 999.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Header;
			XsdAlternateGLAccount.TotalLevel = 21;
			XsdAlternateGLAccount.AccountNum = "31203202";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("A Total Level can only be specified if the Account Type is TTL.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.TotalLevel = 0;
			XsdAlternateGLAccount.AccountNum = "3";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("A Total Level can only be specified if the Account Type is TTL.", notifications.AsString);
		}

		public void TestSetPercentNum()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "123456", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0);
			Factory.Save();

			XsdAlternateGLAccount.PercentNum = "123456";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Percent Account. A valid Percent Account must be 'CLN' or ''TTL' account type.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.PercentNum = "12x213";
			XsdAlternateGLAccount.AccountNum = "1";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Percent Account. A valid Percent Account must be 'CLN' or ''TTL' account type.", notifications.AsString);
			notifications.Clear();

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.Consolidation;
			Factory.Save();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Header;
			XsdAlternateGLAccount.AccountNum = "2";
			XsdAlternateGLAccount.PercentNum = "123456";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("A Percent Account can only be specified if the Account Type is ALT, BSH or P&L.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.AccountNum = "3";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("A Percent Account can only be specified if the Account Type is ALT, BSH or P&L.", notifications.AsString);
		}

		public void TestSetConsolidationNum()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "123456", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0);
			Factory.Save();

			XsdAlternateGLAccount.ConsolidationNum = "123456";
			XsdAlternateGLAccount.AccountNum = "1";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Consolidate Account. A valid Consolidate Account must be 'CLN' account type.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.ConsolidationNum = "12x213";
			XsdAlternateGLAccount.AccountNum = "2";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Consolidate Account. A valid Consolidate Account must be 'CLN' account type.", notifications.AsString);
			notifications.Clear();

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.Consolidation;
			Factory.Save();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Header;
			XsdAlternateGLAccount.ConsolidationNum = "123456";
			XsdAlternateGLAccount.AccountNum = "3";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("A Consolidate Account can only be specified if the Account Type is ALT, BSH, TTL or P&L.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.AccountNum = "4";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("A Consolidate Account can only be specified if the Account Type is ALT, BSH, TTL or P&L.", notifications.AsString);

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Total;
			XsdAlternateGLAccount.AccountNum = "4";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("A Consolidate Account can only be specified if the Account Type is ALT, BSH, TTL or P&L.", notifications.AsString);
		}

		public void TestSetAlternateNum()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "123456", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0);
			Factory.Save();

			XsdAlternateGLAccount.AlternateNum = "123456";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Alternate Number. A valid Alternate Number must be 'ALT' account type.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AlternateNum = "12x213";
			XsdAlternateGLAccount.AccountNum = "1";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Alternate Number. A valid Alternate Number must be 'ALT' account type.", notifications.AsString);
			notifications.Clear();

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.Alternate;
			Factory.Save();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Header;
			XsdAlternateGLAccount.AlternateNum = "123456";
			XsdAlternateGLAccount.AccountNum = "2";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("An Alternate Number can only be specified if the Account Type is 'BSH'.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.AccountNum = "3";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("An Alternate Number can only be specified if the Account Type is 'BSH'.", notifications.AsString);
		}

		public void TestDisplayCannotBeSpecifiedMessageWhenParentAccountHaveNoDissectionButHasAttributeValue()
		{
			var dissection = Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, false);
			Factory.Save();

			XsdAlternateGLAccount.LFEAttrValue = "LOC";
			XsdAlternateGLAccount.ORGAttrValue = "LOC";
			XsdAlternateGLAccount.LFOAttrValue = "LOC";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Row No.1 - Account Number '311010' - LFE attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
			AssertContains("Row No.1 - Account Number '311010' - ORG attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
			AssertContains("Row No.1 - Account Number '311010' - LFO attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
			notifications.Clear();

			dissection.ADC_SeparateNumbering = true;
			Factory.Save();
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("Row No.1 - Account Number '311010' - LFO attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
		}

		public void TestAlternamteAccountNumCanDisplayMessageWhenExistOtherError()
		{
			XsdAlternateGLAccount.AccountNum = "X";
			XsdAlternateGLAccount.TotalLevel = 1;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Row No.1 - Account Number 'X' - A Total Level can only be specified if the Account Type is TTL.", notifications.AsString);
			AssertContains("Row No.1 - Account Number 'X' - AGA_AccountNum: The length of Account Number is invalid. Please enter the number with 6 characters.", notifications.AsString);
		}

		public void TestSetTotalReference()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "123456", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0);
			Factory.Save();

			XsdAlternateGLAccount.TotalReference = "123456";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Total Reference. A valid Total Reference should be 'TTL' account type.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.TotalReference = "12x213";
			XsdAlternateGLAccount.AccountNum = "31203202";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Total Reference. A valid Total Reference should be 'TTL' account type.", notifications.AsString);
			notifications.Clear();

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.Total;
			Factory.Save();

			XsdAlternateGLAccount.TotalReference = "123456";
			XsdAlternateGLAccount.AccountNum = "3456";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Only when the Account Type is 'HDR', Total Reference is allowed.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Header;
			XsdAlternateGLAccount.AccountNum = "3";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("Only when the Account Type is 'HDR', Total Reference is allowed.", notifications.AsString);
		}

		public void TestParentAccount()
		{
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Alternate;
			XsdAlternateGLAccount.ParentAccount = "";
			XsdAlternateGLAccount.LFOAttrValue = "LOC";

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("The attribute value cannot be specified for account type ALT, CLN, HDR, TTL, CHT, RUP, GUP.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.LFOAttrValue = "";
			XsdAlternateGLAccount.AccountNum = "222444";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
			var query = new ZQuery();
			query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Alternate);
			query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, XsdAlternateGLAccount.AccountNum);
			AssertEquals(1, Factory.Load<AccAlternateGLAccount>(query).Length);

			XsdAlternateGLAccount.ParentAccount = "1";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Parent Account must be empty.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.ParentAccount = "";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Parent Account cannot be empty.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.ParentAccount = "3420.13.21";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Parent Account. Parent Account 3420.13.21 is not existing.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.ParentAccount = GLHeader.AG_AccountNum;
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Parent Account. The Type of Parent Account 3410.13.21 is NOT 'P&L'.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.AccountNum = "123456";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
		}

		public void TestRepeatedlyImportAlternateGLAccountWithParentAccount()
		{
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			Factory.Save();

			var second = Value.AlternateGLAccount.AddNew();
			second.ParentAccount = GLHeader.AG_AccountNum;
			second.AccountNum = "311010";
			second.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			second.ChartCode = Chart.AAC_Code;
			second.DebitCredit = "DR";
			second.AccountName = "name";
			second.ReportSection = "TS";
			second.LFOAttrValue = "LOC";
			XsdAlternateGLAccount.LFOAttrValue = "LOC";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Row No.2 - The Alternate account number '311010' mapped to Parent Account '3410.13.21' for the Chart '111' has already been existing.", notifications.AsString);

			second.AccountNum = "434343";
			XsdAlternateGLAccount.AccountNum = "676767";
			notifications.Clear();
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
		}

		public void TestRepeatedlyImportAlternateGLAccountWithoutParentAccount()
		{
			var second = Value.AlternateGLAccount.AddNew();
			second.AccountNum = "311010";
			second.ChartCode = Chart.AAC_Code;
			second.AccountType = Core.Constants.AccountType.Alternate;
			second.DebitCredit = "DR";
			second.AccountName = "name";
			second.ReportSection = "TS";
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Alternate;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Row No.2 - The Alternate account number '311010' for the Chart '111' has already been existing.", notifications.AsString);

			second.AccountNum = "434343";
			XsdAlternateGLAccount.AccountNum = "676767";
			XsdAlternateGLAccount.ParentAccount = "";
			notifications.Clear();
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
		}

		public void TestImportAttribtuteWithoutAttributeValue()
		{
			var glHeader2 = Creator.CreateAccGLHeader("3410.13.22", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.DebitCredit.Credit);
			var glHeader3 = Creator.CreateAccGLHeader("3410.13.23", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.Note, Core.Constants.DebitCredit.Credit);
			glHeader3.AG_StatisticalUnits = "KG";
			glHeader3.AG_CashFlowType = "XXX";
			Factory.Save();

			XsdAlternateGLAccount.LFOAttrValue = "";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			XsdAlternateGLAccount.AccountNum = "111222";
			XsdAlternateGLAccount.ParentAccount = glHeader2.AG_AccountNum;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.Note;
			XsdAlternateGLAccount.AccountNum = "222333";
			XsdAlternateGLAccount.ParentAccount = glHeader3.AG_AccountNum;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);

			var query = new ZQuery();
			query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, "311010");
			query.AddToFilter(JoinCondition.Or, AccAlternateGLAccountSchema.AGA_AccountNum, "111222");
			query.AddToFilter(JoinCondition.Or, AccAlternateGLAccountSchema.AGA_AccountNum, "222333");
			var account = Factory.Load<AccAlternateGLAccount>(query);
			AssertEquals(3, account.Length);

			query = new ZQuery();
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, Chart.PK);
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_Attribute, "");
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_Value, "");
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_Sequence, 1);
			var attribtues = Factory.Load<AccAlternateGLAccountAttribute>(query);
			AssertEquals(3, attribtues.Length);
		}

		public void TestSequenceIsCorrectWhenImportAttributeAfterDeleteAttribute()
		{
			var dissection = Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			Factory.Save();
			Creator.CreateAlternateGlAccountForCSVImport(Value.AlternateGLAccount, GLHeader.AG_AccountNum, "111222", Core.Constants.AccountType.BalanceSheetAccount, Chart.AAC_Code, "test", Core.Constants.DebitCredit.Debit, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement,
				0, 4, lfoAttrValue: "LOC");
			XsdAlternateGLAccount.LFOAttrValue = "LOC";

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);

			var accountQuery = new ZQuery();
			accountQuery.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_Sequence, 1);
			var sequenceIsOne = Factory.Load<AccAlternateGLAccountAttribute>(accountQuery);
			AssertEquals(1, sequenceIsOne.Length);

			accountQuery = new ZQuery();
			accountQuery.AddToFilter(JoinCondition.Or, AccAlternateGLAccountAttributeSchema.AAA_Sequence, 2);
			AssertEquals(1, Factory.Load<AccAlternateGLAccountAttribute>(accountQuery).Length);

			sequenceIsOne.DeleteAll();
			Value.AlternateGLAccount.Clear();
			Creator.CreateAlternateGlAccountForCSVImport(Value.AlternateGLAccount, GLHeader.AG_AccountNum, "333222", Core.Constants.AccountType.BalanceSheetAccount, Chart.AAC_Code, "test", Core.Constants.DebitCredit.Debit, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement,
				0, 4, lfoAttrValue: "LOC");
			notifications.Clear();

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			accountQuery = new ZQuery();
			accountQuery.AddToFilter(JoinCondition.Or, AccAlternateGLAccountAttributeSchema.AAA_Sequence, 3);
			AssertEquals(1, Factory.Load<AccAlternateGLAccountAttribute>(accountQuery).Length);
		}

		public void TestCashFlowTypeAndUnits()
		{
			var alternateGlAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "123456", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGlAccount, GLHeader.PK);
			var gLHeader2 = Creator.CreateAccGLHeader("3410.13.22", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit, CashFlowCodeLists.Codes.F07, "U");
			Factory.Save();

			XsdAlternateGLAccount.ChartCode = Chart.AAC_Code;
			XsdAlternateGLAccount.AccountNum = alternateGlAccount.AGA_AccountNum;
			XsdAlternateGLAccount.ParentAccount = gLHeader2.AccountNum;
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Cash Flow Type as mapped multiple Parent Accounts have different Cash Flow Type", notifications.AsString);
			AssertContains("Invalid Units as mapped multiple Parent Accounts have different Units.", notifications.AsString);

			gLHeader2.AG_CashFlowType = "";
			gLHeader2.AG_StatisticalUnits = "";
			Factory.Save();

			notifications.Clear();
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("Invalid Cash Flow Type as mapped multiple Parent Accounts have different Cash Flow Type", notifications.AsString);
			AssertNotContains("Invalid Units as mapped multiple Parent Accounts have different Units.", notifications.AsString);
		}

		public void TestGLAccountFields()
		{
			var alternateglAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "123456", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0);
			Factory.Save();

			XsdAlternateGLAccount.ChartCode = "111";
			XsdAlternateGLAccount.AccountNum = "123456";
			XsdAlternateGLAccount.ParentAccount = GLHeader.AG_AccountNum;
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			XsdAlternateGLAccount.AccountName = "name";
			XsdAlternateGLAccount.DebitCredit = "CR";
			XsdAlternateGLAccount.ReportSection = AccGLHeader.Constants.SectionTypes.Codes.Liabilities;
			XsdAlternateGLAccount.TotalLevel = 1;
			XsdAlternateGLAccount.PrintSequence = 1;
			XsdAlternateGLAccount.PercentNum = "111111";
			XsdAlternateGLAccount.ConsolidationNum = "222222";
			XsdAlternateGLAccount.AlternateNum = "333333";
			XsdAlternateGLAccount.TotalReference = "444444";

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Account Type as mapped Alternate GL Account have different Account Type.", notifications.AsString);
			AssertContains("Invalid Account Name as mapped Alternate GL Account have different Account Name.", notifications.AsString);
			AssertContains("Invalid Debit/Credit as mapped Alternate GL Account have different Debit/Credit.", notifications.AsString);
			AssertContains("Invalid Report Section as mapped Alternate GL Account have different Report Section.", notifications.AsString);
			AssertContains("Invalid Total Level as mapped Alternate GL Account have different Total Level.", notifications.AsString);
			AssertContains("Invalid Print Sequence as mapped Alternate GL Account have different Print Sequence.", notifications.AsString);
			AssertContains("Invalid Percent Number as mapped Alternate GL Account have different Percent Number.", notifications.AsString);
			AssertContains("Invalid Consolidate as mapped Alternate GL Account have different Consolidate.", notifications.AsString);
			AssertContains("Invalid Alternate Number as mapped Alternate GL Account have different Alternate Number.", notifications.AsString);
			AssertContains("Invalid Total Reference as mapped Alternate GL Account have different Total Reference.", notifications.AsString);

			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.AccountName = "desc";
			XsdAlternateGLAccount.DebitCredit = "DR";
			XsdAlternateGLAccount.ReportSection = AccGLHeader.Constants.SectionTypes.Codes.TradingStatement;
			XsdAlternateGLAccount.TotalLevel = 0;
			XsdAlternateGLAccount.PrintSequence = 0;
			XsdAlternateGLAccount.PercentNum = "";
			XsdAlternateGLAccount.ConsolidationNum = "";
			XsdAlternateGLAccount.AlternateNum = "";
			XsdAlternateGLAccount.TotalReference = "";
			notifications.Clear();
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertNotContains("Invalid Account Type as mapped Alternate GL Account have different Account Type.", notifications.AsString);
			AssertNotContains("Invalid Account Name as mapped Alternate GL Account have different Account Name.", notifications.AsString);
			AssertNotContains("Invalid Debit/Credit as mapped Alternate GL Account have different Debit/Credit.", notifications.AsString);
			AssertNotContains("Invalid Report Section as mapped Alternate GL Account have different Report Section.", notifications.AsString);
			AssertNotContains("Invalid Total Level as mapped Alternate GL Account have different Total Level.", notifications.AsString);
			AssertNotContains("Invalid Print Sequence as mapped Alternate GL Account have different Print Sequence.", notifications.AsString);
			AssertNotContains("Invalid Percent Number as mapped Alternate GL Account have different Percent Number.", notifications.AsString);
			AssertNotContains("Invalid Consolidate as mapped Alternate GL Account have different Consolidate.", notifications.AsString);
			AssertNotContains("Invalid Alternate Number as mapped Alternate GL Account have different Alternate Number.", notifications.AsString);
			AssertNotContains("Invalid Total Reference as mapped Alternate GL Account have different Total Reference.", notifications.AsString);
		}

		public void TestImportedChartMustBeCurrentCompanyChartOrGlobalChart()
		{
			var nonCurrentCompanyChart = Creator.CreateAlternateChart("TCC");
			nonCurrentCompanyChart.AAC_GC_Company = Creator.NonCurrentCompany.PK;

			XsdAlternateGLAccount.ChartCode = nonCurrentCompanyChart.AAC_Code;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Row No.1 - Account Number '311010' - Invalid Chart Code 'TCC'.", notifications.AsString);
			notifications.Clear();

			nonCurrentCompanyChart.AAC_GC_Company = GlbCompany.CurrentCompany.PK;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			AssertNotContains("Row No.1 - Account Number '311010' - Invalid Chart Code 'TCC'.", notifications.AsString);

			XsdAlternateGLAccount.ChartCode = Chart.AAC_Code;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			AssertNotContains("Row No.1 - Account Number '311010' - Invalid Chart Code '111'.", notifications.AsString);
		}

		public void TestORGAttribute()
		{
			var dissection = Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, true);
			Factory.Save();

			XsdAlternateGLAccount.ChartCode = "111";
			XsdAlternateGLAccount.AccountNum = "123456";
			XsdAlternateGLAccount.ParentAccount = GLHeader.AG_AccountNum;
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.ORGAttrValue = "";

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - ORG attribute value must be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.ORGAttrValue = "123";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Attribute Value. A valid Attribute Value must be the code of a valid Receivable Organization (if Parent Account is an AR Control Account) or a valid Payable Organization (if Parent Account is an AP Control Account).", notifications.AsString);
			notifications.Clear();

			var org = Creator.CreateOrgHeader("org1", false, true);
			org.CompanyData.OB_IsDebtor = false;
			Factory.Save();
			notifications.Clear();

			XsdAlternateGLAccount.ORGAttrValue = org.OH_Code;

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			AssertContains("Invalid Attribute Value. A valid Attribute Value must be the code of a valid Receivable Organization (if Parent Account is an AR Control Account) or a valid Payable Organization (if Parent Account is an AP Control Account).", notifications.AsString);
			notifications.Clear();

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader.PK.ToGuid());
			org.CompanyData.OB_IsDebtor = true;
			Factory.Save();
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
			AssertAttributeVaueAndOrgID("ORG", ZString.Empty, org.PK);

			dissection.ADC_Attribute = "OCG";
			Factory.Save();

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - ORG attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
		}

		public void TestOCGAttribute()
		{
			var dissection = Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);
			Factory.Save();

			XsdAlternateGLAccount.ChartCode = "111";
			XsdAlternateGLAccount.AccountNum = "123456";
			XsdAlternateGLAccount.ParentAccount = GLHeader.AG_AccountNum;
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.OCGAttrValue = "";

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - OCG attribute value must be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.OCGAttrValue = "123";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Attribute Value. A valid Attribute Value must be one of the following Class defined in Consolidated Accounting Category List registry or value 'NAV'.", notifications.AsString);
			notifications.Clear();

			var defaultValue = new ConsolidatedAccountingCategoryCollection();
			defaultValue.Add(Enterprise.Core.Constants.AccountsCategory.Unrelated, (NoResString)"Unrelated company", ConsolidatedAccountingCategoryClassList.Codes.ThirdParty);

			AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
			XsdAlternateGLAccount.OCGAttrValue = ConsolidatedAccountingCategoryClassList.Codes.ThirdParty;
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
			AssertAttributeVaueAndOrgID("OCG", ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, null);

			dissection.ADC_Attribute = "ORG";
			Factory.Save();

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - OCG attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
		}

		public void TestLFOAttribute()
		{
			var dissection = Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			Factory.Save();

			XsdAlternateGLAccount.ChartCode = "111";
			XsdAlternateGLAccount.AccountNum = "123456";
			XsdAlternateGLAccount.ParentAccount = GLHeader.AG_AccountNum;
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.LFOAttrValue = "";

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - LFO attribute value must be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.LFOAttrValue = "123";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC' or 'FOR' or 'NAV'.\r\nLOC = 'Local'.\r\nFOR = 'Foreign'.\r\nNAV = No Attribute Value.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.LFOAttrValue = "LOC";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
			AssertAttributeVaueAndOrgID("LFO", "LOC", null);

			dissection.ADC_Attribute = "ORG";
			Factory.Save();

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - LFO attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
		}

		public void TestLFEAttribute()
		{
			var dissection = Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);
			Factory.Save();

			XsdAlternateGLAccount.ChartCode = "111";
			XsdAlternateGLAccount.AccountNum = "123456";
			XsdAlternateGLAccount.ParentAccount = GLHeader.AG_AccountNum;
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.LFEAttrValue = "";

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - LFE attribute value must be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.LFEAttrValue = "123";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC', 'WEU' or 'OEU' or 'NAV'.\r\nLOC = Local.\r\nWEU = Within EU.\r\nOEU = Outside EU.\r\nNAV = No Attribute Value.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.LFEAttrValue = "WEU";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
			AssertAttributeVaueAndOrgID("LFE", "WEU", null);

			dissection.ADC_Attribute = "ORG";
			Factory.Save();

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - LFE attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
		}

		public void TestTICAttribute()
		{
			var dissection = Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, true);
			Factory.Save();

			XsdAlternateGLAccount.ChartCode = "111";
			XsdAlternateGLAccount.AccountNum = "123456";
			XsdAlternateGLAccount.ParentAccount = GLHeader.AG_AccountNum;
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.TICAttrValue = "";

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - TIC attribute value must be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.TICAttrValue = "123";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'STI' or 'ETI' or 'NAV'.\r\nSTI = Standard Tax IDs.\r\nETI = Tax ID with Extra Tax.\r\nNAV = No Attribute Value.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.TICAttrValue = "STI";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
			AssertAttributeVaueAndOrgID("TIC", "STI", null);

			dissection.ADC_Attribute = "ORG";
			Factory.Save();

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - TIC attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
		}

		public void TestSPRAttribute()
		{
			var dissection = Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, true);
			Factory.Save();

			XsdAlternateGLAccount.ChartCode = "111";
			XsdAlternateGLAccount.AccountNum = "123456";
			XsdAlternateGLAccount.ParentAccount = GLHeader.AG_AccountNum;
			XsdAlternateGLAccount.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			XsdAlternateGLAccount.SPRAttrValue = "";

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - SPR attribute value must be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.SPRAttrValue = "123";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'SPS' or 'SPR' or 'NAV'.\r\nSPS = Sales/Purchases.\r\nSPR = Sales/Purchases Returns.\r\nNAV = No Attribute Value.", notifications.AsString);
			notifications.Clear();

			XsdAlternateGLAccount.SPRAttrValue = "SPS";
			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(!notifications.HasErrors);
			AssertAttributeVaueAndOrgID("SPR", "SPS", null);

			dissection.ADC_Attribute = "ORG";
			Factory.Save();

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			notifications = (NotificationBuffer)Context.Notifications;
			Assert(notifications.HasErrors);
			AssertContains("Account Number '123456' - SPR attribute value cannot be specified for Parent Account '3410.13.21' and Alternate Chart '111'. Please check your dissection configuration.", notifications.AsString);
		}

		public void TestOneAlterGLAccountToManyGLHeaderCorrectDataImport()
		{
			var glHeader2 = Creator.CreateAccGLHeader("3454.21.12", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			Creator.CreateAccAlternateGLAccountDissection(glHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);
			Factory.Save();
			Creator.CreateAlternateGlAccountForCSVImport(Value.AlternateGLAccount, glHeader2.AG_AccountNum, "311010", Core.Constants.AccountType.BalanceSheetAccount, Chart.AAC_Code, "test", Core.Constants.DebitCredit.Debit,
				AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0, 5, lfeAttrValue: "WEU");
			XsdAlternateGLAccount.LFOAttrValue = "LOC";

			Factory.Save();

			Adapter.ImportFromValueObjectCore_ForTestOnly(Bizo, Value, Context);
			var notifications = (NotificationBuffer)Context.Notifications;
			AssertEquals(notifications.AsString, "Error: Row No.2 - Account Number '311010' - AGA_AccountNum: The specified Alternate Account's Number '311010' already existed in the Alternate Chart '111' and mapped to Parent Account '3454.21.12'.\r\nIt cannot be mapped to a different Parent Account.\r\n");
		}

		void AssertAttributeVaueAndOrgID(string attributeName, string attributeValue, ZGuid? attributeValueID = null)
		{
			var query = new ZQuery(AccAlternateGLAccountAttributeSchema.AAA_Attribute, attributeName);
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_Value, attributeValue);
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AttributeValueID, attributeValueID);
			Assert(Factory.Exists(typeof(AccAlternateGLAccountAttribute), query));
		}

		#region Base Tests

		protected override BusinessObjectThatDoesntSave NewBusinessObject()
		{
			return new BusinessObjectThatDoesntSave(Factory);
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported => false;

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return Adapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return Adapter.RootElementName; }
		}

		protected override ValueObjectDataAdapter<BusinessObjectThatDoesntSave, Xsd.AlternateGLAccounts> GetNewBizObjXmlDataAdapter()
		{
			return new AlternateGLAccountWithAttributeDataAdapter();
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			Value = new Xsd.AlternateGLAccounts();

			Chart = Creator.CreateAlternateChart("111", "Description", true, true, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(Chart, 1, "9999", "desc", "-");
			Creator.CreateAccAlternateChartFormat(Chart, 2, "99", "desc", "");

			GLHeader = Creator.CreateAccGLHeader("3410.13.21", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			Factory.Save();

			XsdAlternateGLAccount = Creator.CreateAlternateGlAccountForCSVImport(Value.AlternateGLAccount, GLHeader.AG_AccountNum, "311010", Core.Constants.AccountType.BalanceSheetAccount, Chart.AAC_Code, "test", Core.Constants.DebitCredit.Debit,
				AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0, 5);
			Buffer = new NotificationBuffer();
			Context = new ValueObjectImportContext(Factory, Buffer);
			Adapter = new AlternateGLAccountWithAttributeDataAdapter();
			Bizo = new BusinessObjectThatDoesntSave(Factory);
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		BusinessObjectThatDoesntSave Bizo;
		AccAlternateChart Chart;
		AlternateGLAccountWithAttributeDataAdapter Adapter;
		AccAlternateGLAccount AccAlternateGLAccount;
		Xsd.AlternateGLAccounts Value;
		AlternateGLAccountsAlternateGLAccount XsdAlternateGLAccount;
		NotificationBuffer Buffer;
		ValueObjectImportContext Context;

		AccGLHeader GLHeader;
		TestObjectCreator Creator;

		#endregion
	}
}
