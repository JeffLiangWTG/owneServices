using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(InlandTransport))]
sealed class InlandTransportTest : Customs.Business.Testing.CusCodeDataTest<InlandTransport>
{
	public void TestCY_Code_MaxLength()
	{
		AssertEquals(2, inlandTransport.CY_CodeInfo.MaxLength);
	}

	public void TestCY_Data_MaxLength()
	{
		AssertEquals(27, inlandTransport.CY_DataInfo.MaxLength);
	}

	public void TestNationality_MaxLength()
	{
		AssertEquals(2, inlandTransport.NationalityInfo.MaxLength);
	}

	public void TestNationalityGetter()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Nationality empty by default", ZString.Empty, inlandTransport.Nationality);

			GenAddOnHelper.FindOrMakeNewAddOn(InlandTransport.Schema.Nationality, inlandTransport, out var nationalityColumn);
			nationalityColumn.XA_Data = Core.Constants.CountryCodes.Australia;
			AssertEquals("Nationality 'AU'", Core.Constants.CountryCodes.Australia, inlandTransport.Nationality);

			inlandTransport.Nationality = ZString.Empty;
			AssertEquals("Nationality after GenAddOnColumn deleted", ZString.Empty, inlandTransport.Nationality);
		});
	}

	public void TestNationalitySetter()
	{
		GenAddOnColumn nationalityColumn;
		CombineAssertions(() =>
		{
			inlandTransport.Nationality = Core.Constants.CountryCodes.Netherlands;
			GenAddOnHelper.Find(InlandTransport.Schema.Nationality, inlandTransport, out nationalityColumn);
			AssertEquals("Nationality set to 'DE'", Core.Constants.CountryCodes.Netherlands, nationalityColumn.XA_Data);

			inlandTransport.Nationality = Core.Constants.CountryCodes.Australia;
			GenAddOnHelper.Find(InlandTransport.Schema.Nationality, inlandTransport, out nationalityColumn);
			AssertEquals("Nationality set to 'AU'", Core.Constants.CountryCodes.Australia, nationalityColumn.XA_Data);

			inlandTransport.Nationality = ZString.Empty;
			GenAddOnHelper.Find(InlandTransport.Schema.Nationality, inlandTransport, out nationalityColumn);
			AssertNull("Nationality set to empty, GenAddOnColumn deleted", nationalityColumn);

			inlandTransport.Nationality = Core.Constants.CountryCodes.Italy;
			GenAddOnHelper.Find(InlandTransport.Schema.Nationality, inlandTransport, out nationalityColumn);
			AssertEquals("Nationality set to 'IT' after GenAddOnColumn deleted", Core.Constants.CountryCodes.Italy, nationalityColumn.XA_Data);
		});
	}

	public void TestCY_Order()
	{
		var inlandTransport2 = declaration.InlandTransports.AddNew();
		var inlandTransport3 = declaration.InlandTransports.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Order 1", (ZShort)1, inlandTransport.CY_Order);
			AssertEquals("Order 2", (ZShort)2, inlandTransport2.CY_Order);
			AssertEquals("Order 3", (ZShort)3, inlandTransport3.CY_Order);

			declaration.InlandTransports.Remove(inlandTransport2);
			AssertEquals("Order 1 stay same", (ZShort)1, inlandTransport.CY_Order);
			AssertEquals("Order 3 change to 2", (ZShort)2, inlandTransport3.CY_Order);
		});
	}

	public void TestISequenceNumberLine()
	{
		CombineAssertions(() =>
		{
			var sequenceLine = (IShortSequenceNumberLine)inlandTransport;
			AssertEquals("FKToHeader", declaration.PK, sequenceLine.FKToHeader);
			AssertEquals("SequenceNumber", (ZShort)1, sequenceLine.SequenceNumber);
		});
	}

	public void TestDefaultValues()
	{
		var transport = Factory.New<InlandTransport>();
		AssertEquals("CY_Type", EU.Business.CusCodeDataTypeList.Codes.TransportInland, transport.CY_Type);
	}

	public void TestHumanReadableNameCore()
	{
		var inlandTransport = (InlandTransport)GetNewBusinessObject();
		AssertEquals("Inland Transport", inlandTransport.HumanReadableName);
	}

	protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().InlandTransports.AddNew();

	protected override IEnumerable<InlandTransport> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return (InlandTransport)GetNewBusinessObjectForDeleteTest(factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
		return declaration.InlandTransports.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		inlandTransport = declaration.InlandTransports.AddNew();
	}
	InlandTransport inlandTransport;
	JobDeclaration declaration;
}
