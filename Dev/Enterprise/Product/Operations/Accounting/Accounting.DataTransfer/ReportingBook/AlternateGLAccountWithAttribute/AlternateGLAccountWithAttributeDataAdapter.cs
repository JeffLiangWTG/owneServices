using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using AlternateGLAccounts = Enterprise.DataTransfer.Xml.XsdVersion1.AlternateGLAccounts;

namespace Enterprise.Accounting.DataTransfer.ReportingBook.AlternateGLAccountWithAttribute
{
	class AlternateGLAccountWithAttributeDataAdapter : ValueObjectDataAdapter<BusinessObjectThatDoesntSave, AlternateGLAccounts>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "AlternateGLAccounts"; }
		}

		public override string RootElementName
		{
			get { return "AlternateGLAccount"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.AlternateGLAccountSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.AlternateGLAccountsSchema; }
		}

		#endregion

		public List<string> AccountTypeAllowImportedWithEmptyParentAccount => new List<string> { Core.Constants.AccountType.Alternate, Core.Constants.AccountType.ChartOnly, Core.Constants.AccountType.Consolidation, Core.Constants.AccountType.Group, Core.Constants.AccountType.Header, Core.Constants.AccountType.Total, Core.Constants.AccountType.Rollup };

		#region Export

		protected override void ExportToValueObjectCore(BusinessObjectThatDoesntSave bizObj, AlternateGLAccounts value, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting Alternate GL Accounts is currently not supported");
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(BusinessObjectThatDoesntSave bizObj, AlternateGLAccounts value, IValueObjectImportContext context)
		{
			ImportAlternateGLAccountsAndAttributes(value.AlternateGLAccount, bizObj, context);
		}

		IEnumerable<string> ExtractValues(AlternateGLAccountsAlternateGLAccount account)
		{
			yield return account.AccountNum;
			yield return account.PercentNum;
			yield return account.ConsolidationNum;
			yield return account.AlternateNum;
			yield return account.TotalReference;
		}

		void ImportAlternateGLAccountsAndAttributes(AlternateGLAccountsAlternateGLAccountCollection alternateGLAccounts, BusinessObjectThatDoesntSave bizObj, IValueObjectImportContext context)
		{
			var alternateGLAccountsCast = alternateGLAccounts.Cast<AlternateGLAccountsAlternateGLAccount>();
			var chartDict = GetChartDict(alternateGLAccountsCast, bizObj.Factory);
			var chartPKs = chartDict.Values.Select(x => x.PK).ToList();
			var glHeaderAccountNumSet = alternateGLAccountsCast.Select(alternateGLAccount => alternateGLAccount.ParentAccount);
			var accAlternateGLAccountAccountNumSet = alternateGLAccountsCast.SelectMany(alternateGLAccount => ExtractValues(alternateGLAccount)).Distinct().Where(alternateGLAccount => !string.IsNullOrEmpty(alternateGLAccount)).ToHashSet();

			var gLHeaderDict = bizObj.Factory.Load<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, glHeaderAccountNumSet)).ToDictionary(x => x.AG_AccountNum, x => x);
			var dissectionDict = bizObj.Factory.Load<AccAlternateGLAccountDissection>(new ZQuery(AccAlternateGLAccountDissectionSchema.ADC_AAC_AlternateChart, chartPKs)).GroupBy(x => GetDissectionDictKey(x)).ToDictionary(group => group.Key, group => group.AsEnumerable());

			var importedParentAccounts = alternateGLAccountsCast.Select(alternateGLAccount => alternateGLAccount.ParentAccount).ToList();
			var importedParentAccountPKs = importedParentAccounts.Where(parentAccount => !string.IsNullOrEmpty(parentAccount) && gLHeaderDict.ContainsKey(parentAccount)).Select(parentAccount => gLHeaderDict[parentAccount].PK).ToList();
			var maxSequenceDict = GetCurrentAlternateGLAccountAttributesMaxSequenceInDatabase(chartPKs, importedParentAccountPKs, bizObj);

			var accAlternateGLAccountDict = GetCurrentAlternateGLAccountInDatabase(chartPKs, accAlternateGLAccountAccountNumSet, bizObj);
			var rowNumber = 0;

			foreach (var xsdAlternateGLAccount in alternateGLAccountsCast)
			{
				rowNumber++;
				var isValidChart = chartDict.TryGetValue(xsdAlternateGLAccount.ChartCode, out var chart);
				var isValidParentAccount = ValidateParentAccount(xsdAlternateGLAccount, gLHeaderDict, dissectionDict, chart, context, rowNumber, out var allowImportAttribute);
				if (isValidChart && accAlternateGLAccountDict.TryGetValue(chart.PK.ToString() + xsdAlternateGLAccount.AccountNum, out var newAlternateGLAccount))
				{
					ValidateGLAccountFields(newAlternateGLAccount, xsdAlternateGLAccount, accAlternateGLAccountDict, gLHeaderDict, context, rowNumber);
					if (isValidParentAccount)
					{
						if (allowImportAttribute)
						{
							var parentAccount = gLHeaderDict[xsdAlternateGLAccount.ParentAccount];
							var query = new ZQuery();
							query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, parentAccount.PK);
							query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, newAlternateGLAccount.PK);

							if (bizObj.Factory.Exists(typeof(AccAlternateGLAccountAttribute), query))
							{
								context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("E9B6CAB9-7F20-4C32-8DA7-CF6BE501CE0A", "Row No.{0} - The Alternate account number '{1}' mapped to Parent Account '{2}' for the Chart '{3}' has already been existing.", rowNumber, newAlternateGLAccount.AGA_AccountNum, xsdAlternateGLAccount.ParentAccount, xsdAlternateGLAccount.ChartCode)));
							}
							else
							{
								ValidateCashFlowTypeAndUnits(gLHeaderDict[xsdAlternateGLAccount.ParentAccount], newAlternateGLAccount, context, rowNumber);
							}
						}
						else
						{
							context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("4F4E20A0-3EED-40A0-B896-6005CDF226EF", "Row No.{0} - The Alternate account number '{1}' for the Chart '{2}' has already been existing.", rowNumber, newAlternateGLAccount.AGA_AccountNum, xsdAlternateGLAccount.ChartCode)));
						}
					}
				}
				else
				{
					newAlternateGLAccount = bizObj.Factory.New<AccAlternateGLAccount>();
					var validation = newAlternateGLAccount.Validation;
					gLHeaderDict.TryGetValue(xsdAlternateGLAccount.ParentAccount, out var glHeader);

					SetAlternateChartAndCompany(chartDict, xsdAlternateGLAccount, context, newAlternateGLAccount, rowNumber);
					SetAccountType(xsdAlternateGLAccount.AccountType, newAlternateGLAccount, context, rowNumber);
					context.SetPropertyInfoValue(newAlternateGLAccount.AGA_AccountNumInfo, xsdAlternateGLAccount.AccountNum);
					SetAccountName(xsdAlternateGLAccount, newAlternateGLAccount, context, rowNumber);
					SetDebitCredit(xsdAlternateGLAccount, newAlternateGLAccount, context, rowNumber);
					SetReportSection(xsdAlternateGLAccount, newAlternateGLAccount, context, rowNumber);
					SetPercentNum(xsdAlternateGLAccount, accAlternateGLAccountDict, newAlternateGLAccount, context, rowNumber);
					SetConsolidationNum(xsdAlternateGLAccount, accAlternateGLAccountDict, newAlternateGLAccount, context, rowNumber);
					SetAlternateNum(xsdAlternateGLAccount, accAlternateGLAccountDict, newAlternateGLAccount, context, rowNumber);
					SetTotalReference(xsdAlternateGLAccount, accAlternateGLAccountDict, newAlternateGLAccount, context, rowNumber);
					SetTotalLevel(newAlternateGLAccount, xsdAlternateGLAccount, context, rowNumber);
					newAlternateGLAccount.AGA_PrintSequence = xsdAlternateGLAccount.PrintSequence;
					newAlternateGLAccount.Validation.ValidateAGA_PrintSequence();
					var accAlternateGLAccountDictKey = (chart == null ? ZString.Empty : chart.PK.ToString()) + xsdAlternateGLAccount.AccountNum;
					accAlternateGLAccountDict.Add(accAlternateGLAccountDictKey, newAlternateGLAccount);
				}

				if (isValidParentAccount && allowImportAttribute)
				{
					var dissectionDictKey = newAlternateGLAccount.AGA_AAC_AlternateChart.ToString() + gLHeaderDict[xsdAlternateGLAccount.ParentAccount].PK.ToString();
					var maxSequenceDictKey = chart.PK.ToString() + gLHeaderDict[xsdAlternateGLAccount.ParentAccount].PK.ToString();
					if (maxSequenceDict.ContainsKey(maxSequenceDictKey))
					{
						maxSequenceDict[maxSequenceDictKey] = maxSequenceDict[maxSequenceDictKey] + 1;
					}
					else
					{
						maxSequenceDict.Add(maxSequenceDictKey, 1);
					}

					var attributeXsdDict = GetXsdAttributeDict(xsdAlternateGLAccount);
					if (dissectionDict.TryGetValue(dissectionDictKey, out var dissections) && dissections.Any(dissection => dissection.ADC_SeparateNumbering))
					{
						var dissectionNumberingDict = dissections.ToDictionary(dissection => dissection.ADC_Attribute, dissection => dissection.ADC_SeparateNumbering);
						ImportAttribtutes(bizObj, xsdAlternateGLAccount, newAlternateGLAccount, gLHeaderDict, dissectionNumberingDict, attributeXsdDict, maxSequenceDict[maxSequenceDictKey], context, rowNumber);
					}
					else
					{
						var importedAttributeHasValue = false;
						foreach (var attributeName in attributeXsdDict.Keys)
						{
							var value = attributeXsdDict[attributeName];
							if (!string.IsNullOrEmpty(value))
							{
								importedAttributeHasValue = true;
								context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, GetAttributeCannotBeSpecifiedMessage(xsdAlternateGLAccount.AccountNum, attributeName, xsdAlternateGLAccount.ParentAccount, xsdAlternateGLAccount.ChartCode, rowNumber)));
							}
						}
						if (!importedAttributeHasValue)
						{
							ImportAttribtuteWithoutAttributeValue(xsdAlternateGLAccount, newAlternateGLAccount, gLHeaderDict);
						}
					}
				}
				AddErrorsToNotifications(newAlternateGLAccount, xsdAlternateGLAccount, context, rowNumber);
				if (!context.NotificationsHasErrors)
				{
					newAlternateGLAccount.RunPreSaveValidation();
					AddErrorsToNotifications(newAlternateGLAccount, xsdAlternateGLAccount, context, rowNumber);
				}
			}
		}

		Dictionary<ZString, AccAlternateChart> GetChartDict(IEnumerable<AlternateGLAccountsAlternateGLAccount> alternateGLAccountsCast, BusinessObjectFactory factory)
		{
			var chartQuery = new ZQuery(AccAlternateChartSchema.AAC_Code, alternateGLAccountsCast.Select(alternateGLAccount => alternateGLAccount.ChartCode));
			var subQuery = new ZQuery();
			subQuery.AddToFilter(AccAlternateChartSchema.AAC_GC_Company, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(JoinCondition.Or, AccAlternateChartSchema.AAC_GC_Company, null);
			chartQuery.AddToFilter(subQuery);
			return factory.Load<AccAlternateChart>(chartQuery).ToDictionary(x => x.AAC_Code, x => x);
		}

		void SetTotalLevel(AccAlternateGLAccount newAlternateGLAccount, AlternateGLAccountsAlternateGLAccount alternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (alternateGLAccount.AccountType == Core.Constants.AccountType.Total && (alternateGLAccount.TotalLevel > 999 || alternateGLAccount.TotalLevel < 1))
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("B310E7ED-09FE-4313-9261-576F16770411", "Row No.{0} - Account Number '{1}' - Invalid Total Level. Value must be 1 to 999.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
			}
			else if (alternateGLAccount.AccountType != Core.Constants.AccountType.Total && alternateGLAccount.TotalLevel != ZInt.Zero)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("6A4BDC12-C21C-4229-9565-0E64F992501D", "Row No.{0} - Account Number '{1}' - A Total Level can only be specified if the Account Type is TTL.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
			}
			else
			{
				newAlternateGLAccount.AGA_TotalLevel = alternateGLAccount.TotalLevel;
			}
		}

		void SetTotalReference(AlternateGLAccountsAlternateGLAccount alternateGLAccount, Dictionary<string, AccAlternateGLAccount> accAlternateGLAccountDict, AccAlternateGLAccount newAlternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (!string.IsNullOrEmpty(alternateGLAccount.TotalReference))
			{
				if (!accAlternateGLAccountDict.TryGetValue(newAlternateGLAccount.AGA_AAC_AlternateChart.ToString() + alternateGLAccount.TotalReference, out var totalReferenceAccount) || totalReferenceAccount.AGA_AccountType != Core.Constants.AccountType.Total)
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("9D4E4F19-344A-4CCD-8030-182FB046F18A", "Row No.{0} - Account Number '{1}' - Invalid Total Reference. A valid Total Reference should be 'TTL' account type.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
				}
				else if (alternateGLAccount.AccountType != Core.Constants.AccountType.Header)
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("6066B47D-CCF4-4ADB-91F8-B93A64BDE771", "Row No.{0} - Account Number '{1}' - Only when the Account Type is 'HDR', Total Reference is allowed.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
				}
				else
				{
					newAlternateGLAccount.AGA_AGA_HeaderDependsOnTotal = totalReferenceAccount.PK;
				}
			}
		}

		void SetAlternateNum(AlternateGLAccountsAlternateGLAccount alternateGLAccount, Dictionary<string, AccAlternateGLAccount> accAlternateGLAccountDict, AccAlternateGLAccount newAlternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (!string.IsNullOrEmpty(alternateGLAccount.AlternateNum))
			{
				if (!accAlternateGLAccountDict.TryGetValue(newAlternateGLAccount.AGA_AAC_AlternateChart.ToString() + alternateGLAccount.AlternateNum, out var alternateAccount) || alternateAccount.AGA_AccountType != Core.Constants.AccountType.Alternate)
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("1CD7EB5D-8C19-4A7C-8DA9-FBA16B1CA53B", "Row No.{0} - Account Number '{1}' - Invalid Alternate Number. A valid Alternate Number must be 'ALT' account type.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
				}
				else if (alternateGLAccount.AccountType != Core.Constants.AccountType.BalanceSheetAccount)
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("B24DC246-B32A-43F0-A1BF-512BADBF35FB", "Row No.{0} - Account Number '{1}' - An Alternate Number can only be specified if the Account Type is 'BSH'.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
				}
				else
				{
					newAlternateGLAccount.AGA_AGA_AlternateNum = alternateAccount.PK;
				}
			}
		}

		void SetConsolidationNum(AlternateGLAccountsAlternateGLAccount alternateGLAccount, Dictionary<string, AccAlternateGLAccount> accAlternateGLAccountDict, AccAlternateGLAccount newAlternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (!string.IsNullOrEmpty(alternateGLAccount.ConsolidationNum))
			{
				if (!accAlternateGLAccountDict.TryGetValue(newAlternateGLAccount.AGA_AAC_AlternateChart.ToString() + alternateGLAccount.ConsolidationNum, out var consolidationAccount) || consolidationAccount.AGA_AccountType != Core.Constants.AccountType.Consolidation)
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("05C13385-7E63-4ABE-8065-53981C79ABCF", "Row No.{0} - Account Number '{1}' - Invalid Consolidate Account. A valid Consolidate Account must be 'CLN' account type.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
				}
				else if (newAlternateGLAccount.AccountTypeWithoutReferenceAlteranteGLAccountForConsolidationNum.Contains(alternateGLAccount.AccountType))
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("6696CA97-175E-443F-8628-80D182A689BC", "Row No.{0} - Account Number '{1}' - A Consolidate Account can only be specified if the Account Type is ALT, BSH, TTL or P&L.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
				}
				else
				{
					newAlternateGLAccount.AGA_AGA_ConsolidationNum = consolidationAccount.PK;
				}
			}
		}

		void SetPercentNum(AlternateGLAccountsAlternateGLAccount alternateGLAccount, Dictionary<string, AccAlternateGLAccount> accAlternateGLAccountDict, AccAlternateGLAccount newAlternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (!string.IsNullOrEmpty(alternateGLAccount.PercentNum))
			{
				if (!accAlternateGLAccountDict.TryGetValue(newAlternateGLAccount.AGA_AAC_AlternateChart.ToString() + alternateGLAccount.PercentNum, out var percentAccount) || (percentAccount.AGA_AccountType != Core.Constants.AccountType.Consolidation && percentAccount.AGA_AccountType != Core.Constants.AccountType.Total))
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("ABA9B626-59B2-4C60-96E0-12E15E425629", "Row No.{0} - Account Number '{1}' - Invalid Percent Account. A valid Percent Account must be 'CLN' or ''TTL' account type.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
				}
				else if (newAlternateGLAccount.AccountTypeWithoutReferenceAlteranteGLAccountForPercentNum.Contains(alternateGLAccount.AccountType))
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("7A195C2B-20DC-4241-8E41-E4C724CCEEB6", "Row No.{0} - Account Number '{1}' - A Percent Account can only be specified if the Account Type is ALT, BSH or P&L.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
				}
				else
				{
					newAlternateGLAccount.AGA_AGA_PercentNum = percentAccount.PK;
				}
			}
		}

		void SetReportSection(AlternateGLAccountsAlternateGLAccount alternateGLAccount, AccAlternateGLAccount newAlternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (string.IsNullOrEmpty(alternateGLAccount.ReportSection))
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("F2109409-D2B6-4E56-9ECF-F7CD682C7C2D", "Row No.{0} - Account Number '{1}' - Report Section cannot be empty.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
			}
			else if (!newAlternateGLAccount.Lookups.ReportSectionList.ContainsCode(alternateGLAccount.ReportSection))
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("5F2AFE70-A76E-4842-8EA0-543B5EF8264A", "Row No.{0} - Account Number '{1}' - Invalid Report Section. A valid Report Section must be one of the values:TS,OV,AP,OE,AS,LI", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
			}
			else
			{
				context.SetPropertyInfoValue(newAlternateGLAccount.AGA_ReportSectionInfo, alternateGLAccount.ReportSection);
			}
		}

		void SetDebitCredit(AlternateGLAccountsAlternateGLAccount alternateGLAccount, AccAlternateGLAccount newAlternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (string.IsNullOrEmpty(alternateGLAccount.DebitCredit))
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("B5B2620E-C48B-4F89-BC38-81FDE62E094E", "Row No.{0} - Account Number '{1}' - Debit/Credit cannot be empty.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
			}
			else if (newAlternateGLAccount.Lookups.DebitCreditList.ContainsCode(alternateGLAccount.DebitCredit))
			{
				context.SetPropertyInfoValue(newAlternateGLAccount.AGA_DebitCreditInfo, alternateGLAccount.DebitCredit);
			}
			else
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("C20A434D-006D-4EB3-A5B9-3ABFD2E65117", "Row No.{0} - Account Number '{1}' - Invalid Debit/Credit. A valid Debit/Credit should be one of the values: DR or CR.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
			}
		}

		void SetAccountName(AlternateGLAccountsAlternateGLAccount alternateGLAccount, AccAlternateGLAccount newAlternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (alternateGLAccount.AccountName.Length > 128)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("9C8782BF-9CC8-4D0C-90E0-9F0473B09E61", "Row No.{0} - Account Number '{1}' - Name '{2}' exceeds the maximum length 128 characters.", rowNumber, newAlternateGLAccount.AGA_AccountNum, alternateGLAccount.AccountName)));
			}
			else
			{
				context.SetPropertyInfoValue(newAlternateGLAccount.AGA_DescriptionInfo, alternateGLAccount.AccountName);
			}
		}

		void SetAccountType(ZString accountType, AccAlternateGLAccount newAlternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (string.IsNullOrEmpty(accountType))
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("3909ECD7-A407-4D6C-A977-6167BD7E2587", "Row No.{0} - Account Number '{1}' - Account Type cannot be empty.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
			}
			else if (newAlternateGLAccount.Lookups.AccountTypeList.ContainsCode(accountType))
			{
				context.SetPropertyInfoValue(newAlternateGLAccount.AGA_AccountTypeInfo, accountType);
			}
			else
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("EAE2C623-3374-4273-B308-6C6C0F26B18E", "Row No.{0} - Account Number '{1}' - Invalid Account Type. A valid account type must be one of the values: BSH, P&L, TTL, HDR, CLN, ALT,NTE,CHT,RUP,GRP.", rowNumber, newAlternateGLAccount.AGA_AccountNum)));
			}
		}

		void ValidateCashFlowTypeAndUnits(AccGLHeader currentGLHeader, AccAlternateGLAccount existingAlternateGLAccount, IValueObjectImportContext context, int rowNumber)
		{
			if (existingAlternateGLAccount.CashFlowType != currentGLHeader.AG_CashFlowType)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("2D5501B3-531E-48AB-9CC9-99A019B3310A", "Row No.{0} - Account Number '{1}' - Invalid Cash Flow Type as mapped multiple Parent Accounts have different Cash Flow Type", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}
			if (existingAlternateGLAccount.StatisticalUnits != currentGLHeader.AG_StatisticalUnits)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("E66FD4EE-0B55-4544-9B2E-9AE974B0207B", "Row No.{0} - Account Number '{1}' - Invalid Units as mapped multiple Parent Accounts have different Units.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}
		}

		bool ValidateParentAccount(AlternateGLAccountsAlternateGLAccount xsdAlternateGLAccount, Dictionary<ZString, AccGLHeader> gLHeaderDict, Dictionary<ZString, IEnumerable<AccAlternateGLAccountDissection>> dissectionDict, AccAlternateChart chart, IValueObjectImportContext context, int rowNumber, out bool allowImportAttribute)
		{
			var result = true;
			allowImportAttribute = false;
			if (AccountTypeAllowImportedWithEmptyParentAccount.Contains(xsdAlternateGLAccount.AccountType))
			{
				if (string.IsNullOrEmpty(xsdAlternateGLAccount.ParentAccount))
				{
					if (GetXsdAttributeDict(xsdAlternateGLAccount).Values.Any(value => !string.IsNullOrEmpty(value)))
					{
						context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("87089C28-7933-467B-B92E-C9276C36CF4D", "Row No.{0} - Account Number '{1}' - The attribute value cannot be specified for account type ALT, CLN, HDR, TTL, CHT, RUP, GUP.", rowNumber, xsdAlternateGLAccount.AccountNum)));
					}
				}
				else
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("7E16DBB3-D97F-41B4-B4B5-E37F6C8656D6", "Row No.{0} - Account Number '{1}' - Parent Account must be empty.", rowNumber, xsdAlternateGLAccount.AccountNum)));
					result = false;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(xsdAlternateGLAccount.ParentAccount))
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("9246B688-E26C-4DC6-BB25-938599D267A1", "Row No.{0} - Account Number '{1}' - Parent Account cannot be empty.", rowNumber, xsdAlternateGLAccount.AccountNum)));
					result = false;
				}
				else if (!gLHeaderDict.TryGetValue(xsdAlternateGLAccount.ParentAccount, out var glHeader))
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("B0F0555A-E66B-42E8-936F-AFC743B3FE94", "Row No.{0} - Account Number '{1}' - Invalid Parent Account. Parent Account {2} is not existing.", rowNumber, xsdAlternateGLAccount.AccountNum, xsdAlternateGLAccount.ParentAccount)));
					result = false;
				}
				else if (glHeader.AG_AccountType != xsdAlternateGLAccount.AccountType)
				{
					result = false;
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("77896E7D-0B3D-4DEB-B97E-DF8CEA01164C", "Row No.{0} - Account Number '{1}' - Invalid Parent Account. The Type of Parent Account {2} is NOT '{3}'.", rowNumber, xsdAlternateGLAccount.AccountNum, glHeader.AG_AccountNum, xsdAlternateGLAccount.AccountType)));
				}
				else if (chart != null)
				{
					if (!dissectionDict.TryGetValue(chart.PK.ToString() + glHeader.PK.ToString(), out var dissections) || !dissections.Any(d => d.ADC_SeparateNumbering))
					{
						var query = new ZQuery();
						query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, glHeader.PK);
						query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, chart.PK);
						var attribute = glHeader.Factory.LoadTop1<AccAlternateGLAccountAttribute>(query);
						if (attribute != null)
						{
							result = false;
							context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("38B6C147-9E37-4D40-B8E0-E08BF4CD7FBF", "Row No.{0} - Account Number '{1}' - Invalid Parent Account. The selected Parent Account has been referenced to Alternate Account {2} of Chart {3}, please enter another one.", rowNumber, xsdAlternateGLAccount.AccountNum, attribute.AlternateGLAccount.AGA_AccountNum, attribute.AlternateChart.AAC_Code)));
						}
						else
						{
							allowImportAttribute = true;
						}
					}
					else
					{
						allowImportAttribute = true;
					}
				}
			}

			return result;
		}

		void ValidateGLAccountFields(AccAlternateGLAccount existingAlternateGLAccount, AlternateGLAccountsAlternateGLAccount xsdAlternateGLAccount, Dictionary<string, AccAlternateGLAccount> accAlternateGLAccountDict, Dictionary<ZString, AccGLHeader> glHeaderDict, IValueObjectImportContext context, int rowNumber)
		{
			if (glHeaderDict.TryGetValue(xsdAlternateGLAccount.ParentAccount, out var currentImportedParentAccount))
			{
				if (existingAlternateGLAccount.AGA_AccountType == Core.Constants.AccountType.Note && currentImportedParentAccount.AG_AccountType == Core.Constants.AccountType.Note)
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("6CE278C3-C118-463B-8165-03434F35918B", @"Row No.{0} - Account Number '{1}' - The specified Alternate Account's Number '{1}' already existed and cannot be linked to a different Parent Account as NTE Alternate GL Account cannot be mapped from multiple NTE Parent Accounts.
Please enter a different value.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
				}

				var glHeaders = existingAlternateGLAccount.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>().Select(x => x.AAA_AG_GLHeader).ToHashSet();
				glHeaders.Add(currentImportedParentAccount.PK);
				var accounting = ObjectFactory.Get<IAccounting>();
				if (glHeaders.Any(x => accounting.IsNotAllowedForSeparateNumbering(x) || accounting.IsNotAllowedForDissectionAttributes(x) || AccountingMasterFilesUtils.IsNotAllowedForDissectionControlAccount(x.ToGuid())) || currentImportedParentAccount.Factory.Load<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, glHeaders)).Any(x => x.IsBankAccount()))
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("7996F615-F691-4755-8DF9-AE61C8DB858A", @"Row No.{0} - Account Number '{1}' - The specified Alternate Account's Number '{1}' already existed in the Alternate Chart '{2}' and is linked to Parent Account used in the system registries or bank accounts that do not allow dissection. It cannot be mapped to multiple Parent Accounts.
Please enter a different value.", rowNumber, existingAlternateGLAccount.AGA_AccountNum, xsdAlternateGLAccount.ChartCode)));
				}
			}
			if (existingAlternateGLAccount.AGA_AccountType != xsdAlternateGLAccount.AccountType)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("07971D4A-FDFD-4114-B846-12D1C8D1D5CA", "Row No.{0} - Account Number '{1}' - Invalid Account Type as mapped Alternate GL Account have different Account Type.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}
			if (existingAlternateGLAccount.AGA_Description != xsdAlternateGLAccount.AccountName)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("D45DB4D4-FB71-42DF-BD09-6CAD172987F8", "Row No.{0} - Account Number '{1}' - Invalid Account Name as mapped Alternate GL Account have different Account Name.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}
			if (existingAlternateGLAccount.AGA_DebitCredit != xsdAlternateGLAccount.DebitCredit)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("D3B0BD16-9E35-4614-A655-515F50FF4371", "Row No.{0}  - Account Number '{1}' - Invalid Debit/Credit as mapped Alternate GL Account have different Debit/Credit.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}
			if (existingAlternateGLAccount.AGA_ReportSection != xsdAlternateGLAccount.ReportSection)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("3BDA3B77-BA8B-4DD7-ADD8-A5ABABFE14E2", "Row No.{0} - Account Number '{1}' - Invalid Report Section as mapped Alternate GL Account have different Report Section.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}
			if (existingAlternateGLAccount.AGA_TotalLevel != xsdAlternateGLAccount.TotalLevel)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("5DDAE901-E25B-438D-8646-392B2D249A4A", "Row No.{0} - Account Number '{1}' - Invalid Total Level as mapped Alternate GL Account have different Total Level.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}
			if (existingAlternateGLAccount.AGA_PrintSequence != xsdAlternateGLAccount.PrintSequence)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("377AE276-6D52-4214-A6A0-334288F0E4A4", "Row No.{0} - Account Number '{1}' - Invalid Print Sequence as mapped Alternate GL Account have different Print Sequence.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}

			accAlternateGLAccountDict.TryGetValue(existingAlternateGLAccount.AGA_AGA_PercentNum.ToString(), out var percent);
			if ((percent == null ? ZString.Empty : percent.AGA_AccountNum) != xsdAlternateGLAccount.PercentNum)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("502BC787-9117-4C94-9FB6-93CA097E8DCF", "Row No.{0} - Account Number '{1}' - Invalid Percent Number as mapped Alternate GL Account have different Percent Number.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}

			accAlternateGLAccountDict.TryGetValue(existingAlternateGLAccount.AGA_AGA_ConsolidationNum.ToString(), out var consolidate);
			if ((consolidate == null ? ZString.Empty : consolidate.AGA_AccountNum) != xsdAlternateGLAccount.ConsolidationNum)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("FFDFA6BC-FEA3-451D-B829-ADCD2E12EF16", "Row No.{0} - Account Number '{1}' - Invalid Consolidate as mapped Alternate GL Account have different Consolidate.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}

			accAlternateGLAccountDict.TryGetValue(existingAlternateGLAccount.AGA_AGA_AlternateNum.ToString(), out var alternate);
			if ((alternate == null ? ZString.Empty : alternate.AGA_AccountNum) != xsdAlternateGLAccount.AlternateNum)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("A8E60CFC-57A9-465F-99D4-AF4A3517AF1E", "Row No.{0} - Account Number '{1}' - Invalid Alternate Number as mapped Alternate GL Account have different Alternate Number.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}

			accAlternateGLAccountDict.TryGetValue(existingAlternateGLAccount.AGA_AGA_HeaderDependsOnTotal.ToString(), out var totalReference);
			if ((totalReference == null ? ZString.Empty : totalReference.AGA_AccountNum) != xsdAlternateGLAccount.TotalReference)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("BAFB98DF-6172-45A7-BD71-0EE8E8D4B1E0", "Row No.{0} - Account Number '{1}' - Invalid Total Reference as mapped Alternate GL Account have different Total Reference.", rowNumber, existingAlternateGLAccount.AGA_AccountNum)));
			}
		}

		Dictionary<string, ZInt> GetCurrentAlternateGLAccountAttributesMaxSequenceInDatabase(List<ZGuid> chartPKs, List<ZGuid> parentAccountPKs, BusinessObjectThatDoesntSave bizObj)
		{
			var query = new ZQuery();
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, chartPKs);
			query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, parentAccountPKs);
			return bizObj.Factory.Load<AccAlternateGLAccountAttribute>(query).GroupBy(x => x.AAA_AAC_AlternateChart.ToString() + x.AAA_AG_GLHeader.ToString()).ToDictionary(group => group.Key, group => group.Max(attribute => attribute.AAA_Sequence));
		}

		Dictionary<string, AccAlternateGLAccount> GetCurrentAlternateGLAccountInDatabase(List<ZGuid> chartPKList, HashSet<string> alternateGLAccountAccountNumSet, BusinessObjectThatDoesntSave bizObj)
		{
			var gLAccountQuery = new ZQuery();
			gLAccountQuery.AddToFilter(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, chartPKList);
			gLAccountQuery.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, alternateGLAccountAccountNumSet);
			return bizObj.Factory.Load<AccAlternateGLAccount>(gLAccountQuery).ToDictionary(x => x.AGA_AAC_AlternateChart.ToString() + x.AGA_AccountNum, x => x);
		}

		ZString GetDissectionDictKey(AccAlternateGLAccountDissection x)
		{
			return x.ADC_AAC_AlternateChart.ToString() + x.ADC_AG_GLHeader.ToString();
		}

		Dictionary<ZString, ZString> GetXsdAttributeDict(AlternateGLAccountsAlternateGLAccount alternateGLAccount)
		{
			var attributeXsdDict = new Dictionary<ZString, ZString>();
			attributeXsdDict.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, alternateGLAccount.ORGAttrValue);
			attributeXsdDict.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, alternateGLAccount.OCGAttrValue);
			attributeXsdDict.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, alternateGLAccount.LFOAttrValue);
			attributeXsdDict.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, alternateGLAccount.LFEAttrValue);
			attributeXsdDict.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, alternateGLAccount.TICAttrValue);
			attributeXsdDict.Add(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, alternateGLAccount.SPRAttrValue);
			return attributeXsdDict;
		}

		void ImportAttribtuteWithoutAttributeValue(AlternateGLAccountsAlternateGLAccount xsdAlternateGLAccount, AccAlternateGLAccount newAlternateGLAccount, Dictionary<ZString, AccGLHeader> gLHeaderDict)
		{
			var newAttribute = newAlternateGLAccount.AlternateGLAccountAttributes.AddNew();
			newAttribute.AAA_AG_GLHeader = gLHeaderDict[xsdAlternateGLAccount.ParentAccount].PK;
			newAttribute.AAA_AGA_AlternateGLAccount = newAlternateGLAccount.PK;
			newAttribute.AAA_AAC_AlternateChart = newAlternateGLAccount.AGA_AAC_AlternateChart;
			newAttribute.AAA_Sequence = 1;
		}

		void ImportAttribtutes(BusinessObjectThatDoesntSave bizObj, AlternateGLAccountsAlternateGLAccount xsdGLAccount, AccAlternateGLAccount alternateGLAccount, Dictionary<ZString, AccGLHeader> gLHeaderDict, Dictionary<ZString, ZBool> dissectionNumberingDict, Dictionary<ZString, ZString> xsdAttributes, ZInt sequence, IValueObjectImportContext context, int rowNumber)
		{
			foreach (var xsdAttribute in xsdAttributes.Keys)
			{
				if (string.IsNullOrEmpty(xsdAttributes[xsdAttribute]))
				{
					if (dissectionNumberingDict.TryGetValue(xsdAttribute, out var isSeparatorNumbering) && isSeparatorNumbering)
					{
						context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, GetAttributeMustBeSpecifiedMessage(xsdGLAccount.AccountNum, xsdAttribute, xsdGLAccount.ParentAccount, xsdGLAccount.ChartCode, rowNumber)));
					}
				}
				else
				{
					if (dissectionNumberingDict.TryGetValue(xsdAttribute, out var isSeparatorNumbering) && isSeparatorNumbering)
					{
						if (ValidateAttributeValue(bizObj, xsdAttribute, xsdAttributes[xsdAttribute], xsdGLAccount, gLHeaderDict[xsdGLAccount.ParentAccount], context, rowNumber, out var organization))
						{
							var newAttribute = alternateGLAccount.AlternateGLAccountAttributes.AddNew();
							newAttribute.AAA_Attribute = xsdAttribute;
							newAttribute.AAA_AG_GLHeader = gLHeaderDict[xsdGLAccount.ParentAccount].PK;
							newAttribute.AAA_AGA_AlternateGLAccount = alternateGLAccount.PK;
							newAttribute.AAA_AAC_AlternateChart = alternateGLAccount.AGA_AAC_AlternateChart;
							if (xsdAttribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
							{
								newAttribute.AAA_AttributeValueID = organization.PK;
							}
							else
							{
								newAttribute.AAA_Value = xsdAttributes[xsdAttribute];
							}

							newAttribute.AAA_Sequence = sequence;
						}
					}
					else
					{
						context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, GetAttributeCannotBeSpecifiedMessage(xsdGLAccount.AccountNum, xsdAttribute, xsdGLAccount.ParentAccount, xsdGLAccount.ChartCode, rowNumber)));
					}
				}
			}
		}

		bool ValidateAttributeValue(BusinessObjectThatDoesntSave bizObj, ZString xsdAttribute, ZString value, AlternateGLAccountsAlternateGLAccount xsd, AccGLHeader gLHeader, IValueObjectImportContext context, int rowNumber, out OrgHeader organization)
		{
			var result = true;
			organization = null;
			if (xsdAttribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
			{
				organization = bizObj.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, value);
				if (organization != null && organization.IsDebtorForCompany(GlbCompany.CurrentCompany) && AccountingConfigurationRegistry.Instance.ARControlAccount.Value == gLHeader.PK)
				{
				}
				else if (organization != null && organization.IsCreditorForCompany(GlbCompany.CurrentCompany) && AccountingConfigurationRegistry.Instance.APControlAccount.Value == gLHeader.PK)
				{
				}
				else
				{
					result = false;
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("66371B72-5CBD-49D9-8F0C-CE6DB4677113", "Row No.{0} - Account Number '{1}' - Invalid Attribute Value. A valid Attribute Value must be the code of a valid Receivable Organization (if Parent Account is an AR Control Account) or a valid Payable Organization (if Parent Account is an AP Control Account).", rowNumber, xsd.AccountNum)));
				}
			}
			else if (xsdAttribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG && !AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.Cast<CodeDescriptionWithGroup>().Select(x => x.Group).ToHashSet().Contains(value) && !IsNAV(value))
			{
				result = false;
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("33AEC335-926E-4342-B7A0-84BAB74A2000", "Row No.{0} - Account Number '{1}' - Invalid Attribute Value. A valid Attribute Value must be one of the following Class defined in Consolidated Accounting Category List registry or value 'NAV'.", rowNumber, xsd.AccountNum)));
			}
			else if (xsdAttribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO && !AccountingMasterFilesConstants.LFOList.GetAllCodes().ToList().Contains(value))
			{
				result = false;
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("68EFE23A-E5C5-4519-8323-02C9878183CD", "Row No.{0} - Account Number '{1}' - Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC' or 'FOR' or 'NAV'.\r\nLOC = 'Local'.\r\nFOR = 'Foreign'.\r\nNAV = No Attribute Value.", rowNumber, xsd.AccountNum)));
			}
			else if (xsdAttribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE && !AccountingMasterFilesConstants.LFEList.GetAllCodes().ToList().Contains(value))
			{
				result = false;
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("5228D2B0-9A9A-43FA-B16F-ADD31C5E85AB", "Row No.{0} - Account Number '{1}' - Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC', 'WEU' or 'OEU' or 'NAV'.\r\nLOC = Local.\r\nWEU = Within EU.\r\nOEU = Outside EU.\r\nNAV = No Attribute Value.", rowNumber, xsd.AccountNum)));
			}
			else if (xsdAttribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC && !AccountingMasterFilesConstants.TICList.GetAllCodes().ToList().Contains(value))
			{
				result = false;
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("466F1801-8E00-49BF-B1C3-D7FE6E0376AD", "Row No.{0} - Account Number '{1}' - Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'STI' or 'ETI' or 'NAV'.\r\nSTI = Standard Tax IDs.\r\nETI = Tax ID with Extra Tax.\r\nNAV = No Attribute Value.", rowNumber, xsd.AccountNum)));
			}
			else if (xsdAttribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR && !AccountingMasterFilesConstants.SPRList.GetAllCodes().ToList().Contains(value))
			{
				result = false;
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("0FD92EEF-7275-4A89-993B-20C45A83298F", "Row No.{0} - Account Number '{1}' - Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'SPS' or 'SPR' or 'NAV'.\r\nSPS = Sales/Purchases.\r\nSPR = Sales/Purchases Returns.\r\nNAV = No Attribute Value.", rowNumber, xsd.AccountNum)));
			}
			return result;
		}

		void AddErrorsToNotifications(AccAlternateGLAccount alternateGLAccount, AlternateGLAccountsAlternateGLAccount value, IValueObjectImportContext context, int rowNumber)
		{
			foreach (var errorString in alternateGLAccount.Notifications.GetErrors().GetUniqueMessageList())
			{
				var split = errorString.Split('-').Last();
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, GetErrorMessageFormatted(value, split, rowNumber)));
			}

			foreach (var warningString in alternateGLAccount.Notifications.GetWarnings().GetUniqueMessageList())
			{
				var split = warningString.Split('-').Last();
				context.Notify(new WarningNotification(WarningType.Warning, GetErrorMessageFormatted(value, split, rowNumber)));
			}
		}

		string GetErrorMessageFormatted(AlternateGLAccountsAlternateGLAccount value, string message, int rowNumber)
		{
			return Res.GetString("BBE191F7-5338-4FF3-A7DC-3EE4F852FE96", "Row No.{0} - Account Number '{1}' -{2}", rowNumber, value.AccountNum, message);
		}

		void SetAlternateChartAndCompany(Dictionary<ZString, AccAlternateChart> chartDict, AlternateGLAccountsAlternateGLAccount alternateGLAccount, IValueObjectImportContext context, AccAlternateGLAccount newAlternateGLAccount, int rowNumber)
		{
			if (alternateGLAccount.ChartCode.IsEmpty)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("2C08653A-CF68-426D-B171-19A4EB65EDD3", "Row No.{0} - Account Number '{1}' - Chart Code cannot be empty.", rowNumber, alternateGLAccount.AccountNum)));
			}
			else if (chartDict.ContainsKey(alternateGLAccount.ChartCode))
			{
				newAlternateGLAccount.AGA_AAC_AlternateChart = chartDict[alternateGLAccount.ChartCode].PK;
			}
			else
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("56C187AD-24EC-4CDD-8C6C-803032C3D526", "Row No.{0} - Account Number '{1}' - Invalid Chart Code '{2}'.", rowNumber, alternateGLAccount.AccountNum, alternateGLAccount.ChartCode)));
			}
		}

		bool IsNAV(ZString attributeValue)
		{
			return attributeValue == AccountingMasterFilesConstants.NAV.Code;
		}

		ZString GetAttributeMustBeSpecifiedMessage(ZString accountNum, ZString attributeName, ZString parentAccount, ZString chartCode, int rowNumber)
		{
			return Res.GetString("8914DD6E-8C78-4F77-A86F-86C2DA977F0C", "Row No.{0} - Account Number '{1}' - {2} attribute value must be specified for Parent Account '{3}' and Alternate Chart '{4}'. Please check your dissection configuration.", rowNumber, accountNum, attributeName, parentAccount, chartCode);
		}

		ZString GetAttributeCannotBeSpecifiedMessage(ZString accountNum, ZString attributeName, ZString parentAccount, ZString chartCode, int rowNumber)
		{
			return Res.GetString("B60E4E50-8B97-48C4-B174-82374F9FB7DB", "Row No.{0} - Account Number '{1}' - {2} attribute value cannot be specified for Parent Account '{3}' and Alternate Chart '{4}'. Please check your dissection configuration.", rowNumber, accountNum, attributeName, parentAccount, chartCode);
		}
		#endregion

		#region Test metheds/property wrapper
		public void ImportFromValueObjectCore_ForTestOnly(BusinessObjectThatDoesntSave bizObj, AlternateGLAccounts value, IValueObjectImportContext context)
		{
			ImportFromValueObjectCore(bizObj, value, context);
		}
		#endregion
	}
}
