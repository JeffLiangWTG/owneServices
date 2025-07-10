using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(CustomsSummaryController))]
sealed class CustomsSummaryControllerTest : Customs.Module.Testing.StatementControllerTest
{
	protected override ControllerID GetControllerID() => ControllerIDs.Customs.CH.CustomsSummary;

	public void TestConfiguration() => CombineAssertions(() =>
	{
		AssertEquals("ID", ControllerIDs.Customs.CH.CustomsSummary, Controller.ID);
		AssertEquals("ModuleID", ModuleIDs.Customs.CH.CustomsSummary, Controller.ModuleID);
		AssertEquals("TypeOfTopLevelBusinessObject", typeof(CustomsSummaryLine), Controller.TypeOfTopLevelBusinessObject);
	});

	public void TestSecurityCheckpoints() => CombineAssertions(() =>
	{
		AssertEquals("For New", Env.Security.None, Controller.CheckPointForNewExposedForTest);
		AssertEquals("For View", Env.Security.None, Controller.CheckPointForViewExposedForTest);
		AssertEquals("For Edit", Env.Security.None, Controller.CheckPointForEditExposedForTest);
		AssertEquals("For Delete", Env.Security.None, Controller.CheckPointForDeleteExposedForTest);
	});

	public override void TestDeleteForm()
	{
		Assert("Not Implemented", true);
	}

	public override void TestEditForm() => CombineAssertions(() =>
	{
		var currentBranch = GlbBranch.CurrentBranch;
		var otherBranch = CreateNewBranch();

		_ = CreateBusinessObjects("1234.1", e => CreateJobDeclaration(otherBranch, e));
		_ = CreateBusinessObjects("1234", e => CreateJobDeclaration(otherBranch, e));
		_ = CreateBusinessObjects("1234.1", e => CreateShipment(currentBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		var (summaryLine, declaration) = CreateBusinessObjects("1234.1", e => CreateJobDeclaration(currentBranch, e));
		_ = CreateBusinessObjects("1212.2", e => CreateJobDeclaration(currentBranch, e));
		AssertForm<GUI.JobDeclarationForm, JobDeclaration>(() => Controller.ShowEditForm(summaryLine), declaration);

		_ = CreateBusinessObjects("4321.1", e => CreateShipment(otherBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		_ = CreateBusinessObjects("4321", e => CreateShipment(otherBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		(summaryLine, var shipment) = CreateBusinessObjects("4321.1", e => CreateShipment(currentBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		_ = CreateBusinessObjects("2323.2", e => CreateShipment(currentBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		AssertForm<ShipmentForm, ForwardingShipment>(() => Controller.ShowEditForm(summaryLine), shipment);
	});

	public override void TestNewForm()
	{
		Assert("Not Implemented", true);
	}

	public override void TestViewForm() => CombineAssertions(() =>
	{
		var currentBranch = GlbBranch.CurrentBranch;
		var otherBranch = CreateNewBranch();

		_ = CreateBusinessObjects("1234.1", e => CreateJobDeclaration(otherBranch, e));
		_ = CreateBusinessObjects("1234", e => CreateJobDeclaration(otherBranch, e));
		_ = CreateBusinessObjects("1234.1", e => CreateShipment(currentBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		var (summaryLine, declaration) = CreateBusinessObjects("1234.1", e => CreateJobDeclaration(currentBranch, e));
		_ = CreateBusinessObjects("1212.2", e => CreateJobDeclaration(currentBranch, e));
		AssertForm<GUI.JobDeclarationForm, JobDeclaration>(() => Controller.ShowViewForm(summaryLine), declaration);

		_ = CreateBusinessObjects("4321.1", e => CreateShipment(otherBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		_ = CreateBusinessObjects("4321", e => CreateShipment(otherBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		(summaryLine, var shipment) = CreateBusinessObjects("4321.1", e => CreateShipment(currentBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		_ = CreateBusinessObjects("2323.2", e => CreateShipment(currentBranch, e, CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber));
		AssertForm<ShipmentForm, ForwardingShipment>(() => Controller.ShowViewForm(summaryLine), shipment);
	});

	public void TestMissingDeclarationOrShipment() => CombineAssertions(() =>
	{
		AssertNull("No Form appearing", Controller.ShowViewForm(CreateSummaryHeaderAndLinkedLine("1234.1")));
		AssertEquals("Appearing Error Message", "No declaration or shipment found for the selected entry.", UnitTestUserNotification.Instance.LastMessage.Text);
	});

	public override void TestGetOpenFormUrlslDoesNotHitDatabase()
	{
		Assert("Not Implemented", true);
	}

	GlbBranch CreateNewBranch()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		var branch = Factory.New<GlbBranch>();
		branch.GB_GC = company.PK;
		return branch;
	}

	(CustomsSummaryLine, TEntity) CreateBusinessObjects<TEntity>(string entryNum, Func<string, TEntity> createEntity)
	where TEntity : BusinessObject
	{
		var summaryLine = CreateSummaryHeaderAndLinkedLine(entryNum);
		var entity = createEntity(entryNum);
		Factory.Save();
		return (summaryLine, entity);
	}

	JobDeclaration CreateJobDeclaration(GlbBranch branch, string entryNum)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GB = branch.PK;
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		AddEntryNumToBusninessObject(entryHeader, entryNum, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		return declaration;
	}

	ForwardingShipment CreateShipment(GlbBranch branch, string entryNum, string entryNumType)
	{
		var shipment = Factory.New<ForwardingShipment>();
		var header = new JobHeader.Loader(shipment).TryCreateWithMutex();
		header.JH_GB = branch.PK;
		AddEntryNumToBusninessObject(shipment, entryNum, entryNumType);
		return shipment;
	}

	CusEntryNumber AddEntryNumToBusninessObject(BusinessObject parent, string entryNum, string entryNumType)
	{
		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_EntryNum = entryNum;
		entryNumber.CE_EntryType = entryNumType;
		entryNumber.Parent = parent;
		return entryNumber;
	}

	CustomsSummaryLine CreateSummaryHeaderAndLinkedLine(string entryNum)
	{
		var summaryHeader = Factory.New<CustomsSummaryHeader>();
		var summaryLine = Factory.New<CustomsSummaryLine>();
		summaryLine.B3_B2 = summaryHeader.PK;
		summaryLine.B3_EntryNum = entryNum;
		return summaryLine;
	}

	void AssertForm<TFormType, BusinessEntityType>(Func<IZForm> formFactory, BusinessObject expectedObject)
	{
		using var form = (ZForm)formFactory();
		AssertType<TFormType>("Form type", form);
		AssertType<BusinessEntityType>("BusinessEntity type", form.BusinessEntity);
		AssertEquals("BusinessEntity PK", expectedObject.PK, ((BusinessObject)form.BusinessEntity).PK);
	}
}
