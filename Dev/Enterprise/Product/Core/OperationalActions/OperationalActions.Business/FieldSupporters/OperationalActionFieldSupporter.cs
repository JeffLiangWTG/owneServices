using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerDisplay("Field = {Field}")]
	public abstract partial class OperationalActionFieldSupporter : ICodeDescription
	{
		public const int MaxDefaultCaptionLength = 256;
		public const int MaxFieldLength = 256;

		internal OperationalActionFieldSupporter(string field, bool readOnly)
		{
			this.field = field;
			this.readOnly = readOnly;
		}

		public string Field
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return field; }
		}

		public bool ReadOnly
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return readOnly; }
		}

		public FieldDefaultingStrategyList GetDefaultingStrategies()
		{
			List<IFieldDefaultingStrategy> strategies = new List<IFieldDefaultingStrategy>();
			PopulateDefaultingStrategies(strategies);
			return new FieldDefaultingStrategyList(this, strategies);
		}

		public RunnerField NewRunnerField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return NewRunnerFieldCore(factory, descriptor);
		}

		public string AsFilterString(IZType value, BusinessObjectFactory factory)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return AsFilterStringCore(value, factory);
		}

		protected virtual void PopulateDefaultingStrategies(IList<IFieldDefaultingStrategy> strategies)
		{
		}

		protected abstract RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor);

		protected virtual string AsFilterStringCore(IZType value, BusinessObjectFactory factory)
		{
			return value.ToString();
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string field;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly bool readOnly;

		#region ICodeDescription Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string ICodeDescription.Code
		{
			get { return field; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string ICodeDescription.Description
		{
			get { return ""; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		object ICodeDescription.PK
		{
			get { return null; }
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	using System.Collections.Generic;

	public interface IOperationalActionFieldSupporterSelfCheck
	{
		void PerformSelfCheck(BusinessObjectFactory factoryForTesting, List<string> errors);
	}
}

namespace Enterprise.Services.OperationalActions.Business
{
	using System.Collections.Generic;
	using Enterprise.Services.OperationalActions.Business.Testing;

	partial class OperationalActionFieldSupporter : IOperationalActionFieldSupporterSelfCheck
	{
		protected virtual void PerformSelfCheckForTesting(BusinessObjectFactory factoryForTesting, List<string> errors)
		{
			if (Field.Length < 1)
			{
				errors.Add("Field should not be empty.");
			}
			else if (Field.Length > MaxFieldLength)
			{
				errors.Add(string.Format("Field names cannot exceed {0} characters in length", MaxFieldLength));
			}
		}

		#region IOperationalActionFieldSupporterSelfCheck Members

		void IOperationalActionFieldSupporterSelfCheck.PerformSelfCheck(BusinessObjectFactory factoryForTesting, List<string> errors)
		{
			PerformSelfCheckForTesting(factoryForTesting, errors);
		}

		#endregion
	}
}

#endif
#endregion
