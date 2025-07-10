using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class DispatchInstructionNumber : CusEntryNumber
	{
		public DispatchInstructionNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("0B49C0EF-C534-4C15-AB55-C94FB5A9F836", "Dispatch Instruction Documents");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CE_EntryIsSystemGenerated = false;
			CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			CE_Category = CusEntryNumber.Categories.DispatchInstructionDocument;
		}

		public new DispatchInstructionNumberLookups Lookups => (DispatchInstructionNumberLookups)base.Lookups;

		protected override CusEntryNumLookups GetNewLookups() => new DispatchInstructionNumberLookups(this);

		public new DispatchInstructionNumberValidation Validation => (DispatchInstructionNumberValidation)base.Validation;

		protected override CusEntryNumValidation GetNewValidation() => new DispatchInstructionNumberValidation(this);

		[MaxLength(2)]
		public override ZString CE_EntryType { get => base.CE_EntryType; set => base.CE_EntryType = value; }

		[MaxLength(25)]
		public override ZString CE_EntryNum { get => base.CE_EntryNum; set => base.CE_EntryNum = value; }
	}
}
