using System.Windows.Controls;
using CargoWise.Main.Navigation.WPF.Test;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(EntityDetails))]
	[SuppressDataContextCheck]
	class EntityDetailsBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			return new EntityDetails();
		}
	}
}
