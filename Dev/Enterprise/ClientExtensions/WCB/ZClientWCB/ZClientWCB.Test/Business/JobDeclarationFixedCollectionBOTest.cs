using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.WCB.Testing
{
	[TestedType(typeof(JobDeclarationFixedCollection))]
	class JobDeclarationFixedCollectionBOTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			var baseDec = new BaseJobDeclarationCollection(Factory, Core.Constants.CountryCodes.Australia);
			return new JobDeclarationFixedCollection(baseDec);
		}
	}
}
