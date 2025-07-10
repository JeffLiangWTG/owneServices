using System;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public sealed class AuthorizationGridColumnsBag
{
	public static AuthorizationGridColumnsBag Instance => instance ?? (instance = new AuthorizationGridColumnsBag());

	[ThreadStatic]
	static AuthorizationGridColumnsBag instance;

	AuthorizationGridColumnsBag()
	{
		AGC_CodeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusAuthorizationUsage.Schema.AGC_Code, 80);
		CustomsCodeTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(nameof(CusAuthorizationUsage.CustomsCode), 80);
		EffectiveReferenceNumberCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(nameof(CusAuthorizationUsage.EffectiveReferenceNumber), 80);
		AGC_OH_OwnerOrganisationFindBoxColumn = new GridColumnReference<ZOrganisationFindBoxColumnStyleInfo>(CusAuthorizationUsage.Schema.AGC_OH_Owner, 80);
	}

	public IGridColumnReference AGC_CodeDropEditColumn { get; }

	public IGridColumnReference CustomsCodeTextBoxColumn { get; }

	public IGridColumnReference EffectiveReferenceNumberCodeFindBoxColumn { get; }

	public IGridColumnReference AGC_OH_OwnerOrganisationFindBoxColumn { get; }
}
