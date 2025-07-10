using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CusMiscRequestLine : Customs.Business.CusMiscRequestLine
	{
		public CusMiscRequestLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		[ResourceStringData("c79e0ed1-f96f-4bec-b9fc-373aa5a1bf6d", Caption = "Entry Number", MultipleKey = ElectronicDocumentTypeList.Codes._5AC)]
		[ResourceStringData("c79e0ed1-f96f-4bec-b9fc-373aa5a1bf6d", Caption = "Entry Number", MultipleKey = ElectronicDocumentTypeList.Codes._5GW)]
		[ResourceStringData("D507DBA2-E5E5-4308-ABBF-F7B95B3B23F8", Caption = "Import Entry Number", MultipleKey = ElectronicDocumentTypeList.Codes._5SG)]
		public ZString FormattedEntryNumber => MessageFunctions.GetFormattedEntryNumber(CML_EntryNumber, EntryType);

		[List(nameof(Lookups) + "." + nameof(CusMiscRequestLineLookups.ReferenceNumberTypeList))]
		[ResourceStringData("603869CC-E45B-4B63-9B16-02E4E394A784", Caption = "Entry Type")]
		public ZString EntryType => CML_EntryType != ReferenceNumberTypeList.Codes.EXP ? MessageFunctions.GetEntryType(CML_EntryNumber) : CML_EntryType;

		[ResourceStringData("29d5cd0b-e919-403b-b676-b7cbd09208eb", Caption = "Entry Details", MultipleKey = ElectronicDocumentTypeList.Codes._5AC)]
		[ResourceStringData("29d5cd0b-e919-403b-b676-b7cbd09208eb", Caption = "Entry Details", MultipleKey = ElectronicDocumentTypeList.Codes._5GW)]
		[ResourceStringData("8697C136-FF4E-47F2-95D5-1E6A5FB5E29E", Caption = "Request Details", MultipleKey = ElectronicDocumentTypeList.Codes._5SG)]
		public override ZString CML_Remarks
		{
			get => base.CML_Remarks;
			set => base.CML_Remarks = value;
		}
		protected override Customs.Business.CusMiscRequestLineLookups GetNewLookups() => new CusMiscRequestLineLookups(this);
		public new CusMiscRequestLineLookups Lookups => (CusMiscRequestLineLookups)base.Lookups;
	}
}
