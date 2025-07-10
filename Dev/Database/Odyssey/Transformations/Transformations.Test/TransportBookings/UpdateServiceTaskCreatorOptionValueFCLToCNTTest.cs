using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.IO;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings.Testing
{
	[TestedType(typeof(UpdateServiceTaskCreatorOptionValueFCLToCNT))]
	public class UpdateServiceTaskCreatorOptionValueFCLToCNTTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateServiceTaskCreatorOptionValueFCLToCNT();
		}

		protected override void PrepareTestData()
		{
			var sD_Name = UpdateServiceTaskCreatorOptionValueFCLToCNT.RegistryName;
			var sD_Type = "BIN";

			var xml = GetTestXmlData();

			var sD_BinaryValue = ConvertXmlToBytes(xml);

			Helper.InsertStmDataRow(sD_Name, sD_Type, sD_BinaryValue);
		}

		internal XDocument GetTestXmlData()
		{
			var xml = new XDocument(new XElement($"ArrayOf{UpdateServiceTaskCreatorOptionValueFCLToCNT.RegistryName}"));

			xml.Root.Add(
				CreateServiceTaskCreatorOptionElement("LSE", "PTR", "N"),
				CreateServiceTaskCreatorOptionElement("FTL", "PTR", "N"),
				CreateServiceTaskCreatorOptionElement("FCL", "PTR", "N"),
				CreateServiceTaskCreatorOptionElement("MIX", "PTR", "N")
			);

			return xml;
		}

		internal byte[] ConvertXmlToBytes(XDocument xml)
		{
			using (var ms = new MemoryStream())
			{
				xml.Save(ms, SaveOptions.DisableFormatting);
				ms.Position = 0;
				return ms.ToByteArray();
			}
		}

		internal XElement CreateServiceTaskCreatorOptionElement(string containerModeValue, string targetModuleValue, string isSystemDefinedValue)
		{
			var serviceTaskCreatorOption = new XElement(UpdateServiceTaskCreatorOptionValueFCLToCNT.RegistryName);

			var containerMode = new XElement("ContainerMode") { Value = containerModeValue };
			var targetModule = new XElement("TargetModule") { Value = targetModuleValue };
			var isSystemDefined = new XElement("IsSystemDefined") { Value = isSystemDefinedValue };

			serviceTaskCreatorOption.Add(
				containerMode,
				targetModule,
				isSystemDefined
			);

			return serviceTaskCreatorOption;
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("There should only be one row", 1, Helper.GetStmDataRowCount("ServiceTaskCreatorOption"));

			var registryBinaryValue = Helper.GetStmDataValue(UpdateServiceTaskCreatorOptionValueFCLToCNT.RegistryName);
			var xml = GetDocument(registryBinaryValue);

			var originalXmlElements = GetTestXmlData().Root.Elements();

			var zippedElements = originalXmlElements.Zip(xml.Root.Elements(), (original, transformed) => new { original, transformed });

			foreach (var elementPair in zippedElements)
			{
				var elementThatShouldBeTransformed = elementPair.original.Elements().FirstOrDefault(e => e.Name == "ContainerMode" && e.Value == "FCL");

				if (elementThatShouldBeTransformed != null)
				{
					var correspondingTransformedContainerModeElementValue = GetSubElementValue(elementPair.transformed, "ContainerMode");
					var correspondingTransformedIsSystemDefinedValue = GetSubElementValue(elementPair.transformed, "IsSystemDefined");

					AssertEquals("The element should be transformed to the new value", "CNT", correspondingTransformedContainerModeElementValue);
					AssertEquals("The element should be transformed to the new value", "N", correspondingTransformedIsSystemDefinedValue);
					AssertSubElementUnchanged(elementPair.original, elementPair.transformed, "TargetModule");
				}
				else
				{
					AssertServiceTaskCreatorOptionElementsAreUnchanged(elementPair.original, elementPair.transformed);
				}
			}
		}

		void AssertServiceTaskCreatorOptionElementsAreUnchanged(XElement original, XElement transformed)
		{
			AssertSubElementUnchanged(original, transformed, "ContainerMode");
			AssertSubElementUnchanged(original, transformed, "TargetModule");
			AssertSubElementUnchanged(original, transformed, "IsSystemDefined");
		}

		void AssertSubElementUnchanged(XElement original, XElement transformed, string subElementName)
		{
			AssertEquals($"{subElementName} should be identical", GetSubElementValue(original, subElementName), GetSubElementValue(transformed, subElementName));
		}

		string GetSubElementValue(XElement parentElement, string subElementName)
		{
			return parentElement.Elements().FirstOrDefault(e => e.Name == subElementName).Value;
		}

		internal XDocument GetDocument(byte[] data)
		{
			using (var memoryStream = new MemoryStream(data))
			{
				return XDocument.Load(memoryStream);
			}
		}
	}

	[TestedType(typeof(UpdateServiceTaskCreatorOptionValueFCLToCNT))]
	public class UpdateServiceTaskCreatorOptionValueFCLToCNTTest_NullValue : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateServiceTaskCreatorOptionValueFCLToCNT();
		}

		protected override void PrepareTestData()
		{
			var sD_Name = UpdateServiceTaskCreatorOptionValueFCLToCNT.RegistryName;
			var sD_Type = "BIN";

			Helper.InsertStmDataRow(sD_Name, sD_Type, null);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(null, Helper.GetStmDataValue(UpdateServiceTaskCreatorOptionValueFCLToCNT.RegistryName));
		}
	}
}
