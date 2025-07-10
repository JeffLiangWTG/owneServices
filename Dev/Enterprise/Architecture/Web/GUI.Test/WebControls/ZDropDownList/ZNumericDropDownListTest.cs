using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZNumericDropDownListTest : ZDropDownListTest
	{
		#region Setup

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ZNumericDropDownList();
		}

		class DummyWithNumericList : DummyWithCodeDescriptionPairList
		{
			public DummyWithNumericList(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ReadOnlyCodeDescriptionPairList DummyList
			{
				get
				{
					CodeDescriptionPairList list = new CodeDescriptionPairList();
					list.AddPair("1");
					list.AddPair("2");
					list.AddPair("3");
					return list;
				}
			}
		}

		#endregion

		public void TestBindToNumericField()
		{
			TestBindToNumericField<ZInt>(DummyBizoSchema.Constants.Z0_Number, ZInt.ParseSafe);
			TestBindToNumericField<ZDecimal>(DummyBizoSchema.Constants.Z0_Decimal, ZDecimal.ParseSafe);
			TestBindToNumericField<ZShort>(DummyBizoSchema.Constants.Z0_Short, ZShort.ParseSafe);
			TestBindToNumericField<ZByte>(DummyBizoSchema.Constants.Z0_Byte, ZByte.ParseSafe);
		}

		public void TestBindToUnsupportedType()
		{
			try
			{
				TestBindToNumericField<ZString>(DummyBizoSchema.Constants.Z0_NVarChar, null);
				Fail("Should throw NotSupportedException");
			}
			catch (NotSupportedException ex)
			{
				AssertEquals(string.Format("The type you are binding to is not supported by the ZNumericDropDownList: <{0}>.", typeof(ZString).FullName), ex.Message);
			}
		}

		delegate T ParseSafeDelegate<T>(ZString valueAsString, T defaultValue);

		void TestBindToNumericField<T>(string bindTo, ParseSafeDelegate<T> parseSafeDelegate)
		{
			DummyWithNumericList dummy = Factory.New<DummyWithNumericList>();
			DropDown.BindTo = bindTo;
			DropDown.BindToList = "DummyList";
			Page.Controls.Add(DropDown);
			dummy[bindTo] = (parseSafeDelegate != null) ? parseSafeDelegate(dummy.DummyList[0].Code, default(T)) : dummy.DummyList[0].Code;
			DropDown.Bind(dummy);
			AssertEquals(dummy.DummyList[0].Code, DropDown.SelectedValue);

			Page.IsPostBack = true;
			DropDown.CachedSelectedValue = dummy.DummyList[1].Code;
			DropDown.HasChanges = true;
			DropDown.Bind(dummy);
			AssertEquals("2", DropDown.SelectedValue);
			AssertEquals((parseSafeDelegate != null) ? parseSafeDelegate("2", default(T)) : "2", dummy[bindTo]);

			DropDown.CachedSelectedValue = dummy.DummyList[2].Code;
			DropDown.HasChanges = true;
			DropDown.Bind(dummy);
			AssertEquals("3", DropDown.SelectedValue);
			AssertEquals((parseSafeDelegate != null) ? parseSafeDelegate("3", default(T)) : "3", dummy[bindTo]);
		}

		public void TestWrongValue()
		{
			DropDown.SelectedValue = "abc";
			AssertEquals("Selected value should be empty", "", DropDown.SelectedValue);
		}
	}
}
