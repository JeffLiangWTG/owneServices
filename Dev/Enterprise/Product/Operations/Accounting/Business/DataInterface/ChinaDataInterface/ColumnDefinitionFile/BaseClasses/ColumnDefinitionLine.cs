using System.IO;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class ColumnDefinitionLine
	{
		public ColumnDefinitionLine(string header, string value)
		{
			this.fValue = value;
			this.Header = header;
		}

		public ColumnDefinitionLine(string header)
		{
			this.Header = header;
		}

		public virtual string Value
		{
			get
			{
				return fValue;
			}

			set
			{
				fValue = value;
			}
		}

		public void Write(StreamWriter writer)
		{
			writer.WriteLine(this.ToString());
		}

		public override string ToString()
		{
			return Header + Separator + Value;
		}

		#region Implementation

		protected readonly string Separator = "=";

		protected string Header;
		protected string fValue;

		#endregion
	}
}
