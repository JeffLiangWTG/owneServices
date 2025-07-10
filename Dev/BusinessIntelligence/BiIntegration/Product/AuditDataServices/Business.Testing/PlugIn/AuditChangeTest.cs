namespace Enterprise.AuditDataServices.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AuditChange))]
	public class AuditChangeTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AuditChange(Factory);
		}

		#endregion
	}
}
