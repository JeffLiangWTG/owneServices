
namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public enum FileLineType { Numeric, AlphaNumeric, Date }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public class FileDefinitionLine : ColumnDefinitionLine
	{
		public const string NumericType = "数值型";
		public const string AlphaNumericType = "字符型";
		public const string DateType = "日期型";

		public const string FieldSpecifier = "字段";

		public FileDefinitionLine(FileLineType type)
			: base(FieldSpecifier)
		{
			fType = type;
		}

		public override string Value
		{
			get
			{
				return Field + "," + GetFieldTypeInString() + GetFieldDetailInString();
			}
		}

		protected FileLineType fType;
		protected int fLength;
		protected int fPrecision;
		protected string fField;

		public FileLineType Type
		{
			get { return fType; }
		}

		public int Length
		{
			get { return fLength; }
			set { fLength = value; }
		}

		public int Precision
		{
			get { return fPrecision; }
			set { fPrecision = value; }
		}

		public string Field
		{
			get { return fField; }
			set { fField = value; }
		}

		protected string GetFieldTypeInString()
		{
			string returnValue = "";

			switch (fType)
			{
				case FileLineType.AlphaNumeric:
					returnValue = AlphaNumericType;
					break;
				case FileLineType.Date:
					returnValue = DateType;
					break;
				case FileLineType.Numeric:
					returnValue = NumericType;
					break;
			}

			return returnValue;
		}

		protected string GetFieldDetailInString()
		{
			string returnValue = "";

			switch (fType)
			{
				case FileLineType.AlphaNumeric:
					returnValue = "(" + Length + ")";
					break;
				case FileLineType.Date:
					returnValue = "";
					break;
				case FileLineType.Numeric:
					returnValue = "(" + Length + "," + Precision + ")";
					break;
			}

			return returnValue;
		}
	}
}
