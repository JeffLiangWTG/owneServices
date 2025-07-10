using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondDifferenceMoveDetailCollection : DependentBusinessObjectCollection<CusInBondMoveDetail, CusInBondMoveDetail>
	{
		public CusInBondDifferenceMoveDetailCollection(CusInBondMoveDetail master) : base(master)
		{
			MaxCountValidationEnable(1);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is CusInBondMoveDetail newElement)
			{
				newElement.B9_BM = Master.B9_BM;
				newElement.B9_B0 = Master.B9_B0;
				newElement.B9_SeqNo = Master.B9_SeqNo;
				newElement.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.NEW;
			}
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusInBondMoveDetailSchema.B9_B9_InBondMoveDetail;
	}
}
