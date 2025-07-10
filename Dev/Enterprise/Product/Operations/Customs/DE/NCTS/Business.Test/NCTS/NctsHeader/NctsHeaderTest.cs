using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeader))]
	sealed class NctsHeaderTest : EU.NCTS.Business.Testing.NctsHeaderAbstractTest
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusInBondApplicationCodeList.Codes.NCTS5, header.BH_ApplicationCode);
		}

		public void TestSetBH_HeaderType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			CombineAssertions(() =>
			{
				AssertEquals("BH_ExportFlag", EventFlagList.Codes.No, nctsHeader.BH_ExportFlag);
				AssertEquals("DestinationTrader", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, nctsHeader.DestinationTrader.E2_OA_Address);
			});
		}

		public void TestSeals()
		{
			AssertEquals("Type", typeof(SealCollection<Seal>), header.Seals.GetType());
		}

		public void TestBills()
		{
			AssertType<NctsBillCollection<NctsBill>>(header.Bills);
		}

		public void TestPreviousDocuments()
		{
			AssertType<CommonPreviousDocumentCollection<CommonPreviousDocument>>(header.PreviousDocuments);
		}

		public void TestAdditionalDocuments()
		{
			AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(header.AdditionalDocuments);
		}

		public void TestBH_ExportFlag_ReadOnly_Default()
		{
			AssertEquals(false, header.BH_ExportFlagInfo.ReadOnly);
		}

		public void TestBH_ExportFlag_ReadOnly_ValueChanged()
		{
			CombineAssertions(() =>
			{
				header.BH_ExportFlag = EventFlagList.Codes.No;
				AssertEquals("Event flag is No", false, header.BH_ExportFlagInfo.ReadOnly);
				header.BH_ExportFlag = EventFlagList.Codes.Yes;
				AssertEquals("Event flag is Yes", false, header.BH_ExportFlagInfo.ReadOnly);
				header.LogEventFlagYes();
				AssertEquals("Logged", false, header.BH_ExportFlagInfo.ReadOnly);
				header.BH_ExportFlag = EventFlagList.Codes.Cancelled;
				AssertEquals("Event flag is Cancelled", false, header.BH_ExportFlagInfo.ReadOnly);
				header.CancelEventFlagYesLog();
				AssertEquals("Log is cancelled", true, header.BH_ExportFlagInfo.ReadOnly);
			});
		}

		public void TestEventFlagYesLog()
		{
			AssertNull(header.EventFlagYesLog);
		}

		public void TestLogEventFlagYes()
		{
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			header.LogEventFlagYes();
			AssertNotNull(header.EventFlagYesLog);
		}

		public void TestLogEventFlagYes_Default()
		{
			header.LogEventFlagYes();
			AssertNull(header.EventFlagYesLog);
		}

		public void TestLogEventFlagYes_ThereIsOnlyOneLog()
		{
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			header.LogEventFlagYes();
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			header.LogEventFlagYes();
			AssertEquals(1, header.Logs.Find(log => log.SL_SE_NKEvent == Events.MiscellaneousEvent.Code && log.SL_Reference == "EventFlag=Y").Count());
		}

		public void TestCancelEventFlagYesLog()
		{
			CombineAssertions(() =>
			{
				header.BH_ExportFlag = EventFlagList.Codes.Yes;
				header.LogEventFlagYes();
				header.BH_ExportFlag = EventFlagList.Codes.Cancelled;
				header.CancelEventFlagYesLog();
				AssertNotNull("Still existing", header.EventFlagYesLog);
				AssertEquals("But cancelled", true, header.EventFlagYesLog.SL_IsCancelled);
			});
		}

		public void TestNoteTypes()
		{
			Assert("NCTSEventCancellationReason", header.NoteTypes.IsPredefinedNoteTypeByDescription(PredefinedNoteTypes.Instance.NCTSEventCancellationReason.Description));
		}

		public void TestIsArrivalEventAvailable()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeaderForTest>();
				AssertEquals("Default", false, header.IsArrivalEventAvailable);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, EUN, ZDate.Today, true))
				{
					header.BH_ExportFlag = EventFlagList.Codes.Yes;
					AssertEquals("Within Transition Period", true, header.IsArrivalEventAvailable);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, EUN, ZDate.Today, false))
				{
					AssertEquals("Outside Transition Period, No records", false, header.IsArrivalEventAvailable);
					header.EnRouteIncidents.AddNew();
					AssertEquals("Outside Transition Period, Has records", true, header.IsArrivalEventAvailable);
				}
			});
		}

		public void TestBH_ExportFlag()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, EUN, ZDate.Today, true))
				{
					header.BH_ExportFlag = EventFlagList.Codes.No;
					AssertArrivalEventRelatedReadOnly("Default In TP", true);
					header.BH_ExportFlag = EventFlagList.Codes.Yes;
					AssertArrivalEventRelatedReadOnly("Yes In TP", false);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, EUN, ZDate.Today, false))
				{
					header.BH_ExportFlag = EventFlagList.Codes.No;
					AssertArrivalEventRelatedReadOnly("Default", true);
					header.BH_ExportFlag = EventFlagList.Codes.Yes;
					AssertArrivalEventRelatedReadOnly("Yes", true);
				}
			});
		}

		public void TestArrivalEventRelatedReadOnly_Reload_InTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, EUN, ZDate.Today, true))
			{
				header.BH_ExportFlag = EventFlagList.Codes.Yes;
				AssertArrivalEventRelatedReadOnly("Yes", false);
				Factory.Save();
				header = new BusinessObjectFactory().Load<NctsHeader>(header.PK);
				AssertArrivalEventRelatedReadOnly("Reloaded", false);
			}
		}

		public void TestArrivalEventRelatedReadOnly_Reload_OutsideTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, EUN, ZDate.Today, false))
			{
				header.BH_ExportFlag = EventFlagList.Codes.Yes;
				AssertArrivalEventRelatedReadOnly("Yes", true);
				Factory.Save();
				header = new BusinessObjectFactory().Load<NctsHeader>(header.PK);
				AssertArrivalEventRelatedReadOnly("Reloaded", true);
			}
		}

		void AssertArrivalEventRelatedReadOnly(string message, bool readOnly)
		{
			AssertEquals(message + "->EnRouteIncidents.ReadOnly", readOnly, header.EnRouteIncidents.ReadOnly);
			AssertEquals(message + "->EnRouteTransshipments.ReadOnly", readOnly, header.EnRouteTransshipments.ReadOnly);
			AssertEquals(message + "->EnRouteSeals.ReadOnly", readOnly, header.EnRouteSeals.ReadOnly);
		}

		public void TestEventCancellationReason_Caption()
		{
			AssertEquals("Reason", DataBoundResourceStrings.GetDataForProperty(header.EventCancellationReasonInfo).Caption);
		}

		public void TestEventCancellationReason_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("default", true, header.EventCancellationReasonInfo.ReadOnly);
				header.BH_ExportFlag = EventFlagList.Codes.Cancelled;
				AssertEquals("Cancelled", false, header.EventCancellationReasonInfo.ReadOnly);
			});
		}

		public void TestShouldIncludeNotificationsFromInfo()
		{
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var message = header.GetNonMandatoryMessageErrors();
			Assert(!message.Contains("Transport Means"));
			Assert(!message.Contains("Place of Unloading Code"));
		}

		public void TestUnloadingRemarkType()
		{
			AssertType<UnloadingRemarkAddInfo>(header.UnloadingRemark);
		}

		public void TestHeaderContainers()
		{
			header.DepartureHeaderContainers.AddNew();
			header.UnloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
			Assert(header.DepartureHeaderContainers.ReadOnly);
			AssertEquals(0, header.DepartureHeaderContainers.Count);

			header.DepartureHeaderContainers.AddNew();
			header.UnloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.No;
			Assert(!header.DepartureHeaderContainers.ReadOnly);
			AssertEquals(1, header.DepartureHeaderContainers.Count);
		}

		public void TestValidation()
		{
			CombineAssertions(() =>
			{
				AssertType<NctsHeaderValidation>("Not Arrival/Departure", header.Validation);
				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				AssertType<ArrivalNctsHeaderValidation>("Arrival", header.Validation);
				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				AssertType<NctsHeaderValidation>("Departure", header.Validation);
			});
		}

		public void TestPlaceOfUnloading()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Valid Customs Offices for Location", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Germany);
			var aa01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA01", "Loading place AA01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE000011");
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "12345");
			authorisation.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "AA01");

			header.SetMovementType(NctsMovementType.Codes.Arrival);

			var arrivalMovement = header.ArrivalMovementHeader;
			header.DestinationTrader.E2_OA_Address = orgHeader.MainAddress.PK;
			arrivalMovement.DestinationCustomsOfficeCodeForArrival = "DE000011";
			CombineAssertions(() =>
			{
				header.PlaceOfUnloadingCode = "AA02";
				AssertEquals("Invalid Code", ZString.Empty, header.PlaceOfUnloading);
				header.PlaceOfUnloadingCode = "AA01";
				AssertEquals("Description From List", "Loading place AA01", header.PlaceOfUnloading);
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				AssertEquals("Departure", ZString.Empty, header.PlaceOfUnloading);
			});
		}

		public void TestArrivalDetailsReadOnly()
		{
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_ExportFlag = "Y";
			var incident = header.EnRouteIncidents.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Customs Status different from 'UAP' or 'CL1', MRN should be enabled", false, header.ArrivalMrnFromUserInfo.ReadOnly);
				AssertEquals("Customs Status different from 'UAP' or 'CL1', Local Reference Number should be enabled", false, header.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("Customs Status different from 'UAP' or 'CL1', DestinationTrader should be enabled", false, header.DestinationTrader.ReadOnly);
				AssertEquals("Customs Status different from 'UAP' or 'CL1', Destination Customs Office Code for Arrival should be enabled", false, header.DestinationCustomsOfficeCodeForArrivalInfo.ReadOnly);
				AssertEquals("Customs Status different from 'UAP' or 'CL1', Authorizations should be enabled", false, header.CusAuthorizationUsages.ReadOnly);
				AssertEquals("Customs Status different from 'UAP' or 'CL1', Goods Location in incidents should be enabled", false, incident.GoodsLocation.ReadOnly);

				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
				header.BH_ExportFlag = "Y";
				incident = header.EnRouteIncidents.AddNew();
				AssertEquals("Customs Status 'UAP', MRN should be readonly", true, header.ArrivalMrnFromUserInfo.ReadOnly);
				AssertEquals("Customs Status 'UAP', Local Reference Number should be readonly", true, header.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("Customs Status 'UAP', DestinationTrader should be readonly", true, header.DestinationTrader.ReadOnly);
				AssertEquals("Customs Status 'UAP', Destination Customs Office Code for Arrival should be readonly", true, header.DestinationCustomsOfficeCodeForArrivalInfo.ReadOnly);
				AssertEquals("Customs Status 'UAP', Goods Location in incidents should be readonly", true, incident.GoodsLocation.ReadOnly);

				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				header.BH_ExportFlag = "Y";
				incident = header.EnRouteIncidents.AddNew();
				AssertEquals("Customs Status 'CL1', MRN should be readonly", true, header.ArrivalMrnFromUserInfo.ReadOnly);
				AssertEquals("Customs Status 'CL1', Local Reference Number should be readonly", true, header.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("Customs Status 'CL1', DestinationTrader should be readonly", true, header.DestinationTrader.ReadOnly);
				AssertEquals("Customs Status 'CL1', Destination Customs Office Code for Arrival should be readonly", true, header.DestinationCustomsOfficeCodeForArrivalInfo.ReadOnly);
				AssertEquals("Customs Status 'CL1', Goods Location in incidents should be readonly", true, incident.GoodsLocation.ReadOnly);
			});
		}

		public void TestIsArrivalDetailsReadOnly()
		{
			CombineAssertions(() =>
			{
				header.BH_HeaderType = "D";
				AssertEquals("When is Departure, the result should be false", false, header.IsArrivalDetailsReadOnly);
				header.BH_HeaderType = "A";

				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
				AssertEquals("When is Arrival and BM_CustomsStatus is 'UAP', the result should be true", true, header.IsArrivalDetailsReadOnly);
				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				AssertEquals("When is Arrival and BM_CustomsStatus is 'CL1', the result should be true", true, header.IsArrivalDetailsReadOnly);

				header.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
				AssertEquals("When is Arrival and BM_CustomsStatus is empty (not UAP or CL1), the result should be false.", false, header.IsArrivalDetailsReadOnly);
			});
		}

		public void TestPopulateGuaranteeFromPrincipal()
		{
			// Arrange
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
			cusGuaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			cusGuaranteeHeader.CPH_SubType = GuaranteeSubTypeList.Codes._1;
			cusGuaranteeHeader.CPH_Number = "123";

			var rule = cusGuaranteeHeader.AdditionalAccessCodes.AddNew();
			rule.CPR_ValueFrom = "PIN1";
			cusGuaranteeHeader.MainAccessCode = "PIN_MAIN";

			// Act
			header.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
			var guarantees = header.MovementHeader.Guarantees;
			AssertEquals("Precondition", 1, guarantees.Count);

			// Assert
			CombineAssertions(() =>
			{
				var guarantee = guarantees[0];
				AssertEquals("PW_BondType", GuaranteeSubTypeList.Codes._1, guarantee.PW_BondType);
				AssertEquals("PW_BondNumber", "123", guarantee.PW_BondNumber);
				AssertEquals("PW_Password", "PIN1", guarantee.PW_Password);
			});
		}

		public void TestDestinationCustomsOfficeCodeForArrival_DefaultCGL_AdditionalIdentifier()
		{
			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			cusAuthorisationHeader.CPH_Number = "ACE0001";
			cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var rule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			rule1.CPR_ValueFrom = "T001";
			rule1.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000001");

			var rule2 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			rule2.CPR_ValueFrom = "T002";
			rule2.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000002");

			var rule3 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule3.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			rule3.CPR_ValueFrom = "T003";
			rule3.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000002");

			Factory.Save();

			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovement = header.ArrivalMovementHeader;
			var goodsLocation = header.ArrivalMovementHeader.GoodsLocation;

			CombineAssertions(() =>
			{
				arrivalMovement.DestinationCustomsOfficeCodeForArrival = "DE000001";
				AssertEquals("AdditionalIdentifierList count is 1", "T001", goodsLocation.CGL_AdditionalIdentifier);

				arrivalMovement.DestinationCustomsOfficeCodeForArrival = "DE000002";
				AssertEquals("AdditionalIdentifierList count isn't 1, do nothing", "T001", goodsLocation.CGL_AdditionalIdentifier);
			});
		}

		public void TestCGL_AdditionalIdentifier_ShouldBeSet_WhenDestinationCustomsOfficeCodeForArrivalSetByDefault()
		{
			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			cusAuthorisationHeader.CPH_Number = "ACE0001";
			cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var rule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			rule1.CPR_ValueFrom = "T001";
			rule1.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000001");

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsOfficeForTransit, "DE000001");
			GlbCompany.CurrentCompany.Factory.Save();

			header.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertEquals("T001", header.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier);
		}

		public void TestGoodsLocationDescription_ValueSaved_MultipleRulesWithSameCustomsOffice()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var moveHeader = header.MovementHeader;
			moveHeader.IsSimplifiedNctsProcedure = true;

			var cusAuthorisationUsage = moveHeader.CusAuthorizationUsages.AddNew();
			cusAuthorisationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			cusAuthorisationUsage.AGC_Number = "ACR0001";
			cusAuthorisationUsage.AGC_OH_Owner = GlbCompany.CurrentCompany.OrgProxy.PK;

			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			cusAuthorisationHeader.CPH_Number = "ACR0001";
			cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var rule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			rule1.CPR_ValueFrom = "A001";
			rule1.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000001");

			var rule2 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			rule2.CPR_ValueFrom = "B001";
			rule2.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000001");

			var goodsLocation = moveHeader.GoodsLocation;

			CombineAssertions(() =>
			{
				var customsOffice = moveHeader.CustomsOfficesForDeparture.Where(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).FirstOrDefault();
				customsOffice.CY_Data = "DE000001";
				AssertEquals("AdditionalIdentifierList count is 2", 2, goodsLocation.Lookups.AdditionalIdentifierList.Count);
				AssertEquals("Additional Identifier is empty and must be selected manually", ZString.Empty, goodsLocation.CGL_AdditionalIdentifier);

				goodsLocation.CGL_AdditionalIdentifier = "B001";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedGoodsLocation = newFactory.Load<CusGoodsLocation>(goodsLocation.PK);
				AssertEquals("CGL_AdditionalIdentifier is loaded", "B001", reloadedGoodsLocation.CGL_AdditionalIdentifier);
			});
		}

		public void TestCommonGoodsItemsIntegrator()
		{
			AssertType<NctsCommonGoodsItemsIntegrator>("CommonGoodsItemsIntegrator", header.CommonGoodsItemsIntegrator);
		}

		public void TestDocumentSupporter()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertNotNull(nameof(NctsHeader.DocumentSupporter), nctsHeader.DocumentSupporter);
			AssertType<NctsHeaderDocumentSupporter>($"{nameof(NctsHeader.DocumentSupporter)} type", nctsHeader.DocumentSupporter);
		}

		public void TestFallBackIsActive()
		{
			AssertEquals(false, header.FallBackIsActive);

			var fallbackConfiguration = new NctsFallbackConfiguration();
			fallbackConfiguration.CustomsIncidentNumber = "XXX";
			fallbackConfiguration.Start = ZDateTime.Today.AddDays(-1);
			DENctsCustomsDataRegistry.Instance.NctsFallbackConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackConfiguration);

			AssertEquals(true, header.FallBackIsActive);
		}

		public override void TestClone_Phase4() => Assert("DE doesn't support Phase4 => no need to test it.", true);

		const string EUN = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.DENctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
		}

		NctsHeader header;

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new NctsHeaderLightValidationTester(bizObjToTest);

		class NctsHeaderLightValidationTester : LightValidationTester
		{
			public NctsHeaderLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != Common.AutoCusEntryNum.Schema.CE_Category
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_EntryNum
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_EntryType
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_ParentID
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_ParentTable
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_RN_NKCountryCode
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_EntryIsSystemGenerated
						&& propertyName != ResultsOfControlAddInfo.Schema.G9_CorrectedValue
						&& propertyName != ResultsOfControlAddInfo.Schema.G9_PointerToTheAttribute;
			}
		}
	}
}
