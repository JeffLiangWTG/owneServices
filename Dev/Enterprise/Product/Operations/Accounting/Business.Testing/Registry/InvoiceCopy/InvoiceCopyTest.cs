using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceCopy))]
	public class InvoiceCopyTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateName()
		{
			AssertNoErrors("Precondition: Name should not have errors.", BizObj.NameInfo);

			BizObj.IsOriginal = false;

			BizObj.Name = (NoResString)"";
			AssertHasError(BizObj.NameInfo, "Please enter a Name.");

			BizObj.Name = (NoResString)"ABC";
			AssertNoErrors(BizObj.NameInfo);

			BizObj.IsOriginal = true;

			BizObj.Name = (NoResString)"";
			AssertNoErrors(BizObj.NameInfo);

			BizObj.Name = (NoResString)"ABC";
			AssertNoErrors(BizObj.NameInfo);
		}

		public void TestValidateDeliveryMethod()
		{
			AssertNoErrors("Precondition: Delivery Method should not have errors.", BizObj.DeliveryMethodInfo);
			Assert(!string.IsNullOrEmpty(BizObj.DeliveryMethodList[0].Code));

			BizObj.DeliveryMethod = "!@#";
			AssertHasError(BizObj.DeliveryMethodInfo, "Enter a valid selection.");

			BizObj.DeliveryMethod = "";
			AssertHasError(BizObj.DeliveryMethodInfo, "Please enter a Delivery Method.");

			BizObj.DeliveryMethod = BizObj.DeliveryMethodList[0].Code;
			AssertNoErrors(BizObj.DeliveryMethodInfo);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.IsOriginal = false;
			BizObj.Name = (NoResString)"";
			BizObj.DeliveryMethod = "!@#";

			BizObj.ClearAllNotifications();

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.NameInfo);
			AssertHasErrors(BizObj.DeliveryMethodInfo);
		}

		public void TestDeliveryMethodReadOnlyIfIsOriginal()
		{
			BizObj.IsOriginal = true;
			AssertEquals("DeliveryMethodInfo.ReadOnly", true, BizObj.DeliveryMethodInfo.ReadOnly);

			BizObj.IsOriginal = false;
			AssertEquals("DeliveryMethodInfo.ReadOnly", false, BizObj.DeliveryMethodInfo.ReadOnly);
		}

		public void TestCannotDeleteIfIsOriginal()
		{
			BizObj.IsOriginal = true;
			AssertEquals("CanDelete", false, BizObj.CanDelete);

			BizObj.IsOriginal = false;
			AssertEquals("CanDelete", true, BizObj.CanDelete);

			string expectedReason =
				@"This row cannot be deleted because it relates to the existing invoice in the system.
If you would like to use default values, please leave the fields blank.";

			AssertEquals("ReasonForNotAbleToDelete", expectedReason, BizObj.ReasonForNotAbleToDelete);
		}

		public void TestMessage()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Portugal;

			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			nonCurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			Factory.Save();

			var settings = new InvoiceCopyCollection(new FallbackLevel(nonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory) { BizObj };
			var bizObj = settings.AddNew();

			for (int i = 1; i < 6; i++)
			{
				bizObj.Order = i;
				AssertEquals(string.Empty, bizObj.Message);
			}

			nonCurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Portugal;
			for (int i = 1; i < 6; i++)
			{
				bizObj.Order = i;
				AssertEquals(i > 3 ? "Cópia de documento não válida para os fins previstos no regime dos bens em circulação" : string.Empty, bizObj.Message);
			}

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			bizObj.Order = 4;
			AssertEquals("Cópia de documento não válida para os fins previstos no regime dos bens em circulação", bizObj.Message);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			InvoiceCopy result = new InvoiceCopy();

			result.Name = (NoResString)"Name";
			result.DeliveryMethod = "ALL";
			result.IncludeTradingTerms = true;
			result.IsOriginal = true;
			result.Order = 1;
			result.Message = "copy legend";

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("IsOriginal", ((InvoiceCopy)originalBusinessObject).IsOriginal, ((InvoiceCopy)newBusinessObject).IsOriginal);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected new InvoiceCopy BizObj
		{
			get { return (InvoiceCopy)base.BizObj; }
		}

		#endregion
	}
}
