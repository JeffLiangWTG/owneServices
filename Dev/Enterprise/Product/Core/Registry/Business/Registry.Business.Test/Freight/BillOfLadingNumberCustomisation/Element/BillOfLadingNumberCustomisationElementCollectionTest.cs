using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillOfLadingNumberCustomisationElementCollection))]
	sealed class BillOfLadingNumberCustomisationElementCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BillOfLadingNumberCustomisationElementCollection>
	{
		public void TestSerialisation()
		{
			var strategy1 = new CommonElementStrategy("key1", "name1", string.Empty, NumberCustomisationElementCategories.Standard, 1, null);
			var strategy2 = new CommonElementStrategy("key2", "name2", string.Empty, NumberCustomisationElementCategories.Standard, 1, null);
			var strategy3 = new CommonElementStrategy("key3", "name3", string.Empty, NumberCustomisationElementCategories.Standard, 1, null);

			BillOfLadingNumberCustomisation customisation1 = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElementCollection collection1 = new BillOfLadingNumberCustomisationElementCollection(customisation1);
			collection1.Add(new BillOfLadingNumberCustomisationElement(customisation1, strategy1));
			collection1.Add(new BillOfLadingNumberCustomisationElement(customisation1, strategy2));
			collection1.Add(new BillOfLadingNumberCustomisationElement(customisation1, strategy3));

			collection1[strategy1.Key].Include = true;
			collection1[strategy1.Key].Order = 1;
			collection1[strategy1.Key].Detail = "a";
			collection1[strategy1.Key].Fountain = true;

			collection1[strategy2.Key].Include = true;
			collection1[strategy2.Key].Order = 2;
			collection1[strategy2.Key].Detail = "";
			collection1[strategy2.Key].Fountain = false;
			collection1[strategy2.Key].CheckDigit = false;

			collection1[strategy3.Key].Include = false;

			string xml;

			using (System.IO.StringWriter stream = new System.IO.StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stream))
			{
				writer.WriteStartElement("Elements");
				collection1.WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				xml = stream.ToString();
			}

			const string expectedXml =
				"<Elements>" +
					"<Element key=\"key1\">" +
						"<Order>1</Order>" +
						"<Fountain>Y</Fountain>" +
						"<CheckDigit>Y</CheckDigit>" +
						"<Detail>a</Detail>" +
					"</Element>" +
					"<Element key=\"key2\">" +
						"<Order>2</Order>" +
						"<CheckDigit>N</CheckDigit>" +
					"</Element>" +
				"</Elements>" +
				"";

			AssertMultilineASCIIEquals("", expectedXml.Replace("><", ">\n<"), xml.Replace("><", ">\n<"));

			BillOfLadingNumberCustomisation customisation2 = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElementCollection collection2 = new BillOfLadingNumberCustomisationElementCollection(customisation2);
			collection2.Add(new BillOfLadingNumberCustomisationElement(customisation2, strategy1));
			collection2.Add(new BillOfLadingNumberCustomisationElement(customisation2, strategy2));
			collection2.Add(new BillOfLadingNumberCustomisationElement(customisation2, strategy3));

			using (System.IO.StringReader stream = new System.IO.StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				collection2.ReadXml(reader);
			}

			AssertEquals("collection1[strategy1.Key].Include", collection1[strategy1.Key].Include, collection2[strategy1.Key].Include);
			AssertEquals("collection1[strategy1.Key].Order", collection1[strategy1.Key].Order, collection2[strategy1.Key].Order);
			AssertEquals("collection1[strategy1.Key].Fountain", collection1[strategy1.Key].Fountain, collection2[strategy1.Key].Fountain);
			AssertEquals("collection1[strategy1.Key].CheckDigit", collection1[strategy1.Key].CheckDigit, collection2[strategy1.Key].CheckDigit);
			AssertEquals("collection1[strategy1.Key].Detail", collection1[strategy1.Key].Detail, collection2[strategy1.Key].Detail);

			AssertEquals("collection1[strategy2.Key].Include", collection1[strategy2.Key].Include, collection2[strategy2.Key].Include);
			AssertEquals("collection1[strategy2.Key].Order", collection1[strategy2.Key].Order, collection2[strategy2.Key].Order);
			AssertEquals("collection1[strategy2.Key].Fountain", collection1[strategy2.Key].Fountain, collection2[strategy2.Key].Fountain);
			AssertEquals("collection1[strategy2.Key].CheckDigit", collection1[strategy2.Key].CheckDigit, collection2[strategy2.Key].CheckDigit);
			AssertEquals("collection1[strategy2.Key].Detail", collection1[strategy2.Key].Detail, collection2[strategy2.Key].Detail);

			AssertEquals("collection1[strategy3.Key].Include", collection1[strategy3.Key].Include, collection2[strategy3.Key].Include);
			AssertEquals("collection1[strategy3.Key].Order", collection1[strategy3.Key].Order, collection2[strategy3.Key].Order);
			AssertEquals("collection1[strategy3.Key].Fountain", collection1[strategy3.Key].Fountain, collection2[strategy3.Key].Fountain);
			AssertEquals("collection1[strategy3.Key].CheckDigit", collection1[strategy3.Key].CheckDigit, collection2[strategy3.Key].CheckDigit);
			AssertEquals("collection1[strategy3.Key].Detail", collection1[strategy3.Key].Detail, collection2[strategy3.Key].Detail);
		}

		public void TestNewAndPopulate()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();

			AssertContainsExactElementsInAnyOrder(
				Array.ConvertAll(typeof(BillOfLadingNumberCustomisationElement.Keys).GetFields(), (FieldInfo x) => (string)x.GetValue(null)),
				Array.ConvertAll(BillOfLadingNumberCustomisationElementCollection.NewAndPopulate(customisation).ToArray(), (BusinessObject x) => (string)new ZString(x["Key"]))
				);
		}

		public void TestCommonElementsOverrideRexEx()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			var commonElements = BillOfLadingNumberCustomisationElementCollection
				.NewAndPopulate(customisation)
				.Where(x => x.Strategy is CommonElementStrategy)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				new[] {
					(BillOfLadingNumberCustomisationElement.Keys.BranchCode, GetCommonRegExForAnyValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.CarrierPrincipalCode, GetCommonRegExForAnyValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.CompanyCode, GetCommonRegExForAnyValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.ContainerTranshipmentIndicator, GetCommonRegExForAnyValue(1))
					, (BillOfLadingNumberCustomisationElement.Keys.DestinationIATA, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO, GetCommonRegExForAnyValue(5))
					, (BillOfLadingNumberCustomisationElement.Keys.Direction, "[IEDO]{1}")
					, (BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode, GetCommonRegExForAnyValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.JobNo, GetMaxToLengthRegExValue(20))
					, (BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, "[A-L]{1}")
					, (BillOfLadingNumberCustomisationElement.Keys.OriginIATA, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO, GetCommonRegExForAnyValue(5))
					, (BillOfLadingNumberCustomisationElement.Keys.ServerCode, GetCommonRegExForAnyValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.SundryChargesActivity, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.SundryChargesMode, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.SundryChargesType, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.TransportMode, "[ASRO]{1}")
					, (BillOfLadingNumberCustomisationElement.Keys.UniversalOfficeCode, GetMaxToLengthRegExValue(GetMaxUniversalOfficeCodeLength()))
					, (BillOfLadingNumberCustomisationElement.Keys.YearAsLetter, "[A-Z]{1}")
					, (BillOfLadingNumberCustomisationElement.Keys.LoadIATA, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.LoadUNLOCO, GetCommonRegExForAnyValue(5))
					, (BillOfLadingNumberCustomisationElement.Keys.FirstLoadIATA, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.FirstLoadUNLOCO, GetCommonRegExForAnyValue(5))
					, (BillOfLadingNumberCustomisationElement.Keys.DischargeIATA, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.DischargeUNLOCO, GetCommonRegExForAnyValue(5))
					, (BillOfLadingNumberCustomisationElement.Keys.LastDischargeIATA, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.LastDischargeUNLOCO, GetCommonRegExForAnyValue(5))
					, (BillOfLadingNumberCustomisationElement.Keys.ServiceLevel, GetMaxToLengthRegExValue(3))
					, (BillOfLadingNumberCustomisationElement.Keys.Quarter, "[1-4]{1}")
					, (BillOfLadingNumberCustomisationElement.Keys.ClientOrganisation, GetMaxToLengthRegExValue(12))
					, (BillOfLadingNumberCustomisationElement.Keys.WarehouseSubType, GetMaxToLengthRegExValue(3))
				}
				, commonElements.Select(x => (x.Key.ToString(), x.Strategy.GetRegExForDataType(x))));

			string GetCommonRegExForAnyValue(int length)
				=> $"[0-9a-zA-Z]{{{length}}}";

			string GetMaxToLengthRegExValue(int length)
				=> $"[0-9a-zA-Z]{{,{length}}}";
		}
		static int GetMaxUniversalOfficeCodeLength()
		{
			var filter = new ZDBOnlyQuery(ObjectFactory.GetType<IOrgCusCode>());
			filter.AddToFilter(OrgCusCodeSchema.OK_CodeType, "UOC");

			var orgProxiesQuery = new ZDBOnlyQuery(ObjectFactory.GetType<IOrgCusCode>());
			var companiesSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IGlbCompany>(), GlbCompanySchema.GC_OH_OrgProxy);
			var branchesSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IGlbBranch>(), GlbBranchSchema.GB_OH_OrgProxy);

			orgProxiesQuery.AddSubQuery(OrgCusCodeSchema.OK_OH, companiesSubQuery, JoinCondition.And);
			orgProxiesQuery.AddSubQuery(OrgCusCodeSchema.OK_OH, branchesSubQuery, JoinCondition.Or);

			filter.AddToFilter(orgProxiesQuery);

			var codes = new BusinessObjectFactory().Load<IOrgCusCode>(filter);
			if (!codes.Any())
			{
				return 0;
			}

			var maxLength = codes.Select(c => ((BusinessObject)c)[nameof(OrgCusCodeSchema.OK_CustomsRegNo)].ToString().Length).Max();
			return maxLength;
		}

		public void TestSortByOrder()
		{
			var customisation = new BillOfLadingNumberCustomisation();

			var element1 = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode];
			element1.Include = true;
			element1.Order = 1;

			var element2 = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode];
			element2.Include = true;
			element2.Order = 2;

			var element3 = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode];
			element3.Include = true;
			element3.Order = 3;

			Assert("precondition:", customisation.UnFilteredElements.Count > 3);

			customisation.UnFilteredElements.Sort(BillOfLadingNumberCustomisationElement.Schema.Order, ListSortDirection.Ascending);
			AssertEquals(element1, customisation.UnFilteredElements[0]);
			AssertEquals(element2, customisation.UnFilteredElements[1]);
			AssertEquals(element3, customisation.UnFilteredElements[2]);

			customisation.UnFilteredElements.Sort(BillOfLadingNumberCustomisationElement.Schema.Order, ListSortDirection.Descending);
			AssertEquals(element1, customisation.UnFilteredElements[customisation.UnFilteredElements.Count - 1]);
			AssertEquals(element2, customisation.UnFilteredElements[customisation.UnFilteredElements.Count - 2]);
			AssertEquals(element3, customisation.UnFilteredElements[customisation.UnFilteredElements.Count - 3]);
		}

		#region Implementation

		protected override BillOfLadingNumberCustomisationElementCollection GetCollectionToTest()
		{
			return new BillOfLadingNumberCustomisationElementCollection(new BillOfLadingNumberCustomisation());
		}

		byte itemsMade;
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BillOfLadingNumberCustomisationElement(new BillOfLadingNumberCustomisation(), new NullElementStrategy())
			{
				Order = itemsMade++
			};
		}

		#endregion
	}
}
