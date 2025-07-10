using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLJournalForADAWConverter : MultiCompaniesGLJournalFlatFileConverter
	{
		public GLJournalForADAWConverter(INotifications notify, BusinessObjectFactory factory, IBusinessObjectCollection collection)
			: base(notify, factory)
		{
			Collection = collection as GLJournalHeaderForADAWCollection;
		}

		const string FileHeaderType = "F";
		const string JournalHeaderType = "H";

		readonly GLJournalHeaderForADAWCollection Collection;

		public override bool IsFileHeader => Collection[0].HeaderType == FileHeaderType;

		public override void ImportFlatFile(IValueObject valueObject, IFlatFileFormat flatFileFormat, TextReader reader)
		{
			if (Collection.Count == 0)
			{
				Notification.AddError(Res.GetString("D33FAE50-9C9A-492A-B061-E3355B38838F", @"Uploaded failed due to mapping error.
Please check import file values and file mappings."));
				return;
			}
			var journalsXsd = (GLJournalCollection)valueObject;

			PopulateCompanyAndBranchIfNecessary(Collection);

			var newCollection = ConvertFileCollectionToJournalCollection(Collection);

			foreach (GLJournalHeaderForADAW header in newCollection)
			{
				HeaderNumber = header.Sequence + 1;
				LastJournalCompanyCode = header.CompanyCode;

				var xsdJournal = journalsXsd.AddNew();
				previousJournal = xsdJournal;
				ProcessHeader(header, xsdJournal);
			}
		}

		GLJournalHeaderForADAWCollection ConvertFileCollectionToJournalCollection(GLJournalHeaderForADAWCollection collection)
		{
			if (!IsFileHeader)
			{
				collection.Cast<GLJournalHeaderForADAW>().ForEach(header => header.GLJournalLines.Cast<GLJournalLineForADAW>().ForEach(line =>
				{
					if (line.CompanyCode.IsEmpty && line.BranchCode.IsEmpty)
					{
						line.CompanyCode = header.CompanyCode;
						line.BranchCode = header.BranchCode;
					}
				}));
				return collection;
			}

			var fileHeader = collection.First() as GLJournalHeaderForADAW;
			var headerDictionary = new Dictionary<ZString, GLJournalHeaderForADAW>();
			var journalCollection = new GLJournalHeaderForADAWCollection(fileHeader.Factory, JournalHeaderType);
			foreach (GLJournalLineForADAW journalLine in fileHeader.GLJournalLines)
			{
				GLJournalHeaderForADAW journalHeader;
				if (headerDictionary.ContainsKey(journalLine.CompanyCode))
				{
					journalHeader = headerDictionary[journalLine.CompanyCode];
				}
				else
				{
					journalHeader = journalCollection.AddNew();
					headerDictionary[journalLine.CompanyCode] = journalHeader;

					journalHeader.CompanyCode = journalLine.CompanyCode;
					journalHeader.BranchCode = journalLine.BranchCode;

					journalHeader.JournalType = fileHeader.JournalType;
					journalHeader.PostPeriod = fileHeader.PostPeriod;
					journalHeader.ReverseOrEndPeriod = fileHeader.ReverseOrEndPeriod;
					journalHeader.JournalDescription = fileHeader.JournalDescription;
					journalHeader.PresentationCategory = fileHeader.PresentationCategory;
					journalHeader.PostDate = fileHeader.PostDate;
					journalHeader.ReverseOrEndDate = fileHeader.ReverseOrEndDate;
				}

				journalHeader.GLJournalLines.Add(journalLine);
			}

			return journalCollection;
		}

		void ProcessHeader(GLJournalHeaderForADAW journalHeader, GLJournal journalXsd)
		{
			switch (journalHeader.JournalType)
			{
				case TransactionTypes.GLAutoJournal:
					journalXsd.GLDetail.JournalType = Xsd.GLJournalGLDetailJournalType.AJL;
					break;

				case TransactionTypes.GLReversingJournal:
					journalXsd.GLDetail.JournalType = Xsd.GLJournalGLDetailJournalType.RJL;
					break;

				case TransactionTypes.GLStandardJournal:
					journalXsd.GLDetail.JournalType = Xsd.GLJournalGLDetailJournalType.GJL;
					break;

				case TransactionTypes.GLNoteJournal:
					journalXsd.GLDetail.JournalType = Xsd.GLJournalGLDetailJournalType.NJL;
					break;
			}

			journalXsd.GLDetail.Description = journalHeader.JournalDescription;
			journalXsd.GLDetail.Branch = journalHeader.BranchCode;
			journalXsd.GLDetail.Presentation = journalHeader.PresentationCategory;
			journalXsd.GLDetail.InPeriod = journalHeader.PostPeriod;
			journalXsd.GLDetail.OutPeriod = journalHeader.ReverseOrEndPeriod;

			if (!string.IsNullOrEmpty(journalHeader.PostDate))
			{
				ProcessDate(journalXsd, journalHeader.Company, journalHeader.PostDate, journalHeader.ReverseOrEndDate);
			}

			ValidateHeader(journalHeader);

			ProcessLines(journalHeader.GLJournalLines, journalXsd);
		}

		void ProcessLines(GLJournalLineForADAWCollection lineCollection, GLJournal journalXsd)
		{
			if (lineCollection.Count == 0)
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("4739C59E-4A1E-444F-8A12-2C66A9AF9637", "Journal doesn't contain lines.")));
				return;
			}

			foreach (GLJournalLineForADAW line in lineCollection)
			{
				var xsdJournalLine = journalXsd.JournalLines.AddNew();

				xsdJournalLine.Account = line.GLAccount;
				xsdJournalLine.Branch = line.BranchCode;
				xsdJournalLine.Department = line.DepartmentCode;
				xsdJournalLine.Description = line.JournalLineDescription;
				xsdJournalLine.Currency = line.Currency;

				xsdJournalLine.Organisation = new Organisation();
				xsdJournalLine.Organisation.EDICode = line.OrganisationCode;

				ProcessLineAmount(xsdJournalLine, line);
				ProcessExchangeRate(xsdJournalLine, line.Company, journalXsd.GLDetail.InPeriod, journalXsd.GLDetail.InPeriodDate);

				ValidateLine(xsdJournalLine, line);

				ProcessSubAccounts(line, xsdJournalLine);

				if (AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value)
				{
					AddAttributeValue(xsdJournalLine, line);
					ValidateAttributeValue(xsdJournalLine, line.CompanyCode);
				}
			}
		}

		void AddAttributeValue(GLJournalJournalLine journalLine, GLJournalLineForADAW lineInFile)
		{
			journalLine.ORG = lineInFile.AttributeORG;
			journalLine.OCG = lineInFile.AttributeOCG;
			journalLine.LFO = lineInFile.AttributeLFO;
			journalLine.LFE = lineInFile.AttributeLFE;
			journalLine.TIC = lineInFile.AttributeTIC;
			journalLine.SPR = lineInFile.AttributeSPR;
		}

		void ProcessLineAmount(GLJournalJournalLine xsdJournalLine, GLJournalLineForADAW line)
		{
			if (ZDecimal.TryParse(line.LocalAmount, out ZDecimal localAmount))
			{
				if (localAmount < 0)
				{
					xsdJournalLine.DRCR = Xsd.GLJournalJournalLineDRCR.CR;
				}

				xsdJournalLine.LocalAmount.Value = Math.Abs(localAmount);
			}

			if (ZDecimal.TryParse(line.Amount, out ZDecimal amount))
			{
				if (amount < 0)
				{
					xsdJournalLine.DRCR = Xsd.GLJournalJournalLineDRCR.CR;
				}

				xsdJournalLine.Amount.Value = Math.Abs(amount);
			}
		}

		void ProcessExchangeRate(GLJournalJournalLine xsdJournalLine, GlbCompany company, ZString inPeriod, ZDateTime? inPeriodDate = null)
		{
			var shouldHaveExchangeRate = xsdJournalLine.LocalAmount.Value != 0m && xsdJournalLine.Amount.Value == 0m ||
											xsdJournalLine.LocalAmount.Value == 0m && xsdJournalLine.Amount.Value != 0m;
			var gLAccount = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, xsdJournalLine.Account);
			var currencyBizO = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, xsdJournalLine.Currency);
			if (company != null &&
				!IsLocalCurrency(xsdJournalLine, company) &&
				currencyBizO != null &&
				shouldHaveExchangeRate &&
				int.TryParse(inPeriod, out var period) && period != 0 &&
				gLAccount != null && gLAccount.AG_IsActive)
			{
				var exchangeRate = AccountingUtils.GetGLJournalExchangeRate(Factory, gLAccount.AG_AccountType, xsdJournalLine.Currency, period, GetPostDate(company.PK.ToGuid(), inPeriodDate), company);
				xsdJournalLine.ExchangeRate = exchangeRate;
			}
		}

		#region Company and Branch

		void PopulateCompanyAndBranchIfNecessary(GLJournalHeaderForADAWCollection collection)
		{
			foreach (GLJournalHeaderForADAW header in collection)
			{
				PopulateCompanyAndBranch(header, !IsFileHeader);

				foreach (GLJournalLineForADAW line in header.GLJournalLines)
				{
					PopulateCompanyAndBranch(line, IsFileHeader);
				}
			}
		}

		void PopulateCompanyAndBranch(IJournalCompanyAndBranchForImport journalCompanyAndBranch, bool useCurrentAsDefault)
		{
			if (useCurrentAsDefault && journalCompanyAndBranch.CompanyCode.IsEmpty && journalCompanyAndBranch.BranchCode.IsEmpty)
			{
				journalCompanyAndBranch.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
				journalCompanyAndBranch.BranchCode = GlbBranch.CurrentBranch.GB_Code;
			}
			else if (!journalCompanyAndBranch.CompanyCode.IsEmpty && journalCompanyAndBranch.BranchCode.IsEmpty)
			{
				journalCompanyAndBranch.BranchCode = journalCompanyAndBranch.Company?.FirstActiveBranch.GB_Code ?? ZString.Empty;
			}
			else if (journalCompanyAndBranch.CompanyCode.IsEmpty && !journalCompanyAndBranch.BranchCode.IsEmpty)
			{
				journalCompanyAndBranch.CompanyCode = journalCompanyAndBranch.Branch?.Company.GC_Code ?? ZString.Empty;
			}
		}

		string GetJournalCompanyDoesntMatchLineCompanyMessage(ZString journalCompanyCode, ZString lineCompanyCode) => Res.GetString("CB0A7D09-A432-45DF-B697-742F734AFAC5", "Journal line company code '{0}' does not match journal header company code '{1}'.", lineCompanyCode, journalCompanyCode);

		#endregion

		#region Validation

		void ValidateHeader(GLJournalHeaderForADAW header)
		{
			HeaderNumber = header.Sequence + 1;
			LastJournalCompanyCode = header.CompanyCode;

			if (!IsFileHeader)
			{
				NotifyErrorWithHeaderLocation(header.ValidateCompany());
				NotifyErrorWithHeaderLocation(header.ValidateBranch());
				NotifyErrorWithHeaderLocation(header.ValidateCompanyMatchBranch());
			}

			ValidateJournalType(header);
			ValidatePeriodOrDate(header.Company, header.JournalType, header.PostPeriod, header.ReverseOrEndPeriod, header.PostDate, header.ReverseOrEndDate);
		}

		void ValidateJournalType(GLJournalHeaderForADAW header)
		{
			if (!ValidJournalTypes.Contains(header.JournalType)
					&& (!IsFileHeader || (IsFileHeader && header.Sequence == 0)))
			{
				NotifyErrorWithHeaderLocation(Res.GetString("2A4B4DDD-B192-411A-ADFB-63E0735ED7EE", "Invalid journal type: {0}.", header.JournalType));
			}
		}

		void ValidateLine(GLJournalJournalLine xsdJournalLine, GLJournalLineForADAW journalLine)
		{
			LineNumber = journalLine.Sequence + 1;
			LastJournalCompanyCode = journalLine.CompanyCode;

			NotifyErrorWithLineLocation(journalLine.ValidateCompany());
			NotifyErrorWithLineLocation(journalLine.ValidateBranch());
			NotifyErrorWithLineLocation(journalLine.ValidateCompanyMatchBranch());

			ValidateHeaderMatchLineCompany(journalLine);
			ValidateDepartment(xsdJournalLine.Department);

			ValidateAccount(xsdJournalLine, journalLine.Company);
			ValidateCurrency(journalLine);
			ValidateAmount(xsdJournalLine, journalLine);
			ValidateLocalAmountDecimal(xsdJournalLine.LocalAmount.Value, journalLine.Company?.Country.LocalCurrency);
			ValidateAmountDecimal(xsdJournalLine.Amount.Value, journalLine.CurrencyBizO);
			ValidateExchangeRate(xsdJournalLine, journalLine);
		}

		void ValidateHeaderMatchLineCompany(GLJournalLineForADAW journalLine)
		{
			if (journalLine.Company != null && journalLine.Header.Company != null && journalLine.Company.GC_Code != journalLine.Header.Company.GC_Code)
			{
				NotifyErrorWithLineLocation(GetJournalCompanyDoesntMatchLineCompanyMessage(journalLine.Header.CompanyCode, journalLine.CompanyCode));
			}
		}

		void ValidateCurrency(GLJournalLineForADAW journalLine)
		{
			if (journalLine.CurrencyBizO == null)
			{
				NotifyErrorWithLineLocation(Res.GetString("D3E46A86-5FEA-40C7-9979-52C78CEE26A0", "The currency {0} is invalid or inactive.", journalLine.Currency));
			}
		}

		void ValidateAmount(GLJournalJournalLine xsdJournalLine, GLJournalLineForADAW journalLine)
		{
			if (int.TryParse(journalLine.Amount, out int amount) && int.TryParse(journalLine.LocalAmount, out int localAmount))
			{
				if (amount > 0m && localAmount < 0m || amount < 0m && localAmount > 0m)
				{
					NotifyErrorWithLineLocation(Res.GetString("AA27CF86-59F8-4BCD-BA06-B6E906DEE35A", "Amount and Local Amount must be in the same sign."));
				}
			}

			if (xsdJournalLine.LocalAmount.Value == 0m && xsdJournalLine.Amount.Value == 0m)
			{
				NotifyErrorWithLineLocation(Res.GetString("5D719D8C-DB5A-4CAD-98BA-EB4D86FCF245", "Either Amount or Local Amount must be provided with valid value."));
			}

			if (IsLocalCurrency(xsdJournalLine, journalLine.Company) &&
				xsdJournalLine.Amount.Value != 0m &&
				xsdJournalLine.LocalAmount.Value != 0m &&
				xsdJournalLine.Amount.Value != xsdJournalLine.LocalAmount.Value)
			{
				NotifyErrorWithLineLocation(Res.GetString("18127208-5401-452B-B15C-55A2227F6D2B", "Amount and Local Amount must be same for local currency."));
			}
		}

		void ValidateExchangeRate(GLJournalJournalLine xsdJournalLine, GLJournalLineForADAW journalLine)
		{
			var gLAccount = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, journalLine.GLAccount);

			if (xsdJournalLine.ExchangeRateSpecified && xsdJournalLine.ExchangeRate == 0m && gLAccount != null && gLAccount.AG_IsActive)
			{
				var exchangeRateType = AccountingUtils.GetGLJournalExchangeRateType(journalLine.Company.PK.ToGuid(), gLAccount.AG_AccountType);
				NotifyErrorWithLineLocation(Res.GetString("B93AF54F-C3D6-4126-AC8E-079F8C42CF34", "{0} exchange rate is not setup for currency {1} in period {2}.", exchangeRateType, journalLine.Currency, journalLine.Header.PostPeriod));
			}
		}

		#endregion

		#region SubAccounts

		void ProcessSubAccounts(GLJournalLineForADAW line, GLJournalJournalLine xsdJournalLine)
		{
			AddSubAccounts(xsdJournalLine, line.SubAccountType1, line.SubAccountValue1);
			AddSubAccounts(xsdJournalLine, line.SubAccountType2, line.SubAccountValue2);

			foreach (GLJournalLineSubAccountForADAW lineSubAccount in line.SubAccounts)
			{
				AddSubAccounts(xsdJournalLine, lineSubAccount.SubAccountType, lineSubAccount.SubAccountValue);
			}

			ValidateSubAccount(xsdJournalLine);
		}

		protected void ValidateSubAccount(GLJournalJournalLine journalLineXsd)
		{
			foreach (SubAccount subAccountXsd in journalLineXsd.SubAccounts)
			{
				var subAccountTypeList = new AccountingMasterFilesConstants.SubAccountTypeList().GetAllCodes().ToList();
				if (!subAccountTypeList.Contains(subAccountXsd.Type.Code))
				{
					NotifyErrorWithSubAccountLocation(Res.GetString("88E7B14C-AB36-4EE0-8798-14B0E0EC2D1F", "The Sub Account Type '{0}' is invalid.", subAccountXsd.Type.Code));
				}
				else
				{
					var subAccountPK = Invoices.SubAccountHelper.GetSubAccountPKFromCode(Factory, subAccountXsd.Type.Code, subAccountXsd.Code);
					if (subAccountPK.IsEmpty)
					{
						NotifyErrorWithSubAccountLocation(Res.GetString("09A34282-3072-4320-B4E3-0258DD4E9333", "The Sub Account Code '{0}' is invalid or inactive.", subAccountXsd.Code));
					}
				}
			}
		}

		#endregion
	}
}
