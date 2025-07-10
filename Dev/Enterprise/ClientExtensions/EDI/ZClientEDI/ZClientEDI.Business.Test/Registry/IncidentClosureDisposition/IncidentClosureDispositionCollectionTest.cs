using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(IncidentClosureDispositionCollection))]
	public class IncidentClosureDispositionCollectionTest : CodeDescriptionBoolTreeNodeCollectionTest<IncidentClosureDispositionCollection>
	{
		#region Implementation

		protected override IncidentClosureDispositionCollection GetCollectionToTest()
		{
			var result = new IncidentClosureDispositionCollection();
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IncidentClosureDisposition();
		}

		#endregion
	}
}
