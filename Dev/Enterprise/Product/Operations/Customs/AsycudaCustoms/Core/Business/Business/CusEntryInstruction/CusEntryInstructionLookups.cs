using CargoWise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		public override CodeDescriptionPairList StyleList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var declaration = Parent.JobDeclaration;
				if (declaration != null)
				{
					result = EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGroupingAndShipmentType(Factory, declaration.GetDefaultDataGroupingCode(), declaration.JE_MessageType);
				}
				return result;
			}
		}

		public ICodeDescriptionPairList PortOfExitList => Parent.JobDeclaration?.Lookups.CustomsOfficeList ?? new CodeDescriptionPairList();

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
	}
}
