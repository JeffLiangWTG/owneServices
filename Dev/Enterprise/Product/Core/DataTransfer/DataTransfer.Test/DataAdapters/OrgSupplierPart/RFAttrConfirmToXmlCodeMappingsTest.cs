using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business.CodeLists;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(RFAttrConfirmToXmlCodeMappings))]
	sealed class RFAttrConfirmToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(RFAttributeConfirmCode) }; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
			{
				RFAttributeConfirmCode.Codes.None,
				RFAttributeConfirmCode.Codes.PartAttribute1,
				RFAttributeConfirmCode.Codes.PartAttribute2,
				RFAttributeConfirmCode.Codes.PartAttribute3,
				RFAttributeConfirmCode.Codes.SerialNumber
			};
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}
	}
}
