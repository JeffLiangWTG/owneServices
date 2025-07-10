using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LowValueShipmentsUserControlTest : TestCaseWithFactory
	{
		public void TestHideGroupChargesGridForLVXJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice1 = declaration.Invoices.AddNew();
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice2 = lvxJob.Invoices.AddNew();
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice2, declaration);

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				System.Windows.Forms.Application.DoEvents();

				var headersGrid = (LVSSubHeadersGrid)form.Controls.Find("LVSSubHeadersGrid", true)[0];
				var detailsTabControl = (ZTemplateTabControl)form.Controls.Find("DetailsTabControl", true)[0];
				var chargesTabPage = (ZTabPage)form.Controls.Find("ChargesTabPage", true)[0];
				detailsTabControl.SelectTab(chargesTabPage);

				var chargesSplitContainer = (KSplitContainer)form.Controls.Find("ChargesSplitContainer", true)[0];
				headersGrid.SelectSingleElement(invoice1);
				Assert("Group Charges grid should be shown", !chargesSplitContainer.Panel2Collapsed);
				headersGrid.SelectSingleElement(invoice2);
				Assert("Group Charges grid should be hiden", chargesSplitContainer.Panel2Collapsed);
				headersGrid.SelectSingleElement(invoice1);
				Assert("Group Charges grid should be shown", !chargesSplitContainer.Panel2Collapsed);
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			using (var form = new ZForm(declaration))
			using (var control = new LowValueShipmentsUserControl())
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
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is LowValueShipmentsUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is LowValueShipmentsUserControl));
					}
				}
			}
		}
	}
}
