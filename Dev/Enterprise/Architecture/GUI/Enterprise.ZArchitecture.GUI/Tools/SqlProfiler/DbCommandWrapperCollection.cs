using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Tools
{
	public sealed class DbCommandWrapperCollection : NonPersistentBusinessObjectCollection<DbCommandWrapper>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DbCommandWrapper();
		}
	}
}
