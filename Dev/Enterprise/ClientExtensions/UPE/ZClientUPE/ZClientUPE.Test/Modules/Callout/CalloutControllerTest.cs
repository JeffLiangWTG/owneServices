using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.GUI;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class CalloutControllerTest : TestCaseWithFactory
	{
		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.Callout, Controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(Callout), Controller.TypeOfTopLevelBusinessObject);
		}

		public void TestControllerID()
		{
			AssertEquals(ClientControllerRegistration.Callout, Controller.ID);
		}

		public void TestGetForm()
		{
			Callout callout = Factory.New<Callout>();
			using (CalloutForm form = Controller.GetForm(callout) as CalloutForm)
			{
				AssertNotNull("Form type should be CalloutForm", form);
			}
		}

		public void TestTwoUsersCannotEditTheSameFormAtTheSameTime()
		{
			BusinessObject callout = GetSavedCallout();
			IZForm form1 = GetNewController().ShowEditForm(callout);
			IZForm form2 = GetNewController().ShowEditForm(callout);
			try
			{
				AssertEquals(typeof(CalloutForm), form1.GetType());
				AssertNull("Should Not Create second form", form2);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				Assert(lastMessage.StartsWith("Access to Finance Item : 'HOUSEBILL' is denied.\nThe record has been locked since '"));
				Assert(lastMessage.EndsWith("' by '" + Env.CurrentUser.FullName + "'.\nPlease wait until the lock has been released before trying to edit the record.\n"));
			}
			finally
			{
				CloseAndDisposeForm(form1);
			}

			try
			{
				form2 = GetNewController().ShowEditForm(callout);
				AssertEquals(typeof(CalloutForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form2);
			}
		}

		public void TestOneUserCanViewFormWhileOtherUserEditsTheSameForm()
		{
			BusinessObject callout = GetSavedCallout();
			IZForm form1 = GetNewController().ShowEditForm(callout);
			IZForm form2 = GetNewController().ShowViewForm(callout);
			try
			{
				AssertEquals(typeof(CalloutForm), form1.GetType());
				AssertEquals(typeof(CalloutForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form1);
				CloseAndDisposeForm(form2);
			}
		}

		public void TestOneUserCanEditFormWhileOtherUserViewsTheSameForm()
		{
			BusinessObject callout = GetSavedCallout();
			IZForm form1 = GetNewController().ShowViewForm(callout);
			IZForm form2 = GetNewController().ShowEditForm(callout);
			try
			{
				AssertEquals(typeof(CalloutForm), form1.GetType());
				AssertEquals(typeof(CalloutForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form1);
				CloseAndDisposeForm(form2);
			}
		}

		public void TestEditForm_IfRecordCannotBeDisplayed()
		{
			Callout callout = Factory.New<Callout>();
			callout.CS_HAWB = "HOUSEBILL";
			BusinessObject mAWB = Factory.New(typeof(UPECusMAWB));
			callout.CS_CM = mAWB.PK;
			AssertNoExceptionThrown("No NullReferenceException should be thrown", () => GetNewController().ShowEditForm(callout));
		}

		public void TestCalloutChargeCoexistsWithCharge()
		{
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			declaration.CustomsEntryHeaders.AddNew();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_ParentTableCode = "JE";
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = jobHeader.PK;
			charge.JR_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).PK;
			charge.JR_Desc = "International Freight";
			charge.JR_OSSellAmt = 1;
			Factory.Save();
			var controller = GetNewController();
			var controllerFactory = controller.Factory;
			Callout callout = controllerFactory.NewWithValidTestData<Callout>();
			callout.CS_PiecesManifested = 1;
			callout.CS_ConsigneeCity = "CNCity";
			callout.CS_ConsigneeName = "CNName";
			callout.CS_ConsigneePostcode = "CNPostCode";
			callout.CS_ConsigneeState = "CNState";
			callout.CS_ConsigneeStreet = "CNAddress1";
			callout.CS_ConsigneeStreet2 = "CNAddress2";
			callout.CS_RN_NKConsigneeCountry = "AU";
			callout.CS_ConsignorCity = "SHCity";
			callout.CS_ConsignorName = "SHName";
			callout.CS_ConsignorPostcode = "SHPostCode";
			callout.CS_ConsignorState = "SHState";
			callout.CS_ConsignorStreet = "SHAddress1";
			callout.CS_ConsignorStreet2 = "SHAddress2";
			callout.CS_RN_NKConsignorCountry = "TW";
			callout.CS_GoodsDescription = "Goo";
			callout.CS_ResponsiblePartyID = "asldkjf";
			callout.CS_RL_NKOrigin = "TWTPE";
			callout.CS_Weight = 123;
			callout.DutyType = "AB";
			callout.CurrentQueue.P4_CustomAttrib6 = "01";
			callout.CurrentQueue.P4_QueueName = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			callout.CS_JE_CustomsFormalEntry = declaration.PK;
			controllerFactory.Save();
			var form = controller.ShowEditForm(callout);
			var cusHAWB2 = controllerFactory.Load<UPECusHAWB>(callout.PK);
			var taxFilter = new ZQuery(AccTaxRateSchema.AT_Code, GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.Country.Code);
			var gstTaxRate = Factory.LoadTop1<AccTaxRate>(taxFilter);
			var rate = gstTaxRate.GetRate_ForTestOnly() / 100m;
			try
			{
				AssertEquals(typeof(CalloutForm), form.GetType());
				AssertNoExceptionThrown(() =>
				{
					callout.EnsureJobHeaderExists();
					CalloutCharge securityFeeCharge = callout.JobHeader.Charges.AddNew();
					securityFeeCharge.JR_Desc = ShipmentChargeDescription.SecurityFee;
					securityFeeCharge.TaxableAmount = 10m;
					AssertEquals("Security fee", securityFeeCharge.Amount + rate * securityFeeCharge.TaxableAmount, callout.FinanceSecurityFeeIncludingGST);
					controllerFactory.Save();
				});
				callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Finance;
				callout.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.CCAL.Codes.OQ_HeldForPayment;
				controllerFactory.Save();
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "QUC");
				query.AddToFilter(StmALogSchema.SL_Reference, LikeComparisonOperator.StartsWith, "COM\"CAL\",\"OQ\"");
				AssertEquals(2, controllerFactory.GetDatabaseCount(typeof(ZArchitecture.Business.StmALog), query));
			}
			finally
			{
				CloseAndDisposeForm(form);
			}
		}

		void CloseAndDisposeForm(IZForm form)
		{
			if (form != null)
			{
				((ZForm)form).Close();
				form.Dispose();
			}
		}

		Callout GetSavedCallout()
		{
			Callout result = Factory.New<Callout>();
			result.CS_HAWB = "HOUSEBILL";
			BusinessObject mAWB = Factory.New(typeof(UPECusMAWB));
			result.CS_CM = mAWB.PK;
			Factory.Save();
			return result;
		}

		public void TestSecurityCheckpoints()
		{
			AssertEquals(Env.Security.None, Controller.CheckPointForDelete);
			AssertEquals(Env.Security.None, Controller.CheckPointForEdit);
			AssertEquals(Env.Security.None, Controller.CheckPointForNew);
			AssertEquals(Env.Security.None, Controller.CheckPointForView);
		}

		#region CalloutControllerForTest
		CalloutControllerForTest Controller
		{
			get
			{
				if (fController == null)
				{
					fController = GetNewController();
				}

				return fController;
			}
		}

		CalloutControllerForTest fController;
		CalloutControllerForTest GetNewController()
		{
			return new CalloutControllerForTest();
		}

		class CalloutControllerForTest : CalloutController
		{
			public new IZForm GetForm(IBusiness businessEntity)
			{
				return base.GetForm(businessEntity);
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get
				{
					return base.CheckPointForEdit;
				}
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get
				{
					return base.CheckPointForDelete;
				}
			}

			public new SecurityCheckpoint CheckPointForNew
			{
				get
				{
					return base.CheckPointForNew;
				}
			}

			public new SecurityCheckpoint CheckPointForView
			{
				get
				{
					return base.CheckPointForView;
				}
			}
		}

		#endregion
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
