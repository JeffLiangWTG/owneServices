using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.US.Testing
{
	[TestedType(typeof(ExportEntryFilerIDRegistryItem))]
	sealed class ExportEntryFilerIDRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<ExportEntryFilerID>
	{
		public void TestRegistryOptions()
		{
			AssertEquals("Options", RegistryOptions.Default, new ExportEntryFilerIDRegistryItem("DUMMY", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hello World").Options);
		}

		protected override StronglyTypedRegistryItem<ExportEntryFilerID, ExportEntryFilerID> GetNewRegistryItem()
		{
			return new ExportEntryFilerIDRegistryItem("", null, null, null);
		}

		protected override ExportEntryFilerID ValidValue
		{
			get
			{
				var filer = new ExportEntryFilerID();
				filer.EntryFilerID = "12-1234560";
				filer.EntryFilerIDType = AESEntryFilerIDTypeList.Codes.EmployerIdentificationNumber;
				return filer;
			}
		}
	}
}
