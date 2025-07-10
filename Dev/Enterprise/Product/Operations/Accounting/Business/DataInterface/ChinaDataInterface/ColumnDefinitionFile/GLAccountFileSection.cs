using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	/// <summary>
	/// Summary description for GLAccountFile.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public class GLAccountFileSection : FileSection
	{
		public GLAccountFileSection(BusinessObjectFactory factory)
			: base(factory)
		{
			fSectionHeader = "[科目]";
			fFileName = "ACCOUNT.TXT";
		}

		protected override void Define()
		{
			base.Define();
			AccountFormatLine = new ColumnDefinitionLine("科目结构");
			AccountFormatSeparatorLine = new ColumnDefinitionLine("分隔符");
			AccountNameLine = new FileDefinitionLine(FileLineType.AlphaNumeric);
			AccountNumberLine = new FileDefinitionLine(FileLineType.AlphaNumeric);
			DebitCreditLine = new FileDefinitionLine(FileLineType.Numeric);
		}

		protected override void SetValue()
		{
			base.SetValue();
			AccountFormatLine.Value = "8,3";
			AccountFormatSeparatorLine.Value = ".";

			AccountNumberLine.Field = "科目代码";
			AccountNumberLine.Length = 13;

			AccountNameLine.Field = "科目名称";
			AccountNameLine.Length = 40;

			DebitCreditLine.Field = "借贷方向";
			DebitCreditLine.Length = 2;
			DebitCreditLine.Precision = 0;
		}

		protected override void AddLines()
		{
			base.AddLines();
			Lines.Add(AccountFormatLine);
			Lines.Add(AccountFormatSeparatorLine);
			Lines.Add(AccountNumberLine);
			Lines.Add(AccountNameLine);
			Lines.Add(DebitCreditLine);
		}

		protected ColumnDefinitionLine AccountFormatLine;
		protected ColumnDefinitionLine AccountFormatSeparatorLine;
		protected FileDefinitionLine AccountNumberLine;
		protected FileDefinitionLine AccountNameLine;
		protected FileDefinitionLine DebitCreditLine;
	}
}
