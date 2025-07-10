using System;
using System.Collections.Generic;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using CusExitConsignmentPivot = Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ConsignmentItemPackingDetailsUcc6GridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ConsignmentItemPackingDetailsUcc6GridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
			(FormattableString.Invariant($"Package+{CusExitConsignmentPackage.Schema.CXP_Sequence}"), typeof(ZTextBoxColumnStyleInfo), 50),
			(FormattableString.Invariant($"Package+{CusExitConsignmentPackage.Schema.CXP_Quantity}"), typeof(ZCalcEditColumnStyleInfo), 100),
			(FormattableString.Invariant($"Package+{CusExitConsignmentPackage.Schema.CXP_PackageType}"), typeof(ZDropEditColumnStyleInfo), 60),
			(FormattableString.Invariant($"Package+{CusExitConsignmentPackage.Schema.CXP_MarksAndNumbers}"), typeof(ZTextBoxColumnStyleInfo), 100),
			(AutoCusExitConsignmentPivot.Schema.CNP_CXN_Container, typeof(ZGuidDropEditColumnStyleInfo), 112),
			(FormattableString.Invariant($"Package+{CusExitConsignmentPackage.Schema.CXP_MarksAndNumbersStatus}"), typeof(ZDropEditColumnStyleInfo), 50),
		};

	protected override Type GridBoundEntityType => typeof(CusExitConsignmentPivot);
}
