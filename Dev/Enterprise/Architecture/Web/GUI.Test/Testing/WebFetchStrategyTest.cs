using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	public abstract class WebFetchStrategyTest : TestCaseWithFactory
	{
		public virtual void TestFetchStrategy()
		{
			AssertFetchHintsImplementedOnBusinessObject();
		}

		void AssertFetchHintsImplementedOnBusinessObject()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			BusinessObject bizO1 = LoadBusinessObjectAndCheckNotNull(factory, BizOType, BizOQuery1);
			BusinessObject bizO2 = LoadBusinessObjectAndCheckNotNull(factory, BizOType, BizOQuery2);

			TableColumn[] columns = GetTableColumnsFromModule(factory, bizO1, ModuleID);

			bizO1.FetchStrategy.FetchForView(columns);
			bizO2.FetchStrategy.FetchForView(columns);

			AssertHitBOPropertiesDBHits(factory, bizO1, columns, true);
			AssertHitBOPropertiesDBHits(factory, bizO2, columns, false);

			AssertNoFieldsHaveTheSameValue(columns, bizO1, bizO2);
		}

		void AssertHitBOPropertiesDBHits(BusinessObjectFactory factory, BusinessObject bizO, TableColumn[] columns, bool expectDBHitIncrease)
		{
			int dbHits = factory.DatabaseLoadCount;
			ZStringBuilder failedProperties = new ZStringBuilder();

			foreach (TableColumn column in columns)
			{
				string columnName = column.ColumnName;
				AssertNotNull(string.Format("Property: {0}", columnName), bizO[columnName]);
				int dbHitsDifference = factory.DatabaseLoadCount - dbHits;

				if (!expectDBHitIncrease && dbHitsDifference != 0)
				{
					failedProperties.Append(string.Format("- {0} ({1})", columnName, dbHitsDifference));
					dbHits = factory.DatabaseLoadCount;
				}
			}

			if (expectDBHitIncrease)
			{
				AssertNotEquals("Hitting related properties should increase DB hit count", dbHits, factory.DatabaseLoadCount);
			}
			else if (failedProperties.Length != 0)
			{
				string fetchHintRequiredMessage = string.Format("There were extra DB hits when hitting the following properties: {1}{0}{1}"
					+ "Fetch hints should be implemented for the appropriate business object.",
					failedProperties.ToStringWithNewLineBetweenAppends(), System.Environment.NewLine);
				Fail(fetchHintRequiredMessage);
			}
		}

		void AssertNoFieldsHaveTheSameValue(TableColumn[] columns, BusinessObject bizO1, BusinessObject bizO2)
		{
			ZStringBuilder failedProperties = new ZStringBuilder();
			bool failure = false;

			foreach (TableColumn column in columns)
			{
				string columnName = column.ColumnName;
				AssertNotNull(string.Format("Property: '{0}' on BizO1", columnName), bizO1[columnName]);
				AssertNotNull(string.Format("Property: '{0}' on BizO2", columnName), bizO2[columnName]);
				if (bizO1[columnName].Equals(bizO2[columnName]))
				{
					failedProperties.Append("- " + columnName + " (" + bizO1[columnName] + ")");
					failure = true;
				}
			}

			failedProperties.Prepend("Required fetch hints may have been missed due to caching, because the following properties were equal:");
			failedProperties.Append("Please ensure test data is set up so that no two properties are equal.");

			Assert(failedProperties.ToStringWithNewLineBetweenAppends(), !failure);
		}

		TableColumn[] GetTableColumnsFromModule(BusinessObjectFactory factory, BusinessObject bizO, WebModuleID moduleID)
		{
			TableColumn[] columns;

			using (ZFilterGridModule module = ZWebModuleFactory.Create(moduleID, factory, new ZTestPage()))
			{
				TableColumnCalculator calc = new TableColumnCalculator();
				columns = calc.GetTableColumnsOnThisObject(bizO, module.GridColumnFields);
			}

			return columns;
		}

		BusinessObject LoadBusinessObjectAndCheckNotNull(BusinessObjectFactory factory, Type businessObjectType, ZQuery query)
		{
			BusinessObject result = factory.LoadTop1(businessObjectType, query);
			AssertNotNull("No business object found. Check test data setup, and the business object query.", result);
			return result;
		}

		#region Properties

		public abstract Type BizOType { get; }
		public abstract WebModuleID ModuleID { get; }
		public abstract ZQuery BizOQuery1 { get; }
		public abstract ZQuery BizOQuery2 { get; }

		#endregion
	}
}
