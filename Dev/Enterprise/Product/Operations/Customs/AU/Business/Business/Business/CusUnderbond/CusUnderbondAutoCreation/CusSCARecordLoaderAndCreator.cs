// using Enterprise.ZArchitecture.Schema;
// using Enterprise.Customs.AU.SeaCargo.Business;
// using CargoWise.EntityFramework;
// using CargoWise.Types;
// using Enterprise.Customs.Business.Interfaces;

// namespace Enterprise.Customs.AU.Declaration.Business
// {
// 	public class CusSCARecordLoader : BaseRecordLoaderAndCreator
// 	{
// 		public CusSCARecordLoader(BusinessObjectFactory Factory) : base(Factory)
// 		{
// 		}

// 		public ICusUnderbondDependentCollectionParent LoadSCARecord(ZString LloydsNumber, ZString VoyageNumber, ZString OceanBillNum, ZString HouseBillNum, ZString ContainerNumber, ZString ContainerMode)
// 		{
// 			ICusUnderbondDependentCollectionParent Result = null;
// 			CMRCusSCAContainer Container = null;
// 			ZQuery ContainerFilter = new ZQuery(CusSCAContainerSchema.CN_ContainerNumber, ContainerNumber);
// 			foreach (CMRCusSCAContainer MatchingContainer in Factory.Load(typeof(CMRCusSCAContainer), ContainerFilter))
// 			{
// 				if (MatchingContainer.OceanBill != null && (MatchingContainer.OceanBill.CB_LloydsIMO == LloydsNumber && MatchingContainer.OceanBill.CB_Voyage == VoyageNumber))
// 				{
// 					if (OceanBillNum.IsEmpty || MatchingContainer.OceanBill.CB_OceanBill.IsEmpty || OceanBillNum == MatchingContainer.OceanBill.CB_OceanBill)
// 					{
// 						Container = MatchingContainer;
// 					}
// 				}
// 			}
// 			if (Container == null)
// 			{
// 				CMRCusSCAOceanBill OceanBill = null;
// 				if (!OceanBillNum.IsEmpty)
// 				{
// 					ZQuery OceanBillFilter = new ZQuery();
// 					OceanBillFilter.AddToFilter(CusSCAOceanBillSchema.CB_LloydsIMO, LloydsNumber);
// 					OceanBillFilter.AddToFilter(CusSCAOceanBillSchema.CB_Voyage, VoyageNumber);
// 					OceanBillFilter.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, OceanBillNum);
// 					OceanBill = Factory.LoadTop1<CMRCusSCAOceanBill>(OceanBillFilter);
// 				}
// 			}
// 			if (ContainerMode == CMRImportCargoTypes.Codes.FullContainerLoad || ContainerMode == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills)
// 			{
// 				Result = Container;
// 			}
// 			if (!HouseBillNum.IsEmpty && Container != null)
// 			{
// 				CMRCusSCAHouse House = null;
// 				foreach (CMRCusSCAHouse OceanBillHouse in Container.OceanBill.HouseBills)
// 				{
// 					if (OceanBillHouse.CA_HouseBill == HouseBillNum)
// 					{
// 						House = OceanBillHouse;
// 						break;
// 					}
// 				}
// 				if (House != null)
// 				{
// 					CMRCusSCAPivot Pivot = null;
// 					foreach (CMRCusSCAPivot ContainerPivot in Container.Pivots)
// 					{
// 						if (ContainerPivot.HouseBill == House)
// 						{
// 							Pivot = ContainerPivot;
// 							break;
// 						}
// 					}
// 					if (Pivot != null)
// 					{
// 						Result = Pivot;
// 					}
// 				}
// 			}
// 			return Result;
// 		}

// 		#region Implementation

// 		#endregion
// 	}
// }
