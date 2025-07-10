using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class GlowDataDefinitionReferenceTest : TestCase
	{
		public void TestGetDataDefinitionNameFromType()
		{
			AssertEquals(null, GlowDataDefinitionReference.FromType(null));

			AssertEquals(null, GlowDataDefinitionReference.FromType(typeof(C1))?.DataDefinitionName);
			AssertEquals("IJobShipment", GlowDataDefinitionReference.FromType(typeof(C2))?.DataDefinitionName);
			AssertEquals("IMyC2Extension", GlowDataDefinitionReference.FromType(typeof(C2SubTypeWithExtension))?.DataDefinitionName);
			AssertEquals("CargoWise.Glow.Model.Interfaces.IJobConsol", GlowDataDefinitionReference.FromType(typeof(C3))?.DataDefinitionName);
			AssertEquals(null, GlowDataDefinitionReference.FromType(typeof(C4))?.DataDefinitionName);
			AssertEquals("IMyOrgHeaderExtension", GlowDataDefinitionReference.FromType(typeof(C5WithBothAttributes))?.DataDefinitionName);
			AssertEquals("IOrgHeader", GlowDataDefinitionReference.FromType(typeof(OrgHeader))?.DataDefinitionName);
			AssertEquals("IMyExtension", GlowDataDefinitionReference.FromType(typeof(MyType))?.DataDefinitionName);
			AssertEquals("IMyJobDeclarationExtension", GlowDataDefinitionReference.FromType(typeof(JobDeclaration))?.DataDefinitionName);
			AssertEquals("IMyJobDeclarationExtension", GlowDataDefinitionReference.FromType(typeof(AUJobDeclaration))?.DataDefinitionName);
			AssertEquals("IMyCAJobDeclarationExtension", GlowDataDefinitionReference.FromType(typeof(CAJobDeclaration))?.DataDefinitionName);
		}

		#region Test classes

		class C1 { }

		[GlowInterfaceReference("IJobShipment")]
		class C2 { }

		[GlowDataDefinition("IMyC2Extension")]
		class C2SubTypeWithExtension : C2 { }

		[GlowInterfaceReference("CargoWise.Glow.Model.Interfaces.IJobConsol")]
		class C3 { }

		[GlowInterfaceReference("INonExistingInterface")]
		class C4 { }

		[GlowInterfaceReference("IOrgHeader")]
		[GlowDataDefinition("IMyOrgHeaderExtension")]
		class C5WithBothAttributes { }

		class AutoOrgHeader { }

		class OrgHeader : AutoOrgHeader { }

		[GlowDataDefinition("IMyExtension")]
		class MyType { }

		class AutoJobDeclaration { }

		[GlowDataDefinition("IMyJobDeclarationExtension")]
		class JobDeclaration : AutoJobDeclaration { }

		class AUJobDeclaration : JobDeclaration { }

		[GlowDataDefinition("IMyCAJobDeclarationExtension")]
		class CAJobDeclaration : JobDeclaration { }

		#endregion
	}
}
