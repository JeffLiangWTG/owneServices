using System;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(AddressTypeXmlMappings))]
	sealed class AddressTypeXmlMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(OrgConstants.AddressType) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		protected override bool ShouldExcludeMapping(EnterpriseCodeExternalCodeMappings.Mapping mapping)
		{
			return mapping.ExternalCode == nameof(Xsd.AddressCapabilityAddressType.MAIN);
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[] { OrgConstants.AddressType.Documentary };
		}
	}
}
