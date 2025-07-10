using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class GlowInterfaceReferenceAttributeTest : TestCase
	{
		public void TestGetGlowInterface()
		{
			AssertEquals(null, GlowInterfaceReferenceAttribute.GetGlowInterface(null));
			AssertEquals(null, GlowInterfaceReferenceAttribute.GetGlowInterface(string.Empty));
			AssertEquals(null, GlowInterfaceReferenceAttribute.GetGlowInterface("ISomethingUnknown"));
			AssertEquals("CargoWise.Glow.Model.Interfaces.IJobDeclaration", GlowInterfaceReferenceAttribute.GetGlowInterface("IJobDeclaration")?.FullName);
			AssertEquals("CargoWise.Glow.Model.Interfaces.IJobDeclaration", GlowInterfaceReferenceAttribute.GetGlowInterface("CargoWise.Glow.Model.Interfaces.IJobDeclaration")?.FullName);
		}

		public void TestGetGlowInterfaceFromType()
		{
			AssertNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(C1)));
			AssertEquals("IJobShipment", GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(C2))?.Name);
			AssertEquals("IJobShipment", GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(C2SubTypeWithExtension))?.Name);
			AssertEquals("IJobConsol", GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(C3))?.Name);
			AssertNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(C4)));
			AssertEquals("IOrgHeader", GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(C5WithBothAttributes))?.Name);
			AssertEquals("IOrgHeader", GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(OrgHeader), true)?.Name);
			AssertNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(OrgHeader)));
			AssertNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(null));

			AssertNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(MyType), tryToFindOutIfNotSpecified: false));
			AssertNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(MyType), tryToFindOutIfNotSpecified: true));
			AssertNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(JobDeclaration), tryToFindOutIfNotSpecified: false));
			AssertEquals("IJobDeclaration", GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(JobDeclaration), tryToFindOutIfNotSpecified: true)?.Name);
			AssertNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(AUJobDeclaration), tryToFindOutIfNotSpecified: false));
			AssertEquals("IJobDeclaration", GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(AUJobDeclaration), tryToFindOutIfNotSpecified: true)?.Name);
			AssertNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(CAJobDeclaration), tryToFindOutIfNotSpecified: false));
			AssertEquals("IJobDeclaration", GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(CAJobDeclaration), tryToFindOutIfNotSpecified: true)?.Name);
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
