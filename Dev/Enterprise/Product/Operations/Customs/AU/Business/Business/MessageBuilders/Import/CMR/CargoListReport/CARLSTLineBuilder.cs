using System;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CARLSTLineBuilder : UniqueIdentifierMessageLine
	{
		public CARLSTLineBuilder(ICargoListReportLine reportLine)
		{
			this.reportLine = reportLine;
		}

		public override SegmentGroup GetNewSegmentGroup(SegmentGroup message)
		{
			CUSCARMessage cUSCAR = message as CUSCARMessage;
			return cUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection();
		}

		protected internal override Type SegmentGroupType
		{
			get { return typeof(SegmentGroup7); }
		}

		public override string UniqueIdentifier
		{
			get
			{
				return "CARGOTYPE=" + reportLine.ImportCargoType + "CARGOCODE=" + reportLine.CargoCode + "CARGOID=" + reportLine.CargoIdentifier;
			}
		}

		public override void Populate(SegmentGroup segmentGroup, string lineActionCode)
		{
			SegmentGroup7 group7 = segmentGroup as SegmentGroup7;
			MessageUtilities.PopulateCNI(group7.CNI[0], null, lineActionCode);
			if (!reportLine.CargoCode.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.ShippingUnitIdentification, reportLine.CargoCode, null);
			}
			if (!reportLine.CargoCode.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.PackagingUnitIdentification, reportLine.CargoIdentifier, null);
			}
			if (group7.Group8.Count > 0)
			{
				SegmentGroup8 group8 = group7.Group8[group7.Group8.Count - 1];
				if (!reportLine.PortOfDestination.IsEmpty)
				{
					MessageUtilities.PopulateLOC(group8.LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PlaceOfDestination, reportLine.PortOfDestination, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
				if (!reportLine.PortOfLoading.IsEmpty)
				{
					MessageUtilities.PopulateLOC(group8.LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.OriginalPortOfLoading, reportLine.PortOfLoading, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}

				if (!reportLine.ImportCargoType.IsEmpty)
				{
					MessageUtilities.PopulatePAC(group8.Group14[0].PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.ImportCargoType, CodeListIdentificationCodeList.TypeOfPackage, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
				}
				MessageUtilities.PopulatePAC(group8.Group14[0].PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.NumberOfPackages);
				if (!reportLine.PackageType.IsEmpty)
				{
					MessageUtilities.PopulatePAC(group8.Group14[0].PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.PackageType, CodeListIdentificationCodeList.ItemType, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
				}
			}
			foreach (SegmentGroup8 group8 in group7.Group8)
			{
				MessageUtilities.PopulateGID(group8.Group14[0].GID[0], "1");
			}
		}

		readonly ICargoListReportLine reportLine;
	}
}
