using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed partial class OperationalActionCodeFieldSupporter : OperationalActionTextFieldSupporter
	{
		public OperationalActionCodeFieldSupporter(string field, bool readOnly, ReadOnlyCodeDescriptionPairList list)
			: base(field, readOnly, list.MaxCodeLength)
		{
			this.list = list;
		}

		public ReadOnlyCodeDescriptionPairList List
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return list; }
		}

		protected override void PopulateDefaultingStrategies(IList<IFieldDefaultingStrategy> strategies)
		{
			strategies.Add(new FixedCodeFieldDefaultingStrategy(this));
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerCodeField(factory, descriptor, this);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ReadOnlyCodeDescriptionPairList list;
	}
}

#region Test
#if DEBUG

#region Self Checking

namespace Enterprise.Services.OperationalActions.Business
{
	using System.Collections.Generic;

	partial class OperationalActionCodeFieldSupporter
	{
		protected override void PerformSelfCheckForTesting(BusinessObjectFactory factoryForTesting, List<string> errors)
		{
			base.PerformSelfCheckForTesting(factoryForTesting, errors);

			if (List.Count < 2)
			{
				errors.Add("A list of fiewer than 2 options is not meaningful");
			}
		}
	}
}

#endregion

#endif
#endregion
