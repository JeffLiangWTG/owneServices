using System;
using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting;

[TestedType(typeof(UpdateRegistryImageRegistryItemClass))]
class UpdateRegistryImageRegistryItemClassTest : RegistryDataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new UpdateRegistryImageRegistryItemClass();
	}

	protected override void PrepareTestData()
	{
		Helper.InsertStmDataRow(SdName, "BIN", Encoding.Unicode.GetBytes(OriginalXml1));
		Helper.InsertStmDataRow(SdName, company1, "BIN", Encoding.Unicode.GetBytes(OriginalXml2));
		Helper.InsertStmDataRow(SdName, company2, "BIN", Encoding.Unicode.GetBytes(OriginalXml3));
		Helper.InsertStmDataRow(SdName, company3, "BIN", null);
	}

	protected override void AssertTransformationResults()
	{
		var binaryValue1 = Helper.GetStmDataValue(SdName);
		AssertNotNull("Should have a not NULL binary value.", binaryValue1);
		AssertEquals("Registry item without ArrayOfRegistryImage should not be modified.", OriginalXml1, Encoding.Unicode.GetString(binaryValue1));

		var binaryValue2 = Helper.GetStmDataValue(SdName, company1);
		AssertNotNull("Should have a not NULL binary value.", binaryValue2);
		AssertEqualsIgnoreLineBreaks("Registry item with ArrayOfRegistryImage should be updated correctly.", ExpectedXml2, Encoding.Unicode.GetString(binaryValue2));

		var binaryValue3 = Helper.GetStmDataValue(SdName, company2);
		AssertNotNull("Should have a not NULL binary value.", binaryValue3);
		AssertEqualsIgnoreLineBreaks("Registry item with ArrayOfSystemDefinableRegistryImage should not be modified.", ExpectedXml3, Encoding.Unicode.GetString(binaryValue3));

		var binaryValue4 = Helper.GetStmDataValue(SdName, company3);
		AssertNull("Should have a NULL binary value.", binaryValue4);
	}

	const string SdName = "DocumentImages";
	readonly Guid company1 = Guid.NewGuid();
	readonly Guid company2 = Guid.NewGuid();
	readonly Guid company3 = Guid.NewGuid();

	const string OriginalXml1 = "invalid xml";

	const string OriginalXml2 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfRegistryImage xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><RegistryImage><CodeMaxLength>3</CodeMaxLength><Code>BHJ</Code><Description>789</Description><ImagePK>5e48bf5f-b01f-48c3-95cc-919889293924</ImagePK><FallbackKey>AAAAAAAAAAAAAAAAAAAAAA==AAAAAAAAAAAAAAAAAAAAAA==</FallbackKey></RegistryImage><RegistryImage><CodeMaxLength>3</CodeMaxLength><Code>ACV</Code><Description>acv description</Description><ImagePK>fd0a1465-7b53-46f1-8157-e88a17873999</ImagePK><FallbackKey>AAAAAAAAAAAAAAAAAAAAAA==AAAAAAAAAAAAAAAAAAAAAA==</FallbackKey></RegistryImage></ArrayOfRegistryImage>";
	const string ExpectedXml2 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfSystemDefinableRegistryImage xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><SystemDefinableRegistryImage><CodeMaxLength>3</CodeMaxLength><Code>BHJ</Code><Description>789</Description><ImagePK>5e48bf5f-b01f-48c3-95cc-919889293924</ImagePK><FallbackKey>AAAAAAAAAAAAAAAAAAAAAA==AAAAAAAAAAAAAAAAAAAAAA==</FallbackKey></SystemDefinableRegistryImage><SystemDefinableRegistryImage><CodeMaxLength>3</CodeMaxLength><Code>ACV</Code><Description>acv description</Description><ImagePK>fd0a1465-7b53-46f1-8157-e88a17873999</ImagePK><FallbackKey>AAAAAAAAAAAAAAAAAAAAAA==AAAAAAAAAAAAAAAAAAAAAA==</FallbackKey></SystemDefinableRegistryImage></ArrayOfSystemDefinableRegistryImage>";

	const string OriginalXml3 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfSystemDefinableRegistryImage xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><SystemDefinableRegistryImage><CodeMaxLength>3</CodeMaxLength><Code>BHJ</Code><Description>789</Description><ImagePK>5e48bf5f-b01f-48c3-95cc-919889293924</ImagePK><FallbackKey>AAAAAAAAAAAAAAAAAAAAAA==AAAAAAAAAAAAAAAAAAAAAA==</FallbackKey><SystemDefined>N</SystemDefined></SystemDefinableRegistryImage><SystemDefinableRegistryImage><CodeMaxLength>3</CodeMaxLength><Code>NEW</Code><Description>new image</Description><ImagePK>e80614f8-f434-4afd-964e-b0d973dfef20</ImagePK><FallbackKey>AAAAAAAAAAAAAAAAAAAAAA==AAAAAAAAAAAAAAAAAAAAAA==</FallbackKey><SystemDefined>N</SystemDefined></SystemDefinableRegistryImage></ArrayOfSystemDefinableRegistryImage>";
	const string ExpectedXml3 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfSystemDefinableRegistryImage xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><SystemDefinableRegistryImage><CodeMaxLength>3</CodeMaxLength><Code>BHJ</Code><Description>789</Description><ImagePK>5e48bf5f-b01f-48c3-95cc-919889293924</ImagePK><FallbackKey>AAAAAAAAAAAAAAAAAAAAAA==AAAAAAAAAAAAAAAAAAAAAA==</FallbackKey><SystemDefined>N</SystemDefined></SystemDefinableRegistryImage><SystemDefinableRegistryImage><CodeMaxLength>3</CodeMaxLength><Code>NEW</Code><Description>new image</Description><ImagePK>e80614f8-f434-4afd-964e-b0d973dfef20</ImagePK><FallbackKey>AAAAAAAAAAAAAAAAAAAAAA==AAAAAAAAAAAAAAAAAAAAAA==</FallbackKey><SystemDefined>N</SystemDefined></SystemDefinableRegistryImage></ArrayOfSystemDefinableRegistryImage>";
}
