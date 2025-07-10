using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business
{
	public partial class OperationalActionTextFieldSupporter : OperationalActionFieldSupporter
	{
		public OperationalActionTextFieldSupporter(string field, bool readOnly, int maxLength)
			: base(field, readOnly)
		{
			this.maxLength = maxLength;
		}

		public int MaxLength
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return maxLength; }
		}

		protected override void PopulateDefaultingStrategies(IList<IFieldDefaultingStrategy> strategies)
		{
			strategies.Add(new FixedTextFieldDefaultingStrategy(this));
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerTextField(factory, descriptor, this);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly int maxLength;
	}
}

#region Test
#if DEBUG

#region Self Checking

namespace Enterprise.Services.OperationalActions.Business
{
	partial class OperationalActionTextFieldSupporter
	{
		protected override void PerformSelfCheckForTesting(BusinessObjectFactory factoryForTesting, List<string> errors)
		{
			base.PerformSelfCheckForTesting(factoryForTesting, errors);

			if (MaxLength == 0)
			{
				errors.Add("MaxLength should be >0.");
			}
		}
	}
}

#endregion

#endif
#endregion
