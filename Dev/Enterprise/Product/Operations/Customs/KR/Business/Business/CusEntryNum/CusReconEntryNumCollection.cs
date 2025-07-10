using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntryNumCollection : BaseCusEntryNumCollection<CusReconDeclaration>
	{
		public CusReconEntryNumCollection(CusReconDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			((CusEntryNumber)dependent).CE_ParentTable = CusReconDeclarationSchema.Constants.TableName;
		}
	}
}
