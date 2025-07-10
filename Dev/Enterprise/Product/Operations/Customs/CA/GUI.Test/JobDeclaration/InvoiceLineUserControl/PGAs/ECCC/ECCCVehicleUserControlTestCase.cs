using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ECCCVehicleUserControlTestCase : TestCaseWithFactory
	{
		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new ECCCVehicleUserControl(true))
			{
				filterControl.Show();
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_AlternativeQuotaUQ).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenUserControlIsDisposed_ZPropertyInfoValueChangedEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.JobComInvoiceLines.AddNew();
			invoiceline.CA_ECCCInd = "Y";
			var ecccPGAHeader = invoiceline.ECCCPGAHeader;
			ecccPGAHeader.CA_VEEProgramInd = "Y";
			using (var userControl = new ECCCVehicleUserControl())
			{
				userControl.SetDataBinding(ecccPGAHeader, "");
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
					Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriber.Value.Target.GetType()}.", !(subcriber.Value.Target is ECCCVehicleUserControl));
				}
			}
		}
	}
}
