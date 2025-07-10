using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class TranslatableDataTransformHelperTest : TransactionedTestCase
	{
		public void TestDelete()
		{
			using (ResourceStringsFactory.MockSources())
			{
				var eng = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English);
				eng.Put("k1", new ResourceStringData("k1", "ENG1"));
				eng.Put("k2", new ResourceStringData("k2", "ENG2"));
				eng.Put("k3", new ResourceStringData("k2", "ENG3"));
				ResourceStringsFactory.Save(EditReasons.Codes.TradosImport,
					new HelpDataString() { HD_Language = Core.SharedConstants.Languages.French, HD_Code = "k1", HD_Caption = "FRN1" },
					new HelpDataString() { HD_Language = Core.SharedConstants.Languages.French, HD_Code = "k2", HD_Caption = "FRN2" },
					new HelpDataString() { HD_Language = Core.SharedConstants.Languages.French, HD_Code = "k3", HD_Caption = "FRN3" },
					new HelpDataString() { HD_Language = Core.SharedConstants.Languages.ChineseSimplified, HD_Code = "k1", HD_Caption = "CHS1" },
					new HelpDataString() { HD_Language = Core.SharedConstants.Languages.ChineseSimplified, HD_Code = "k2", HD_Caption = "CHS2" },
					new HelpDataString() { HD_Language = Core.SharedConstants.Languages.ChineseSimplified, HD_Code = "k3", HD_Caption = "CHS3" },
					new HelpDataString() { HD_Language = Core.SharedConstants.Languages.German, HD_Code = "k1", HD_Caption = "GRM1" },
					new HelpDataString() { HD_Language = Core.SharedConstants.Languages.German, HD_Code = "k2", HD_Caption = "GRM2" },
					new HelpDataString() { HD_Language = Core.SharedConstants.Languages.German, HD_Code = "k3", HD_Caption = "GRM3" }
				);
				var helper = new TranslatableDataTransformHelper();
				helper.DeleteResourceString("k1", Core.SharedConstants.Languages.French);
				helper.DeleteResourceString("k2", Core.SharedConstants.Languages.ChineseSimplified);
				helper.DeleteResourceString("kx", Core.SharedConstants.Languages.ChineseSimplified);
				helper.DeleteResourceString("k2", Core.SharedConstants.Languages.German);
				helper.DeleteResourceString("k3", Core.SharedConstants.Languages.German);
				helper.SaveChanges();
				var checkedOut = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertContainsExactElementsInAnyOrder(new[] { "FRN2", "FRN3", "CHS1", "CHS3", "GRM1" }, checkedOut.Cast<HelpDataString>().Select(s => (string)s.HD_Caption));
			}
		}

		public void TestSaveDoesNotUseFactory()
		{
			ResourcesDeltaSource.UseDatabaseStrings.Value = true;
			var saveCount = BusinessObjectFactory.GlobalSaveCount;
			var helper = new TranslatableDataTransformHelper();
			helper.AddResourceString(new ResourceStringData(TranslatableDataTransformHelper.realTestString.ResourceKey, "chaine essai"), Core.SharedConstants.Languages.French);
			helper.SaveChanges();
			AssertEquals(saveCount, BusinessObjectFactory.GlobalSaveCount);
			var checkedOut = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertContainsExactElementsInAnyOrder(new[] { "chaine essai" }, new DatabaseResourceStringSource(Core.SharedConstants.Languages.French).ReadAll().Select(s => s.Caption));
		}
	}
}
