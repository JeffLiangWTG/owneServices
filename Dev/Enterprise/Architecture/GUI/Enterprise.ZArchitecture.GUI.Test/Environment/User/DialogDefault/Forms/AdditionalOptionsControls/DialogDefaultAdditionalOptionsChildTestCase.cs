using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.DialogDefault.Testing
{
	public abstract class DialogDefaultAdditionalOptionsChildTestCase : TestCaseWithFactory
	{
		readonly DialogDefaultContext ArbitraryContext = new DialogDefaultContext(
			new ZGuid("AD5A25BC-001B-4200-9B1B-38C6299E2291"),
			(NoResString)"",
			null,
			ZMessageBoxIcon.None,
			new ZGuid("37DE5FAA-412B-43D0-8B04-C93D2073B894"),
			nullContextDescription: Res.GetData("D8EFE9E1-AF42-43AA-BE01-57B320652E38", "Description..."));

		public void TestSaveForAllContextsCheckbox()
		{
			using (var control = GetControl(ArbitraryContext))
			{
				//Would prefer to make DialogDefaultAdditionalOptions abstract but VS Designer complains
				AssertNotNull("SaveForAllContextsCheckbox should be overriden in subclasses.", control.SaveForAllContextsCheckbox);
			}
		}

		public void TestHasMinimumSize()
		{
			using (var control = GetControl(ArbitraryContext))
			{
				Assert("You must set a minimum size", !control.MinimumSize.IsEmpty);
			}
		}

		DialogDefaultAdditionalOptions GetControl(DialogDefaultContext context)
		{
			return (DialogDefaultAdditionalOptions)GetControlCore(context);
		}

		protected abstract Control GetControlCore(DialogDefaultContext context);
	}
}
