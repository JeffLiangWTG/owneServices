using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class IEEntryInstructionGuaranteeGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<IEEntryInstructionGuaranteeGridColumnsLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(CusBondDetail.Schema.PW_CPH_Guarantee, typeof(ZGuidFindBoxColumnStyleInfo), 120),
			(CusBondDetail.Schema.PW_BondType, typeof(ZDropEditColumnStyleInfo), 40),
			(CusBondDetail.Schema.PW_BondNumber, typeof(ZTextBoxColumnStyleInfo), 120),
			(CusBondDetail.Schema.PW_GuaranteeDescription, typeof(ZTextBoxColumnStyleInfo), 120),
			(CusBondDetail.Schema.PW_Password, typeof(ZTextBoxColumnStyleInfo), 120),
			(CusBondDetail.Schema.PW_BondAmount, typeof(ZCalcEditColumnStyleInfo), 80),
			(CusBondDetail.Schema.PW_RX_NKCurrency, typeof(ZDropEditColumnStyleInfo), 80),
			(CusBondDetail.Schema.PW_BondFiledPort, typeof(ZCodeFindBoxColumnStyleInfo), 100),
			(CusBondDetail.Schema.PW_RN_NKCountryOfIssue, typeof(ZDropEditColumnStyleInfo), 120),
		};

		protected override Type GridBoundEntityType => typeof(CusBondDetail);

		protected override void SetUp()
		{
			base.SetUp();
			EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
