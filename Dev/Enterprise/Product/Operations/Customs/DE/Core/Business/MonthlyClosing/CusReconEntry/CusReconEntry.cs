using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using CusReconSnapshot = Enterprise.Customs.Business.CusReconSnapshot;

namespace Enterprise.Customs.DE.Business
{
	public class CusReconEntry : Customs.Business.CusReconEntry, Integration.Customs.DE.ICusReconEntry
	{
		public CusReconEntry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusReconEntryLookups Lookups => (CusReconEntryLookups)base.Lookups;

		protected override Customs.Business.CusReconBase.CusReconEntryLookups GetNewLookups() => new CusReconEntryLookups(this);

		[ResourceStringData("0B2F72C6-6831-4B12-821A-61EC930444E8", Caption = "Local Clearance Date")]
		[ReadOnly(true)]
		public override ZDate CRE_EntryDate
		{
			get => base.CRE_EntryDate;
			set => base.CRE_EntryDate = value;
		}

		[ResourceStringData("BC3B9193-B19A-49E9-BE24-345D52CD82AC", Caption = "Registration Number")]
		[ReadOnly(true)]
		public override ZString CRE_OriginalEntryNumber
		{
			get => base.CRE_OriginalEntryNumber;
			set => base.CRE_OriginalEntryNumber = value;
		}

		[ReadOnly(true)]
		public override ZString CRE_EntryType
		{
			get => base.CRE_EntryType;
			set => base.CRE_EntryType = value;
		}

		[ResourceStringData("27F04208-D138-4C41-A3EE-0CA525CEC999", Caption = "Job Number")]
		public ZString JobNumber => EntryHeader.Declaration.JE_DeclarationReference;

		[ResourceStringData("1F931A59-6BBF-44F2-9670-8C1ED38B67DC", Caption = "Owner Ref")]
		public ZString OwnerRef => EntryHeader.Declaration.JE_OwnerRef;

		[ResourceStringData("7FA63767-8A35-4D54-A55D-48B5D115035D", Caption = "Has Changes?")]
		public ZString EntryHasChanges => CurrentSnapshot != null ? YesNoList.Descriptions.Yes : string.Empty;

		public new CusReconEntryLineCollection CusReconEntryLines => (CusReconEntryLineCollection)base.CusReconEntryLines;

		protected override Customs.Business.CusReconEntryLineCollection CreateNewCusReconEntryLineCollection() => new CusReconEntryLineCollection(this);

		internal CusReconSnapshot CurrentSnapshot => CusReconSnapshots.FirstOrDefault(x => x.CRS_Type == CusReconConstants.Current);

		internal CusReconSnapshot LodgedSnapshot => CusReconSnapshots.FirstOrDefault(x => x.CRS_Type == CusReconConstants.Lodged);
	}
}
