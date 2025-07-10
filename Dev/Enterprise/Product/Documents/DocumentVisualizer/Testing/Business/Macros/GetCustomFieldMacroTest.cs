using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class GetCustomFieldMacroTest : TestCaseWithFactory
	{
		#region TestRunMacro_Object

		public void TestRunMacro_Object()
		{
			var expr = "GetCustomField(\"Custom Field\")".With<DataLibrary>().CreateExpression();
			var res = expr.Evaluate(new object());

			AssertNull("default value for objects that are neither ICustomFieldProvider nor ICustomFieldProviderProxy", res);
		}

		#endregion

		#region TestRunMacro_IDynamicData

		public void TestRunMacro_IDynamicData_NoFactory() => TestRunMacro_IDynamicData_Factory(new DataLibrary());

		public void TestRunMacro_IDynamicData_Factory() => TestRunMacro_IDynamicData_Factory(new DataLibrary(Factory));

		void TestRunMacro_IDynamicData_Factory(DataLibrary dataLibrary)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_CustomField");
			var customBusinessObj = new CustomBusinessObject(dummy, customPropertyCollection)
			{
				["ZZZ_CustomField"] = "text"
			};

			var providerMock = new Mock<ICustomFieldProvider>();
			providerMock.Setup(p => p.GetCustomBusinessObject(It.IsAny<bool>())).Returns(customBusinessObj);

			var dynamicData = providerMock.Object.MakeDynamic();

			var context = new[] { dataLibrary }.CreateContext();
			var expr = "GetCustomField(\"Custom Field\")".With(context).CreateExpression();
			var res = expr.Evaluate(dynamicData);

			AssertEquals("returned custom field value", "text", res);
		}

		#endregion

		#region TestRunMacro_ICustomFieldProvider

		public void TestRunMacro_ICustomFieldProvider_NoFactory() => AssertRunMacro_ICustomFieldProvider_Factory(new DataLibrary());

		public void TestRunMacro_ICustomFieldProvider_Factory() => AssertRunMacro_ICustomFieldProvider_Factory(new DataLibrary(Factory));

		void AssertRunMacro_ICustomFieldProvider_Factory(DataLibrary dataLibrary)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_CustomField");
			var customBusinessObj = new CustomBusinessObject(dummy, customPropertyCollection)
			{
				["ZZZ_CustomField"] = "text"
			};

			var mock = new Mock<ICustomFieldProvider>();
			mock.Setup(p => p.GetCustomBusinessObject(It.IsAny<bool>())).Returns(customBusinessObj);

			var context = new[] { dataLibrary }.CreateContext();
			var expr = "GetCustomField(\"Custom Field\")".With(context).CreateExpression();
			var res = expr.Evaluate(mock.Object);

			AssertEquals("returned custom field value", "text", res);
		}

		#endregion

		#region TestRunMacro_ICustomFieldProviderProxy

		public void TestRunMacro_ICustomFieldProviderProxy_NoFactory() => AssertRunMacro_ICustomFieldProviderProxy_Factory(new DataLibrary());

		public void TestRunMacro_ICustomFieldProviderProxy_Factory() => AssertRunMacro_ICustomFieldProviderProxy_Factory(new DataLibrary(Factory));

		void AssertRunMacro_ICustomFieldProviderProxy_Factory(DataLibrary dataLibrary)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_CustomField");
			var customBusinessObj = new CustomBusinessObject(dummy, customPropertyCollection)
			{
				["ZZZ_CustomField"] = "text"
			};

			var mock = new Mock<ICustomFieldProviderProxy>();
			mock.SetupGet(p => p.CustomBusinessObject).Returns(customBusinessObj);

			var context = new[] { dataLibrary }.CreateContext();
			var expr = "GetCustomField(\"Custom Field\")".With(context).CreateExpression();
			var res = expr.Evaluate(mock.Object);

			AssertEquals("returned custom field value", "text", res);
		}

		#endregion

		#region Implementation

		CustomPropertyCollectionImpl GetCustomPropertyCollection()
		{
			var values = new Dictionary<string, object>();

			return new CustomPropertyCollectionImpl(
				propertyName => values.TryGetValue(propertyName, out var value) ? value : null,
				(propertyName, value) =>
				{
					values[propertyName] = value;
					return true;
				});
		}

		#endregion
	}
}
