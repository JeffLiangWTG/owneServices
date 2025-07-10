using System.IO;
using System.Text;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	class RequestDeserializerBuilderTest : TestCase
	{
		public void TestNamespaceIsHandledCorrectly()
		{
			var xmlForRequestDeserializer_Versioned_Native = @"
<ns0:Native version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<ns0:Header></ns0:Header>
	<ns0:Body></ns0:Body>
</ns0:Native>";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlForRequestDeserializer_Versioned_Native)))
			{
				AssertType<RequestDeserializer_Versioned_Native>(RequestDeserializerBuilder.GetDeserializer(stream));
			}

			var xmlForRequestDeserializer_Unversioned_Native = @"
<ns0:Native version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native"">
	<ns0:Header></ns0:Header>
	<ns0:Body></ns0:Body>
</ns0:Native>";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlForRequestDeserializer_Unversioned_Native)))
			{
				AssertType<RequestDeserializer_Unversioned_Native>(RequestDeserializerBuilder.GetDeserializer(stream));
			}

			var xmlForRequestDeserializer_Universal = @"
<ns0:ReferenceData version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Universal"">
	<ns0:Header></ns0:Header>
	<ns0:Body></ns0:Body>
</ns0:ReferenceData>";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlForRequestDeserializer_Universal)))
			{
				AssertType<RequestDeserializer_Universal>(RequestDeserializerBuilder.GetDeserializer(stream));
			}
		}

		public void TestEmptyNamespaceIsHandledCorrectly()
		{
			var xmlForRequestDeserializer_Versioned_Native = @"
<Native version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header></Header>
	<Body></Body>
</Native>";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlForRequestDeserializer_Versioned_Native)))
			{
				AssertType<RequestDeserializer_Versioned_Native>(RequestDeserializerBuilder.GetDeserializer(stream));
			}

			var xmlForRequestDeserializer_Unversioned_Native = @"
<Native version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Native"">
	<Header></Header>
	<Body></Body>
</Native>";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlForRequestDeserializer_Unversioned_Native)))
			{
				AssertType<RequestDeserializer_Unversioned_Native>(RequestDeserializerBuilder.GetDeserializer(stream));
			}

			var xmlForRequestDeserializer_Universal = @"
<ReferenceData version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
	<Header></Header>
	<Body></Body>
</ReferenceData>";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlForRequestDeserializer_Universal)))
			{
				AssertType<RequestDeserializer_Universal>(RequestDeserializerBuilder.GetDeserializer(stream));
			}
		}
	}
}