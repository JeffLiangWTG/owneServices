using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace CargoWise.Bi.Product.Manager.Business
{
#if DEBUG
	[TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class EdwTableCollection<T> : NonPersistentBusinessObjectCollection<T> where T : EdwTable
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return (T)Activator.CreateInstance(typeof(T), new Object[] { "", "" });
		}
	}
}
