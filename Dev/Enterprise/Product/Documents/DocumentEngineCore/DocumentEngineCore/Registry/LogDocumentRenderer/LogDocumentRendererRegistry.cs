using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public sealed class LogDocumentRendererRegistry : RegistryBusinessObjectTemplate
	{
		ZString logFilePath;
		ZBool generateCallStacks;

		[CargoWise.ComponentModel.MaxLength(256)]
		public ZString LogFilePath
		{
			get { return logFilePath; }
			set { SetNonPersistentPropertyValue(LogFilePathInfo, ref logFilePath, value); }
		}

		public ZPropertyInfo LogFilePathInfo
		{
			get { return GetZPropertyInfo(nameof(LogFilePath)); }
		}

		public ZBool GenerateCallStacks
		{
			get { return generateCallStacks; }
			set { SetNonPersistentPropertyValue(GenerateCallStacksInfo, ref generateCallStacks, value); }
		}

		public ZPropertyInfo GenerateCallStacksInfo
		{
			get { return GetZPropertyInfo(nameof(GenerateCallStacks)); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			using (var stream = new MemoryStream())
			{
				var serializer = ZXmlSerializer.New(GetType());
				serializer.Serialize(stream, this);

				stream.Position = 0;
				return (LogDocumentRendererRegistry)serializer.Deserialize(stream);
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString("LogFilePath", LogFilePath);
			writer.WriteElementString("GenerateCallStacks", GenerateCallStacks.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper readerWrapper)
		{
			var reader = readerWrapper.Reader;

			LogFilePath = reader.ReadElementString("LogFilePath");
			GenerateCallStacks = new ZBool(reader.ReadElementString("GenerateCallStacks"));
		}
	}
}
