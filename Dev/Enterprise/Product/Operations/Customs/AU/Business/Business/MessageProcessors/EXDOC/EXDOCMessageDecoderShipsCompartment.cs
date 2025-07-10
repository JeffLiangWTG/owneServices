using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCMessageDecoderShipsCompartment
	{
		public EXDOCMessageDecoderShipsCompartment(SegmentGroup8 group8, BusinessObjectFactory factory)
		{
			this.group8 = group8;
			this.factory = factory;
		}

		public void Process()
		{
			ProcessIMD();
			ProcessDTM();
		}

		void ProcessIMD()
		{
			foreach (IMDSegment iMD in group8.IMD)
			{
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitGrainsAndPlantsHeaderMessageBuilder.Compartment)
				{
					compartmentNumbers = iMD.ItemDescription.ItemDescription1;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitGrainsAndPlantsHeaderMessageBuilder.InspectionPortCode)
				{
					inspectionPortName = iMD.ItemDescription.ItemDescription1;
				}
			}
		}

		void ProcessDTM()
		{
			foreach (DTMSegment dTM in group8.DTM)
			{
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.InspectionDate)
				{
					ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out inspectionDate, "yyyyMMdd");
				}
			}
		}

		public ZString InspectionPort
		{
			get
			{
				if (inspectionPortUnloco.IsEmpty && !inspectionPortName.IsEmpty)
				{
					ZQuery inspectionPortQuery = new ZQuery();
					inspectionPortQuery.AddToFilter(RefUNLOCOSchema.RL_PortName, SQLComparisonOperator.Equal, inspectionPortName);
					RefUNLOCO borderInspectionRefUnloco = factory.LoadTop1<RefUNLOCO>(inspectionPortQuery);
					if (borderInspectionRefUnloco != null)
					{
						inspectionPortUnloco = borderInspectionRefUnloco.RL_Code;
					}
				}
				return inspectionPortUnloco;
			}
		}

		public ZDateTime inspectionDate;
		public ZString compartmentNumbers;
		ZString inspectionPortName;
		ZString inspectionPortUnloco;
		readonly SegmentGroup8 group8;
		readonly BusinessObjectFactory factory;
	}
}
