using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal
{
	public static class UniversalDataBussExtensions
	{
		public static string ToXmlFragment(this IDataObject universalXmlObject)
		{
			Argument.NotNull(universalXmlObject, nameof(universalXmlObject));

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(universalXmlObject, stream);
				stream.Seek(0L, SeekOrigin.Begin);

				var xDoc = XDocument.Load(stream);
				xDoc.Declaration = null;
				return xDoc.ToString();
			}
		}

		public static string ToBase64EncodedXmlFragment(this IDataObject universalXmlObject)
		{
			return Convert.ToBase64String(
				MessageEncoding.UTF8WithoutBOM.GetBytes(universalXmlObject.ToXmlFragment()));
		}
	}
}
