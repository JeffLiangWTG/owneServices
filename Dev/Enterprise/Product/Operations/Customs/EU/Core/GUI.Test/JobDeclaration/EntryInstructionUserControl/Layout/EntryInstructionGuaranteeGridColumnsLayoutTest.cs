using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryInstructionGuaranteeGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<EntryInstructionGuaranteeGridColumnsLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(CusBondDetail.Schema.PW_BondType, typeof(ZDropEditColumnStyleInfo), 40),
			(CusBondDetail.Schema.PW_BondNumber, typeof(ZMultiControlColumnStyleInfo), 120),
			(CusBondDetail.Schema.PW_BondNumber2, typeof(ZTextBoxColumnStyleInfo), 120),
			(CusBondDetail.Schema.PW_Password, typeof(ZTextBoxColumnStyleInfo), 120),
			(CusBondDetail.Schema.PW_HolderIdentification, typeof(ZTextBoxColumnStyleInfo), 120),
			(CusBondDetail.Schema.PW_RX_NKCurrency, typeof(ZCodeFindBoxColumnStyleInfo), 80),
			(CusBondDetail.Schema.PW_BondAmount, typeof(ZCalcEditColumnStyleInfo), 80),
			(CusBondDetail.Schema.PW_BondFiledPort, typeof(ZCodeFindBoxColumnStyleInfo), 100),
			(CusBondDetail.Schema.PW_SuretyCode, typeof(ZDropEditColumnStyleInfo), 100),
		};

		protected override Type GridBoundEntityType => typeof(CusBondDetail);
	}
}
