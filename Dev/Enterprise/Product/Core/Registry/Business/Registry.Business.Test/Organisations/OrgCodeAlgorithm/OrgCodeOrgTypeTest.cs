using CargoWise.Organizations.CodeGeneration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgCodeOrgType))]
	sealed class OrgCodeOrgTypeTest : RegistryBusinessObjectTemplateTestCase<OrgCodeOrgType>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override OrgCodeOrgType GetBusinessObjectToClone()
		{
			OrgCodeOrgType result = new OrgCodeOrgType("Food");
			result.Selected = true;
			return result;
		}

		protected override OrgCodeOrgType GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public void TestIOrgCodeOrgTypeDescription()
		{
			var result = ((IOrgCodeOrgType)new OrgCodeOrgType("Something")).Description;
			AssertEquals("Something", result);
		}

		public void TestIOrgCodeOrgTypeSelected()
		{
			BizObj.Selected = true;
			var result = ((IOrgCodeOrgType)BizObj).Selected;
			AssertEquals(true, result);
		}

		public void TestSelectedInfoReadOnly()
		{
			OrgCodeOrgTypeCollection collection = new OrgCodeOrgTypeCollection(new OrgCodeAlgorithm());
			OrgCodeOrgType orgType = collection.AddNew();

			collection.Parent.AlgorithmType = OrgCodeAlgorithmType.Override;
			AssertEquals("SelectedInfo.ReadOnly", false, BizObj.SelectedInfo.ReadOnly);

			collection.Parent.AlgorithmType = OrgCodeAlgorithmType.Default;
			AssertEquals("SelectedInfo.ReadOnly", true, orgType.SelectedInfo.ReadOnly);
		}
	}
}
