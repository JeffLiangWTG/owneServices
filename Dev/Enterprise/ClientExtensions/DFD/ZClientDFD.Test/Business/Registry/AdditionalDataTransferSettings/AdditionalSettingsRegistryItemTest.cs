using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Registry.Testing
{
	[TestedType(typeof(AdditionalSettingsRegistryItem))]
	internal class AdditionalSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<AdditionalSettingsRegistryBusinessObject>
	{
		[TestDate(2006, 1, 1, 1, 1, 50)]
		public override void TestCasting()
		{
			base.TestCasting();
		}

		protected override AdditionalSettingsRegistryBusinessObject ValidValue
		{
			get
			{
				AdditionalSettingsRegistryBusinessObject bizObj = new AdditionalSettingsRegistryBusinessObject();
				bizObj.ExportFileName = "FILENAME";
				bizObj.Directory = TempForTest.TempPath;
				bizObj.Interval = 2;
				bizObj.LastRunDateTime = new DateTime(2006, 1, 1, 1, 1, 50);
				bizObj.NextRunDateTime = new DateTime(2006, 1, 2, 1, 1, 50);
				bizObj.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
				return bizObj;
			}
		}

		protected override StronglyTypedRegistryItem<AdditionalSettingsRegistryBusinessObject, AdditionalSettingsRegistryBusinessObject> GetNewRegistryItem()
		{
			return new AdditionalSettingsRegistryItem("", "", "", "", RegistryStorageFlags.System);
		}
	}
}
