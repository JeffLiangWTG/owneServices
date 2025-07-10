using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(EventRegistryBusinessObject))]
	class EventRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<EventRegistryBusinessObject>
	{
		protected override EventRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override EventRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			EventRegistryBusinessObject bizObj = new EventRegistryBusinessObject();
			bizObj.Code = Events.Authorised.Code;
			bizObj.Reference = "ABC";
			return bizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		public void TestProperties()
		{
			EventRegistryBusinessObject bizObj = new EventRegistryBusinessObject();
			bizObj.Code = ZString.Empty;
			bizObj.Reference = ZString.Empty;

			AssertEquals("Code should have errors", true, bizObj.CodeInfo.HasErrors());
			AssertEquals("Reference should not have errors", false, bizObj.ReferenceInfo.HasErrors());

			bizObj.Code = Events.Authorised.Code;
			AssertEquals("Code should not have errors", false, bizObj.CodeInfo.HasErrors());

			bizObj.Code = "ABC";
			AssertEquals("Code should have errors", true, bizObj.CodeInfo.HasErrors());
		}
	}
}
