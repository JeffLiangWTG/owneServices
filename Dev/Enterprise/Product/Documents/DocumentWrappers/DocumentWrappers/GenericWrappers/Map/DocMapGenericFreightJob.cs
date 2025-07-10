using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Map;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class DocMapGenericFreightJob
	{
		/// <summary>
		/// This is a place holder with a static constructor so that the DocumentWrapperFactory can handle the DataContext 
		/// "MapGenericFreightJob" without any other "Speshel" coding.
		/// </summary>
		/// <param name="parentBO">Should be the parent NonPersistentBO from the "Customise Documents" form.</param>
		/// <param name="factory">Self explanatory. I hope.</param>
		/// <returns>Returns a wrapper used for displaing a full schema from the FreightJobWrapper down.</returns>
		public static SchemaWrapper New(MenuCustomisation parentBO, BusinessObjectFactory factory)
		{
			return new SchemaWrapper(typeof(FreightWrapper), Res.GetString("0f21a653-0b11-437e-8e9c-56825d6c1449", "Freight Job Wrapper"), factory);
		}
	}
}
