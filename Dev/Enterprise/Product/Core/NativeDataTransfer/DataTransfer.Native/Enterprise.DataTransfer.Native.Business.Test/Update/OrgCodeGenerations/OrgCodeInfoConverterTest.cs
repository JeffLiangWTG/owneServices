using System;
using Enterprise.DataTransfer.Native.Common;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCodeGenerations
{
	public class OrgCodeInfoConverterTest : TransactionedTestCase
	{
		public void TestConvert()
		{
			var orgCodeInfo = converter.Convert(org);
			AssertEquals("Zayden Zubin Rakhsh Lola", orgCodeInfo.OH_FullName);
			AssertEquals("ENG", orgCodeInfo.OH_Language);
			AssertEquals("AUSYD", orgCodeInfo.UnlocoCode);
			AssertEquals("SYD", orgCodeInfo.IataCode);
			AssertEquals("AU", orgCodeInfo.CountryCode);
			AssertEquals("Sydney", orgCodeInfo.PortName);
			AssertEquals("Australia", orgCodeInfo.CountryName);
		}

		public void TestConvert_Entity_Is_Not_OrgHeader()
		{
			var definition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader");
			var dummy = new Entity(definition, new AncillaryImportServices());
			AssertExceptionThrown
				(
					typeof(ArgumentException),
					() => converter.Convert(dummy)
				);
		}

		public void TestConvert_UNLOCO_Is_Empty()
		{
			org.ParentCollection.RemoveAll();
			var orgCodeInfo = converter.Convert(org);
			AssertEquals(string.Empty, orgCodeInfo.UnlocoCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var sessionServices = new AncillaryImportServices();
			converter = new OrgCodeInfoConverter(TestUtil.Connection, sessionServices);
			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var unlocoDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.ClosestPort");
			org = new Entity(definition, sessionServices);
			org["FullName"] = "Zayden Zubin Rakhsh Lola";
			org["Language"] = "ENG";

			var port = new Entity(unlocoDefinition, sessionServices);
			port["Code"] = "AUSYD";

			org.ParentCollection.Add(port);
		}
		OrgCodeInfoConverter converter;
		Entity org;
	}
}
