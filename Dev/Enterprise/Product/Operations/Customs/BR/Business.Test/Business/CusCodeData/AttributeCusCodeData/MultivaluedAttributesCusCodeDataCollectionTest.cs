using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(MultivaluedAttributesCusCodeDataCollection))]
	public class MultivaluedAttributesCusCodeDataCollectionTest : CusCodeDataCollectionTest<AttributeCusCodeData>
	{
		protected override CusCodeDataCollection<AttributeCusCodeData> GetCusCodeDataCollection()
		{
			var stringQuestion = Factory.New<RefCusProfileQuestion>();
			stringQuestion.XQ2_Code = "ATT_1";
			stringQuestion.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			stringQuestion.XQ2_AllowMultipleAnswers = true;
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var attribute = Factory.NewWithValidTestData<AttributeCusCodeData>();
			attribute.CY_ParentID = goodsCatalog.PK;
			attribute.CY_ParentTableCode = goodsCatalog.TablePrefix;
			attribute.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);

			return new MultivaluedAttributesCusCodeDataCollection(attribute);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var result = Factory.NewWithValidTestData<AttributeCusCodeData>();
			result.CY_ParentID = goodsCatalog.PK;
			result.CY_ParentTableCode = goodsCatalog.TablePrefix;
			result.CY_Order = 1;
			return result;
		}

		public void TestRelationshipFilter()
		{
			var stringQuestion = Factory.New<RefCusProfileQuestion>();
			stringQuestion.XQ2_Code = "ATT_1";
			stringQuestion.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			stringQuestion.XQ2_AllowMultipleAnswers = true;
			var catalog = Factory.New<CusGoodsCatalog>();
			AttributeCusCodeData AddAttributeCusCodeData(ZString type, ZString code, ZShort order)
			{
				var attribute = Factory.New<AttributeCusCodeData>();
				attribute.CY_ParentID = catalog.PK;
				attribute.CY_ParentTableCode = catalog.TablePrefix;
				attribute.CY_Type = type;
				attribute.CY_Code = code;
				attribute.CY_Order = order;
				return attribute;
			}

			var attribute1 = AddAttributeCusCodeData("ATT", "ATT_1", 0);
			attribute1.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			var attribute2 = AddAttributeCusCodeData("ATT", "ATT_1", 1);
			var attribute3 = AddAttributeCusCodeData("ATT", "ATT_1", 2);
			var attribute4 = AddAttributeCusCodeData("ATT", "ATT_2", 0);
			attribute4.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			var attribute5 = AddAttributeCusCodeData("ATT", "ATT_2", 1);
			var attribute6 = AddAttributeCusCodeData("ATT", "ATT_2", 2);
			var attribute7 = AddAttributeCusCodeData("RTA", "ATT_2", 0);
			attribute7.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			var attribute8 = AddAttributeCusCodeData("RTA", "ATT_2", 1);
			var attribute9 = AddAttributeCusCodeData("RTA", "ATT_2", 2);

			var collection = new MultivaluedAttributesCusCodeDataCollection(attribute1);
			collection.Load();
			AssertContainsExactElementsInAnyOrder([attribute2, attribute3], collection);
			collection = new MultivaluedAttributesCusCodeDataCollection(attribute2);
			collection.Load();
			AssertEquals("AllowMultipleAnswers = false", 0, collection.Count);
			collection = new MultivaluedAttributesCusCodeDataCollection(attribute3);
			collection.Load();
			AssertEquals("AllowMultipleAnswers = false", 0, collection.Count);

			collection = new MultivaluedAttributesCusCodeDataCollection(attribute4);
			collection.Load();
			AssertContainsExactElementsInAnyOrder([attribute5, attribute6], collection);
			collection = new MultivaluedAttributesCusCodeDataCollection(attribute5);
			collection.Load();
			AssertEquals("AllowMultipleAnswers = false", 0, collection.Count);
			collection = new MultivaluedAttributesCusCodeDataCollection(attribute6);
			collection.Load();
			AssertEquals("AllowMultipleAnswers = false", 0, collection.Count);

			collection = new MultivaluedAttributesCusCodeDataCollection(attribute7);
			collection.Load();
			AssertContainsExactElementsInAnyOrder([attribute8, attribute9], collection);
			collection = new MultivaluedAttributesCusCodeDataCollection(attribute8);
			collection.Load();
			AssertEquals("AllowMultipleAnswers = false", 0, collection.Count);
			collection = new MultivaluedAttributesCusCodeDataCollection(attribute9);
			collection.Load();
			AssertEquals("AllowMultipleAnswers = false", 0, collection.Count);
		}

		public void TestUpdateAll()
		{
			var catalog = Factory.New<CusGoodsCatalog>();
			var parentAttribute = catalog.Attributes.AddNew();
			parentAttribute.CY_Code = "ATT_1";
			parentAttribute.CY_Data = "TEST";
			AssertUpdateAllMultivaluedAttributes(["1"]);
			AssertUpdateAllMultivaluedAttributes(["1", "2", "3", "4"]);
			AssertUpdateAllMultivaluedAttributes(["1", "3"]);
			AssertUpdateAllMultivaluedAttributes([]);

			void AssertUpdateAllMultivaluedAttributes(ZString[] lines) => CombineAssertions(() =>
			{
				parentAttribute.MultivaluedAttributesLinked.UpdateAll(lines);

				AssertEquals("Count", lines.Length, parentAttribute.MultivaluedAttributesLinked.Count);

				for (var i = 0; i < lines.Length; i++)
				{
					var attribute = parentAttribute.MultivaluedAttributesLinked.Cast<AttributeCusCodeData>().Single(x => x.CY_Order == i + 1);
					AssertEquals("CY_Type", CusCodeDataTypeList.Codes.Attribute, attribute.CY_Type);
					AssertEquals("CY_Code", "ATT_1", attribute.CY_Code);
					AssertEquals("CY_Data", lines[i], attribute.CY_Data);
				}
			});
		}
	}
}
