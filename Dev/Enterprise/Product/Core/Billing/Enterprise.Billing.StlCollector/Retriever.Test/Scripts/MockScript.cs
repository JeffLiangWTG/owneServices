using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	public class MockScript : IStlScript
	{
		public virtual string ScriptText => throw new NotImplementedException();
		public virtual int TimeoutSecs => throw new NotImplementedException();
		public virtual string Company => throw new NotImplementedException();
		public virtual string Branch => throw new NotImplementedException();
		public virtual string TransactionDateUtc => throw new NotImplementedException();
		public virtual string Reference1 => throw new NotImplementedException();
		public virtual string Reference2 => throw new NotImplementedException();
		public virtual string Reference3 => throw new NotImplementedException();
		public virtual string Reference4 => throw new NotImplementedException();
		public virtual string GuidReference => throw new NotImplementedException();
		public virtual string AdditionalRefs => throw new NotImplementedException();
		public virtual string User => throw new NotImplementedException();
		public virtual string ActiveOn => throw new NotImplementedException();
		public virtual string Preparation => throw new NotImplementedException();
		public virtual string From => throw new NotImplementedException();
		public virtual string Where => throw new NotImplementedException();
		public virtual string BillableCount => throw new NotImplementedException();
		public virtual bool WithRecompile => throw new NotImplementedException();
		public virtual string Code => throw new NotImplementedException();
		public virtual string Role => throw new NotImplementedException();
		public virtual string Module => throw new NotImplementedException();
		public virtual string Function => throw new NotImplementedException();
		public virtual string Feature => throw new NotImplementedException();
		public virtual StlDataGrain StlGrain => throw new NotImplementedException();
		public virtual bool IsSystemLevel => throw new NotImplementedException();
		public virtual bool IsMandatoryForMilestones { get; set; }
		public virtual bool IsActive => throw new NotImplementedException();
		public virtual Exception CollectionException { get; set; }
		public virtual bool CollectionOccurred { get; set; }
		public virtual StlDateType DateType => throw new NotImplementedException();
		public virtual StlCollectorType CollectorType { get; set; }
		public virtual string Name => throw new NotImplementedException();
		public virtual string MinCW1Version => throw new NotImplementedException();
		public virtual string MaxCW1Version => throw new NotImplementedException();
		public virtual int AdditionalRefsEncoding => 65001;
		public virtual DateTime CollectionStartDateUtc => DateTime.MinValue;

		public virtual IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange)
		{
			throw new NotImplementedException();
		}
	}
}
