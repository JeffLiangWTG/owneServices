using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatHeader : AutoCusIntrastatHeader
		, IClusterKeyMaster
		, Integration.Customs.EU.ICusIntrastatHeader
	{
		public CusIntrastatHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("f5afef61-6161-45d6-b24b-716a7bac5634", Caption = "Traders Reference")]
		public override ZString CIH_TradersReference
		{
			get => base.CIH_TradersReference;
			set => base.CIH_TradersReference = value;
		}

		[ResourceStringData("5e0a2518-723a-44c8-945f-1f86cc06fd64", Caption = "Consignee VAT")]
		public override ZString CIH_ConsigneeVAT
		{
			get => base.CIH_ConsigneeVAT;
			set => base.CIH_ConsigneeVAT = value;
		}

		[ResourceStringData("49b0eab9-9900-42fa-9abb-31342711e7ce", Caption = "Nature of Transaction", ShortCaption = "Nature of Tran.")]
		[List(nameof(Lookups) + "." + nameof(CusIntrastatHeaderLookups.NatureOfTransactionList))]
		public override ZString CIH_NatureOfTransaction
		{
			get => base.CIH_NatureOfTransaction;
			set => base.CIH_NatureOfTransaction = value;
		}

		[ResourceStringData("ad830a6f-bdea-420f-a9bf-485d09fd7b3d", Caption = "Supplier VAT")]
		public override ZString CIH_SupplierVAT
		{
			get => base.CIH_SupplierVAT;
			set => base.CIH_SupplierVAT = value;
		}

		[ResourceStringData("0101ed29-838f-4c7e-827a-e6e13ac3ec7f", Caption = "Supplier Code")]
		public override ZGuid CIH_OH_Supplier
		{
			get => base.CIH_OH_Supplier;
			set => base.CIH_OH_Supplier = value;
		}

		[ResourceStringData("01c6d429-4518-4ae2-b059-b82dd45e78da", Caption = "Supplier Name")]
		public override ZString CIH_SupplierName
		{
			get => base.CIH_SupplierName;
			set => base.CIH_SupplierName = value;
		}

		[ResourceStringData("8f2ee04a-0dd9-483b-95a0-35b6dadd8900", Caption = "Consignee Code")]
		public override ZGuid CIH_OH_Consignee
		{
			get => base.CIH_OH_Consignee;
			set => base.CIH_OH_Consignee = value;
		}

		[ResourceStringData("b452f37c-ee91-41f7-8577-0f48c9cc6312", Caption = "Consignee Name")]
		public override ZString CIH_ConsigneeName
		{
			get => base.CIH_ConsigneeName;
			set => base.CIH_ConsigneeName = value;
		}

		[ResourceStringData("071a6ca7-d2f1-45a5-a703-0343ca54eb68", Caption = "Country Of Supply")]
		[List(nameof(Lookups) + "." + nameof(CusIntrastatHeaderLookups.Countries))]
		public override ZString CIH_CountryOfSupply
		{
			get => base.CIH_CountryOfSupply;
			set => base.CIH_CountryOfSupply = value;
		}

		[ResourceStringData("faf7592f-9b97-4ebe-a318-479b1d8c2e5a", Caption = "Country Of Receipt")]
		[List(nameof(Lookups) + "." + nameof(CusIntrastatHeaderLookups.Countries))]
		public override ZString CIH_CountryOfReceipt
		{
			get => base.CIH_CountryOfReceipt;
			set => base.CIH_CountryOfReceipt = value;
		}

		[ResourceStringData("a81799bb-a5f9-4220-beb9-8fc3808be32c", Caption = "Transaction Date")]
		public override ZDate CIH_TransactionDate
		{
			get => base.CIH_TransactionDate;
			set => base.CIH_TransactionDate = value;
		}

		[ResourceStringData("6e7d24a6-d238-4b4f-bc02-b6e0fbbb26b4", Caption = "Mode Of Transport")]
		[List(nameof(Lookups) + "." + nameof(CusIntrastatHeaderLookups.ModeOfTransportList))]
		public override ZString CIH_ModeOfTransport
		{
			get => base.CIH_ModeOfTransport;
			set => base.CIH_ModeOfTransport = value;
		}

		[ResourceStringData("adf890c8-4f45-4418-b5d0-fdccfc875a33", Caption = "Incoterm")]
		[List(nameof(Lookups) + "." + nameof(CusIntrastatHeaderLookups.IncoTermList))]
		public override ZString CIH_IncoTerm
		{
			get => base.CIH_IncoTerm;
			set => base.CIH_IncoTerm = value;
		}

		public ZString CountryCode => Company.GC_RN_NKCountryCode;

		public ZBool IsImport => CIH_CountryOfSupply != CountryCode;

		[LightValidationTestExempt]
		public override ZInt CIH_ClusterKey
		{
			get => base.CIH_ClusterKey;
			set
			{
				if (CIH_ClusterKey != value)
				{
					this.CheckCanSetMasterClusterKey();
					base.CIH_ClusterKey = value;
					if (!IsCopying)
					{
						CusIntrastatLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ChildEditable(true)]
		public ICusIntrastatLineCollection<CusIntrastatLine> CusIntrastatLines
		{
			get
			{
				if (cusIntrastatLines == null)
				{
					cusIntrastatLines = CreateNewCusIntrastatLineCollection();
					RegisterEditableChildObject(cusIntrastatLines);
				}
				return cusIntrastatLines;
			}
		}
		ICusIntrastatLineCollection<CusIntrastatLine> cusIntrastatLines;

		protected virtual ICusIntrastatLineCollection<CusIntrastatLine> CreateNewCusIntrastatLineCollection() => new CusIntrastatLineCollection<CusIntrastatLine>(this);

		#region IClusterKeyMaster

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CIH_ClusterKeyInfo;

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("DEDB4E52-1753-49DA-932D-E26DDF545C0F", "Intrastat - Transaction");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CIH_GC_Company = GlbBranch.CurrentBranch.GB_GC;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;
	}
}
