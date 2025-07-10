using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public class ActionInfo : IUniversalActionInfo
	{
		/// <summary>
		/// This should be used for testing only, not a real business case. Don't be lazy, use the other one. 
		/// At some point this should be changed to a static constructor "NewForTesting(RecipientRoleType recipientRole, BusinessObject topLevelBO)".
		/// </summary>
		/// <param name="recipientRole"></param>
		/// <param name="topLevelBO"></param>
		public ActionInfo(RecipientRoleType recipientRole, BusinessObject topLevelBO)
			: this(recipientRole.ToRecipientRoleDetails(), topLevelBO)
		{
		}

		/// <summary>
		/// This is the right constructor to use. The other one is just short form for testing. PS: If you are adding another constructor, you are probably doing it wrong.
		/// </summary>
		/// <param name="recipientRoleDetails"></param>
		/// <param name="topLevelBO"></param>
		public ActionInfo(RecipientRoleDetail[] recipientRoleDetails, BusinessObject topLevelBO)
			: this(recipientRoleDetails, topLevelBO, topLevelBO?.Factory)
		{
		}

		public ActionInfo(RecipientRoleDetail[] recipientRoleDetails, BusinessObject topLevelBO, BusinessObjectFactory factory)
		{
			this.ParentBO = Argument.NotNull(topLevelBO, "topLevelBO");
			FactoryForProcessing = factory;
			this.RecipientRoleDetails = recipientRoleDetails;
			this.TriggerType = TriggerType.Manual;
			this.TriggerEventBranch = Env.CurrentBranch;
			this.TriggerEventDepartment = Env.CurrentDepartment;
			this.TriggerEventUser = Env.CurrentUser;
			this.TriggerActualDate = ZDateTimeOffset.Now;
		}

		public RecipientRoleDetail[] RecipientRoleDetails { get; private set; }
		public BusinessObject ParentBO { get; private set; }
		public BusinessObjectFactory FactoryForProcessing { get; }

		public ZString ActionType { get; set; }
		public ZString PurposeCode { get; set; }

		public ZString TriggerEventCode { get; set; }
		public ZString TriggerReference { get; set; }

		public TriggerType TriggerType { get; set; }
		public ZString TriggerDescription { get; set; }
		public ZDateTimeOffset TriggerScheduledDate { get; set; }
		public ZDateTimeOffset TriggerActualDate { get; set; }
		public ZInt TriggerCount { get; set; }

		public IBranch TriggerEventBranch { get; set; }
		public IDepartment TriggerEventDepartment { get; set; }
		public IUser TriggerEventUser { get; set; }

		public IStmALog TriggeringEvent { get; set; }

		public void PopulateRecipientRoleDetails(ZString recipientTypeCode, ZString recipientServiceCode) => throw new NotImplementedException("If this happens then there is something wrong with the grouping code in WorkflowTriggerActionManager");

		public IOrgHeader RecipientOrganization => null;

		public INotifications Notifications { get; set; }
	}
}
