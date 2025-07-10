using System.Linq;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Common.JP.AFR.Testing
{
	class AFRBillCustomsStatusListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestIsRegisteredType()
		{
			var registeredCode = new[] {
				AFRBillCustomsStatusList.Codes.Registered,
				AFRBillCustomsStatusList.Codes.DoNotLoad,
				AFRBillCustomsStatusList.Codes.DoNotUnload,
				AFRBillCustomsStatusList.Codes.HLD,
				AFRBillCustomsStatusList.Codes.NL1,
				AFRBillCustomsStatusList.Codes.NL2,
				AFRBillCustomsStatusList.Codes.NL3,
				AFRBillCustomsStatusList.Codes.NL4,
				AFRBillCustomsStatusList.Codes.NL5,
				AFRBillCustomsStatusList.Codes.ReleasedHold,
				AFRBillCustomsStatusList.Codes.ReleasedDoNotLoad,
				AFRBillCustomsStatusList.Codes.ReleasedDoNotUnload
			};

			foreach (ICodeDescription pair in new AFRBillCustomsStatusList())
			{
				NUnit.Framework.Assert.That(AFRBillCustomsStatusList.IsRegisteredType(pair.Code), Is.EqualTo(registeredCode.Contains(pair.Code)), pair.Code);
			}
		}

		[ExpectNoExceptions]
		public void TestIsRiskAssessmentReceivedType()
		{
			var riskAssessedCode = new[] {
				AFRBillCustomsStatusList.Codes.DoNotLoad,
				AFRBillCustomsStatusList.Codes.DoNotUnload,
				AFRBillCustomsStatusList.Codes.HLD
			};

			foreach (ICodeDescription pair in new AFRBillCustomsStatusList())
			{
				NUnit.Framework.Assert.That(AFRBillCustomsStatusList.IsRiskAssessmentReceivedType(pair.Code), Is.EqualTo(riskAssessedCode.Contains(pair.Code)), pair.Code);
			}
		}
	}
}
