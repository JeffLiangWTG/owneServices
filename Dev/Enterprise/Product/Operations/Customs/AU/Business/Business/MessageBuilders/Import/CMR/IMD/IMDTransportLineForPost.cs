
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDTransportLineForPost : IMDTransportLine
	{
		public IMDTransportLineForPost(JobDeclaration declaration, SegmentGroup21 group21)
			: base(declaration, group21)
		{
		}

		public override void Populate(int lineNumber, string lineActionCode)
		{
			AggreateValuesOnPackLines();

			if (TotalNumberOfPacks > 0 || TotalWarehousePacks > 0 || TotalPackingUnitCount > 0 || !MarksAndNumbers.IsEmpty)
			{
				PopulateLIN(1, lineActionCode);
				PopulateGIS();
				PopulateGroup28();
				PopulateGroup29();
			}
		}

		#region Aggreate Values On Pack Lines

		void AggreateValuesOnPackLines()
		{
			foreach (PackingGroup currentHouseBillContainerPack in Declaration.PackingGroups)
			{
				TotalNumberOfPacks += currentHouseBillContainerPack.TotalNumberOfPackages;
				TotalWarehousePacks += currentHouseBillContainerPack.WarehouseNumberOfPackages;
				TotalPackingUnitCount += currentHouseBillContainerPack.OuterPackingUnitCount;

				if (!currentHouseBillContainerPack.MarksAndNumbers.IsEmpty)
				{
					MarksAndNumbers += currentHouseBillContainerPack.MarksAndNumbers + " ";
				}
			}
		}

		#endregion

		#region Total Number of Packs

		ZInt TotalNumberOfPacks
		{
			get { return fTotalNumberOfPacks; }
			set { fTotalNumberOfPacks = value; }
		}
		ZInt fTotalNumberOfPacks;

		#endregion

		#region Total Packing Unit Count

		ZInt TotalPackingUnitCount
		{
			get { return fTotalPackingUnitCount; }
			set { fTotalPackingUnitCount = value; }
		}
		ZInt fTotalPackingUnitCount;

		#endregion

		#region Total Warehouse Packs

		ZInt TotalWarehousePacks
		{
			get { return fTotalWarehousePacks; }
			set { fTotalWarehousePacks = value; }
		}
		ZInt fTotalWarehousePacks;

		#endregion

		#region Marks And Numbers

		ZString MarksAndNumbers
		{
			get { return fMarksAndNumbers; }
			set { fMarksAndNumbers = value; }
		}
		ZString fMarksAndNumbers;

		#endregion

		#region Segment Group 28

		protected override void PopulateGroup28()
		{
			Group28 = group21.Group28.InstantiateAChildAndAddItToChildrenCollection();
			PopulatePAC(TotalNumberOfPacks, PackagingLevelCodedList.Inner);
			PopulatePAC(TotalWarehousePacks, PackagingLevelCodedList.Intermediate);
			PopulatePAC(TotalPackingUnitCount, PackagingLevelCodedList.Outer);
		}

		#endregion

		#region Segment Group 29

		protected override void PopulateGroup29()
		{
			PopulatePCI(MarkingInstructionsCodedList.MarkFreeText, MarksAndNumbers);
		}

		#endregion
	}
}
