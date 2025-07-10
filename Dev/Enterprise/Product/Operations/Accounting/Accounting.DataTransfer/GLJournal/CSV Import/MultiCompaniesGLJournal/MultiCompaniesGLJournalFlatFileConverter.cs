using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class MultiCompaniesGLJournalFlatFileConverter : GLJournalFlatFileConverterBase
	{
		public MultiCompaniesGLJournalFlatFileConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		protected Xsd.GLJournal previousJournal;

		GLJournalHeaderInfo fileHeaderInfo;

		public int HeaderNumber { get; protected set; }
		protected int LineNumber;

		public int LineCount { get; protected set; }
		bool IsValidAccount { get; set; }
		int rowNumber;
		protected string LastJournalCompanyCode;
		const string GLJLINE = "GLJL";
		const string GLJLINESUBACCOUNT = "GLJS";
		public virtual bool IsFileHeader => fileHeaderInfo != null;

		protected override GLLineConstantsBase GLLineConstantsInstance => new MultiCompaniesGLLineConstants();

		MultiCompaniesGLLineConstants MultiCompaniesGLLineConstantsInstance { get => GLLineConstantsInstance as MultiCompaniesGLLineConstants; }

		class MultiCompaniesGLLineConstants : GLLineConstantsBase
		{
			public int Company => 2;
			public override int Branch => 3;
			public override int Department => 4;
			public int LocalAmount => 5;
			public int Currency => 6;
			public int Amount => 7;
			public override int Description => 8;
			public int OrganisationCode => 9;
			public int SubAccountType1 => 10;
			public int SubAccountValue1 => 11;
			public int SubAccountType2 => 12;
			public int SubAccountValue2 => 13;
			public int AttributeORG => 14;
			public int AttributeOCG => 15;
			public int AttributeLFO => 16;
			public int AttributeLFE => 17;
			public int AttributeTIC => 18;
			public int AttributeSPR => 19;
		}

		enum ImportAttributeEnum
		{
			MustImportAttr,
			CanImportAttr,
			MustNotImportAttr
		}

		static class HeaderTypes
		{
			public const string MultiCompanyFileHeader = "GLJF";
			public const string MultiCompanyJournalHeader = "GLJH";
		}

		Dictionary<ZString, ZInt> MultiCompaniesGLFileHeaderMappingPositions
		{
			get
			{
				if (fMultiCompaniesGLFileHeaderMappingPositions == null)
				{
					fMultiCompaniesGLFileHeaderMappingPositions = new Dictionary<ZString, ZInt>()
					{
						{ HeaderSchema.LineType, HeaderLineType },
						{ HeaderSchema.JournalType, 1 },
						{ HeaderSchema.InPeriod, 2 },
						{ HeaderSchema.OutPeriod, 3 },
						{ HeaderSchema.Description, 4 },
						{ HeaderSchema.PresentationCategory, 5 },
						{ HeaderSchema.PostDate, 6 },
						{ HeaderSchema.ReverseOrEndDate, 7 },
					};
				}
				return fMultiCompaniesGLFileHeaderMappingPositions;
			}
		}
		Dictionary<ZString, ZInt> fMultiCompaniesGLFileHeaderMappingPositions;

		Dictionary<ZString, ZInt> MultiCompaniesGLJournalHeaderMappingPositions
		{
			get
			{
				if (fMultiCompaniesGLJournalHeaderMappingPositions == null)
				{
					fMultiCompaniesGLJournalHeaderMappingPositions = new Dictionary<ZString, ZInt>()
					{
						{ HeaderSchema.LineType, HeaderLineType },
						{ HeaderSchema.CompanyCode, 1 },
						{ HeaderSchema.BranchCode, 2 },
						{ HeaderSchema.JournalType, 3 },
						{ HeaderSchema.InPeriod, 4 },
						{ HeaderSchema.OutPeriod, 5 },
						{ HeaderSchema.Description, 6 },
						{ HeaderSchema.PresentationCategory, 7 },
						{ HeaderSchema.PostDate, 8 },
						{ HeaderSchema.ReverseOrEndDate, 9 },
					};
				}
				return fMultiCompaniesGLJournalHeaderMappingPositions;
			}
		}
		Dictionary<ZString, ZInt> fMultiCompaniesGLJournalHeaderMappingPositions;

		protected override Dictionary<ZString, ZInt> GetHeaderMappingPositions(ZString lineType)
		{
			var result = new Dictionary<ZString, ZInt>();

			switch (lineType)
			{
				case HeaderTypes.MultiCompanyFileHeader:
					result = MultiCompaniesGLFileHeaderMappingPositions;
					break;

				case HeaderTypes.MultiCompanyJournalHeader:
					result = MultiCompaniesGLJournalHeaderMappingPositions;
					break;
			}

			return result;
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			if (ValidateImportFile(fileLines))
			{
				var journalsXsd = (Xsd.GLJournalCollection)valueObject;
				var headerDictionary = new Dictionary<ZString, Xsd.GLJournal>();
				LineCount = 0;
				rowNumber = 0;
				HeaderNumber = 0;

				foreach (FlatFileDataRow lineInFile in fileLines)
				{
					rowNumber++;
					var lineType = lineInFile.GetField(HeaderLineType);
					if (lineType == HeaderTypes.MultiCompanyJournalHeader)
					{
						HeaderNumber++;
						LineNumber = 0;
						var headerInfo = new GLJournalHeaderInfo();
						SetHeaderInfo(headerInfo, lineInFile, lineType);

						var tryGetResult = GetCompanyAndBranch(headerInfo.CompanyCode, headerInfo.BranchCode, useCurrentCompanyAsDefault: true);
						LastJournalCompanyCode = tryGetResult.CompanyCode;
						ValidateCompanyAndBranch(LastJournalCompanyCode, tryGetResult.BranchCode, NotifyErrorWithHeaderLocation);
						headerInfo.CompanyCode = LastJournalCompanyCode;
						headerInfo.BranchCode = tryGetResult.BranchCode;

						previousJournal = journalsXsd.AddNew();
						ProcessHeader(headerInfo, previousJournal);
					}
					else if (lineType == HeaderTypes.MultiCompanyFileHeader)
					{
						LineNumber = 0;
						fileHeaderInfo = new GLJournalHeaderInfo();
						SetHeaderInfo(fileHeaderInfo, lineInFile, lineType);
					}
					else if (lineType == GLJLINE && (previousJournal != null || fileHeaderInfo != null))
					{
						LineCount++;
						LineNumber++;
						if (fileHeaderInfo != null)
						{
							var tryGetResult = GetCompanyAndBranch(lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.Company), lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.Branch), useCurrentCompanyAsDefault: true);
							LastJournalCompanyCode = tryGetResult.CompanyCode;

							if (headerDictionary.ContainsKey(LastJournalCompanyCode))
							{
								previousJournal = headerDictionary[LastJournalCompanyCode];
							}
							else
							{
								previousJournal = journalsXsd.AddNew();
								fileHeaderInfo.CompanyCode = LastJournalCompanyCode;
								fileHeaderInfo.BranchCode = tryGetResult.BranchCode;
								headerDictionary.Add(LastJournalCompanyCode, previousJournal);

								ProcessHeader(fileHeaderInfo, previousJournal);
							}
						}

						if (previousJournal != null)
						{
							var journalLine = previousJournal.JournalLines.AddNew();
							ProcessLine(lineInFile, journalLine, previousJournal.GLDetail.InPeriodDate);
						}
					}
					else if (lineType == GLJLINESUBACCOUNT && previousJournalLine != null)
					{
						ProcessSubAccount(lineInFile, previousJournalLine);
					}
					else
					{
						NotifyErrorWithRowNumber(Res.GetString("7AE36E65-038C-40E1-9D47-41FDF3073AC0", "Line type {0} is invalid.", lineType));
					}
				}
			}
		}

		bool ValidateImportFile(FlatFileDataRowCollection fileLines)
		{
			if (fileLines.Count == 0)
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, (Res.GetString("19AF3C2D-4C2A-4DE1-AD8F-0524B27245ED", "The file does not contain any record."))));
			}
			else
			{
				var lineTypes = fileLines.ToArray().Select(l => l.GetField(HeaderLineType));
				var fileHeaderCount = lineTypes.Count(t => t == HeaderTypes.MultiCompanyFileHeader);
				var journalHeaderCount = lineTypes.Count(t => t == HeaderTypes.MultiCompanyJournalHeader);

				if (fileHeaderCount > 1)
				{
					Notification.Notify(new ErrorNotification(ErrorType.Error, (Res.GetString("CA20516A-A604-45C5-8AAB-EE014FDF267A", "The file contains multiple GJLF records which is not supported."))));
				}
				else if (fileHeaderCount == 1)
				{
					if (journalHeaderCount > 0)
					{
						Notification.Notify(new ErrorNotification(ErrorType.Error, (Res.GetString("4E894F2E-CA38-4219-9039-D3782D5BBEEA", "The file contains both GJLF and GJLH records which is not supported."))));
					}
				}
				else if (journalHeaderCount == 0)
				{
					Notification.Notify(new ErrorNotification(ErrorType.Error, (Res.GetString("50D71F92-32E8-4E65-990A-95DAB5D0E13A", "There is no GLJF nor GLJH record in the file."))));
				}
			}

			return !(Notification is NotificationBuffer notificationBuffer && notificationBuffer.HasErrors);
		}

		(ZString CompanyCode, ZString BranchCode) GetCompanyAndBranch(ZString companyCode, ZString branchCode, bool useCurrentCompanyAsDefault)
		{
			var resultCompanyCode = "";
			var resultBranchCode = "";

			if (companyCode.IsEmpty && branchCode.IsEmpty)
			{
				if (useCurrentCompanyAsDefault)
				{
					resultCompanyCode = GlbCompany.CurrentCompany.GC_Code;
					resultBranchCode = GlbCompany.CurrentCompany.FirstActiveBranch.GB_Code;
				}
			}
			else if (!companyCode.IsEmpty && !branchCode.IsEmpty)
			{
				resultCompanyCode = companyCode;
				resultBranchCode = branchCode;
			}
			else if (companyCode.IsEmpty)
			{
				resultBranchCode = branchCode;
				var tryGetBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
				if (tryGetBranch != null)
				{
					resultCompanyCode = tryGetBranch.Company.GC_Code;
				}
			}
			else
			{
				resultCompanyCode = companyCode;
				var tryGetCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
				if (tryGetCompany != null && tryGetCompany.FirstActiveBranch != null)
				{
					resultBranchCode = tryGetCompany.FirstActiveBranch.GB_Code;
				}
			}

			return (resultCompanyCode, resultBranchCode);
		}

		void ValidateCompanyAndBranch(ZString companyCode, ZString branchCode, Action<string> notifyErrorMethod)
		{
			GlbBranch validBranch = null;

			if (!branchCode.IsEmpty)
			{
				var branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, naturalKeyValue: branchCode);
				if (branch != null && branch.GB_IsActive)
				{
					validBranch = branch;
				}
				else
				{
					notifyErrorMethod(Res.GetString("2FB15199-F46F-4B99-BC88-4E2E31DEFA8A", "The branch code {0} is invalid or inactive.", branchCode));
				}
			}

			if (!companyCode.IsEmpty)
			{
				var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
				if (company != null && company.GC_IsActive)
				{
					if (company.FirstActiveBranch == null)
					{
						notifyErrorMethod(Res.GetString("4FE21CEC-4D71-458A-832F-D0B33A25695D", "There is no active branch in company {0}.", companyCode));
					}
				}
				else
				{
					notifyErrorMethod(Res.GetString("01C5451E-8BC1-4CB6-84AD-95FAAE79A713", "The company code {0} is invalid or inactive.", companyCode));
				}

				if (validBranch != null && validBranch.Company.GC_Code != companyCode)
				{
					notifyErrorMethod(Res.GetString("b349a928-ac27-470c-b829-d5181e929ca8", "The branch code {0} is not valid in company {1}.", validBranch.GB_Code, companyCode));
				}
			}
		}

		protected override void ProcessHeader(GLJournalHeaderInfo headerInfo, Xsd.GLJournal journalXsd)
		{
			base.ProcessHeader(headerInfo, journalXsd);

			CheckJournalType(headerInfo.JournalType);

			journalXsd.GLDetail.Branch = headerInfo.BranchCode;
			journalXsd.GLDetail.Presentation = headerInfo.PresentationCategory;

			if (!string.IsNullOrEmpty(headerInfo.PostDate))
			{
				ProcessDate(journalXsd, Company, headerInfo.PostDate, headerInfo.ReverseOrEndDate);
			}

			ValidatePeriodOrDate(Company, headerInfo.JournalType, headerInfo.InPeriod, headerInfo.OutPeriod, headerInfo.PostDate, headerInfo.ReverseOrEndDate);
		}

		protected void ProcessDate(Xsd.GLJournal journalXsd, GlbCompany company, string postDate, string reverseDate)
		{
			if (company != null)
			{
				var calculator = new AccountingPeriodCalculator(Factory, company);
				if (IsValidDate(postDate, out var parsedPostDate))
				{
					journalXsd.GLDetail.InPeriodDate = new ZDate(parsedPostDate);

					var postPeriod = calculator.GetPeriodFromDate(parsedPostDate);
					journalXsd.GLDetail.InPeriod = postPeriod.ToString();
				}

				if (IsValidDate(reverseDate, out var parsedReverseDate))
				{
					journalXsd.GLDetail.OutPeriodDate = new ZDate(parsedReverseDate);

					var reversePeriod = calculator.GetPeriodFromDate(parsedReverseDate);
					journalXsd.GLDetail.OutPeriod = reversePeriod.ToString();
				}
			}
		}

		GlbCompany Company => Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, LastJournalCompanyCode);

		protected void ValidatePeriodOrDate(GlbCompany company, string journalType, string postPeriod, string agePeriod, string postDate, string reverseOrEndDate)
		{
			if (company != null)
			{
				var calculator = new AccountingPeriodCalculator(Factory, company);
				var isValidPostDate = IsValidDate(postDate, out var parsedPostDate);

				if (!string.IsNullOrEmpty(postDate) && !isValidPostDate)
				{
					NotifyErrorWithHeaderLocation(Res.GetString("46B18B7C-AA60-4809-8791-E1DF35DB229E", "The post date '{0}' is invalid.", postDate));
					return;
				}

				if (isValidPostDate)
				{
					var isValidDueDate = IsValidDate(reverseOrEndDate, out var parsedDueDate);

					if (IsAJLOrRJLType(journalType))
					{
						if (!isValidDueDate)
						{
							NotifyErrorWithHeaderLocation(Res.GetString("44698D68-1F06-4BDF-8F0F-7F36585348C8", "The reverse/end date '{0}' is invalid.", reverseOrEndDate));
							return;
						}

						if (calculator.GetPeriodFromDate(parsedPostDate) == calculator.GetPeriodFromDate(parsedDueDate))
						{
							NotifyErrorWithHeaderLocation(Res.GetString("A5B3EB9A-A950-47E6-980A-4437F2A4CB56", "The reverse/end period must not be in the same period as the post period."));
							return;
						}
					}

					if (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
					{
						if (journalType == TransactionTypes.GLAutoJournal)
						{
							if (!IsLastDayForPeriod(calculator, parsedDueDate) || !IsLastDayForPeriod(calculator, parsedPostDate))
							{
								NotifyErrorWithHeaderLocation(Res.GetString("991A16CE-24AC-4AEE-8DDD-1AABE065E958", "Both the post date '{0}' and the end date '{1}' must be End Date of a period.", postDate, reverseOrEndDate));
							}
						}
					}
					else
					{
						if (!IsLastDayForPeriod(calculator, parsedPostDate))
						{
							NotifyErrorWithHeaderLocation(Res.GetString("E03FDDED-4479-4B97-A44D-2444AB5CE6DB", "The post date '{0}' does not match the 'End Date' of a valid period or the relative period has not been setup. Please check the specified value against your Period Management setup.", postDate));
						}

						switch (journalType)
						{
							case TransactionTypes.GLReversingJournal:
								if (!IsFirstDayForPeriod(calculator, parsedDueDate))
								{
									NotifyErrorWithHeaderLocation(Res.GetString("35D6E9BC-4E86-48B6-83CC-D09FA5706BFB", "The reverse date '{0}' must be Start Date of a period.", reverseOrEndDate));
								}
								break;
							case TransactionTypes.GLAutoJournal:
								if (!IsLastDayForPeriod(calculator, parsedDueDate))
								{
									NotifyErrorWithHeaderLocation(Res.GetString("3D444758-EA1F-4C8E-9B36-DE2E1E24AEAA", "The end date '{0}' must be End Date of a period.", reverseOrEndDate));
								}
								break;
						}
					}
				}
				else
				{
					if (string.IsNullOrEmpty(postPeriod))
					{
						NotifyErrorWithHeaderLocation(Res.GetString("20AA4903-A231-4D67-BB0D-5B79AAB24026", "No Post Date or Post Period specified. Please specify at least one of the values."));
					}
					else if (IsInvalidPeriod(postPeriod, calculator))
					{
						NotifyErrorWithHeaderLocation(Res.GetString("8DB70FF3-4920-4F41-9896-EB57B95AE543", "Post period {0} is invalid. Please check specified value against your Period Management setup.", postPeriod));
					}

					if (IsAJLOrRJLType(journalType))
					{
						if (string.IsNullOrEmpty(agePeriod))
						{
							NotifyErrorWithHeaderLocation(Res.GetString("C1069FB5-0870-468E-A9AD-F295BF88F510", "No Reverse/Ending Period specified. Please specify a value."));
						}
						else if (IsInvalidPeriod(agePeriod, calculator))
						{
							NotifyErrorWithHeaderLocation(Res.GetString("B406742E-871B-45F4-93DD-8A70FBEAE5E5", "Reverse/Ending period {0} is invalid. Please check specified value against your Period Management setup.", agePeriod));
						}
					}
				}
			}
		}

		bool IsInvalidPeriod(string periode, AccountingPeriodCalculator calculator)
		{
			return !int.TryParse(periode, out var parsedPeriod) || !calculator.IsPeriodValid(parsedPeriod);
		}

		bool IsAJLOrRJLType(string journalType)
		{
			return journalType == TransactionTypes.GLAutoJournal || journalType == TransactionTypes.GLReversingJournal;
		}

		bool IsValidDate(string rawDate, out DateTime parsedDate)
		{
			return DateTime.TryParse(rawDate, out parsedDate) || DateTime.TryParseExact(rawDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
		}

		protected bool IsLastDayForPeriod(AccountingPeriodCalculator periodCalculator, DateTime dateTime)
		{
			var date = new ZDateTime(dateTime).Date;
			return date == periodCalculator.GetLastDayForPeriod(dateTime).Date;
		}

		protected bool IsFirstDayForPeriod(AccountingPeriodCalculator periodCalculator, DateTime dateTime)
		{
			var date = new ZDateTime(dateTime).Date;
			return date == periodCalculator.GetFirstDayForPeriod(dateTime).Date;
		}

		void CheckJournalType(ZString journalType)
		{
			if (!ValidJournalTypes.Contains(journalType))
			{
				NotifyErrorWithHeaderLocation(Res.GetString("8a386e1c-8382-4e4c-ae8d-6578f9068f82", "Invalid Journal Type {0} in CSV file.", journalType));
			}
		}

		protected ZString[] ValidJournalTypes
		{
			get
			{
				if (fValidJournalTypes == null)
				{
					fValidJournalTypes = new ZString[]
					{
						TransactionTypes.GLAutoJournal,
						TransactionTypes.GLReversingJournal,
						TransactionTypes.GLStandardJournal,
						TransactionTypes.GLNoteJournal
					};
				}

				return fValidJournalTypes;
			}
		}
		ZString[] fValidJournalTypes;

		protected void ProcessLine(FlatFileDataRow lineInFile, GLJournalJournalLine journalLine, ZDateTime? postDate = null)
		{
			base.ProcessLine(lineInFile, journalLine);

			var journalBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, previousJournal.GLDetail.Branch);
			var journalCompany = journalBranch?.Company;

			var lineCompany = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.Company);
			var tryGetResult = GetCompanyAndBranch(lineCompany, journalLine.Branch, useCurrentCompanyAsDefault: false);

			if (tryGetResult.CompanyCode.IsEmpty && tryGetResult.BranchCode.IsEmpty)
			{
				journalLine.Branch = previousJournal.GLDetail.Branch;
			}

			if (ZDecimal.TryParse(lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.LocalAmount), out ZDecimal localAmount))
			{
				if (localAmount < 0)
				{
					journalLine.DRCR = Xsd.GLJournalJournalLineDRCR.CR;
				}

				journalLine.LocalAmount.Value = Math.Abs(localAmount);
			}

			journalLine.Currency = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.Currency);

			if (ZDecimal.TryParse(lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.Amount), out ZDecimal amount))
			{
				if (amount < 0)
				{
					journalLine.DRCR = Xsd.GLJournalJournalLineDRCR.CR;
				}

				journalLine.Amount.Value = Math.Abs(amount);
			}

			CalculateAmount(journalLine, journalCompany, postDate);

			journalLine.Organisation = new Organisation();
			journalLine.Organisation.EDICode = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.OrganisationCode);

			var subAccountType1 = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.SubAccountType1);
			var subAccountValue1 = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.SubAccountValue1);
			ValidateSubAccount(subAccountType1, subAccountValue1);
			AddSubAccounts(journalLine, subAccountType1, subAccountValue1);

			var subAccountType2 = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.SubAccountType2);
			var subAccountValue2 = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.SubAccountValue2);
			ValidateSubAccount(subAccountType2, subAccountValue2);
			AddSubAccounts(journalLine, subAccountType2, subAccountValue2);

			ValidateLine(lineInFile, journalLine);
			if (AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value)
			{
				AddAttributeValue(journalLine, lineInFile);
				ValidateAttributeValue(journalLine, lineCompany);
			}
		}

		#region Line Validation

		void ValidateLine(FlatFileDataRow lineInFile, GLJournalJournalLine journalLine)
		{
			var journalBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, previousJournal.GLDetail.Branch);
			var journalCompany = journalBranch?.Company;

			var lineCompanyCode = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.Company);
			var tryGetResult = GetCompanyAndBranch(lineCompanyCode, journalLine.Branch, useCurrentCompanyAsDefault: false);

			if (!tryGetResult.CompanyCode.IsEmpty || !tryGetResult.BranchCode.IsEmpty)
			{
				ValidateCompanyAndBranch(tryGetResult.CompanyCode, tryGetResult.BranchCode, NotifyErrorWithLineLocation);
				if (journalCompany != null && !tryGetResult.CompanyCode.IsEmpty && tryGetResult.CompanyCode != journalCompany.GC_Code)
				{
					NotifyErrorWithLineLocation(Res.GetString("CB0A7D09-A432-45DF-B697-742F734AFAC5", "Journal line company code '{0}' does not match journal header company code '{1}'.", tryGetResult.CompanyCode, journalCompany.GC_Code));
				}
			}

			var lineCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, tryGetResult.CompanyCode) ?? GlbCompany.CurrentCompany;

			ValidateDepartment(journalLine.Department);
			ValidateAccount(journalLine, lineCompany);
			ValidateLocalAmountDecimal(journalLine.LocalAmount.Value, lineCompany.Country.LocalCurrency);

			var oSCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, journalLine.Currency) ?? lineCompany.Country.LocalCurrency;
			ValidateAmountDecimal(journalLine.Amount.Value, oSCurrency);
		}

		protected void ValidateDepartment(string departmentCode)
		{
			if (string.IsNullOrEmpty(departmentCode))
			{
				NotifyErrorWithLineLocation(Res.GetString("37ECC5C1-9ADC-46E3-9EF7-85DBDF1FA923", "The department code must not be empty."));
			}
			else
			{
				var department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode);
				if (department == null || !department.GE_IsActive)
				{
					NotifyErrorWithLineLocation(Res.GetString("7D5E45AB-E57E-46DF-A317-0B1B1A1C0D9B", "The department code {0} is invalid or inactive.", departmentCode));
				}
			}
		}

		protected void ValidateAccount(GLJournalJournalLine xsdJournalLine, GlbCompany company)
		{
			if (string.IsNullOrEmpty(xsdJournalLine.Account))
			{
				NotifyErrorWithLineLocation(Res.GetString("58EBAF8B-C6EE-42CE-BEC5-E677E6F3DC75", "The account number must not be empty."));
			}
			else
			{
				var gLAccount = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, xsdJournalLine.Account);
				if (gLAccount == null || !gLAccount.AG_IsActive)
				{
					NotifyErrorWithLineLocation(Res.GetString("8B558081-3681-468F-9CBC-6A098F32E1D0", "The account number {0} is invalid or inactive.", xsdJournalLine.Account));
				}
				else if (previousJournal.GLDetail.JournalType != GLJournalGLDetailJournalType.NJL && gLAccount.AG_AccountType != AccountType.Note && !IsLocalCurrency(xsdJournalLine, company))
				{
					if (gLAccount.AG_ControlAccount || GLJournalLineHelper.IsGlAccountConfiguredAsPlAppropriationAccount(gLAccount.PK.ToGuid()) || GLJournalLineHelper.IsGlAccountConfiguredAsControlOrLinkAccount(gLAccount.PK.ToGuid()))
					{
						NotifyErrorWithLineLocation(Res.GetString("D30E4415-A2DB-4796-B5EF-8DD5223FF020", "{0}: For a foreign currency line you can not post to a control account or any account that is configured as PL Appropriation, Control or Link Account in the Registry", xsdJournalLine.Account));
					}
				}
				else if (gLAccount.AG_ControlAccount && !Env.Security.GeneralLedgerJournalPostToControlAccounts.IsAllowed)
				{
					NotifyErrorWithLineLocation(Res.GetString("003C23EA-1EF1-400A-9B5D-13E80555F1C9", "{0}: This account is flagged as a control account. You do not have security rights to post to control accounts.\r\n{1}", xsdJournalLine.Account, Env.Security.GeneralLedgerJournalPostToControlAccounts.ErrorMessageForNotAllowed));
				}
				else
				{
					IsValidAccount = true;
				}

				if (gLAccount != null && !gLAccount.AG_IsGlobal && company != null && !gLAccount.CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Any(x => x.ACF_GC_Company == company.PK))
				{
					NotifyErrorWithLineLocation(Res.GetString("40E9E399-BA8E-4879-BE32-C3BBB056B164", "{0}: This GL account cannot be used for posting in this company.", xsdJournalLine.Account));
				}
				else
				{
					IsValidAccount = true;
				}
			}
		}

		protected void ValidateLocalAmountDecimal(ZDecimal localAmount, RefCurrency localCurrency)
		{
			if (localCurrency != null && localAmount.DecimalPlaces > localCurrency.Decimals)
			{
				NotifyErrorWithLineLocation(Res.GetString("638CBCA1-1776-48AD-B47B-52EE0337D810", "Local currency {0} only allows entering amounts up to {1} decimal places. Local Amount: {2}", localCurrency.Code, localCurrency.Decimals, localAmount));
			}
		}

		protected void ValidateAmountDecimal(ZDecimal amount, RefCurrency currency)
		{
			if (currency != null && amount.DecimalPlaces > currency.Decimals)
			{
				NotifyErrorWithLineLocation(Res.GetString("2664B52C-2F57-481D-977F-0581CA0A8E51", "Currency {0} only allows entering amounts up to {1} decimal places. Amount: {2}", currency.Code, currency.Decimals, amount));
			}
		}

		void CalculateAmount(GLJournalJournalLine journalLine, GlbCompany journalCompany, ZDateTime? postDate = null)
		{
			var gLAccount = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, journalLine.Account);

			if (journalCompany == null || !journalCompany.GC_IsActive || gLAccount == null || !gLAccount.AG_IsActive)
			{
				return;
			}

			if (!IsLocalCurrency(journalLine, journalCompany) && IsOSAmountOrLocalAmountProvided(journalLine))
			{
				int.TryParse(previousJournal.GLDetail.InPeriod, out var postPeriod);

				var exchangeRate = AccountingUtils.GetGLJournalExchangeRate(Factory, gLAccount.AG_AccountType, journalLine.Currency, postPeriod, GetPostDate(journalCompany.PK.ToGuid(), postDate), journalCompany);

				if (exchangeRate == 0m)
				{
					var exchangeRateType = AccountingUtils.GetGLJournalExchangeRateType(journalCompany.PK.ToGuid(), gLAccount.AG_AccountType);
					NotifyErrorWithLineLocation(Res.GetString("B93AF54F-C3D6-4126-AC8E-079F8C42CF34", "{0} exchange rate is not setup for currency {1} in period {2}.", exchangeRateType, journalLine.Currency, postPeriod));

					return;
				}

				if (journalLine.LocalAmount.Value == 0)
				{
					journalLine.LocalAmount.Value = journalCompany.GetExchangeRate().ForeignToLocal(journalLine.Amount.Value, exchangeRate);
				}
			}
		}

		protected ZDateTime? GetPostDate(Guid companyPK, ZDateTime? postDate)
		{
			return AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty) ? postDate : null;
		}

		protected bool IsLocalCurrency(GLJournalJournalLine journalLine, ICompany company)
		{
			return !journalLine.CurrencySpecified || journalLine.Currency == company?.LocalCurrency.Code;
		}

		bool IsOSAmountOrLocalAmountProvided(GLJournalJournalLine journalLine)
		{
			return journalLine.LocalAmount.Value == 0 || journalLine.Amount.Value == 0;
		}

		#endregion

		protected override void ProcessSubAccount(FlatFileDataRow lineInFile, GLJournalJournalLine journalLineXsd)
		{
			ValidateSubAccount(lineInFile.GetField(GLLineSubAccountConstants.SubAccountType), lineInFile.GetField(GLLineSubAccountConstants.SubAccountCode));
			base.ProcessSubAccount(lineInFile, journalLineXsd);
		}

		void ValidateSubAccount(string subAccountType, string subAccountCode)
		{
			if (!string.IsNullOrEmpty(subAccountType))
			{
				var subAccountTypeList = new AccountingMasterFilesConstants.SubAccountTypeList().GetAllCodes().ToList();
				if (!subAccountTypeList.Contains(subAccountType))
				{
					NotifyErrorWithSubAccountLocation(Res.GetString("88E7B14C-AB36-4EE0-8798-14B0E0EC2D1F", "The Sub Account Type '{0}' is invalid.", subAccountType));
				}
				else if (!string.IsNullOrEmpty(subAccountCode))
				{
					var subAccountPK = Invoices.SubAccountHelper.GetSubAccountPKFromCode(Factory, subAccountType, subAccountCode);
					if (subAccountPK.IsEmpty)
					{
						NotifyErrorWithSubAccountLocation(Res.GetString("09A34282-3072-4320-B4E3-0258DD4E9333", "The Sub Account Code '{0}' is invalid or inactive.", subAccountCode));
					}
				}
			}
		}

		void AddAttributeValue(GLJournalJournalLine journalLine, FlatFileDataRow lineInFile)
		{
			journalLine.ORG = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.AttributeORG);
			journalLine.OCG = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.AttributeOCG);
			journalLine.LFO = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.AttributeLFO);
			journalLine.LFE = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.AttributeLFE);
			journalLine.TIC = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.AttributeTIC);
			journalLine.SPR = lineInFile.GetField(MultiCompaniesGLLineConstantsInstance.AttributeSPR);
		}

		protected void ValidateAttributeValue(GLJournalJournalLine journalLine, ZString companyCode)
		{
			if (IsValidAccount)
			{
				var dissections = LoadDissections(journalLine.Account);
				var attributeDict = dissections.GroupBy(d => d.ADC_Attribute).ToDictionary(g => g.Key, g => g.AsEnumerable());
				ValidateAttributeValueCore(attributeDict, journalLine, dissections.Count != 0 ? dissections[0].ADC_AG_GLHeader : ZGuid.Empty, companyCode);
			}
		}

		void ValidateAttributeValueCore(Dictionary<ZString, IEnumerable<AccAlternateGLAccountDissection>> attributeDict, GLJournalJournalLine journalLine, ZGuid glHeaderPK, ZString companyCode)
		{
			CheckAttr(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, journalLine.ORG, attributeDict, journalLine.Account, glHeaderPK, journalLine.Branch, companyCode);
			CheckAttr(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, journalLine.OCG, attributeDict, journalLine.Account, glHeaderPK);
			CheckAttr(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, journalLine.LFO, attributeDict, journalLine.Account, glHeaderPK);
			CheckAttr(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, journalLine.LFE, attributeDict, journalLine.Account, glHeaderPK);
			CheckAttr(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, journalLine.TIC, attributeDict, journalLine.Account, glHeaderPK);
			CheckAttr(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, journalLine.SPR, attributeDict, journalLine.Account, glHeaderPK);
		}

		void CheckAttr(ZString attr, ZString attrValue, Dictionary<ZString, IEnumerable<AccAlternateGLAccountDissection>> dissectionDict, ZString account, ZGuid glHeaderPK, string branchCode = "", string companyCode = "")
		{
			var importAttributeEnum = GetImportAttributeEnum(dissectionDict, attr);
			
			if (string.IsNullOrEmpty(attrValue))
			{
				if (importAttributeEnum == ImportAttributeEnum.MustImportAttr)
				{
					NotifyErrorWithLineLocation(Res.GetString("FEA994BF-0C40-468A-8084-E7EA3CB0AB1D", "{0} attribute value must be specified for GL Account '{1}' because separate numbering is required. Please check your dissection configuration.", attr, account));
				}
			}
			else
			{
				if (importAttributeEnum == ImportAttributeEnum.MustNotImportAttr)
				{
					NotifyErrorWithLineLocation(Res.GetString("DB5AAFFA-0492-4BD2-A9D3-0B478653AFA8", "{0} attribute value cannot be specified for GL Account '{1}'. Please check your dissection configuration.", attr, account));
				}
				else
				{
					if (attr == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
					{
						var branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
						var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
						var organization = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, attrValue);
						if (organization != null && (organization.IsDebtorForCompany(branch?.Company) || organization.IsDebtorForCompany(company)) && AccountingConfigurationRegistry.Instance.ARControlAccount.Value == glHeaderPK)
						{
						}
						else if (organization != null && (organization.IsCreditorForCompany(branch?.Company) || organization.IsCreditorForCompany(company)) && AccountingConfigurationRegistry.Instance.APControlAccount.Value == glHeaderPK)
						{
						}
						else
						{
							NotifyErrorWithLineLocation(Res.GetString("66E1657D-D4DF-4EC8-ACDC-FF5EAD838146", "Invalid Attribute Value. A valid Attribute Value must be the code of a valid Receivable Organization (if Parent Account is an AR Control Account) or a valid Payable Organization (if Parent Account is an AP Control Account)."));
						}
					}
					else if (attr == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG && !AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.Cast<CodeDescriptionWithGroup>().Select(x => x.Group).ToHashSet().Contains(attrValue) && attrValue != AccountingMasterFilesConstants.NAV.Code)
					{
						NotifyErrorWithLineLocation(Res.GetString("011DEFF0-0A44-4891-9AE4-57247E7356E0", "Invalid Attribute Value. A valid Attribute Value must be one of the following Class defined in Consolidated Accounting Category List registry or value 'NAV'."));
					}
					else if (attr == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO && !AccountingMasterFilesConstants.LFOList.GetAllCodes().ToList().Contains(attrValue))
					{
						NotifyErrorWithLineLocation(Res.GetString("5853B6B1-797B-41B3-BB70-8FC19D749CB2", "Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC' ,'FOR' or 'NAV'.\r\nLOC = Local.\r\nFOR = Foreign.\r\nNAV = No Attribute Value."));
					}
					else if (attr == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE && !AccountingMasterFilesConstants.LFEList.GetAllCodes().ToList().Contains(attrValue))
					{
						NotifyErrorWithLineLocation(Res.GetString("D9CBE9D6-AD93-4155-A7C1-D53D9931471A", "Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'LOC', 'WEU' ,'OEU' or 'NAV'.\r\nLOC = Local.\r\nWEU = Within EU.\r\nOEU = Outside EU.\r\nNAV = No Attribute Value."));
					}
					else if (attr == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC && !AccountingMasterFilesConstants.TICList.GetAllCodes().ToList().Contains(attrValue))
					{
						NotifyErrorWithLineLocation(Res.GetString("94B219D0-EB30-4ACC-A2A9-7D75E4EEB3E9", "Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'STI' or 'ETI' or 'NAV'.\r\nSTI = Standard Tax IDs.\r\nETI = Tax ID with Extra Tax.\r\nNAV = No Attribute Value."));
					}
					else if (attr == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR && !AccountingMasterFilesConstants.SPRList.GetAllCodes().ToList().Contains(attrValue))
					{
						NotifyErrorWithLineLocation(Res.GetString("81480F72-0CFE-44FA-88A1-95C472949F89", "Invalid Attribute Value. A valid Attribute Value must be one of the following values: 'SPS' , 'SPR' or 'NAV'.\r\nSPS = Sales/Purchases.\r\nSPR = Sales/Purchases Returns.\r\nNAV = No Attribute Value."));
					}
				}
			}
		}

		ImportAttributeEnum GetImportAttributeEnum(Dictionary<ZString, IEnumerable<AccAlternateGLAccountDissection>> dissectionDict, ZString attr)
		{
			ImportAttributeEnum importAttributeEnum;
			if (dissectionDict.TryGetValue(attr, out var dissections))
			{
				if (dissections.Any(d => d.ADC_SeparateNumbering))
				{
					importAttributeEnum = ImportAttributeEnum.MustImportAttr;
				}
				else
				{
					importAttributeEnum = ImportAttributeEnum.CanImportAttr;
				}
			}
			else
			{
				importAttributeEnum = ImportAttributeEnum.MustNotImportAttr;
			}
			return importAttributeEnum;
		}

		List<AccAlternateGLAccountDissection> LoadDissections(ZString account)
		{
			var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccountDissection));
			var subQuery = new ZDBOnlySubQuery(typeof(AccGLHeader), AccGLHeaderSchema.PK, AccAlternateGLAccountDissectionSchema.ADC_AG_GLHeader);
			subQuery.AddToFilter(AccGLHeaderSchema.AG_AccountNum, account);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return Factory.Load<AccAlternateGLAccountDissection>(query).ToList();
		}

		void NotifyErrorWithRowNumber(string message)
		{
			var errorWithRowNumber = Res.GetString("23A840D1-E744-444C-B2F1-85D9C0A55E62", "Row {0} - {1}", rowNumber, message);
			Notification.Notify(new ErrorNotification(ErrorType.Error, errorWithRowNumber));
		}

		protected void NotifyErrorWithHeaderLocation(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				var error = Res.GetString("20A7A94A-71B8-4DA3-B8A6-CD333B7F2A81", "{0}.{1} - {2}", GetHeaderLocation(), LastJournalCompanyCode, message);
				Notification.Notify(new ErrorNotification(ErrorType.Error, error));
			}
		}

		protected void NotifyErrorWithLineLocation(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				var error = Res.GetString("5D0D0905-1E12-4F58-B74C-D95D789C6255", "{0}.{1} - {2}", GetLineLocation(), LastJournalCompanyCode, message);
				Notification.Notify(new ErrorNotification(ErrorType.Error, error));
			}
		}

		protected void NotifyErrorWithSubAccountLocation(string message)
		{
			var error = Res.GetString("B7E1A4BF-3E03-47E7-A5CF-A21D24AEE75C", "{0}.Sub Account.{1} - {2}", GetLineLocation(), LastJournalCompanyCode, message);
			Notification.Notify(new ErrorNotification(ErrorType.Error, error));
		}

		string GetHeaderLocation()
		{
			if (IsFileHeader)
			{
				return Res.GetString("9AFA884E-DEA7-4C79-B4CB-A5FF0AF98A04", "File");
			}
			else
			{
				return Res.GetString("1E0EE161-D3EE-4549-8FED-1219110A4D4F", "Header[{0}]", HeaderNumber);
			}
		}

		string GetLineLocation()
		{
			string prefix = GetHeaderLocation();
			return Res.GetString("D47E0C19-B158-4ACD-883E-442A15AEE217", "{0}.Line[{1}]", prefix, LineNumber);
		}
	}
}
