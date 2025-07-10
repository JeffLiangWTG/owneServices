using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public abstract class MessageBlock
	{
		protected MessageBlock(string mandatoryCharacters)
		{
			MandatoryCharacters = mandatoryCharacters;
		}

		public readonly string MandatoryCharacters;

		public void Deserialise(string characterBlock)
		{
			if (characterBlock == null)
			{
				throw new ArgumentNullException(nameof(characterBlock));
			}

			if (!characterBlock.StartsWith(MandatoryCharacters))
			{
				throw new InvalidMessageFormatException("Deserialised data does not match mandatory values for ControlIdentifier."
					+ System.Environment.NewLine
					+ "Expected value : " + MandatoryCharacters + System.Environment.NewLine
					+ "Actual value : " + characterBlock.Substring(0, MandatoryCharacters.Length));
			}

			foreach (AttributeFieldInfo attributeFieldInfo in GetAttributeFieldInfos())
			{
				try
				{
					IZType value = attributeFieldInfo.Attribute.DeSerialise(characterBlock);
					attributeFieldInfo.FieldInfo.SetValue(this, value);
				}
				catch (ArgumentException ex)
				{
					string rawValue = attributeFieldInfo.Attribute.GetRawData(characterBlock);
					throw new InvalidMessageFormatException("Error deserialising " + GetType().FullName + "." + attributeFieldInfo.FieldInfo.Name + ", Value = " + rawValue, ex);
				}
				catch (FormatException ex)
				{
					string rawValue = attributeFieldInfo.Attribute.GetRawData(characterBlock);
					throw new InvalidMessageFormatException("Error deserialising " + GetType().FullName + "." + attributeFieldInfo.FieldInfo.Name + ", Value = " + rawValue, ex);
				}
			}
		}

		public string Serialise()
		{
			return Serialise(false);
		}

		class SerialisedValues
		{
			public string Title
			{
				get { return title; }
				set
				{
					ZString fieldName = value;
					for (char c = (char)65; c <= 65 + 25; c++)
					{
						fieldName = fieldName.Replace(c.ToString(), " " + c.ToString());
					}
					fieldName = fieldName.Replace("of ", " of ");
					title = fieldName.TrimEnd();
				}
			}

			public string Value
			{
				get { return _value; }
				set { _value = value.Trim(); }
			}

			string _value;
			string title;
		}

		public string Serialise(bool humanFriendly)
		{
			StringBuilder result = new StringBuilder();
			if (!humanFriendly)
			{
				result.Append(MandatoryCharacters);
			}
			int resultLength = MandatoryCharacters.Length;

			List<SerialisedValues> values = new List<SerialisedValues>();
			string blockName = GetType().Name;
			foreach (AttributeFieldInfo info in GetAttributeFieldInfos())
			{
				SerialisedValues value = new SerialisedValues();
				values.Add(value);

				IZType fieldValue = (IZType)info.FieldInfo.GetValue(this);
				string fieldInfoName = info.FieldInfo.Name;
				MessageBlockAttribute attribute = info.Attribute;
				string serialisedValue = null;

				try
				{
					try
					{
						serialisedValue = attribute.Serialise(fieldValue, humanFriendly);
					}
					catch (MessageBlockSerialisationException serialisationException)
					{
						ErrorReporter.ReportOnce(blockName + fieldInfoName, "Error serialising " + blockName + "." + fieldInfoName + ", Value = " + fieldValue.ToString(), serialisationException);
						serialisedValue = serialisationException.NewInvalidFormat;
					}
					value.Value = serialisedValue;
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException("Error serialising " + blockName + "." + fieldInfoName + ", Value = " + fieldValue.ToString(), ex);
				}
				bool shouldBeMoreExplanatory = humanFriendly && serialisedValue.Trim().Length > 0;
				string fieldValueToAppend = shouldBeMoreExplanatory ? serialisedValue : serialisedValue.PadLeft
					(
						attribute.Offset > resultLength ?
							serialisedValue.Length + attribute.Offset - resultLength
							: serialisedValue.Length
					);

				resultLength += fieldValueToAppend.Length;
				if (humanFriendly)
				{
					value.Title = fieldInfoName + " (" + (attribute.Offset + 1).ToString() + "-" + (attribute.Offset + attribute.Length).ToString() + ")";
				}
				else
				{
					result.Append(fieldValueToAppend);
				}
			}

			if (humanFriendly)
			{
				bool reportSegment = true;
				int maxVisibleCharacters = 0;
				foreach (SerialisedValues value in values)
				{
					if (value.Value.Length > 0)
					{
						reportSegment = true;
						maxVisibleCharacters = Math.Max(value.Title.Length, maxVisibleCharacters);
					}
				}

				if (reportSegment)
				{
					ZString title = blockName;
					title = title.PadLeft((40 - title.Length) / 2, '-').PadRight(40, '-');
					result.AppendLine(title);
					foreach (SerialisedValues value in values)
					{
						if (value.Value.Trim().Length > 0)
						{
							result.AppendLine(value.Title.PadRight(maxVisibleCharacters) + " :" + value.Value);
						}
					}
					result.AppendLine();
				}
			}
			return result.ToString();
		}

#if DEBUG
		internal
#endif
 class AttributeFieldInfo
		{
			public AttributeFieldInfo(MessageBlockAttribute attribute, FieldInfo fieldInfo)
			{
				Attribute = attribute;
				FieldInfo = fieldInfo;
			}
			public readonly MessageBlockAttribute Attribute;
			public readonly FieldInfo FieldInfo;
		}

		static int CompareAttributeFieldInfo(AttributeFieldInfo x, AttributeFieldInfo y)
		{
			if (x == null)
			{
				return (y == null) ? 0 : -1;
			}
			else
			{
				return (y == null) ? 1 : x.Attribute.Offset.CompareTo(y.Attribute.Offset);
			}
		}

#if DEBUG
		internal
#endif
 IEnumerable<AttributeFieldInfo> GetAttributeFieldInfos()
		{
			List<AttributeFieldInfo> result;
			Type blockType = GetType();
			if (!dictionary.TryGetValue(blockType, out result))
			{
				result = new List<AttributeFieldInfo>();
				foreach (FieldInfo fieldInfo in blockType.GetFields())
				{
					MessageBlockAttribute[] attributes = (MessageBlockAttribute[])fieldInfo.GetCustomAttributes(typeof(MessageBlockAttribute), true);
					if (attributes.Length > 1)
					{
						throw new Exception(blockType.FullName + "." + fieldInfo.Name + " has too many 'MessageBlockAttribute'; it should have one.");
					}
					else if (attributes.Length == 1)
					{
						result.Add(new AttributeFieldInfo(attributes[0], fieldInfo));
					}
				}
				result.Sort(CompareAttributeFieldInfo);
				dictionary.Add(blockType, result);
			}
			return result;
		}

		readonly Dictionary<Type, List<AttributeFieldInfo>> dictionary = new Dictionary<Type, List<AttributeFieldInfo>>();
	}
}
