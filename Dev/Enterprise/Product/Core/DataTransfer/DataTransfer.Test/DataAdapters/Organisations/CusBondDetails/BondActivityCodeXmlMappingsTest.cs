using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(BondActivityCodeXmlMappings))]
	sealed class BondActivityCodeXmlMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(ActivityCodeList) }; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
			{
				ActivityCodeList.Codes._1,
				ActivityCodeList.Codes._1a,
				ActivityCodeList.Codes._1a1,
				ActivityCodeList.Codes._2,
				ActivityCodeList.Codes._3,
				ActivityCodeList.Codes._3a,
				ActivityCodeList.Codes._3a3,
				ActivityCodeList.Codes._4,
				ActivityCodeList.Codes._5,
				ActivityCodeList.Codes._6,
				ActivityCodeList.Codes._7,
				ActivityCodeList.Codes._8,
				ActivityCodeList.Codes._9,
				ActivityCodeList.Codes._10,
				ActivityCodeList.Codes._11,
				ActivityCodeList.Codes._12,
				ActivityCodeList.Codes._13,
				ActivityCodeList.Codes._14,
				ActivityCodeList.Codes._15,
				ActivityCodeList.Codes._16,
				ActivityCodeList.Codes._17,
				ActivityCodeList.Codes._18,
				ActivityCodeList.Codes._19,
				ActivityCodeList.Codes._20,
			};
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}
	}
}
