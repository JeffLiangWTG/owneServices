using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestsSubclassesOf(typeof(NonPersistentBusinessObjectRegistryDataType<>), typeof(ExcludeFromRegistryDataTypeTestAttribute))]
	public abstract class NonPersistentBusinessObjectRegistryDataTypeTestCase<T> : RegistryDataTypeTestCase<T> where T : IRegistryDataType
	{
		protected TBizo GetOrNew<TBizo>(BusinessObjectFactory factory, Guid pk, Action<TBizo> itemCreatedCallback = null, bool fillWithValidTestData = true) where TBizo : BusinessObject
		{
			var callback = itemCreatedCallback != null
				? (item => itemCreatedCallback((TBizo)item))
				: (Action<BusinessObject>)null;

			return (TBizo)GetOrNew(factory, typeof(TBizo), pk, callback, fillWithValidTestData);
		}

		protected BusinessObject GetOrNew(BusinessObjectFactory factory, Type tBizo, Guid pk, Action<BusinessObject> itemCreatedCallback, bool fillWithValidTestData = true)
		{
			var item = factory.Load(tBizo, pk);
			if (item == null)
			{
				item = factory.New(tBizo, pk);

				if (fillWithValidTestData)
				{
					item.FillWithValidTestData();
				}

				itemCreatedCallback?.Invoke(item);

				factory.Save();
			}

			return item;
		}

		protected string CodeFromGuid(Guid guid)
		{
			return new string(guid.ToString().Where(char.IsLetter).Take(3).ToArray());
		}

		public void TestNullReferenceExceptionNotThrow()
		{
			var dataType = new NonPersistentBusinessObjectRegistryDataTypeForTest();
			AssertNoExceptionThrown(() => { _ = dataType.CloneValue(null); });
		}

		public void TestEditor()
		{
			RegistryEditorAttribute attribute = (RegistryEditorAttribute)TypeDescriptor.GetAttributes(DataType)[typeof(RegistryEditorAttribute)];

			if (HasEditor)
			{
				Type editorType = Type.GetType(attribute.TypeName);
				AssertNotNull("Cant find the editor type", editorType);

				var fallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
				var factory = new BusinessObjectFactory();

				var editor = Activator.CreateInstance(editorType, [DataType, fallback, factory]);
				AssertNotNull("Editor should not be null.", editor);
				AssertEquals("Editor.GetType().Name", ExpectedEditorName, editor.GetType().Name);
			}
			else
			{
				AssertNull("Expected Editor attribute to be null as there should not be an editor.", attribute);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "System.AppDomain.CurrentDomain is available in NetCore.")]
		public void TestRegistryPerformance()
		{
			static HashSet<string> GetAssemblies()
			{
				return AppDomain.CurrentDomain.GetAssemblies()
				.Select(assembly => assembly.FullName)
				.ToHashSet();
			}
			var binaryValue = GetValidSamples()[0].BinaryValue;
			_ = DataType.Deserialise(binaryValue);
			var beforeAssemblies = GetAssemblies();

			for (var i = 0; i < 100; i++)
			{
				_ = DataType.Deserialise(binaryValue);
			}

			var afterAssemblies = GetAssemblies();

			var leakedAssemblies = afterAssemblies.Except(beforeAssemblies).ToList();
			AssertEquals("Registry XML serialisation should not leak assemblies.", 0, leakedAssemblies.Count);
		}

		public void TestIsAbstract()
		{
			AssertEquals(
				$"{typeof(NonPersistentBusinessObjectRegistryDataType<>).Name} should be abstract to force the developer to write a unit test that extends {typeof(NonPersistentBusinessObjectRegistryDataTypeTestCase<>).Name}",
				expected: true, typeof(NonPersistentBusinessObjectRegistryDataType<>).IsAbstract);
		}

		public void TestValidate()
		{
			var factory = new BusinessObjectFactory();

			var dataType = new NonPersistentBusinessObjectRegistryDataTypeForTest();
			var bO = new DummyNonPersistentBusinessObject();
			var dependent = bO.Related.AddNew();

			// expect no validation exception
			dataType.Validate(null, bO, Guid.Empty, Guid.Empty, Guid.Empty);

			dependent.Property = "invalid";
			AssertEquals("Dependent business object should have errors for test", expected: true, dependent.HasErrors);
			try
			{
				dataType.Validate(null, bO, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expected an exception");
			}
			catch (RegistryValidationException)
			{
			}
		}

		public void TestCode()
		{
			AssertEquals("RegistryDataType Code", ExpectedCode, DataType.Code);
		}

		#region Implementation

		protected virtual string ExpectedCode
		{
			get { return RegistryDataTypes.Codes.Binary; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1080:DoNotUseTypeofIsAssignableFrom", Justification = "Baseline")]
		protected NonPersistentBusinessObject CreateNonPersistentBusinessObjectWithTestData()
		{
			Type businessOrCollectionElementType = DataType.DataType;
			if (typeof(BusinessObjectCollection).IsAssignableFrom(businessOrCollectionElementType))
			{
				businessOrCollectionElementType = BusinessObjectCollection.GetElementTypeFromCollectionType(businessOrCollectionElementType);
			}

			NonPersistentBusinessObject result = (NonPersistentBusinessObject)Activator.CreateInstance(businessOrCollectionElementType);
			foreach (ZPropertyInfo property in result.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				property.Value = (IZType)GetDummyValueForProperty(property);
			}
			return result;
		}

		protected BusinessObjectCollection CreateNonPersistentBusinessObjectCollectionWithTestData()
		{
			Type businessType = DataType.DataType;
			BusinessObjectCollection result = (BusinessObjectCollection)Activator.CreateInstance(businessType);
			result.Add(CreateNonPersistentBusinessObjectWithTestData());
			result.Add(CreateNonPersistentBusinessObjectWithTestData());
			return result;
		}

		delegate object GetDummyValueForPropertyDelegate(ZPropertyInfo property);
		protected virtual object GetDummyValueForProperty(ZPropertyInfo property)
		{
			object result;
			if (property.PropertyType == typeof(ZString))
			{
				ZString str = "";
				for (int i = 0; i < property.MaxLength; i++)
				{
					str += "x";
				}
				result = str;
			}
			else if (property.PropertyType == typeof(ZInt))
			{
				result = new ZInt(82);
			}
			else if (property.PropertyType == typeof(ZDecimal))
			{
				result = new ZDecimal(-0.00001);
			}
			else if (property.PropertyType == typeof(ZBool))
			{
				result = ZBool.True;
			}
			else
			{
				throw new NotSupportedException(
					"Didn't know how to create a dummy value for property with data type " +
					property.PropertyType.FullName +
					". Either add this type to " +
					typeof(NonPersistentBusinessObjectRegistryDataTypeTestCase<>).Name +
					" or override " +
					new GetDummyValueForPropertyDelegate(GetDummyValueForProperty).Method.Name +
					" and return a dummy value.");
			}
			return result;
		}

		protected sealed override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			AssertNotNull(lhs);
			AssertNotNull(rhs);
			AssertEquals(message, lhs.GetType(), rhs.GetType());

			if (lhs is BusinessObjectCollection expectedCollection)
			{
				var actualCollection = (BusinessObjectCollection)rhs;
				AssertEquals(message, expectedCollection.Count, actualCollection.Count);
				for (var i = 0; i < expectedCollection.Count; i++)
				{
					AssertValuesEqual(message, (NonPersistentBusinessObject)((IList)expectedCollection)[i], (NonPersistentBusinessObject)((IList)actualCollection)[i]);
				}
			}
			else
			{
				AssertValuesEqual(message, (NonPersistentBusinessObject)lhs, (NonPersistentBusinessObject)rhs);
			}
		}

		protected virtual void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			AssertNotNull(lhs);
			AssertNotNull(rhs);
			foreach (ZPropertyInfo property in lhs.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				AssertEquals(message + ": " + property.Name, lhs[property.Name], rhs[property.Name]);
			}
		}

		protected virtual bool HasEditor
		{
			get { return true; }
		}

		protected virtual string ExpectedEditorName
		{
			get { return null; }
		}

		#endregion
	}
}
