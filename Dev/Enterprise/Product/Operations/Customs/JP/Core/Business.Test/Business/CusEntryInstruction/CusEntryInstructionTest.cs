using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(ValueTypeList.Codes.L, EntryInstruction.CEI_ValueType);
		}

		public void TestDeleteChildlessEntryHeader()
		{
			var childlessEntryHeader = Declaration.ActiveEntryHeaders.AddNew();
			childlessEntryHeader.CH_CEI_Instruction = EntryInstruction.PK;
			EntryInstruction.Factory.Save();

			CombineAssertions(() =>
			{
				EntryInstruction.Delete();
				Assert("childlessEntryHeader should be deleted when entry instruction is deleted.", childlessEntryHeader.IsDeleted);
				Assert("childlessEntryHeader should not be active when entry instruction is deleted.", !childlessEntryHeader.IsActive);
			});
		}

		public void TestCEI_SpecialCargoCode()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(EntryInstruction.CEI_SpecialCargoCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("ShortCaption", "SPC.", resourceStringData.ShortCaption);
				AssertEquals("Caption", "Special Cargo Code", resourceStringData.Caption);
			});
		}

		public void TestSpecialCargoCode()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "AOG", "EPG");
			CombineAssertions(() =>
			{
				EntryInstruction.CEI_SpecialCargoCode = "AOG";
				AssertEquals("AOG Code", "AOG", EntryInstruction.SpecialCargoCode.ZZD_Code);
				AssertEquals("AOG Description", "AOG Description", EntryInstruction.SpecialCargoCode.ZZD_Description);

				EntryInstruction.CEI_SpecialCargoCode = "EPG";
				AssertEquals("EPG Code", "EPG", EntryInstruction.SpecialCargoCode.ZZD_Code);
				AssertEquals("EPG Description", "EPG Description", EntryInstruction.SpecialCargoCode.ZZD_Description);
			});
		}

		public void TestCEI_DisplaySequence()
		{
			Assert(EntryInstruction.CEI_DisplaySequenceInfo.ReadOnly);

			var entryInstruction1 = Declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = Declaration.CustomsEntryInstructions.AddNew();

			var zshort1 = new ZShort(1);
			var zshort2 = new ZShort(2);
			AssertEquals(zshort1, EntryInstruction.CEI_DisplaySequence);
			AssertEquals(zshort2, entryInstruction1.CEI_DisplaySequence);
			AssertEquals(new ZShort(3), entryInstruction2.CEI_DisplaySequence);

			Declaration.CustomsEntryInstructions.RemoveAndDelete(EntryInstruction);
			AssertEquals(zshort1, entryInstruction1.CEI_DisplaySequence);
			AssertEquals(zshort2, entryInstruction2.CEI_DisplaySequence);

			var entryInstructionProperty = DataBoundResourceStrings.GetDataForProperty(EntryInstruction.CEI_DisplaySequenceInfo);

			CombineAssertions(() =>
			{
				AssertEquals("ShortCaption", "#", entryInstructionProperty.ShortCaption);
				AssertEquals("MediumCaption", "Seq #", entryInstructionProperty.MediumCaption);
				AssertEquals("Caption", "Display Sequence", entryInstructionProperty.Caption);
			});
		}

		public void TestTradeType_GetSetCharValue()
		{
			EntryInstruction.TradeTypeThirdChar = TradeTypeThirdChar.Codes.A;
			Assert(EntryInstruction.TradeTypeFirstChar.Equals(ZString.Empty));
			Assert(EntryInstruction.TradeTypeSecondChar.Equals(ZString.Empty));
			AssertEquals(TradeTypeThirdChar.Codes.A, EntryInstruction.TradeTypeThirdChar);
			AssertEquals("__1", EntryInstruction.CEI_TradeType);

			EntryInstruction.TradeTypeThirdChar = ZString.Empty;
			Assert(EntryInstruction.TradeTypeThirdChar.Equals(ZString.Empty));
			AssertEquals("___", EntryInstruction.CEI_TradeType);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)EntryInstruction).GetCusSupportingInfoTypes();
			AssertEquals("CER - ApprovalCertificateInfo", typeof(ApprovalCertificateInfo), actualTypes[CusSupportingInfoTypeList.Codes.ApprovalCertificate]);
			AssertEquals("ECR - ExpectedMoveInDestination", typeof(MoveInDestination), actualTypes[CusSupportingInfoTypeList.Codes.ExpectedMoveInDestination]);
		}

		public void TestGetFetchStrategies()
		{
			var expectedTypes = new[] { typeof(CusSupportingInfoTypeSupporterFetchStrategy) };
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)EntryInstruction).GetFetchStrategies().Select(c => c.GetType());

			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void TestMarksAndNumbers()
		{
			var text = "MARKS AND NUMBERS SHOULD BE LESS THAN 140 CHARACTERS";

			EntryInstruction.JP_MarksAndNumbers = text;
			Factory.Save();

			var note = EntryInstruction.Notes.FindByDescription(Constants.StmNoteDescriptions.MarksAndNumbersDescription).Single();
			AssertEquals("Should add a new note from the value.", text, note.ST_NoteText);

			EntryInstruction.Reload();
			AssertEquals("JP_MarksAndNumbers", text, EntryInstruction.JP_MarksAndNumbers);

			EntryInstruction.JP_MarksAndNumbers = string.Empty;
			var notes = EntryInstruction.Notes.FindByDescription(Constants.StmNoteDescriptions.MarksAndNumbersDescription);
			AssertEquals("Should delete the note as the value is empty.", 0, notes.Length);
		}

		public void TestJP_ECRNotes()
		{
			var text = "ECRNotes SHOULD BE LESS THAN 140 CHARACTERS";

			EntryInstruction.JP_ECRNotes = text;
			Factory.Save();

			var note = EntryInstruction.Notes.FindByDescription(Constants.StmNoteDescriptions.ECRNotesDescription).Single();
			AssertEquals("Should add a new note from the value.", text, note.ST_NoteText);

			EntryInstruction.Reload();
			AssertEquals("JP_ECRNotes", text, EntryInstruction.JP_ECRNotes);

			EntryInstruction.JP_ECRNotes = string.Empty;
			var notes = EntryInstruction.Notes.FindByDescription(Constants.StmNoteDescriptions.ECRNotesDescription);
			AssertEquals("Should delete the note as the value is empty.", 0, notes.Length);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(EntryInstruction.JP_ECRNotesInfo);
			AssertEquals("JP_ECRNotes Caption", "ECR Notes", resourceStringData.Caption);
		}

		public void TestNotesForCustoms()
		{
			CombineAssertions(() =>
			{
				EntryInstruction.CEI_BondedLocationName = "XXX";
				AssertEquals("Default", "蔵入等先保税地域名: XXX", EntryInstruction.JP_CustomsNotes);
				Assert("Readonly when default", EntryInstruction.JP_CustomsNotesInfo.ReadOnly);

				EntryInstruction.JP_CustomsNotes_Override = true;
				Assert("Editable when overridden", !EntryInstruction.JP_CustomsNotesInfo.ReadOnly);

				EntryInstruction.JP_CustomsNotes = "Overridden text";
				EntryInstruction.JP_CustomsNotes_Override = false;
				AssertEquals("Back to default when cancelling override", "蔵入等先保税地域名: XXX", EntryInstruction.JP_CustomsNotes);

				EntryInstruction.JP_CustomsNotes = "IN ORDER TO GENERATE A FULL NACCS FILE, WE NEED TO ADD TEH MISSING ITEMS TO THE CUSTOMS DECLARATION.";
				Factory.Save();

				Declaration.Reload();
				AssertEquals("JP_CustomsNotes", "IN ORDER TO GENERATE A FULL NACCS FILE, WE NEED TO ADD TEH MISSING ITEMS TO THE CUSTOMS DECLARATION.", EntryInstruction.JP_CustomsNotes);
			});
		}

		public void TestIsExportControlEntryNumSystemGenerated()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, "123", false);
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, "123", true);
			CombineAssertions(() =>
			{
				AssertEquals("entryInstruction1", false, entryInstruction1.IsExportControlEntryNumSystemGenerated);
				AssertEquals("entryInstruction2", true, entryInstruction2.IsExportControlEntryNumSystemGenerated);
			});
		}

		public void TestDeleteExportControlEntryNumEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, "123", false);
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, "123", true);
			CombineAssertions(() =>
			{
				AssertEquals("entryInstruction1", false, entryInstruction1.DeleteExportControlEntryNumEnabled);
				AssertEquals("entryInstruction2", true, entryInstruction2.DeleteExportControlEntryNumEnabled);

				entryInstruction2.ExportControlNumberInfo.ClearValue();
				AssertEquals("entryInstruction2 when ExportControlNumber is empty", false, entryInstruction2.DeleteExportControlEntryNumEnabled);
			});
		}

		public void TestCEI_BillNumber()
		{
			EntryInstruction.CEI_BillNumber = "123";
			var entryNumber = CusEntryNumber.Load(EntryInstruction, CusEntryNumberTypes.JP.BillNumber, EntryInstruction.CountryCode);
			AssertEquals("123", entryNumber.CE_EntryNum);

			entryNumber.CE_EntryNum = "456";
			AssertEquals("456", EntryInstruction.CEI_BillNumber);

			EntryInstruction.CEI_BillNumber = ZString.Empty;
			Assert(entryNumber.IsDeleted);

			var info = EntryInstruction.CEI_BillNumberInfo;
			AssertEquals("Air Caption", "AWB Number", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, CusEntryInstruction.AirCaptionKey).Caption);
			AssertEquals("Sea Caption", "B/L Number", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, CusEntryInstruction.SeaCaptionKey).Caption);
		}

		public void TestMoveInNotice()
		{
			EntryInstruction.MoveInNotice = "123";
			var entryNumber = CusEntryNumber.Load(EntryInstruction, CusEntryNumberTypes.JP.MoveInNotice, EntryInstruction.CountryCode);
			AssertEquals("123", entryNumber.CE_EntryNum);

			entryNumber.CE_EntryNum = "456";
			AssertEquals("456", EntryInstruction.MoveInNotice);
			AssertEquals("Move-In Notice", DataBoundResourceStrings.GetDataForProperty(EntryInstruction.MoveInNoticeInfo).Caption);
		}

		public void TestRequestMoveInNotice()
		{
			AssertEquals(true, EntryInstruction.RequestMoveInNotice);
			EntryInstruction.RequestMoveInNotice = false;
			var entryNumber = CusEntryNumber.Load(EntryInstruction, CusEntryNumberTypes.JP.MoveInNotice, EntryInstruction.CountryCode);
			AssertEquals(false, entryNumber.CE_EntryIsSystemGenerated);

			entryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals(true, EntryInstruction.RequestMoveInNotice);
			AssertEquals("Request Move-In Notice", DataBoundResourceStrings.GetDataForProperty(EntryInstruction.RequestMoveInNoticeInfo).Caption);
		}

		public void TestMoveInDate()
		{
			var moveInDestinationInfo = EntryInstruction.MoveInDestinationInfos.AddNew();
			moveInDestinationInfo.CSI_DateOfIssue = new ZDateTime(2025, 5, 15);
			AssertEquals(new ZDateTime(2025, 5, 15), EntryInstruction.MoveInDate);

			EntryInstruction.MoveInDate = new ZDateTime(2025, 5, 26);
			AssertEquals(new ZDateTime(2025, 5, 26), moveInDestinationInfo.CSI_DateOfIssue);
			AssertEquals("Move-In Date", DataBoundResourceStrings.GetDataForProperty(EntryInstruction.MoveInDateInfo).Caption);
		}

		public void TestMoveInDestination()
		{
			var moveInDestinationInfo = EntryInstruction.MoveInDestinationInfos.AddNew();
			moveInDestinationInfo.CSI_Code = "123";
			AssertEquals("123", EntryInstruction.MoveInDestination);

			EntryInstruction.MoveInDestination = "456";
			AssertEquals("456", moveInDestinationInfo.CSI_Code);
			AssertEquals("Move-In Destination", DataBoundResourceStrings.GetDataForProperty(EntryInstruction.MoveInDestinationInfo).Caption);
		}

		public void TestMoveInQuantity()
		{
			var moveInDestinationInfo = EntryInstruction.MoveInDestinationInfos.AddNew();
			moveInDestinationInfo.CSI_Quantity = 1m;
			AssertEquals(1m, EntryInstruction.MoveInQuantity);

			EntryInstruction.MoveInQuantity = 2m;
			AssertEquals(2m, moveInDestinationInfo.CSI_Quantity);

			var info = EntryInstruction.MoveInQuantityInfo;
			AssertEquals("Move-In Quantity", DataBoundResourceStrings.GetDataForProperty(info).Caption);
			AssertHasDecimalPlacesAttribute(info, 0);
		}

		public void TestMoveInWeight()
		{
			var moveInDestinationInfo = EntryInstruction.MoveInDestinationInfos.AddNew();
			moveInDestinationInfo.CSI_Quantity2 = 1m;
			AssertEquals(1m, EntryInstruction.MoveInWeight);

			EntryInstruction.MoveInWeight = 2m;
			AssertEquals(2m, moveInDestinationInfo.CSI_Quantity2);

			var info = EntryInstruction.MoveInWeightInfo;
			AssertEquals("Move-In Weight", DataBoundResourceStrings.GetDataForProperty(info).Caption);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertHasDecimalPlacesAttribute(info, 3);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertHasDecimalPlacesAttribute(info, 1);
		}

		public void TestCEI_GrossWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(3, entryInstruction.JPGrossWeightDecimalPlacesNum);

			declaration.JE_TransportMode = "AIR";
			declaration.JE_MessageType = "IMP";

			AssertEquals(1, entryInstruction.JPGrossWeightDecimalPlacesNum);
		}

		public void TestNotesForBroker()
		{
			EntryInstruction.JP_BrokersNotes = "BROKERS NOTES CAN ONLY BE 70 CHARACTERS";
			Factory.Save();

			declaration.Reload();
			AssertEquals("JP_BrokersNotes", "BROKERS NOTES CAN ONLY BE 70 CHARACTERS", EntryInstruction.JP_BrokersNotes);
		}

		public void TestNotesForCargoOwner()
		{
			EntryInstruction.JP_OwnersNotes = "OWNER NOTES BEHAVE THE SAME AS BROKER NOTES";
			Factory.Save();

			Declaration.Reload();
			AssertEquals("JP_OwnersNotes", "OWNER NOTES BEHAVE THE SAME AS BROKER NOTES", EntryInstruction.JP_OwnersNotes);
		}

		public void TestCustomsWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_GrossWeight = 16m;
			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<DecimalPlacesAttribute>(entryInstruction.GetType(), "CEI_CustomsWeight", includesInherit: true, attribute => attribute.DecimalPlaces == 3);

				entryInstruction.CEI_GrossWeightUnit = Weight.Kilograms;
				AssertEquals("Kilograms CEI_CustomsWeight", 16m, entryInstruction.CEI_CustomsWeight);
				AssertEquals("Kilograms CEI_CustomsWeightUnit", CustomsWeightUnitList.Codes.Kilograms, entryInstruction.CEI_CustomsWeightUnit);

				entryInstruction.CEI_GrossWeightUnit = Weight.Tonnes;
				AssertEquals("Tonnes CEI_CustomsWeight", 16m, entryInstruction.CEI_CustomsWeight);
				AssertEquals("Tonnes CEI_CustomsWeightUnit", CustomsWeightUnitList.Codes.Tonnes, entryInstruction.CEI_CustomsWeightUnit);

				entryInstruction.CEI_GrossWeightUnit = Weight.Pounds;
				AssertEquals("Pounds CEI_CustomsWeight", 16m, entryInstruction.CEI_CustomsWeight);
				AssertEquals("Pounds CEI_CustomsWeightUnit", CustomsWeightUnitList.Codes.Pound, entryInstruction.CEI_CustomsWeightUnit);

				entryInstruction.CEI_GrossWeightUnit = Weight.Kilotonnes;
				AssertEquals("Kilotonnes CEI_CustomsWeight", 16000000m, entryInstruction.CEI_CustomsWeight);
				AssertEquals("Kilotonnes CEI_CustomsWeightUnit", CustomsWeightUnitList.Codes.Kilograms, entryInstruction.CEI_CustomsWeightUnit);
			});

			CombineAssertions(() =>
			{
				entryInstruction.CEI_CustomsWeight = 16m;
				entryInstruction.CEI_CustomsWeightUnit = CustomsWeightUnitList.Codes.Kilograms;
				AssertEquals("Kilograms CEI_GrossWeight", 16m, entryInstruction.CEI_GrossWeight);
				AssertEquals("Kilograms CEI_GrossWeightUnit", Weight.Kilograms, entryInstruction.CEI_GrossWeightUnit);

				entryInstruction.CEI_CustomsWeightUnit = CustomsWeightUnitList.Codes.Tonnes;
				AssertEquals("Tonnes CEI_GrossWeight", 16m, entryInstruction.CEI_GrossWeight);
				AssertEquals("Tonnes CEI_GrossWeightUnit", Weight.Tonnes, entryInstruction.CEI_GrossWeightUnit);

				entryInstruction.CEI_CustomsWeightUnit = CustomsWeightUnitList.Codes.Pound;
				AssertEquals("Pounds CEI_GrossWeight", 16m, entryInstruction.CEI_GrossWeight);
				AssertEquals("Pounds CEI_GrossWeightUnit", Weight.Pounds, entryInstruction.CEI_GrossWeightUnit);
			});
		}

		public void TestCustomsVolume()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<DecimalPlacesAttribute>(entryInstruction.GetType(), "CEI_CustomsVolume", includesInherit: true, attribute => attribute.DecimalPlaces == 3);

				entryInstruction.CEI_Volume = 16m;
				entryInstruction.CEI_VolumeUnit = VolumeList.Codes.BoardFoot;
				AssertEquals("BoardFoot CEI_CustomsVolume", 16m, entryInstruction.CEI_CustomsVolume);
				AssertEquals("BoardFoot CEI_CustomsVolumeUnit", CustomsVolumeUnitList.Codes.BoardFeet, entryInstruction.CEI_CustomsVolumeUnit);

				entryInstruction.CEI_VolumeUnit = Volume.CubicMetres;
				AssertEquals("CubicMetres CEI_CustomsVolume", 16m, entryInstruction.CEI_CustomsVolume);
				AssertEquals("CubicMetres CEI_CustomsVolumeUnit", CustomsVolumeUnitList.Codes.CubicMeters, entryInstruction.CEI_CustomsVolumeUnit);

				entryInstruction.CEI_VolumeUnit = Volume.CubicFeet;
				AssertEquals("CubicFeet CEI_CustomsVolume", 16m, entryInstruction.CEI_CustomsVolume);
				AssertEquals("CubicFeet CEI_CustomsVolumeUnit", CustomsVolumeUnitList.Codes.CubicFeet, entryInstruction.CEI_CustomsVolumeUnit);

				entryInstruction.CEI_VolumeUnit = Volume.Litre;
				AssertEquals("Litre CEI_CustomsVolume", 0.016m, entryInstruction.CEI_CustomsVolume);
				AssertEquals("Litre CEI_CustomsVolumeUnit", CustomsVolumeUnitList.Codes.CubicMeters, entryInstruction.CEI_CustomsVolumeUnit);
			});

			CombineAssertions(() =>
			{
				entryInstruction.CEI_CustomsVolume = 16m;
				entryInstruction.CEI_CustomsVolumeUnit = CustomsVolumeUnitList.Codes.BoardFeet;
				AssertEquals("BoardFoot CEI_Volume", 16m, entryInstruction.CEI_Volume);
				AssertEquals("BoardFoot CEI_VolumeUnit", VolumeList.Codes.BoardFoot, entryInstruction.CEI_VolumeUnit);

				entryInstruction.CEI_CustomsVolumeUnit = CustomsVolumeUnitList.Codes.CubicMeters;
				AssertEquals("CubicMetres CEI_Volume", 16m, entryInstruction.CEI_CustomsVolume);
				AssertEquals("CubicMetres CEI_VolumeUnit", Volume.CubicMetres, entryInstruction.CEI_VolumeUnit);

				entryInstruction.CEI_CustomsVolumeUnit = CustomsVolumeUnitList.Codes.CubicFeet;
				AssertEquals("CubicFeet CEI_Volume", 16m, entryInstruction.CEI_CustomsVolume);
				AssertEquals("CubicFeet CEI_VolumeUnit", Volume.CubicFeet, entryInstruction.CEI_VolumeUnit);
			});
		}

		public void TestCEI_DeclarationCondition()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			EntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
			entryInstruction.CEI_DeclarationCondition = ExportDeclarationConditionListWhenDeclarationTypeIsNotEorR.Codes.I;
			var declarationConditionResourceStringData = DataBoundResourceStrings.GetDataForProperty(EntryInstruction.CEI_DeclarationConditionInfo);
			var declarationConditionDescriptionResourceStringData = DataBoundResourceStrings.GetDataForProperty(EntryInstruction.DeclarationConditionDescriptionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("CEI_DeclarationCondition Caption", "Declaration Condition", declarationConditionResourceStringData.Caption);
				AssertEquals("CEI_DeclarationCondition Short Caption", "Condition", declarationConditionResourceStringData.ShortCaption);
				AssertEquals("DeclarationConditionDescription Caption", "Declaration Condition Description", declarationConditionDescriptionResourceStringData.Caption);
				AssertEquals("DeclarationConditionDescription Medium Caption", "Condition Description", declarationConditionDescriptionResourceStringData.MediumCaption);
				AssertEquals("DeclarationConditionDescription Short Caption", "Condition Desc.", declarationConditionDescriptionResourceStringData.ShortCaption);
				AssertEquals("DeclarationConditionDescription Full Description", "The description of the Declaration Condition.", declarationConditionDescriptionResourceStringData.FullDescription);
				AssertEquals("DeclarationConditionDescription", "搬入時申告の登録", EntryInstruction.DeclarationConditionDescription);
			});
		}

		public void TestCEI_ECRCargoTypeWhenCEI_StyleChanged()
		{
			var targetInfo = EntryInstruction.CEI_ECRCargoTypeInfo;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			EntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.R;
			AssertEquals(CargoTypeList.Codes.R, EntryInstruction.CEI_ECRCargoType);

			targetInfo.ClearValue();
			EntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
			AssertEquals(CargoTypeList.Codes.R, EntryInstruction.CEI_ECRCargoType);

			targetInfo.ClearValue();
			EntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.N;
			AssertEquals(CargoTypeList.Codes.T, EntryInstruction.CEI_ECRCargoType);

			targetInfo.ClearValue();
			EntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.M;
			AssertEquals(CargoTypeList.Codes.T, EntryInstruction.CEI_ECRCargoType);

			targetInfo.ClearValue();
			EntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.T;
			AssertEquals(CargoTypeList.Codes.T, EntryInstruction.CEI_ECRCargoType);

			targetInfo.ClearValue();
			EntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.E;
			AssertEquals(ZString.Empty, EntryInstruction.CEI_ECRCargoType);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			EntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.R;
			AssertEquals(ZString.Empty, EntryInstruction.CEI_ECRCargoType);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			EntryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
			AssertEquals(ZString.Empty, EntryInstruction.CEI_ECRCargoType);
		}

		public void TestCEI_ECRCargoTypeCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(EntryInstruction.CEI_ECRCargoTypeInfo);
			AssertEquals("CEI_ECRCargoType Caption", "ECR Cargo Type", resourceStringData.Caption);
		}

		public void TestCEI_CargoQuantityUnit()
		{
			var targetInfo = EntryInstruction.CEI_CargoQuantityUnitInfo;
			EntryInstruction.CEI_CargoQuantityUnit = ZString.Empty;
			Declaration.JE_TransportMode = "AIR";
			AssertEquals("NO", EntryInstruction.CEI_CargoQuantityUnit);
			AssertEquals(true, targetInfo.ReadOnly);

			Declaration.JE_TransportMode = "SEA";
			AssertEquals(ZString.Empty, EntryInstruction.CEI_CargoQuantityUnit);
			AssertEquals(false, targetInfo.ReadOnly);
		}

		public void TestCodeProperty()
		{
			var codeProperty = CodePropertyAttribute.CodePropertyNameFromType(GetExpectedBusinessObjectType());

			EntryInstruction.CEI_DisplaySequence = 123;
			AssertEquals("123", EntryInstruction.CodeProperty);
		}

		public void TestDescriptionProperty()
		{
			var descriptionProperty = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(GetExpectedBusinessObjectType());
			EntryInstruction.CEI_Description = "CEI_Description";
			AssertEquals("CEI_Description", EntryInstruction.DescriptionProperty);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = TransportModes.Sea;
			EntryInstruction.ExportControlNumber = "123";
			EntryInstruction.CEI_BillNumber = "456";
			AssertEquals("CEI_Description|123", EntryInstruction.DescriptionProperty);

			EntryInstruction.CEI_Style = "E";
			AssertEquals("CEI_Description|123|E", EntryInstruction.DescriptionProperty);

			EntryInstruction.CEI_SubStyle = "C";
			AssertEquals("CEI_Description|123|E|C", EntryInstruction.DescriptionProperty);

			EntryInstruction.CEI_Style = null;
			AssertEquals("CEI_Description|123|C", EntryInstruction.DescriptionProperty);

			EntryInstruction.CEI_Description = null;
			AssertEquals("123|C", EntryInstruction.DescriptionProperty);

			Declaration.JE_TransportMode = TransportModes.Air;
			AssertEquals("456|C", EntryInstruction.DescriptionProperty);
		}

		public void TestDefaultBillNumberIfNeeded()
		{
			Declaration.JE_HouseBill = "H0001";
			Declaration.JE_MasterBill = "M0001";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = TransportModes.Sea;
			var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("EXP, SEA", ZString.Empty, entryInstruction.CEI_BillNumber);

			Declaration.JE_TransportMode = TransportModes.Air;
			entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("EXP, AIR", "H0001", entryInstruction.CEI_BillNumber);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("IMP, AIR", ZString.Empty, entryInstruction.CEI_BillNumber);

			Declaration.JE_TransportMode = TransportModes.Sea;
			entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("IMP, SEA", ZString.Empty, entryInstruction.CEI_BillNumber);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = TransportModes.Air;
			Declaration.JE_HouseBill = ZString.Empty;
			entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("EXP, AIR", "M0001", entryInstruction.CEI_BillNumber);
		}

		public void TestEntryInstructionDescription()
		{
			Declaration.JE_TransportMode = TransportModes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			EntryInstruction.CEI_Description = "TEST DESCRIPTION";
			EntryInstruction.CEI_GoodsDescription = "TEST GOODS DESCRIPTION";

			AssertEquals("TEST GOODS DESCRIPTION", EntryInstruction.EntryInstructionDescription);

			Declaration.JE_TransportMode = TransportModes.Air;
			AssertEquals("TEST DESCRIPTION", EntryInstruction.EntryInstructionDescription);
		}

		public void TestCEI_JE()
		{
			var bondedWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			bondedWarehouse.OH_FullName = "Test Company";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.WarehouseDocAddress.OrganisationPK = bondedWarehouse.PK;
			var instruction1 = Declaration.CustomsEntryInstructions.AddNew();

			AssertEquals("99999", instruction1.CEI_BondedLocationCode);
			AssertEquals("Test Company", instruction1.CEI_BondedLocationName);

			bondedWarehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2HDN8", Core.Constants.CountryCodes.Japan);
			var instruction2 = Declaration.CustomsEntryInstructions.AddNew();

			AssertEquals("2HDN8", instruction2.CEI_BondedLocationCode);
			AssertEquals(ZString.Empty, instruction2.CEI_BondedLocationName);
		}

		public void TestIsECR()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			EntryInstruction.ExportControlNumber = "123";
			Assert(!EntryInstruction.IsECR);

			EntryInstruction.ExportControlNumber = ZString.Empty;
			Assert(EntryInstruction.IsECR);

			EntryInstruction.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, "123", true);
			Assert(EntryInstruction.IsECR);
		}

		public void TestCEI_SubStyleMaxLength()
		{
			AssertEquals(1, EntryInstruction.CEI_SubStyleInfo.MaxLength);
		}

		public void TestGetTariffAttributesByKey()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Japan, Universal.Constants.TariffTypes.Import);
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "JP";
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Japan, tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");
			helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "AN", tariff1);
			helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "BN", tariff1);
			helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "CN", tariff1);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Japan, tariffType.PK, "987654321", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");
			helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "DN", tariff2);
			helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "EN", tariff2);
			helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "FN", tariff2);
			Factory.Save();

			Declaration.JE_MessageType = Universal.Constants.TariffTypes.Import;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = EntryInstruction.PK;
			invoiceLine1.JI_Tariff = "123456789";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = EntryInstruction.PK;
			invoiceLine2.JI_Tariff = "987654321";

			AssertContainsExactElementsInAnyOrder(["AN", "BN", "DN", "EN"], EntryInstruction.GetTariffAttributesByKey(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws));
			AssertContainsExactElementsInAnyOrder(["CN", "FN"], EntryInstruction.GetTariffAttributesByKey(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return EntryInstruction;
		}

		public void TestCEI_NSI()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var cusEntryNumber = entryInstruction1.CreateNewEntryNumber(CusEntryNumberTypes.JP.NSI, "123");
			entryInstruction1.NSI = cusEntryNumber.CE_EntryNum;
			AssertEquals("NSI", "123", entryInstruction1.NSI);

			entryInstruction1.NSI = "111";
			AssertEquals("NSI", "111", cusEntryNumber.CE_EntryNum);

			AssertEquals(35, entryInstruction1.NSIInfo.MaxLength);
		}

		public void TestSupportsCloneCore()
		{
			AssertEquals(true, EntryInstruction.SupportsClone());
		}

		JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();

		JobDeclaration declaration;

		CusEntryInstruction EntryInstruction => entryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();

		CusEntryInstruction entryInstruction;
	}
}
