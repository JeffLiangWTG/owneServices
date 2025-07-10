using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoForwarderUnderbondApprovalFactory : ExpectedArrivalCusUnderbondFactory
	{
		protected internal ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParentInternal(CMRUBMREQRMessage message, int lineNumber) => LoadOrCreateUnderbondParent(message, lineNumber);
		protected override ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParent(CMRUBMREQRMessage message, int lineNumber)
		{
			ICusUnderbondDependentCollectionParent result = null;
			CusUnderbond[] relatedUnderbonds = (CusUnderbond[])message.Factory.Load(typeof(CusUnderbond), new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, message.UBMSendersReference));
			foreach (CusUnderbond relatedUnderbond in relatedUnderbonds)
			{
				CusSCAContainer containerParent = relatedUnderbond.LinkedObject as CusSCAContainer;
				CusSCAPivot pivotParent = relatedUnderbond.LinkedObject as CusSCAPivot;
				if (containerParent != null && containerParent.CN_ContainerNumber == message.GetContainerNumber(lineNumber))
				{
					result = containerParent;
				}
				else if (pivotParent != null && pivotParent.HouseBill != null && pivotParent.HouseBill.CA_HouseBill == message.GetHouseBillOfLading(lineNumber))
				{
					result = pivotParent;
				}
			}
			return result;
		}

		protected override CusUnderbond CreateOrLoadUnderbond(CMRUBMREQRMessage message, int lineNumber)
		{
			CusUnderbond result = LoadExistingUnderbondByUniqueReference(message, lineNumber);
			result.C4_Status = message.GetStatusCode();
			return result;
		}

		protected internal bool IsInterestedInUBMREQRInternal(CMRUBMREQRMessage message) => IsInterestedInUBMREQR(message);
		protected override bool IsInterestedInUBMREQR(CMRUBMREQRMessage message)
		{
			return message.IsSea && SeaCargoForwarderUnderbondRequestExists(message);
		}

		protected internal bool SeaCargoForwarderUnderbondRequestExists(CMRUBMREQRMessage message)
		{
			return LoadExistingUnderbondByUniqueReference(message, 1) != null;
		}

		protected CusUnderbond LoadExistingUnderbondByUniqueReference(CMRUBMREQRMessage message, int lineNumber)
		{
			CusUnderbond result = null;
			if (message.UnderbondNoticeType == CMRUBMREQRMessage.UnderbondApproval || message.UnderbondNoticeType == CMRUBMREQRMessage.UnderbondApprovalRescind)
			{
				ZQuery subFilter = new ZQuery(CusUnderbondSchema.C4_ParentTableCode, CusSCAContainerSchema.Constants.Prefix);
				subFilter.AddToFilter(JoinCondition.Or, CusUnderbondSchema.C4_ParentTableCode, SQLComparisonOperator.Equal, CusSCAPivotSchema.Constants.Prefix);

				ZQuery filter = new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, message.UBMSendersReference);
				filter.AddToFilter(subFilter);

				CusUnderbond[] results = (CusUnderbond[])message.Factory.Load(typeof(CusUnderbond), filter);
				foreach (CusUnderbond underbond in results)
				{
					CusSCAContainer container = underbond.LinkedObject as CusSCAContainer;
					CusSCAPivot pivot = underbond.LinkedObject as CusSCAPivot;
					if ((container != null && container.CN_ContainerNumber == message.GetContainerNumber(lineNumber))
						|| (pivot != null && pivot.HouseBill != null && pivot.HouseBill.CA_HouseBill == message.GetHouseBillOfLading(lineNumber))
						|| (IsBulkOrBreakBulk(message.GetContainerMode(lineNumber)) && pivot != null && pivot.OceanBill != null && pivot.OceanBill.CB_OceanBill == message.GetOceanBillOfLading(lineNumber)))
					{
						result = underbond;
					}
				}
			}
			return result;
		}

		bool IsBulkOrBreakBulk(ZString code)
		{
			return code == CMRImportCargoTypes.Codes.BreakBulk || code == CMRImportCargoTypes.Codes.Bulk;
		}
	}
}
