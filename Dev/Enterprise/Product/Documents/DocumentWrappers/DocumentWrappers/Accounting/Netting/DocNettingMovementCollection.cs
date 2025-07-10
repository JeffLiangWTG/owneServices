using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingMovementCollection : DocumentWrapperCollection<DocNettingMovement>
	{
		protected DocNettingMovementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocNettingMovementCollection New(BusinessObjectFactory factory)
		{
			return new DocNettingMovementCollection(factory);
		}
	}
}
