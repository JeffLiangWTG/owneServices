using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class FrDeclarationSendValideeMessageApplicator : OperationalActionMethodApplicator
	{
		public FrDeclarationSendValideeMessageApplicator() : base((NoResString)"FR Send Validated")
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new SendValideeMessageOperationalActionRunner(log, targets);
			runner.SendValidee(SendMessagesEvenWithMessageErrors);
		}

		[ReadOnlyMember(nameof(SendMessagesEvenWithMessageErrors_ReadOnly))]
		[ResourceStringData("FrDeclarationSendValideeMessageApplicator.SendMessagesEvenWithMessageErrors", Caption = "Send messages even with message errors")]
		public ZBool SendMessagesEvenWithMessageErrors
		{
			get => sendMessagesEvenWithMessageErrors;
			set
			{
				SetNonPersistentPropertyValue(SendMessagesEvenWithMessageErrorsInfo, ref sendMessagesEvenWithMessageErrors, value);
			}
		}

		ZBool sendMessagesEvenWithMessageErrors;

		public ZPropertyInfo SendMessagesEvenWithMessageErrorsInfo => GetZPropertyInfo(nameof(SendMessagesEvenWithMessageErrors));

		protected bool SendMessagesEvenWithMessageErrors_ReadOnly => !Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
	}
}
