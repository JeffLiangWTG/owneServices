using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(VisualisationColumnSpecification))]
	public class VisualisationColumnSpecificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Test Visualisation");
			return new VisualisationColumnSpecification(visualisation);
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "Column";
				yield return "ColumnDisplay";
				yield return "Selected";
				yield return "Sequence";
			}
		}
	}
}
