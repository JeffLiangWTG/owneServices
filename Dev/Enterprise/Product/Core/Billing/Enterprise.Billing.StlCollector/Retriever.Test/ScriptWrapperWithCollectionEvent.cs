using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class ScriptWrapperWithCollectionEvent : MockScript
	{
		public IStlItem InnerScript { get; private set; }

		public ScriptWrapperWithCollectionEvent(IStlItem innerScript)
		{
			InnerScript = innerScript;
		}

		public event CollectionOccurredDelegate CollectionOccurredEvent;

		public override string Code => InnerScript.Code;
		public override string Role => InnerScript.Role;
		public override string Module => InnerScript.Module;
		public override string Function => InnerScript.Function;
		public override string Feature => InnerScript.Feature;
		public override StlDataGrain StlGrain => InnerScript.StlGrain;
		public override bool IsSystemLevel => InnerScript.IsSystemLevel;
		public override bool IsMandatoryForMilestones => InnerScript.IsMandatoryForMilestones;
		public override bool IsActive => InnerScript.IsActive;
		public override Exception CollectionException { get => InnerScript.CollectionException; set => InnerScript.CollectionException = value; }
		public override StlDateType DateType { get => InnerScript.DateType; }
		public override StlCollectorType CollectorType => InnerScript.CollectorType;
		public override DateTime CollectionStartDateUtc => InnerScript.CollectionStartDateUtc;

		public override bool CollectionOccurred
		{
			get
			{
				return InnerScript.CollectionOccurred;
			}
			set
			{
				InnerScript.CollectionOccurred = value;
				if (value)
				{
					CollectionOccurredEvent?.Invoke(this);
				}
			}
		}

		public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange) => InnerScript.GetInputParameters(dateTimeRange);
		public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) => InnerScript.Run(dateTimeRange);
	}
}
