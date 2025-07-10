using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using DeclarationValueChangedAnnouncer = Enterprise.Customs.CA.Business.DeclarationValueChangedAnnouncer;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

#if !WINZOR
namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CustomsPackingUserControl))]
	sealed class CustomsPackingUserControlBasherTest : ImportCustomsUserControlBasherTest
	{
		protected override Type UserControlToBashType => typeof(CustomsPackingUserControl);

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Packages.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new CustomsPackingUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
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
					var subcriberTarget = subcriber.Value.Target;
					if (subcriberTarget is DeclarationValueChangedAnnouncer)
					{
						var onValueChangedField = subcriberTarget.GetType().BaseType.GetField("OnValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
						var onValueChanged = onValueChangedField.GetValue(subcriberTarget);
						var onValueChangedTarget = ((EventHandler)onValueChanged).Target;
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is CustomsPackingUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is CustomsPackingUserControl));
					}
				}
			}
		}
	}
}
#endif
