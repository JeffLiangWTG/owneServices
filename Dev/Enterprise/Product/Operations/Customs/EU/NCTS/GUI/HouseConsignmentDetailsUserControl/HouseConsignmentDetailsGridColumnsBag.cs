using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentDetailsGridColumnsBag
	{
		HouseConsignmentDetailsGridColumnsBag()
		{
			SequenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>($"{nameof(NctsBill.SequenceNumber)}", 115);
			CountryOfExportDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusInBondBillSchema.Constants.B0_RN_NKCountryOfExport, 115);
			WeightCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(CusInBondBillSchema.Constants.B0_Weight, 110, c =>
			{
				c.BindToDecimalPlaces = null;
				c.Decimals = 6;
			});
			WeightUQDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusInBondBillSchema.Constants.B0_WeightUQ, 115);
			ReferenceIDTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusInBondBillSchema.Constants.B0_ReferenceID, 330);
			TransportPaymentMethodDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusInBondBillSchema.Constants.B0_TransportPaymentMethod, 200, c =>
			{
				c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			});

			CountryOfDestinationDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusInBondBillSchema.Constants.B0_RN_NKCountryOfDestination, 115, c =>
			{
				c.IsVisible = false;
			});

			ConsignorOrganisationFindBoxColumn = new GridColumnReference<ZOrganisationFindBoxColumnStyleInfo>($"{nameof(NctsBill.Consignor)}.{nameof(JobDocAddress.OrganisationPK)}", 115, c =>
			{
				c.BindToList = $"{nameof(NctsBill.Lookups)}.{nameof(NctsBillLookups.Consignors)}";
				c.Caption = Res.GetString("d869e2ed-94f1-4e63-a319-6e8d72413bfe", "Consignor Organization");
				c.GroupName = Res.GetData("4e91558d-3dd3-4210-8b87-e21f19665d05", "Consignor");
				c.IsVisible = false;
			});

			ConsignorAddressDropEditColumn = new GridColumnReference<ZAddressDropEditColumnStyleInfo>($"{nameof(NctsBill.Consignor)}.{nameof(JobDocAddress.E2_OA_Address)}", 115, c =>
			{
				c.Caption = Res.GetString("21e82782-f51c-4888-afa5-7648539c0b76", "Consignor Address");
				c.GroupName = Res.GetData("4e91558d-3dd3-4210-8b87-e21f19665d05", "Consignor");
				c.IsVisible = false;
			});

			ConsigneeOrganisationFindBoxColumn = new GridColumnReference<ZOrganisationFindBoxColumnStyleInfo>($"{nameof(NctsBill.Consignee)}.{nameof(JobDocAddress.OrganisationPK)}", 115, c =>
			{
				c.BindToList = $"{nameof(NctsBill.Lookups)}.{nameof(NctsBillLookups.Consignees)}";
				c.Caption = Res.GetString("5f47af88-24fa-4f09-9f2f-106bd92a2252", "Consignee Organization");
				c.GroupName = Res.GetData("8bb67141-ef6b-4948-b6bf-268a23bcf0d1", "Consignee");
				c.IsVisible = false;
			});

			ConsigneeAddressDropEditColumn = new GridColumnReference<ZAddressDropEditColumnStyleInfo>($"{nameof(NctsBill.Consignee)}.{nameof(JobDocAddress.E2_OA_Address)}", 115, c =>
			{
				c.Caption = Res.GetString("7f6ac6fc-386a-437a-bd54-370887d02cd5", "Consignee Address");
				c.GroupName = Res.GetData("8bb67141-ef6b-4948-b6bf-268a23bcf0d1", "Consignee");
				c.IsVisible = false;
			});
			LinePriceCurrencyDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusInBondBillSchema.Constants.B0_RX_NKLinePriceCurrency, 115);
		}

		public static HouseConsignmentDetailsGridColumnsBag Instance => instanceLazy.Value;

		[ThreadSafe]
		static readonly Lazy<HouseConsignmentDetailsGridColumnsBag> instanceLazy = new(() => new());

		public IGridColumnReference SequenceNumberTextBoxColumn { get; }

		public IGridColumnReference CountryOfExportDropEditColumn { get; }

		public IGridColumnReference WeightCalcEditColumn { get; }

		public IGridColumnReference WeightUQDropEditColumn { get; }

		public IGridColumnReference ReferenceIDTextBoxColumn { get; }

		public IGridColumnReference TransportPaymentMethodDropEditColumn { get; }

		public IGridColumnReference CountryOfDestinationDropEditColumn { get; }

		public IGridColumnReference ConsignorOrganisationFindBoxColumn { get; }

		public IGridColumnReference ConsignorAddressDropEditColumn { get; }

		public IGridColumnReference ConsigneeOrganisationFindBoxColumn { get; }

		public IGridColumnReference ConsigneeAddressDropEditColumn { get; }

		public IGridColumnReference LinePriceCurrencyDropEditColumn { get; }
	}
}
