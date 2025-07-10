using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	public abstract class AutoCompleteHelperTest : TestCaseWithFactory
	{
		#region Test GetKey and GetText

		public virtual void TestGetKeyAndText()
		{
			BusinessObject bizO = GetBusinessObject();

			ZString text = "Unique name 1";
			ZPropertyAccessor.Set(bizO, TextColumn.ObjectName, text);

			IZType key = (IZType)ZPropertyAccessor.Get(bizO, KeyColumn.ObjectName);

			AssertEquals("Expected key", key, Helper.GetKey(text));
			AssertEquals("Expected text", text, Helper.GetText(key));
		}

		[HttpContextEnabledTest]
		public void TestGetText_MaxLength()
		{
			if (TextColumn.HasMaxLength)
			{
				var bizO = GetBusinessObject();

				var maxLengthText = new ZString('A', TextColumn.MaxLength);
				ZPropertyAccessor.Set(bizO, TextColumn.ObjectName, maxLengthText);
				Factory.Save();

				var key = (IZType)ZPropertyAccessor.Get(bizO, KeyColumn.ObjectName);

				AssertEquals("Expected key if text exceeds max length but matches", key, Helper.GetKey(maxLengthText + "A"));
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Test GetList and MaxOptionsCount

		public virtual void TestGetList()
		{
			BusinessObject[] bizOs = new BusinessObject[] {
				GetBusinessObject(),
				GetBusinessObject(),
				GetBusinessObject()
			};

			ZPropertyAccessor.Set(bizOs[0], TextColumn.ObjectName, (ZString)"Unique name 1");
			ZPropertyAccessor.Set(bizOs[1], TextColumn.ObjectName, (ZString)"Unique name 2A");
			ZPropertyAccessor.Set(bizOs[2], TextColumn.ObjectName, (ZString)"Unique name 3");

			Factory.Save();

			Helper.MaxOptionsCount = 10;
			AssertGetList("Unique na", 3);
			AssertGetList("Unique name 2", 1);

			Helper.MaxOptionsCount = 2;
			AssertGetList("Unique na", 2);
		}

		[HttpContextEnabledTest]
		public void TestGetList_MaxLength()
		{
			if (TextColumn.HasMaxLength)
			{
				var bizO = GetBusinessObject();

				var maxLengthText = new ZString('A', TextColumn.MaxLength);
				ZPropertyAccessor.Set(bizO, TextColumn.ObjectName, maxLengthText);
				Factory.Save();

				var result = Helper.GetList(maxLengthText + "A");

				AssertEquals("Expected items count: 1", 1, result.Count);
				Assert(result[0].Contains(maxLengthText));
			}
			else
			{
				Assert(true);
			}
		}

		protected void AssertGetList(string searchText, int expectedItemsCount)
		{
			List<string> res = Helper.GetList(searchText);

			AssertEquals(string.Format("Expected items count: {0}", expectedItemsCount), expectedItemsCount, res.Count);

			for (int i = 0; i < res.Count; i++)
			{
				string message = string.Format("'{0}' contains '{1}'", res[i], searchText);
				Assert(message, res[i].ToLower().Contains(searchText.ToLower()));
			}
		}

		#endregion

		#region Test Get PK from Code

		public void TextGetPKFromCode()
		{
			SchemaColumn codeColumn = GetProtectedProperty(Helper, "CodeColumn") as SchemaColumn;
			if (codeColumn != null)
			{
				BusinessObject bizO = GetBusinessObject();
				ZString code = (ZString)ZPropertyAccessor.Get(bizO, codeColumn.ObjectName);

				AssertEquals("Expected PK", bizO.PK, Helper.GetPKFromCode(code));
			}
		}

		#endregion

		#region Test Using Multi Row Options

		public void TestUsesMultiRowOptions()
		{
			SchemaColumn[] columnsToSelect = (SchemaColumn[])ExecuteProtectedMethod(Helper, "ColumnsToSelectForListFilter");

			AssertEquals(columnsToSelect.Length > 0, Helper.UsesMultiRowOptions);
		}

		#endregion

		#region Implementation

		protected abstract AutoCompleteHelper GetHelper();

		protected AutoCompleteHelper Helper
		{
			get
			{
				return helper ?? (helper = GetHelper());
			}
		}
		AutoCompleteHelper helper;

		protected virtual BusinessObject GetBusinessObject()
		{
			return Factory.NewWithValidTestData((Type)GetProtectedProperty(Helper, "BusinessObjectType"));
		}

		object GetProtectedProperty(object obj, string propertyName)
		{
			return InvokeProtectedMember(obj, propertyName, BindingFlags.GetProperty);
		}

		object ExecuteProtectedMethod(object obj, string methodName)
		{
			return InvokeProtectedMember(obj, methodName, BindingFlags.InvokeMethod);
		}

		object InvokeProtectedMember(object obj, string memberName, BindingFlags flag)
		{
			return obj.GetType().InvokeMember(memberName,
				flag | BindingFlags.Instance | BindingFlags.NonPublic,
				null, obj, null);
		}

		protected SchemaColumn TextColumn
		{
			get
			{
				return textColumn ?? (textColumn = (SchemaColumn)GetProtectedProperty(Helper, nameof(TextColumn)));
			}
		}
		SchemaColumn textColumn;

		protected SchemaColumn KeyColumn
		{
			get
			{
				return keyColumn ?? (keyColumn = (SchemaColumn)GetProtectedProperty(Helper, nameof(KeyColumn)));
			}
		}
		SchemaColumn keyColumn;

		#endregion
	}
}
