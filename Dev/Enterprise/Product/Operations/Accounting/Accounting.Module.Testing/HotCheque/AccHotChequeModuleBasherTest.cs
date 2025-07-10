using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccHotChequeModule))]
	public class AccHotChequeModuleBasherTest : ZModuleBasherTest
	{
		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			collection.Add(Factory.NewWithValidTestData<AccHotCheque>(TestBusinessObjectKind.MinimumRequiredToSave));
		}
		public AccHotChequeModuleBasherTest()
			: base()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccHotCheque;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}
	}
}
