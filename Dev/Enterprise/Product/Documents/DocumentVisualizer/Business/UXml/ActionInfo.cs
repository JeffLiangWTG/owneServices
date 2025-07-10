using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class ActionInfo : IUniversalActionInfo
	{
		public ActionInfo(BusinessObject businessObject, string purposeCode)
		{
			Argument.NotNull(businessObject, nameof(businessObject));
			ParentBO = businessObject;
			PurposeCode = purposeCode;
		}

		public BusinessObject ParentBO { get; }

		public ZString ActionType => ZString.Empty;

		public BusinessObjectFactory FactoryForProcessing => ParentBO.Factory;

		public ZString PurposeCode { get; }

		public RecipientRoleDetail[] RecipientRoleDetails => Array.Empty<RecipientRoleDetail>();

		public ZString? RecipientRoleServiceCode => null;

		public ZDateTimeOffset TriggerActualDate => ZDateTimeOffset.Now;

		public ZInt TriggerCount => 1;

		public ZString TriggerDescription => ZString.Empty;

		public ZString TriggerEventCode => ZString.Empty;

		public ZString TriggerReference => ZString.Empty;

		public ZDateTimeOffset TriggerScheduledDate => ZDateTimeOffset.Now;

		public TriggerType TriggerType => TriggerType.Manual;

		public IStmALog TriggeringEvent => null;

		public IOrgHeader RecipientOrganization => null;

		public INotifications Notifications { get; set; }

		public void PopulateRecipientRoleDetails(ZString recipientTypeCode, ZString recipientServiceCode) => throw new NotImplementedException("If this happens then there is something wrong with the grouping code in WorkflowTriggerActionManager");
	}
}
