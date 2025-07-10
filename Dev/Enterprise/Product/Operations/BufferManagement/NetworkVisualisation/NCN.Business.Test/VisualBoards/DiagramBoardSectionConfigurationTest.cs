using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(DiagramBoardSectionConfiguration))]
	class DiagramBoardSectionConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		protected override IEnumerable<string> XmlMemberNames
		{
			get { yield return "DiagramPK"; }
		}
	}
}
