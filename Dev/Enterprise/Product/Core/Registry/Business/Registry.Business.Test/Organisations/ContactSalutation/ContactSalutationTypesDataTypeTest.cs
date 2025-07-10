using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ContactSalutationTypesDataType))]
	sealed class ContactSalutationTypesDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ContactSalutationTypesDataType>
	{
		protected override ContactSalutationTypesDataType GetNewDataType()
		{
			return new ContactSalutationTypesDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ContactSalutationRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			ContactSalutation lhsContactSalutation = (ContactSalutation)lhs;
			ContactSalutation rhsContactSalutation = (ContactSalutation)rhs;

			AssertEquals("ContactSalutation.Salutation", lhsContactSalutation.Salutation, rhsContactSalutation.Salutation);
			AssertEquals("ContactSalutation.Gender", lhsContactSalutation.Gender, rhsContactSalutation.Gender);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ContactSalutationCollection collection = new ContactSalutationCollection();

			ContactSalutation contactSalutation1 = collection.AddNew();
			contactSalutation1.EnglishSalutation = "Dear";
			contactSalutation1.Gender = "Female";

			var byteArrayValue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfContactSalutation xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ContactSalutation><Salutation>Dear</Salutation><Gender>Female</Gender></ContactSalutation></ArrayOfContactSalutation>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		public void TestSerialiseAndDeSerialise()
		{
			ContactSalutationTypesDataType dataType = new ContactSalutationTypesDataType();
			ContactSalutationCollection collection = new ContactSalutationCollection();
			SalutationHelper.LoadDefaultSalutations(collection);
			byte[] value = dataType.Serialise(collection);

			ContactSalutationCollection deserializedValue = dataType.Deserialise(value);
			AssertEquals(collection.Count, deserializedValue.Count);

			for (int i = 0; i < collection.Count; i++)
			{
				AssertEquals(collection[i].Salutation, deserializedValue[i].Salutation);
				AssertEquals(collection[i].Gender, deserializedValue[i].Gender);
			}
		}

		public void TestDeserialiseWithDuplicateSalutation()
		{
			var collection = OrganisationsDataRegistry.Instance.ContactSalutation.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			List<string> keys = new List<string>();
			foreach (ContactSalutation item in collection)
			{
				var resString = item.RawSalutation as ResourceString;
				AssertNotNull(resString);
				if (keys.Contains(resString.ResourceKey))
				{
					Assert("Converted salutation should not reference to the same ResString.", false);
				}
				keys.Add(resString.ResourceKey);
			}
		}

		public void TestTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				string key = ((ResourceString)Constants.DefaultSalutations.ToWhomItMayConcern).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "敬启者"));

				var registryItem = OrganisationsDataRegistry.Instance.ContactSalutation;
				var toWhomItMayConcern = (ContactSalutation)registryItem.Value.First(item => ((ContactSalutation)item).EnglishSalutation == Constants.DefaultSalutations.ToWhomItMayConcern.GetUnresolvedString());
				AssertEquals("敬启者", toWhomItMayConcern.Salutation.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var newValue = new ContactSalutationCollection();
				var newItem = newValue.AddNew();
				newItem.EnglishSalutation = "Wassup";
				newItem.Gender = Constants.SalutationGenders.All;
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
				key = ((ResourceString)registryItem.Value[0].RawSalutation).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "废话"));
				AssertEquals("废话", registryItem.Value[0].Salutation.ToString(Core.SharedConstants.Languages.ChineseSimplified));
			}
		}

		public void TestGenderAnnotations()
		{
			var registryItem = OrganisationsDataRegistry.Instance.ContactSalutation;
			var captionSource = registryItem as IRegistryItemCaptionSource;
			var collection = new ContactSalutationCollection();
			var item = collection.AddNew();
			item.Gender = Constants.SalutationGenders.Man;
			item.Salutation = Constants.DefaultSalutations.DearMale;

			AssertEquals("Dear", item.Salutation);
			AssertEquals("Dear", item.EnglishSalutation);
			AssertEquals("[m] Dear", item.AnnotatedEnglishSalutation);
			AssertEquals("[m] Dear", item.RawSalutation);
			AssertContainsExactElementsInAnyOrder(new string[] { "[m] Dear" }, captionSource.GetCaptions(collection));
			AssertEquals(((ResourceString)Constants.DefaultSalutations.DearMale).ResourceKey, CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "[m] Dear").ResourceKey);
		}

		public void TestGetRunTimeCaptions()
		{
			var captionSource = new TranslatableRegistryItemValueCaptionSource(OrganisationsDataRegistry.Instance.ContactSalutation, OrganisationsDataRegistry.Instance.ContactSalutation.Value);
			var expected = string.Join(";", OrganisationsDataRegistry.Instance.ContactSalutation.Value.OfType<ContactSalutation>().OrderBy(a => a.RawSalutation).ToArray().Select(a => a.RawSalutation.ToString(Res.DefaultLanguage)));
			var actual = string.Join(";", captionSource.GetRuntimeCaptions().OrderBy(a => a.ToString()).Select(a => a.ToString()).ToArray());

			var expectedStringWithParameters = string.Join(";", OrganisationsDataRegistry.Instance.ContactSalutation.Value.OfType<ContactSalutation>().Where(a => a.RawSalutation is ResourceString).OrderBy(a => ((ResourceString)a.RawSalutation).ToStringWithParameters()).Select(a => ((ResourceString)a.RawSalutation).ToStringWithParameters()).ToArray());
			var actualStringWithParameters = string.Join(";", captionSource.GetRuntimeCaptions().OrderBy(a => a.EnglishText).Select(a => a.EnglishText).ToArray());

			AssertEquals("Runtime captions should match the registry settings.", expected, actual);
			AssertEquals("Runtime captions should match the registry settings.", expectedStringWithParameters, actualStringWithParameters);
		}
	}
}
