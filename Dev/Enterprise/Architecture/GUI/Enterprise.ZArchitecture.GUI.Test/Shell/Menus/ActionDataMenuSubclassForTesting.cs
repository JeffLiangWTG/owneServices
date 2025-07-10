using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ActionDataMenuSubclassForTesting : ActionDataMenuItem
	{
		public ActionDataMenuSubclassForTesting(IDataBoundControl owner)
			: base(owner)
		{
		}

		void TestClick(object sender, EventArgs e)
		{
		}

		public static new ActionDataMenuItem New(IDataBoundControl owner)
		{
			return new ActionDataMenuSubclassForTesting(owner);
		}

		protected override void AddCustomMenuItems(IBusiness businessEntity)
		{
			base.AddCustomMenuItems(businessEntity);
			MenuItems.Add("test", new EventHandler(TestClick));
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(New);
		}

		public IBusiness BusinessEntityForTesting
		{
			get { return BusinessEntity; }
		}
	}
}
