using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : BaseJobDeclarationAbstractTest
	{
		public void TestJE_UCR_Caption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_UCRInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("Caption", "[2/4] UCR", captionResourceString.Caption);
		}

		public void TestJE_UCR_Caption_ImportUCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_UCRInfo, JobDeclaration.CaptionKeyImportUCC6);
			AssertEquals("ShortCaption", "UCR", captionResourceString.ShortCaption);
			AssertEquals("Caption", "[12 08 001 000] UCR", captionResourceString.Caption);
		}

		public void TestUpdateDucrIfNotLocked()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			declaration.JE_UCR = ZString.Empty;
			declaration.UpdateDucrIfNotLocked();
			AssertEquals("UCR should NOT be generated for UCC5", ZString.Empty, declaration.JE_UCR);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			declaration.JE_UCR = ZString.Empty;
			declaration.UpdateDucrIfNotLocked();
			AssertEquals("UCR should NOT be generated for UCC6", ZString.Empty, declaration.JE_UCR);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.JE_UCR = ZString.Empty;
			declaration.UpdateDucrIfNotLocked();
			AssertEquals("UCR should NOT be generated for ITF", ZString.Empty, declaration.JE_UCR);
		}

		public void TestJE_PaymentMethodCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_PaymentMethodInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "[4/8] Pay. Method", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[4/8] Pref. Payment Method", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[4/8] Preferred Payment Method", captionResourceString.Caption);
		}
		public void TestJE_DeclarantTypeCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_DeclarantTypeInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "Rep. Status", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[3/21] Rep. Status", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[3/21] Rep. Status Code", captionResourceString.Caption);
			AssertEquals("FullDescription", "[3/21] Representative Status Code", captionResourceString.FullDescription);
		}

		public void TestRefreshIncotermAndChargeFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var incoTermAndChargeFactory = declaration.IncoTermAndChargeFactory;
			Assert("IncotermAndChargeFactory is loaded, unnecessarry to refresh", !declaration.NeedToGetNewIncoTermAndChargeFactory);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			Assert("JE_MessageType is changed, necessarry to refresh", declaration.NeedToGetNewIncoTermAndChargeFactory);
			incoTermAndChargeFactory = declaration.IncoTermAndChargeFactory;
			Assert("IncotermAndChargeFactory is loaded, unnecessarry to refresh", !declaration.NeedToGetNewIncoTermAndChargeFactory);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Assert("JE_TransportMode is changed, necessarry to refresh", declaration.NeedToGetNewIncoTermAndChargeFactory);
		}

		public void TestIsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "B";

			Assert("No 1D95", !declaration.IsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference);

			var instructionAdditionalInfo = instruction.AdditionalInfos.AddNew();
			instructionAdditionalInfo.CSI_SubType = "REF";
			instructionAdditionalInfo.CSI_Code = "1D95";

			Assert("Contains 1D95", declaration.IsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference);

			instructionAdditionalInfo.Delete();
			Assert("No 1D95", !declaration.IsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var invoiceAdditionalInfo = invoice.AdditionalInfos.AddNew();
			invoiceAdditionalInfo.CSI_SubType = "REF";
			invoiceAdditionalInfo.CSI_Code = "1D95";

			Assert("Contains 1D95", declaration.IsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference);

			invoiceAdditionalInfo.Delete();
			Assert("No 1D95", !declaration.IsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference);

			var invoiceLineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
			invoiceLineAdditionalInfo.CSI_SubType = "REF";
			invoiceLineAdditionalInfo.CSI_Code = "1D95";

			Assert("Contains 1D95", declaration.IsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference);
		}

		public void TestLogCustomsCommenced()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.LogCustomsCommenced("IM415: Customs Declaration");
			Factory.Save();
			CombineAssertions(() =>
			{
				var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
				AssertEquals("CCC (Customs Commenced) Log Entry event should have been created", true, logEntries.Any());
				AssertEquals("SL_Reference", "IM415: Customs Declaration", logEntries[0].SL_Reference);
				AssertNotNull("JE_CustomsCommencedDate is not null", declaration.JE_CustomsCommencedDate);
				AssertEquals("JE_GS_NKCustomsCommencedUser", GlbStaff.CurrentUser.GS_Code, declaration.JE_GS_NKCustomsCommencedUser);
			});
		}

		public void TestTradersOwnReferenceFullForBox7()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OwnerRef = "ref001";
			declaration.Invoices.AddNew().JZ_UCR = "ref002";
			declaration.Invoices.AddNew().JZ_UCR = "";
			declaration.Invoices.AddNew().JZ_UCR = "ref004";
			AssertEquals("TradersOwnReferenceFullForBox7", "ref001, ref002, ref004", declaration.TradersOwnReferenceFullForBox7);
		}

		public void TestBox30LocationOfGoodsForDocumentsAndMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_LocationOfGoods = "IEDUB";
			AssertEquals("Box30LocationOfGoodsForDocumentsAndMessaging", "IEDUB", declaration.Box30LocationOfGoodsForDocumentsAndMessaging);
		}

		public void TestBox18IdentityOfTransportAtDepartureForDocumentsAndMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportIDInland = "123";
			AssertEquals("Box18IdentityOfTransportAtDepartureForDocumentsAndMessaging", "123", declaration.Box18IdentityOfTransportAtDepartureForDocumentsAndMessaging);
		}

		public void TestDefaultJE_ApplicationCode()
		{
			TestDefaultJE_ApplicationCodeBasedOnRegistrySetting(DeclarationApplicationCodeList.Codes.Builtin, ImportDeclarationApplicationCodeList.Codes.V1);
			TestDefaultJE_ApplicationCodeBasedOnRegistrySetting(DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted, ImportDeclarationApplicationCodeList.Codes.V1);
			TestDefaultJE_ApplicationCodeBasedOnRegistrySetting(DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted, ImportDeclarationApplicationCodeList.Codes.Interfaced);
			TestDefaultJE_ApplicationCodeBasedOnRegistrySetting(DeclarationApplicationCodeList.Codes.Interfaced, ImportDeclarationApplicationCodeList.Codes.Interfaced);
		}

		void TestDefaultJE_ApplicationCodeBasedOnRegistrySetting(string registrySettingSubmissionType, string expectedApplicationCode)
		{
			var customsInterface = new LocalCountryCustomsInterface { SubmissionType = registrySettingSubmissionType };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals($"Application code should be {expectedApplicationCode} by default for UCC5 Imports when registry is set to {registrySettingSubmissionType}", expectedApplicationCode, declaration.JE_ApplicationCode);
			}
		}

		public void TestApplicationCodeOnJE_MessageTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("Export should do nothing", "BLT", declaration.JE_ApplicationCode);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertEquals("Import should set to ITF when registry setting is blank", "ITF", declaration.JE_ApplicationCode);

			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID", SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("LocalCountryCustomsInterface ITF : Import should set ApplicationCode to ITF", "ITF", declaration.JE_ApplicationCode);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("LocalCountryCustomsInterface ITF : Export should leave ApplicationCode at ITF", "ITF", declaration.JE_ApplicationCode);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("LocalCountryCustomsInterface BLT : Import should set ApplicationCode to V1", "V1", declaration.JE_ApplicationCode);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("LocalCountryCustomsInterface BLT : Export should set ApplicationCode to BLT", "BLT", declaration.JE_ApplicationCode);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("LocalCountryCustomsInterface BTH : Import should set ApplicationCode to V1", "V1", declaration.JE_ApplicationCode);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("LocalCountryCustomsInterface BTH : Export should set ApplicationCode to BLT", "BLT", declaration.JE_ApplicationCode);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("LocalCountryCustomsInterface BIT : Import should set ApplicationCode to ITF", "ITF", declaration.JE_ApplicationCode);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("LocalCountryCustomsInterface BIT : Export should leave ApplicationCode at ITF", "ITF", declaration.JE_ApplicationCode);
			}
		}

		public void TestJE_ApplicationCode_ReadOnly()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();

				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;

				var entry = declaration.ActiveEntryHeaders.AddNew();
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					using (IECustomsDataRegistry.Instance.IsUCC6EnabledForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
						AssertEquals("Should allow selection of Built-in Types", false, declaration.JE_ApplicationCodeInfo.ReadOnly);

						declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
						AssertEquals("Should allow selection of Built-in Types - V2", false, declaration.JE_ApplicationCodeInfo.ReadOnly);

						declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
						AssertEquals("Should allow selection of Built-in Types - V1", false, declaration.JE_ApplicationCodeInfo.ReadOnly);

						var message = entry.Messages.AddNew();
						AssertEquals("Should be read only when message started", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
						message.Delete();
						AssertEquals("Should allow selection of Built-in Types if no message", false, declaration.JE_ApplicationCodeInfo.ReadOnly);

						declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
						AssertEquals("Should be read only when EXP", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
					}

					using (IECustomsDataRegistry.Instance.IsUCC6EnabledForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
						AssertEquals("Should allow selection of Built-in Types", true, declaration.JE_ApplicationCodeInfo.ReadOnly);

						declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
						AssertEquals("Should allow selection of Built-in Types - V1", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
					}
				}
			});
		}

		public void TestDefaultDataGrouping()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = string.Empty;
			AssertEquals("DefaultDataGrouping None: IE when non-UCC5", Core.Constants.CountryCodes.Ireland, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.None));

			declaration.JE_ApplicationCode = "INF";
			AssertEquals("DefaultDataGrouping None: IE when non-UCC5", Core.Constants.CountryCodes.Ireland, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.None));

			declaration.JE_ApplicationCode = "V1";
			AssertEquals("DefaultDataGrouping None: IE when UCC5 but non-IMP", Core.Constants.CountryCodes.Ireland, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.None));

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "V1";
			AssertEquals("DefaultDataGrouping None: IE when UCC5 IMP", "IE5", declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.None));
			AssertEquals("DefaultDataGrouping Tariff: IE even when UCC5", Core.Constants.CountryCodes.Ireland, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
			AssertEquals("DefaultDataGrouping CusProcedure: IE even when UCC5", Core.Constants.CountryCodes.Ireland, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
			AssertEquals("DefaultDataGrouping DutyRateCodes: IE even when UCC5", Core.Constants.CountryCodes.Ireland, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));

			declaration.JE_ApplicationCode = "V2";
			AssertEquals("DefaultDataGrouping None: IE when UCC6", Core.Constants.CountryCodes.Ireland, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.None));
		}

		public void TestDefaultLocationTypesWhenMessageTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("JE_LocationOtherInformation", Constants.TypeOfLocation.DesignatedLocation, declaration.JE_LocationOtherInformation);
			AssertEquals("JE_LocationQualifier", Constants.LocationQualifier.UNLoco, declaration.JE_LocationQualifier);
		}

		public void TestDefaultOwnerWhenMessageTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;

				var instruction = declaration.CustomsEntryInstructions[0];
				var instruction2 = declaration.CustomsEntryInstructions.AddNew();

				instruction.CEI_OH_Owner = ZGuid.BrettsGuid;
				instruction2.CEI_OH_Owner = ZGuid.BrettsGuid;
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				CombineAssertions("When the declaration is an import Owner can be present", () =>
				{
					AssertEquals("instruction", ZGuid.BrettsGuid, instruction.CEI_OH_Owner);
					AssertEquals("instruction2", ZGuid.BrettsGuid, instruction2.CEI_OH_Owner);
				});

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				CombineAssertions("When the type of declaration is no longer import Owner should be reset", () =>
				{
					AssertEquals("instruction", ZGuid.Empty, instruction.CEI_OH_Owner);
					AssertEquals("instruction2", ZGuid.Empty, instruction2.CEI_OH_Owner);
				});
			}
		}

		public void TestDefaultRegionOfDestinationWhenMessageTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				declaration.ZG_RegionOfDestination = "AA";

				AssertEquals("When JE_MessageType is Import ZG_RegionOfDestination can be present", "AA", declaration.ZG_RegionOfDestination);

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;

				AssertEquals("When JE_MessageType is no longer import ZG_RegionOfDestination should be empty", ZString.Empty, declaration.ZG_RegionOfDestination);
			}
		}

		public void TestDefaultInvoiceLinesRegionOfDestinationWhenMessageTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;

				var line1 = declaration.InvoiceLines.AddNew();
				line1.ZG_RegionOfDestination = "AA";
				var line2 = declaration.InvoiceLines.AddNew();
				line2.ZG_RegionOfDestination = "BB";

				CombineAssertions("When JE_MessageType is Import ZG_RegionOfDestination can be present", () =>
				{
					AssertEquals("AA", line1.ZG_RegionOfDestination);
					AssertEquals("BB", line2.ZG_RegionOfDestination);
				});

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				CombineAssertions("When JE_MessageType is no longer import ZG_RegionOfDestination should be empty", () =>
				{
					AssertEquals(ZString.Empty, line1.ZG_RegionOfDestination);
					AssertEquals(ZString.Empty, line2.ZG_RegionOfDestination);
				});
			}
		}

		public void TestDefermentPartyDocAddressRequirement_ValidateOrganisationPK()
		{
			const string messageError = "[BR2073] Deferment Party (Person Providing a Guarantee) is required when H3, H4, or (H1 when Requested Procedure is 44).";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var info = declaration.DefermentPartyDocAddress.OrganisationPKInfo;

			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;
			declaration.DefermentPartyDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("CEI_Style is 'H3' and JI_Procedure is empty", info, messageError);

			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
			declaration.DefermentPartyDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("CEI_Style is 'H4' and JI_Procedure is empty", info, messageError);

			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			declaration.DefermentPartyDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("CEI_Style is 'H1' and JI_Procedure is empty", info, messageError);

			invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._44;
			declaration.DefermentPartyDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("CEI_Style is 'H1' and JI_Procedure is '44'", info, messageError);

			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			declaration.DefermentPartyDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("CEI_Style is 'H2' and JI_Procedure is '44'", info, messageError);
		}

		public void TestDefaultLocationOfGoodsWhenDispatchChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("JE_LocationOfGoods - Dispatch Empty", string.Empty, declaration.JE_LocationOfGoods);
			declaration.JE_RL_NKOrigin = "GBBEL";
			AssertEquals("JE_LocationOfGoods - Dispatch Not IE", string.Empty, declaration.JE_LocationOfGoods);
			declaration.JE_RL_NKOrigin = "IEDUB";
			AssertEquals("JE_LocationOfGoods - Dispatch IE", "IEDUB", declaration.JE_LocationOfGoods);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_LocationOfGoods = string.Empty;
			declaration.JE_RL_NKOrigin = "IEDUB";
			AssertEquals("JE_LocationOfGoods - Declaration not Export", string.Empty, declaration.JE_LocationOfGoods);
		}

		public void TestDoNotDefaultSELToJE_DeclarantType()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DE1234567", Core.Constants.CountryCodes.Iceland);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = org.PK;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.JE_DeclarantType = ZString.Empty;
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			AssertNotEquals("No SEL", RepresentationTypeList.Codes._1Self, declaration.JE_DeclarantType);
		}

		public void TestCountryContext()
		{
			var declaration = Factory.New<JobDeclaration>();
			var holder = (Common.IApportionInvoiceHolder)declaration;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("Export", "IEEXP", holder.CountryContext);
		}

		public void TestMultipleKeysToUse()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			{
				CombineAssertions("JE_ApplicationCode Default: BLT", () =>
				{
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
					AssertSequencesEqual("ExitSummary", new[] { JobDeclaration.CaptionKeyBLT, JobDeclaration.CaptionKeyExportUCC6EXS, JobDeclaration.CaptionKeyExportUCC6 }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
					AssertSequencesEqual("ReExport", new[] { JobDeclaration.CaptionKeyBLT, JobDeclaration.CaptionKeyExportUCC6 }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
					AssertSequencesEqual("Export", new[] { JobDeclaration.CaptionKeyBLT, JobDeclaration.CaptionKeyExportUCC6 }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
					AssertSequencesEqual("Import", new[] { JobDeclaration.CaptionKeyImportUCC6 }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
					AssertSequencesEqual("MiscellaneousCustoms", new[] { JobDeclaration.CaptionKeyUCC }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = "@#";
					AssertSequencesEqual("Empty or invalid", new[] { JobDeclaration.CaptionKeyUCC }, declaration.MultipleKeysToUse);
					declaration.JE_ApplicationCode = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
					AssertEquals("not BLT", false, declaration.MultipleKeysToUse.Contains(JobDeclaration.CaptionKeyBLT));
				});
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			{
				CombineAssertions("JE_ApplicationCode Default: INF", () =>
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
					AssertSequencesEqual("ExitSummary", new[] { JobDeclaration.CaptionKeyExportUCC6EXS, JobDeclaration.CaptionKeyExportUCC6 }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
					AssertSequencesEqual("ReExport", new[] { JobDeclaration.CaptionKeyExportUCC6 }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
					AssertSequencesEqual("Export", new[] { JobDeclaration.CaptionKeyExportUCC6 }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
					AssertSequencesEqual("Import", new[] { JobDeclaration.CaptionKeyImportUCC6 }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
					AssertSequencesEqual("MiscellaneousCustoms", new[] { JobDeclaration.CaptionKeyUCC }, declaration.MultipleKeysToUse);
					declaration.JE_MessageType = "@#";
					AssertSequencesEqual("Empty or invalid", new[] { JobDeclaration.CaptionKeyUCC }, declaration.MultipleKeysToUse);
					declaration.JE_ApplicationCode = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
					AssertEquals("not INF", true, declaration.MultipleKeysToUse.Contains(JobDeclaration.CaptionKeyBLT));
				});
			}
		}

		public void TestMultipleKeysToUse_CaptionKeyUCC5() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("import", true, declaration.MultipleKeysToUse.Contains(JobDeclaration.CaptionKeyImportUCC5));
			}
		});

		public void TestCreateNewDocumentSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<JobDeclarationDocumentSupporter>(declaration.DocumentSupporter);
		}

		public void TestGetCusEntryHeaderDocumentSupporterCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertType<CusEntryHeaderDocumentSupporter>(declaration.GetCusEntryHeaderDocumentSupporter(entryHeader));
		}

		public void TestPiggyBackedDocAddressValidation() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("ImportJobDocAddressValidation", typeof(ImportDeclarationJobDocAddressValidation), declaration.PiggyBackedDocAddressValidation(declaration.DepotDocAddress).GetType());
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("ExportJobDocAddressValidation", typeof(ExportDeclarationJobDocAddressValidation), declaration.PiggyBackedDocAddressValidation(declaration.DepotDocAddress).GetType());
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("ExportJobDocAddressValidation", typeof(ExitSummaryDeclarationJobDocAddressValidation), declaration.PiggyBackedDocAddressValidation(declaration.DepotDocAddress).GetType());
		});

		public void TestDeclarationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "ABC123";
			entryHeader.MovementReferenceNumberSetter("22IEDU4EU155462431", ZDateTime.BrettsBirthday);
			AssertEquals("MRN", "22IEDU4EU155462431", declaration.DeclarationNumber);
			AssertEquals("Issue Date", ZDateTime.BrettsBirthday, declaration.EarliestCustomsEntryIssueDate);
		}

		public void TestDeclarationNumber_Multiple()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.MovementReferenceNumberSetter("22IEDU4EU155462111", ZDateTime.BrettsBirthday);
			entryHeader2.MovementReferenceNumberSetter("22IEDU4EU155462222", ZDateTime.BrettsBirthday.AddDays(1));
			AssertEquals("Multiple MRNs", "22IEDU4EU155462111,22IEDU4EU155462222", declaration.DeclarationNumber);
			AssertEquals("Issue Date", ZDateTime.BrettsBirthday, declaration.EarliestCustomsEntryIssueDate);
		}

		public void TestGetEntryStyleByEntryType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("JE_EntryStyle should have been set EX while setting JE_MessageType EXP.", MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion, declaration.JE_EntryStyle);
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
		}

		public void TestJE_MessageType_AddDefaultInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			declaration.JE_MessageType = ZString.Empty;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("declaration.CustomsEntryInstructions.Count", 1, declaration.CustomsEntryInstructions.Count);
			var instruction = declaration.CustomsEntryInstructions[0];
			AssertEquals("new added and default to B1", ExportDeclarationTypeList.Codes.B1, instruction.CEI_Style);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertEquals("declaration.CustomsEntryInstructions.Count", 1, declaration.CustomsEntryInstructions.Count);
			AssertSame("No new added", instruction, declaration.CustomsEntryInstructions[0]);
			AssertEquals("update to A3", ReExportDeclarationTypeList.Codes.A3, instruction.CEI_Style);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("declaration.CustomsEntryInstructions.Count", 1, declaration.CustomsEntryInstructions.Count);
			AssertSame("No new added", instruction, declaration.CustomsEntryInstructions[0]);
			AssertEquals("update to A1", ExitSummaryDeclarationTypeList.Codes.A1, instruction.CEI_Style);

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B2;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("declaration.CustomsEntryInstructions.Count", 1, declaration.CustomsEntryInstructions.Count);
			AssertSame("No new added", instruction, declaration.CustomsEntryInstructions[0]);
			AssertEquals("B2 is valid", true, instruction.Lookups.DeclarationTypeList.ContainsCode(ExportDeclarationTypeList.Codes.B2));
			AssertEquals("Not update when EXP and existing code is valid", ExportDeclarationTypeList.Codes.B2, instruction.CEI_Style);

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertEquals("declaration.CustomsEntryInstructions.Count", 2, declaration.CustomsEntryInstructions.Count);
			AssertEquals("Not update when there is more than one Entry Instruction", ExportDeclarationTypeList.Codes.B2, instruction.CEI_Style);
			AssertEquals("Not update when there is more than one Entry Instruction", ExportDeclarationTypeList.Codes.B1, instruction2.CEI_Style);

			instruction2.Delete();
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.CH_Status = LogicalStatusList.Codes.Sent;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("Not update when messaging started", ExportDeclarationTypeList.Codes.B2, instruction.CEI_Style);
		}

		public void TestJE_GoodsOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GoodsOrigin = "AU";
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_CountryOfOrigin = "IE";
			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_CountryOfOrigin = "FR";

			declaration.JE_GoodsOrigin = "IE";
			AssertEquals("JI_CountryOfOrigin cleared", "", line1["JI_CountryOfOrigin"]);
			AssertEquals("JI_CountryOfOrigin not cleared", "FR", line2["JI_CountryOfOrigin"]);
		}

		public void TestJE_RL_NKOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "AU001";
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_RN_NKCountryOfExport = "IE";
			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_RN_NKCountryOfExport = "FR";

			declaration.JE_RL_NKOrigin = "IE001";
			AssertEquals("JI_RN_NKCountryOfExport cleared", "", line1["JI_RN_NKCountryOfExport"]);
			AssertEquals("JI_RN_NKCountryOfExport not cleared", "FR", line2["JI_RN_NKCountryOfExport"]);
		}

		public void TestIsCoJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("IsCoJob: false for empty JE_EntryStyle.", false, declaration.IsCoJob);

			declaration.JE_EntryStyle = "XX";
			AssertEquals("IsCoJob: fFalse for unrecognized JE_EntryStyle.", false, declaration.IsCoJob);

			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;
			AssertEquals("IsCoJob: false for EX.", false, declaration.IsCoJob);

			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
			AssertEquals("IsCoJob: true for CO.", true, declaration.IsCoJob);
		}

		public void TestCountryOfExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CountryOfExport = "IE001";

			AssertEquals("CountryOfExport", "IE", declaration.CountryOfExport);
			AssertEquals("CountryOfExport", "IE001", declaration.JE_RL_NKOrigin);
		}

		public void TestTransportInlandOwnPropulsionUserControlVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			Assert("TransportInlandOwnPropulsionUserControl should not be Visible", !declaration.IsJE_TransportMeansRequired);

			declaration.JE_TransportModeInland = "OWN";
			Assert("TransportInlandOwnPropulsionUserControl should be Visible", declaration.IsJE_TransportMeansRequired);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			Assert("TransportInlandOwnPropulsionUserControl should not be Visible", !declaration.IsJE_TransportMeansRequired);
		}

		public void TestIsExitSummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("IsExitSummary", true, declaration.IsExitSummary);
			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("IsExitSummary", false, declaration.IsExitSummary);
		}

		public void TestIsReExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertEquals("IsReExport", true, declaration.IsReExport);
			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("IsReExport", false, declaration.IsReExport);
		}

		public void TestIsUCCCompliant()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(declaration.IsUCCCompliant);
		}

		public void TestIsNonTransportDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("IsNonTransportDeclarationType", true, declaration.IsNonTransportDeclarationType);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("IsNonTransportDeclarationType", false, declaration.IsNonTransportDeclarationType);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertEquals("IsNonTransportDeclarationType", false, declaration.IsNonTransportDeclarationType);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertEquals("IsNonTransportDeclarationType", true, declaration.IsNonTransportDeclarationType);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("IsNonTransportDeclarationType", true, declaration.IsNonTransportDeclarationType);
		}

		public void TestIsExpressConsignmentsOfExitSummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("False for empty ZG_SpecificCircumstanceIndicator.", false, declaration.IsExpressConsignmentsOfExitSummary);
			declaration.ZG_SpecificCircumstanceIndicator = "XXX";
			AssertEquals("False for unrecognized ZG_SpecificCircumstanceIndicator.", false, declaration.IsExpressConsignmentsOfExitSummary);
			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;
			AssertEquals("True for A20.", true, declaration.IsExpressConsignmentsOfExitSummary);
		}

		public void TestMergeManager()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<MergeManager>(declaration.MergeManager);
		}

		public void TestCustomsOffices()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<OfficeCodeCollection>(declaration.CustomsOffices);
		}

		public void TestClearJE_RN_NKTransportNationality_RAI() => AssertClearJE_RN_NKTransportNationality(Core.Constants.TransportModes.Rail);

		public void TestClearZG_Box18TransportNationality_FIX() => AssertClearZG_Box18TransportNationality(Core.Constants.TransportModes.FixedTransportInstallations);

		public void TestClearZG_Box18TransportNationality_MAI() => AssertClearZG_Box18TransportNationality(Core.Constants.TransportModes.Mail);

		public void TestClearZG_Box18TransportNationality_RAI() => AssertClearZG_Box18TransportNationality(Core.Constants.TransportModes.Rail);

		public void TestJE_TransportModeInland_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Inland M.O.T", declaration.JE_TransportModeInlandInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestJE_TransportModeInland_Caption_IMPUCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_TransportModeInlandInfo, JobDeclaration.CaptionKeyImportUCC6);
			AssertEquals("Caption", "Inland M.O.T", captionResourceString.Caption);
			AssertEquals("ShortCaption", "Inland", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "Inland M.O.T.", captionResourceString.MediumCaption);
			AssertEquals("FullDescription", "[19 04 001 000] Inland mode of transport", captionResourceString.FullDescription);
		}

		public void TestJE_TransportMode_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertEquals("Trans. Mode", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_TransportModeInfo, null).Caption);
		}

		public void TestJE_TransportMode_Caption_UCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_TransportModeInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Trans. Mode", captionResourceString.Caption);
			AssertEquals("FullDescription", "[19 03 001 000] Transport Mode at Border", captionResourceString.FullDescription);

			captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_TransportModeInfo, JobDeclaration.CaptionKeyImportUCC6);
			AssertEquals("Caption", "Trans. Mode", captionResourceString.Caption);
			AssertEquals("FullDescription", "[19 03 001 000] Transport Mode at Border", captionResourceString.FullDescription);
		}

		public void TestJE_TransportMeans_Caption_IMPUCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_TransportMeansInfo, JobDeclaration.CaptionKeyImportUCC6);
			AssertEquals("Caption", "Type of ID", captionResourceString.Caption);
			AssertEquals("MediumCaption", "ID Type", captionResourceString.MediumCaption);
			AssertEquals("FullDescription", "[19 06 061 000] Arrival transport means < Type of identification", captionResourceString.FullDescription);
		}

		public void TestJE_TransportMeans_Caption_EXPUCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_TransportMeansInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Type of ID", captionResourceString.Caption);
			AssertEquals("FullDescription", "[19 05 061 000] Type of Identification", captionResourceString.FullDescription);
		}

		public void TestZG_BorderTransportMeans_Caption_UCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().ZG_BorderTransportMeansInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Trans. ID.", captionResourceString.Caption);
			AssertEquals("FullDescription", "[19 08 061 000] Transport Identification Means at Border", captionResourceString.FullDescription);

			captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().ZG_BorderTransportMeansInfo, JobDeclaration.CaptionKeyImportUCC6);
			AssertEquals("Caption", "Border T.O.ID.", captionResourceString.Caption);
			AssertEquals("FullDescription", "[19 05 061 000] Departure Transport Means < Type of Identification", captionResourceString.FullDescription);
		}

		public void TestZG_Box18TransportNationality_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Nationality", declaration.ZG_Box18TransportNationalityInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestJE_RN_NKTransportNationality_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertEquals("Nationality", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_RN_NKTransportNationalityInfo, null).Caption);
		}

		public void TestJE_RN_NKTransportNationality_Caption_UCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_RN_NKTransportNationalityInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Nationality", captionResourceString.Caption);
			AssertEquals("FullDescription", "[19 08 062 000] Nationality", captionResourceString.FullDescription);
		}

		public void TestJE_EntryStyle_Caption_UCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_EntryStyleInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Declaration Type", captionResourceString.Caption);
			AssertEquals("FullDescription", "[11 01 001 000] Declaration Type", captionResourceString.FullDescription);
		}

		public void TestJE_ContainerMode_Caption_UCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_ContainerModeInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Container", captionResourceString.Caption);
			AssertEquals("FullDescription", "[19 01 001 000] Container Indicator", captionResourceString.FullDescription);

			captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_ContainerModeInfo, JobDeclaration.CaptionKeyImportUCC6);
			AssertEquals("Caption", "Container", captionResourceString.Caption);
			AssertEquals("FullDescription", "[19 01 001 000] Container Indicator", captionResourceString.FullDescription);
		}

		public void TestJE_TotalNoOfPacks_Caption_UCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_TotalNoOfPacksInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "No. Pkgs.", captionResourceString.Caption);
			AssertEquals("FullDescription", "Number of Packages", captionResourceString.FullDescription);
		}

		public void TestJE_OwnerRef_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OwnerRefInfo, declaration.MultipleKeysToUse, "Declarant's Ref", shortCaption: "Dec. Ref");
		}

		public void TestJE_OwnerRef_Caption_UCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_OwnerRefInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Owner Ref.", captionResourceString.Caption);
			AssertEquals("FullDescription", "Owner's Reference", captionResourceString.FullDescription);
		}

		public void TestJE_VoyageFlightNo_Caption_UCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_VoyageFlightNoInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Flight No.", captionResourceString.Caption);
			AssertEquals("FullDescription", "[19 08 017 000] Flight Number", captionResourceString.FullDescription);
		}

		public void TestJE_SubLocationOfGoods_MaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("MaxLength should be 3.", 3, declaration.JE_SubLocationOfGoodsInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestJE_SubLocationOfGoods_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringDataAttribute = declaration.JE_SubLocationOfGoodsInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Additional Identifier", resourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", "Add. Identifier", resourceStringDataAttribute.ShortCaption);
			});
		}

		public void TestJE_LocationQualifier_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_LocationQualifierInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Qualifier", captionResourceString.Caption);
				AssertEquals("[16 15 046 000] Qualifier of the identification", captionResourceString.FullDescription);
				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_LocationQualifierInfo, null);
				AssertEquals("Qualifier", captionResourceString.Caption);
				AssertEquals(string.Empty, captionResourceString.FullDescription);
			});
		}

		public void TestJE_LocationOtherInformation_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_LocationOtherInformationInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Type", captionResourceString.Caption);
				AssertEquals("[16 15 045 000] Type of Location", captionResourceString.FullDescription);
				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_LocationOtherInformationInfo, null);
				AssertEquals("Type", captionResourceString.Caption);
				AssertEquals(string.Empty, captionResourceString.FullDescription);
			});
		}

		public void TestJE_Calc_LocationOfGoodsCtry_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringDataAttribute = declaration.JE_Calc_LocationOfGoodsCtryInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Country", resourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", "Ctry.", resourceStringDataAttribute.ShortCaption);
			});
		}

		public void TestJE_Calc_LocationOfGoodsCtry()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_Calc_LocationOfGoodsCtry", Core.Constants.CountryCodes.Ireland, declaration.JE_Calc_LocationOfGoodsCtry);
		}

		public void TestCustomsOffices_NoExceptionWhenAccessing()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertNoExceptionThrown(() =>
			{
				_ = declaration.CustomsOffices;
			});
		}

		public void TestDefaultMandatoryCustomsOffices_DoNothingWhenMessageTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			CombineAssertions(() =>
			{
				var expectedCount = 0;
				AssertEquals("Before", expectedCount, declaration.CustomsOffices.Count);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals("JE_MessageTypeInfo.HasChanges", true, declaration.JE_MessageTypeInfo.HasChanges);
				AssertEquals("After", expectedCount, declaration.CustomsOffices.Count);
			});
		}

		public void TestOfficeCodeType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(OfficeCode), ((Integration.Customs.ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.OfficeCode]);
		}

		public void TestJE_DeclarantType_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_DeclarantTypeInfo, declaration.MultipleKeysToUse);
			AssertEquals("Rep. Status", captionResourceString.Caption);
		}

		public void TestJE_DeclarantType_Caption_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_DeclarantTypeInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Rep. Status", captionResourceString.Caption);
		}

		public void TestJE_PaymentMethod_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_PaymentMethodInfo, null);
			AssertEquals("Payment Method", captionResourceString.Caption);
		}

		public void TestJE_PaymentMethod_DefaultExciseMethodOfPayment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var excise1 = invoiceLine1.CusLineTariffDetails.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var excise2 = invoiceLine2.CusLineTariffDetails.AddNew();

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.A;

			CombineAssertions("Set ZG_MethodOfPayment from JE_PaymentMethod.", () =>
			{
				AssertEquals("excise1", PaymentMethodList.Codes.A, excise1.ZG_MethodOfPayment);
				AssertEquals("excise2", PaymentMethodList.Codes.A, excise2.ZG_MethodOfPayment);

				declaration.JE_PaymentMethod = PaymentMethodList.Codes.J;
				AssertEquals("ZG_MethodOfPayment keep synchronized with JE_PaymentMethod.", PaymentMethodList.Codes.J, excise1.ZG_MethodOfPayment);
				AssertEquals("ZG_MethodOfPayment keep synchronized with JE_PaymentMethod.", PaymentMethodList.Codes.J, excise2.ZG_MethodOfPayment);
			});
		}

		public void TestGetCusSupportingInfoTypes_PreviousDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)declaration).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		}

		public void TestGetCusSupportingInfoTypes_AdditionalInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)declaration).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestGetCusSupportingInfoTypes_SupportingDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)declaration).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		}

		public void TestPreviousDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<PreviousDocumentCollection>(declaration.PreviousDocuments);
		}

		public void TestAdditionalInfos()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<AdditionalInfoCollection>(declaration.AdditionalInfos);
		}

		public void TestSupportingDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<SupportingDocumentCollection>(declaration.SupportingDocuments);
		}

		public void TestCustomsEntryInstructions()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>>(declaration.CustomsEntryInstructions);
		}

		public void TestCustomsEntryHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>>(declaration.CustomsEntryHeaders);
		}

		public void TestEntryCreationStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			AssertType<ImportEntryCreationStrategy>("Should Be Enterprise.Customs.IE.Business.Declaration.ImportEntryCreationStrategy", declaration.CreateEntryCreationStrategy());
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			AssertType<ImportUCC5EntryCreationStrategy>("Should Be Enterprise.Customs.IE.Business.Declaration.ImportUCC5EntryCreationStrategy", declaration.CreateEntryCreationStrategy());
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportEntryCreationStrategy>("Should Be Enterprise.Customs.IE.Business.Declaration.ExportEntryCreationStrategy", declaration.CreateEntryCreationStrategy());
		}

		public void TestJE_CustomsOffice_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_CustomsOfficeInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Office of Lodgement", captionResourceString.Caption);
		}

		public void TestJE_CustomsOffice_Caption_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_CustomsOfficeInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Office of Export", captionResourceString.Caption);
		}

		public void TestJE_CustomsOffice_Caption_ExitSummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_CustomsOfficeInfo, declaration.MultipleKeysToUse, "Office of Lodgement");
		}

		public void TestCaptionFallback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_CustomsOfficeInfo, declaration.MultipleKeysToUse, "Office of Lodgement");
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OA_DeclarantAddressInfo, declaration.MultipleKeysToUse, "Declarant", fullDescription: "[13 05 000 000] Declarant");

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_CustomsOfficeInfo, declaration.MultipleKeysToUse, "Office of Export");
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OA_DeclarantAddressInfo, declaration.MultipleKeysToUse, "Declarant", fullDescription: "[13 05 000 000] Declarant");

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_CustomsOfficeInfo, declaration.MultipleKeysToUse, "Office of Lodgement");
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OA_DeclarantAddressInfo, declaration.MultipleKeysToUse, "Declarant", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[13 05 000 000] Declarant");
		}

		public void TestJE_OA_DeclarantAddress_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_DeclarantAddressInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Declarant", captionResourceString.Caption);
			AssertEquals("Caption", "[13 05 000 000] Declarant", captionResourceString.FullDescription);
		}

		public void TestJE_OA_DeclarantAddress_Caption_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_DeclarantAddressInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Declarant", captionResourceString.Caption);
				AssertEquals("FullDescription", "[13 05 000 000] Declarant", captionResourceString.FullDescription);
			});
		}

		public void TestJE_OA_DeclarantAddress_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_DeclarantAddressInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[3/18] Declarant ID", captionResourceString.Caption);
				AssertEquals("FullDescription", "[3/18] Declarant Identification Number", captionResourceString.FullDescription);
				AssertEquals("ShortCaption", "Declarant", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[3/18] Declarant", captionResourceString.MediumCaption);
			});
		}

		public void TestJE_OA_Representative_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_RepresentativeInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Representative", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Represent.", captionResourceString.ShortCaption);
				AssertEquals("FullDescription", "[13 06 000 000] Representative", captionResourceString.FullDescription);
			});
		}

		public void TestJE_OA_Representative_Caption_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_RepresentativeInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Representative", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Represent.", captionResourceString.ShortCaption);
				AssertEquals("FullDescription", "[13 06 000 000] Representative", captionResourceString.FullDescription);
			});
		}

		public void TestJE_OA_Representative_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_RepresentativeInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[3/19 & 3/20] Representative (ID)", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Representative", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[3/19] Representative", captionResourceString.MediumCaption);
				AssertEquals("FullDescription", "[3/19] Representative & [3/20] Representative Identification Number", captionResourceString.FullDescription);
			});
		}

		public void TestJE_OA_SellerAddress_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_SellerAddressInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Seller", captionResourceString.Caption);
		}

		public void TestJE_OA_SellerAddress_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_SellerAddressInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[3/24 & 3/25] Seller (ID)", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Seller", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[3/24] Seller", captionResourceString.MediumCaption);
				AssertEquals("FullDescription", "[3/24] Buyer & [3/25] Seller Identification Number", captionResourceString.FullDescription);
			});
		}

		public void TestJE_OH_Buyer_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OH_BuyerInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Buyer", captionResourceString.Caption);
		}

		public void TestJE_OH_Buyer_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OH_BuyerInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[3/26 & 3/27] Buyer (ID)", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Buyer", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[3/26] Buyer", captionResourceString.MediumCaption);
				AssertEquals("FullDescription", "[3/26] Buyer & [3/27] Buyer Identification Number", captionResourceString.FullDescription);
			});
		}

		public void TestJE_OH_DutyPayer_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OH_DutyPayerInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[3/46] Duty Payer ID", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Duty Payer", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[3/46] Duty Payer", captionResourceString.MediumCaption);
				AssertEquals("FullDescription", "[3/46] Person paying the customs duty identification number", captionResourceString.FullDescription);
			});
		}

		public void TestJE_RL_NKFinalDestination_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_RL_NKFinalDestinationInfo, (string)null, "Destination");
		}

		public void TestJE_RL_NKOrigin_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_RL_NKOriginInfo, (string)null, "Dispatch");
		}

		public void TestCustomsOfficeRequirementHelper()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<JobDeclarationCustomsOfficeRequirementHelper>(declaration.CustomsOfficeRequirementHelper);
		}

		public void TestValidation_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			AssertType<ImportJobDeclarationValidation>(declaration.Validation);
		}

		public void TestValidation_Import_UCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			AssertType<ImportUCC6JobDeclarationValidation>(declaration.Validation);
		}

		public void TestValidation_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationValidation>(declaration.Validation);
		}

		public void TestValidation_ExitSummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertType<ExitSummaryJobDeclarationValidation>(declaration.Validation);
		}

		public void TestValidation_ReExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertType<ReExportJobDeclarationValidation>(declaration.Validation);
		}

		public void TestValidation_MiscellaneousCustoms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationValidation>(declaration.Validation);
		}

		public void TestImportLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationLookups>(declaration.Lookups);
		}

		public void TestExportLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationLookups>(declaration.Lookups);
		}

		public void TestMiscellaneousCustomsLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationLookups>(declaration.Lookups);
		}

		public void TestGetPackagesCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<BaseDeclarationLevelPackageCollection<Package>>(declaration.Packages);
		}

		public void TestInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InvoiceHeaderActiveCollection>(declaration.Invoices);
		}

		public void TestInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InvoiceLineCompleteCollection>(declaration.InvoiceLines);
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestJobComInvoiceGroupHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>>(declaration.JobComInvoiceGroupHeaders);
		}

		public void TestLocalCurrencyCoreOverride()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(Core.Constants.CurrencyCodes.Ireland, declaration.LocalCurrencyCode);
		}

		public void TestAreMultipleEntryInstructionsAllowed()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.AreMultipleEntryInstructionsAllowed);
		}

		public void TestSetDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("OfficeOfExit", ZString.Empty, declaration.OfficeOfExit);
				AssertEquals("CustomsOffices.Count", 0, declaration.CustomsOffices.Count);
			});
		}

		public void TestJE_VesselName_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Vessel", declaration.JE_VesselNameInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestBox18TransportNationalityVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("AIR", true, declaration.Box18TransportNationalityRequired);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("SEA", true, declaration.Box18TransportNationalityRequired);

				declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				AssertEquals("FIX", false, declaration.Box18TransportNationalityRequired);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("MAI", false, declaration.Box18TransportNationalityRequired);

				declaration.JE_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
				AssertEquals("OWN", false, declaration.Box18TransportNationalityRequired);

				declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("IWT", false, declaration.Box18TransportNationalityRequired);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals("RAI", false, declaration.Box18TransportNationalityRequired);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("ROA", false, declaration.Box18TransportNationalityRequired);
			});
		}

		public void TestIsTransportNationalityMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			CombineAssertions("", () =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("IsTransportNationalityMandatory true for IMP AIR.", true, declaration.IsTransportNationalityMandatory);

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("IsTransportNationalityMandatory is false when EXP AIR when VesselName empty.", false, declaration.IsTransportNationalityMandatory);

				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
				declaration.JE_VoyageFlightNo = "V001";
				AssertEquals("IsTransportNationalityMandatory is true when EXP AIR when ActiveTransportMeansID(JE_VoyageFlightNo) not empty.", true, declaration.IsTransportNationalityMandatory);
			});
		}

		public void TestActiveTransportMeansID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VesselName = "VES001";
			AssertEquals("ActiveTransportMeansID", ZString.Empty, declaration.ActiveTransportMeansID);

			declaration.JE_VoyageFlightNo = "V001";
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
			AssertEquals("ActiveTransportMeansID=>JE_VoyageFlightNo", "V001", declaration.ActiveTransportMeansID);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._10;
			declaration.JE_LloydsIMO = "LLOYDS";
			AssertEquals("ActiveTransportMeansID=>JE_LloydsIMO", "LLOYDS", declaration.ActiveTransportMeansID);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
			declaration.JE_AircraftRegistration = "AIR001";
			AssertEquals("ActiveTransportMeansID=>JE_AircraftRegistration", "AIR001", declaration.ActiveTransportMeansID);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "VES001";
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._11;
			AssertEquals("ActiveTransportMeansID=>JE_VesselName", "VES001", declaration.ActiveTransportMeansID);
		}

		public void TestJE_EntryStyle_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertEquals("Declaration Type", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_EntryStyleInfo, null).Caption);
		}

		public void TestJE_ContainerMode_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertEquals("Container", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_ContainerModeInfo, null).Caption);
		}

		public void TestJE_TotalNoOfPacks_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertEquals("No. Pkgs.", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_TotalNoOfPacksInfo, null).Caption);
		}

		public void TestJE_LocationOfGoods_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_LocationOfGoodsInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Goods Location", captionResourceString.Caption);
				AssertEquals("Goods Location - [16 15 036 000] UN/LOCODE", captionResourceString.FullDescription);
				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobDeclaration>().JE_LocationOfGoodsInfo, null);
				AssertEquals("Goods Location", captionResourceString.Caption);
				AssertEquals(string.Empty, captionResourceString.FullDescription);
			});
		}

		public void TestJE_ShipmentIncoTerm_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_ShipmentIncoTermInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Incoterm", captionResourceString.Caption);
		}

		public void TestJE_ShipmentIncoTerm_Caption_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_ShipmentIncoTermInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Incoterm", captionResourceString.Caption);
		}

		public void TestJE_ShipmentIncoTermPlace_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_ShipmentIncoTermPlaceInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Place", captionResourceString.Caption);
		}

		public void TestJE_ShipmentIncoTermPlace_Caption_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_ShipmentIncoTermPlaceInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Place", captionResourceString.Caption);
		}

		public void TestDateAtOriginCreatesAdditionalCode1D23()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday;
				AssertEquals("Count", 1, instruction.AdditionalInfos.Count);
				var addInfo = instruction.AdditionalInfos[0];
				AssertEquals("1D23 AdditionalCode Added - code", "1D23", addInfo.CSI_Code);
				AssertEquals("1D23 AdditionalCode Added - reference", "197109180000", addInfo.CSI_ReferenceNumber);
				AssertEquals("1D23 AdditionalCode Added - sub-type", "REF", addInfo.CSI_SubType);
				addInfo.Delete();
				declaration.JE_DateAtOrigin = ZDateTime.Empty;
				AssertEquals("When Emtpy date, should not create", 0, instruction.AdditionalInfos.Count);
				declaration.JE_MessageType = "IMP";
				declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday.AddDays(1);
				AssertEquals("When IMP, should not create", 0, instruction.AdditionalInfos.Count);
			});
		}

		public void TestDateAtFinalDestinationCreateOrUpdateInstructionSupportingDocumentCode1D24()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				declaration.JE_DateAtFinalDestination = ZDateTime.BrettsBirthday;
				AssertEquals("Count", 1, instruction.SupportingDocuments.Count);
				var addInfo = instruction.SupportingDocuments[0];
				AssertEquals("1D23 AdditionalCode Added - code", "1D24", addInfo.CSI_Code);
				AssertEquals("1D23 AdditionalCode Added - reference", "197109180000", addInfo.CSI_ReferenceNumber);
			});
		}

		public void TestDateAtOriginUpdatesAdditionalCode1D23()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			var additionalInfo1_1 = instruction1.AdditionalInfos.AddNew();
			additionalInfo1_1.CSI_SubType = "REF";
			additionalInfo1_1.CSI_Code = "1D23";
			additionalInfo1_1.CSI_ReferenceNumber = "202210111010";
			var additionalInfo1_2 = instruction1.AdditionalInfos.AddNew();
			additionalInfo1_2.CSI_SubType = "REF";
			additionalInfo1_2.CSI_Code = "1D24";
			additionalInfo1_2.CSI_ReferenceNumber = "202210111010";
			var additionalInfo2_1 = instruction2.AdditionalInfos.AddNew();
			additionalInfo2_1.CSI_SubType = "REF";
			additionalInfo2_1.CSI_Code = "1D23";
			additionalInfo2_1.CSI_ReferenceNumber = "202210111010";
			var additionalInfo2_2 = instruction2.AdditionalInfos.AddNew();
			additionalInfo2_2.CSI_SubType = "INF";
			additionalInfo2_2.CSI_Code = "1D23";
			additionalInfo2_2.CSI_ReferenceNumber = "202210111010";

			CombineAssertions(() =>
			{
				declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday;
				AssertEquals("Should update", "197109180000", additionalInfo1_1.CSI_ReferenceNumber);
				AssertEquals("Should not update - different CSI_Code", "202210111010", additionalInfo1_2.CSI_ReferenceNumber);

				AssertEquals("Should update", "197109180000", additionalInfo2_1.CSI_ReferenceNumber);
				AssertEquals("Should not update - different CSI_SubType", "202210111010", additionalInfo2_2.CSI_ReferenceNumber);
				declaration.JE_DateAtOrigin = ZDateTime.Empty;
				AssertEquals("Should not update - emtpy date", "197109180000", additionalInfo1_1.CSI_ReferenceNumber);
				declaration.JE_MessageType = "IMP";
				declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday.AddDays(1);
				AssertEquals("Should not update - Import", "197109180000", additionalInfo1_1.CSI_ReferenceNumber);
			});
		}

		public void TestExitControlTabVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("True for export", true, declaration.ExitControlTabVisible);
			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("False for Misc", false, declaration.ExitControlTabVisible);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("False for Import", false, declaration.ExitControlTabVisible);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertEquals("True for REX", true, declaration.ExitControlTabVisible);
		}

		public void TestValidationModes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			declaration.RecalculateValidationModesOnAllLevel();
			AssertEquals("NONE for declaration without an entry with MRN.", EU.Business.Declaration.ValidationModes.None, declaration.ValidationModes);
			entryHeader.MovementReferenceNumberSetter("MRN001");
			declaration.RecalculateValidationModesOnAllLevel();
			AssertEquals("None, Amendment for declaration with an entry with MRN.", (ValidationModes)5, declaration.ValidationModes);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			declaration.RecalculateValidationModesOnAllLevel();
			AssertEquals("NONE for declaration in REX job.", EU.Business.Declaration.ValidationModes.None, declaration.ValidationModes);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.RecalculateValidationModesOnAllLevel();
			AssertEquals("None, Amendment for Import declaration with an entry with MRN.", (ValidationModes)5, declaration.ValidationModes);
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("Gets GlbCompany & RefCountry for JE_EntryStatus. Expected loading.", true);
		}

		public void TestZG_AgreedPlaceCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(String.Empty, DataBoundResourceStrings.GetDataForProperty(declaration.ZG_AgreedPlaceCodeInfo).FullDescription);
		}

		public void TestZG_AgreedPlaceCodeValidationSupport()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.ZG_AgreedPlaceCodeValidationSupport);
		}

		public void TestJE_DefermentAccountNumber_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringDataAttribute = declaration.JE_DefermentAccountNumberInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Account Number", resourceStringDataAttribute.Caption);
		}

		public void TestInventorySelectionHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InventorySelectionHeader>(declaration.InventorySelectionHeader);
		}

		public void TestJobDocAddressValidation() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertEquals("ImportJobDocAddressValidation", typeof(ImportDeclarationJobDocAddressValidation), declaration.PiggyBackedDocAddressValidation(declaration.DepotDocAddress).GetType());
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("ExportJobDocAddressValidation", typeof(ExportDeclarationJobDocAddressValidation), declaration.PiggyBackedDocAddressValidation(declaration.DepotDocAddress).GetType());
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("ExportJobDocAddressValidation", typeof(ExitSummaryDeclarationJobDocAddressValidation), declaration.PiggyBackedDocAddressValidation(declaration.DepotDocAddress).GetType());
		});

		public void TestIsUcc5Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", configurationValue: true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				Assert(declaration.IsUCC5AndIsImport);

				declaration.JE_MessageType = "COM";
				Assert(!declaration.IsUCC5AndIsImport);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", configurationValue: false))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				Assert(!declaration.IsUCC5AndIsImport);
			}
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.BaseJobDeclaration);

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>();

		void AssertClearJE_RN_NKTransportNationality(string transportMode)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Ireland;
			declaration.JE_TransportMode = transportMode;
			AssertEquals(ZString.Empty, declaration.JE_RN_NKTransportNationality);
		}

		void AssertClearZG_Box18TransportNationality(string transportMode)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_Box18TransportNationality = Core.Constants.CountryCodes.Ireland;
			declaration.JE_TransportMode = transportMode;
			AssertEquals(ZString.Empty, declaration.ZG_Box18TransportNationality);
		}

		public void TestIsITFApplicationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = string.Empty;
			Assert(!declaration.IsITFApplicationCode);
			declaration.JE_ApplicationCode = "ab";
			Assert(!declaration.IsITFApplicationCode);
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.Interfaced;
			Assert(declaration.IsITFApplicationCode);
		}

		public void TestAllowGoodsLocationFromImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			AssertEquals("Allow Location of Goods to be exported to UXML when declaration type is IMP", true, declaration.AllowGoodsLocationFromImport);
			declaration.JE_MessageType = "EXP";
			AssertEquals("Allow Location of Goods to be exported to UXML when declaration type is EXP", false, declaration.AllowGoodsLocationFromImport);
		}

		public void TestSupportValidateCustomsMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supporter = (IValidateForCustomsMessagingSupporter)declaration;
			Assert("Export should support ValidateCustomsMessaging Message", supporter.SupportValidateCustomsMessaging);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert("Import-UCC5 should support ValidateCustomsMessaging Message", supporter.SupportValidateCustomsMessaging);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			Assert("Import-UCC6 should not support ValidateCustomsMessaging Message", !supporter.SupportValidateCustomsMessaging);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert("Should not support ValidateCustomsMessaging Message when SendToCustoms disabled", !supporter.SupportValidateCustomsMessaging);
		}

		public void TestSupportEntryDeclarationMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration;
			Assert("Export should support EntryDeclaration Message", supporter.SupportEntryDeclarationMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert("Import-UCC5 should support EntryDeclaration Message", supporter.SupportEntryDeclarationMessage);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			Assert("Import-UCC6 should not support EntryDeclaration Message", !supporter.SupportEntryDeclarationMessage);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert("Should not support EntryDeclaration Message when SendToCustoms disabled", !supporter.SupportEntryDeclarationMessage);
		}

		public void TestGetReasonForNotSupportEntryDeclarationMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration;
			AssertEquals("You have to enable 'Send To Customs' for Imports Registry to support Send Entry/Declaration Message trigger for Imports.", supporter.GetReasonForNotSupportEntryDeclarationMessage);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("The Send Entry/Declaration Message trigger is only supported for Export or Import-UCC5 Declaration.", supporter.GetReasonForNotSupportEntryDeclarationMessage);

			declaration.JE_MessageType = "XXX";
			AssertEquals("The Send Entry/Declaration Message trigger is only supported for Export or Import-UCC5 Declaration.", supporter.GetReasonForNotSupportEntryDeclarationMessage);
		}

		public void TestCreateEntryDeclarationMessageProcessor()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration;
			AssertType<AutoIM515MessageProcessor>("Export should invoke IE515", supporter.CreateEntryDeclarationMessageProcessor());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertType<AutoIM415MessageProcessor>("Import-UCC5 should invoke IE515", supporter.CreateEntryDeclarationMessageProcessor());
		}

		public void TestDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, declaration.DeclarationType);

			var instruction = declaration.CustomsEntryInstructions.AddNew().CEI_Style = "AA";
			AssertEquals("AA", declaration.DeclarationType);

			declaration.CustomsEntryInstructions.AddNew().CEI_Style = "BB";
			declaration.CustomsEntryInstructions.AddNew().CEI_Style = "CC";
			AssertEquals("AA,BB,CC", declaration.DeclarationType);
		}

		public void TestEntrySubStyleForCommonTransit()
		{
			PrepareForCalculatingEntryStyleBasedOnFallBackProcedure(out var orgHeaderLV, out var orgHeaderIT, out var orgHeaderCH, out var orgHeaderGF, out var orgHeaderGB, out var orgHeaderPL);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true))
			{
				var fallbackInfoProvider = (IEntryStyleCalculatorFallbackInfoProvider)declaration;
				CombineAssertions(() =>
				{
					AssertEquals("UCC5 Should be IM", EntryStyleListImport.Codes.ImportNormal, fallbackInfoProvider.GetEntrySubStyleForCommonTransit(Factory.New<RefCountry>()));
				});
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", false))
			{
				var fallbackInfoProvider = (IEntryStyleCalculatorFallbackInfoProvider)declaration;
				CombineAssertions(() =>
				{
					AssertEquals("UCC5 Should be IM", EntryStyleListImport.Codes.ImportFromEFTAMember, fallbackInfoProvider.GetEntrySubStyleForCommonTransit(Factory.New<RefCountry>()));
				});
			}
		}

		void PrepareForCalculatingEntryStyleBasedOnFallBackProcedure(out OrgHeader orgHeaderLV, out OrgHeader orgHeaderIT, out OrgHeader orgHeaderCH, out OrgHeader orgHeaderGF, out OrgHeader orgHeaderGB, out OrgHeader orgHeaderPL)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var euctp = helper.CreateTradeGroup("EUN", "EUCTP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, "CH", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var eusft = helper.CreateTradeGroup("EUN", "EUSFT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusft, "GF", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var eusfr = helper.CreateTradeGroup("EUN", "EUSFR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusfr, "GB", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(eusfr, "PL", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			orgHeaderLV = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderLV.OH_RL_NKClosestPort = "LV";

			//An EU country without the special territories
			orgHeaderIT = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderIT.OH_RL_NKClosestPort = "IT";

			//A country eligible to a common transit procedure
			orgHeaderCH = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderCH.OH_RL_NKClosestPort = "CH";

			//An EU special territories
			orgHeaderGF = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderGF.OH_RL_NKClosestPort = "GF";

			//A non EU country that has the special territories
			orgHeaderGB = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderGB.OH_RL_NKClosestPort = "GB";

			//An EU country that has the special territories
			orgHeaderPL = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderPL.OH_RL_NKClosestPort = "PL";

			Factory.Save();
		}
	}
}
