using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business;

public class ECRMessageSendingNotificationCollector(DeclarationMessageSendingObjectParent parent, IEnumerable<CusEntryHeader> selectedEntries, bool isDeclarationTopLevel = false)
	: CustomsNotificationCollector(isDeclarationTopLevel ? parent.ParentDeclaration : parent, true, false, PropertyDescriptionType.HumanReadableName)
{
	protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
	{
		return businessObject switch
		{
			DeclarationMessageSendingObjectParent => businessObject.PK == parent.PK,
			MessageSendingObject sendingObject => selectedEntries.Any(x => x.PK == sendingObject.Header.PK),
			CusEntryInstruction => selectedEntries.Any(x => x.CH_CEI_Instruction == businessObject.PK),
			JobDeclaration => selectedEntries.Any(x => x.CH_JE == businessObject.PK),
			MoveInDestination moveInDestination => selectedEntries.Any(x => x.EntryInstruction.PK == moveInDestination.Parent.PK),
			InventoryInfo inventoryInfo => selectedEntries.Any(x => x.EntryInstruction.PK == inventoryInfo.Parent.PK),
			_ => false
		};
	}

	protected override bool ShouldIncludeNotificationsFromInfo(ZPropertyInfo info)
	{
		return info.BizObj switch
		{
			JobDeclaration when !propertyInfoToIncludeOnDeclration.Contains(info.Name) => false,
			CusEntryInstruction when !propertyInfoToIncludeOnEntryInstruction.Contains(info.Name) => false,
			_ => true
		};
	}

	readonly DeclarationMessageSendingObjectParent parent = parent;

	readonly IEnumerable<CusEntryHeader> selectedEntries = selectedEntries;

	readonly ImmutableHashSet<string> propertyInfoToIncludeOnDeclration = new[]
	{
		nameof(JobDeclaration.JE_ReceiptMode),
		nameof(JobDeclaration.JE_RL_NKFinalDestination),
	}.ToImmutableHashSet();

	readonly ImmutableHashSet<string> propertyInfoToIncludeOnEntryInstruction = new[]
	{
		nameof(CusEntryInstruction.CEI_GoodsDescription),
		nameof(CusEntryInstruction.CEI_GrossWeight),
		nameof(CusEntryInstruction.CEI_CargoQuantity),
		nameof(CusEntryInstruction.CEI_Volume),
	}.ToImmutableHashSet();
}
