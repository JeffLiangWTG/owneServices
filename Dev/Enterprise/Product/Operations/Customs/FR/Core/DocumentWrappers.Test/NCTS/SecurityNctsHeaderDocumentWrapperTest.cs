using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.NCTS.Testing;

[TestedType(typeof(SecurityNctsHeaderDocumentWrapper))]
sealed class SecurityNctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestNew()
	{
		var header = Factory.New<NctsHeader>();

		AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => SecurityNctsHeaderDocumentWrapper.New(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => SecurityNctsHeaderDocumentWrapper.New(header, null));

		AssertNotNull("Instance of SecurityNctsHeaderDocumentWrapper expected", SecurityNctsHeaderDocumentWrapper.New(header, Factory));
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return SecurityNctsHeaderDocumentWrapper.New(Factory.New<NctsHeader>(), Factory);
	}

	public void TestLinesCollectionType()
	{
		var header = Factory.New<NctsHeader>();

		var wrapper = new SecurityNctsHeaderDocumentWrapperForTest(header);
		AssertType<NctsDepartureCargoDescWrapperCollection>("The returned lines collection is ESNctsDepartureCargoDescWrapperCollection", wrapper.GetLinesCore_Exposed());
	}

	class SecurityNctsHeaderDocumentWrapperForTest : SecurityNctsHeaderDocumentWrapper
	{
		public SecurityNctsHeaderDocumentWrapperForTest(NctsHeader nctsHeader) : base(nctsHeader, nctsHeader.Factory)
		{
		}

		public DocBaseWrapperCollection<Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper> GetLinesCore_Exposed() => base.GetLinesCore();
	}
}
