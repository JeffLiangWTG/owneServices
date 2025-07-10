using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentDeliveryDefaultLanguages))]
	public class DocumentDeliveryDefaultLanguagesTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestFallbacksList()
		{
			var testSubject = (DocumentDeliveryDefaultLanguages)GetNewBusinessObject();
			AssertEquals("Contact, Address, Organization, Branch, Company, System", testSubject.Fallbacks.CodesAsString);
		}

		public void TestFallbackValidation()
		{
			var collection = new DocumentDeliveryDefaultLanguagesCollection();
			var entry1 = collection.AddNew();
			entry1.Fallback = Constants.DocumentDeliveryDefaultLanguagesFallbackType.System;
			AssertNoErrors("Prerequisite: should have no errors", entry1.FallbackInfo);

			entry1 = collection.AddNew();
			entry1.RunPreSaveValidation();
			AssertHasError(entry1.FallbackInfo, "Please enter a value.");
			entry1.Fallback = "XYZ";
			AssertHasError(entry1.FallbackInfo, "Enter a valid selection.");
			entry1.Fallback = Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact;
			AssertNoErrors(entry1.FallbackInfo);

			var entry2 = collection.AddNew();
			entry2.Fallback = Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact;
			AssertHasError(entry2.FallbackInfo, "There must be only one 'Contact' line.");
		}

		public void TestOrderValidation()
		{
			var collection = new DocumentDeliveryDefaultLanguagesCollection();
			var entry1 = collection.AddNew();
			entry1.Order = 1;
			AssertNoErrors("Prerequisite: should have no errors", entry1.OrderInfo);

			entry1 = collection.AddNew();
			entry1.RunPreSaveValidation();
			AssertHasError(entry1.OrderInfo, "Please enter a value.");
			entry1.Order = 0;
			AssertHasError(entry1.OrderInfo, "Please enter a value.");
			entry1.Order = 2;
			AssertNoErrors(entry1.OrderInfo);

			var entry2 = collection.AddNew();
			entry2.Order = 2;
			AssertHasError(entry2.OrderInfo, "Order sequence can only be used once.");
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new DocumentDeliveryDefaultLanguages();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentDeliveryDefaultLanguages();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		#endregion
	}
}
