using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ShapeAffinityLink))]
	class ShapeAffinityLinkTest : NonPersistentBusinessObjectTestCase
	{
		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "IsApplied";
				yield return "ShapeAffinityPK";
				yield return "ShapePK";
				yield return "TimeApplied";
				yield return "TimeRemoved";
			}
		}
	}
}
