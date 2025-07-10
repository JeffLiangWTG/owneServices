using System;
using CargoWise.Types;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class UBMREQMessageLine : UniqueIdentifierMessageLine
	{
		public UBMREQMessageLine(IUnderbondMovementRequestLine requestLine, ZInt numberOfPackages, ZString packageType)
		{
			this.requestLine = requestLine;
			this.numberOfPackages = numberOfPackages;
			this.packageType = packageType;
		}
		readonly ZInt numberOfPackages;
		readonly ZString packageType;

		public override string UniqueIdentifier
		{
			get
			{
				return "CONTAINERNUMBER=" + requestLine.ContainerNumber + "OCEANBILL=" + requestLine.OceanBillOfLading + "HOUSEBILL=" + requestLine.HouseBillOfLading + "MAWB=" + requestLine.MasterAirWaybillNumber + "HAWB=" + requestLine.HouseAirWaybillNumber;
			}
		}

		public override void Populate(SegmentGroup segmentGroup, string lineActionCode)
		{
			SegmentGroup7 group7 = (SegmentGroup7)segmentGroup;
			MessageUtilities.PopulateCNI(group7.CNI[0], null, lineActionCode);
			PopulateReferences(group7);
			SegmentGroup14 group14 = null;
			foreach (SegmentGroup8 group8 in group7.Group8)
			{
				group14 = group8.Group14[0];
				MessageUtilities.PopulateGID(group14.GID[0], "1");
			}
			if (group14 != null)
			{
				PopulatePackingDetails(group14);
			}
		}

		public override SegmentGroup GetNewSegmentGroup(SegmentGroup message)
		{
			return ((CUSCARMessage)message).Group7.InstantiateAChildAndAddItToChildrenCollection();
		}

		#region Implementation

		readonly IUnderbondMovementRequestLine requestLine;

		protected internal override Type SegmentGroupType => typeof(SegmentGroup7);

		#region References

		void PopulateReferences(SegmentGroup7 group7)
		{
			if (!requestLine.HouseAirWaybillNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.HouseWaybillNumber, requestLine.HouseAirWaybillNumber, null);
			}
			if (!requestLine.MasterAirWaybillNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber, requestLine.MasterAirWaybillNumber, null);
			}
			if (!requestLine.ContainerNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, requestLine.ContainerNumber, null);
			}
			if (!requestLine.HouseBillOfLading.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber, requestLine.HouseBillOfLading, null);
			}
			if (!requestLine.OceanBillOfLading.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber, requestLine.OceanBillOfLading, null);
			}
			if (!requestLine.UniqueConsignmentReferenceNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.UniqueConsignmentReferenceNumber, requestLine.UniqueConsignmentReferenceNumber, null);
			}
		}

		#endregion

		#region Packing

		void PopulatePackingDetails(SegmentGroup14 group14)
		{
			if (!numberOfPackages.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), numberOfPackages);
			}
			else if (!requestLine.NumberOfPackages.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), requestLine.NumberOfPackages);
			}

			if (!requestLine.ImportCargoType.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), requestLine.ImportCargoType, CodeListIdentificationCodeList.TypeOfPackage, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}

			if (!packageType.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), packageType, CodeListIdentificationCodeList.ItemType, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			else if (!requestLine.PackageType.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), requestLine.PackageType, CodeListIdentificationCodeList.ItemType, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		#endregion

		#endregion
	}
}
