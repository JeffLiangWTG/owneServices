using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.CusTempStorage.Testing;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.GUI;
using Enterprise.Customs.ES.GUI.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.TemporaryStorage.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants.PNTS;
using BuilderHelperTest = Enterprise.Customs.ES.Business.Testing.BuilderHelperTest;
using CusTempStorageRegHeader = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine;
using CusTempStorageRegLineCollection = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineCollection;
using CusTempStorageRegLineItemPivot = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineItemPivot;
using CusTempStorageRegLineTransaction = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransaction;
using TemporaryStorageHeader = Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

[TestedType(typeof(G5V1TemporaryStorageMessagesMenu))]
sealed class G5V1TemporaryStorageMessagesMenuTest : TestCaseWithFactory
{
	public void TestCreateMenuItems()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
				"&Send To Customs",
				"-",
				"TS Register Management",
				"View on Customs Website",
				"Update CSV Clearance",
				"Make G5 Reception",
				"Roll-back Inbound Transaction in TS",
				"-",
				"Into Temporary Storage",
				"View TS Register",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Select(x => x.Text));
		}
	}

	#region Refresh Menu

	public void TestRefreshMenu_G5PWithoutSUMCusEntryNumAndMessageStatusEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
				"View on Customs Website",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5PWithSUMCusEntryNumAndMessageStatusEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
				"View on Customs Website",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5PWithSUMCusEntryNumAndMessageStatusNotSNTAndNotEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.AMA_MessageStatus = "AAA";

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
				"View on Customs Website",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5PWithSUMCusEntryNumAndMessageStatusSNT()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.AMA_MessageStatus = LogicalStatusList.Codes.Sent;

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
				"&Send To Customs",
				"View on Customs Website",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5PWithNoSUMCusEntryNumAndMessageStatusSNT()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.AMA_MessageStatus = LogicalStatusList.Codes.Sent;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
				"&Send To Customs",
				"View on Customs Website",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5PWithCustomsStatusCLR()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
				"View on Customs Website",
				"-",
				"Into Temporary Storage",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5PWithCustomsStatusTSA()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
				"&Send To Customs",
				"View on Customs Website",
				"-",
				"Into Temporary Storage",
				"View TS Register"
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5PWithoutMRN()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5XWithSUMCusEntryNumAndMessageStatusEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
				"View on Customs Website",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5XWithCustomsStatusTSA()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
				"&Send To Customs",
				"View on Customs Website",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_TSMWithoutSUMCusEntryNumAndMessageStatusEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"-",
				"Into Temporary Storage",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_TSMWithSUMCusEntryNumAndMessageStatusEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"-",
				"Into Temporary Storage",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_TSMWithSUMCusEntryNumAndMessageStatusNotSNTAndNotEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.AMA_MessageStatus = "AAA";

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"-",
				"Into Temporary Storage",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_TSMWithSUMCusEntryNumAndMessageStatusSNT()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.AMA_MessageStatus = LogicalStatusList.Codes.Sent;

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_TSMWithNoSUMCusEntryNumAndMessageStatusSNT()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.AMA_MessageStatus = LogicalStatusList.Codes.Sent;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_TSMWithCustomStatusTSA()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
				"Roll-back Inbound Transaction in TS",
				"-",
				"Into Temporary Storage",
				"View TS Register",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_LAMWithSUMCusEntryNumAndMessageStatusEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"-",
				"Into Temporary Storage",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_LAMWithoutASYCusEntryNumAndMessageStatusEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"-",
				"Into Temporary Storage",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_LAMWithASYCusEntryNumAndMessageStatusEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"-",
				"Into Temporary Storage",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_LAMWithASYCusEntryNumAndMessageStatusNotSNTAndNotEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.AMA_MessageStatus = "AAA";

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"-",
				"Into Temporary Storage",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_LAMWithSUMCusEntryNumAndMessageStatusSNT()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.AMA_MessageStatus = LogicalStatusList.Codes.Sent;

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = tempStorage.PK;
		entryNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_LAMWithNoASYCusEntryNumAndMessageStatusSNT()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.AMA_MessageStatus = LogicalStatusList.Codes.Sent;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_LAMWithCustomStatusTSA()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
				"Roll-back Inbound Transaction in TS",
				"-",
				"Into Temporary Storage",
				"View TS Register",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5XWithCustomsStatusEmpty()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		tempStorage.CustomsStatus = ZString.Empty;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
				"View on Customs Website",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5XWithCustomsStatusCLR()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
				"View on Customs Website",
				"Make G5 Reception",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_CustomsStatusTUC()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.MRN = "AH3RRRRRRNNNNNNNN";
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
				"Update CSV Clearance",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_G5XWithoutMRN()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	#endregion

	public void TestViewOnCustomsWebsite()
	{
		var expectedMRN = "AH3RRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADDS-JDIT/CtrlG5Sede?op=detCab&mrn=" + expectedMRN;

		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			var viewOnCustomsWebsiteMenuItem = form.Menu.MenuItems.FindByText("View on Customs Website", true);
			AssertNotNull(viewOnCustomsWebsiteMenuItem);

			viewOnCustomsWebsiteMenuItem.PerformClick();
			AssertNullOrEmpty("No url was launched when tempStorage has no MRN", WebUrlLauncher.LastUrlLaunched);

			tempStorage.MRN = expectedMRN;
			viewOnCustomsWebsiteMenuItem.PerformClick();
			AssertEquals("The correct url has been launched when there is MRN and the Message Type is G5X", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	#region Into Temporary Storage

	#region Into Temporary Storage G5P

	public void TestIntoTemporaryStorage_G5P_SaveAndContinue()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Should have message asking to save the declaration before Into Temporary Storage",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestIntoTemporaryStorage_G5P_Mutex()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";

		tempStorage.Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				var provider = new G5V1TemporaryStorageMessagesMenu(form);
				_ = provider.Mutex.Lock();
				intoTempStorageMenuItem.PerformClick();
				provider.Mutex.Unlock();

				const string expectedMessage = "is already in the process of creating a Temporary Storage Register Header.\r\nYou should be able to access this option when the person has saved the record. Please try later.";

				AssertEquals(message: "Mutex already locked",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Location", UnitTestUserNotification.Instance.LastMessage.Text);

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Mutex not already locked",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_G5P_DestinationGoodsLocationExistsInPremises()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";

		tempStorage.Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();

				const string expectedMessage = "Temporary Storage Location (ES009999000002) does not exist in Maintain/Customs/Customs Files/Temporary Storage Premises module.";
				const string expectedMessage2 = "The associated Premises to the Temporary Storage Location (ES009999000002) has no active numbering configuration or it has no available numbers.";

				AssertEquals(message: "Goods Location doesn't exist in premises",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Location", UnitTestUserNotification.Instance.LastMessage.Text);

				var newFactory = new BusinessObjectFactory();
				var premises = newFactory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
				premises.SRP_Code = "COD";
				premises.SRP_Description = "Desc";
				var orgHeader = newFactory.New<OrgHeader>();
				orgHeader.OH_Code = "AH3";
				var orgAddress = newFactory.New<OrgAddress>();
				orgAddress.OA_OH = orgHeader.PK;
				orgAddress.OA_Address1 = "Address";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;
				premises.SRP_CustomsLocation = "ES009999000002";
				newFactory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Location exists in premises",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				AssertEquals(message: "Premises NumberProvider has not active wrapper",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

				var provider = premises.NumberProvider;
				_ = provider.CustomsNumbers.AddNew();
				var wrapper1 = provider.CustomsNumberWrappers[0];
				wrapper1.IsActive = true;
				wrapper1.SN_MinimumValue = 1;
				wrapper1.SN_Count = 1;
				newFactory.Save();
				_ = wrapper1.StmNums.GenerateNextCustomsNumber(newFactory);

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Premises NumberProvider has active wrapper without available numbers",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

				var otherFactory = new BusinessObjectFactory();
				var reloadedPremises = otherFactory.Load<EU.TemporaryStorage.Business.CusTempStorageRegPremises>(premises.PK);
				reloadedPremises.NumberProvider.CustomsNumberWrappers[0].IsActive = false;

				_ = provider.CustomsNumbers.AddNew();
				var wrapper2 = provider.CustomsNumberWrappers[1];
				wrapper2.IsActive = true;
				wrapper2.SN_MinimumValue = 1;
				wrapper2.SN_Count = 1;
				newFactory.Save();
				otherFactory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Premises NumberProvider has active wrapper with available numbers",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));
			});
		}
	}

	public void TestIntoTemporaryStorage_G5P_GuaranteeWithReferenceAndLiabilityAmount()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();

				const string expectedMessage = "To enter goods into the Temporary Storage, a liability amount for a related Guarantee must be supplied.";
				AssertEquals(message: "No guarantee declared (all fields empty)",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.Guarantee.PW_BondNumber = "AAA";
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "No liability amount declared in the guarantee (BondNumber is not empty), no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.Guarantee.PW_BondNumber = ZString.Empty;
				tempStorage.Guarantee.PW_BondAmount = 20m;
				tempStorage.Guarantee.PW_Override = true;
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "No reference declared in the guarantee (BondAmount is not empty)",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.Guarantee.PW_BondNumber = "AAA";
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee has liability amount declared",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_G5P_GuaranteNumberExistsAndIsValid()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "Guarantee Nº (GUARANTEEREF) does not exist in Maintain/Customs/Customs Files/Customs Guarantees module or is not valid.";
				AssertEquals(message: "Guarantee doesn't exist",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
				cusPermitHeader.CPH_Number = "GUARANTEEREF";
				cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
				cusPermitHeader.CPH_Type = "TST";
				cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee start date is in the future",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
				cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee end date is in the past",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee has no opening balance transaction",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
				transaction.CPL_TransactionType = "OBL";
				transaction.CPL_Reference = "REF";
				transaction.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee exists and is valid",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_G5P_AtLeastOneLineNotMissing()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var bill2 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = true;
		var item2 = bill2.PackedItems.AddNew();
		item2.IsMissing = true;
		var item3 = bill2.PackedItems.AddNew();
		item3.IsMissing = true;
		item3.API_GrossWeight = 1;
		item3.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		item3.PackagesPivot.AddPivotFor(pack1);

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "There are no good items available to enter the Temporary Storage.";
				AssertEquals(message: "Any Line no Missing doesn't exist in Items",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Lines", UnitTestUserNotification.Instance.LastMessage.Text);

				item2.IsMissing = false;
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Any Line no Missing exist in Items but has no packages",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Lines", UnitTestUserNotification.Instance.LastMessage.Text);

				item3.IsMissing = false;
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Any Line no Missing exists in Items",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_G5P_GoodsItemsAreNotInTemporaryStorage()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);
		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "99984000037";

		Factory.Save();

		var query = new ZQuery(AsycudaManifestHeaderSchema.PK, tempStorage.PK);

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				var temporaryStorageLoaded = Factory.LoadTop1<TemporaryStorageHeader>(query);
				const string expectedMessage = "Goods Items from Summary Declaration 99984000037 are already in the Temporary Storage.";
				AssertEquals(message: "Goods Items are already in Temporary Storage",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorageHeader.SRH_Reference = "DifferentReference";
				tempStorageHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				temporaryStorageLoaded = Factory.LoadTop1<TemporaryStorageHeader>(query);
				AssertEquals(message: "Goods Items are not already in Temporary Storage",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_G5P_ShowWarningIfMoreThanOnePackageLinkedToALine()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 2;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "DifferentReference";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "There are some Items with multiple package lines. For these packages, the Goods Item’s gross weight will be automatically apportioned on the Temporary Storage Register.";
				AssertEquals(message: "Warning is not shown when no lines has linked more than 1 pack",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.DsdtMrnNumber = "24ES00999880000383";
				item1.PackagesPivot.AddPivotFor(pack2);
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Warning is shown when one line has linked more than 1 pack",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_G5P_GuaranteeLiabilityAmountIsZero()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 2;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 0;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "DifferentReference";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessage = @"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)";

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guatentee's BondAmount is 0",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				guarantee.PW_BondAmount = 20;

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guatentee's BondAmount is not 0",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_G5P_Warnings()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 2;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 0;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "DifferentReference";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessageTSCreated = "Temporary Storage data created successfully.";
				const string expectedMessageBondAmountError = @"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)";

				ZFormModaliser.ShowDialogsInTest = false;
				UnitTestUserNotification.Instance.AddYesAnswer();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Nothing done when response is Yes (BondAmount 0)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageTSCreated));
				AssertEquals(message: "Guatentee's BondAmount is 0, response is Yes",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageBondAmountError));
				AssertEquals("Temporary Storage Customs Status was not changed", ZString.Empty, tempStorage.CustomsStatus);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Functionality is done correctly when response is No for both warnings",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageTSCreated));
				AssertEquals(message: "Guatentee's BondAmount is 0, response is No",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageBondAmountError));
				AssertEquals("Temporary Storage Customs Status changed", "TSA", tempStorage.CustomsStatus);
			});
		}
	}

	public void TestIntoTemporaryStorageClick_G5P()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			Factory.SetBulkTypeHelper();

			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_JobReference = "TS00000001";
			temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AH3";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var bill = temporaryStorageHeader.Bills.AddNew();
			temporaryStorageHeader.Bills.FirstOrDefault().ABL_OA_Consignee = orgAddress.PK;

			temporaryStorageHeader.DsdtMrnNumber = "24ES00999880000373";
			temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
			temporaryStorageHeader.ClearanceDate = new ZDateTime(2024, 01, 01, 02, 01, 00);
			temporaryStorageHeader.MRN = "1234567890";

			TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
			cusPermitHeader.CPH_Number = "GUARANTEEREF";
			cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
			cusPermitHeader.CPH_Type = "TST";
			cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = "OBL";
			transaction.CPL_Reference = "REF";

			var guarantee = temporaryStorageHeader.Guarantee;
			guarantee.PW_BondNumber = "GUARANTEEREF";
			guarantee.PW_BondAmount = 2000.0m;
			guarantee.PW_Override = true;
			guarantee.PW_RX_NKCurrency = "USD";

			var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
			premises.SRP_Code = "COD";
			premises.SRP_Description = "DESC";
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = "TSLoc";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;
			var provider = premises.NumberProvider;
			_ = provider.CustomsNumbers.AddNew();
			var wrapper1 = provider.CustomsNumberWrappers[0];
			wrapper1.IsActive = true;
			wrapper1.NumberPadding = 3;
			wrapper1.NumberPrefix = "PP";
			wrapper1.NumberSuffix = "SS";
			wrapper1.SN_MinimumValue = 10;
			wrapper1.SN_Count = 2;

			var item = bill.PackedItems.AddNew();
			item.SetValues(1, "111111", "CUSCODE", "Description", 6);

			var pack = bill.Packs.AddNew();
			pack.SetValues("BX", "marks", 5);
			item.PackagesPivot.AddPivotFor(pack);

			temporaryStorageHeader.Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					const string expectedMessage = "Temporary Storage data created successfully.";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);

					AssertEquals(message: "The data has been created",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

					var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99984000037");
					var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
					AssertEquals("regHeaders created", 1, regHeaders.Length);

					var regHeader = regHeaders[0];
					AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
					AssertEquals("regHeader.SRH_Reference", "99984000037", regHeader.SRH_Reference);
					AssertEquals("regHeader.SRH_InternalReference", "PP010SS", regHeader.SRH_InternalReference);
					AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
					AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
					AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.G5Reception, regHeader.SRH_PreviousReferenceType);
					AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
					AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
					AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);

					var regHeaderGuarantee = regHeader.Guarantee;
					AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
					AssertEquals("regHeaderGuarantee.PW_BondAmount", 2000.0m, regHeaderGuarantee.PW_BondAmount);
					AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
					AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

					var regLines = regHeader.CusTempStorageRegLines;

					AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
															new ZInt[] { 1 }, regLines.Select(x => x.SRL_LineNumber));
					AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
															{
															("BX", "marks", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
															}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

					AssertRegLineWithOnePivot(regLines, "marks", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 6.00000m, 5, 2000.0m, new ZDateTimeOffset(2024, 01, 01, 02, 01, 00), 1, "111111", "CUSCODE", "Description");

					var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
					AssertEquals("regLineTransactions created", 1, regLineTransactions.Length);

					var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
					AssertEquals("regLineItemPivots created", 1, regLineItemPivots.Length);

					var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
					AssertEquals("regLineItems created", 1, regLineItems.Length);

					AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
				});
			}
		}
	}

	public void TestIntoTemporaryStoragePremisesDeactivated_G5P() => AssertIntoTemporaryStoragePremisesDeactivated(G5MessageTypeCodeList.Codes.G5v1Reception);

	#endregion

	#region Into Temporary Storage TSM

	public void TestIntoTemporaryStorage_TSM_SaveAndContinue()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Should have message asking to save the declaration before Into Temporary Storage",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestIntoTemporaryStorage_TSM_NoDSDTNumber()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "When TSM and there is no DSDT Number there should be an error",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("To enter goods into the Temporary Storage, DSDT number must be supplied."));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				tempStorage.UnionGoods = true;
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "When TSM with UnionGoods and there is no DSDT Number there should not be an error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("To enter goods into the Temporary Storage, DSDT number must be supplied."));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_TSM_Mutex()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		tempStorage.Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				var provider = new G5V1TemporaryStorageMessagesMenu(form);
				_ = provider.Mutex.Lock();
				intoTempStorageMenuItem.PerformClick();
				provider.Mutex.Unlock();

				const string expectedMessage = "is already in the process of creating a Temporary Storage Register Header.\r\nYou should be able to access this option when the person has saved the record. Please try later.";

				AssertEquals(message: "Mutex already locked",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Location", UnitTestUserNotification.Instance.LastMessage.Text);

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Mutex not already locked",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_TSM_DestinationGoodsLocationExistsInPremises()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		tempStorage.Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();

				const string expectedMessage = "Temporary Storage Location (ES009999000002) does not exist in Maintain/Customs/Customs Files/Temporary Storage Premises module.";
				const string expectedMessage2 = "The associated Premises to the Temporary Storage Location (ES009999000002) has no active numbering configuration or it has no available numbers.";

				AssertEquals(message: "Goods Location doesn't exist in premises",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Location", UnitTestUserNotification.Instance.LastMessage.Text);

				var newFactory = new BusinessObjectFactory();
				var premises = newFactory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
				premises.SRP_Code = "COD";
				premises.SRP_Description = "Desc";
				var orgHeader = newFactory.New<OrgHeader>();
				orgHeader.OH_Code = "AH3";
				var orgAddress = newFactory.New<OrgAddress>();
				orgAddress.OA_OH = orgHeader.PK;
				orgAddress.OA_Address1 = "Address";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;
				premises.SRP_CustomsLocation = "ES009999000002";
				newFactory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Location exists in premises",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				AssertEquals(message: "Premises NumberProvider has not active wrapper",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

				var provider = premises.NumberProvider;
				_ = provider.CustomsNumbers.AddNew();
				var wrapper1 = provider.CustomsNumberWrappers[0];
				wrapper1.IsActive = true;
				wrapper1.SN_MinimumValue = 1;
				wrapper1.SN_Count = 1;
				newFactory.Save();
				_ = wrapper1.StmNums.GenerateNextCustomsNumber(newFactory);

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Premises NumberProvider has active wrapper without available numbers",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

				var otherFactory = new BusinessObjectFactory();
				var reloadedPremises = otherFactory.Load<EU.TemporaryStorage.Business.CusTempStorageRegPremises>(premises.PK);
				reloadedPremises.NumberProvider.CustomsNumberWrappers[0].IsActive = false;

				_ = provider.CustomsNumbers.AddNew();
				var wrapper2 = provider.CustomsNumberWrappers[1];
				wrapper2.IsActive = true;
				wrapper2.SN_MinimumValue = 1;
				wrapper2.SN_Count = 1;
				newFactory.Save();
				otherFactory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Premises NumberProvider has active wrapper with available numbers",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));
			});
		}
	}

	public void TestIntoTemporaryStorage_TSM_GuaranteeWithReferenceAndLiabilityAmount()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.DsdtMrnNumber = "24ES00999880000373";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();

				const string expectedMessage = "To enter goods into the Temporary Storage, a liability amount for a related Guarantee must be supplied.";
				AssertEquals(message: "No guarantee declared (all fields empty)",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.Guarantee.PW_BondNumber = "AAA";
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "No liability amount declared in the guarantee (BondNumber is not empty), no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.Guarantee.PW_BondNumber = ZString.Empty;
				tempStorage.Guarantee.PW_BondAmount = 20m;
				tempStorage.Guarantee.PW_Override = true;
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "No reference declared in the guarantee (BondAmount is not empty)",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.Guarantee.PW_BondNumber = "AAA";
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee has liability amount declared",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_TSMAndUnionGoods_GuaranteeWithReferenceAndLiabilityAmount()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.DsdtMrnNumber = "24ES00999880000373";
		tempStorage.UnionGoods = true;
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			intoTempStorageMenuItem.PerformClick();

			const string expectedMessage = "To enter goods into the Temporary Storage, a liability amount for a related Guarantee must be supplied.";
			AssertEquals(message: "Liability amount error should not be shown for TSM with UnionGoods",
							expected: false,
							actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		}
	}

	public void TestIntoTemporaryStorage_TSM_GuaranteNumberExistsAndIsValid()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.DsdtMrnNumber = "24ES00999880000373";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "Guarantee Nº (GUARANTEEREF) does not exist in Maintain/Customs/Customs Files/Customs Guarantees module or is not valid.";
				AssertEquals(message: "Guarantee doesn't exist",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
				cusPermitHeader.CPH_Number = "GUARANTEEREF";
				cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
				cusPermitHeader.CPH_Type = "TST";
				cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee start date is in the future",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
				cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee end date is in the past",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee has no opening balance transaction",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
				transaction.CPL_TransactionType = "OBL";
				transaction.CPL_Reference = "REF";
				transaction.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee exists and is valid",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_TSMAndUnionGoods_GuaranteNumberExistsAndIsValid()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.DsdtMrnNumber = "24ES00999880000373";
		tempStorage.UnionGoods = true;
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			intoTempStorageMenuItem.PerformClick();
			const string expectedMessage = "Guarantee Nº (GUARANTEEREF) does not exist in Maintain/Customs/Customs Files/Customs Guarantees module or is not valid.";
			AssertEquals(message: "Guarantee doesn't exist and valid is not applied to TSM with UnionGoods",
							expected: false,
							actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		}
	}

	public void TestIntoTemporaryStorage_TSM_AtLeastOneLineNotMissing()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.DsdtMrnNumber = "24ES00999880000373";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var bill2 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = true;
		var item2 = bill2.PackedItems.AddNew();
		item2.IsMissing = true;
		var item3 = bill2.PackedItems.AddNew();
		item3.IsMissing = true;
		item3.API_GrossWeight = 1;
		item3.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		item3.PackagesPivot.AddPivotFor(pack1);

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "There are no good items available to enter the Temporary Storage.";
				AssertEquals(message: "Any Line no Missing doesn't exist in Items",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Lines", UnitTestUserNotification.Instance.LastMessage.Text);

				item2.IsMissing = false;
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Any Line no Missing exist in Items but has no packages",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Lines", UnitTestUserNotification.Instance.LastMessage.Text);

				item3.IsMissing = false;
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Any Line no Missing exists in Items",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_TSM_GoodsItemsAreNotInTemporaryStorage()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);
		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "24ES00999880000373";

		Factory.Save();

		var query = new ZQuery(AsycudaManifestHeaderSchema.PK, tempStorage.PK);

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				var temporaryStorageLoaded = Factory.LoadTop1<TemporaryStorageHeader>(query);
				const string expectedMessage = "Goods Items from Summary Declaration 24ES00999880000373 are already in the Temporary Storage.";
				AssertEquals(message: "Goods Items are already in Temporary Storage",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorageHeader.SRH_Reference = "DifferentReference";
				tempStorageHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				temporaryStorageLoaded = Factory.LoadTop1<TemporaryStorageHeader>(query);
				AssertEquals(message: "Goods Items are not already in Temporary Storage",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_TSMAndUnionGoods_GoodsItemsAreNotInTemporaryStorage()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.UnionGoods = true;
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);
		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "24ES00999880000373";

		Factory.Save();

		var query = new ZQuery(AsycudaManifestHeaderSchema.PK, tempStorage.PK);

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			intoTempStorageMenuItem.PerformClick();
			var temporaryStorageLoaded = Factory.LoadTop1<TemporaryStorageHeader>(query);
			const string expectedMessage = "Goods Items from Summary Declaration 24ES00999880000373 are already in the Temporary Storage.";
			AssertEquals(message: "Goods Items are already in Temporary Storage is not applied to TSM with UnionGoods",
							expected: false,
							actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		}
	}

	public void TestIntoTemporaryStorage_TSM_ShowWarningIfMoreThanOnePackageLinkedToALine()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 2;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "DifferentReference";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "There are some Items with multiple package lines. For these packages, the Goods Item’s gross weight will be automatically apportioned on the Temporary Storage Register.";
				AssertEquals(message: "Warning is not shown when no lines has linked more than 1 pack",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.DsdtMrnNumber = "24ES00999880000383";
				item1.PackagesPivot.AddPivotFor(pack2);
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Warning is shown when one line has linked more than 1 pack",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_TSM_GuaranteeLiabilityAmountIsZero()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 2;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 0;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "DifferentReference";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessage = @"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)";

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guatentee's BondAmount is 0",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				guarantee.PW_BondAmount = 20;

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guatentee's BondAmount is not 0",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_TSMAndUnionGoods_Goods_GuaranteeLiabilityAmountIsZero()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.UnionGoods = true;
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 2;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 0;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "DifferentReference";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			const string expectedMessage = @"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)";

			intoTempStorageMenuItem.PerformClick();
			AssertEquals(message: "Guatentee's BondAmount is 0",
							expected: false,
							actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		}
	}

	public void TestIntoTemporaryStorage_TSM_Warnings()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 2;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 0;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "DifferentReference";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessageTSCreated = "Temporary Storage data created successfully.";
				const string expectedMessageBondAmountError = @"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)";

				ZFormModaliser.ShowDialogsInTest = false;
				UnitTestUserNotification.Instance.AddYesAnswer();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Nothing done when response is Yes (BondAmount 0)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageTSCreated));
				AssertEquals(message: "Guatentee's BondAmount is 0, response is Yes",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageBondAmountError));
				AssertEquals("Temporary Storage Customs Status was not changed", ZString.Empty, tempStorage.CustomsStatus);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Functionality is done correctly when response is No for both warnings",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageTSCreated));
				AssertEquals(message: "Guatentee's BondAmount is 0, response is No",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageBondAmountError));
				AssertEquals("Temporary Storage Customs Status changed", "TSA", tempStorage.CustomsStatus);
			});
		}
	}

	public void TestIntoTemporaryStorageClick_TSM()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			Factory.SetBulkTypeHelper();

			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_JobReference = "TS00000001";
			temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AH3";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var bill = temporaryStorageHeader.Bills.AddNew();
			temporaryStorageHeader.Bills.FirstOrDefault().ABL_OA_Consignee = orgAddress.PK;

			temporaryStorageHeader.DsdtMrnNumber = "24ES00999880000373";
			temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
			temporaryStorageHeader.MRN = "1234567890";

			TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
			cusPermitHeader.CPH_Number = "GUARANTEEREF";
			cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
			cusPermitHeader.CPH_Type = "TST";
			cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = "OBL";
			transaction.CPL_Reference = "REF";

			var guarantee = temporaryStorageHeader.Guarantee;
			guarantee.PW_BondNumber = "GUARANTEEREF";
			guarantee.PW_BondAmount = 2000.0m;
			guarantee.PW_Override = true;
			guarantee.PW_RX_NKCurrency = "USD";

			var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
			premises.SRP_Code = "COD";
			premises.SRP_Description = "DESC";
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = "TSLoc";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;
			var provider = premises.NumberProvider;
			_ = provider.CustomsNumbers.AddNew();
			var wrapper1 = provider.CustomsNumberWrappers[0];
			wrapper1.IsActive = true;
			wrapper1.NumberPadding = 3;
			wrapper1.NumberPrefix = "PP";
			wrapper1.NumberSuffix = "SS";
			wrapper1.SN_MinimumValue = 10;
			wrapper1.SN_Count = 2;

			var item = bill.PackedItems.AddNew();
			item.SetValues(1, "111111", "CUSCODE", "Description", 6);

			var pack = bill.Packs.AddNew();
			pack.SetValues("BX", "marks", 5);
			item.PackagesPivot.AddPivotFor(pack);

			temporaryStorageHeader.Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					const string expectedMessage = "Temporary Storage data created successfully.";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);

					AssertEquals(message: "The data has been created",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

					var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "24ES00999880000373");
					var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
					AssertEquals("regHeaders created", 1, regHeaders.Length);

					var regHeader = regHeaders[0];
					AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
					AssertEquals("regHeader.SRH_Reference", "24ES00999880000373", regHeader.SRH_Reference);
					AssertEquals("regHeader.SRH_InternalReference", "PP010SS", regHeader.SRH_InternalReference);
					AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
					AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
					AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.ManualEntries, regHeader.SRH_PreviousReferenceType);
					AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
					AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
					AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);

					var regHeaderGuarantee = regHeader.Guarantee;
					AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
					AssertEquals("regHeaderGuarantee.PW_BondAmount", 2000.0m, regHeaderGuarantee.PW_BondAmount);
					AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
					AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

					var regLines = regHeader.CusTempStorageRegLines;

					AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
															new ZInt[] { 1 }, regLines.Select(x => x.SRL_LineNumber));
					AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
															{
															("BX", "marks", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
															}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

					AssertRegLineWithOnePivot(regLines, "marks", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 6.00000m, 5, 2000.0m, new ZDateTimeOffset(2023, 01, 01, 02, 01, 00), 1, "111111", "CUSCODE", "Description");

					var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
					AssertEquals("regLineTransactions created", 1, regLineTransactions.Length);

					var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
					AssertEquals("regLineItemPivots created", 1, regLineItemPivots.Length);

					var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
					AssertEquals("regLineItems created", 1, regLineItems.Length);

					AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
				});
			}
		}
	}

	public void TestIntoTemporaryStorageClick_TSMAndUnionGoods()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			Factory.SetBulkTypeHelper();

			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_JobReference = "TS00000001";
			temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";
			temporaryStorageHeader.UnionGoods = true;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AH3";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var bill = temporaryStorageHeader.Bills.AddNew();
			temporaryStorageHeader.Bills.FirstOrDefault().ABL_OA_Consignee = orgAddress.PK;

			temporaryStorageHeader.DsdtMrnNumber = "24ES00999880000373";
			temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
			temporaryStorageHeader.MRN = "1234567890";

			TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
			cusPermitHeader.CPH_Number = "GUARANTEEREF";
			cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
			cusPermitHeader.CPH_Type = "TST";
			cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = "OBL";
			transaction.CPL_Reference = "REF";

			var guarantee = temporaryStorageHeader.Guarantee;
			guarantee.PW_BondNumber = "GUARANTEEREF";
			guarantee.PW_BondAmount = 2000.0m;
			guarantee.PW_Override = true;
			guarantee.PW_RX_NKCurrency = "USD";

			var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
			premises.SRP_Code = "COD";
			premises.SRP_Description = "DESC";
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = "TSLoc";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;
			var provider = premises.NumberProvider;
			_ = provider.CustomsNumbers.AddNew();
			var wrapper1 = provider.CustomsNumberWrappers[0];
			wrapper1.IsActive = true;
			wrapper1.NumberPadding = 3;
			wrapper1.NumberPrefix = "PP";
			wrapper1.NumberSuffix = "SS";
			wrapper1.SN_MinimumValue = 10;
			wrapper1.SN_Count = 2;

			var item = bill.PackedItems.AddNew();
			item.SetValues(1, "111111", "CUSCODE", "Description", 6);

			var pack = bill.Packs.AddNew();
			pack.SetValues("BX", "marks", 5);
			item.PackagesPivot.AddPivotFor(pack);

			temporaryStorageHeader.Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					const string expectedMessage = "Temporary Storage data created successfully.";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);

					AssertEquals(message: "The data has been created",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

					var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "1234567890");
					var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
					AssertEquals("regHeaders created", 1, regHeaders.Length);

					var regHeader = regHeaders[0];
					AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
					AssertEquals("regHeader.SRH_Reference", "1234567890", regHeader.SRH_Reference);
					AssertEquals("regHeader.SRH_InternalReference", "1234567890", regHeader.SRH_InternalReference);
					AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
					AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
					AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.ManualEntries, regHeader.SRH_PreviousReferenceType);
					AssertEquals("regHeader.SRH_PreviousReference", ZString.Empty, regHeader.SRH_PreviousReference);
					AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
					AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);

					var regHeaderGuarantee = regHeader.Guarantee;
					AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
					AssertEquals("regHeaderGuarantee.PW_BondAmount", 2000.0m, regHeaderGuarantee.PW_BondAmount);
					AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
					AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

					var regLines = regHeader.CusTempStorageRegLines;

					AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
															new ZInt[] { 1 }, regLines.Select(x => x.SRL_LineNumber));
					AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
															{
															("BX", "marks", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01))
															}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

					AssertRegLineWithOnePivot(regLines, "marks", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 6.00000m, 5, 0.0m, new ZDateTimeOffset(2023, 01, 01, 02, 01, 00), 1, "111111", "CUSCODE", "Description");

					var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
					AssertEquals("regLineTransactions created", 1, regLineTransactions.Length);

					var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
					AssertEquals("regLineItemPivots created", 1, regLineItemPivots.Length);

					var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
					AssertEquals("regLineItems created", 1, regLineItems.Length);

					AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
				});
			}
		}
	}

	public void TestIntoTemporaryStoragePremisesDeactivated_TSM() => AssertIntoTemporaryStoragePremisesDeactivated(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry);

	#endregion

	#region Into Temporary Storage LAM

	public void TestIntoTemporaryStorage_LAM_SaveAndContinue()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Should have message asking to save the declaration before Into Temporary Storage",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_NoEntryDate()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "When LAM and there is no Entry Date there should be an error",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("To enter goods into the LAME, Entry Date must be supplied."));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_Mutex()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);

		tempStorage.Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				var provider = new G5V1TemporaryStorageMessagesMenu(form);
				_ = provider.Mutex.Lock();
				intoTempStorageMenuItem.PerformClick();
				provider.Mutex.Unlock();

				const string expectedMessage = "is already in the process of creating a Temporary Storage Register Header.\r\nYou should be able to access this option when the person has saved the record. Please try later.";

				AssertEquals(message: "Mutex already locked",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Location", UnitTestUserNotification.Instance.LastMessage.Text);

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Mutex not already locked",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_DestinationGoodsLocationExistsInPremises()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);

		tempStorage.Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();

				const string expectedMessage = "Temporary Storage Location (ES009999000002) does not exist in Maintain/Customs/Customs Files/Temporary Storage Premises module.";
				const string expectedMessage2 = "The associated Premises to the Temporary Storage Location (ES009999000002) has no active numbering configuration or it has no available numbers.";

				AssertEquals(message: "Goods Location doesn't exist in premises",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Location", UnitTestUserNotification.Instance.LastMessage.Text);

				var newFactory = new BusinessObjectFactory();
				var premises = newFactory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
				premises.SRP_Type = "ADT";
				premises.SRP_Code = "COD";
				premises.SRP_Description = "Desc";
				var orgHeader = newFactory.New<OrgHeader>();
				orgHeader.OH_Code = "AH3";
				var orgAddress = newFactory.New<OrgAddress>();
				orgAddress.OA_OH = orgHeader.PK;
				orgAddress.OA_Address1 = "Address";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;
				premises.SRP_CustomsLocation = "ES009999000002";
				newFactory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Location doesn't exist in premises when premises exists but not LAM",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Location when premises exists but not LAM", UnitTestUserNotification.Instance.LastMessage.Text);

				premises.SRP_Type = "LAM";
				newFactory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Location exists in premises",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				AssertEquals(message: "Premises NumberProvider has not active wrapper",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

				var provider = premises.NumberProvider;
				_ = provider.CustomsNumbers.AddNew();
				var wrapper1 = provider.CustomsNumberWrappers[0];
				wrapper1.IsActive = true;
				wrapper1.SN_MinimumValue = 1;
				wrapper1.SN_Count = 1;
				newFactory.Save();
				_ = wrapper1.StmNums.GenerateNextCustomsNumber(newFactory);

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Premises NumberProvider has active wrapper without available numbers",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

				var otherFactory = new BusinessObjectFactory();
				var reloadedPremises = otherFactory.Load<EU.TemporaryStorage.Business.CusTempStorageRegPremises>(premises.PK);
				reloadedPremises.NumberProvider.CustomsNumberWrappers[0].IsActive = false;

				_ = provider.CustomsNumbers.AddNew();
				var wrapper2 = provider.CustomsNumberWrappers[1];
				wrapper2.IsActive = true;
				wrapper2.SN_MinimumValue = 1;
				wrapper2.SN_Count = 1;
				newFactory.Save();
				otherFactory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Premises NumberProvider has active wrapper with available numbers",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_GuaranteeWithReferenceAndLiabilityAmount()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "LAM";
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();

				const string expectedMessage = "To enter goods into the Temporary Storage, a liability amount for a related Guarantee must be supplied.";
				AssertEquals(message: "No guarantee declared (all fields empty), for LAM there is no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.Guarantee.PW_BondNumber = "AAA";
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "No liability amount declared in the guarantee (BondNumber is not empty), for LAM there is no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.Guarantee.PW_BondNumber = ZString.Empty;
				tempStorage.Guarantee.PW_BondAmount = 20m;
				tempStorage.Guarantee.PW_Override = true;
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "No reference declared in the guarantee (BondAmount is not empty), for LAM there is no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.Guarantee.PW_BondNumber = "AAA";
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee has liability amount declared",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_GuaranteNumberExistsAndIsValid()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "LAM";
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		guarantee.PW_Override = true;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "Guarantee Nº (GUARANTEEREF) does not exist in Maintain/Customs/Customs Files/Customs Guarantees module or is not valid.";
				AssertEquals(message: "Guarantee doesn't exist, for LAM there is no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
				cusPermitHeader.CPH_Number = "GUARANTEEREF";
				cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
				cusPermitHeader.CPH_Type = "TST";
				cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee start date is in the future, for LAM there is no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
				cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee end date is in the past, for LAM there is no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee has no opening balance transaction, for LAM there is no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
				transaction.CPL_TransactionType = "OBL";
				transaction.CPL_Reference = "REF";
				transaction.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee exists and is valid",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_AtLeastOneLineNotMissing()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "LAM";
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var bill1 = tempStorage.Bills.AddNew();
		var bill2 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = true;
		var item2 = bill2.PackedItems.AddNew();
		item2.IsMissing = true;
		var item3 = bill2.PackedItems.AddNew();
		item3.IsMissing = true;
		item3.API_GrossWeight = 1;
		item3.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		item3.PackagesPivot.AddPivotFor(pack1);

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "There are no good items available to enter the Temporary Storage.";
				AssertEquals(message: "Any Line no Missing doesn't exist in Items",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Lines", UnitTestUserNotification.Instance.LastMessage.Text);

				item2.IsMissing = false;
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Any Line no Missing exist in Items but has no packages",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Lines", UnitTestUserNotification.Instance.LastMessage.Text);

				item3.IsMissing = false;
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Any Line no Missing exists in Items",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_GoodsItemsAreNotInTemporaryStorage()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "LAM";
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);
		tempStorage.DsdtMrnNumber = "24ES00999880000373";
		tempStorage.EntryNumber = "24ES00999880000484";
		tempStorage.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "24ES00999880000484";

		Factory.Save();

		var query = new ZQuery(AsycudaManifestHeaderSchema.PK, tempStorage.PK);

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				var temporaryStorageLoaded = Factory.LoadTop1<TemporaryStorageHeader>(query);
				const string expectedMessage = "Goods Items for LAME Reception Certificate 24ES00999880000484 are already in the Temporary Storage.";
				AssertEquals(message: "Goods Items are already in Temporary Storage",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorageHeader.SRH_Reference = "DifferentReference";
				tempStorageHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				temporaryStorageLoaded = Factory.LoadTop1<TemporaryStorageHeader>(query);
				AssertEquals(message: "Goods Items are not already in Temporary Storage",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_OnlyOneLineForLAM()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "LAM";
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);
		var item2 = bill1.PackedItems.AddNew();

		tempStorage.DsdtMrnNumber = "24ES00999880000373";
		tempStorage.EntryNumber = "24ES00999880000484";
		tempStorage.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);

		Factory.Save();

		var query = new ZQuery(AsycudaManifestHeaderSchema.PK, tempStorage.PK);

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				var temporaryStorageLoaded = Factory.LoadTop1<TemporaryStorageHeader>(query);
				const string expectedMessage = "Only one Goods Item is allowed per LAME Reception Certificate. Please, create a different LAM record per Goods Item and try again.";
				AssertEquals(message: "Only one Goods Item is allowed for LAM and there are more",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				item2.Delete();
				tempStorage.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				temporaryStorageLoaded = Factory.LoadTop1<TemporaryStorageHeader>(query);
				AssertEquals(message: "There is only one Goods Item",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_ShowWarningIfMoreThanOnePackageLinkedToALine()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "LAM";
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 2;
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		tempStorage.DsdtMrnNumber = "24ES00999880000373";
		tempStorage.EntryNumber = "24ES00999880000484";
		tempStorage.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "DifferentReference";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "There are some Items with multiple package lines. For these packages, the Goods Item’s gross weight will be automatically apportioned on the Temporary Storage Register.";
				AssertEquals(message: "Warning is not shown when no lines has linked more than 1 pack",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorage.EntryNumber = "24ES00999880000494";
				item1.PackagesPivot.AddPivotFor(pack2);
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Warning is shown when one line has linked more than 1 pack",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_LAM_GuaranteeLiabilityAmountIsZero()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999000002";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 2;
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var guarantee = tempStorage.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 0;
		guarantee.PW_Override = true;
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		tempStorage.DsdtMrnNumber = "24ES00999880000373";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "DifferentReference";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessage = @"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)";

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guatentee's BondAmount is 0, for LAM there is no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				guarantee.PW_BondAmount = 20;

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guatentee's BondAmount is not 0",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorageClick_LAM()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			Factory.SetBulkTypeHelper();

			var (temporaryStorageHeader, premises) = CreateDataForTemporaryStorageLAM();

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					const string expectedMessage = "Temporary Storage data created successfully.";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);

					AssertEquals(message: "The data has been created",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

					AssertEquals("temporaryStorageHeader.EntryNumber is set correctly", "PP010SS", temporaryStorageHeader.EntryNumber);

					var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_AppCode, CusTempStorageRegHeader.ESAppCode);
					var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
					AssertEquals("regHeaders created", 1, regHeaders.Length);

					var regHeader = regHeaders[0];
					AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
					AssertEquals("regHeader.SRH_Reference", "PP010SS", regHeader.SRH_Reference);
					AssertEquals("regHeader.SRH_InternalReference", "PP010SS", regHeader.SRH_InternalReference);
					AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2024, 02, 02), regHeader.SRH_ArrivalDate);
					AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2024, 02, 02, 03, 03, 00), regHeader.SRH_PresentationDate);
					AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.LameEntries, regHeader.SRH_PreviousReferenceType);
					AssertEquals("regHeader.SRH_PreviousReference", ZString.Empty, regHeader.SRH_PreviousReference);
					AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
					AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);

					var regHeaderGuarantee = regHeader.Guarantee;
					AssertEquals("regHeaderGuarantee.PW_BondNumber", ZString.Empty, regHeaderGuarantee.PW_BondNumber);
					AssertEquals("regHeaderGuarantee.PW_BondAmount", ZDecimal.Zero, regHeaderGuarantee.PW_BondAmount);
					AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", ZString.Empty, regHeaderGuarantee.PW_RX_NKCurrency);
					AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", ZGuid.Empty, regHeaderGuarantee.PW_CPH_Guarantee);

					var regLines = regHeader.CusTempStorageRegLines;

					AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
															new ZInt[] { 1 }, regLines.Select(x => x.SRL_LineNumber));
					AssertContainsExactElementsInAnyOrder("regLines (SRL_PackageType, SRL_PackageMarks, SRL_GoodsOwnerIdentifier, SRL_GrossWeightUQ, SRL_CustomsStatus, SRL_UnionStatus, SRL_LimitDate)",
															new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
															{
															("BX", "marks", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty)
															}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

					AssertRegLineWithOnePivot(regLines, "marks", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, 6.00000m, 5, 0m, new ZDateTimeOffset(2024, 02, 02, 03, 03, 00), 1, "111111", "CUSCODE", "Description");

					var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
					AssertEquals("regLineTransactions created", 1, regLineTransactions.Length);

					var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
					AssertEquals("regLineItemPivots created", 1, regLineItemPivots.Length);

					var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
					AssertEquals("regLineItems created", 1, regLineItems.Length);

					AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
				});
			}
		}
	}

	public void TestIntoTemporaryStorageClick_LAM_EntryNumberNotEmpty()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			Factory.SetBulkTypeHelper();

			var (temporaryStorageHeader, premises) = CreateDataForTemporaryStorageLAM(true);

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					const string expectedMessage = "Temporary Storage data created successfully.";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);

					AssertEquals(message: "The data has been created",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

					AssertEquals("temporaryStorageHeader.EntryNumber not change", "24ES00999880000484", temporaryStorageHeader.EntryNumber);

					var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_AppCode, CusTempStorageRegHeader.ESAppCode);
					var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
					AssertEquals("regHeaders created", 1, regHeaders.Length);

					var regHeader = regHeaders[0];

					AssertEquals("regHeader.SRH_Reference is EntryNumber", "24ES00999880000484", regHeader.SRH_Reference);
					AssertEquals("regHeader.SRH_InternalReference is EntryNumber", "24ES00999880000484", regHeader.SRH_InternalReference);
				});
			}
		}
	}

	(TemporaryStorageHeader, EU.TemporaryStorage.Business.CusTempStorageRegPremises) CreateDataForTemporaryStorageLAM(bool hasEntryNumber = false)
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.OH_Code = "AH2";
		orgHeader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ES555555555", "ES");
		var orgAddress2 = Factory.New<OrgAddress>();
		orgAddress2.OA_OH = orgHeader2.PK;
		orgAddress2.OA_Address1 = "Address2";

		var bill = temporaryStorageHeader.Bills.AddNew();
		var firstBill = temporaryStorageHeader.Bills.FirstOrDefault();
		firstBill.ABL_OA_Consignee = orgAddress.PK;
		firstBill.ABL_OA_Shipper = orgAddress2.PK;

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999880000373";
		if (hasEntryNumber)
		{
			temporaryStorageHeader.EntryNumber = "24ES00999880000484";
		}
		temporaryStorageHeader.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "1234567890";

		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "LAM";
		premises.SRP_Code = "COD";
		premises.SRP_Description = "DESC";
		premises.SRP_CustomsLocation = "TSLoc";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.NumberPadding = 3;
		wrapper1.NumberPrefix = "PP";
		wrapper1.NumberSuffix = "SS";
		wrapper1.SN_MinimumValue = 10;
		wrapper1.SN_Count = 2;

		var item = bill.PackedItems.AddNew();
		item.SetValues(1, "111111", "CUSCODE", "Description", 6);

		var pack = bill.Packs.AddNew();
		pack.SetValues("BX", "marks", 5);
		item.PackagesPivot.AddPivotFor(pack);

		temporaryStorageHeader.Factory.Save();

		return (temporaryStorageHeader, premises);
	}

	public void TestIntoTemporaryStoragePremisesDeactivated_LAM() => AssertIntoTemporaryStoragePremisesDeactivated(G5MessageTypeCodeList.Codes.LameManualEntry);

	#endregion

	void AssertIntoTemporaryStoragePremisesDeactivated(string messageType)
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = messageType;
		tempStorage.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
		tempStorage.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var bill = tempStorage.Bills.AddNew();
		bill.ABL_OA_Consignee = orgAddress.PK;

		var item = bill.PackedItems.AddNew();
		item.SetValues(1, "111111", "CUSCODE", "Description", 6);

		var pack = bill.Packs.AddNew();
		pack.SetValues("BX", "marks", 5);
		item.PackagesPivot.AddPivotFor(pack);

		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		premises.SRP_IsActive = false;
		premises.SRP_CustomsLocation = "ES009999000002";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();

		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.NumberPadding = 3;
		wrapper1.NumberPrefix = "PP";
		wrapper1.NumberSuffix = "SS";
		wrapper1.SN_MinimumValue = 10;
		wrapper1.SN_Count = 2;

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();
			var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

			intoTempStorageMenuItem.PerformClick();
			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("CusTempStorageRegLineTransaction not created", 0, regLineTransactions.Length);
		}
	}

	void AssertRegLineWithOnePivot(CusTempStorageRegLineCollection regLines, ZString marks, ZString internalReferenteType, ZDecimal grossWeight, ZInt packageQty, ZDecimal bondAmount, ZDateTimeOffset transactionDate, ZInt itemNumber, ZString tariff, ZString cusCode, ZString description)
	{
		var regLine = regLines.First(x => x.SRL_PackageMarks == marks);
		var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Transactions created", 1, regLineTransactions.Length);
		AssertRegLineTransaction("regLine with marks " + marks + " Transactions[0]", regLineTransactions[0], internalReferenteType, grossWeight, packageQty, bondAmount, transactionDate);
		var regLinePivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Pivots created", 1, regLinePivots.Length);
		AssertRegLineItemPivotAndItem("regLine with marks " + marks + " Pivot1", regLinePivots[0], grossWeight, itemNumber, tariff, cusCode, description);
	}

	void AssertRegLineTransaction(ZString assertMessage, CusTempStorageRegLineTransaction transaction, ZString internalReferenteType, ZDecimal grossWeight, ZInt packageQty, ZDecimal bondAmount, ZDateTimeOffset expectedDate)
	{
		AssertEquals(assertMessage + ".SRT_GrossWeight", grossWeight, transaction.SRT_GrossWeight);
		AssertEquals(assertMessage + ".SRT_PackageQty", packageQty, transaction.SRT_PackageQty);
		AssertEquals(assertMessage + ".SRT_TransactionType", "OBL", transaction.SRT_TransactionType);
		AssertEquals(assertMessage + ".SRT_InternalReferenceNumber", "TS00000001", transaction.SRT_InternalReferenceNumber);
		AssertEquals(assertMessage + ".SRT_InternalReferenceType", internalReferenteType, transaction.SRT_InternalReferenceType);
		AssertEquals(assertMessage + ".SRT_TransactionDate", expectedDate, transaction.SRT_TransactionDate);
		AssertEquals(assertMessage + ".SRT_PhysicalInOutDate", expectedDate, transaction.SRT_PhysicalInOutDate);
		AssertEquals(assertMessage + ".SRT_BondAmount", bondAmount, transaction.SRT_BondAmount);
	}

	void AssertRegLineItemPivotAndItem(ZString assertMessage, EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot pivot, ZDecimal grossWeight, ZInt itemNumber, ZString tariff, ZString cusCode, ZString description)
	{
		AssertEquals(assertMessage + ".SRV_GrossWeight", grossWeight, pivot.SRV_GrossWeight);
		var regLineItem = pivot.RegLineItem;
		AssertEquals(assertMessage + ".RegLineItem.SRI_GoodsItemNumber", itemNumber, regLineItem.SRI_GoodsItemNumber);
		AssertEquals(assertMessage + ".RegLineItem.SRI_Tariff", tariff, regLineItem.SRI_Tariff);
		AssertEquals(assertMessage + ".RegLineItem.SRI_CusC4Number", cusCode, regLineItem.SRI_CusC4Number);
		AssertEquals(assertMessage + ".RegLineItem.SRI_GoodsDescription", description, regLineItem.SRI_GoodsDescription);
	}

	#endregion

	#region View TS Register

	public void TestViewTSRegisterOpenCusTempStorageRegHeaderWithMessageModeG5R() => runViewTSRegisterAssertions(G5MessageTypeCodeList.Codes.G5v1Reception, CusEntryNumberTypes.Spain.SummaryEntryNumber, "9999000002", "12345678901234567890", "78902234567");

	public void TestViewTSRegisterNoMatchingCusTempStorageRegHeaderWithMessageModeG5R() => runViewTSRegisterNoMatchingRegisterAssertions(G5MessageTypeCodeList.Codes.G5v1Reception, CusEntryNumberTypes.Spain.SummaryEntryNumber, "9999000002", "9999000003", "12345678901234567890", "88902234567", "78902234567");

	public void TestViewTSRegisterOpenCusTempStorageRegHeaderWithMessageModeTSM() => runViewTSRegisterAssertions(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, CusEntryNumberTypes.Spain.SummaryEntryNumber, "9999000002", "12345678901234567890");

	public void TestViewTSRegisterNoMatchingCusTempStorageRegHeaderWithMessageModeTSM() => runViewTSRegisterNoMatchingRegisterAssertions(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, CusEntryNumberTypes.Spain.SummaryEntryNumber, "9999000002", "9999000003", "12345678901234567890", "12345678901234567891");

	public void TestViewTSRegisterOpenCusTempStorageRegHeaderWithMessageModeLAM() => runViewTSRegisterAssertions(G5MessageTypeCodeList.Codes.LameManualEntry, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, "9999000002", "12345678901234567890");

	public void TestViewTSRegisterNoMatchingCusTempStorageRegHeaderWithMessageModeLAM() => runViewTSRegisterNoMatchingRegisterAssertions(G5MessageTypeCodeList.Codes.LameManualEntry, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, "9999000002", "9999000003", "12345678901234567890", "12345678901234567891");

	void runViewTSRegisterAssertions(ZString messageMode, ZString entryType, ZString authNumber, ZString entryNum, string formattedEntryNum = null) => CombineAssertions(() =>
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = messageMode;
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = authNumber;
		temporaryStorageHeader.AMA_GS_NKCustomsAgent = Staff.GS_Code;
		temporaryStorageHeader.AMA_CustomsProfile = BuilderHelperTest.CertificateName;
		temporaryStorageHeader.AMA_CustomsOffice = "ES009999";
		temporaryStorageHeader.AMA_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;

		if (entryType == CusEntryNumberTypes.ASYCUDA.AsycudaRegistration)
		{
			temporaryStorageHeader.EntryNumber = entryNum;
		}
		else
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = temporaryStorageHeader.PK;
			entryNumber.CE_ParentTable = temporaryStorageHeader.TableName;
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			entryNumber.CE_EntryNum = entryNum;
			entryNumber.CE_IssueDate = ZDate.BrettsBirthday;
			Factory.Save();
		}

		SetupCusTempStorageRegHeaderAndPremise(formattedEntryNum == null ? entryNum : formattedEntryNum, authNumber);
		using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
		{
			var viewOnCustomsWebsiteMenuItem = form.Menu.MenuItems.FindByText("View TS Register", true);
			AssertNotNull(viewOnCustomsWebsiteMenuItem);

			viewOnCustomsWebsiteMenuItem.PerformClick();
			AssertType<TempStorageRegisterForm>("The Temporary Storage Register Window is open", ZFormModaliser.LastFormShownDialogForTest);
		}
	});

	void runViewTSRegisterNoMatchingRegisterAssertions(ZString messageMode, ZString entryType, ZString authNumber, ZString wrongAuthNumber, ZString entryNum, ZString wrongEntryNum, string formattedEntryNum = null) => CombineAssertions(() =>
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = messageMode;
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = authNumber;
		temporaryStorageHeader.AMA_GS_NKCustomsAgent = Staff.GS_Code;
		temporaryStorageHeader.AMA_CustomsProfile = BuilderHelperTest.CertificateName;
		temporaryStorageHeader.AMA_CustomsOffice = "ES009999";

		if (entryType == CusEntryNumberTypes.ASYCUDA.AsycudaRegistration)
		{
			temporaryStorageHeader.EntryNumber = entryNum;
		}
		else
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = temporaryStorageHeader.PK;
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			entryNumber.CE_EntryNum = entryNum;
			entryNumber.CE_IssueDate = ZDate.BrettsBirthday;
		}

		SetupCusTempStorageRegHeaderAndPremise(wrongEntryNum, authNumber);
		using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
		{
			var viewOnCustomsWebsiteMenuItem = form.Menu.MenuItems.FindByText("View TS Register", true);
			AssertNotNull(viewOnCustomsWebsiteMenuItem);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			viewOnCustomsWebsiteMenuItem.PerformClick();
			ZString entryNumValueForMessage = formattedEntryNum != null ? formattedEntryNum : entryNum;
			AssertContains($"No entry in the Temporary Storage Register found for {entryNumValueForMessage} (TSD Number) in {authNumber} (Premise Location)", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	});

	void SetupCusTempStorageRegHeaderAndPremise(ZString regHeaderReference, ZString locationInPremises)
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		var premises1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises1.SRP_CustomsLocation = locationInPremises;
		premises1.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		regHeader.SRH_SRP_Premises = premises1.PK;
	}

	#endregion

	#region Guarantee Transaction

	#region Guarantee Transaction G5P

	[TestDate(2023, 01, 01, 02, 01, 0)]
	[RequiresSTA]
	public void TestIntoTemporaryStorageClick_G5P_Transaction()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.G5v1Reception);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);
					AssertNull("Warning was not display", UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.WasWarning));

					AssertTransactionIsCreated(temporaryStorageHeader, -50.0m, "99994234567", "G5P LRNTest. MRN: 24ES009998987654321", new ZDateTime(2024, 01, 01, 02, 01, 00));
				});
			}
		}
	}

	public void TestIntoTemporaryStorageClick_G5P_TransactionItemMIS()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.G5v1Reception, itemStatus: true);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);

					AssertEquals("With all Item MIS, no transaction is created", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
				});
			}
		}
	}

	[TestDate(2003, 02, 01, 0, 0, 0)]
	public void TestIntoTemporaryStorageClick_G5P_TransactionNoLiabilityAmount()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.G5v1Reception, liabilityAmount: 0.0m);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);

					AssertEquals("With Liability amount 0, no transaction is created", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
				});
			}
		}
	}

	[TestDate(2003, 02, 01, 0, 0, 0)]
	public void TestIntoTemporaryStorageClick_G5P_TransactionTransactionExists()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.G5v1Reception, addExistingTransaction: true);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] one transaction", 1, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionStatus == "CON" && x.CPL_Reference == "24ES00999912345678"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);

					AssertEquals("When there is a transaction with same reference, no transaction is created", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
				});
			}
		}
	}

	[TestDate(2003, 02, 01, 0, 0, 0)]
	public void TestIntoTemporaryStorageClick_G5P_TransactionWarning()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.G5v1Reception, liabilityAmount: 5000.5m);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);
					var warning = UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.WasWarning);
					AssertNotNull("Warning exists", warning);
					AssertEquals("Warning was display", "Guarantee Nº (Test1) has not enough remaining balance (1000.00) to create the guarantee transaction (5000.50). It will be created anyway.", warning?.Text);

					AssertTransactionIsCreated(temporaryStorageHeader, -5000.5m, "99994234567", "G5P LRNTest. MRN: 24ES009998987654321", new ZDateTime(2024, 01, 01, 02, 01, 00));
				});
			}
		}
	}

	#endregion

	#region Guarantee Transaction TSM

	public void TestIntoTemporaryStorageClick_TSM_Transaction()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);
					AssertNull("Warning was not display", UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.WasWarning));

					AssertTransactionIsCreated(temporaryStorageHeader, -50.0m, "24ES00999912345678", "TSM TS00000001. MRN: 24ES009998987654321", new ZDateTime(2023, 01, 01, 02, 01, 00));
				});
			}
		}
	}

	public void TestIntoTemporaryStorageClick_TSMAndUnionGoods_Transaction()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, isUnionGoods: true);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);
					AssertNull("Warning was not display", UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.WasWarning));

					AssertEquals("No new transaction is created", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
				});
			}
		}
	}

	[TestDate(2003, 02, 01, 0, 0, 0)]
	[RequiresSTA]
	public void TestIntoTemporaryStorageClick_TSM_TransactionNoLiabilityAmount()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, liabilityAmount: 0.0m);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);

					AssertEquals("With Liability amount 0, no transaction is created", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
				});
			}
		}
	}

	[TestDate(2003, 02, 01, 0, 0, 0)]
	public void TestIntoTemporaryStorageClick_TSM_TransactionTransactionExists()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, addExistingTransaction: true);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);
					AssertNull("Warning was not display", UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.WasWarning));

					AssertTransactionIsCreated(temporaryStorageHeader, -50.0m, "24ES00999912345678", "TSM TS00000001. MRN: 24ES009998987654321", new ZDateTime(2023, 01, 01, 02, 01, 00));
				});
			}
		}
	}

	[TestDate(2003, 02, 01, 0, 0, 0)]
	public void TestIntoTemporaryStorageClick_TSM_TransactionWarning()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var temporaryStorageHeader = CreateTemporaryStorageForGuaranteeTransaction(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, liabilityAmount: 5000.5m);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
			{
				form.Show();
				var intoTempStorageMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					temporaryStorageHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", temporaryStorageHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					intoTempStorageMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", temporaryStorageHeader.Factory);
					var warning = UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.WasWarning);
					AssertNotNull("Warning exists", warning);
					AssertEquals("Warning was display", "Guarantee Nº (Test1) has not enough remaining balance (1000.00) to create the guarantee transaction (5000.50). It will be created anyway.", warning?.Text);

					AssertTransactionIsCreated(temporaryStorageHeader, -5000.5m, "24ES00999912345678", "TSM TS00000001. MRN: 24ES009998987654321", new ZDateTime(2023, 01, 01, 02, 01, 00));
				});
			}
		}
	}

	#endregion

	void AssertTransactionIsCreated(TemporaryStorageHeader temporaryStorageHeader, ZDecimal expectedTranValue, ZString expectedReference, ZString comment, ZDateTime expectedDate)
	{
		var transactions = temporaryStorageHeader.Guarantee?.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == "TRA");
		var transaction = transactions.FirstOrDefault();
		AssertEquals("With at least one item not MIS and Liability Amount not 0, transaction is created", 1, transactions.Count());
		AssertEquals("Transaction Date", expectedDate, transaction.CPL_TransactionDate);
		AssertEquals("Transaction Type", "TRA", transaction.CPL_TransactionType);
		AssertEquals("Reference", expectedReference, transaction.CPL_Reference);
		AssertEquals("Value", expectedTranValue, transaction.CPL_TranValue);
		AssertEquals("Comment", comment, transaction.CPL_Comment);
		AssertEquals("Status", "CON", transaction.CPL_TransactionStatus);
	}

	TemporaryStorageHeader CreateTemporaryStorageForGuaranteeTransaction(ZString messageType, bool itemStatus = false, decimal liabilityAmount = 50.0m, bool addExistingTransaction = false, bool isUnionGoods = false)
	{
		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = messageType;
		temporaryStorageHeader.LRN = "LRNTest";
		temporaryStorageHeader.UnionGoods = isUnionGoods;

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999912345678";
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.ClearanceDate = new ZDateTime(2024, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "24ES009998987654321";
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = orgHeader.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;
		guarantee.CPH_Balance = 1000.0m;
		guarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
		guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		if (addExistingTransaction)
		{
			guarantee.AddTransaction("24ES00999912345678", ZString.Empty, ZString.Empty, ZString.Empty, -1, ZDecimal.Zero, transactionType: Customs.Business.PermitTransactionTypeList.Codes.ADJ, status: PermitTransactionStatusList.Codes.Confirmed, checkBursting: false);
		}

		var nctsGuarantee = temporaryStorageHeader.Guarantee;
		nctsGuarantee.PW_BondNumber = "Test1";
		nctsGuarantee.PW_Override = true;
		nctsGuarantee.PW_BondAmount = liabilityAmount;

		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "TSLoc";
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;

		var bill = temporaryStorageHeader.Bills.AddNew();

		var item1 = bill.PackedItems.AddNew();
		item1.SetValues(1, "11111", "CUSCODE1", "Description1", 11, itemStatus);

		var pack1 = bill.Packs.AddNew();
		pack1.SetValues("CT", "marks1", 2);
		item1.PackagesPivot.AddPivotFor(pack1);

		temporaryStorageHeader.Factory.Save();

		return temporaryStorageHeader;
	}

	#endregion

	#region Send to Customs

	[RequiresSTA]
	public void TestSendToCustomsCore_Validations()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			tempStorage.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing broker and certificate are needed", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after broker not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					tempStorage.AMA_GS_NKCustomsAgent = Staff.GS_Code;
					tempStorage.AMA_CustomsProfile = ZString.Empty;
					tempStorage.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate is empty", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					tempStorage.AMA_CustomsProfile = "INVALID";
					tempStorage.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared invalid", UnitTestUserNotification.Instance.LastMessage.Text);
					tempStorage.AMA_CustomsProfile = BuilderHelperTest.CertificateName;
					tempStorage.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared not authorized", UnitTestUserNotification.Instance.LastMessage.Text);
					tempStorage.Reload();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	public void TestSendToCustomsCore_PreSave()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_GS_NKCustomsAgent = Staff.GS_Code;
		tempStorage.AMA_CustomsProfile = BuilderHelperTest.CertificateName;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
				AssertNotNull(sendToCustomsMenuItem);

				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before Send Pre-Declaration", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestSendToCustomsCore()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempStorage.AMA_JobReference = "JobReference";
			tempStorage.AMA_GS_NKCustomsAgent = Staff.GS_Code;
			tempStorage.AMA_CustomsProfile = BuilderHelperTest.CertificateName;
			tempStorage.AMA_CustomsOffice = "ES009999";

			tempStorage.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					tempStorage.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", tempStorage.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", tempStorage.Factory);
					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, tempStorage.AMA_MessageStatus);
					var msg = tempStorage.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);
				});
			}
		}
	}

	public void TestSendToCustoms_EditMessageText()
	{
		using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempStorage.AMA_JobReference = "JobReference";
			tempStorage.AMA_GS_NKCustomsAgent = Staff.GS_Code;
			tempStorage.AMA_CustomsProfile = BuilderHelperTest.CertificateName;
			tempStorage.AMA_CustomsOffice = "ES009999";
			tempStorage.Factory.Save();

			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			using (var menu = new SendMenuForTest(form))
			{
				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var sendToCustomsMenuItem = menu.MenuItems.FindByText("&Send To Customs", true);

				CombineAssertions(() =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					tempStorage.Factory.Save();
					form.Refresh();

					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", tempStorage.Factory);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", tempStorage.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, tempStorage.AMA_MessageStatus);
					var msg = tempStorage.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertNotContains("New message's text has been edited, does not contain correct customs office", "ES009999", msg.EM_MessageText);
					AssertContains("New message's text has been edited, contains edited customs office", "FR001234", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Make G5 Reception
	public void TestMakeG5V1ReceptionClick_PreSave()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;
		tempStorage.MRN = "24ES009998987654321";

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var makeG5v1ReceptionMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Make G5 Reception");
				AssertNotNull(makeG5v1ReceptionMenuItem);

				makeG5v1ReceptionMenuItem.PerformClick();
				AssertEquals(message: "Should have message asking to save the declaration before Make G5 Reception",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestMakeG5V1ReceptionClick_ReceptionExists()
	{
		var messageError = "The 24ES009998987654321 MRN is already registered at JobReference2 G5 Reception.";

		var tempStorageExpedition = Factory.New<TemporaryStorageHeader>();
		tempStorageExpedition.AMA_JobReference = "JobReference";
		tempStorageExpedition.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		tempStorageExpedition.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;
		tempStorageExpedition.MRN = "24ES009998987654321";

		var tempStorageReception = Factory.New<TemporaryStorageHeader>();
		tempStorageReception.AMA_JobReference = "JobReference2";
		tempStorageReception.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempStorageReception.MRN = "24ES009998987654321";

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorageExpedition))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var makeG5v1ReceptionMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Make G5 Reception");
				AssertNotNull(makeG5v1ReceptionMenuItem);

				Factory.Save();
				TestHelper.CheckFactoryHasNoPendingChanges("Before sending", Factory);
				makeG5v1ReceptionMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After sending", Factory);

				AssertEquals(message: "Should have error message saying Reception already exists",
							expected: messageError,
							actual: UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestMakeG5V1ReceptionClick()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;
		tempStorage.MRN = "24ES009998987654321";

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var queryTempStorageReception = new ZQuery(AsycudaManifestHeaderSchema.AMA_MessageType, G5MessageTypeCodeList.Codes.G5v1Reception);
				var tempStorageReception = Factory.Load<TemporaryStorageHeader>(queryTempStorageReception);
				AssertEquals("Prereq: no reception exists in Factory", 0, tempStorageReception.Length);

				var makeG5v1ReceptionMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Make G5 Reception");
				AssertNotNull(makeG5v1ReceptionMenuItem);

				tempStorage.Factory.Save();
				TestHelper.CheckFactoryHasNoPendingChanges("Before sending", tempStorage.Factory);
				makeG5v1ReceptionMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After sending", tempStorage.Factory);

				tempStorageReception = Factory.Load<TemporaryStorageHeader>(queryTempStorageReception);
				AssertEquals("A new reception exists in Factory", 1, tempStorageReception.Length);
				AssertEquals("New reception has the correct MRN", "24ES009998987654321", tempStorageReception[0].MRN);

				var receptionForm = ZFormModaliser.LastFormShownDialogForTest;
				AssertType<G5V1TemporaryStorageForm>(receptionForm);
				AssertNotEquals("New reception form is not the same as the expefition form", form, receptionForm);

				AssertEquals("Reception Form has correct ControllerID", ControllerIDs.Customs.EU.UCC6TemporaryStorage, ((G5V1TemporaryStorageForm)receptionForm).ControllerID);
				ZFormUtilities.ReloadCurrentForm((G5V1TemporaryStorageForm)receptionForm);
				AssertNotContains("Reception Form can be reloaded without form type cannot be reloaded error", "This form type cannot be reloaded", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	#endregion

	#region Inventory Management

	#region G5X

	public void TestSendToCustoms_G5X_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods();

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005");

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWithout337doc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference");

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(setCorrectPremisesInHeader: false);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("TS00000001/ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs."));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertNotEquals("The message has not been created nor sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has not changed, AMA_MessageStatus", ZString.Empty, header.AMA_MessageStatus);
					AssertEquals("header has no messages", 0, header.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithoutRegLineItem()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(prevDocLineNo: 2);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing there is no line item associated", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again."));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertNotEquals("The message has not been created nor sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has not changed, AMA_MessageStatus", ZString.Empty, header.AMA_MessageStatus);
					AssertEquals("header has no messages", 0, header.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithVINError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing expected vin is not in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again."));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertNotEquals("The message has not been created nor sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has not changed, AMA_MessageStatus", ZString.Empty, header.AMA_MessageStatus);
					AssertEquals("header has no messages", 0, header.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithPackageError_NotBulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing there is not enough pack qty in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again."));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertNotEquals("The message has not been created nor sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has not changed, AMA_MessageStatus", ZString.Empty, header.AMA_MessageStatus);
					AssertEquals("header has no messages", 0, header.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithPackageError_Bulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing there is not enough pack qty in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again."));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertNotEquals("The message has not been created nor sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has not changed, AMA_MessageStatus", ZString.Empty, header.AMA_MessageStatus);
					AssertEquals("header has no messages", 0, header.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithGrossWeightError_AnswerYes()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					UnitTestUserNotification.Instance.AddYesAnswer();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing there is not enough gross weight in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 16.000000\n\nDo you want to cancel this declaration to check?"));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertNotEquals("The message has not been created nor sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has not changed, AMA_MessageStatus", ZString.Empty, header.AMA_MessageStatus);
					AssertEquals("header has no messages", 0, header.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithGrossWeightForVINsError_AnswerYes()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods();

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					UnitTestUserNotification.Instance.AddYesAnswer();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing there is not enough gross weight in temp storage (transaction OBL)", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001:\nGross weight 11 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?"));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertNotEquals("The message has not been created nor sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has not changed, AMA_MessageStatus", ZString.Empty, header.AMA_MessageStatus);
					AssertEquals("header has no messages", 0, header.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithGrossWeightError_AnswerNo()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing there is not enough gross weight in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 16.000000\n\nDo you want to cancel this declaration to check?"));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -5m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithGrossWeightForVINsError_AnswerNo()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods();

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing there is not enough gross weight in temp storage (transaction OBL)", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001:\nGross weight 11 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?"));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(prevDocReference: FormattedPrevDocReference, transactionGrossWeightForVINs: 11m);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -11m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: PrevDocReference, transactionGrossWeightForVINs: 11m);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -11m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	public void TestSendToCustoms_G5X_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(transactionGrossWeightForVINs: 11m);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Expedition, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -11m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	#endregion

	public void TestSendToCustoms_G5P_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_NothingIsDone()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods();
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var form = new G5V1TemporaryStorageForm(header))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("&Send To Customs", true);
					AssertNotNull(sendToCustomsMenuItem);

					header.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", header.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", header.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("Should not have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertEquals("The message has been created and sent", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Header message status has changed, AMA_MessageStatus", PNTSMessageStatusList.Codes.Sent, header.AMA_MessageStatus);
					var msg = header.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.G5v1Reception, msg.EM_MessageType);
					AssertContains("New message's text has not been edited, contains correct customs office", "ES009999", msg.EM_MessageText);
					AssertNotContains("New message's text has not been edited, does not contain edited customs office", "FR001234", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	const string EntryReference = "ES00001";
	const string LocationInEntry = "9999000002";
	const string PrevDocCode = "337";
	const string PrevDocReference = "24ES00999980001282";
	const string FormattedPrevDocReference = "99994000128";

	(TemporaryStorageHeader header, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine1, CusTempStorageRegLine regLine2, CusTempStorageRegLine regLine3)
		SetUpDataForForReserveTemporaryStorageGoods(bool shouldAddDoc = true, bool setCorrectPremisesInHeader = true, string prevDocCode = PrevDocCode, string prevDocReference = PrevDocReference, int prevDocLineNo = 1,
		string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry, string regHeaderReference = FormattedPrevDocReference, string packageVin = "VIN1", int packageQtyNotBulk = 9, decimal transactionGrossWeight = 40m, string bulkPackageTypeForRegLine = "VG", decimal transactionGrossWeightForVINs = 5m)
	{
		Factory.SetBulkTypeHelper();

		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		temporaryStorageHeader.GoodsLocation.Address.AuthorisationNumber = locationInEntry;
		temporaryStorageHeader.LRN = EntryReference;
		temporaryStorageHeader.AMA_GS_NKCustomsAgent = Staff.GS_Code;
		temporaryStorageHeader.AMA_CustomsProfile = BuilderHelperTest.CertificateName;
		temporaryStorageHeader.AMA_CustomsOffice = "ES009999";

		temporaryStorageHeader.Bills.RemoveAndDeleteAll();
		var bill1 = temporaryStorageHeader.Bills.AddNew();

		var packedItem1 = bill1.PackedItems.AddNew();
		packedItem1.API_GrossWeight = 11m;
		packedItem1.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

		var vehicle1 = bill1.Packs.AddNew();
		vehicle1.SetValues("FR", "VIN1", 5);
		var linkVehicle1 = packedItem1.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == vehicle1);
		linkVehicle1.IsLinked = true;

		var packedItem2 = bill1.PackedItems.AddNew();
		packedItem2.API_GrossWeight = 11m;
		packedItem2.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

		var package1 = bill1.Packs.AddNew();
		package1.SetValues("BX", "marks", 9);
		var linkPackage1 = packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package1);
		linkPackage1.IsLinked = true;

		var package2 = bill1.Packs.AddNew();
		package2.SetValues("VG", "bulk gas marks", 0);
		var linkPackage2 = packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package2);
		linkPackage2.IsLinked = true;

		if (shouldAddDoc)
		{
			var previousDoc1 = packedItem1.PreviousDocuments.AddNew();
			previousDoc1.CSI_Code = prevDocCode;
			previousDoc1.CSI_ReferenceNumber = prevDocReference;
			previousDoc1.CSI_LineNo = prevDocLineNo;

			var previousDoc2 = packedItem2.PreviousDocuments.AddNew();
			previousDoc2.CSI_Code = prevDocCode;
			previousDoc2.CSI_ReferenceNumber = prevDocReference;
			previousDoc2.CSI_LineNo = prevDocLineNo;
		}

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		var premises1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises1.SRP_Type = "ADT";
		premises1.SRP_CustomsLocation = locationInPremises;
		premises1.SRP_Code = "X";
		premises1.SRP_Description = "DESC";
		premises1.SRP_OA_PremisesAddress = orgAddress.PK;
		var premises2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises2.SRP_Type = "ADT";
		premises2.SRP_CustomsLocation = "9999000005";
		premises2.SRP_Code = "A";
		premises2.SRP_Description = "DESC2";
		premises2.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		regHeader.SRH_SRP_Premises = setCorrectPremisesInHeader ? premises1.PK : premises2.PK;
		var regLine1 = Factory.New<CusTempStorageRegLine>();
		regLine1.SRL_LineNumber = 1;
		regLine1.SRL_CustomsStatus = "OPN";
		regLine1.SRL_PackageType = "FR";
		regLine1.SRL_PackageMarks = packageVin;
		regLine1.SRL_SRH = regHeader.PK;
		var regLineTransactionPND1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransactionPND1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransactionPND1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransactionPND1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransactionPND1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		var regLineTransaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		regLineTransaction1.SRT_PackageQty = 10;
		regLineTransaction1.SRT_GrossWeight = transactionGrossWeightForVINs;

		var regLine2 = Factory.New<CusTempStorageRegLine>();
		regLine2.SRL_LineNumber = 2;
		regLine2.SRL_CustomsStatus = "OPN";
		regLine2.SRL_PackageType = bulkPackageTypeForRegLine;
		regLine2.SRL_SRH = regHeader.PK;
		var regLineTransaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction2.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		regLineTransaction2.SRT_PackageQty = 10;
		regLineTransaction2.SRT_GrossWeight = transactionGrossWeight;

		var regLine3 = Factory.New<CusTempStorageRegLine>();
		regLine3.SRL_LineNumber = 3;
		regLine3.SRL_CustomsStatus = "OPN";
		regLine3.SRL_PackageType = "BX";
		regLine3.SRL_SRH = regHeader.PK;
		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AAAAA";
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		regLineTransaction3.SRT_PackageQty = packageQtyNotBulk;
		regLineTransaction3.SRT_GrossWeight = 6m;

		var regLineItem = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = 1;

		var regLineItemPivot1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

		var regLineItemPivot2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot2.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

		var regLineItemPivot3 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot3.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

		return (temporaryStorageHeader, regLineTransactionPND1, regLine1, regLine2, regLine3);
	}

	void AssertNewTransactionToReserveGoods(CusTempStorageRegLine regLine, ZInt expectedPackQty, ZDecimal expectedGrossWeight, string expectedComment = "")
	{
		var lineNum = regLine.SRL_LineNumber;
		var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_GrossWeight == expectedGrossWeight);
		AssertNotNull("Line " + lineNum + " has new transaction", transaction);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, transaction.SRT_InternalReferenceType);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_PackageQty correct", expectedPackQty, transaction.SRT_PackageQty);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_GrossWeight correct", expectedGrossWeight, transaction.SRT_GrossWeight);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_Comments correct", expectedComment, transaction.SRT_Comments);
	}

	#endregion

	#region Update CSV

	public void TestNotificationOnUpdateCSVClearanceG5X()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		temporaryStorageHeader.CustomsStatus = CustomsStatus.UnderControl;
		using (var form = new G5V1TemporaryStorageForm(temporaryStorageHeader))
		using (var userControl = new MessageUserControlForTesting(form))
		{
			form.Show();
			ZFormModaliser.ShowDialogsInTest = false;
			var updateCSVClearanceMenuItem = userControl.MenuItems.FindByText("Update CSV Clearance");
			CombineAssertions(() =>
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, temporaryStorageHeader.ClearanceNumber);
				AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, temporaryStorageHeader.ClearanceDate);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				userControl.csvCode = "+--**__aa/#€&@";
				userControl.clearanceDate = ZDateTime.Invalid;
				var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, temporaryStorageHeader.ClearanceNumber);
				AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, temporaryStorageHeader.ClearanceDate);
				AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				userControl.csvCode = "CLEARANCE1234567";
				userControl.clearanceDate = ZDateTime.Empty;
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, temporaryStorageHeader.ClearanceNumber);
				AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, temporaryStorageHeader.ClearanceDate);
				AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				userControl.csvCode = "CLEARANCE1234567";
				userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("CustomsStatus change to CLR", EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance, temporaryStorageHeader.CustomsStatus);
				AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", temporaryStorageHeader.ClearanceNumber);
				AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), temporaryStorageHeader.ClearanceDate);
				AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code|TYP=CSV", temporaryStorageHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
				AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date|TYP=CSV", temporaryStorageHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				userControl.csvCode = "AAAAAAAAAAAAAAAA";
				userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", temporaryStorageHeader.ClearanceNumber);
				AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), temporaryStorageHeader.ClearanceDate);
				AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date|TYP=CSV", temporaryStorageHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
			});
		}
	}

	class MessageUserControlForTesting : G5V1TemporaryStorageMessagesMenu
	{
		public ZString csvCode = ZString.Empty;
		public ZDateTime clearanceDate = ZDateTime.Empty;

		public MessageUserControlForTesting(ZForm parentForm) : base(parentForm)
		{
		}

		protected override UpdateCSVClearanceForm GetUpdateCSVClearanceForm(CsvCodeInfo csvCodeInfo)
			=> new UpdateCSVClearanceFormForTesting(csvCodeInfo, csvCode, clearanceDate);
	}

	class UpdateCSVClearanceFormForTesting : UpdateCSVClearanceForm
	{
		public UpdateCSVClearanceFormForTesting(CsvCodeInfo csvCodeInfo, ZString csvCode, ZDateTime clearanceDate)
			: base(csvCodeInfo)
		{
			CSVClearance.Text = csvCode;
			ClearanceDate.DateTimeValue = clearanceDate;
			csvCodeInfo.CsvCodeFromUser = csvCode;
			csvCodeInfo.ClearanceDateFromUser = clearanceDate;
		}
	}
	#endregion

	#region Rollback Inbound Transaction

	public void TestRollbackInboundTransactionClick_CusTempStorageRefHeaderExists()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";

		CreateRegHeader();

		var testCases = new[]
		{
			(messageType: G5MessageTypeCodeList.Codes.LameManualEntry, entryNumber: "", hasError: true, assertMessageText: "LAME no entryNumber"),
			(messageType: G5MessageTypeCodeList.Codes.LameManualEntry, entryNumber: "RegHeaderReference", hasError: false, assertMessageText: "LAME"),
			(messageType: G5MessageTypeCodeList.Codes.LameManualEntry, entryNumber: "NewReference", hasError: true, assertMessageText: "LAME"),
			(messageType: G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, entryNumber: "", hasError: true, assertMessageText: "TSM no entryNumber"),
			(messageType: G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, entryNumber: "RegHeaderReference", hasError: false, assertMessageText: "TSM"),
			(messageType: G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, entryNumber: "NewReference", hasError: true, assertMessageText: "TSM"),
		};

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			foreach (var (messageType, entryNumber, hasError, assertMessageText) in testCases)
			{
				var expectedError = $"No Temporary Storage Register Entry has been found with Job Reference ({entryNumber}). The Status of this record will be cleared.";
				UnitTestUserNotification.Instance.ClearMessages();

				tempStorage.AMA_MessageType = messageType;
				tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
				if (tempStorage.IsMessageTypeLAM)
				{
					tempStorage.EntryNumber = entryNumber;
				}
				else
				{
					tempStorage.DsdtMrnNumber = entryNumber;
				}

				Factory.Save();

				CombineAssertions(() =>
				{
					var rollbackInboundTransactionMenuItem = form.Menu.MenuItems.FindByText("Roll-back Inbound Transaction in TS", true);
					AssertNotNull(rollbackInboundTransactionMenuItem);

					rollbackInboundTransactionMenuItem.PerformClick();

					if (hasError)
					{
						AssertEquals($"{assertMessageText}: Error is display when there are not RegHeader associated", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals($"{assertMessageText}: Customs Status is empty when there are not RegHeader associated", ZString.Empty, tempStorage.CustomsStatus);
					}
					else
					{
						AssertNotEquals($"{assertMessageText}: Error is not display when there are RegHeader associated", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
						CreateRegHeader();
					}
				});
			}
		}

		void CreateRegHeader()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "RegHeaderReference";

			Factory.Save();
		}
	}

	public void TestRollbackInboundTransactionClick_CusTempStorageRefHeaderHasTransactions()
	{
		var expectedError = "Inbound transactions cannot be rolled back when there are already transactions that represent an outbound or adjustment for those goods.";

		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
		tempStorage.EntryNumber = "RegHeaderReference";
		tempStorage.DsdtMrnNumber = "RegHeaderReference";

		var regLineTransaction = CreateRegLineTransaction();

		var testCases = new[]
		{
			(messageType: G5MessageTypeCodeList.Codes.LameManualEntry, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Pending, hasError: true, assertMessageText: "LAME: with Transaction Status PND"),
			(messageType: G5MessageTypeCodeList.Codes.LameManualEntry, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Deleted, hasError: false, assertMessageText: "LAME:"),
			(messageType: G5MessageTypeCodeList.Codes.LameManualEntry, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, hasError: true, assertMessageText: "LAME: with Transaction Status CON"),
			(messageType: G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Pending, hasError: true, assertMessageText: "TSM: with Transaction Status PND"),
			(messageType: G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Deleted, hasError: false, assertMessageText: "TSM:"),
			(messageType: G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, hasError: true, assertMessageText: "TSM: with Transaction Status CON"),
		};

		using (var form = new G5V1TemporaryStorageForm(tempStorage))
		{
			var rollbackInboundTransactionMenuItem = form.Menu.MenuItems.FindByText("Roll-back Inbound Transaction in TS", true);
			AssertNotNull(rollbackInboundTransactionMenuItem);

			foreach (var (messageType, transactionStatus, hasError, assertMessageText) in testCases)
			{
				UnitTestUserNotification.Instance.ClearMessages();
				tempStorage.AMA_MessageType = messageType;

				regLineTransaction.SRT_TransactionStatus = transactionStatus;

				Factory.Save();

				rollbackInboundTransactionMenuItem.PerformClick();
				if (hasError)
				{
					AssertEquals($"{assertMessageText} error is display", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertNotEquals($"{assertMessageText} with Transaction Status Not PND or CON error is not display when there is a Transaction with Status not PND or CON", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
					regLineTransaction = CreateRegLineTransaction();
				}
			}
		}

		CusTempStorageRegLineTransaction CreateRegLineTransaction()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "RegHeaderReference";

			Factory.Save();

			var regLine = Factory.New<CusTempStorageRegLine>();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_SRH = regHeader.PK;

			var regLineTransaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;

			var regLineTransaction2 = regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;

			return regLineTransaction2;
		}
	}

	[TestDate(2023, 01, 01, 12, 30, 00)]
	public void TestRollbackInboundTransactionClick()
	{
		var expectedMessage = "Temporary Storage data deleted successfully.";
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated;
		tempStorage.EntryNumber = "RegHeaderReference";
		tempStorage.DsdtMrnNumber = "RegHeaderReference";

		var testCases = new[]
		{
			(messageType: G5MessageTypeCodeList.Codes.LameManualEntry, assertMessageText: "LAME"),
			(messageType: G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry, assertMessageText: "TSM"),
		};

		foreach (var (messageType, assertMessageText) in testCases)
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var guarantee = CreateRegHeaderAndChildren(messageType);

			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var rollbackInboundTransactionMenuItem = form.Menu.MenuItems.FindByText("Roll-back Inbound Transaction in TS", true);
				rollbackInboundTransactionMenuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals($"{assertMessageText}: successfully message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals($"{assertMessageText}: Customs Status value should be Empty", ZString.Empty, tempStorage.CustomsStatus);
					AssertRegHeaderAndChildren(false, assertMessageText);
					if (tempStorage.IsMessageTypeTSM)
					{
						AssertGuaranteeTransactionIsCreated(guarantee);
					}
				});
			}
		}

		CusGuaranteeHeader CreateRegHeaderAndChildren(ZString messageType)
		{
			tempStorage.AMA_MessageType = messageType;

			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "RegHeaderReference";

			var regLine = Factory.New<CusTempStorageRegLine>();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_SRH = regHeader.PK;

			var regLineTransaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;

			var regLineItem = Factory.New<CusTempStorageRegLineItem>();
			regLineItem.SRI_GoodsItemNumber = 1;

			var regLineItemPivot1 = Factory.New<CusTempStorageRegLineItemPivot>();
			regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot1.SRV_SRL_Line = regLine.PK;

			var isTSM = tempStorage.IsMessageTypeTSM;

			if (isTSM)
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "AH3";
				var orgAddress = Factory.New<OrgAddress>();
				orgAddress.OA_OH = orgHeader.PK;
				orgAddress.OA_Address1 = "Address";

				var cusGuarantee = Factory.New<CusGuaranteeHeader>();
				cusGuarantee.CPH_Number = "Test1";
				cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
				cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
				cusGuarantee.CPH_SubType = "1";
				cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
				cusGuarantee.CPH_Balance = 1000.0m;

				var commonGuarantee = Factory.New<EU.Business.Declaration.CommonGuarantee>();
				commonGuarantee.PW_BondNumber = "Test1";
				commonGuarantee.PW_ParentID = regHeader.PK;
				commonGuarantee.PW_ParentTableCode = regHeader.TablePrefix;
				commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;
				commonGuarantee.PW_BondAmount = -10.0m;

				AssertEquals("[PreReq] no transactions", 0, regHeader.Guarantee?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
			}

			Factory.Save();

			AssertRegHeaderAndChildren(true);

			return isTSM ? regHeader.Guarantee?.CusGuarantee : null;
		}

		void AssertRegHeaderAndChildren(bool isPreReq, string assertionText = "")
		{
			var expectedResult = isPreReq ? 1 : 0;
			var assertionPrefixText = isPreReq ? "PreReq" : assertionText;
			var assertionSufixText = isPreReq ? "created" : "deleted";
			AssertEquals($"{assertionPrefixText}: RegHeaders {assertionSufixText}", expectedResult, Factory.Load<CusTempStorageRegHeader>(new ZQuery()).Length);
			AssertEquals($"{assertionPrefixText}: RegLines {assertionSufixText}", expectedResult, Factory.Load<CusTempStorageRegLine>(new ZQuery()).Length);
			AssertEquals($"{assertionPrefixText}: RegLineTransactions {assertionSufixText}", expectedResult, Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery()).Length);
			AssertEquals($"{assertionPrefixText}: RegLineItemPivots {assertionSufixText}", expectedResult, Factory.Load<CusTempStorageRegLineItemPivot>(new ZQuery()).Length);
			AssertEquals($"{assertionPrefixText}: RegLineItems {assertionSufixText}", expectedResult, Factory.Load<CusTempStorageRegLineItem>(new ZQuery()).Length);
		}

		void AssertGuaranteeTransactionIsCreated(CusGuaranteeHeader guarantee)
		{
			var transactions = guarantee.GetTransactions().Where(x => x.CPL_TransactionType == "TRA");
			var transaction = transactions.FirstOrDefault();
			AssertEquals("Transaction is created", 1, transactions.Count());
			AssertEquals("Transaction Date", new ZDateTime(2023, 01, 01, 12, 30, 00), transaction.CPL_TransactionDate);
			AssertEquals("Transaction Type", "TRA", transaction.CPL_TransactionType);
			AssertEquals("Reference", "RegHeaderReference", transaction.CPL_Reference);
			AssertEquals("Value", 10.0m, transaction.CPL_TranValue);
			AssertEquals("Comment", "Roll-back inbound transaction in TS Register without any outbound", transaction.CPL_Comment);
			AssertEquals("Status", "CON", transaction.CPL_TransactionStatus);
		}
	}

	#endregion

	public GlbStaff Staff => staff ?? (staff = Factory.GetStaffAccount());
	GlbStaff staff;

	const string WrongBrokerOrCertificatePopUpText = "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.";

	const string ShouldSaveJobPopUpText = "The Job has not yet been saved. Do you want to save and proceed?";

	class SendMenuForTest : G5V1TemporaryStorageMessagesMenu
	{
		public SendMenuForTest(ZForm parentForm) : base(parentForm)
		{
		}

		protected override MessageEditForm GetMessageEditForm()
			 => new MessageEditFormForTest();
	}

	class MessageEditFormForTest : MessageEditForm
	{
		public MessageEditFormForTest()
			: base()
		{
		}

		public override (ZString, ZBool) EditMessage(ZString messageText) => (messageText.Replace("ES009999", "FR001234"), true);
	}
}
