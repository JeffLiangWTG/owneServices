using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;
using Incoterms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Registry.Business.Testing
{
	sealed class IncoTermChargeCodesBOTest : TestCaseWithFactory
	{
		public List<string> FieldsExcludedFromListValidation = new List<string>()
		{
			IncoTermChargeCodes.Schema.IncoTerm,
			IncoTermChargeCodes.Schema.IncoTermDescription
		};

		public void TestValidation()
		{
			IncoTermChargeCodes testObj = new IncoTermChargeCodes();

			foreach (FieldInfo field in typeof(IncoTermChargeCodes.Schema).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (field.Name != IncoTermChargeCodes.Schema.IncoTerm)
				{
					string propertyName = (string)field.GetValue(null);
					ZPropertyInfo propertyInfo = (ZPropertyInfo)testObj[propertyName + "Info"];

					if (!FieldsExcludedFromListValidation.Contains(field.Name))
					{
						testObj[propertyName] = "XXX";
						AssertHasError(propertyInfo, "Enter a valid selection.");

						testObj[propertyName] = Constants.PaymentParty.Consignor;
						AssertNoErrors(propertyInfo);
					}

					testObj[propertyName] = Constants.PaymentParty.Consignee;
					AssertNoErrors(propertyInfo);

					testObj[propertyName] = string.Empty;
					AssertHasError(propertyInfo, "Please enter a value.");
				}
			}
		}

		public void TestIncoTermsObsoleteWarning()
		{
			IncoTermChargeCodes testObj = new IncoTermChargeCodes();

			testObj.IncoTerm = Incoterms.DeliveredAtPlace;
			AssertNoWarnings(testObj.IncoTermInfo);

			testObj.IncoTerm = Incoterms.DeliveredAtFrontier;
			AssertHasWarning(testObj.IncoTermInfo, "This Incoterm is obsolete from 1 January 2011 according to the Incoterms 2010 rules.");

			testObj.IncoTerm = Incoterms.DeliveredExShip;
			AssertHasWarning(testObj.IncoTermInfo, "This Incoterm is obsolete from 1 January 2011 according to the Incoterms 2010 rules.");

			testObj.IncoTerm = Incoterms.DeliveredExQuay;
			AssertHasWarning(testObj.IncoTermInfo, "This Incoterm is obsolete from 1 January 2011 according to the Incoterms 2010 rules.");

			testObj.IncoTerm = Incoterms.DeliveredDutyUnpaid;
			AssertHasWarning(testObj.IncoTermInfo, "This Incoterm is obsolete from 1 January 2011 according to the Incoterms 2010 rules.");

			testObj.IncoTerm = Incoterms.DeliveredAtPlace;
			AssertNoWarnings(testObj.IncoTermInfo);
		}

		[TestDate(2020, 1, 1)]
		public void TestDefaultCodesInclude2020ChangesAfter2020()
		{
			var chargeCodes = new IncoTermChargeCodes();

			chargeCodes.SetDefaults(Incoterms.DeliveredDutyPaid);
			AssertEquals(Constants.PaymentParty.Consignee, chargeCodes.Unloading);

			chargeCodes.SetDefaults(Incoterms.DeliveredAtPlace);
			AssertEquals(Constants.PaymentParty.Consignee, chargeCodes.Unloading);

			chargeCodes.SetDefaults(Incoterms.DeliveredAtPlaceUnloaded);
			AssertEquals(Constants.PaymentParty.Consignor, chargeCodes.OriginBrokerage);
			AssertEquals(Constants.PaymentParty.Consignor, chargeCodes.Origin);
			AssertEquals(Constants.PaymentParty.Consignor, chargeCodes.Loading);
			AssertEquals(Constants.PaymentParty.Consignor, chargeCodes.Freight);
			AssertEquals(Constants.PaymentParty.Consignor, chargeCodes.Insurance);
			AssertEquals(Constants.PaymentParty.Consignor, chargeCodes.Unloading);
			AssertEquals(Constants.PaymentParty.Consignor, chargeCodes.Destination);
			AssertEquals(Constants.PaymentParty.Consignee, chargeCodes.Brokerage);
			AssertEquals(Constants.PaymentParty.Consignee, chargeCodes.CustomsDuty);
		}

		[TestDate(2019, 1, 1)]
		public void TestDefaultCodesExclude2020ChangesBefore2020()
		{
			var chargeCodes = new IncoTermChargeCodes();

			chargeCodes.SetDefaults(Incoterms.DeliveredDutyPaid);
			AssertEquals(Constants.PaymentParty.Consignor, chargeCodes.Unloading);

			chargeCodes.SetDefaults(Incoterms.DeliveredAtPlace);
			AssertEquals(Constants.PaymentParty.Consignor, chargeCodes.Unloading);
		}

		[TestDate(2020, 1, 1)]
		public void TestIncoTermsObsoleteWarning2020_Post2020()
		{
			var chargeCodes = new IncoTermChargeCodes();

			chargeCodes.IncoTerm = Incoterms.ExWorks;
			AssertNoWarnings(chargeCodes.IncoTermInfo);

			chargeCodes.IncoTerm = Incoterms.DeliveredAtTerminal;
			AssertHasWarning(chargeCodes.IncoTermInfo, "This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules.");

			chargeCodes.IncoTerm = Incoterms.CarriagePaidTo;
			AssertNoWarnings(chargeCodes.IncoTermInfo);
		}

		[TestDate(2019, 1, 1)]
		public void TestIncoTermsObsoleteWarning2020_Pre2020()
		{
			var chargeCodes = new IncoTermChargeCodes() { IncoTerm = Incoterms.DeliveredAtTerminal };

			AssertNoWarnings(chargeCodes.IncoTermInfo);
		}
	}
}
