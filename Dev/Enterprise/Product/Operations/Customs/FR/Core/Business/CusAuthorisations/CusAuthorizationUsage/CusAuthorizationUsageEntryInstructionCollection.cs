using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business
{
	[DependentBusinessObject(typeof(CusEntryInstruction), nameof(CusEntryInstruction.CusAuthorizationUsages))]
	public class CusAuthorizationUsageEntryInstructionCollection : EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>
	{
		public CusAuthorizationUsageEntryInstructionCollection(CusEntryInstruction master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			SetDefaultAGC_Code((CusAuthorizationUsage)child);
		}

		void SetDefaultAGC_Code(CusAuthorizationUsage child)
		{
			if (Master.CEI_Style == DeltaIEImportDeclarationTypeList.Codes.I1)
			{
				child.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			}
		}
	}
}
