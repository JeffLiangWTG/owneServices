using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TemplateRegistryDataType))]
	public class TemplateRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TemplateRegistryDataType>
	{
		protected override TemplateRegistryDataType GetNewDataType()
		{
			return new TemplateRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "TemplateRegistryItemEditor"; }
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			var lhsParent = (TemplateCriteria)lhs;
			var rhsParent = (TemplateCriteria)rhs;

			AssertEquals(lhsParent.Enabled, rhsParent.Enabled);
			AssertEquals(lhsParent.Criterion1, rhsParent.Criterion1);
			AssertEquals(lhsParent.Criterion2, rhsParent.Criterion2);
			AssertEquals(lhsParent.Criterion3, rhsParent.Criterion3);
			AssertEquals(lhsParent.Criterion4, rhsParent.Criterion4);
			AssertEquals(lhsParent.Criterion5, rhsParent.Criterion5);
		}

		// Copied from https://stackoverflow.com/a/311179
		// Because alternate answer used SoapHexBinary https://stackoverflow.com/a/2556329
		// Mono Reference Implementation: https://github.com/mono/mono/blob/main/mcs/class/corlib/System.Runtime.Remoting.Metadata.W3cXsd2001/SoapHexBinary.cs#L70-L93
		static byte[] StringToByteArray(string hex)
		{
			var numberChars = hex.Length;
			var bytes = new byte[numberChars / 2];

			for (var i = 0; i < numberChars; i += 2)
			{
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}

			return bytes;
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			Mock<ISelectionCriteriaLookup> lookup = new Mock<ISelectionCriteriaLookup>();
			lookup.Setup(m => m.GetCriterionList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new CodeDescriptionPairList
				{
					new CodeDescriptionPair("ENT", "desc1"),
					new CodeDescriptionPair("BMS", "desc2"),
					new CodeDescriptionPair("FIX", "desc3")
				});
			ObjectFactory.Substitute(lookup.Object);

			var criteria1 = new TemplateCriteria();
			var criteria2 = new TemplateCriteria();

			criteria1.Enabled = true;
			criteria1.Criterion1 = "";
			criteria1.Criterion2 = "";
			criteria1.Criterion3 = "";
			criteria1.Criterion4 = "";
			criteria1.Criterion5 = "";

			criteria2.Enabled = false;
			criteria2.Criterion1 = "ENT";
			criteria2.Criterion2 = "";
			criteria2.Criterion3 = "BMS";
			criteria2.Criterion4 = "FIX";
			criteria2.Criterion5 = "";

			var byteArrayValue1 = StringToByteArray("3C003F0078006D006C002000760065007200730069006F006E003D00220031002E0030002200200065006E0063006F00640069006E0067003D0022007500740066002D003100360022003F003E003C00540065006D0070006C00610074006500430072006900740065007200690061003E003C0043006F00640065004D00610078004C0065006E006700740068003E0033003C002F0043006F00640065004D00610078004C0065006E006700740068003E003C0043006F006400650020002F003E003C004400650073006300720069007000740069006F006E0020002F003E003C0045006E00610062006C00650064003E0059003C002F0045006E00610062006C00650064003E003C0043007200690074006500720069006F006E00310020002F003E003C0043007200690074006500720069006F006E00320020002F003E003C0043007200690074006500720069006F006E00330020002F003E003C0043007200690074006500720069006F006E00340020002F003E003C0043007200690074006500720069006F006E00350020002F003E003C002F00540065006D0070006C00610074006500430072006900740065007200690061003E00");
			var byteArrayValue2 = StringToByteArray("3C003F0078006D006C002000760065007200730069006F006E003D00220031002E0030002200200065006E0063006F00640069006E0067003D0022007500740066002D003100360022003F003E003C00540065006D0070006C00610074006500430072006900740065007200690061003E003C0043006F00640065004D00610078004C0065006E006700740068003E0033003C002F0043006F00640065004D00610078004C0065006E006700740068003E003C0043006F006400650020002F003E003C004400650073006300720069007000740069006F006E0020002F003E003C0045006E00610062006C00650064003E004E003C002F0045006E00610062006C00650064003E003C0043007200690074006500720069006F006E0031003E0045004E0054003C002F0043007200690074006500720069006F006E0031003E003C0043007200690074006500720069006F006E00320020002F003E003C0043007200690074006500720069006F006E0033003E0042004D0053003C002F0043007200690074006500720069006F006E0033003E003C0043007200690074006500720069006F006E0034003E004600490058003C002F0043007200690074006500720069006F006E0034003E003C0043007200690074006500720069006F006E00350020002F003E003C002F00540065006D0070006C00610074006500430072006900740065007200690061003E00");

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(criteria1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(criteria2, byteArrayValue2)
			};
		}
	}
}
