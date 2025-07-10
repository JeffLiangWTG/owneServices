using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;

namespace Enterprise.Services.OperationalActions.Business
{
	interface IOperationalActionMenuEditable : IMenuEditable
	{
		bool ReadOnly { get; set; }
		ZPropertyInfo SystemDefinedInfo { get; }
	}
}
