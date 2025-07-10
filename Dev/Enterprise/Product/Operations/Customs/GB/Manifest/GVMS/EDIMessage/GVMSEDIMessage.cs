using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GVMSEDIMessage : GbEDIMessage
	{
		public GVMSEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCustomsGVMSManifest);
		}

		public new CDSInterchange Interchange => Factory.Load<CDSInterchange>(EM_EI);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsGVMSManifest;
			EM_MessageType = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
		}

		public GVMSMessageDataObject MessageDataObject => messageDataObject ?? (messageDataObject = GetMessageDataObject(EM_MessageText));
		GVMSMessageDataObject messageDataObject;

		static GVMSMessageDataObject GetMessageDataObject(ZString messageText)
		{
			GVMSMessageDataObject obj = null;

			try
			{
				obj = JsonSerializer.Deserialize<GVMSMessageDataObject>(messageText);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Nothing to do here, this may occur if we receive a text error instead of json
			}
			return obj;
		}

		static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
			WriteIndented = false
		};

		public static ZString Serialize<T>(T messageDataObject, bool indentedNicely = false)
			where T : GVMSMessageDataObject
		{
			using var stream = new MemoryStream();
			using var writer = new Utf8JsonWriter(stream, new() { Indented = indentedNicely });
			JsonSerializer.Serialize(writer, messageDataObject, jsonOptions);
			return Encoding.UTF8.GetString(stream.ToArray());
		}
	}
}
