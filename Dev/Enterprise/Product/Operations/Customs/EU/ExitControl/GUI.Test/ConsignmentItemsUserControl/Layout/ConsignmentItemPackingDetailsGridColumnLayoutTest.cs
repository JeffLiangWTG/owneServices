using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ConsignmentItemPackingDetailsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ConsignmentItemPackingDetailsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(FormattableString.Invariant($"Package+{CusExitConsignmentPackage.Schema.CXP_Sequence}"), typeof(ZTextBoxColumnStyleInfo), 50),
			(FormattableString.Invariant($"Package+{CusExitConsignmentPackage.Schema.CXP_Quantity}"), typeof(ZCalcEditColumnStyleInfo), 100),
			(FormattableString.Invariant($"Package+{CusExitConsignmentPackage.Schema.CXP_PackageType}"), typeof(ZDropEditColumnStyleInfo), 60),
			(FormattableString.Invariant($"Package+{CusExitConsignmentPackage.Schema.CXP_MarksAndNumbers}"), typeof(ZTextBoxColumnStyleInfo), 100),
			(CusExitConsignmentPivot.Schema.CNP_CXN_Container, typeof(ZGuidDropEditColumnStyleInfo), 112),
		};

		protected override Type GridBoundEntityType => typeof(CusExitConsignmentPivot);
	}
}
