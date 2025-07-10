using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DocDeclaration))]
sealed class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
{
	protected override DocDeclaration CreateDeclarationWrapper(JobDeclaration declaration)
	{
		var result = DocDeclaration.New(declaration, Factory);
		((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>());
		result.SetReportNameForTesting("Report Name");
		return result;
	}

	public void TestDocCusEntryHeaderCollectionType()
	{
		AssertEquals(typeof(DocCusEntryHeaderCollection), DeclarationWrapper.RateEntryHeaders.GetType());
	}

	public void TestAirlineName()
	{
		var mockDeclaration = Factory.NewMoq<JobDeclaration>();
		mockDeclaration.Setup(m => m.AirlineName).Returns(new ZString("Test Airline"));
		DocDeclaration wrapper = DocDeclaration.New(mockDeclaration.Object, Factory);
		AssertEquals("Test Airline", wrapper.AirlineName);
	}

	public void TestMasterBillWithHeading()
	{
		Declaration.JE_TransportMode = ZString.Empty;
		Assert("MasterBill With Heading", DeclarationWrapper.MasterBillWithHeading.IsEmpty);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		Declaration.JE_MasterBill = "ABCDEFG";
		AssertEquals("MasterBillHeading", "MAWB ABCDEFG", DeclarationWrapper.MasterBillWithHeading);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		AssertEquals("MasterBillHeading", "OCEAN BILL OF LADING ABCDEFG", DeclarationWrapper.MasterBillWithHeading);
	}

	public void TestHouseBillWithHeading()
	{
		Declaration.JE_TransportMode = ZString.Empty;
		Assert("HouseBillHeading", DeclarationWrapper.HouseBillWithHeading.IsEmpty);

		Declaration.JE_HouseBill = "123456";
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		AssertEquals("HouseBillHeading", "HAWB 123456", DeclarationWrapper.HouseBillWithHeading);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		AssertEquals("HouseBillHeading", "HOUSE BILL OF LADING 123456", DeclarationWrapper.HouseBillWithHeading);
	}

	public void TestConsignee_ExporterWhenImport()
	{
		var importerOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
		importerOrg.OH_IsConsignee = true;

		Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
		Declaration.JE_OH_Importer = importerOrg.PK;

		AssertEquals(importerOrg.OH_FullName, DeclarationWrapper.Consignee_Exporter);
	}

	public void TestConsignee_ExporterWhenExport()
	{
		var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
		org.OH_IsConsignor = true;
		Declaration.JE_OH_Supplier = org.PK;
		Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;

		AssertEquals(org.OH_FullName, DeclarationWrapper.Consignee_Exporter);
	}

	#region Implementation

	protected override string TestingCountry
	{
		get { return Enterprise.Core.Constants.CountryCodes.UnitedArabEmirates; }
	}

	#endregion
}
