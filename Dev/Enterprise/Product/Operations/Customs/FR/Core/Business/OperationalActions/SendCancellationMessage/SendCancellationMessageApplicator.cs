using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class SendCancellationMessageApplicator : OperationalActionMethodApplicator
	{
		public SendCancellationMessageApplicator(BusinessObjectFactory factory, IApplicatorValidationSupport sendCancellationMessageSupport)
			: base((NoResString)"Send Cancellation Message", factory)
		{
			this.sendCancellationMessageSupport = sendCancellationMessageSupport;
		}

		protected override void BuildCore(ZGuid[] selectItemPKs)
		{
			var targets = new List<BusinessObject>();
			var declarations = Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, selectItemPKs));
			if (declarations is JobDeclaration[] { Length: > 0 })
			{
				targets.AddRange(declarations);
			}
			else
			{
				targets.AddRange(Factory.Load<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.PK, selectItemPKs)));
			}

			CancellationMessage.Targets = targets.ToArray();
			if (sendCancellationMessageSupport != null)
			{
				sendCancellationMessageSupport.IsValid = CancellationMessage.Validation.AreTargetsValid();
			}
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (!sendCancellationMessageSupport.IsValid)
			{
				foreach (var notification in cancellationMessage.GetErrors())
				{
					log.Notify(OperationalActionLogErrorLevel.Error, notification.Message);
				}
			}
			else
			{
				var runner = new SendCancellationMessageOperationalActionRunner(log, Factory);
				runner.SendCancellationMessages(CancellationMessage);
			}
		}

		public SendCancellationMessageDataObject CancellationMessage => cancellationMessage ??= new SendCancellationMessageDataObject();

		SendCancellationMessageDataObject cancellationMessage;
		readonly IApplicatorValidationSupport sendCancellationMessageSupport;
	}
}
