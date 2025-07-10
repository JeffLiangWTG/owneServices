using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer.ReportingBook.AlternateGLAccountWithAttribute
{
	internal class AlternateGLAccountWithAttributeCSVFlatFileConverter : FlatFileConverter
	{
		public const string AlternateGLAccountRowType = "AGACCOUNT";

		public AlternateGLAccountWithAttributeCSVFlatFileConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{ }

		public class AlternateGLAccountsWithAlternateAttributesConstants
		{
			public const int RecordType = 0;
			public const int ChartCode = 1;
			public const int AccountType = 2;
			public const int ParentAccount = 3;
			public const int AccountNum = 4;
			public const int AccountName = 5;
			public const int DebitCredit = 6;
			public const int ReportSection = 7;
			public const int PercentNum = 8;
			public const int ConsolidationNum = 9;
			public const int AlternateNum = 10;
			public const int TotalReference = 11;
			public const int TotalLevel = 12;
			public const int PrintSequence = 13;
			public const int ORGAttrValue = 14;
			public const int OCGAtrrValue = 15;
			public const int LFOAtrrValue = 16;
			public const int LFEAtrrValue = 17;
			public const int TICAtrrValue = 18;
			public const int SPRAtrrValue = 19;
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			var accountsWithAlternateAttributes = (AlternateGLAccounts)valueObject;
			var alternateGLAccounts = accountsWithAlternateAttributes.AlternateGLAccount;
			var rowNumber = 0;

			if (fileLines.Count == 0)
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("19AF3C2D-4C2A-4DE1-AD8F-0524B27245ED", "The file does not contain any record.")));
			}
			else
			{
				foreach (FlatFileDataRow lineInFile in fileLines)
				{
					rowNumber++;
					if (lineInFile[0] == AlternateGLAccountRowType)
					{
						var newAccountWithAlternateAttributes = alternateGLAccounts.AddNew();
						ProcessAlternateGLAccountAndAttributeRow(newAccountWithAlternateAttributes, lineInFile, rowNumber);
					}
					else
					{
						ProcessInvalidRecordTypeRow(lineInFile, rowNumber);
					}
				}
			}
		}

		void ProcessAlternateGLAccountAndAttributeRow(AlternateGLAccountsAlternateGLAccount newAccountWithAlternateAttributes, FlatFileDataRow lineInFile, int rowNumber)
		{
			newAccountWithAlternateAttributes.ChartCode = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.ChartCode];
			newAccountWithAlternateAttributes.AccountType = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.AccountType];
			newAccountWithAlternateAttributes.ParentAccount = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.ParentAccount];
			newAccountWithAlternateAttributes.AccountNum = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.AccountNum];
			newAccountWithAlternateAttributes.AccountName = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.AccountName];
			newAccountWithAlternateAttributes.DebitCredit = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.DebitCredit];
			newAccountWithAlternateAttributes.ReportSection = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.ReportSection];
			newAccountWithAlternateAttributes.PercentNum = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.PercentNum];
			newAccountWithAlternateAttributes.ConsolidationNum = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.ConsolidationNum];
			newAccountWithAlternateAttributes.AlternateNum = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.AlternateNum];
			newAccountWithAlternateAttributes.TotalReference = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.TotalReference];
			newAccountWithAlternateAttributes.ORGAttrValue = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.ORGAttrValue];
			newAccountWithAlternateAttributes.OCGAttrValue = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.OCGAtrrValue];
			newAccountWithAlternateAttributes.LFOAttrValue = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.LFOAtrrValue];
			newAccountWithAlternateAttributes.LFEAttrValue = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.LFEAtrrValue];
			newAccountWithAlternateAttributes.TICAttrValue = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.TICAtrrValue];
			newAccountWithAlternateAttributes.SPRAttrValue = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.SPRAtrrValue];
			var totalLevel = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.TotalLevel];
			var printSequence = lineInFile[AlternateGLAccountsWithAlternateAttributesConstants.PrintSequence];
			if (ZInt.CanParse(totalLevel))
			{
				newAccountWithAlternateAttributes.TotalLevel = ZInt.Parse(totalLevel);
			}
			else
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("E09DBF16-88D3-416F-89E3-57E19A1A403E", "Row {0} - Account Number '{1}' - Invalid Total Level.", rowNumber, newAccountWithAlternateAttributes.AccountNum)));
			}
			if (ZInt.CanParse(printSequence))
			{
				newAccountWithAlternateAttributes.PrintSequence = ZInt.Parse(printSequence);
			}
			else
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("EE444C6D-3BA4-4A55-91C2-6BAD398631D2", "Row {0} - Account Number '{1}' - Invalid Print Sequence.", rowNumber, newAccountWithAlternateAttributes.AccountNum)));
			}
		}

		void ProcessInvalidRecordTypeRow(FlatFileDataRow row, int rowNumber)
		{
			string message;
			if (string.IsNullOrEmpty(row[AlternateGLAccountsWithAlternateAttributesConstants.RecordType]))
			{
				message = Res.GetString("FECF403A-BA9B-4F1F-8926-112754127D77", "Row {0} - Account Number '{1}' - Record Type cannot be empty.", rowNumber, row[AlternateGLAccountsWithAlternateAttributesConstants.AccountNum]);
			}
			else
			{
				message = Res.GetString("C08AFC29-D880-48C9-A93D-E3EB8AFF50C5", "Row {0} - Account Number '{1}' - Record type '{2}' is invalid. A valid Record Type must be 'AGACCOUNT'.", rowNumber, row[AlternateGLAccountsWithAlternateAttributesConstants.AccountNum], row[AlternateGLAccountsWithAlternateAttributesConstants.RecordType]);
			}

			if (Notification != null)
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, message));
			}
		}
	}
}
