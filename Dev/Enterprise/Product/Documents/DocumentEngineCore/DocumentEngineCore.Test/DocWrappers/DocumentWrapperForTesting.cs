using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	/// <summary>
	/// This Simple First Concrete is used in other solutions as well. 
	/// Please don't move it inside the TestCase above unless you're 
	/// prepared to do a full build and fix.
	/// </summary>
	public class DocumentWrapperForTesting : DocumentWrapper
	{
		public DocumentWrapperForTesting()
			: base()
		{
		}

		public DocumentWrapperForTesting(object objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public override string ToString()
		{
			return "Test";
		}
	}
}
