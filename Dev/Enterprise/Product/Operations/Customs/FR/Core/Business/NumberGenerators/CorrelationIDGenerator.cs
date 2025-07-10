using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business
{
	public class CorrelationIDGenerator
	{
		public CorrelationIDGenerator(ICorrelationIDProvider parent, IBillGenerationSupport freightValueSource)
		{
			Argument.NotNull(parent, nameof(parent));
			this.parent = parent;
			Argument.NotNull(freightValueSource, nameof(freightValueSource));
			sourceForFreightValues = freightValueSource;
		}

		readonly ICorrelationIDProvider parent;
		readonly IBillGenerationSupport sourceForFreightValues;

		public void InitCorrelationID(Predicate<ZString> correlationIduUniquenessPredicate)
		{
			if (parent.CorrelationID.IsEmpty)
			{
				var newCorrelationID = GetUniqueEntryCorrelationID(correlationIduUniquenessPredicate);
				if (!newCorrelationID.IsEmpty)
				{
					parent.CorrelationID = parent.CorrelationIDPrefix + newCorrelationID;
					parent.CorrelationIDInfo.RefreshBinding();
				}
			}
		}

		public void ClearCorrelationIDOnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !parent.IsInDatabase && !parent.CorrelationID.IsEmpty)
			{
				parent.CorrelationID = ZString.Empty;
			}
		}

		ZString GetUniqueEntryCorrelationID(Predicate<ZString> correlationIduUniquenessPredicate)
		{
			var corel = GetEntryCorrelationID();

			if (correlationIduUniquenessPredicate != null)
			{
				while (!correlationIduUniquenessPredicate(corel))
				{
					corel = GetEntryCorrelationID();
				}
			}

			return corel;
		}

		ZString GetEntryCorrelationID()
		{
			var target = new CorrelationIDNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = parent.Factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.GetTransactionIDSendCounter(),
				FountainGetter = Env.NumberFountains.GetCorrelationIDGeneratorFountain,
				PrimaryTarget = target
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(sourceForFreightValues));
			generator.Generate();
			generator.EnforceMaxLengths();
			return target.Value.ToUpper();
		}
	}
}
