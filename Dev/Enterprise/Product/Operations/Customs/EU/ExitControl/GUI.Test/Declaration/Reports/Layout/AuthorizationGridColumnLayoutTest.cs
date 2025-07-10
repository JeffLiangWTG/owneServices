using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class AuthorizationGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<AuthorizationGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns =>
	[
		(CusAuthorizationUsage.Schema.AGC_Code, typeof(ZDropEditColumnStyleInfo), 80),
		(nameof(CusAuthorizationUsage.CustomsCode), typeof(ZTextBoxColumnStyleInfo), 80),
		(nameof(CusAuthorizationUsage.EffectiveReferenceNumber), typeof(ZCodeFindBoxColumnStyleInfo), 80),
		(CusAuthorizationUsage.Schema.AGC_OH_Owner, typeof(ZOrganisationFindBoxColumnStyleInfo), 80),
	];

	protected override Type GridBoundEntityType => typeof(CusAuthorizationUsage);
}
