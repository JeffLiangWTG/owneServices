using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgCodeAlgorithmRegistryDataType))]
	sealed class OrgCodeAlgorithmRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OrgCodeAlgorithmRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "OrgCodeAlgorithmConfigRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			OrgCodeAlgorithmTest.AssertAlgorithmEquals((OrgCodeAlgorithm)lhs, (OrgCodeAlgorithm)rhs, false);
		}

		protected override OrgCodeAlgorithmRegistryDataType GetNewDataType()
		{
			return new OrgCodeAlgorithmRegistryDataType(OrgCodeAlgorithmType.Override);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			OrgCodeAlgorithm algorithm = OrgCodeAlgorithmTest.GetTestAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Override;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(algorithm, Encoding.Unicode.GetBytes(
					@"<?xml version=""1.0"" encoding=""utf-16""?>
					<OrgCodeAlgorithm>
						<RegenerateOrgCodeOnChanges>Y</RegenerateOrgCodeOnChanges>
						<AllowRecalculatedOrgCodeByUser>Y</AllowRecalculatedOrgCodeByUser>
						<ArrayOfOrgCodeElement>
							<OrgCodeElement>
								<Description>First Name</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Second Name</Description>
								<Length>5</Length>
								<Order>3</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Last Name</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Country Code</Description>
								<Length>2</Length>
								<Order>6</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>UnlocoCode Code</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>IataCode Code</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Globally Unique Number</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Code Specific Unique Number</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
						</ArrayOfOrgCodeElement>
						<ArrayOfOrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Receivables</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Payables</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Consignor</Description>
								<Selected>Y</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Consignee</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Carrier</Description>
								<Selected>Y</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Forwarder</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Broker</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Services</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Competitor</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Sales</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
						</ArrayOfOrgCodeOrgType>
					</OrgCodeAlgorithm>"))
			};
		}

		public void TestDeserializeSetsAlgorithmType()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			byte[] bytes = DataType.Serialise(algorithm);
			AssertEquals("Deserialise().AlgorithmType", OrgCodeAlgorithmType.Override, DataType.Deserialise(bytes).AlgorithmType);

			OrgCodeAlgorithmRegistryDataType dataType = new OrgCodeAlgorithmRegistryDataType(OrgCodeAlgorithmType.Default);
			AssertEquals("Deserialise().AlgorithmType", OrgCodeAlgorithmType.Default, dataType.Deserialise(bytes).AlgorithmType);
		}

		public void TestInvalidAlgorithmType()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 1;
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			DataType.Validate(null, algorithm, Guid.Empty, Guid.Empty, Guid.Empty);

			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			try
			{
				DataType.Validate(null, algorithm, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("An exception should have been thrown.");
			}
			catch (RegistryValidationException ex)
			{
				AssertEquals("Exception Message", "The algorithm type specified ('Default') is incorrect. It should be 'Override'.", ex.Message);
			}
		}
	}
}
