using System;
using System.Xml.Serialization;

namespace Enterprise.BusinessObjectGenerator.ModelView
{
	public sealed class AddInfo : IEquatable<AddInfo>
	{
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }

		public string NormalizedName => Name?.Substring((Name?.IndexOf("_") ?? -1) + 1);

		[XmlAttribute(AttributeName = "DataType")]
		public string DataType { get; set; }

		public int? Precision { get; set; }

		[XmlAttribute(AttributeName = "Precision")]
		public string PrecisionText
		{
			get { return Precision.HasValue ? Precision.ToString() : null; }
			set { Precision = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
		}

		public int? Scale { get; set; }

		[XmlAttribute(AttributeName = "Scale")]
		public string ScaleText
		{
			get { return Scale.HasValue ? Scale.ToString() : null; }
			set { Scale = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
		}

		public int? MaxLength { get; set; }

		[XmlAttribute(AttributeName = "MaxLength")]
		public string MaxLengthText
		{
			get { return MaxLength.HasValue ? MaxLength.ToString() : null; }
			set { MaxLength = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
		}

		[XmlAttribute(AttributeName = "Indexed")]
		public bool Indexed { get; set; }

		[XmlAttribute(AttributeName = "IsUnicode")]
		public bool IsUnicode { get; set; }

		[XmlAttribute(AttributeName = "IsNullable")]
		public bool IsNullable { get; set; } = true;

		public bool Equals(AddInfo other)
		{
			if (other == null)
			{
				return false;
			}
			if (Name.Equals(other.Name, System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.ToLowerInvariant().GetHashCode() : 0);
		}

		public static bool operator ==(AddInfo left, AddInfo right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(AddInfo left, AddInfo right)
		{
			return !Equals(left, right);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (ReferenceEquals(obj, null))
			{
				return false;
			}

			return Equals(obj as AddInfo);
		}
	}
}
