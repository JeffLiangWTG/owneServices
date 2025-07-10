using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public class BalanceSection : FileSection
	{
		public BalanceSection(BusinessObjectFactory factory)
			: base(factory)
		{
			fSectionHeader = "[年初余额]";
			fFileName = "BALANCE.TXT";
		}

		protected override void Define()
		{
			base.Define();
			AccountNumberLine = new FileDefinitionLine(FileLineType.AlphaNumeric);
			LocaLBalanceLine = new FileDefinitionLine(FileLineType.Numeric);
		}

		protected override void SetValue()
		{
			base.SetValue();

			AccountNumberLine.Field = "科目代码";
			AccountNumberLine.Length = 13;

			LocaLBalanceLine.Field = "年初金额";
			LocaLBalanceLine.Length = 15;
			LocaLBalanceLine.Precision = 2;
		}

		protected override void AddLines()
		{
			base.AddLines();
			Lines.Add(AccountNumberLine);
			Lines.Add(LocaLBalanceLine);
		}

		protected FileDefinitionLine AccountNumberLine;
		protected FileDefinitionLine LocaLBalanceLine;
	}
}
