using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(SetEntryStatusDetail))]
	public class SetEntryStatusDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new SetEntryStatusDetail(declaration);
		}

		[TestDate(2021, 1, 1)]
		public void TestDefaultValue()
		{
			var now = ZDateTime.Now;
			var declaration = Factory.New<JobDeclaration>();
			{
				SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
				Assert("No Default Value when no entries in Decalration", helperBO.CusEntryHeaderPK.IsEmpty);
				Assert("No Default Value when no entries in Decalration", helperBO.EntryStatus.IsEmpty);
				AssertEquals(now, helperBO.EventTime);
			}
			{
				var entry1 = declaration.ActiveEntryHeaders.AddNew();
				entry1.CH_EntryStatus = "ST1";
				SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
				AssertEquals("Has Default Value when only one entry in Decalration", entry1.PK, helperBO.CusEntryHeaderPK);
				AssertEquals("Has Default Value when only one entry in Decalration", "ST1", helperBO.EntryStatus);
				AssertEquals(now, helperBO.EventTime);
			}
			{
				declaration.ActiveEntryHeaders.AddNew();
				SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
				Assert("No Default Value when multiplus entries in Decalration", helperBO.CusEntryHeaderPK.IsEmpty);
				Assert("No Default Value when multiplus entries in Decalration", helperBO.EntryStatus.IsEmpty);
				AssertEquals(now, helperBO.EventTime);
			}
		}

		public void TestCusEntryHeaderPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
			AssertEquals("Entry", DataBoundResourceStrings.GetDataForProperty(helperBO.CusEntryHeaderPKInfo).Caption);
		}

		public void TestEventTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
			AssertEquals("Event Time", DataBoundResourceStrings.GetDataForProperty(helperBO.EventTimeInfo).Caption);
		}

		public void TestEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
			AssertEquals("Entry Status", DataBoundResourceStrings.GetDataForProperty(helperBO.EntryStatusInfo).Caption);
		}

		public void TestAddEvent()
		{
			Factory.SetupEntryStatusList();
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_EntryStatus = "ST1";
			Factory.Save();
			declaration.Reload();
			{
				SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
				helperBO.RunPreSaveValidation();
				bool isStatusChanged = helperBO.SetEntryStatus();
				bool hasLog = entry1.Logs.HasLogWith(log => true);
				Assert("status not changed", !isStatusChanged);
				Assert("No logs generated since not pass validation", !hasLog);
			}
			{
				SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
				helperBO.CusEntryHeaderPK = entry1.PK;
				helperBO.EventTime = new ZDateTime(2021, 11, 11);
				helperBO.EntryStatus = "ST2";
				helperBO.RunPreSaveValidation();
				bool isStatusChanged = helperBO.SetEntryStatus();
				bool hasLog = entry1.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.CustomsEntryStatusCode && log.SL_Reference == "ST2" && log.SL_EventTime == helperBO.EventTime);
				Assert("status changed", isStatusChanged);
				Assert("log generated ", hasLog);
				AssertEquals("entrystatus on EntryHeader after clicked ok buton", "ST2", entry1.CH_EntryStatus);
			}
			{
				SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
				helperBO.CusEntryHeaderPK = entry1.PK;
				helperBO.EventTime = new ZDateTime(2021, 11, 11);
				helperBO.EntryStatus = "ST3";
				helperBO.RunPreSaveValidation();
				bool isStatusChanged = helperBO.SetEntryStatus();
				bool hasLog = entry1.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.CustomsEntryStatusCode && log.SL_Reference == "ST3" && log.SL_EventTime == helperBO.EventTime);
				Assert("status changed", isStatusChanged);
				Assert("log generated", hasLog);
				AssertEquals("entrystatus on EntryHeader after clicked ok buton", "ST3", entry1.CH_EntryStatus);
			}
		}
	}
}
