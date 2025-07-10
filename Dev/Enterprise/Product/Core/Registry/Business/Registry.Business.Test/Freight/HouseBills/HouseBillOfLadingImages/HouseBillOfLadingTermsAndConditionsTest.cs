using System;
using System.Drawing;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HouseBillOfLadingTermsAndConditions))]
	sealed class HouseBillOfLadingTermsAndConditionsTest : RegistryImageTestCase
	{
		public void TestDeliveryModeListForFWBandSWB()
		{
			BizObj.Code = "BLA";
			AssertContainsExactElementsInAnyOrder(Enum.GetNames(typeof(PrintCopyType)), from ICodeDescription pair in BizObj.DeliveryModeList select pair.Code);

			BizObj.Code = HouseBillOfLadingTermsAndConditions.FWB;
			AssertContainsExactElementsInAnyOrder(new[] { nameof(PrintCopyType.ALL) }, from ICodeDescription pair in BizObj.DeliveryModeList select pair.Code);

			BizObj.Code = "TTC";
			AssertContainsExactElementsInAnyOrder(Enum.GetNames(typeof(PrintCopyType)), from ICodeDescription pair in BizObj.DeliveryModeList select pair.Code);

			BizObj.Code = HouseBillOfLadingTermsAndConditions.SWB;
			AssertContainsExactElementsInAnyOrder(new[] { nameof(PrintCopyType.ALL) }, from ICodeDescription pair in BizObj.DeliveryModeList select pair.Code);
		}

		public void TestDeliveryModeList()
		{
			AssertEquals("Count", 4, BizObj.DeliveryModeList.Count);
			AssertEquals("DeliveryModeList[0].Code", "PRN", BizObj.DeliveryModeList[0].Code);
			AssertEquals("DeliveryModeList[1].Code", "FAX", BizObj.DeliveryModeList[1].Code);
			AssertEquals("DeliveryModeList[2].Code", "EML", BizObj.DeliveryModeList[2].Code);
			AssertEquals("DeliveryModeList[3].Code", "ALL", BizObj.DeliveryModeList[3].Code);
		}

		public override void TestImageIsMandatory()
		{
			AssertEquals("Precondition: There should not be any row errors.", false, BizObj.HasRowErrors);

			BizObj.DeliveryMode = "XYZ";
			BizObj.Image = null;
			BizObj.RunPreSaveValidation();
			AssertEquals("There should not be any row errors.", false, BizObj.HasRowErrors);

			BizObj.DeliveryMode = "ALL";
			BizObj.Image = null;
			BizObj.RunPreSaveValidation();
			AssertEquals("There should be an error if image is null and delivery mode is ALL", true, BizObj.RowErrors.Contains("Please select an image."));

			BizObj.Image = new Bitmap(1, 1);
			BizObj.RunPreSaveValidation();
			AssertEquals("There should not be any row errors.", false, BizObj.HasRowErrors);
		}

		public void TestIsDeliveryModeALL()
		{
			BizObj.DeliveryMode = "AAA";
			AssertEquals("IsDeliveryModeALL", false, BizObj.IsDeliveryModeALL);

			BizObj.DeliveryMode = nameof(PrintCopyType.ALL);
			AssertEquals("IsDeliveryModeALL", true, BizObj.IsDeliveryModeALL);
		}

		public void TestValidateDeliveryMode()
		{
			HouseBillOfLadingTermsAndConditionsCollection collection = new HouseBillOfLadingTermsAndConditionsCollection();
			HouseBillOfLadingTermsAndConditions element1 = collection.AddNew();

			Assert("HouseBillOfLadingTermsAndConditions should not have errors yet", !element1.HasErrors);

			element1.DeliveryMode = "";
			element1.ValidateDeliveryMode();
			Assert("HouseBillOfLadingTermsAndConditions should have an error if DeliveryMode is empty", element1.HasErrors);

			element1.DeliveryMode = "!@$";
			element1.ValidateDeliveryMode();
			Assert("HouseBillOfLadingTermsAndConditions should have an error if DeliveryMode is invalid", element1.HasErrors);

			element1.DeliveryMode = nameof(PrintCopyType.EML);
			element1.ValidateDeliveryMode();
			Assert("HouseBillOfLadingTermsAndConditions should have an error if no rows with the same Code has DeliveryMode of ALL", element1.HasErrors);

			element1.DeliveryMode = nameof(PrintCopyType.ALL);
			element1.ValidateDeliveryMode();
			Assert("HouseBillOfLadingTermsAndConditions should not have an error", !element1.HasErrors);

			HouseBillOfLadingTermsAndConditions element2 = collection.AddNew();

			element2.DeliveryMode = nameof(PrintCopyType.ALL);
			element1.ValidateDeliveryMode();
			element2.ValidateDeliveryMode();
			Assert("HouseBillOfLadingTermsAndConditions should have an error if a duplicate row with the same Code and DeliveryMode was found", element1.HasErrors);
			Assert("HouseBillOfLadingTermsAndConditions should have an error if a duplicate row with the same Code and DeliveryMode was found", element2.HasErrors);

			element2.DeliveryMode = nameof(PrintCopyType.EML);
			element1.ValidateDeliveryMode();
			element2.ValidateDeliveryMode();
			Assert("HouseBillOfLadingTermsAndConditions should not have an error", !element1.HasErrors);
			Assert("HouseBillOfLadingTermsAndConditions should not have an error", !element2.HasErrors);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.DeliveryMode = "!@#";
			BizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: DeliveryMode should not have errors.", BizObj.DeliveryModeInfo);

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.DeliveryModeInfo);
		}

		public void TestHouseBillOfLadingTermsAndConditions_Equals()
		{
			var image1 = (HouseBillOfLadingTermsAndConditions)GetBusinessObjectToClone();
			var image2 = (HouseBillOfLadingTermsAndConditions)GetBusinessObjectToClone();
			Assert(image1.Equals(image2));

			image1.DeliveryMode = "DDD";
			Assert(!image1.Equals(image2));
		}

		public void TestHouseBillOfLadingTermsAndConditions_GetHashCode()
		{
			var image1 = (HouseBillOfLadingTermsAndConditions)GetBusinessObjectToClone();
			var image2 = (HouseBillOfLadingTermsAndConditions)GetBusinessObjectToClone();
			AssertEquals(image1.GetHashCode(), image2.GetHashCode());
		}

		#region Implementation

		protected override bool IsCodeUniqueInCollection
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			HouseBillOfLadingTermsAndConditions result = (HouseBillOfLadingTermsAndConditions)base.GetBusinessObjectToClone();
			result.DeliveryMode = "ALL";
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		new HouseBillOfLadingTermsAndConditions BizObj
		{
			get { return (HouseBillOfLadingTermsAndConditions)base.BizObj; }
		}

		#endregion
	}
}
