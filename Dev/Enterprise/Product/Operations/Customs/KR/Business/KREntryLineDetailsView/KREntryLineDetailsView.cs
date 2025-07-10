using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.KR.Business
{
	[CodeProperty(nameof(FormattedOriginalEntryLineNumber)), DescriptionProperty(nameof(FormattedOriginalEntryLineNumber))]
	public partial class KREntryLineDetailsView : AutoKREntryLineDetailsView
	{
		public KREntryLineDetailsView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("568A3472-457C-43CA-A507-0E5EE2F6B0DC", Caption = "Entry Number")]
		public override ZString KEL_EntryNum { get => base.KEL_EntryNum; set => base.KEL_EntryNum = value; }

		[ResourceStringData("E6890856-50B9-4AEC-A61D-C95BA9F6ECC4", Caption = "Entry Line No.")]
		public override ZShort KEL_LineNumber { get => base.KEL_LineNumber; set => base.KEL_LineNumber = value; }

		public ZString FormattedOriginalEntryLineNumber => base.KEL_LineNumber.ToString();

		[ResourceStringData("7D13D164-B1F0-49D8-AEEF-38D62B29EFEA", Caption = "Accepted Date")]
		public override ZDateTime KEL_EntryNumIssueDate { get => base.KEL_EntryNumIssueDate; set => base.KEL_EntryNumIssueDate = value; }

		[ResourceStringData("36EC8032-E869-494E-B84C-27F147832747", Caption = "Payer")]
		[List(nameof(Lookups) + "." + nameof(KREntryHeaderDetailsViewLookups.ConsigneeList))]
		public override ZGuid KEL_OH_DutyPayer { get => base.KEL_OH_DutyPayer; set => base.KEL_OH_DutyPayer = value; }

		[ResourceStringData("127C306B-5C36-4299-9495-691ABAA131F4", Caption = "Tariff")]
		public override ZString KEL_AdValoremTariff { get => base.KEL_AdValoremTariff; set => base.KEL_AdValoremTariff = value; }

		[ResourceStringData("3B1E2C72-0D63-45EC-BE25-6521D5AD1304", Caption = "Customs Value (KRW)")]
		public override ZDecimal KEL_CustomsValue { get => base.KEL_CustomsValue; set => base.KEL_CustomsValue = value; }

		[ResourceStringData("10035168-1F63-4E23-AE2A-6B27A0BAAC7D", Caption = "Invoice Description")]
		public override ZString KEL_Description { get => base.KEL_Description; set => base.KEL_Description = value; }

		[ResourceStringData("C5BD515F-F98E-40CE-9357-D2020E81C282", Caption = "Value For VAT")]
		public override ZDecimal KEL_ValueForVAT { get => base.KEL_ValueForVAT; set => base.KEL_ValueForVAT = value; }

		[ResourceStringData("0D267D85-E0E5-42CD-9829-190D5755AA33", Caption = "Invoice Lines Count")]
		public override ZInt KEL_InvoiceLineCount { get => base.KEL_InvoiceLineCount; set => base.KEL_InvoiceLineCount = value; }
	}
}
