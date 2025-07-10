using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLJournalFlatFileConverter : GLJournalFlatFileConverterBase
	{
		public static string MoreThanOneJournalErrorMsg
		{
			get { return Res.GetString("01c9c90c-8ea5-4a4a-ace6-45a844fccc19", "There is more than one Journal transaction in the CSV file."); }
		}

		static class HeaderTypes
		{
			public const string JournalHeader = "GLJHEAD";
		}

		const string GLJLINE = "GLJLINE";
		const string GLJLINESUBACCOUNT = "GLJLINESUBACCOUNT";

		protected override GLLineConstantsBase GLLineConstantsInstance => new GLLineConstants();

		protected class GLLineConstants : GLLineConstantsBase
		{
			public int Amount => 5;
			public int DRCR => 6;
			public int SubAccountType => 7;
			public int SubAccountCode => 8;
		}

		public GLJournalFlatFileConverter(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			int noOfHeaderSections = 0;
			Xsd.GLJournal journalXsd = (Xsd.GLJournal)valueObject;

			foreach (FlatFileDataRow lineInFile in fileLines)
			{
				var lineType = lineInFile.GetField(HeaderLineType);
				if (lineType == HeaderTypes.JournalHeader)
				{
					noOfHeaderSections++;

					var headerInfo = new GLJournalHeaderInfo();
					SetHeaderInfo(headerInfo, lineInFile, lineType);
					ProcessHeader(headerInfo, journalXsd);
				}
				else if (lineInFile.GetField(GLLineConstantsInstance.LineType) == GLJLINE)
				{
					Xsd.GLJournalJournalLine journalLine = journalXsd.JournalLines.AddNew();
					ProcessLine(lineInFile, journalLine);
				}
				else if (lineInFile.GetField(GLLineConstantsInstance.LineType) == GLJLINESUBACCOUNT)
				{
					ProcessSubAccount(lineInFile, previousJournalLine);
				}
				else
				{
					Notification.Notify(new ErrorNotification(ErrorType.Error, InvalidCsvFileErrorMessage));
				}
			}

			if (noOfHeaderSections > 1)
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, MoreThanOneJournalErrorMsg));
			}
		}

		protected Dictionary<ZString, ZInt> GLJournalHeaderMappingPositions
		{
			get
			{
				if (fGLJournalHeaderMappingPositions == null)
				{
					fGLJournalHeaderMappingPositions = new Dictionary<ZString, ZInt>()
					{
						{ HeaderSchema.LineType, HeaderLineType },
						{ HeaderSchema.JournalType, 1 },
						{ HeaderSchema.Description, 2 },
						{ HeaderSchema.InPeriod, 3 },
						{ HeaderSchema.OutPeriod, 4 },
						{ HeaderSchema.BranchCode, 5 },
						{ HeaderSchema.DepartmentCode, 6 },
					};
				}
				return fGLJournalHeaderMappingPositions;
			}
		}
		Dictionary<ZString, ZInt> fGLJournalHeaderMappingPositions;

		protected override Dictionary<ZString, ZInt> GetHeaderMappingPositions(ZString lineType)
		{
			return GLJournalHeaderMappingPositions;
		}

		protected override void ProcessHeader(GLJournalHeaderInfo headerInfo, Xsd.GLJournal journalXsd)
		{
			base.ProcessHeader(headerInfo, journalXsd);

			journalXsd.GLDetail.Branch = headerInfo.BranchCode;
			journalXsd.GLDetail.Department = headerInfo.DepartmentCode;
		}

		protected override void ProcessLine(FlatFileDataRow lineInFile, Xsd.GLJournalJournalLine journalLine)
		{
			base.ProcessLine(lineInFile, journalLine);

			var gLLineConstants = GLLineConstantsInstance as GLLineConstants;

			if (ZDecimal.TryParse(lineInFile.GetField(gLLineConstants.Amount), out ZDecimal amount))
			{
				journalLine.LocalAmount.Value = amount;
			}

			switch (lineInFile.GetField(gLLineConstants.DRCR))
			{
				case "DR":
					journalLine.DRCR = Xsd.GLJournalJournalLineDRCR.DR;
					break;

				case "CR":
					journalLine.DRCR = Xsd.GLJournalJournalLineDRCR.CR;
					break;
			}

			ImportSubAccountForBackwardCompatibility(lineInFile, journalLine);
		}

		void ImportSubAccountForBackwardCompatibility(FlatFileDataRow lineInFile, Xsd.GLJournalJournalLine journalLineXsd)
		{
			var gLLineConstants = GLLineConstantsInstance as GLLineConstants;

			var subAccountType = lineInFile.GetField(gLLineConstants.SubAccountType);
			if (!subAccountType.IsEmpty)
			{
				var subAccountCode = lineInFile.GetField(gLLineConstants.SubAccountCode);
				var subAccount = journalLineXsd.SubAccounts.AddNew();
				subAccount.Type = new Xsd.SubAccountType();
				subAccount.Type.Code = subAccountType;
				subAccount.Code = subAccountCode;
			}
		}
	}
}
