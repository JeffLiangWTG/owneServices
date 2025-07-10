using System;
using System.IO;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	public static class UniversalDataBussExtensionsTestHelper
	{
		public static T AssertBase64UniversalXMLIsParsableWithoutErrors<T>(ZString base64EncodedXml, string propertyName = "") where T : IDataObject, new()
		{
			Assertion.AssertNotNullOrEmpty(propertyName, base64EncodedXml);

			var actualXmlBytes = Convert.FromBase64String(base64EncodedXml);
			using (var stream = (SubStreamableStream)new MemoryStream(actualXmlBytes))
			{
				var dataObject = new T();
				var logger = new TestErrorLogger();
				ObjectFactory.Get<IXmlReader>().ReadXML(dataObject, stream, logger);
				Assertion.Assert($"Universal XML of type '{typeof(T).FullName}' should be parsed without error. Errors: " + logger.Logs, !logger.HasErrors);
				return dataObject;
			}
		}
	}
}