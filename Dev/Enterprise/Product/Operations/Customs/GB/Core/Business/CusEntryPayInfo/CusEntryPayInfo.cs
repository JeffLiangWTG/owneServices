using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	[CodeProperty(CusEntryPayInfo.Schema.LRN)]
	public class CusEntryPayInfo : Customs.Business.CusEntryPayInfo, Integration.Customs.GB.ICusEntryPayInfo
	{
		public CusEntryPayInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : Customs.Business.AutoCusEntryPayInfo.Schema
		{
			public const string LRN = "LRN";
			public const string MRN = "MRN";
			public const string Importer = "Importer";
			public const string ImporterName = "ImporterName";
			public const string DeclarationReference = "DeclarationReference";
			public const string TransactionTypeDescription = "TransactionTypeDescription";
			public const string PaymentStatusDescription = "PaymentStatusDescription";
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override Customs.Business.CusEntryPayInfoLookups GetNewLookups()
		{
			return new CusEntryPayInfoLookups(this);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("2EF823C3-2F3A-46FA-AC35-7BA9D5F275C0", "CDS Cash Payment {0}", LRN);

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|LRN", Caption = "DUCR", FullDescription = "Declaration Unique Consignment Reference")]
		public ZString LRN => EntryHeader?.CH_BGMReference ?? ZString.Empty;

		public ZPropertyInfo LRNInfo => GetZPropertyInfo(Schema.LRN);

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|MRN", Caption = "MRN", FullDescription = "Movement Reference Number")]
		public ZString MRN => EntryHeader?.MovementReferenceNumber ?? ZString.Empty;

		public ZPropertyInfo MRNInfo => GetZPropertyInfo(Schema.MRN);

		[List(nameof(EntryHeader) + "." + nameof(CusEntryHeader.Declaration) + "." + nameof(CusEntryHeader.Declaration.Lookups) + "." + nameof(JobDeclarationLookups.Importers))]
		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|Importer", Caption = "Importer")]
		public ZGuid Importer => EntryHeader?.Declaration?.JE_OH_Importer ?? ZGuid.Empty;
		public ZPropertyInfo ImporterInfo => GetZPropertyInfo(Schema.Importer);

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|ImporterName", Caption = "Importer Name")]
		public ZString ImporterName => EntryHeader?.Declaration?.ImporterName ?? ZString.Empty;
		public ZPropertyInfo ImporterNameInfo => GetZPropertyInfo(Schema.ImporterName);

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|DeclarationReference", Caption = "Declaration Reference")]
		public ZString DeclarationReference => EntryHeader?.Declaration?.JE_DeclarationReference ?? ZString.Empty;
		public ZPropertyInfo DeclarationReferenceInfo => GetZPropertyInfo(Schema.DeclarationReference);

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|TransactionTypeDescription", Caption = "Transaction Type Description")]
		public ZString TransactionTypeDescription => Lookups.TransactionTypeList.GetDescriptionFromCode(C9_TransactionType);
		public ZPropertyInfo TransactionTypeDescriptionInfo => GetZPropertyInfo(Schema.TransactionTypeDescription);

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|PaymentStatusDescription", Caption = "Payment Status Description")]
		public ZString PaymentStatusDescription => Lookups.PaymentStatusList.GetDescriptionFromCode(C9_PaymentStatus);
		public ZPropertyInfo PaymentStatusDescriptionInfo => GetZPropertyInfo(Schema.PaymentStatusDescription);

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|C9_PaymentAmount", Caption = "Payment Amount")]
		public override ZDecimal C9_PaymentAmount { get => base.C9_PaymentAmount; set => base.C9_PaymentAmount = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryPayInfoLookups.TransactionTypeList))]
		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|C9_TransactionType", Caption = "Transaction Type")]
		public override ZString C9_TransactionType { get => base.C9_TransactionType; set => base.C9_TransactionType = value; }

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|C9_PaymentDate", Caption = "Payment Date")]
		public override ZDateTime C9_PaymentDate { get => base.C9_PaymentDate; set => base.C9_PaymentDate = value; }

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|C9_PaymentReference", Caption = "Payment Reference")]
		public override ZString C9_PaymentReference { get => base.C9_PaymentReference; set => base.C9_PaymentReference = value; }

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|C9_IncomingPayResponseNo", Caption = "Incoming Pay Response No.")]
		public override ZString C9_IncomingPayResponseNo { get => base.C9_IncomingPayResponseNo; set => base.C9_IncomingPayResponseNo = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryPayInfoLookups.PaymentStatusList))]
		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|C9_PaymentStatus", Caption = "Payment Status")]
		public override ZString C9_PaymentStatus { get => base.C9_PaymentStatus; set => base.C9_PaymentStatus = value; }

		[ResourceStringData("Enterprise.Customs.GB.Business.CusEntryPayInfo|C9_ReceiptDate", Caption = "Receipt Date")]
		public override ZDate C9_ReceiptDate { get => base.C9_ReceiptDate; set => base.C9_ReceiptDate = value; }
	}
}
