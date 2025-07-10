using System;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(URLTypeXmlMappings))]
	sealed class URLTypeXmlMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(OrgWebUrlList) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		protected override bool ShouldExcludeMapping(EnterpriseCodeExternalCodeMappings.Mapping mapping)
		{
			return false;
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return Array.Empty<string>();
		}
	}
}
