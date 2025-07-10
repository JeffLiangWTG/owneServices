using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentDetailsGridColumnsBag))]
	sealed class HouseConsignmentDetailsGridColumnsBagTest : TestCase
	{
		public void TestSequenceNumberTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.SequenceNumberTextBoxColumn);
			var columnInfo = ColumnsBag.SequenceNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "SequenceNumber", columnInfo.ColumnName);
		}

		public void TestCountryOfExportDropEditColumn()
		{
			AssertNotNull(ColumnsBag.CountryOfExportDropEditColumn);
			var columnInfo = ColumnsBag.CountryOfExportDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "B0_RN_NKCountryOfExport", columnInfo.ColumnName);
		}

		public void TestCountryOfDestinationDropEditColumn()
		{
			AssertNotNull(ColumnsBag.CountryOfDestinationDropEditColumn);
			var columnInfo = ColumnsBag.CountryOfDestinationDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "B0_RN_NKCountryOfDestination", columnInfo.ColumnName);
		}

		public void TestWeightCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.WeightCalcEditColumn);
			var columnInfo = ColumnsBag.WeightCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertNull("BindToDecimalPlaces", columnInfo.BindToDecimalPlaces);
				AssertEquals("Decimals", 6, columnInfo.Decimals);
				AssertEquals("ColumnName", "B0_Weight", columnInfo.ColumnName);
			});
		}

		public void TestWeightUQDropEditColumn()
		{
			AssertNotNull(ColumnsBag.WeightUQDropEditColumn);
			var columnInfo = ColumnsBag.WeightUQDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "B0_WeightUQ", columnInfo.ColumnName);
		}

		public void TestReferenceIDTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.ReferenceIDTextBoxColumn);
			var columnInfo = ColumnsBag.ReferenceIDTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "B0_ReferenceID", columnInfo.ColumnName);
		}

		public void TestTransportPaymentMethodDropEditColumn()
		{
			AssertNotNull(ColumnsBag.TransportPaymentMethodDropEditColumn);
			var columnInfo = ColumnsBag.TransportPaymentMethodDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("ColumnName", "B0_TransportPaymentMethod", columnInfo.ColumnName);
			});
		}

		public void TestConsigneeAddressDropEditColumn()
		{
			AssertNotNull(ColumnsBag.ConsigneeAddressDropEditColumn);
			var columnInfo = ColumnsBag.ConsigneeAddressDropEditColumn.CreateGridColumnInfo() as ZAddressDropEditColumnStyleInfo;
			CombineAssertions(() =>
			{
				AssertNotNull(columnInfo);
				AssertEquals("ColumnName", "Consignee+E2_OA_Address", columnInfo.ColumnName);
				AssertEquals("Width", 115, columnInfo.Width);
				AssertEquals("Caption", "Consignee Address", columnInfo.Caption);
			});
		}

		public void TestConsigneeOrganisationFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.ConsigneeOrganisationFindBoxColumn);
			var columnInfo = ColumnsBag.ConsigneeOrganisationFindBoxColumn.CreateGridColumnInfo() as ZOrganisationFindBoxColumnStyleInfo;
			CombineAssertions(() =>
			{
				AssertNotNull(columnInfo);
				AssertEquals("ColumnName", "Consignee+OrganisationPK", columnInfo.ColumnName);
				AssertEquals("Width", 115, columnInfo.Width);
				AssertEquals("Caption", "Consignee Organization", columnInfo.Caption);
			});
		}

		public void TestConsignorAddressDropEditColumn()
		{
			AssertNotNull(ColumnsBag.ConsignorAddressDropEditColumn);
			var columnInfo = ColumnsBag.ConsignorAddressDropEditColumn.CreateGridColumnInfo() as ZAddressDropEditColumnStyleInfo;
			CombineAssertions(() =>
			{
				AssertNotNull(columnInfo);
				AssertEquals("ColumnName", "Consignor+E2_OA_Address", columnInfo.ColumnName);
				AssertEquals("Width", 115, columnInfo.Width);
				AssertEquals("Caption", "Consignor Address", columnInfo.Caption);
			});
		}

		public void TestConsignorOrganisationFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.ConsignorOrganisationFindBoxColumn);
			var columnInfo = ColumnsBag.ConsignorOrganisationFindBoxColumn.CreateGridColumnInfo() as ZOrganisationFindBoxColumnStyleInfo;
			CombineAssertions(() =>
			{
				AssertNotNull(columnInfo);
				AssertEquals("ColumnName", "Consignor+OrganisationPK", columnInfo.ColumnName);
				AssertEquals("Width", 115, columnInfo.Width);
				AssertEquals("Caption", "Consignor Organization", columnInfo.Caption);
			});
		}

		public void TestLinePriceCurrencyDropEditColumn()
		{
			AssertNotNull(ColumnsBag.LinePriceCurrencyDropEditColumn);
			var columnInfo = ColumnsBag.LinePriceCurrencyDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "B0_RX_NKLinePriceCurrency", columnInfo.ColumnName);
		}

		HouseConsignmentDetailsGridColumnsBag ColumnsBag => HouseConsignmentDetailsGridColumnsBag.Instance;
	}
}
