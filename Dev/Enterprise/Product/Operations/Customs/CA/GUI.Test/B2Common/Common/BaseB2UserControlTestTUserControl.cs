using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	abstract class BaseB2UserControlTest<TUserControl> : TestCaseWithFactory
			where TUserControl : ZUserControl
	{
		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed_ZPropertyInfoValueChangedEvent()
		{
			var declaration = GetJobDeclarationForBOSubscribersShouldBeDetached();
			using (var form = new JobDeclarationForm(declaration))
			{
			}

			var propertyInfoStorageField = declaration.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(declaration.Factory);

			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);

			var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var dictionary = (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);

			foreach (var dictionaryOfSubcriber in dictionary)
			{
				foreach (var subcriber in dictionaryOfSubcriber.Value)
				{
					Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriber.Value.Target.GetType()}.", !(subcriber.Value.Target is TUserControl));
				}
			}
		}

		protected abstract JobDeclaration GetJobDeclarationForBOSubscribersShouldBeDetached();
	}
}
