using System;
using System.Collections.Generic;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(DocAddressTypeXmlMappings))]
	sealed class DocAddressTypeXmlMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(DocAddressTypes.Codes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		protected override bool ShouldExcludeMapping(EnterpriseCodeExternalCodeMappings.Mapping mapping)
		{
			return (ExcludedMappings.Contains(mapping.ExternalCode));
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[] { DocAddressTypes.Codes.BranchOrCompanyProxyARAdress, DocAddressTypes.Codes.DebtorAddress };

		List<string> ExcludedMappings
		{
			get
			{
				if (excludedMappings == null)
				{
					excludedMappings = new List<string>();
					excludedMappings.Add(nameof(Xsd.DocAddressAddressType.NONE));
					excludedMappings.Add(nameof(Xsd.DocAddressAddressType.CCP));
					excludedMappings.Add(nameof(Xsd.DocAddressAddressType.CPP));
					excludedMappings.Add(nameof(Xsd.DocAddressAddressType.CPR));
				}
				return excludedMappings;
			}
		}

		List<string> excludedMappings;
	}
}
