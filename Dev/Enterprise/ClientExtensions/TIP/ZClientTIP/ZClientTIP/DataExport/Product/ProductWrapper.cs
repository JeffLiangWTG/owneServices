using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using ECB = Enterprise.Customs.Business;

namespace Enterprise.Client.TIP
{
	public class ProductWrapper
	{
		public ZString GetProductLine(AUOrgSupplierPart part, bool isExport, ZGuid orgRelationPK, INotifications notify)
		{
			ZString result = ZString.Empty;
			var pivotType = isExport ? ECB.ClassificationTypeList.Codes.HTE : ECB.ClassificationTypeList.Codes.HTI;
			var orgRelationOH = ((MasterFiles.Business.OrgPartRelation)part.RelatedOrganisations.FindByPK(orgRelationPK))?.OU_OH ?? ZGuid.Invalid;
			var pivot = part.PivotsForBinding.OfType<CusClassPartPivot>().FirstOrDefault(x => x.CI_ChildType == pivotType && x.CI_OH.IsEmpty || (orgRelationOH.IsValid && x.CI_OH == orgRelationOH));
			Classification partClassification = pivot?.Classification;

			if (partClassification != null)
			{
				ZString partNumber = part.OP_PartNum;
				ZString lookupCode = partClassification.CC_LookupCode;
				ZString tariffNum = partClassification.CC_TariffNum;
				ZString origin = ZString.Empty;
				ZString preference = ZString.Empty;
				ZString treatment = ZString.Empty;
				ZString instrumentType = ZString.Empty;
				ZString instrumentCode = ZString.Empty;
				ZString dutyRate = ZString.Empty;

				if (!isExport)
				{
					origin = (!pivot.EffectiveAddInfo.ZA_ORG.IsEmpty) ? pivot.EffectiveAddInfo.ZA_ORG : partClassification.AddInfo.ZA_ORG;
					preference = (!pivot.EffectiveAddInfo.ZA_PRF.IsEmpty) ? pivot.EffectiveAddInfo.ZA_PRF : partClassification.AddInfo.ZA_PRF;
					treatment = (!pivot.EffectiveAddInfo.ZA_TreatmentCode_Hidden.IsEmpty) ? pivot.EffectiveAddInfo.ZA_TreatmentCode_Hidden : partClassification.AddInfo.ZA_TreatmentCode_Hidden;
					instrumentType = (!pivot.EffectiveAddInfo.ZA_InstrumentType_Hidden.IsEmpty) ? pivot.EffectiveAddInfo.ZA_InstrumentType_Hidden : partClassification.AddInfo.ZA_InstrumentType_Hidden;
					instrumentCode = (!pivot.EffectiveAddInfo.ZA_InstrumentCode_Hidden.IsEmpty) ? pivot.EffectiveAddInfo.ZA_InstrumentCode_Hidden : partClassification.AddInfo.ZA_InstrumentCode_Hidden;
					dutyRate = pivot.ImportDutyPercentage.ToStringTrimZeros();
				}

				result = partNumber + "," +
					   lookupCode + "," +
					   tariffNum + "," +
					   origin + "," +
					   preference + "," +
					   treatment + "," +
					   instrumentType + "," +
					   instrumentCode + "," +
					   dutyRate;
			}
			else
			{
				notify.Notify(new WarningNotification($"Cannot Export Product '{part.OP_PartNum}'- Classification of type '{pivotType}' is not found."));
			}
			return result;
		}
	}
}
