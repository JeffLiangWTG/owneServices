using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test
{
	[TestedType(typeof(StatusSentimentControl))]
	public class StatusSentimentControlTest : RegistryZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new StatusSentimentCollection();
			collection.AddNew();
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly;
		}

		#endregion
	}
}
