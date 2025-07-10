using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GuaranteeForDeclarationCollection : DependentBusinessObjectCollection<GuaranteeForDeclaration, JobDeclaration>
	{
		public GuaranteeForDeclarationCollection(JobDeclaration declaration)
				: base(declaration)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((GuaranteeForDeclaration)child).EnsurePivotExists();
		}

		protected override string FkColumnName => CusBondDetailSchema.PW_ParentID.Name;
	}
}
