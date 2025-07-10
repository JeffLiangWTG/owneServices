using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	/// <summary>
	/// An XmlTextReader that filters the xml content by the elements and attributes that exist in the given XmlSchema.
	/// </summary>
	public class XmlTextReaderFilter : XmlTextReader
	{
		public XmlTextReaderFilter(XmlSchema schema, string xmlFragment, XmlNodeType fragType, XmlParserContext context) : base(xmlFragment, fragType, context)
		{
			Init(schema);
		}

		public XmlTextReaderFilter(XmlSchema schema, Stream xmlFragment, XmlNodeType fragType, XmlParserContext context) : base(xmlFragment, fragType, context)
		{
			Init(schema);
		}

		#region Element Filtering

		public override bool Read()
		{
			bool result = base.Read();
			if (result && SkippingSemaphore == 0)
			{
				AfterRead();
			}
			CurrentAttributeIndex = -1;
			return result;
		}

		void AfterRead()
		{
			if (NodeType == XmlNodeType.Element)
			{
				SchemaNavigator.NavigateToElement(LocalName, IsEmptyElement);
				SkipElementsNotInSchema();
			}
			if (NodeType == XmlNodeType.EndElement)
			{
				SchemaNavigator.NavigateBack(true);
			}
		}

		void SkipElementsNotInSchema()
		{
			bool elementDoesntExistInSchema;
			do
			{
				elementDoesntExistInSchema = (SchemaNavigator.CurrentElement == null && !(SchemaNavigator.Current is XmlSchemaAny));
				if (elementDoesntExistInSchema)
				{
					Skip();
				}
			}
			while (elementDoesntExistInSchema && NodeType == XmlNodeType.Element);
		}

		public override void Skip()
		{
			if (SkippingSemaphore == 0)
			{
				if (NodeType == XmlNodeType.Element)
				{
					SchemaNavigator.NavigateBack();
				}
			}

			SkippingSemaphore++;
			try
			{
				base.Skip();
			}
			finally
			{
				SkippingSemaphore--;
			}

			if (SkippingSemaphore == 0)
			{
				FixupCurrentSchemaNavigationPositionAfterSkip();
			}
		}

		void FixupCurrentSchemaNavigationPositionAfterSkip()
		{
			if (NodeType == XmlNodeType.Element)
			{
				SchemaNavigator.NavigateToElement(LocalName, IsEmptyElement);
				SkipElementsNotInSchema();
			}
			else if (NodeType == XmlNodeType.EndElement)
			{
				SchemaNavigator.NavigateBack();
			}
		}

		#endregion

		#region Attribute Filtering

		public override bool MoveToFirstAttribute()
		{
			bool result = base.MoveToFirstAttribute();
			while (result && !IsNameSpaceAttribute() && !SchemaNavigator.CurrentElementHasAttribute(LocalName))
			{
				result = base.MoveToNextAttribute();
			}
			if (!result)
			{
				MoveToElement();
			}
			CurrentAttributeIndex = 0;
			return result;
		}

		public override bool MoveToNextAttribute()
		{
			bool result = base.MoveToNextAttribute();
			while (result && !IsNameSpaceAttribute() && !SchemaNavigator.CurrentElementHasAttribute(LocalName))
			{
				result = base.MoveToNextAttribute();
			}
			if (result)
			{
				CurrentAttributeIndex++;
			}
			else if (CurrentAttributeIndex <= 0)
			{
				MoveToElement();
			}
			return result;
		}

		public override void MoveToAttribute(int i)
		{
			MoveToFirstAttribute();

			int current = 0;
			while (current < i && MoveToNextAttribute())
			{
				current++;
			}
			CurrentAttributeIndex = i;
		}

		public override string GetAttribute(int i)
		{
			using (new AttributePositionPreserver(this))
			{
				MoveToAttribute(i);
				return Value;
			}
		}

		public override bool HasAttributes
		{
			get { return AttributeCount > 0; }
		}

		public override int AttributeCount
		{
			get
			{
				using (new AttributePositionPreserver(this))
				{
					int result = 0;
					if (MoveToFirstAttribute())
					{
						do
						{
							result++;
						}
						while (MoveToNextAttribute());
					}
					return result;
				}
			}
		}

		#endregion

		#region Unimplemented Members

		public override bool MoveToAttribute(string name)
		{
			throw new NotImplementedException();
		}

		public override bool MoveToAttribute(string name, string ns)
		{
			throw new NotImplementedException();
		}

		public override string GetAttribute(string name)
		{
			throw new NotImplementedException();
		}

		public override string GetAttribute(string name, string namespaceURI)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation

		XmlSchemaNavigator SchemaNavigator;
		int CurrentAttributeIndex;
		int SkippingSemaphore;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "xml namespace attribute name")]
		public bool IsNameSpaceAttribute()
		{
			return (!string.IsNullOrEmpty(Prefix) ? Prefix : Name) == "xmlns";
		}

		class AttributePositionPreserver : IDisposable
		{
			readonly XmlTextReaderFilter Reader;
			readonly int SavedIndex;

			public AttributePositionPreserver(XmlTextReaderFilter reader)
			{
				this.Reader = reader;
				this.SavedIndex = reader.CurrentAttributeIndex;
			}

			public void Dispose()
			{
				if (SavedIndex == -1)
				{
					Reader.MoveToElement();
				}
				else
				{
					Reader.MoveToAttribute(SavedIndex);
				}
				Reader.CurrentAttributeIndex = SavedIndex;
			}
		}

		void Init(XmlSchema schema)
		{
			this.SchemaNavigator = new XmlSchemaNavigator(schema);
		}

		#endregion
	}
}
