using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[ValueObjectSubclass("Enterprise.DocumentEngine.RuntimeOptions.AutoWorksheet")]
	public class Worksheet : AutoWorksheet
	{
		public Worksheet()
		{ }

		public Worksheet(string name)
		{
			this.Name = name;
		}

		public Worksheet(string name, string title)
		{
			this.Name = name;
			this.Title = title;
		}

		public Worksheet(ColumnHeadingCollection columnHeadings, string name) : this(name)
		{
			this.ColumnHeadings = columnHeadings;
		}

		public Worksheet(ColumnHeadingCollection columnHeadings, string name, string title)
			: this(name, title)
		{
			this.ColumnHeadings = columnHeadings;
		}

		public Worksheet Clone()
		{
			Worksheet clone = new Worksheet(ColumnHeadings.Clone(), Name, Title);
			clone.NameOverride = NameOverride;
			return clone;
		}

		[XmlIgnore]
		public string NameLocalized
		{
			get
			{
				if (!fnameLocalized.IsEmpty)
				{
					return fnameLocalized;
				}
				else
				{
					return Name;
				}
			}
			set
			{
				fnameLocalized = value;
			}
		}
		ZString fnameLocalized;

		[XmlIgnore]
		public ZString NameOverride { get; set; }

		public override string ToString()
		{
			return this.Name;
		}
	}
}
