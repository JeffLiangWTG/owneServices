using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(EUOrgSupplierPartDataLoad))]
	public class EUOrgSupplierPartDataLoadTest : DataLoadTestCase<EUOrgSupplierPartDataLoad>
	{
		public void TestEUProductLoadLinksToPivot()
		{
			var owner = Factory.LoadTop1<OrgHeader>(new ZQuery());
			owner.OH_Code = "ORG01";
			Factory.Save();

			var dataLoad = new EUOrgSupplierPartDataLoad();
			using (var tempFile1 = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile1.Filename))
				{
					sw.WriteLine("Code,Description,Owner,       Tariff,ClassificationType,   ECSUPPLEMENT");
					sw.WriteLine("P001,Part1 desc, ORG01,             ,               HTB, 1111;2222;3333");
					sw.WriteLine("P002,Part2 desc, ORG01,1234.56 78 00,               HTI, 1111;2222;3333");
					sw.WriteLine("P003,Part3 desc, ORG01,1234.56 78   ,               HTI, 1111;2222;3333;4444;5555;6666;7777;8888;9999;0000;AAAA");
					sw.WriteLine("P004,Part4 desc, ORG01,1234.56 78   ,               HTE, 1111;0123456789012345678901234567890123456789");
				}

				dataLoad.ImportProductData(tempFile1.Filename, true, false);
			}

			var logText = string.Concat(dataLoad.Log.ToList());
			AssertContains("T O T A L : Products created = 4, Products updated = 0, Products excluded = 0", logText);

			var secondFactory = new BusinessObjectFactory();
			AssertEquals("Products Created", 4, dataLoad.RunCounters.RecsCreated);
			var p001Product = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P001"));
			var p002Product = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P002"));
			var p003Product = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P003"));
			var p004Product = secondFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P004"));

			AssertEquals("Part1 desc", p001Product.OP_Desc);
			AssertEquals("No tariff data given, so no pivot created", 0, p001Product.PivotsForBinding.Count);

			AssertEquals("Part2 desc", p002Product.OP_Desc);
			AssertEquals("1234567800", p002Product.PivotsForBinding[0].CI_TariffNum);
			AssertEquals("HTI", p002Product.PivotsForBinding[0].CI_ChildType);
			AssertEquals(3, p002Product.PivotsForBinding[0].SupplementaryCodes.Count());
			AssertEquals("1111", p002Product.PivotsForBinding[0].CI_Supplement1);
			AssertEquals("2222", p002Product.PivotsForBinding[0].CI_Supplement2);
			AssertEquals("3333", p002Product.PivotsForBinding[0].CI_AdditionalSupplements);

			AssertEquals("Part3 desc", p003Product.OP_Desc);
			AssertEquals("12345678", p003Product.PivotsForBinding[0].CI_TariffNum);
			AssertEquals("HTI", p003Product.PivotsForBinding[0].CI_ChildType);
			AssertEquals(10, p003Product.PivotsForBinding[0].SupplementaryCodes.Count());
			AssertEquals("1111", p003Product.PivotsForBinding[0].CI_Supplement1);
			AssertEquals("2222", p003Product.PivotsForBinding[0].CI_Supplement2);
			AssertEquals("The max number of supplementCodes is 10 if not overrided in EU countries", "3333,4444,5555,6666,7777,8888,9999,0000", p003Product.PivotsForBinding[0].CI_AdditionalSupplements);

			AssertEquals("Part4 desc", p004Product.OP_Desc);
			AssertEquals("12345678", p004Product.PivotsForBinding[0].CI_TariffNum);
			AssertEquals("HTE", p004Product.PivotsForBinding[0].CI_ChildType);
			AssertEquals(2, p004Product.PivotsForBinding[0].SupplementaryCodes.Count());
			AssertEquals("1111", p004Product.PivotsForBinding[0].CI_Supplement1);
			AssertEquals("The Length validation(only 4) is just a messsage error and Truncates when code > CY_CodeMaxLength as GB does", "012345678901234", p004Product.PivotsForBinding[0].CI_Supplement2);
		}

		protected override EUOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new EUOrgSupplierPartDataLoad();
		}
	}
}
