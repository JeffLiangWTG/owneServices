using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentDetailsGridColumnLayout))]
	sealed class HouseConsignmentDetailsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<HouseConsignmentDetailsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			("SequenceNumber", typeof(ZTextBoxColumnStyleInfo), 115),
			(CusInBondBillSchema.Constants.B0_RN_NKCountryOfExport, typeof(ZDropEditColumnStyleInfo), 115),
			(CusInBondBillSchema.Constants.B0_Weight, typeof(ZCalcEditColumnStyleInfo), 110),
			(CusInBondBillSchema.Constants.B0_WeightUQ, typeof(ZDropEditColumnStyleInfo), 115),
			(CusInBondBillSchema.Constants.B0_ReferenceID, typeof(ZTextBoxColumnStyleInfo), 330),
			(CusInBondBillSchema.Constants.B0_TransportPaymentMethod, typeof(ZDropEditColumnStyleInfo), 200),
			(CusInBondBillSchema.Constants.B0_RN_NKCountryOfDestination, typeof(ZDropEditColumnStyleInfo), 115),
			("Consignor+OrganisationPK", typeof(ZOrganisationFindBoxColumnStyleInfo), 115),
			("Consignor+E2_OA_Address", typeof(ZAddressDropEditColumnStyleInfo), 115),
			("Consignee+OrganisationPK", typeof(ZOrganisationFindBoxColumnStyleInfo), 115),
			("Consignee+E2_OA_Address", typeof(ZAddressDropEditColumnStyleInfo), 115),
			(CusInBondBillSchema.Constants.B0_RX_NKLinePriceCurrency, typeof(ZDropEditColumnStyleInfo), 115),
		};

		protected override Type GridBoundEntityType => typeof(NctsBill);
	}
}
