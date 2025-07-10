using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ClientInTemplateSelectionCriteriaOrgType))]
	sealed class ClientInTemplateSelectionCriteriaOrgTypeTest : RegistryBusinessObjectTemplateTestCase<ClientInTemplateSelectionCriteriaOrgType>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ClientInTemplateSelectionCriteriaOrgType GetBusinessObjectToClone()
		{
			var result = (ClientInTemplateSelectionCriteriaOrgType)GetNewBusinessObject();
			result.OrgTypeCode = "XYZ";

			return result;
		}

		protected override ClientInTemplateSelectionCriteriaOrgType GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
