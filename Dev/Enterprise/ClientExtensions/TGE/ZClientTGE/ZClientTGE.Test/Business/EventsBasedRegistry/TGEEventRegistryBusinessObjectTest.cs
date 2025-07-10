using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.TGE.Business.Testing
{
	[TestedType(typeof(TGEEventRegistryBusinessObject))]
	internal class TGEEventRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<TGEEventRegistryBusinessObject>
	{
		protected override TGEEventRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override TGEEventRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			TGEEventRegistryBusinessObject bizObj = new TGEEventRegistryBusinessObject();
			bizObj.Code = Events.Authorised.Code;
			return bizObj;
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		public void TestProperties()
		{
			TGEEventRegistryBusinessObject bizObj = new TGEEventRegistryBusinessObject();
			bizObj.Code = ZString.Empty;
			AssertEquals("Code should have errors", true, bizObj.CodeInfo.HasErrors());
			bizObj.Code = Events.Authorised.Code;
			AssertEquals("Code should not have errors", false, bizObj.CodeInfo.HasErrors());
			bizObj.Code = "ABC";
			AssertEquals("Code should have errors", true, bizObj.CodeInfo.HasErrors());
		}
	}
}
