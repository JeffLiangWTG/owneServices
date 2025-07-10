//using System;
//using Enterprise.ZArchitecture;
//
//
//#if DEBUG
//using NUnit.Framework;
//using Enterprise.ZArchitecture.Business.Testing;
//#endif
//
//namespace Enterprise.Customs.AU.SeaCargo.Business
//{
//	public class CusSCAHouseCollectionForContainer : ManyToManyBusinessObjectCollection
//	{
//		public CusSCAHouseCollectionForContainer(CusSCAContainer AssociatedContainer): base(AssociatedContainer)
//		{
//			Container = AssociatedContainer;
//		}
//
//		public CusSCAHouse this[int Index]
//		{
//			get { return (CusSCAHouse)(Elements[Index]); }
//		}
//
//		public new CusSCAHouse AddNew()
//		{
//			return (CusSCAHouse)base.AddNew();
//		}
//
//		protected override void SetCollectionRelationships(BusinessObject Child)
//		{
//			if (!Container.OceanBill.HouseBills.Contains(Child))
//			{
//				Container.OceanBill.HouseBills.Add((CusSCAHouse)Child);
//			}
//			base.SetCollectionRelationships (Child);
//		}
//
//
//		protected override Type TypeOfRelationshipBusinessObject
//		{
//			get { return typeof(CusSCAPivot); }
//		}
//
//		CusSCAContainer Container;
//
//#if DEBUG
//
//		public class CusSCAHouseCollectionForContainerBOCollectionTest : BusinessObjectCollectionTestCase
//		{
//			public void TestSetCollectionRelationships()
//			{
//				CusSCAContainer TestContainer = OceanBill.Containers.AddNew();
//				CusSCAHouseCollectionForContainer Collection = new CusSCAHouseCollectionForContainer(TestContainer);
//				CusSCAHouse House = Factory.New<CusSCAHouse>();
//				AssertEquals("OceanBill.HouseBills.Contains(House)", false, OceanBill.HouseBills.Contains(House));
//				Collection.SetCollectionRelationships(House);
//				AssertEquals("OceanBill.HouseBills.Contains(House)", true, OceanBill.HouseBills.Contains(House));
//			}
//
//			protected override BusinessObjectCollection GetCollectionToTest()
//			{
//				CusSCAContainer TestContainer = OceanBill.Containers.AddNew();//Factory.New<CusSCAContainer>();
//				return TestContainer.HouseBills;
//			}
//
//			protected override BusinessObject GetNewElementToAddToTheCollection()
//			{
//				return OceanBill.HouseBills.AddNew();
//			}
//
//			CusSCAOceanBill oceanBill;
//			protected CusSCAOceanBill OceanBill
//			{
//				get
//				{
//					if (oceanBill == null)
//					{
//						oceanBill = Factory.New<CusSCAOceanBill>();
//					}
//					return oceanBill;
//				}
//			}
//		}
//#endif
//
//	}
//}
//
