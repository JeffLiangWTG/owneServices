using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(UserControlProviderList))]
	sealed class UserControlProviderListTest : NonPersistentBusinessObjectCollectionTestCase<UserControlProviderList>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TextField(Factory);
		}

		protected override UserControlProviderList GetCollectionToTest()
		{
			return new UserControlProviderList();
		}

		public void TestSDFFieldsTakePriorityOverUDFFields()
		{
			UserControlProviderList userDefinedFieldList = new UserControlProviderList();

			FilterField userDefinedField1 = new TextField(Factory);
			userDefinedField1.DisplayName = "Field 1";
			userDefinedFieldList.Add(userDefinedField1);

			FilterField userDefinedField3 = new TextField(Factory);
			userDefinedField3.DisplayName = "Field 3";
			userDefinedFieldList.Add(userDefinedField3);

			UserControlProviderList systemDefinedFieldList = new UserControlProviderList();

			FilterField systemDefinedField1 = new TextField(Factory);
			systemDefinedField1.DisplayName = "Field 1";
			systemDefinedFieldList.Add(systemDefinedField1);

			FilterField systemDefinedField2 = new TextField(Factory);
			systemDefinedField2.DisplayName = "Field 2";
			systemDefinedFieldList.Add(systemDefinedField2);

			UserControlProviderList amalgamatedList = new UserControlProviderList(systemDefinedFieldList, userDefinedFieldList);

			AssertEquals("amalgamatedList.Contains(systemDefinedField1)", true, amalgamatedList.Contains(systemDefinedField1));
			AssertEquals("amalgamatedList.Contains(systemDefinedField2)", true, amalgamatedList.Contains(systemDefinedField2));
			AssertEquals("amalgamatedList.Contains(userDefinedField1)", false, amalgamatedList.Contains(userDefinedField1));
			AssertEquals("amalgamatedList.Contains(userDefinedField3)", true, amalgamatedList.Contains(userDefinedField3));
			AssertEquals("amalgamatedList.Count", 3, amalgamatedList.Count);
		}

		public void TestConstructorCanHandleDoubleNulls()
		{
			UserControlProviderList nullList = null;
			UserControlProviderList list = new UserControlProviderList(nullList, nullList);
			AssertEquals("list.Count", 0, list.Count);
		}

		public void TestIndex()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			UserControlProviderList list = new UserControlProviderList();
			TextField field1 = new TextField(factory);
			field1.DisplayName = "Field1";

			TextField field2 = new TextField(factory);
			field2.DisplayName = "Field2";

			TextField field3 = new TextField(factory);
			field3.DisplayName = "Field3";

			list.Add(field1);
			list.Add(field2);

			AssertEquals("Contains Field1", true, list.ContainsName("Field1"));
			AssertEquals("Contains Field2", true, list.ContainsName("Field2"));
			AssertEquals("Contains Field3", false, list.ContainsName("Field3"));

			AssertEquals("Field1", field1, list["Field1"]);
			AssertEquals("Field2", field2, list["Field2"]);
			AssertEquals("Field3", null, list["Field3"]);
		}

		public void TestEnumerator()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			UserControlProviderList list = new UserControlProviderList();
			TextField field1 = new TextField(factory);
			field1.DisplayName = "Field1";

			TextField field2 = new TextField(factory);
			field2.DisplayName = "Field2";

			TextField field3 = new TextField(factory);
			field3.DisplayName = "Field3";

			list.Add(field1);
			list.Add(field2);
			list.Add(field3);

			string displayNames = "";
			foreach (TextField field in list)
			{
				displayNames += field.DisplayName;
			}

			AssertEquals("Field1Field2Field3", displayNames);
		}
	}
}
