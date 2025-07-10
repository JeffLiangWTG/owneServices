using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TIP.Testing
{
	[TestedType(typeof(OrgPartRelationRegistryDataType))]
	class OrgPartRelationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OrgPartRelationRegistryDataType>
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			OrgPartRelationRegistryBusinessObjectCollection collection = new OrgPartRelationRegistryBusinessObjectCollection();
			OrgPartRelationRegistryBusinessObject element = (OrgPartRelationRegistryBusinessObject)collection.AddNew();
			element.OrgHeaderPK = new ZGuid("C3F842EF-3BE5-448C-BED3-0017B232C624");
			element.RelationshipType = OrgPartRelation.RelationshipTypes.Owner;
			string expectedXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
					<ArrayOfOrgPartRelationRegistryBusinessObject>
						<OrgPartRelationRegistryBusinessObject>
							<OrgHeaderPK>c3f842ef-3be5-448c-bed3-0017b232c624</OrgHeaderPK>
							<RelationshipType>OWN</RelationshipType>
						</OrgPartRelationRegistryBusinessObject>
						</ArrayOfOrgPartRelationRegistryBusinessObject>";
			byte[] bytes = Encoding.Unicode.GetBytes(expectedXml);
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, bytes) };
		}

		protected override OrgPartRelationRegistryDataType GetNewDataType()
		{
			return new OrgPartRelationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "OrgPartRelationRegistryItemEditor";
			}
		}
	}
}
