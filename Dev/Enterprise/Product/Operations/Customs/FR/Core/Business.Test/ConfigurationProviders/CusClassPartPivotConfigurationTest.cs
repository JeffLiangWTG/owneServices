using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotConfiguration))]
	public class CusClassPartPivotConfigurationTest : CusClassPartPivotConfigurationAbstractTest
	{
		public override void TestUCCAdditionalInfosSupport()
		{
			CombineAssertions("EXP pivot supports UCC6 when EnableDeltaIEForExports registry is enabled.", () =>
			{
				AssertUCC6AdditionalInfoSupported(false, ClassificationType.EXP, false, false);
				AssertUCC6AdditionalInfoSupported(false, ClassificationType.EXP, false, true);
				AssertUCC6AdditionalInfoSupported(true, ClassificationType.EXP, true, false);
				AssertUCC6AdditionalInfoSupported(true, ClassificationType.EXP, true, true);
			});

			CombineAssertions("IMP pivot supports UCC6 when EnableDeltaIEForImports registry is enabled.", () =>
			{
				AssertUCC6AdditionalInfoSupported(false, ClassificationType.IMP, false, false);
				AssertUCC6AdditionalInfoSupported(true, ClassificationType.IMP, false, true);
				AssertUCC6AdditionalInfoSupported(false, ClassificationType.IMP, true, false);
				AssertUCC6AdditionalInfoSupported(true, ClassificationType.IMP, true, true);
			});

			CombineAssertions("BTH pivot supports UCC6 when either EnableDeltaIEForExports or EnableDeltaIEForImports registry is enabled.", () =>
			{
				AssertUCC6AdditionalInfoSupported(false, ClassificationType.Both, false, false);
				AssertUCC6AdditionalInfoSupported(true, ClassificationType.Both, false, true);
				AssertUCC6AdditionalInfoSupported(true, ClassificationType.Both, true, false);
				AssertUCC6AdditionalInfoSupported(true, ClassificationType.Both, true, true);
			});
		}

		void AssertUCC6AdditionalInfoSupported(bool expectedResult, string childType, bool isUCC6RegistyEnabledForExport, bool isUCC6RegistyEnabledForImport)
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = childType;

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, isUCC6RegistyEnabledForExport))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, isUCC6RegistyEnabledForImport))
			{
				AssertEquals(expectedResult, partPivot.Configuration.UCCAdditionalInfosSupport(partPivot));
			}
		}
	}
}
