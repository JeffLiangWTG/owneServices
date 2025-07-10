using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business;

public class ManifestMessageSendingNotificationCollector : CustomsNotificationCollector
{
	public ManifestMessageSendingNotificationCollector(ManifestMessageSendingObjectParent parent, IEnumerable<AsycudaBill> selectedBills, bool isManifestHeaderTopLevel = false)
		: base(isManifestHeaderTopLevel ? parent.TopLevelBusinessObject as AsycudaManifestHeader : parent, true, false, PropertyDescriptionType.HumanReadableName)
	{
		this.selectedBills = selectedBills;
		this.parent = Argument.NotNull(parent, nameof(parent));
		this.header = Argument.NotNull(parent.header, nameof(header));
	}

	readonly IEnumerable<AsycudaBill> selectedBills;
	readonly ManifestMessageSendingObjectParent parent;
	readonly AsycudaManifestHeader header;

	protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
	{
		var result = base.ShouldIncludeNotificationsFromObject(businessObject);
		if (result)
		{
			result = businessObject switch
			{
				AsycudaBill => selectedBills.Any(x => x.PK == businessObject.PK) || (businessObject as AsycudaBill).IsChildMasterBill,
				_ => true,
			};
		}
		return result;
	}

	protected override bool ShouldIncludeNotificationsFromInfo(ZPropertyInfo info)
	{
		if (parent.IsSendingNVC01BondedLocationAmendment)
		{
			return ShouldIncludeNotificationsForHeader_NVC01(info) || ShouldIncludeNotificationsForBill_NVC01(info);
		}
		else if (parent.CurrentProcedureCode == JPProcedureCodeList.Codes.HDE)
		{
			return ShouldIncludeNotificationsForHeader_HDE(info);
		}

		return true;
	}

	public bool ShouldIncludeNotificationsForHeader_NVC01(ZPropertyInfo info)
	{
		switch (info.Name)
		{
			case nameof(AsycudaManifestHeader.AMA_MasterBill):
			case nameof(AsycudaManifestHeader.AMA_CustomsOffice):
			case nameof(AsycudaManifestHeader.AMA_GS_NKCustomsAgent):
			case nameof(AsycudaManifestHeader.AMA_CustomsAgentCredentialPK):
				return true;
		}

		return false;
	}

	public bool ShouldIncludeNotificationsForBill_NVC01(ZPropertyInfo info)
	{
		switch (info.Name)
		{
			case nameof(AsycudaBill.ABL_GoodsLocation):
				if (info.BizObj is AsycudaBill && (info.BizObj as AsycudaBill).IsChildMasterBill)
				{
					return true;
				}
				break;
		}

		return false;
	}

	bool ShouldIncludeNotificationsForHeader_HDE(ZPropertyInfo info)
	{
		switch (info.Name)
		{
			case nameof(AsycudaManifestHeader.AMA_MasterBill):
				return true;
		}
		return false;
	}
}
