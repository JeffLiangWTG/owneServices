using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIGlbStaffWorkflowDescriptor : GlbStaffWorkflowDescriptor
	{
		#region ID / Description / Type

		public override Type WorkflowProviderType
		{
			get { return typeof(EDIGlbStaff); }
		}

		#endregion

		protected override bool SupportsWtaEnrolmentTriggerActionCore => true;

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			return new CodeDescriptionPairList { new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.EnrolInWiseTechAcademyCourse, WorkflowTriggerActionTypeConstants.Descriptions.EnrolInWiseTechAcademyCourse) };
		}

		protected override GlbStaff GetStaffForWtaEnrolment(BusinessObject parent)
		{
			return (GlbStaff)parent;
		}

		protected override WorkflowTriggerNotification GetWorkflowTriggerForNotificationEmail(Lazy<MessageProcessorCommunicationModesResult> modes, ProcessTaskNotification action, BusinessObject parent, Lazy<IStmALog> logProvider)
		{
			var workflowTriggerNotification = new WorkflowTriggerNotification(modes, action, parent, logProvider);

			var contactForNotification = GetContactForNotification(action, (EDIGlbStaff)parent);
			if (contactForNotification != null)
			{
				var passwordInstructionToken = PasswordInstructionUrlStrategy.GeneratePasswordInstructionToken(contactForNotification, contactForNotification.GetPasswordInstructionType(), passwordResetInfo: null, time: null);
				workflowTriggerNotification.ExtraPostMacroDataSubstitution = (arg1, arg2, data) => data.Replace(OrgContact.TokenMacro, passwordInstructionToken);
			}

			return workflowTriggerNotification;
		}

		OrgContact GetContactForNotification(ProcessTaskNotification action, EDIGlbStaff staff)
		{
			if (!action.PQ_EmailTextFallbackToTemplate.Contains(nameof(OrgContact.PasswordInstructionMacroUrl), StringComparison.Ordinal))
			{
				return null;
			}

			var factory = staff.Factory;
			var database = StaffContactImporter.FindProductRegistrationDatabase(factory);

			if (database == null)
			{
				return null;
			}

			var ediUserAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, database.PK).AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, staff.GS_Code);
			var userAccount = factory.LoadTop1<EdiCustomerUserAccount>(ediUserAccountQuery);
			return userAccount?.WebAccessContact;
		}
	}
}
