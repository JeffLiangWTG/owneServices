using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class SendCancellationMessageDataObject : AutoSendCancellationMessageDataObject
	{
		public new static class Schema
		{
			public const int MaxEntriesNumber = 99;
		}

		public SendCancellationMessageDataObjectLookups Lookups => new SendCancellationMessageDataObjectLookups(this);

		[List(nameof(Lookups) + "." + nameof(SendCancellationMessageDataObjectLookups.MotivationList))]
		public override ZString InvalidationMotivation
		{
			get => base.InvalidationMotivation;
			set => base.InvalidationMotivation = value;
		}

		public BusinessObject[] Targets
		{
			get
			{
				if (targets == null)
				{
					targets = System.Array.Empty<BusinessObject>();
				}
				return targets;
			}
			set
			{
				targets = value;
			}
		}
		BusinessObject[] targets;

		public virtual int MaxCount()
		{
			return Schema.MaxEntriesNumber;
		}

		protected override SendCancellationMessageDataObjectValidation GetNewValidation() => new SendCancellationMessageDataObjectValidation(this);
	}
}
