using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCommodityCollection))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI007:CustomisableDataTranslationRule", Justification = "Test code")]
	sealed class DocCommodityCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCommodityCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocCommodity.New(Factory, Core.Constants.CargoTypes.General);
		}

		protected override DocCommodityCollection GetCollectionToTest()
		{
			return new DocCommodityCollection(Factory);
		}

		public void TestCommodityCollection()
		{
			DocCommodityCollection coll = new DocCommodityCollection(Factory);
			AssertEquals("", coll.Description);
			AssertEquals("", coll.Code);
			AssertEquals(ZBool.False, coll.ContainsHazardous);

			DocCommodity gEN = DocCommodity.New(Factory, Core.Constants.CargoTypes.General);
			DocCommodity hAZ = DocCommodity.New(Factory, Core.Constants.CargoTypes.Hazardous);

			coll.Add(hAZ);
			coll.Add(gEN);

			var hazardous = Factory.LoadTop1<RefCommodityCode>(new ZQuery(Enterprise.ZArchitecture.Schema.RefCommodityCodeSchema.RH_Code, "HAZ"));
			var general = Factory.LoadTop1<RefCommodityCode>(new ZQuery(Enterprise.ZArchitecture.Schema.RefCommodityCodeSchema.RH_Code, "GEN"));

			AssertEquals(hazardous.RH_Description + ", " + general.RH_Description, coll.Description);
			AssertEquals("HAZ, GEN", coll.Code);
			Assert(coll.ContainsHazardous);

			coll.Remove(hAZ);
			AssertEquals(ZBool.False, coll.ContainsHazardous);
		}
	}
}
