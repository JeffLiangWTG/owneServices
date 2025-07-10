using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Business.Testing;

[TestedType(typeof(AuditChange.AuditChangeWithBinaryValue))]
public class AuditChangeWithBinaryValueTest : NonPersistentBusinessObjectTestCase
{
	#region Implementation

	protected override BusinessObject GetNewBusinessObject()
	{
		return new AuditChange.AuditChangeWithBinaryValue(Factory);
	}

	#endregion
}