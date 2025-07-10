using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(RelationshipToXmlCodeMappings))]
	sealed class RelationshipToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
			{
				OrgPartRelation.RelationshipTypes.WarehouseConsignee
			};
		}

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(OrgPartRelation.RelationshipTypes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}
	}
}
