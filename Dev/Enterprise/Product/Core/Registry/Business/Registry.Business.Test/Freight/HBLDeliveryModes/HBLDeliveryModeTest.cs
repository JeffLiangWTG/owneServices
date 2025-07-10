using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryMode))]
	sealed class HBLDeliveryModeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestCodeAndDescriptionReadOnly()
		{
			var hblDeliveryMode = new HBLDeliveryMode();
			AssertEquals(true, hblDeliveryMode.CodeInfo.ReadOnly);
			AssertEquals(true, hblDeliveryMode.DescriptionInfo.ReadOnly);
			AssertEquals(true, hblDeliveryMode.EnglishDescriptionInfo.ReadOnly);
			AssertEquals(false, hblDeliveryMode.ShowInListInfo.ReadOnly);
		}

		public void TestValidateShowInList()
		{
			var hblDeliveryModeCollection = new HBLDeliveryModeCollection();
			var hblDeliveryMode1 = hblDeliveryModeCollection.Add("AAA", (NoResString)"AAA DESCRIPTION");
			var hblDeliveryMode2 = hblDeliveryModeCollection.Add("BBB", (NoResString)"BBB DESCRIPTION");

			AssertEquals(true, hblDeliveryMode1.ShowInList);
			AssertEquals(true, hblDeliveryMode2.ShowInList);

			hblDeliveryMode1.ShowInList = ZBool.False;
			hblDeliveryMode1.ValidateShowInList();
			hblDeliveryMode2.ValidateShowInList();
			AssertNoErrors(hblDeliveryMode1.ShowInListInfo);
			AssertNoErrors(hblDeliveryMode2.ShowInListInfo);

			hblDeliveryMode2.ShowInList = ZBool.False;
			hblDeliveryMode2.ValidateShowInList();
			AssertHasError("Should have error.", hblDeliveryMode2.ShowInListInfo, "At least one item must be 'Show In List'.");

			hblDeliveryMode1.ValidateShowInList();
			AssertHasError("Should have error.", hblDeliveryMode1.ShowInListInfo, "At least one item must be 'Show In List'.");
		}

		#region Implementation

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new HBLDeliveryMode();
			result.Code = "XYZ";
			result.Description = (NoResString)"XYZ Organisation";
			result.ShowInList = true;
			result.CodeMaxLength = 9;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
