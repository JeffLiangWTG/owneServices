using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(PkgUnitXmlCodeMappingsIncludingReferenceFiles))]
	sealed class PkgUnitXmlCodeMappingsIncludingReferenceFilesTest : PkgUnitXmlCodeMappingsTest
	{
		public void TestItemsAreIncluded()
		{
			var factory = new BusinessObjectFactory();

			RefPackType packType = factory.New<RefPackType>();
			packType.F3_Code = "XXX";
			packType.F3_Description = (NoResString)"XXX Desc";

			Assert(new PkgUnitXmlCodeMappingsIncludingReferenceFiles(factory).ContainsEnterpriseCode("XXX"));
		}

		#region Implementation

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(Core.Constants.PkgUnit) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		#endregion
	}
}
