using System;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AvsWebServiceUriCollectionRegistryDataType))]
	public class AvsWebServiceUriCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AvsWebServiceUriCollectionRegistryDataType>
	{
		public void TestValidation()
		{
			var dataType = new AvsWebServiceUriCollectionRegistryDataType();

			AssertExceptionThrown<RegistryValidationException>("2 URIs", "The number of URIs should be fixed at 3, which are primary, secondary and background URIs.", () =>
			{
				var collection = new AvsWebServiceUriRegistryBusinessObjectCollection()
				{
					{ AvsWebServiceUriType.Primary, (NoResString)"https://primary.url", true },
					{ AvsWebServiceUriType.Secondary, (NoResString)"", false },
				};

				dataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
			});

			AssertExceptionThrown<RegistryValidationException>("Type Duplicated", "The number of URIs should be fixed at 3, which are primary, secondary and background URIs.", () =>
			{
				var collection = new AvsWebServiceUriRegistryBusinessObjectCollection()
				{
					{ AvsWebServiceUriType.Primary, (NoResString)"https://primary1.url", true },
					{ AvsWebServiceUriType.Primary, (NoResString)"https://primary2.url", true },
					{ AvsWebServiceUriType.Background, (NoResString)"http://background.url", false }
				};

				dataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
			});

			AssertExceptionThrown<RegistryValidationException>("Invalid URIs", "One or more URLs are invalid, please input valid URLs that must be HTTPS or HTTP.", () =>
			{
				var collection = new AvsWebServiceUriRegistryBusinessObjectCollection()
				{
					{ AvsWebServiceUriType.Primary, (NoResString)"https://primary.url", true },
					{ AvsWebServiceUriType.Secondary, (NoResString)"https://secondary.url", false },
					{ AvsWebServiceUriType.Background, (NoResString)"invalid", false }
				};

				dataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
			});

			AssertExceptionThrown<RegistryValidationException>("Invalid URIs", "One or more URLs are invalid, please input valid URLs that must be HTTPS or HTTP.", () =>
			{
				var collection = new AvsWebServiceUriRegistryBusinessObjectCollection()
				{
					{ AvsWebServiceUriType.Primary, (NoResString)"https://primary.url", true },
					{ AvsWebServiceUriType.Secondary, (NoResString)"", false },
					{ AvsWebServiceUriType.Background, (NoResString)"http://background.url", false }
				};

				dataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
			});

			AssertNoExceptionThrown(() =>
			{
				var collection = new AvsWebServiceUriRegistryBusinessObjectCollection()
				{
					{ AvsWebServiceUriType.Primary, (NoResString)"https://primary.url", true },
					{ AvsWebServiceUriType.Secondary, (NoResString)"https://secondary.url", false },
					{ AvsWebServiceUriType.Background, (NoResString)"http://background.url", false }
				};

				dataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
			});
		}

		protected override bool HasEditor => false;

		protected override AvsWebServiceUriCollectionRegistryDataType GetNewDataType()
		{
			return new AvsWebServiceUriCollectionRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new AvsWebServiceUriRegistryBusinessObjectCollection()
			{
				{ AvsWebServiceUriType.Primary, (NoResString)"https://primary.url", true },
				{ AvsWebServiceUriType.Secondary, (NoResString)"https://secondary.url", false },
				{ AvsWebServiceUriType.Background, (NoResString)"http://background.url", false }
			};

			var xml1 = Regex.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfAvsWebServiceUriRegistryBusinessObject xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <AvsWebServiceUriRegistryBusinessObject>
    <CodeMaxLength>10</CodeMaxLength>
    <Code>Primary</Code>
    <Description>https://primary.url</Description>
    <Bool>Y</Bool>
    <SystemDefined>False</SystemDefined>
  </AvsWebServiceUriRegistryBusinessObject>
  <AvsWebServiceUriRegistryBusinessObject>
    <CodeMaxLength>10</CodeMaxLength>
    <Code>Secondary</Code>
    <Description>https://secondary.url</Description>
    <Bool>N</Bool>
    <SystemDefined>False</SystemDefined>
  </AvsWebServiceUriRegistryBusinessObject>
  <AvsWebServiceUriRegistryBusinessObject>
    <CodeMaxLength>10</CodeMaxLength>
    <Code>Background</Code>
    <Description>http://background.url</Description>
    <Bool>N</Bool>
    <SystemDefined>False</SystemDefined>
  </AvsWebServiceUriRegistryBusinessObject>
</ArrayOfAvsWebServiceUriRegistryBusinessObject>", @">\s*<", "><");

			var collection2 = new AvsWebServiceUriRegistryBusinessObjectCollection()
			{
				{ AvsWebServiceUriType.Primary, (NoResString)"https://primary2.url", true },
				{ AvsWebServiceUriType.Secondary, (NoResString)"https://secondary2.url", false },
				{ AvsWebServiceUriType.Background, (NoResString)"http://background2.url", false }
			};

			var xml2 = Regex.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfAvsWebServiceUriRegistryBusinessObject xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <AvsWebServiceUriRegistryBusinessObject>
    <CodeMaxLength>10</CodeMaxLength>
    <Code>Primary</Code>
    <Description>https://primary2.url</Description>
    <Bool>Y</Bool>
    <SystemDefined>False</SystemDefined>
  </AvsWebServiceUriRegistryBusinessObject>
  <AvsWebServiceUriRegistryBusinessObject>
    <CodeMaxLength>10</CodeMaxLength>
    <Code>Secondary</Code>
    <Description>https://secondary2.url</Description>
    <Bool>N</Bool>
    <SystemDefined>False</SystemDefined>
  </AvsWebServiceUriRegistryBusinessObject>
  <AvsWebServiceUriRegistryBusinessObject>
    <CodeMaxLength>10</CodeMaxLength>
    <Code>Background</Code>
    <Description>http://background2.url</Description>
    <Bool>N</Bool>
    <SystemDefined>False</SystemDefined>
  </AvsWebServiceUriRegistryBusinessObject>
</ArrayOfAvsWebServiceUriRegistryBusinessObject>", @">\s*<", "><");

			return new []
			{
				new ValidSampleAndBinaryValueInDB(collection1, Encoding.Unicode.GetBytes(xml1)),
				new ValidSampleAndBinaryValueInDB(collection2, Encoding.Unicode.GetBytes(xml2))
			};
		}
	}
}
