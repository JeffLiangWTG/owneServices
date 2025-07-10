using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StaffReportingRoleRegistryDataType))]
	sealed class StaffReportingRoleRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<StaffReportingRoleRegistryDataType>
	{
		protected override StaffReportingRoleRegistryDataType GetNewDataType()
		{
			var defaultRoles = new StaffReportingRoleCollection();
			defaultRoles.Add("AAA", (NoResString)"AAA Description", enabled: true, isMandatory: false, sharedRoleAllowed: true);
			defaultRoles.Add("ZZZ", (NoResString)"ZZZ Description", enabled: true, isMandatory: false, sharedRoleAllowed: false);
			return new StaffReportingRoleRegistryDataType(defaultRoles);
		}

		protected override string ExpectedEditorName
		{
			get { return "StaffReportingRoleRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var defaultRoles = new StaffReportingRoleCollection();
			var role1 = defaultRoles.AddNew();
			role1.Code = "AAA";
			role1.EnglishDescription = "AAA Description";
			role1.Bool = true;
			role1.SharedRoleAllowed = false;
			role1.IsMandatory = false;

			var role2 = defaultRoles.AddNew();
			role2.Code = "ZZZ";
			role2.EnglishDescription = "ZZZ Description";
			role2.Bool = false;
			role2.SharedRoleAllowed = false;
			role2.IsMandatory = false;

			byte[] byteArrayvalue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfStaffReportingRole xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><StaffReportingRole><CodeMaxLength>3</CodeMaxLength><Code>AAA</Code><Description>AAA Description</Description><Bool>Y</Bool><SharedRoleAllowed>N</SharedRoleAllowed><IsMandatory>N</IsMandatory></StaffReportingRole><StaffReportingRole><CodeMaxLength>3</CodeMaxLength><Code>ZZZ</Code><Description>ZZZ Description</Description><Bool>N</Bool><SharedRoleAllowed>N</SharedRoleAllowed><IsMandatory>N</IsMandatory></StaffReportingRole></ArrayOfStaffReportingRole>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(defaultRoles, byteArrayvalue)
			};
		}
	}
}
