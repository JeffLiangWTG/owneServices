using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDTransportLineForSeaAndAir : IMDTransportLine
	{
		public IMDTransportLineForSeaAndAir(IPackingGroup transportLine, CusEntryHeader entryHeader, SegmentGroup21 group21)
			: this(transportLine.Declaration, group21)
		{
			this.transportLine = transportLine;
			this.entryHeader = entryHeader;
		}

		IMDTransportLineForSeaAndAir(JobDeclaration declaration, SegmentGroup21 group21)
			: base(declaration, group21)
		{
		}

		public override void Populate(int lineNumber, string lineActionCode)
		{
			PopulateLIN(lineNumber, lineActionCode);
			PopulateGIS();
			PopulateGroup28();
			PopulateGroup29();
		}

		#region Segment Group 28

		protected override void PopulateGroup28()
		{
			Group28 = group21.Group28.InstantiateAChildAndAddItToChildrenCollection();

			AddCargoTypeToSegment();

			PopulatePAC(transportLine.NumberOfPackages, PackagingLevelCodedList.Inner);
			PopulatePAC(transportLine.WarehouseNumberOfPackages, PackagingLevelCodedList.Intermediate);
			PopulatePAC(transportLine.PackingUnitCount, PackagingLevelCodedList.Outer);
		}

		void AddCargoTypeToSegment()
		{
			if (Declaration != null && Declaration.IsSea)
			{
				ZString cargoType = ZString.Empty;

				if (!transportLine.ContainerMode.IsEmpty)
				{
					if (transportLine.ContainerMode == Core.Constants.ContainerModes.BreakBulk)
					{
						cargoType = CMRCargoTypes.Codes.BreakBulk;
					}
					else
					{
						cargoType = transportLine.ContainerMode;
					}
				}
				else
				{
					cargoType = Declaration.JE_ContainerMode;

					if (Declaration.HasBreakBulk)
					{
						cargoType = CMRCargoTypes.Codes.BreakBulk;
					}
					else if (Declaration.IsLiquid)
					{
						cargoType = Core.Constants.ContainerModes.Bulk;
					}
				}

				if (!cargoType.IsEmpty)
				{
					PopulatePAC(cargoType);
				}
			}
		}

		#endregion

		#region Segment Group 29

		protected override void PopulateGroup29()
		{
			if (Declaration != null)
			{
				ReferenceFunctionCodeQualifierList masterBillCode = Declaration.IsAir ? ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber : ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber;
				ReferenceFunctionCodeQualifierList houseBillCode = Declaration.IsAir ? ReferenceFunctionCodeQualifierList.HouseWaybillNumber : ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber;
				MarkingInstructionsCodedList marksAndNumbersCode = MarkingInstructionsCodedList.MarkFreeText;
				if (entryHeader.Nature == CusEntryHeader.NatureTypesForImportCMR.Nature20)
				{
					marksAndNumbersCode = MarkingInstructionsCodedList.LineItemOnly;
				}

				PopulatePCI(marksAndNumbersCode, transportLine.MarksAndNumbers);
				if (!transportLine.ContainerMode.IsEmpty && transportLine.ContainerMode != Core.Constants.ContainerModes.BreakBulk && transportLine.ContainerMode != Core.Constants.ContainerModes.Bulk)
				{
					PopulateRFF(ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, transportLine.ContainerNumber);
				}

				PopulateRFF(masterBillCode, transportLine.MasterBillNumber);
				PopulateRFF(houseBillCode, transportLine.HouseBillNumber);

				if (Declaration.IsAir)
				{
					var conRefNo = transportLine.ConsignRefNumber;
					if (Declaration.IsConsignmentReferenceActive && !Declaration.JE_PartShipConsignmentReference.IsEmpty)
					{
						conRefNo = Declaration.JE_PartShipConsignmentReference;
					}

					PopulateRFF(ReferenceFunctionCodeQualifierList.GetFromString("CNR"), conRefNo);
				}
			}
		}

		#endregion

		readonly CusEntryHeader entryHeader;
		readonly IPackingGroup transportLine;
	}
}
