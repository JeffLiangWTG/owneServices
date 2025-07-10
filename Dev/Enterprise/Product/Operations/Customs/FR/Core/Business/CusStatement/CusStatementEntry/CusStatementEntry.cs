using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	[DependentBusinessObject(typeof(CusStatementHeader), "Entries")]
	public class CusStatementEntry : CusStatementLine
	{
		public CusStatementEntry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusStatementLineValidation GetNewValidation() => new CusStatementEntryValidation(this);

		protected override Customs.Business.CusStatementLineLookups GetNewLookups() => new CusStatementEntryLookups(this);

		public new CusStatementEntryLookups Lookups => (CusStatementEntryLookups)base.Lookups;

		[ResourceStringData("FR.CusStatementEntry.B3_EntryType", Caption = "Entry Type")]
		[List(nameof(Lookups) + "." + nameof(CusStatementEntryLookups.EntryTypeList))]
		public override ZString B3_EntryType { get => base.B3_EntryType; set => base.B3_EntryType = value; }

		[ResourceStringData("FR.CusStatementEntry.B3_EntryNum", Caption = "Entry Number")]
		public override ZString B3_EntryNum { get => base.B3_EntryNum; set => base.B3_EntryNum = value; }

		[ResourceStringData("FR.CusStatementEntry.B3_BrokerReference", Caption = "Reference Number")]
		public override ZString B3_BrokerReference { get => base.B3_BrokerReference; set => base.B3_BrokerReference = value; }
	}
}
