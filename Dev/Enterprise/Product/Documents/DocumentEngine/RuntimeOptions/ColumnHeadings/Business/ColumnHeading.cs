using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[XmlSerializerAssembly("Enterprise.DocumentEngine.XmlSerializers")]
	[ValueObjectSubclass("Enterprise.DocumentEngine.RuntimeOptions.AutoColumnHeading")]
	public class ColumnHeading : AutoColumnHeading, IXmlSerializable
	{
		public ColumnHeading()
		{
		}
		public ColumnHeading(string displayLabel, string headingText, int originalColumnNumber, int currentPosition, int widthInPixels, bool hidden, string tagName = null)
			: this(displayLabel, null, headingText, originalColumnNumber, currentPosition, widthInPixels, hidden, false, tagName)
		{ }
		public ColumnHeading(string displayLabel, string description, string headingText, int originalColumnNumber, int currentPosition, int widthInPixels, bool hidden, bool showPerformanceWarning, string tagName = "")
			: this(displayLabel, description, headingText, originalColumnNumber, currentPosition, widthInPixels, hidden, tagName)
		{
			ShowPerformanceWarning = showPerformanceWarning;
		}

		public ColumnHeading(string displayLabel)
		{
			this.DisplayLabel = displayLabel;
		}

		public ColumnHeading(string displayLabel, string description, string headingText, int originalColumnNumber, int currentPosition, int widthInPixels, bool hidden, string tagName = "")
			: this(displayLabel)
		{
			Description = description;
			HeadingText = headingText;
			TagName = tagName;
			OriginalColumnNumber = originalColumnNumber;
			CurrentPosition = currentPosition;
			WidthInPixels = widthInPixels;
			Hidden = hidden;
		}

		public ColumnHeading Clone()
		{
			ColumnHeading result = new ColumnHeading(DisplayLabel);
			result.CurrentPosition = CurrentPosition;
			result.Description = Description;
			result.HeadingText = HeadingText;
			result.EnglishHeadingText = EnglishHeadingText;
			result.TagName = TagName;
			result.Hidden = Hidden;
			result.OriginalColumnNumber = OriginalColumnNumber;
			result.WidthInPixels = WidthInPixels;
			result.ShowPerformanceWarning = ShowPerformanceWarning;
			result.HideIfDescriptionEmpty = HideIfDescriptionEmpty;
			return result;
		}

		[XmlIgnoreAttribute]
		public string EnglishHeadingText { get; set; }

		public override string ToString()
		{
			return Description.IsEmpty ? DisplayLabel : Description;
		}

		[XmlIgnoreAttribute]
		public bool ShowPerformanceWarning { get; set; }

		[XmlIgnoreAttribute]
		public bool IsSafeToRemove
		{
			get
			{
				bool result = true;

				if (!Hidden)
				{
					result = false;
				}
				else
				{
					foreach (ColumnHeading referencingColumn in this.ReferencedBy)
					{
						if (!referencingColumn.IsSafeToRemove)
						{
							result = false;
							break;
						}
					}
				}

				return result;
			}
		}

		[XmlIgnoreAttribute]
		public readonly ColumnHeadingCollection ReferencedBy = new ColumnHeadingCollection();

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		#region SuppressResourceStringsCheckRegion
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			while (reader.IsStartElement())
			{
				switch (reader.Name)
				{
					case "ColumnHeading":
						reader.Read();
						break;

					case "DisplayLabel":
						DisplayLabel = reader.ReadElementString();
						break;

					case "OriginalColumnNumber":
						OriginalColumnNumber = int.Parse(reader.ReadElementString());
						break;

					case "Description":
						Description = reader.ReadElementString();
						break;

					case "HeadingText":
						HeadingText = reader.ReadElementString();
						break;

					case "TagName":
						TagName = reader.ReadElementString();
						break;

					case "Hidden":
						Hidden = bool.Parse(reader.ReadElementString());
						break;

					case "WidthInPixels":
					case "Width":
						WidthInPixels = int.Parse(reader.ReadElementString());
						break;

					case "CurrentPosition":
						CurrentPosition = int.Parse(reader.ReadElementString());
						break;

					case "HideIfDescriptionEmpty":
						HideIfDescriptionEmpty = bool.Parse(reader.ReadElementString());
						break;

					default:
						throw new Exception("Unexpected " + reader.Name);
				}
			}
			reader.ReadEndElement();
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("DisplayLabel", DisplayLabel);
			writer.WriteElementString("OriginalColumnNumber", OriginalColumnNumber.ToString());
			writer.WriteElementString("Description", Description);
			writer.WriteElementString("HeadingText", HeadingText);
			writer.WriteElementString("TagName", TagName);
			writer.WriteElementString("Hidden", Hidden.ToString().ToLower());
			writer.WriteElementString("WidthInPixels", WidthInPixels.ToString());
			writer.WriteElementString("CurrentPosition", CurrentPosition.ToString());
			writer.WriteElementString("HideIfDescriptionEmpty", HideIfDescriptionEmpty.ToString().ToLower());
		}

		#endregion
	}
}
