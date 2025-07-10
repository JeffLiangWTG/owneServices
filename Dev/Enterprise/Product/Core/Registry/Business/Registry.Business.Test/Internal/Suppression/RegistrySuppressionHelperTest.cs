using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistrySuppressionHelperTest : TestCaseWithFactory
	{
		public void TestGetCollection()
		{
			CodeDescriptionBoolCollection list = RegistrySuppressionHelper.GetCollection(null);
			AssertItem(list[0], "001", "ETA", false);
			AssertItem(list[1], "002", "ETD", false);
			AssertItem(list[2], "004", "ATA", false);
			AssertItem(list[3], "008", "ATD", false);
			AssertItem(list[4], "016", "Flight Number", false);
			AssertItem(list[5], "064", "Transport Info", false);
			AssertItem(list[6], "128", "Carrier", false);
			AssertItem(list[7], "256", "Declaration Export Date", false);
			AssertItem(list[8], "512", "Declaration Date At Origin", false);
			AssertItem(list[9], "102", "Declaration Folio", false);
			AssertItem(list[10], "204", "Master Bill", false);

			List<SuppressFields> fields = new List<SuppressFields>();
			list = RegistrySuppressionHelper.GetCollection(fields);

			AssertItem(list[0], "001", "ETA", false);
			AssertItem(list[1], "002", "ETD", false);
			AssertItem(list[2], "004", "ATA", false);
			AssertItem(list[3], "008", "ATD", false);
			AssertItem(list[4], "016", "Flight Number", false);
			AssertItem(list[5], "064", "Transport Info", false);
			AssertItem(list[6], "128", "Carrier", false);
			AssertItem(list[7], "256", "Declaration Export Date", false);
			AssertItem(list[8], "512", "Declaration Date At Origin", false);
			AssertItem(list[9], "102", "Declaration Folio", false);
			AssertItem(list[10], "204", "Master Bill", false);

			fields.Add(SuppressFields.ETA);
			fields.Add(SuppressFields.Carrier);
			fields.Add(SuppressFields.DeclarationFolio);
			fields.Add(SuppressFields.FlightNumber);
			fields.Add(SuppressFields.TransportInfo);
			list = RegistrySuppressionHelper.GetCollection(fields);

			AssertItem(list[0], "001", "ETA", true);
			AssertItem(list[1], "002", "ETD", false);
			AssertItem(list[2], "004", "ATA", false);
			AssertItem(list[3], "008", "ATD", false);
			AssertItem(list[4], "016", "Flight Number", true);
			AssertItem(list[5], "064", "Transport Info", true);
			AssertItem(list[6], "128", "Carrier", true);
			AssertItem(list[7], "256", "Declaration Export Date", false);
			AssertItem(list[8], "512", "Declaration Date At Origin", false);
			AssertItem(list[9], "102", "Declaration Folio", true);
			AssertItem(list[10], "204", "Master Bill", false);
		}

		void AssertItem(CodeDescriptionBool item, string code, string name, bool value)
		{
			AssertEquals(name, item.Description);
			AssertEquals(code, item.Code);
			AssertEquals(value, item.Bool);
		}

		public void TestGetSetting()
		{
			List<SuppressFields> fields = new List<SuppressFields> { SuppressFields.TransportInfo, SuppressFields.FlightNumber, SuppressFields.DeclarationFolio, SuppressFields.Carrier, SuppressFields.ETA };

			CodeDescriptionBoolCollection list = RegistrySuppressionHelper.GetCollection(fields);

			AssertEquals(true, RegistrySuppressionHelper.GetSetting(list, SuppressFields.ETA));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.ETD));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.ATA));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.ATD));
			AssertEquals(true, RegistrySuppressionHelper.GetSetting(list, SuppressFields.FlightNumber));
			AssertEquals(true, RegistrySuppressionHelper.GetSetting(list, SuppressFields.TransportInfo));
			AssertEquals(true, RegistrySuppressionHelper.GetSetting(list, SuppressFields.Carrier));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.DeclarationExportDate));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.DeclarationDateAtOrigin));
			AssertEquals(true, RegistrySuppressionHelper.GetSetting(list, SuppressFields.DeclarationFolio));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.MasterBill));

			list = new CodeDescriptionBoolCollection();

			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.ETA));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.ETD));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.ATA));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.ATD));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.FlightNumber));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.TransportInfo));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.Carrier));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.DeclarationExportDate));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.DeclarationDateAtOrigin));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.DeclarationFolio));
			AssertEquals(false, RegistrySuppressionHelper.GetSetting(list, SuppressFields.MasterBill));
		}

		public void TestGetDefaultFields()
		{
			CodeDescriptionBoolCollection list = RegistrySuppressionHelper.GetDefaultFields(Guid.Empty);

			AssertItem(list[0], "001", "ETA", false);
			AssertItem(list[1], "002", "ETD", false);
			AssertItem(list[2], "004", "ATA", false);
			AssertItem(list[3], "008", "ATD", false);
			AssertItem(list[4], "016", "Flight Number", false);
			AssertItem(list[5], "064", "Transport Info", false);
			AssertItem(list[6], "128", "Carrier", false);
			AssertItem(list[7], "256", "Declaration Export Date", false);
			AssertItem(list[8], "512", "Declaration Date At Origin", false);
			AssertItem(list[9], "102", "Declaration Folio", false);
			AssertItem(list[10], "204", "Master Bill", false);
		}
	}
}
