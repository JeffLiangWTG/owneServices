using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public abstract class GLJournalFlatFileConverterBase : FlatFileConverter
	{
		public GLJournalFlatFileConverterBase(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
		{
		}

		public static string InvalidCsvFileErrorMessage
		{
			get { return Res.GetString("ea93b0fd-aaef-4822-85d1-5518f8ce95f1", "The journal CSV file you tried to import was invalid."); }
		}

		public static string InvalidLineSubAccountErrorMessage => Res.GetString("B6F2059D-03E4-438b-9942-48F4A7E00890", "There is a sub account which does not belong to any journal line in this CSV file.");

		protected virtual GLLineConstantsBase GLLineConstantsInstance => new GLLineConstantsBase();

		protected class GLLineConstantsBase
		{
			public virtual int LineType => 0;
			public virtual int GLAccount => 1;
			public virtual int Branch => 2;
			public virtual int Department => 3;
			public virtual int Description => 4;
		}

		protected static class GLLineSubAccountConstants
		{
			public const int LineType = 0;
			public const int SubAccountType = 1;
			public const int SubAccountCode = 2;
		}

		#region Header Info
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Property Name")]
		protected static class HeaderSchema
		{
			public const string LineType = "LineType";
			public const string JournalType = "JournalType";
			public const string InPeriod = "InPeriod";
			public const string OutPeriod = "OutPeriod";
			public const string Description = "Description";
			public const string PresentationCategory = "PresentationCategory";
			public const string CompanyCode = "CompanyCode";
			public const string BranchCode = "BranchCode";
			public const string DepartmentCode = "DepartmentCode";
			public const string PostDate = "PostDate";
			public const string ReverseOrEndDate = "ReverseOrEndDate";
		}

		protected const int HeaderLineType = 0;

		protected class GLJournalHeaderInfo
		{
			public ZString JournalType;
			public ZString Description;
			public ZString InPeriod;
			public ZString OutPeriod;
			public ZString CompanyCode;
			public ZString BranchCode;
			public ZString PresentationCategory;
			public ZString DepartmentCode;
			public ZString PostDate;
			public ZString ReverseOrEndDate;
		}

		#endregion

		protected void SetHeaderInfo(GLJournalHeaderInfo headerInfo, FlatFileDataRow lineInFile, ZString lineType)
		{
			var mappingPositions = GetHeaderMappingPositions(lineType);

			headerInfo.JournalType = GetValue(nameof(headerInfo.JournalType));
			headerInfo.Description = GetValue(nameof(headerInfo.Description));
			headerInfo.InPeriod = GetValue(nameof(headerInfo.InPeriod));
			headerInfo.OutPeriod = GetValue(nameof(headerInfo.OutPeriod));
			headerInfo.CompanyCode = GetValue(nameof(headerInfo.CompanyCode));
			headerInfo.BranchCode = GetValue(nameof(headerInfo.BranchCode));
			headerInfo.PresentationCategory = GetValue(nameof(headerInfo.PresentationCategory));
			headerInfo.DepartmentCode = GetValue(nameof(headerInfo.DepartmentCode));
			headerInfo.PostDate = GetValue(nameof(headerInfo.PostDate));
			headerInfo.ReverseOrEndDate = GetValue(nameof(headerInfo.ReverseOrEndDate));

			ZString GetValue(ZString nameOfInfo)
			{
				return mappingPositions.TryGetValue(nameOfInfo, out ZInt position) ? lineInFile.GetField(position) : ZString.Empty;
			}
		}

		protected abstract Dictionary<ZString, ZInt> GetHeaderMappingPositions(ZString lineType);

		protected virtual void ProcessHeader(GLJournalHeaderInfo headerInfo, Xsd.GLJournal journalXsd)
		{
			switch (headerInfo.JournalType)
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

			journalXsd.GLDetail.Description = headerInfo.Description;
			journalXsd.GLDetail.InPeriod = headerInfo.InPeriod;
			journalXsd.GLDetail.OutPeriod = headerInfo.OutPeriod;
		}

		protected virtual void ProcessLine(FlatFileDataRow lineInFile, Xsd.GLJournalJournalLine journalLine)
		{
			journalLine.Account = lineInFile.GetField(GLLineConstantsInstance.GLAccount);
			journalLine.Branch = lineInFile.GetField(GLLineConstantsInstance.Branch);
			journalLine.Department = lineInFile.GetField(GLLineConstantsInstance.Department);
			journalLine.Description = lineInFile.GetField(GLLineConstantsInstance.Description);

			previousJournalLine = journalLine;
		}

		protected virtual void ProcessSubAccount(FlatFileDataRow lineInFile, Xsd.GLJournalJournalLine journalLineXsd)
		{
			if (journalLineXsd == null)
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, InvalidLineSubAccountErrorMessage));
				return;
			}

			AddSubAccounts(journalLineXsd, lineInFile.GetField(GLLineSubAccountConstants.SubAccountType), lineInFile.GetField(GLLineSubAccountConstants.SubAccountCode));
		}

		protected void AddSubAccounts(Xsd.GLJournalJournalLine journalLineXsd, ZString subAccountType, ZString subAccountCode)
		{
			if (!subAccountType.IsEmpty)
			{
				var subAccount = journalLineXsd.SubAccounts.AddNew();
				subAccount.Type = new Xsd.SubAccountType();
				subAccount.Type.Code = subAccountType;
				subAccount.Code = subAccountCode;
			}
		}

		protected Xsd.GLJournalJournalLine previousJournalLine;
	}
}
