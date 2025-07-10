using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(BorderTransport))]
class BorderTransportTest : Customs.Business.Testing.CusCodeDataTest<BorderTransport>
{
	public void TestHumanReadableName()
	{
		AssertEquals("Transport at border", borderTransport.HumanReadableName);
	}

	public void TestCY_Code_MaxLength()
	{
		AssertEquals(2, borderTransport.CY_CodeInfo.MaxLength);
	}

	public void TestCY_Data_MaxLength()
	{
		AssertEquals(35, borderTransport.CY_DataInfo.MaxLength);
	}

	public void TestNationality_MaxLength()
	{
		AssertEquals(2, borderTransport.NationalityInfo.MaxLength);
	}

	public void TestNationality()
	{
		borderTransport.Nationality = Core.Constants.CountryCodes.Netherlands;

		ZQuery addOnQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, borderTransport.PK);
		addOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusCodeDataSchema.Constants.Prefix);
		addOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, nameof(borderTransport.Nationality));
		addOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Type, AddOnColumnDataType.Codes.String);
		GenAddOnColumn addOn = Factory.LoadTop1<GenAddOnColumn>(addOnQuery);

		AssertEquals(Core.Constants.CountryCodes.Netherlands, addOn.XA_Data);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		return declaration.BorderTransports.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		borderTransport = declaration.BorderTransports.AddNew();
	}
	BorderTransport borderTransport;
	JobDeclaration declaration;
}
