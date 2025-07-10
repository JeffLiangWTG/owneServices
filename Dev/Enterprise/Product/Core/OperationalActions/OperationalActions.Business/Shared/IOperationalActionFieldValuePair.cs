using System;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public interface IOperationalActionFieldValuePair
	{
		OperationalActionFieldSupporter Field { get; }
		IFilterExpression Filter { get; }
		IZType GetValue(Type expectedType);
	}
}
