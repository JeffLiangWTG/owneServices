using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(CDSCashPaymentsController))]
	class CDSCashPaymentsControllerTests : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(CDSCashPaymentsController); }
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(CusEntryPayInfo);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.GB.CDSCashPaymentsController;
		}

		public override void TestEditForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestNewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestTemplateCopyForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert("Not Implemented", true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var payInfo = cusEntryHeader.EntryPayInfos.AddNew();
			payInfo.C9_PaymentDate = ZDateTime.Now;
			payInfo.C9_PaymentAmount = 100m;
			payInfo.C9_PaymentReference = "GB1Reference";
			payInfo.C9_PaymentStatus = Customs.Business.CusEntryPayInfoStatusList.Codes.Clear;
			payInfo.C9_ReceiptDate = ZDate.Today;
			Factory.Save();
			return payInfo;
		}
	}
}
