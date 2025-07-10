using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTemplateColumnTestOnce : TestCase
	{
		public void TestGetNew()
		{
			ZTemplateColumn column = ZTemplateColumn.GetNew("calc", DummyBizoSchema.Z0_Decimal, ZString.Empty);
			AssertColumn(column, typeof(ZCalcEditColumn), "calc", DummyBizoSchema.Z0_Decimal.Name);

			column = ZTemplateColumn.GetNew("text", DummyBizoSchema.Z0_NVarCharMax, ZString.Empty);
			AssertColumn(column, typeof(ZTextEditColumn), "text", DummyBizoSchema.Z0_NVarCharMax.Name);

			column = ZTemplateColumn.GetNew("date", DummyBizoSchema.Z0_Date, ZString.Empty);
			AssertColumn(column, typeof(ZDateTimeColumn), "date", DummyBizoSchema.Z0_Date.Name);

			column = ZTemplateColumn.GetNew("flag", DummyBizoSchema.Z0_Bool, ZString.Empty);
			AssertColumn(column, typeof(ZCheckBoxColumn), "flag", DummyBizoSchema.Z0_Bool.Name);

			column = ZTemplateColumn.GetNew("flag", DummyBizoSchema.Z0_Bool, "blah");
			AssertColumn(column, typeof(ZCheckBoxColumn), "flag", "blah+" + DummyBizoSchema.Z0_Bool.Name);

			column = ZTemplateColumn.GetNew("contact", DummyBizoSchema.Z0_Number, "blah");
			AssertColumn(column, typeof(ZTextEditColumn), "contact", "blah+" + DummyBizoSchema.Z0_Number.Name);

			column = ZTemplateColumn.GetNew("calc", typeof(ZDecimal), DummyBizoSchema.Z0_Decimal.Name);
			AssertColumn(column, typeof(ZCalcEditColumn), "calc", DummyBizoSchema.Z0_Decimal.Name);

			column = ZTemplateColumn.GetNew("text", typeof(ZString), DummyBizoSchema.Z0_NVarCharMax.Name);
			AssertColumn(column, typeof(ZTextEditColumn), "text", DummyBizoSchema.Z0_NVarCharMax.Name);

			column = ZTemplateColumn.GetNew("date", typeof(ZDateTime), DummyBizoSchema.Z0_Date.Name);
			AssertColumn(column, typeof(ZDateTimeColumn), "date", DummyBizoSchema.Z0_Date.Name);

			column = ZTemplateColumn.GetNew("flag", typeof(ZBool), DummyBizoSchema.Z0_Bool.Name);
			AssertColumn(column, typeof(ZCheckBoxColumn), "flag", DummyBizoSchema.Z0_Bool.Name);

			column = ZTemplateColumn.GetNew("contact", typeof(MasterFiles.Business.OrgContact), "ZZ_Contact");
			AssertColumn(column, typeof(ZTextEditColumn), "contact", "ZZ_Contact");
		}

		void AssertColumn(ZTemplateColumn column, Type type, ZString caption, ZString bindTo)
		{
			AssertEquals("Column type", type, column.GetType());
			AssertEquals("Column caption", caption, column.HeaderText);
			AssertEquals("Column BindTo", bindTo, column.BindTo);
		}
	}
}
