using System;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Xml
{
	public class ZXmlSchema // Can't extend XmlSchema because Write() won't work so we implicit cast instead.
	{
		public ZXmlSchema()
		{
		}

		ZXmlSchema(XmlSchema schema)
		{
			this.inner = schema;
		}

		#region Implicit Casts

		public static implicit operator XmlSchema(ZXmlSchema schema)
		{
			return schema.Inner;
		}

		public static implicit operator ZXmlSchema(XmlSchema schema)
		{
			return new ZXmlSchema(schema);
		}

		#endregion

		#region Proxying to XmlSchema Inner

		public static ZXmlSchema Read(Stream stream, ValidationEventHandler validationEventHandler)
		{
			return new ZXmlSchema(XmlSchema.Read(stream, validationEventHandler)); // This class is the replacement for XmlSchema.
		}

		public static ZXmlSchema Read(TextReader reader, ValidationEventHandler validationEventHandler)
		{
			return new ZXmlSchema(XmlSchema.Read(reader, validationEventHandler)); // This class is the replacement for XmlSchema.
		}

		public XmlSchemaObjectCollection Includes
		{
			get { return Inner.Includes; }
		}

		public XmlSchemaObjectTable Elements
		{
			get { return Inner.Elements; }
		}

		public XmlSchemaObjectCollection Items
		{
			get { return Inner.Items; }
		}

		#endregion

		#region Proxying with Retry

		public void Write(XmlWriter writer)
		{
			Try3Times(delegate
			{ Inner.Write(writer); });
		}

		public void Write(Stream stream)
		{
			Try3Times(delegate
			{ Inner.Write(stream); });
		}

		public void Write(TextWriter writer)
		{
			Try3Times(delegate
			{ Inner.Write(writer); });
		}

		public void Write(XmlWriter writer, XmlNamespaceManager namespaceManager)
		{
			Try3Times(delegate
			{ Inner.Write(writer); });
		}

		public void Write(Stream stream, XmlNamespaceManager namespaceManager)
		{
			Try3Times(delegate
			{ Inner.Write(stream, namespaceManager); });
		}

		public void Write(TextWriter writer, XmlNamespaceManager namespaceManager)
		{
			Try3Times(delegate
			{ Inner.Write(writer, namespaceManager); });
		}

		#endregion

		#region Implementation

		/// <summary>
		/// This is required because a .NET bug causes an ExternalException 'Cannot execute a program' intermittently when
		/// a domain controller goes down. It is a rare bug, but one that almost always recovers.
		/// </summary>
		internal void Try3Times(Try3TimesDelegate action)
		{
			for (int i = 1; i <= 3; i++)
			{
				try
				{
					action();
					break;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (i < 3)
					{
						Thread.Sleep(1000 * i);
					}
					else
					{
						throw;
					}
				}
			}
		}

		XmlSchema Inner
		{
			get
			{
				if (inner == null)
				{
					inner = new XmlSchema(); // This class is the replacement for XmlSchema.
				}

				return inner;
			}
		}

		XmlSchema inner;

		internal delegate void Try3TimesDelegate();

		#endregion
	}
}
