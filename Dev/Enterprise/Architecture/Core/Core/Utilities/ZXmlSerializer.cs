using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;
using CargoWise.Common.Testing;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Xml
{
	public sealed class ZXmlSerializer
	{
		#region Factory Methods

		ZXmlSerializer(Type type)
		{
			this.type = type;
		}

		public static ZXmlSerializer New(Type type)
		{
			return new ZXmlSerializer(type);
		}

		#endregion

		#region Serialize

		public void Serialize(Stream stream, object o)
		{
			XmlSerializerInternal.Serialize(stream, o);
		}

		public void Serialize(TextWriter textWriter, object o)
		{
			XmlSerializerInternal.Serialize(textWriter, o);
		}

		public void Serialize(XmlWriter xmlWriter, object o)
		{
			XmlSerializerInternal.Serialize(xmlWriter, o);
		}

		public void Serialize(Stream stream, object o, XmlSerializerNamespaces namespaces)
		{
			XmlSerializerInternal.Serialize(stream, o, namespaces);
		}

		public void Serialize(TextWriter textWriter, object o, XmlSerializerNamespaces namespaces)
		{
			XmlSerializerInternal.Serialize(textWriter, o, namespaces);
		}

		public void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces)
		{
			XmlSerializerInternal.Serialize(xmlWriter, o, namespaces);
		}

		public void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle)
		{
			XmlSerializerInternal.Serialize(xmlWriter, o, namespaces, encodingStyle);
		}

		public void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle, string id)
		{
			XmlSerializerInternal.Serialize(xmlWriter, o, namespaces, encodingStyle, id);
		}

		#endregion

		#region CanDeserialize

		public bool CanDeserialize(XmlReader xmlReader)
		{
			return XmlSerializerInternal.CanDeserialize(xmlReader);
		}

		#endregion

		#region Deserialize

		public object Deserialize(Stream stream)
		{
			return XmlSerializerInternal.Deserialize(stream);
		}

		public object Deserialize(TextReader textReader)
		{
			return XmlSerializerInternal.Deserialize(textReader);
		}

		public object Deserialize(XmlReader xmlReader)
		{
			return XmlSerializerInternal.Deserialize(xmlReader);
		}

		public object Deserialize(XmlReader xmlReader, string encodingStyle)
		{
			return XmlSerializerInternal.Deserialize(xmlReader, encodingStyle);
		}

		public object Deserialize(XmlReader xmlReader, XmlDeserializationEvents events)
		{
			return XmlSerializerInternal.Deserialize(xmlReader, events);
		}

		public object Deserialize(XmlReader xmlReader, string encodingStyle, XmlDeserializationEvents events)
		{
			return XmlSerializerInternal.Deserialize(xmlReader, encodingStyle, events);
		}

		#endregion

		#region XmlSerializerInternal

		internal XmlSerializer XmlSerializerInternal
		{
			get { return xmlSerializerInternal ?? (xmlSerializerInternal = GetXmlSerializer()); }
		}
		XmlSerializer xmlSerializerInternal;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string, The only place XmlSerializer should be created")]
		XmlSerializer GetXmlSerializer()
		{
			XmlSerializer result;

			if (!Cache.TryGetValue(type, out result))
			{
				try
				{
					// This loop is required because a .NET bug causes an ExternalException 'Cannot execute a program'
					// intermittently when a domain controller goes down. It is a rare bug, and one that almost always recovers.
					var retryHandler = new RetryHandler("1s;2s");
					result = retryHandler.Invoke(() =>
					{
#if DEBUG
						ThrowIfRequiredForTest();
#endif
						return new XmlSerializer(type, string.Empty);
					});
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(ex);
					throw;
				}
				CheckIsSgenAssembly(result);
				Cache[type] = result;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query on type name")]
		void CheckIsSgenAssembly(XmlSerializer serializer)
		{
			var assemblyFieldName = "assembly";
			var tempAssembly = serializer.GetType().GetField("tempAssembly", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(serializer);
			if (tempAssembly == null)
			{
				tempAssembly = serializer.GetType().GetField("_tempAssembly", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(serializer);
				assemblyFieldName = "_assembly";
			}
			if (tempAssembly != null)
			{
				var assembly = (Assembly)tempAssembly.GetType().GetField(assemblyFieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempAssembly);
				if (assembly != null && !assembly.GetName().Name.EndsWith("XmlSerializers")
					&& !(Globals.IsTest && type.Name.StartsWith("Dummy")))
				{
					ErrorReporter.ReportOnce(
						"NoSgen." + type.FullName,
						string.Format("A dynamically compiled assembly is being used to Create the XmlSerializer for type '{0}' in assembly '{1}'\r\n" +
						"To fix, apply the [XmlSerializerAssembly] attribute to type '{0}'. and run SGen over assembly '{1}'", type.FullName, type.Assembly.GetName().Name));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message Text")]
		void HandleException(Exception ex)
		{
			if (ex.Message.StartsWith("Unable to generate a temporary class"))
			{
				throw new Exception(string.Format("Could not create a dynamically compiled XmlSerializer for type '{0}' in assembly '{1}'\r\n" +
					"To fix, apply the [XmlSerializerAssembly] attribute to type '{0}'. and run SGen over assembly '{1}'", type.FullName, type.Assembly.GetName().Name), ex);
			}

			ExternalException externalEx = ex as ExternalException;
			if ((externalEx != null) && (externalEx.ErrorCode == 5) && externalEx.Message.StartsWith("Cannot execute a program"))
			{
				throw new ZXmlSerializerCompilationException(
Enterprise.Core.Constants.ProductName + @" could not create an Xml serializer. Please ask your system administrator to try the following in order to resolve this issue:

- Turn off the virus scanner.
- Check that you have access to the system and user temporary directories.
- Check that the domain controller (if available) and network authentication is functioning properly.");
			}
		}

#if DEBUG
		static void ThrowIfRequiredForTest()
		{
			if (Globals.IsTest && (exceptionsToThrow != null) && (exceptionsToThrow.Count != 0))
			{
				Exception exception = exceptionsToThrow.Dequeue();
				throw exception;
			}
		}
#endif

		static internal Dictionary<Type, XmlSerializer> Cache
		{
			get { return cache ?? (cache = new Dictionary<Type, XmlSerializer>()); }
		}
		[ThreadStatic]
		static Dictionary<Type, XmlSerializer> cache;

		#endregion

		#region Implementation

		readonly Type type;

		#endregion

		#region Test
#if DEBUG

		[XmlRoot("TypeSpecifiedRoot")]
		public class TestTypeWithXmlRoot { }

		[SuppressThreadStaticFieldMessage]
		internal static Queue<Exception> exceptionsToThrow;

#endif
		#endregion
	}
}
