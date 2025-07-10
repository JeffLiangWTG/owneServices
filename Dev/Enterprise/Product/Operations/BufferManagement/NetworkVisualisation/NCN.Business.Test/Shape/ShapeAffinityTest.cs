using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ShapeAffinity))]
	class ShapeAffinityTest : NonPersistentBusinessObjectTestCase
	{
		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "AffinityPK";
				yield return "AllowedConcurrency";
				yield return "Color";
				yield return "Name";
			}
		}
	}
}
