using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using RefCusConditionType = Enterprise.Customs.KR.Messaging.Constants.ZZ.RefCusConditionType;
using RefCusConditionValueType = Enterprise.Customs.KR.Messaging.Constants.ZZ.RefCusConditionValueType;

namespace Enterprise.Customs.KR.Business
{
	public class GAApprovalCollection : CusSupportingInfoCollection<GAApproval>
	{
		public GAApprovalCollection(ILineOrProduct parent)
			: base((BusinessObject)parent, CusSupportingInfoTypeList.Codes.GAApproval)
		{
			this.parent = parent;
		}

		readonly ILineOrProduct parent;

		public void UpdateExportConditions(TariffView universalTariff, ZDateTime effectiveAssessmentDate)
		{
			if (universalTariff != null)
			{
				if (parent.IsExport)
				{
					var conditions = universalTariff.Conditions.Where(x => x.ZX1_IsExport && x.ConditionType == RefCusConditionType.OGA && x.ZX1_StartDate <= effectiveAssessmentDate && x.ZX1_EndDate >= effectiveAssessmentDate);
					var conditionValues = new Dictionary<ZString, ZString>();
					foreach (var condition in conditions)
					{
						var procedure = condition.ConditionValues.FirstOrDefault(x => x.ValueType == RefCusConditionValueType.OGARegulationNumber)?.ZX3_Value ?? ZString.Empty;
						var description = condition.ConditionValues.FirstOrDefault(x => x.ValueType == RefCusConditionValueType.OGADocumentName)?.ZX3_Value ?? ZString.Empty;

						conditionValues.Add(procedure, description);
					}

					var elementsToDelete = new List<GAApproval>(this);

					foreach (var conditionValue in conditionValues)
					{
						var existingValue = this.Cast<GAApproval>().FirstOrDefault(x => x.CSI_Procedure == conditionValue.Key);
						if (existingValue == null)
						{
							var gAApproval = AddNew();
							gAApproval.CSI_Procedure = conditionValue.Key.SubstringSafe(0, gAApproval.CSI_ProcedureInfo.MaxLength);
							gAApproval.CSI_Description = conditionValue.Value;
						}
						else
						{
							elementsToDelete.Remove(existingValue);
						}
					}
					elementsToDelete.ForEach(x => x.Delete());
				}
			}
			else
			{
				RemoveAndDeleteAll();
			}
		}

		public void UpdateExportConditions(CusClassPartPivot pivot)
		{
			if (parent.IsExport)
			{
				if (pivot != null)
				{
					var elementsToDelete = new List<GAApproval>(this);
					foreach (var pivotGAApproval in pivot.GAApprovalDataCollection)
					{
						var existingValue = this.Cast<GAApproval>().FirstOrDefault(x => x.CSI_Procedure == pivotGAApproval.CSI_Procedure);
						if (existingValue == null)
						{
							var gAApproval = AddNew();
							using (gAApproval.GetDefaultValueSuspender())
							{
								gAApproval.CSI_Procedure = pivotGAApproval.CSI_Procedure;
								gAApproval.UpdateGAApprovalData(pivotGAApproval);
							}
						}
						else
						{
							existingValue.UpdateGAApprovalData(pivotGAApproval);
							elementsToDelete.Remove(existingValue);
						}
					}
					elementsToDelete.ForEach(x => x.Delete());
				}
			}
		}
	}
}
