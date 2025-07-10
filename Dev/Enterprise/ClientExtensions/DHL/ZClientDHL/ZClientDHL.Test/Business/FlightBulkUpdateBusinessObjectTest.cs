using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusNZ = Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Client.DHL.Business.Testing
{
	[TestedType(typeof(FlightBulkUpdateBusinessObject))]
	public class FlightBulkUpdateBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExistingMAWB()
		{
			AssertNoNotifications(BizO.ExistingMAWBInfo);
			BizO.ExistingMAWB = ZString.Empty;
			AssertHasNotifications(BizO.ExistingMAWBInfo);
			AssertHasErrorContaining(BizO.ExistingMAWBInfo, "Please enter a value");
			BizO.ExistingMAWB = "abc-123";
			AssertHasNotifications(BizO.ExistingMAWBInfo);
			AssertHasErrorContaining(BizO.ExistingMAWBInfo, "There was no existing standalone, air freight declaration");
			AssertHasWarningContaining(BizO.ExistingMAWBInfo, "The MAWB should contain 11 digits");
			BizO.ExistingMAWB = "abc-12345678";
			AssertHasWarningContaining(BizO.ExistingMAWBInfo, "The MAWB can only contain numbers");
			BizO.ExistingMAWB = "123-12345678";
			AssertHasWarningContaining(BizO.ExistingMAWBInfo, "Invalid check digit. The last digit should be '5'");
			JobDec.JE_MasterBill = "081-12345675";
			Factory.Save();
			BizO.ExistingMAWB = "081-12345675";
			AssertNoErrorContaining(BizO.ExistingMAWBInfo, "There was no existing standalone, air freight declaration");
			AssertNoNotifications(BizO.ExistingMAWBInfo);
		}

		public void TestMAWB()
		{
			AssertNoNotifications(BizO.MAWBInfo);
			bizO.MAWB = ZString.Empty;
			AssertNoNotifications(BizO.MAWBInfo);
			BizO.MAWB = "abc-123";
			AssertHasNotifications(BizO.MAWBInfo);
			AssertNoErrors(bizO);
			AssertHasWarningContaining(BizO.MAWBInfo, "The MAWB should contain 11 digits");
			BizO.MAWB = "abc-12345678";
			AssertHasWarningContaining(BizO.MAWBInfo, "The MAWB can only contain numbers");
			BizO.MAWB = "123-12345678";
			AssertHasWarningContaining(BizO.MAWBInfo, "Invalid check digit. The last digit should be '5'");
			JobDec.JE_MasterBill = "081-12345675";
			Factory.Save();
			BizO.MAWB = JobDec.JE_MasterBill;
			AssertNoNotifications(BizO.MAWBInfo);
		}

		public void TestFlightNo()
		{
			AssertNoNotifications(BizO.FlightNoInfo);
			BizO.FlightNo = ZString.Empty;
			AssertNoNotifications(BizO.FlightNoInfo);
			BizO.FlightNo = "abc";
			AssertNoNotifications(BizO.FlightNoInfo);
		}

		public void TestArrivalDate()
		{
			AssertNoNotifications(BizO.ArrivalDateInfo);
			BizO.ArrivalDate = ZDateTime.Empty;
			AssertNoNotifications(BizO.ArrivalDateInfo);
			BizO.ArrivalDate = ZDateTime.Invalid;
			AssertHasNotifications(BizO.ArrivalDateInfo);
			BizO.ArrivalDate = ArrivalDate;
			AssertNoNotifications(BizO.ArrivalDateInfo);
		}

		public void TestDepartureDate()
		{
			AssertNoNotifications(BizO.DepartureDateInfo);
			BizO.DepartureDate = ZDateTime.Empty;
			AssertNoNotifications(BizO.DepartureDateInfo);
			BizO.DepartureDate = ZDateTime.Invalid;
			AssertHasNotifications(BizO.DepartureDateInfo);
			BizO.DepartureDate = DepartureDate;
			AssertNoNotifications(BizO.DepartureDateInfo);
		}

		public void TestArrivalAndDepartureDates()
		{
			AssertNoNotifications(BizO.ArrivalDateInfo);
			AssertNoNotifications(BizO.DepartureDateInfo);
			BizO.ArrivalDate = ZDateTime.Empty;
			BizO.DepartureDate = ZDateTime.Empty;
			AssertNoNotifications(BizO.ArrivalDateInfo);
			AssertNoNotifications(BizO.DepartureDateInfo);
			BizO.ArrivalDate = ZDateTime.Now.AddDays(-1);
			BizO.DepartureDate = ZDateTime.Now.AddDays(1);
			AssertHasNotifications(BizO.ArrivalDateInfo);
			AssertHasNotifications(BizO.DepartureDateInfo);
			BizO.ArrivalDate = BizO.DepartureDate.AddDays(1);
			AssertNoNotifications(BizO.ArrivalDateInfo);
			AssertNoNotifications(BizO.DepartureDateInfo);
		}

		public void TestEDITransmitDate()
		{
			AssertNoNotifications(BizO.EDITransmitDateInfo);
			BizO.EDITransmitDate = ZDateTime.Empty;
			AssertNoNotifications(BizO.EDITransmitDateInfo);
			BizO.EDITransmitDate = ZDateTime.Invalid;
			AssertHasNotifications(BizO.EDITransmitDateInfo);
			BizO.EDITransmitDate = ZDateTime.Now.AddDays(-1);
			AssertHasErrors(BizO.EDITransmitDateInfo);
			AssertHasErrorContaining(BizO.EDITransmitDateInfo, "EDI date must occur today or later");
			BizO.EDITransmitDate = ZDateTime.Now;
			AssertNoErrors(BizO.EDITransmitDateInfo);
			AssertNoNotifications(BizO.EDITransmitDateInfo);
		}

		public void TestRunPreSaveValidation()
		{
			AssertNoNotifications(BizO);
			BizO.RunPreSaveValidation();
			Assert(BizO.HasErrors);
			JobDec.JE_MasterBill = "081-12345675";
			Factory.Save();
			BizO.ExistingMAWB = JobDec.JE_MasterBill;
			BizO.FlightNo = "FLIGHT";
			bizO.ClearAllNotifications();
			BizO.RunPreSaveValidation();
			Assert(!BizO.HasErrors);
			AssertNoNotifications(BizO);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new FlightBulkUpdateBusinessObject();
		}

		FlightBulkUpdateBusinessObject BizO
		{
			get
			{
				return bizO ?? (bizO = new FlightBulkUpdateBusinessObject());
			}
		}

		FlightBulkUpdateBusinessObject bizO;
		ZDateTime DepartureDate
		{
			get
			{
				return !newDeparture.IsEmpty ? newDeparture : (newDeparture = new ZDateTime(ZDateTime.Today.Year, 6, 1));
			}
		}

		ZDateTime newDeparture;
		ZDateTime ArrivalDate
		{
			get
			{
				return DepartureDate.AddDays(1);
			}
		}

		JobDeclaration JobDec
		{
			set
			{
				jobDec = value;
			}

			get
			{
				if (jobDec == null)
				{
					jobDec = Factory.NewWithValidTestData<JobDeclaration>();
					jobDec.JE_MessageType = Customs.NZ.Business.JobMessageTypeList.Codes.Import;
					jobDec.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
					jobDec.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
					jobDec.JE_MasterBill = "081-11111111";
					jobDec.JE_VoyageFlightNo = "QF253";
					jobDec.JE_RL_NKPortOfLoading = "USLAX";
					jobDec.JE_RL_NKPortOfArrival = "NZAKL";
					jobDec.JE_ExportDate = new ZDateTime(2005, 12, 12);
					jobDec.JE_DateOfArrival = new ZDateTime(2005, 12, 13);
					jobDec.JE_EDITransmitDate = ZDateTime.Now;
					jobDec.JE_TransportMode = Constants.TransportModes.Air;
					BaseCusContainer container = jobDec.CusContainers.AddNew();
					CusNZ.Bill bill = jobDec.Bills.AddNew();
					BasePackingGroup packingGroup = bill.PackingGroups.AddNew();
					packingGroup.CR_CO_Container = container.PK;
					BasePackage package = packingGroup.Packages.AddNew();
				}

				return jobDec;
			}
		}

		JobDeclaration jobDec;
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.NewZealand);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DHL");
		}
		#endregion
	}
}
