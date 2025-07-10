using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.ZArchitecture
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Doesn't need [Serializable], it's using IXmlSerializable")]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		const string Item = "item";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		const string Key = "key";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		const string Value = "value";

		public SerializableDictionary()
		{
		}

#if NETFRAMEWORK
		protected SerializableDictionary(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			ZXmlSerializer keySerializer = ZXmlSerializer.New(typeof(TKey));
			ZXmlSerializer valueSerializer = ZXmlSerializer.New(typeof(TValue));

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
			{
				return;
			}

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				reader.ReadStartElement(Item);

				reader.ReadStartElement(Key);
				TKey key = (TKey)keySerializer.Deserialize(reader);
				reader.ReadEndElement();

				reader.ReadStartElement(Value);
				TValue value = (TValue)valueSerializer.Deserialize(reader);
				reader.ReadEndElement();

				this.Add(key, value);

				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			ZXmlSerializer keySerializer = ZXmlSerializer.New(typeof(TKey));
			ZXmlSerializer valueSerializer = ZXmlSerializer.New(typeof(TValue));

			foreach (TKey key in this.Keys)
			{
				writer.WriteStartElement(Item);

				writer.WriteStartElement(Key);
				keySerializer.Serialize(writer, key);
				writer.WriteEndElement();

				writer.WriteStartElement(Value);
				TValue value = this[key];
				valueSerializer.Serialize(writer, value);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}

		public override bool Equals(object obj)
		{
			var dictionary = obj as SerializableDictionary<TKey, TValue>;
			return dictionary != null && dictionary.Count == this.Count && dictionary.Keys.SequenceEqual(this.Keys) && dictionary.Values.SequenceEqual(this.Values);
		}

		public override int GetHashCode()
		{
			var hash = 0;

			if (Keys.Count > 0)
			{
				foreach (var item in Keys)
				{
					hash = hash ^ item.GetHashCode();
				}

				foreach (var item in Values)
				{
					hash = hash ^ item.GetHashCode();
				}
			}

			return hash;
		}
	}
}
