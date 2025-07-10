using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportEntryMessageSendingAction))]
	sealed class ImportEntryMessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTransportIDInLand()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, action.TransportIDInLand);
				declaration.ZG_Box18TransportID = "Transport ID";
				AssertEquals("Valid", "Transport ID", action.TransportIDInLand);
			});
		}

		public void TestTransportIDInLand_Caption()
		{
			AssertEquals("[18] Transport ID (Inland)", DataBoundResourceStrings.GetDataForProperty(typeof(ImportEntryMessageSendingAction), nameof(ImportEntryMessageSendingAction.TransportIDInLand)).Caption);
		}

		public void TestGoodsLocation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, action.GoodsLocation);
				declaration.JE_LocationOfGoods = "Location of Goods";
				AssertEquals("Valid", "Location of Goods", action.GoodsLocation);
			});
		}

		public void TestGoodsLocation_Caption()
		{
			AssertEquals("[30] Goods Location", DataBoundResourceStrings.GetDataForProperty(typeof(ImportEntryMessageSendingAction), nameof(ImportEntryMessageSendingAction.GoodsLocation)).Caption);
		}

		public void TestInvoice()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No invoice", null, action.Invoice);

				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				AssertEquals("Added an invoice", invoice, action.Invoice);
			});
		}

		public void TestEntryInstruction()
		{
			AssertSame(entryInstruction, action.EntryInstruction);
		}

		public void TestRegistrationNumber()
		{
			AssertEquals(entry.EntryNumber, action.Details);
		}

		public void TestDeclarationType()
		{
			AssertEquals(entry.EntryInstruction.CEI_Style, action.DeclarationType);
		}

		public void TestSubStyle()
		{
			AssertEquals(entry.EntryInstruction.CEI_SubStyle, action.SubStyle);
		}

		public void TestDescription()
		{
			AssertEquals(entry.EntryInstruction.CEI_Description, action.Description);
		}

		public void TestEntryStatus()
		{
			AssertEquals(entry.CH_EntryStatus, action.EntryStatus);
		}

		public void TestCusCon()
		{
			AssertEquals(false, action.CusCon);
		}

		public void TestCusCon_ReadOnly_EntryNumber()
		{
			entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RC1;
			entryInstruction.CEI_SubStyle = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			CombineAssertions(() =>
			{
				entry.EntryNumber = "123456";
				AssertEquals("Registration Number not starts with ATA", true, action.CusConInfo.ReadOnly);

				entry.EntryNumber = "ATA123456789012345678";
				AssertEquals("Registration Number starts with ATA", false, action.CusConInfo.ReadOnly);

				entry.EntryNumber = "24DE12345678901234";
				AssertEquals("Registration Number not a working MRN", true, action.CusConInfo.ReadOnly);

				entry.EntryNumber = "24DE12345A78901234";
				AssertEquals("Registration Number is working MRN", false, action.CusConInfo.ReadOnly);
			});
		}

		public void TestCusCon_ReadOnly_SubStyle()
		{
			var enabledSubStyles = new[]
			{
				EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,
				EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB,
				EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC
			};
			var readOnlySubStyles = new EU.Business.EntrySubStyleList().GetAllCodes().Except(enabledSubStyles);

			entry.EntryNumber = "ATA123456789012345678";
			entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RC1;

			CombineAssertions(() =>
			{
				foreach (var substyle in enabledSubStyles)
				{
					entryInstruction.CEI_SubStyle = substyle;
					AssertEquals($"SubStyle {substyle}", false, action.CusConInfo.ReadOnly);
				}

				foreach (var substyle in readOnlySubStyles)
				{
					entryInstruction.CEI_SubStyle = substyle;
					AssertEquals($"SubStyle {substyle}", true, action.CusConInfo.ReadOnly);
				}
			});
		}

		public void TestCusCon_ReadOnly_EntryStatus()
		{
			entry.EntryNumber = "ATA123456789012345678";
			entryInstruction.CEI_SubStyle = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			CombineAssertions(() =>
			{
				entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RC1;
				AssertEquals("EntryStatus RC1", false, action.CusConInfo.ReadOnly);

				entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RL2;
				AssertEquals("EntryStatus RL2", false, action.CusConInfo.ReadOnly);

				entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RL3;
				AssertEquals("EntryStatus RL3", false, action.CusConInfo.ReadOnly);

				entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RL5;
				AssertEquals("EntryStatus RL5", false, action.CusConInfo.ReadOnly);

				entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RLB;
				AssertEquals("EntryStatus RLB", false, action.CusConInfo.ReadOnly);

				entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RC2;
				AssertEquals("EntryStatus RC2", true, action.CusConInfo.ReadOnly);
			});
		}

		public void TestRegistrationNumberReadOnlyWhenEntryStatus_Empty_SubStyle_C() => AssertRegistrationNumberReadOnly(ZString.Empty, ImportSubStyleList.Codes.C, true);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_Empty_SubStyle_D() => AssertRegistrationNumberReadOnly(ZString.Empty, ImportSubStyleList.Codes.D, false);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_Empty_SubStyle_E() => AssertRegistrationNumberReadOnly(ZString.Empty, ImportSubStyleList.Codes.E, false);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_Empty_SubStyle_F() => AssertRegistrationNumberReadOnly(ZString.Empty, ImportSubStyleList.Codes.F, false);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_REJ_SubStyle_C() => AssertRegistrationNumberReadOnly(UniversalReferenceConstants.EntryStatus.REJ, ImportSubStyleList.Codes.C, true);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_REJ_SubStyle_D() => AssertRegistrationNumberReadOnly(UniversalReferenceConstants.EntryStatus.REJ, ImportSubStyleList.Codes.D, false);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_REJ_SubStyle_E() => AssertRegistrationNumberReadOnly(UniversalReferenceConstants.EntryStatus.REJ, ImportSubStyleList.Codes.E, false);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_REJ_SubStyle_F() => AssertRegistrationNumberReadOnly(UniversalReferenceConstants.EntryStatus.REJ, ImportSubStyleList.Codes.F, false);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_CCM_SubStyle_C() => AssertRegistrationNumberReadOnly(UniversalReferenceConstants.EntryStatus.CCM, ImportSubStyleList.Codes.C, true);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_CCM_SubStyle_D() => AssertRegistrationNumberReadOnly(UniversalReferenceConstants.EntryStatus.CCM, ImportSubStyleList.Codes.D, true);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_CCM_SubStyle_E() => AssertRegistrationNumberReadOnly(UniversalReferenceConstants.EntryStatus.CCM, ImportSubStyleList.Codes.E, true);
		public void TestRegistrationNumberReadOnlyWhenEntryStatus_CCM_SubStyle_F() => AssertRegistrationNumberReadOnly(UniversalReferenceConstants.EntryStatus.CCM, ImportSubStyleList.Codes.F, true);

		void AssertRegistrationNumberReadOnly(string entryStatus, string importSubStyle, bool expectedReadOnly)
		{
			entry.CH_EntryStatus = entryStatus;
			entryInstruction.CEI_SubStyle = importSubStyle;
			AssertEquals(expectedReadOnly, action.RegistrationNumberInfo.ReadOnly);
		}

		public void Test_RegistrationNumberValidation()
		{
			var importSubStyles = new ImportSubStyleList().GetAllCodes();

			var firstDigitsErrMsg = "Registration Number digit 1-3 must start with 'ATA'.";
			var lengthErrMsg = "Registration Number must have 21 characters.";

			entry.CH_EntryStatus = ZString.Empty;

			foreach (var subStyle in importSubStyles)
			{
				entryInstruction.CEI_SubStyle = subStyle;
				var validationErrorsExpected = SubStyleForEditableRegistrationNumber.Contains(subStyle);

				CheckValidation(validationErrorsExpected);
			}

			entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.REJ;

			foreach (var subStyle in importSubStyles)
			{
				entryInstruction.CEI_SubStyle = subStyle;

				CheckValidation(false);
			}

			entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.CCM;

			foreach (var subStyle in importSubStyles)
			{
				entryInstruction.CEI_SubStyle = subStyle;

				CheckValidation(false);
			}

			void CheckValidation(bool errorsExpected)
			{
				CombineAssertions(() =>
				{
					action.RegistrationNumber = "TESTMRN123";
					if (errorsExpected)
					{
						AssertHasMessageError(action.RegistrationNumberInfo, firstDigitsErrMsg);
						AssertHasMessageError(action.RegistrationNumberInfo, lengthErrMsg);
					}
					else
					{
						AssertNoMessageError(action.RegistrationNumberInfo, firstDigitsErrMsg);
						AssertNoMessageError(action.RegistrationNumberInfo, lengthErrMsg);
					}

					action.RegistrationNumber = "ATA123";
					if (errorsExpected)
					{
						AssertHasMessageError(action.RegistrationNumberInfo, lengthErrMsg);
					}
					else
					{
						AssertNoMessageError(action.RegistrationNumberInfo, lengthErrMsg);
					}
					AssertNoMessageError(action.RegistrationNumberInfo, firstDigitsErrMsg);

					action.RegistrationNumber = "ATA123456789123456789";
					AssertNoMessageError(action.RegistrationNumberInfo, lengthErrMsg);
					AssertNoMessageError(action.RegistrationNumberInfo, firstDigitsErrMsg);
				});
			}
		}

		public void Test_CusCon_AutomaticallySet()
		{
			var allSubStyles = new ImportSubStyleList().GetAllCodes();

			// EntryStatus values where CusCon is automatically set + additional values to test
			var entryStatusToTest = EntryStatusForCusConAutoSet.Concat(new[]
			{
				UniversalReferenceConstants.EntryStatus.CON,
				UniversalReferenceConstants.EntryStatus.CWT,
				UniversalReferenceConstants.EntryStatus.DWH
			}).ToList();

			TestSubStylesAndEntryStatuses("ATA123456789123456789", true);

			TestSubStylesAndEntryStatuses("OTHER1234", false);

			void TestSubStylesAndEntryStatuses(ZString regNo, bool registrationNumberValid)
			{
				foreach (var subStyle in allSubStyles)
				{
					entryInstruction.CEI_SubStyle = subStyle;
					foreach (var entryStatus in entryStatusToTest)
					{
						entry.CH_EntryStatus = entryStatus;

						var cusConSet = registrationNumberValid && SubStyleForCusConAutoSet.Contains(subStyle) && EntryStatusForCusConAutoSet.Contains(entryStatus);

						action.RegistrationNumber = regNo;
						AssertEquals(cusConSet, action.CusCon);
					}
				}
			}
		}

		public void Test_Action_CanSend()
		{
			action.RegistrationNumber = "MRN12345";
			AssertEquals(true, action.CanSend);

			action.RegistrationNumber = ZString.Empty;
			AssertEquals(false, action.CanSend);
		}

		public void TestShouldSend_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ShouldSend should be editable by default", false, action.ShouldSendInfo.ReadOnly);

				entry.CH_Status = EDIMessage.Status.Sent;
				AssertEquals("ShouldSend should be readonly when CH_Status is SNT ", true, action.ShouldSendInfo.ReadOnly);

				entry.CH_Status = EDIMessage.Status.Received;
				foreach (var entryStatus in new ZString[]
						 {
							 UniversalReferenceConstants.EntryStatus.RC2,
							 UniversalReferenceConstants.EntryStatus.RL5,
							 UniversalReferenceConstants.EntryStatus.TX4,
							 UniversalReferenceConstants.EntryStatus.TX5,
							 UniversalReferenceConstants.EntryStatus.TX6,
							 UniversalReferenceConstants.EntryStatus.TX7,
							 UniversalReferenceConstants.EntryStatus.TX8,
							 UniversalReferenceConstants.EntryStatus.TXF,
							 UniversalReferenceConstants.EntryStatus.TXR,
							 UniversalReferenceConstants.EntryStatus.TRA
						 })
				{
					entry.CH_EntryStatus = entryStatus;
					AssertEquals($"ShouldSend should be readonly when CH_Status = {entryStatus}", true, action.ShouldSendInfo.ReadOnly);
				}

				entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RC1;
				AssertEquals("ShouldSend should be editable when CH_Status = RC1", false, action.ShouldSendInfo.ReadOnly);
			});
		}

		static readonly ImmutableHashSet<string> EntryStatusForCusConAutoSet = ImmutableHashSet.Create(
			UniversalReferenceConstants.EntryStatus.RC1,
			UniversalReferenceConstants.EntryStatus.RL2,
			UniversalReferenceConstants.EntryStatus.RL3,
			UniversalReferenceConstants.EntryStatus.RL5,
			UniversalReferenceConstants.EntryStatus.RLB,
			UniversalReferenceConstants.EntryStatus.REJ);

		static readonly ImmutableHashSet<string> SubStyleForCusConAutoSet = ImmutableHashSet.Create(
			ImportSubStyleList.Codes.D,
			ImportSubStyleList.Codes.E,
			ImportSubStyleList.Codes.F);

		static ImmutableHashSet<string> SubStyleForEditableRegistrationNumber => SubStyleForCusConAutoSet;

		protected override BusinessObject GetNewBusinessObject() => new ImportEntryMessageSendingAction(entry, actionParent);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			entry = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			action = (ImportEntryMessageSendingAction)GetNewBusinessObject();
			actionParent = new ImportDeclarationMessageSendingActionParentForTest(declaration);
		}
		ImportEntryMessageSendingAction action;
		CusEntryHeader entry;
		CusEntryInstruction entryInstruction;
		JobDeclaration declaration;
		ImportDeclarationMessageSendingActionParentForTest actionParent;
	}
}
