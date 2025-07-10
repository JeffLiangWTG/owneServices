using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(MultipleEmailTemplatesRegistryItem))]
	class MultipleEmailTemplatesRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionEmailTemplateCollection, CodeDescriptionEmailTemplateCollection>
	{
		public void TestDefaultValue()
		{
			CodeDescriptionEmailTemplateCollection collection = new CodeDescriptionEmailTemplateCollection(typeof(DocSupportIncident));
			CodeDescriptionEmailTemplate template1 = collection.AddNew();
			template1.Code = "AAA";
			template1.Description = (NoResString)"Template One";
			template1.EmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject one", "Email body template one");
			CodeDescriptionEmailTemplate template2 = collection.AddNew();
			template2.Code = "BBB";
			template2.Description = (NoResString)"Template Two";
			template2.EmailTemplate = new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject two", "Email body template two");

			MultipleEmailTemplatesRegistryItem item = new MultipleEmailTemplatesRegistryItem("", null, null, null, RegistryStorageFlags.System, typeof(DocSupportIncident), collection);
			AssertEquals(true, item.DefaultValue.ContainsCode("AAA"));
			AssertEquals(true, item.DefaultValue.ContainsCode("BBB"));
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionEmailTemplateCollection, CodeDescriptionEmailTemplateCollection> GetNewRegistryItem()
		{
			return new MultipleEmailTemplatesRegistryItem("", null, null, null, RegistryStorageFlags.System, typeof(DocSupportIncident));
		}
	}

	[TestedType(typeof(MultipleEmailTemplatesRegistryDataType))]
	class MultipleEmailTemplatesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MultipleEmailTemplatesRegistryDataType>
	{
		#region Implementation

		protected override MultipleEmailTemplatesRegistryDataType GetNewDataType()
		{
			return new MultipleEmailTemplatesRegistryDataType(typeof(DocSupportIncident));
		}

		protected override string ExpectedEditorName
		{
			get { return "MultipleEmailTemplatesRegistryEditor"; }
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			CodeDescriptionEmailTemplate lhsParent = (CodeDescriptionEmailTemplate)lhs;
			CodeDescriptionEmailTemplate rhsParent = (CodeDescriptionEmailTemplate)rhs;

			AssertEquals(lhsParent.EmailTemplate.EmailSubject, rhsParent.EmailTemplate.EmailSubject);
			AssertEquals(lhsParent.EmailTemplate.EmailBody, rhsParent.EmailTemplate.EmailBody);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CodeDescriptionEmailTemplateCollection collection = new CodeDescriptionEmailTemplateCollection(typeof(DocSupportIncident));

			CodeDescriptionEmailTemplate template1 = collection.AddNew();
			template1.Code = "ABC";
			template1.Description = (NoResString)"ABC Description";
			template1.EmailTemplate.EmailSubject = "ABC Email Subject";
			template1.EmailTemplate.EmailBody = "ABC Email Body";

			CodeDescriptionEmailTemplate template2 = collection.AddNew();
			template2.Code = "DDD";
			template2.Description = (NoResString)"DDD Description";
			template2.Bool = true;
			template2.EmailTemplate.EmailSubject = "DDD Email Subject";
			template2.EmailTemplate.EmailBody = "DDD Email Body";

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,69,0,109,0,97,
				0,105,0,108,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,
				0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,
				0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,
				0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,69,0,109,0,97,0,105,0,108,0,84,
				0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,
				0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,65,0,66,0,67,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,
				0,111,0,110,0,62,0,65,0,66,0,67,0,32,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,
				0,66,0,111,0,111,0,108,0,62,0,78,0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,0,78,0,111,0,116,0,105,0,102,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,69,0,109,0,97,0,105,0,108,0,84,0,101,
				0,109,0,112,0,108,0,97,0,116,0,101,0,62,0,60,0,69,0,109,0,97,0,105,0,108,0,83,0,117,0,98,0,106,0,101,0,99,0,116,0,62,0,65,0,66,0,67,0,32,0,69,0,109,0,97,0,105,0,108,0,32,0,83,0,117,
				0,98,0,106,0,101,0,99,0,116,0,60,0,47,0,69,0,109,0,97,0,105,0,108,0,83,0,117,0,98,0,106,0,101,0,99,0,116,0,62,0,60,0,69,0,109,0,97,0,105,0,108,0,66,0,111,0,100,0,121,0,62,0,65,0,66,
				0,67,0,32,0,69,0,109,0,97,0,105,0,108,0,32,0,66,0,111,0,100,0,121,0,60,0,47,0,69,0,109,0,97,0,105,0,108,0,66,0,111,0,100,0,121,0,62,0,60,0,47,0,78,0,111,0,116,0,105,0,102,0,105,0,99,
				0,97,0,116,0,105,0,111,0,110,0,69,0,109,0,97,0,105,0,108,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,62,0,60,0,47,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,
				0,105,0,111,0,110,0,69,0,109,0,97,0,105,0,108,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,
				0,69,0,109,0,97,0,105,0,108,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,
				0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,68,0,68,0,68,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,
				0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,68,0,68,0,68,0,32,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,
				0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,89,0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,0,78,0,111,0,116,0,105,0,102,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,69,
				0,109,0,97,0,105,0,108,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,62,0,60,0,69,0,109,0,97,0,105,0,108,0,83,0,117,0,98,0,106,0,101,0,99,0,116,0,62,0,68,0,68,0,68,0,32,0,69,0,109,
				0,97,0,105,0,108,0,32,0,83,0,117,0,98,0,106,0,101,0,99,0,116,0,60,0,47,0,69,0,109,0,97,0,105,0,108,0,83,0,117,0,98,0,106,0,101,0,99,0,116,0,62,0,60,0,69,0,109,0,97,0,105,0,108,0,66,
				0,111,0,100,0,121,0,62,0,68,0,68,0,68,0,32,0,69,0,109,0,97,0,105,0,108,0,32,0,66,0,111,0,100,0,121,0,60,0,47,0,69,0,109,0,97,0,105,0,108,0,66,0,111,0,100,0,121,0,62,0,60,0,47,0,78,
				0,111,0,116,0,105,0,102,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,69,0,109,0,97,0,105,0,108,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,62,0,60,0,47,0,67,0,111,0,100,0,101,0,68,0,101,
				0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,69,0,109,0,97,0,105,0,108,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,
				0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,69,0,109,0,97,0,105,0,108,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
