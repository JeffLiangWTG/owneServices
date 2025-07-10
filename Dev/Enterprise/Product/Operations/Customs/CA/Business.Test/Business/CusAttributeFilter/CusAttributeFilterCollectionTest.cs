using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusAttributeFilterCollection))]
	sealed class CusAttributeFilterCollectionTest : ActiveBusinessObjectCollectionTestCase<CusAttributeFilterCollection>
	{
		public void TestCorrectTypeCasting()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot1 = part.PivotsForBinding.AddNew();
				CusAttributeFilter attrib = null;
				AssertNoExceptionThrown(() => attrib = pivot1.Attributes1.AddNew());
				AssertEquals(typeof(CusAttributeFilter), attrib.GetType());

				AssertNoExceptionThrown(() => attrib = pivot1.Attributes1[0]);
				AssertEquals(typeof(CusAttributeFilter), attrib.GetType());

				CusAttributeFilter[] attribs = null;
				AssertNoExceptionThrown(() => attribs = pivot1.Attributes1.ToArray());
				AssertEquals(typeof(CusAttributeFilter[]), attribs.GetType());
			}
		}

		public void TestHasSameAttribute1()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			var attrib1 = pivot1.Attributes1.AddNew();
			attrib1.BG_AttributeValue1 = "1";
			var attrib2 = pivot1.Attributes1.AddNew();
			attrib2.BG_AttributeValue1 = "2";
			var pivot2 = part.PivotsForBinding.AddNew();
			var attrib3 = pivot2.Attributes1.AddNew();
			attrib3.BG_AttributeValue1 = "3";
			var attrib4 = pivot2.Attributes1.AddNew();
			attrib4.BG_AttributeValue1 = "1";
			AssertEquals(true, pivot1.Attributes1.HasSameValue1(attrib4));
			AssertEquals(false, pivot2.Attributes1.HasSameValue1(attrib4));
			attrib4.BG_AttributeValue1 = "3";
			AssertEquals(false, pivot1.Attributes1.HasSameValue1(attrib4));
			AssertEquals(true, pivot2.Attributes1.HasSameValue1(attrib4));
			attrib4.BG_AttributeValue1 = "4";
			AssertEquals(false, pivot1.Attributes1.HasSameValue1(attrib4));
			AssertEquals(false, pivot2.Attributes1.HasSameValue1(attrib4));
		}

		protected override CusAttributeFilterCollection GetCollectionToTest()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			return new CusAttributeFilterCollection(pivot, nameof(CusAttributeFilter.AttributeFilterName.AT1));
		}
	}
}
