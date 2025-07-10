using System.Linq;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class OutwardProcessingProviderTest : Customs.Business.Testing.DataProviderTestCase<OutwardProcessingProvider>
	{
		public void TestNew()
		{
			AssertNull(OutwardProcessingProvider.NewOrNull(null));
		}

		public void TestReimportCountries()
		{
			entryInstruction.ReimportCountryCodes.AddNew(Core.Constants.CountryCodes.Germany);
			entryInstruction.ReimportCountryCodes.AddNew(Core.Constants.CountryCodes.Italy);
			entryInstruction.ReimportCountryCodes.AddNew(Core.Constants.CountryCodes.Australia);
			AssertArrayEqualsByElements(new string[] { Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Italy }, dataProvider.ReimportCountries.ToArray());
		}

		public void TestIdentificationMeans()
		{
			AddIdentificationMeanCode(IdentificationMeansList.Codes.S, "Description1", 1);
			AddIdentificationMeanCode(IdentificationMeansList.Codes.A, "Description2", 3);
			AddIdentificationMeanCode(IdentificationMeansList.Codes.Z, "Description3", 2);
			AssertArrayEqualsByElements(new[] { "S, Description1", "Z, Description3", "A, Description2" },
				dataProvider.IdentificationMeans.Select(x => $"{x.Type}, {x.Description}").ToArray());

			void AddIdentificationMeanCode(string code, string data, short order)
			{
				var idMeansCode = entryInstruction.IdentificationMeanCodes.AddNew(code, data);
				idMeansCode.CY_Order = order;
			}
		}

		public void TestProducts()
		{
			AddProduct("123456", "Description1", 1);
			AddProduct("456789", "Description2", 3);
			AddProduct("222222", "Description3", 2);
			AssertArrayEqualsByElements(new[] { "123456, Description1", "222222, Description3", "456789, Description2" },
				dataProvider.Products.Select(x => $"{x.CommodityCode}, {x.GoodsDescription}").ToArray());

			void AddProduct(string code, string description, short lineNo)
			{
				var product = entryInstruction.Products.AddNew();
				product.CSI_Code = code;
				product.CSI_Description = description;
				product.CSI_LineNo = lineNo;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			dataProvider = OutwardProcessingProvider.NewOrNull(entryInstruction);
		}
		CusEntryInstruction entryInstruction;
		OutwardProcessingProvider dataProvider;

		protected override OutwardProcessingProvider GetProvider() => dataProvider;
	}
}
