using System;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class DummyOperationalActionFieldValuePair : IOperationalActionFieldValuePair
	{
		public DummyOperationalActionFieldValuePair(OperationalActionFieldSupporter fieldSupporter, IZType value) : this(fieldSupporter, value, null)
		{
		}

		public DummyOperationalActionFieldValuePair(OperationalActionFieldSupporter fieldSupporter, IZType value, IFilterExpression filter)
		{
			this.fieldSupporter = fieldSupporter;
			this.filter = filter ?? new AST.FilterEmpty();
			this.value = value;
		}

		#region IOperationalActionFieldValuePair Members
		public IFilterExpression Filter
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return filter;
			}
		}

		public OperationalActionFieldSupporter Field
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return fieldSupporter;
			}
		}

		public IZType GetValue(Type expectedValue)
		{
			return value;
		}

		#endregion
		readonly IFilterExpression filter;
		readonly OperationalActionFieldSupporter fieldSupporter;
		readonly IZType value;
	}
}
