using System;
using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(UpdateEnableBoleroEBLAndEHBLIntegrationRegistryDataTransformation))]
	public class UpdateEnableBoleroEBLAndEHBLIntegrationRegistryDataTransformationTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var binaryValue1 = Helper.GetStmDataValue("EnableBoleroEBLIntegration", Guid.Parse("5a1a4969-44df-4bc1-8800-08ce58380560"));
			AssertNotNull(binaryValue1);
			AssertEquals(
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration>
    <EnableEBLIntegration>Y</EnableEBLIntegration>
    <GalileoEndPointUrl>https://wisetechglobal.sharepoint.com/</GalileoEndPointUrl>
    <GalileoAudience>84838b53-79e0-42c4-9f19-1b0dae0cf284</GalileoAudience>
    <GalileoTestEndPointUrl>https://wisetechglobal.sharepoint.testing.com/</GalileoTestEndPointUrl>
    <GalileoTestAudience>fb2b6c48-94f7-48c6-b488-5664ba01ea99</GalileoTestAudience>
</BoleroEBLConfiguration>
"
				, Encoding.Unicode.GetString(binaryValue1));

			var binaryValue2 = Helper.GetStmDataValue("EnableBoleroEBLIntegration", Guid.Parse("f3023fce-8d74-41d8-823f-c7a3fa50c0d2"));
			AssertNotNull(binaryValue2);
			AssertEquals(
				"<?xml version=\"1.0\" encoding=\"utf-16\"?><BoleroEBLConfiguration><EnableEBLIntegration>N</EnableEBLIntegration><GalileoEndPointUrl>https://galileo.boleroserve.net/galileo-portal/jwt/login</GalileoEndPointUrl><GalileoAudience>ab2e99f4-15c9-41c8-a138-7d873c3c4f9f</GalileoAudience><GalileoTestEndPointUrl>https://galileo.training.boleroserve.net/galileo-portal/jwt/login</GalileoTestEndPointUrl><GalileoTestAudience>ef46c619-92d6-4f56-af3d-327cf3646508</GalileoTestAudience></BoleroEBLConfiguration>"
				, Encoding.Unicode.GetString(binaryValue2));

			var binaryValue3 = Helper.GetStmDataValue("EnableBoleroEBLIntegration", Guid.Parse("c2836290-21da-4a66-9241-de9a111c5125"));
			AssertNotNull(binaryValue3);
			AssertEquals(
				"<?xml version=\"1.0\" encoding=\"utf-16\"?><BoleroEBLConfiguration><EnableEBLIntegration>Y</EnableEBLIntegration><GalileoEndPointUrl>https://wisetechglobal.sharepoint.com/</GalileoEndPointUrl><GalileoAudience>ab2e99f4-15c9-41c8-a138-7d873c3c4f9f</GalileoAudience><GalileoTestEndPointUrl>https://galileo.training.boleroserve.net/galileo-portal/jwt/login</GalileoTestEndPointUrl><GalileoTestAudience>ef46c619-92d6-4f56-af3d-327cf3646508</GalileoTestAudience></BoleroEBLConfiguration>"
				, Encoding.Unicode.GetString(binaryValue3));

			var binaryValue4 = Helper.GetStmDataValue("EnableBoleroEBLIntegration", Guid.Parse("a286ef02-d86e-477f-b0ce-a79aae5c050b"));
			AssertNotNull(binaryValue4);
			AssertEquals("True", Encoding.Unicode.GetString(binaryValue4));

			var binaryValue5 = Helper.GetStmDataValue("EnableBoleroEBLIntegration", Guid.Parse("ae640d85-63d0-4ece-8623-88ba46cbc955"));
			AssertNotNull(binaryValue5);
			AssertEquals(string.Empty, Encoding.Unicode.GetString(binaryValue5));

			var binaryValue6 = Helper.GetStmDataValue("EnableBoleroEBLIntegration", Guid.Parse("a809a85d-334a-492b-92e8-6e56369cef2d"));
			AssertNotNull(binaryValue6);
			AssertEquals(
				"<?xml version=\"1.0\" encoding=\"utf-16\"?><BoleroEBLConfiguration><EnableEBLIntegration>N</EnableEBLIntegration><GalileoEndPointUrl>https://galileo.boleroserve.net/galileo-portal/jwt/login</GalileoEndPointUrl><GalileoAudience>ab2e99f4-15c9-41c8-a138-7d873c3c4f9f</GalileoAudience><GalileoTestEndPointUrl>https://galileo.training.boleroserve.net/galileo-portal/jwt/login</GalileoTestEndPointUrl><GalileoTestAudience>ef46c619-92d6-4f56-af3d-327cf3646508</GalileoTestAudience></BoleroEBLConfiguration>"
				, Encoding.Unicode.GetString(binaryValue6));

			var binaryValue7 = Helper.GetStmDataValue("EnableBoleroEHBLIntegration", Guid.Parse("5a1a4969-44df-4bc1-8800-08ce58380560"));
			AssertNotNull(binaryValue7);
			AssertEquals(
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration>
    <EnableEBLIntegration>N</EnableEBLIntegration>
    <GalileoEndPointUrl>https://wisetechglobal.com/</GalileoEndPointUrl>
    <GalileoAudience>03f9bb33-7f3d-4175-aff2-de3000dc9373</GalileoAudience>
    <GalileoTestEndPointUrl>https://wisetechglobal.testing.com/</GalileoTestEndPointUrl>
    <GalileoTestAudience>3aab2156-7e86-4ed7-94ae-4efa94eca9ad</GalileoTestAudience>
</BoleroEBLConfiguration>
"
				, Encoding.Unicode.GetString(binaryValue7));

			var binaryValue8 = Helper.GetStmDataValue("EnableBoleroEHBLIntegration", Guid.Parse("f3023fce-8d74-41d8-823f-c7a3fa50c0d2"));
			AssertNotNull(binaryValue8);
			AssertEquals(
				"<?xml version=\"1.0\" encoding=\"utf-16\"?><BoleroEBLConfiguration><EnableEBLIntegration>N</EnableEBLIntegration><GalileoEndPointUrl>https://galileo.boleroserve.net/galileo-portal/jwt/login</GalileoEndPointUrl><GalileoAudience>ab2e99f4-15c9-41c8-a138-7d873c3c4f9f</GalileoAudience><GalileoTestEndPointUrl>https://galileo.training.boleroserve.net/galileo-portal/jwt/login</GalileoTestEndPointUrl><GalileoTestAudience>ef46c619-92d6-4f56-af3d-327cf3646508</GalileoTestAudience></BoleroEBLConfiguration>"
				, Encoding.Unicode.GetString(binaryValue8));

			var binaryValue9 = Helper.GetStmDataValue("EnableBoleroEHBLIntegration", Guid.Parse("c2836290-21da-4a66-9241-de9a111c5125"));
			AssertNotNull(binaryValue9);
			AssertEquals(
				"<?xml version=\"1.0\" encoding=\"utf-16\"?><BoleroEBLConfiguration><GalileoTestEndPointUrl>https://wisetechglobal.sharepoint.testing.com/</GalileoTestEndPointUrl><GalileoTestAudience>fb2b6c48-94f7-48c6-b488-5664ba01ea99</GalileoTestAudience><EnableEBLIntegration>N</EnableEBLIntegration><GalileoEndPointUrl>https://galileo.boleroserve.net/galileo-portal/jwt/login</GalileoEndPointUrl><GalileoAudience>ab2e99f4-15c9-41c8-a138-7d873c3c4f9f</GalileoAudience></BoleroEBLConfiguration>"
				, Encoding.Unicode.GetString(binaryValue9));

			var binaryValue10 = Helper.GetStmDataValue("EnableBoleroEHBLIntegration", Guid.Parse("a286ef02-d86e-477f-b0ce-a79aae5c050b"));
			AssertNotNull(binaryValue10);
			AssertEquals("True", Encoding.Unicode.GetString(binaryValue10));

			var binaryValue11 = Helper.GetStmDataValue("EnableBoleroEHBLIntegration", Guid.Parse("ae640d85-63d0-4ece-8623-88ba46cbc955"));
			AssertNotNull(binaryValue11);
			AssertEquals(string.Empty, Encoding.Unicode.GetString(binaryValue11));

			var binaryValue12 = Helper.GetStmDataValue("EnableBoleroEHBLIntegration", Guid.Parse("a809a85d-334a-492b-92e8-6e56369cef2d"));
			AssertNotNull(binaryValue12);
			AssertEquals(
				"<?xml version=\"1.0\" encoding=\"utf-16\"?><BoleroEBLConfiguration><EnableEBLIntegration>N</EnableEBLIntegration><GalileoEndPointUrl>https://galileo.boleroserve.net/galileo-portal/jwt/login</GalileoEndPointUrl><GalileoAudience>ab2e99f4-15c9-41c8-a138-7d873c3c4f9f</GalileoAudience><GalileoTestEndPointUrl>https://galileo.training.boleroserve.net/galileo-portal/jwt/login</GalileoTestEndPointUrl><GalileoTestAudience>ef46c619-92d6-4f56-af3d-327cf3646508</GalileoTestAudience></BoleroEBLConfiguration>"
				, Encoding.Unicode.GetString(binaryValue12));

			var binaryValue13 = Helper.GetStmDataValue("EnableBoleroEHBLIntegration", Guid.Parse("3abcc7a8-136b-4178-b6a4-e715c1466c4e"));
			AssertNull(binaryValue13);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateEnableBoleroEBLAndEHBLIntegrationRegistryDataTransformation();
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("EnableBoleroEBLIntegration", Guid.Parse("5a1a4969-44df-4bc1-8800-08ce58380560"), "BIN", Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration>
    <EnableEBLIntegration>Y</EnableEBLIntegration>
    <GalileoEndPointUrl>https://wisetechglobal.sharepoint.com/</GalileoEndPointUrl>
    <GalileoAudience>84838b53-79e0-42c4-9f19-1b0dae0cf284</GalileoAudience>
    <GalileoTestEndPointUrl>https://wisetechglobal.sharepoint.testing.com/</GalileoTestEndPointUrl>
    <GalileoTestAudience>fb2b6c48-94f7-48c6-b488-5664ba01ea99</GalileoTestAudience>
</BoleroEBLConfiguration>
"));
			Helper.InsertStmDataRow("EnableBoleroEBLIntegration", Guid.Parse("f3023fce-8d74-41d8-823f-c7a3fa50c0d2"), "BIN", Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration>
    <EnableEBLIntegration></EnableEBLIntegration>
    <GalileoEndPointUrl></GalileoEndPointUrl>
    <GalileoAudience></GalileoAudience>
    <GalileoTestEndPointUrl></GalileoTestEndPointUrl>
    <GalileoTestAudience></GalileoTestAudience>
</BoleroEBLConfiguration>
"));
			Helper.InsertStmDataRow("EnableBoleroEBLIntegration", Guid.Parse("c2836290-21da-4a66-9241-de9a111c5125"), "BIN", Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration>
    <EnableEBLIntegration>Y</EnableEBLIntegration>
    <GalileoEndPointUrl>https://wisetechglobal.sharepoint.com/</GalileoEndPointUrl>
</BoleroEBLConfiguration>
"));
			Helper.InsertStmDataRow("EnableBoleroEBLIntegration", Guid.Parse("a286ef02-d86e-477f-b0ce-a79aae5c050b"), "BOL", Encoding.Unicode.GetBytes("True"));
			Helper.InsertStmDataRow("EnableBoleroEBLIntegration", Guid.Parse("ae640d85-63d0-4ece-8623-88ba46cbc955"), "BIN", Encoding.Unicode.GetBytes(string.Empty));
			Helper.InsertStmDataRow("EnableBoleroEBLIntegration", Guid.Parse("a809a85d-334a-492b-92e8-6e56369cef2d"), "BIN", Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration1>
</BoleroEBLConfiguration1>
"));

			Helper.InsertStmDataRow("EnableBoleroEHBLIntegration", Guid.Parse("5a1a4969-44df-4bc1-8800-08ce58380560"), "BIN", Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration>
    <EnableEBLIntegration>N</EnableEBLIntegration>
    <GalileoEndPointUrl>https://wisetechglobal.com/</GalileoEndPointUrl>
    <GalileoAudience>03f9bb33-7f3d-4175-aff2-de3000dc9373</GalileoAudience>
    <GalileoTestEndPointUrl>https://wisetechglobal.testing.com/</GalileoTestEndPointUrl>
    <GalileoTestAudience>3aab2156-7e86-4ed7-94ae-4efa94eca9ad</GalileoTestAudience>
</BoleroEBLConfiguration>
"));
			Helper.InsertStmDataRow("EnableBoleroEHBLIntegration", Guid.Parse("f3023fce-8d74-41d8-823f-c7a3fa50c0d2"), "BIN", Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration>
    <EnableEBLIntegration/>
    <GalileoEndPointUrl/>
    <GalileoAudience/>
    <GalileoTestEndPointUrl/>
    <GalileoTestAudience/>
</BoleroEBLConfiguration>
"));
			Helper.InsertStmDataRow("EnableBoleroEHBLIntegration", Guid.Parse("c2836290-21da-4a66-9241-de9a111c5125"), "BIN", Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration>
    <GalileoTestEndPointUrl>https://wisetechglobal.sharepoint.testing.com/</GalileoTestEndPointUrl>
    <GalileoTestAudience>fb2b6c48-94f7-48c6-b488-5664ba01ea99</GalileoTestAudience>
</BoleroEBLConfiguration>
"));
			Helper.InsertStmDataRow("EnableBoleroEHBLIntegration", Guid.Parse("a286ef02-d86e-477f-b0ce-a79aae5c050b"), "BOL", Encoding.Unicode.GetBytes("True"));
			Helper.InsertStmDataRow("EnableBoleroEHBLIntegration", Guid.Parse("ae640d85-63d0-4ece-8623-88ba46cbc955"), "BIN", Encoding.Unicode.GetBytes(string.Empty));
			Helper.InsertStmDataRow("EnableBoleroEHBLIntegration", Guid.Parse("a809a85d-334a-492b-92e8-6e56369cef2d"), "BIN", Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<BoleroEBLConfiguration>
    <EnableEBLIntegration></EnableEBLIntegration>
    <GalileoEndPointUrl/>
</BoleroEBLConfiguration>
"));
			Helper.InsertStmDataRow("EnableBoleroEHBLIntegration", Guid.Parse("3abcc7a8-136b-4178-b6a4-e715c1466c4e"), "BIN", null);
		}
	}
}
