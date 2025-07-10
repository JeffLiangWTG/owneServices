using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Registry.Business
{
	[TestedType(typeof(CognosModeMapping))]
	class CognosModeMappingTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestSerializeAndDeserialize()
		{
			var item = new CognosModeMappingRegistryItem("TestCategory");
			var value = item.Value;
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			var readValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(value, readValue);
			value = (CognosModeMapping)GetBusinessObjectToSerialise();
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			readValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(value, readValue);
			value = new CognosModeMapping();
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			readValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(value, readValue);
		}

		#region Mode
		public void TestSelectedMode()
		{
			ModeMapping.SelectedMode = "AI";
			AssertEquals("AI", ModeMapping.SelectedMode);
			AssertNoErrors("Valid mode, should have no errors", ModeMapping.SelectedModeInfo);
			ModeMapping.SelectedMode = "";
			AssertEquals("", ModeMapping.SelectedMode);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(ModeMapping.SelectedModeInfo, true);
		}

		public void TestSelectedMode_RebuildMappedDepartmentsCollectionWhenChanged()
		{
			ModeMapping.SelectedMode = "AI";
			ModeMapping.MapDepartments(DeptCollection[0], DeptCollection[1], DeptCollection[2]);
			ModeMapping.SelectedMode = "AE";
			ModeMapping.MapDepartments(DeptCollection[3], DeptCollection[4]);
			ModeMapping.SelectedMode = "MI";
			ModeMapping.MapDepartments(DeptCollection[5], DeptCollection[6], DeptCollection[7], DeptCollection[8]);
			ModeMapping.SelectedMode = "ME";
			ModeMapping.MapDepartments(DeptCollection[9]);
			ModeMapping.SelectedMode = "AI";
			AssertEquals("Should be rebuilt", 3, ModeMapping.MappedDepartments.Count);
			ModeMapping.SelectedMode = "AE";
			AssertEquals("Should be rebuilt", 2, ModeMapping.MappedDepartments.Count);
			ModeMapping.SelectedMode = "MI";
			AssertEquals("Should be rebuilt", 4, ModeMapping.MappedDepartments.Count);
			ModeMapping.SelectedMode = "ME";
			AssertEquals("Should be rebuilt", 1, ModeMapping.MappedDepartments.Count);
			ModeMapping.SelectedMode = "NV";
			AssertEquals("Should be rebuilt", 0, ModeMapping.MappedDepartments.Count);
		}

		public void TestSelectedModeMaxLength()
		{
			AssertEquals(4, ModeMapping.SelectedModeInfo.MaxLength);
		}

		public void TestIsValidModeSelected()
		{
			AssertIsValidModeSelected("Should be valid", "AI", true);
			AssertIsValidModeSelected("Should be valid", "AE", true);
			AssertIsValidModeSelected("Should be valid", "MI", true);
			AssertIsValidModeSelected("Should be valid", "ME", true);
			AssertIsValidModeSelected("Should be valid", "CHB", true);
			AssertIsValidModeSelected("Should be valid", "NV", true);
			AssertIsValidModeSelected("Should be valid", "WPT", true);
			AssertIsValidModeSelected("Should be valid", "PR", true);
			AssertIsValidModeSelected("Should be valid", "OTH", true);
			AssertIsValidModeSelected("Empty. Should be invalid", "", false);
			AssertIsValidModeSelected("Not in the enum list. Should be invalid", "}{", false);
		}

		public void TestModes()
		{
			AssertEquals(9, ModeMapping.Modes.Count);
			AssertEquals("Should contain code value pair", "Air Import", ModeMapping.Modes.GetDescriptionFromCode("AI"));
			AssertEquals("Should contain code value pair", "Air Export", ModeMapping.Modes.GetDescriptionFromCode("AE"));
			AssertEquals("Should contain code value pair", "Maritime Import", ModeMapping.Modes.GetDescriptionFromCode("MI"));
			AssertEquals("Should contain code value pair", "Maritime Export", ModeMapping.Modes.GetDescriptionFromCode("ME"));
			AssertEquals("Should contain code value pair", "Customs House Brokerage", ModeMapping.Modes.GetDescriptionFromCode("CHB"));
			AssertEquals("Should contain code value pair", "Maritime Export NVOCC", ModeMapping.Modes.GetDescriptionFromCode("NV"));
			AssertEquals("Should contain code value pair", "Warehouse/Packing/Trucking", ModeMapping.Modes.GetDescriptionFromCode("WPT"));
			AssertEquals("Should contain code value pair", "Projects", ModeMapping.Modes.GetDescriptionFromCode("PR"));
			AssertEquals("Should contain code value pair", "Other", ModeMapping.Modes.GetDescriptionFromCode("OTH"));
		}

		public void TestConvertToModeEnum()
		{
			AssertEquals(CognosModes.AI, ModeMapping.ConvertToModeEnum("AI"));
			AssertEquals(CognosModes.AE, ModeMapping.ConvertToModeEnum("AE"));
			AssertEquals(CognosModes.MI, ModeMapping.ConvertToModeEnum("MI"));
			AssertEquals(CognosModes.ME, ModeMapping.ConvertToModeEnum("ME"));
			AssertEquals(CognosModes.CHB, ModeMapping.ConvertToModeEnum("CHB"));
			AssertEquals(CognosModes.NV, ModeMapping.ConvertToModeEnum("NV"));
			AssertEquals(CognosModes.PR, ModeMapping.ConvertToModeEnum("PR"));
			AssertEquals(CognosModes.OTH, ModeMapping.ConvertToModeEnum("OTH"));
			AssertEquals(CognosModes.WPT, ModeMapping.ConvertToModeEnum("WPT"));
			AssertEquals(CognosModes.Empty, ModeMapping.ConvertToModeEnum(""));
			AssertEquals(CognosModes.Empty, ModeMapping.ConvertToModeEnum("X}X"));
		}

		void AssertIsValidModeSelected(string errorMessage, string modeAsString, bool shouldBeValid)
		{
			ModeMapping.SelectedMode = modeAsString;
			AssertEquals(errorMessage, shouldBeValid, ModeMapping.IsValidModeSelected);
		}

		#endregion
		#region Departments
		public void TestMappedDepartments()
		{
			List<ZGuid> aIList = new List<ZGuid>(new ZGuid[] { DeptCollection[0].PK, DeptCollection[1].PK, DeptCollection[2].PK });
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.AI, aIList);
			List<ZGuid> mEList = new List<ZGuid>(new ZGuid[] { DeptCollection[3].PK, DeptCollection[4].PK, DeptCollection[5].PK });
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, mEList);
			ModeMapping.SelectedMode = "";
			AssertEquals("Invalid mode, collection should have no elements", 0, ModeMapping.MappedDepartments.Count);
			ModeMapping.SelectedMode = "AI";
			AssertEquals(3, ModeMapping.MappedDepartments.Count);
			AssertNotNull(ModeMapping.MappedDepartments.FindByPK(DeptCollection[0].PK));
			AssertNotNull(ModeMapping.MappedDepartments.FindByPK(DeptCollection[1].PK));
			AssertNotNull(ModeMapping.MappedDepartments.FindByPK(DeptCollection[2].PK));
			ModeMapping.SelectedMode = "ME";
			AssertEquals(3, ModeMapping.MappedDepartments.Count);
			AssertNotNull(ModeMapping.MappedDepartments.FindByPK(DeptCollection[3].PK));
			AssertNotNull(ModeMapping.MappedDepartments.FindByPK(DeptCollection[4].PK));
			AssertNotNull(ModeMapping.MappedDepartments.FindByPK(DeptCollection[5].PK));
		}

		public void TestAvailableDepartments()
		{
			AssertEquals("Pre-condition, all departments should be available", DeptCollection.Count, ModeMapping.AvailableDepartments.Count);
			List<ZGuid> aIList = new List<ZGuid>(new ZGuid[] { DeptCollection[0].PK, DeptCollection[1].PK, DeptCollection[2].PK });
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.AI, aIList);
			List<ZGuid> mEList = new List<ZGuid>(new ZGuid[] { DeptCollection[3].PK, DeptCollection[4].PK, DeptCollection[5].PK });
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, mEList);
			(ModeMapping.AvailableDepartments as IActiveBusinessObjectCollection).Refresh();
			AssertEquals("Should not include the mapped departments", DeptCollection.Count - 6, ModeMapping.AvailableDepartments.Count);
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[0].PK));
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[1].PK));
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[2].PK));
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[3].PK));
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[4].PK));
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[5].PK));
			ModeMapping.MappedDepartmentDictionary.Remove(CognosModes.ME);
			(ModeMapping.AvailableDepartments as IActiveBusinessObjectCollection).Refresh();
			AssertEquals("Should not include the mapped departments", DeptCollection.Count - 3, ModeMapping.AvailableDepartments.Count);
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[0].PK));
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[1].PK));
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[2].PK));
			ModeMapping.MappedDepartmentDictionary[CognosModes.AI].Remove(DeptCollection[0].PK);
			(ModeMapping.AvailableDepartments as IActiveBusinessObjectCollection).Refresh();
			AssertEquals("Should not include the mapped departments", DeptCollection.Count - 2, ModeMapping.AvailableDepartments.Count);
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[1].PK));
			AssertNull("Should not include the mapped departments", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[2].PK));
			ModeMapping.MappedDepartmentDictionary[CognosModes.AI].Clear();
			(ModeMapping.AvailableDepartments as IActiveBusinessObjectCollection).Refresh();
			AssertEquals("Should include all departments", DeptCollection.Count, ModeMapping.AvailableDepartments.Count);
		}

		public void TestMapDepartments_ModeNotSelected()
		{
			AssertEquals("Pre-condition", 0, ModeMapping.MappedDepartments.Count);
			AssertEquals("Pre-condition", 0, ModeMapping.MappedDepartmentDictionary.Count);
			AssertEquals("Pre-condition", ModeMapping.AllDepartments.Count, ModeMapping.AvailableDepartments.Count);
			ModeMapping.MapDepartments(DeptCollection[0], DeptCollection[1]);
			AssertEquals("Mode is not selected, should not add to the dictionary", 0, ModeMapping.MappedDepartments.Count);
			AssertEquals("Mode is not selected, should not add to the dictionary", 0, ModeMapping.MappedDepartmentDictionary.Count);
			AssertEquals("Mode is not selected, should not add to the dictionary", ModeMapping.AllDepartments.Count, ModeMapping.AvailableDepartments.Count);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Should not add a department which have already been mapped")]
		public void TestMapDepartments_AlreadyMappedDepartment()
		{
			AssertEquals("Pre-condition", 0, ModeMapping.MappedDepartments.Count);
			AssertEquals("Pre-condition", 0, ModeMapping.MappedDepartmentDictionary.Count);
			AssertEquals("Pre-condition", ModeMapping.AllDepartments.Count, ModeMapping.AvailableDepartments.Count);
			ModeMapping.SelectedMode = "AE";
			ModeMapping.MapDepartments(DeptCollection[0], DeptCollection[1]);
			AssertEquals("2 GlbDepartments should be added to the collection", 2, ModeMapping.MappedDepartments.Count);
			ModeMapping.MapDepartments(DeptCollection[0]);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Should not add a department which have already been mapped")]
		public void TestMapDepartments_AlreadyMappedToOtherModeDepartment()
		{
			AssertEquals("Pre-condition", 0, ModeMapping.MappedDepartments.Count);
			AssertEquals("Pre-condition", 0, ModeMapping.MappedDepartmentDictionary.Count);
			AssertEquals("Pre-condition", ModeMapping.AllDepartments.Count, ModeMapping.AvailableDepartments.Count);
			ModeMapping.SelectedMode = "AE";
			ModeMapping.MapDepartments(DeptCollection[0], DeptCollection[1]);
			ModeMapping.SelectedMode = "MI";
			ModeMapping.MapDepartments(DeptCollection[1]);
		}

		public void TestMapDepartments()
		{
			AssertEquals("Pre-condition", 0, ModeMapping.MappedDepartments.Count);
			AssertEquals("Pre-condition", 0, ModeMapping.MappedDepartmentDictionary.Count);
			AssertEquals("Pre-condition", ModeMapping.AllDepartments.Count, ModeMapping.AvailableDepartments.Count);
			ModeMapping.SelectedMode = "AE";
			ModeMapping.MapDepartments(DeptCollection[0], DeptCollection[1]);
			AssertEquals("2 GlbDepartments should be added to the collection", 2, ModeMapping.MappedDepartments.Count);
			AssertEquals("Should add one KeyValuePair", 1, ModeMapping.MappedDepartmentDictionary.Count);
			AssertEquals("2 GlbDepartments should be removed from the AvailableDepts collection", ModeMapping.AllDepartments.Count - 2, ModeMapping.AvailableDepartments.Count);
			AssertNotNull("should be added to the collection", ModeMapping.MappedDepartments.FindByPK(DeptCollection[0].PK));
			AssertNotNull("should be added to the collection", ModeMapping.MappedDepartments.FindByPK(DeptCollection[1].PK));
			AssertNull("should be removed from the collection", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[0].PK));
			AssertNull("should be removed from the collection", ModeMapping.AvailableDepartments.FindByPK(DeptCollection[1].PK));
		}

		public void TestUnmapDepartments_ModeNotSelected()
		{
			List<ZGuid> mEList = new List<ZGuid>(new ZGuid[] { DeptCollection[3].PK, DeptCollection[4].PK, DeptCollection[5].PK });
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, mEList);
			AssertEquals("Pre-condition", 1, ModeMapping.MappedDepartmentDictionary.Count);
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[3].PK));
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[4].PK));
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[5].PK));
			AssertEquals("Pre-condition", ModeMapping.AllDepartments.Count - 3, ModeMapping.AvailableDepartments.Count);
			ModeMapping.UnmapDepartments(DeptCollection[3], DeptCollection[4], DeptCollection[5]);
			AssertEquals("Mode is not selected, should not remove from the dictionary", 1, ModeMapping.MappedDepartmentDictionary.Count);
			Assert("Should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[3].PK));
			Assert("Should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[4].PK));
			Assert("should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[5].PK));
			AssertEquals("Mode is not selected, should not remove from the dictionary", ModeMapping.AllDepartments.Count - 3, ModeMapping.AvailableDepartments.Count);
		}

		public void TestUnmapDepartments_DifferentModeSelected()
		{
			List<ZGuid> mEList = new List<ZGuid>(new ZGuid[] { DeptCollection[3].PK, DeptCollection[4].PK, DeptCollection[5].PK });
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, mEList);
			AssertEquals("Pre-condition", 1, ModeMapping.MappedDepartmentDictionary.Count);
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[3].PK));
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[4].PK));
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[5].PK));
			AssertEquals("Pre-condition", ModeMapping.AllDepartments.Count - 3, ModeMapping.AvailableDepartments.Count);
			ModeMapping.SelectedMode = "AE";
			ModeMapping.UnmapDepartments(DeptCollection[3], DeptCollection[4], DeptCollection[5]);
			AssertEquals("A different mode is selected, should not remove from the dictionary", 1, ModeMapping.MappedDepartmentDictionary.Count);
			Assert("Should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[3].PK));
			Assert("Should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[4].PK));
			Assert("should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[5].PK));
			AssertEquals("A different mode is selected, should not remove from the dictionary", ModeMapping.AllDepartments.Count - 3, ModeMapping.AvailableDepartments.Count);
		}

		public void TestUnmapDepartments_TryingToUnmapAndUnmappedDepartment()
		{
			List<ZGuid> mEList = new List<ZGuid>(new ZGuid[] { DeptCollection[3].PK, DeptCollection[4].PK, DeptCollection[5].PK });
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, mEList);
			AssertEquals("Pre-condition", 1, ModeMapping.MappedDepartmentDictionary.Count);
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[3].PK));
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[4].PK));
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[5].PK));
			AssertEquals("Pre-condition", ModeMapping.AllDepartments.Count - 3, ModeMapping.AvailableDepartments.Count);
			ModeMapping.SelectedMode = "ME";
			ModeMapping.UnmapDepartments(DeptCollection[6]);
			AssertEquals("Trying to unmap an unmapped department, should not remove anything from the dictionary", 1, ModeMapping.MappedDepartmentDictionary.Count);
			Assert("should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[3].PK));
			Assert("should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[4].PK));
			Assert("should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[5].PK));
			AssertEquals("Trying to unmap a different department, should not remove anything from the dictionary", ModeMapping.AllDepartments.Count - 3, ModeMapping.AvailableDepartments.Count);
		}

		public void TestUnmapDepartments()
		{
			List<ZGuid> mEList = new List<ZGuid>(new ZGuid[] { DeptCollection[3].PK, DeptCollection[4].PK, DeptCollection[5].PK });
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, mEList);
			AssertEquals("Pre-condition", 1, ModeMapping.MappedDepartmentDictionary.Count);
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[3].PK));
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[4].PK));
			Assert("Pre-condition", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[5].PK));
			AssertEquals("Pre-condition", ModeMapping.AllDepartments.Count - 3, ModeMapping.AvailableDepartments.Count);
			ModeMapping.SelectedMode = "ME";
			ModeMapping.UnmapDepartments(DeptCollection[3], DeptCollection[4]);
			AssertEquals("2 GlbDepartments should be removed from the list", 1, ModeMapping.MappedDepartmentDictionary.Count);
			Assert("Should be removed from the list", !ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[3].PK));
			Assert("Should be removed from the list", !ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[4].PK));
			Assert("should not be removed from the list", ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Contains(DeptCollection[5].PK));
			AssertEquals("2 GlbDepartments should be placed back to the AvailableDepartments collection", ModeMapping.AllDepartments.Count - 1, ModeMapping.AvailableDepartments.Count);
			ModeMapping.UnmapDepartments(DeptCollection[5]);
			AssertEquals("1 GlbDepartments should be removed from the list, KeyValuePair should be removed from the dictionary if empty", 0, ModeMapping.MappedDepartmentDictionary.Count);
			AssertEquals("1 GlbDepartments should be placed back to the AvailableDepartments collection", ModeMapping.AllDepartments.Count, ModeMapping.AvailableDepartments.Count);
		}

		public void TestGetOrCreateNewDepartmentPKList()
		{
			Assert("Pre-condition", !ModeMapping.MappedDepartmentDictionary.ContainsKey(CognosModes.ME));
			List<ZGuid> mEList = ModeMapping.GetOrCreateNewDepartmentPKList(CognosModes.ME);
			AssertNotNull("Should create a new list", mEList);
			Assert("Should exist now", ModeMapping.MappedDepartmentDictionary.ContainsKey(CognosModes.ME));
			AssertEquals(mEList, ModeMapping.MappedDepartmentDictionary[CognosModes.ME]);
			AssertEquals("Should not create a new one", mEList, ModeMapping.GetOrCreateNewDepartmentPKList(CognosModes.ME));
		}

		public void TestAddUniquePKs()
		{
			List<ZGuid> pKList = new List<ZGuid>();
			ModeMapping.AddUniquePKs(pKList, new GlbDepartment[] { DeptCollection[0], DeptCollection[1], DeptCollection[2] });
			AssertEquals("Should add 3 unique PKs", 3, pKList.Count);
			ModeMapping.AddUniquePKs(pKList, new GlbDepartment[] { DeptCollection[2], DeptCollection[3], DeptCollection[4] });
			AssertEquals("Should add 2 unique PKs", 5, pKList.Count);
			ModeMapping.AddUniquePKs(pKList, new GlbDepartment[] { DeptCollection[5], DeptCollection[1], DeptCollection[0] });
			AssertEquals("Should add 1 unique PK", 6, pKList.Count);
			Assert(pKList.Contains(DeptCollection[0].PK));
			Assert(pKList.Contains(DeptCollection[1].PK));
			Assert(pKList.Contains(DeptCollection[2].PK));
			Assert(pKList.Contains(DeptCollection[3].PK));
			Assert(pKList.Contains(DeptCollection[4].PK));
			Assert(pKList.Contains(DeptCollection[5].PK));
		}

		public void TestRemovePKs()
		{
			List<ZGuid> pKList = new List<ZGuid>();
			pKList.AddRange(new ZGuid[] { DeptCollection[0].PK, DeptCollection[1].PK, DeptCollection[2].PK });
			ModeMapping.RemovePKs(pKList, new GlbDepartment[] { DeptCollection[0], DeptCollection[1], DeptCollection[3] });
			AssertEquals("Should remove 2 PKs", 1, pKList.Count);
			Assert(!pKList.Contains(DeptCollection[0].PK));
			Assert(!pKList.Contains(DeptCollection[1].PK));
			Assert(pKList.Contains(DeptCollection[2].PK));
			ModeMapping.RemovePKs(pKList, new GlbDepartment[] { DeptCollection[2], DeptCollection[1], DeptCollection[4] });
			AssertEquals("Should remove 1 PKs", 0, pKList.Count);
		}

		#endregion
		#region Equals
		public void TestEquals()
		{
			ZGuid pK1 = ZGuid.NewZGuid();
			ZGuid pK2 = ZGuid.NewZGuid();
			List<ZGuid> mEList = new List<ZGuid>();
			mEList.Add(pK1);
			mEList.Add(pK2);
			List<ZGuid> newMEList = new List<ZGuid>();
			newMEList.Add(pK2);
			newMEList.Add(pK1);
			ZGuid pK3 = ZGuid.NewZGuid();
			ZGuid pK4 = ZGuid.NewZGuid();
			ZGuid pK5 = ZGuid.NewZGuid();
			List<ZGuid> aIList = new List<ZGuid>();
			aIList.Add(pK3);
			aIList.Add(pK4);
			aIList.Add(pK5);
			List<ZGuid> newAIList = new List<ZGuid>();
			newAIList.Add(pK5);
			newAIList.Add(pK3);
			newAIList.Add(pK4);
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, mEList);
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.AI, aIList);
			CognosModeMapping newMapping = new CognosModeMapping();
			newMapping.MappedDepartmentDictionary.Add(CognosModes.ME, newMEList);
			newMapping.MappedDepartmentDictionary.Add(CognosModes.AI, newAIList);
			Assert("Should be equal", ModeMapping.Equals(newMapping));
			ModeMapping.MappedDepartmentDictionary.Remove(CognosModes.ME);
			Assert("Should not be equal", !ModeMapping.Equals(newMapping));
		}

		public void TestEquals_DictionaryHasDifferrentCount()
		{
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, new List<ZGuid>());
			CognosModeMapping newMapping = new CognosModeMapping();
			Assert("Dictionary has different element count", !ModeMapping.Equals(newMapping));
		}

		public void TestEquals_DictionaryHasDifferentKeys()
		{
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, new List<ZGuid>());
			CognosModeMapping newMapping = new CognosModeMapping();
			newMapping.MappedDepartmentDictionary.Add(CognosModes.AI, new List<ZGuid>());
			Assert("Dictionary has different keys", !ModeMapping.Equals(newMapping));
		}

		public void TestEquals_PKListHasDifferentCount()
		{
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, new List<ZGuid>());
			CognosModeMapping newMapping = new CognosModeMapping();
			newMapping.MappedDepartmentDictionary.Add(CognosModes.ME, new List<ZGuid>());
			newMapping.MappedDepartmentDictionary[CognosModes.ME].Add(ZGuid.NewZGuid());
			Assert("PKList has different count", !ModeMapping.Equals(newMapping));
		}

		public void TestEquals_PKListHasDifferentElements()
		{
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.ME, new List<ZGuid>());
			ModeMapping.MappedDepartmentDictionary[CognosModes.ME].Add(ZGuid.NewZGuid());
			CognosModeMapping newMapping = new CognosModeMapping();
			newMapping.MappedDepartmentDictionary.Add(CognosModes.ME, new List<ZGuid>());
			newMapping.MappedDepartmentDictionary[CognosModes.ME].Add(ZGuid.NewZGuid());
			Assert("PKList has different elements", !ModeMapping.Equals(newMapping));
		}

		#endregion
		public void TestValidation()
		{
			AssertEquals(typeof(CognosModeMappingValidation), ModeMapping.Validation.GetType());
		}

		public new void TestClone()
		{
			ZGuid pRListGuid = ZGuid.NewZGuid();
			List<ZGuid> pRList = new List<ZGuid>();
			pRList.Add(pRListGuid);
			ZGuid wPTListGuid = ZGuid.NewZGuid();
			List<ZGuid> wPTList = new List<ZGuid>();
			wPTList.Add(wPTListGuid);
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.PR, pRList);
			ModeMapping.MappedDepartmentDictionary.Add(CognosModes.WPT, wPTList);
			CognosModeMapping clonedModeMapping = (CognosModeMapping)ModeMapping.Clone(null, null);
			AssertEquals("Should be ModeMapping = ClonedModeMapping", ModeMapping, clonedModeMapping);
			Assert("Has to be cloned, not assigned the same reference", ModeMapping.MappedDepartmentDictionary != clonedModeMapping.MappedDepartmentDictionary);
			Assert("Has to be cloned, not assigned the same reference", pRList != clonedModeMapping.MappedDepartmentDictionary[CognosModes.PR]);
			Assert("Has to be cloned, not assigned the same reference", wPTList != clonedModeMapping.MappedDepartmentDictionary[CognosModes.WPT]);
			AssertEquals(pRListGuid, ModeMapping.MappedDepartmentDictionary[CognosModes.PR][0]);
			AssertEquals(wPTListGuid, ModeMapping.MappedDepartmentDictionary[CognosModes.WPT][0]);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGetCognosMode_NullParam()
		{
			ModeMapping.GetCognosMode(null);
		}

		public void TestGetCognosMode_FromDepartment()
		{
			ModeMapping.SelectedMode = "AI";
			ModeMapping.MapDepartments(DeptCollection[0]);
			ModeMapping.SelectedMode = "AE";
			ModeMapping.MapDepartments(DeptCollection[1]);
			ModeMapping.SelectedMode = "MI";
			ModeMapping.MapDepartments(DeptCollection[5]);
			ModeMapping.SelectedMode = "ME";
			ModeMapping.MapDepartments(DeptCollection[4]);
			ModeMapping.SelectedMode = "CHB";
			ModeMapping.MapDepartments(DeptCollection[3]);
			ModeMapping.SelectedMode = "WPT";
			ModeMapping.MapDepartments(DeptCollection[2]);
			ModeMapping.SelectedMode = "NV";
			ModeMapping.MapDepartments(DeptCollection[6]);
			AssertEquals("AI", ModeMapping.GetCognosMode(DeptCollection[0]));
			AssertEquals("AE", ModeMapping.GetCognosMode(DeptCollection[1]));
			AssertEquals("WPT", ModeMapping.GetCognosMode(DeptCollection[2]));
			AssertEquals("CHB", ModeMapping.GetCognosMode(DeptCollection[3]));
			AssertEquals("ME", ModeMapping.GetCognosMode(DeptCollection[4]));
			AssertEquals("MI", ModeMapping.GetCognosMode(DeptCollection[5]));
			AssertEquals("NV", ModeMapping.GetCognosMode(DeptCollection[6]));
			AssertEquals("", ModeMapping.GetCognosMode(DeptCollection[7]));
		}

		public void TestGetCognosMode_FromDepartmentPK()
		{
			ModeMapping.SelectedMode = "AI";
			ModeMapping.MapDepartments(DeptCollection[0]);
			ModeMapping.SelectedMode = "AE";
			ModeMapping.MapDepartments(DeptCollection[1]);
			ModeMapping.SelectedMode = "MI";
			ModeMapping.MapDepartments(DeptCollection[5]);
			ModeMapping.SelectedMode = "ME";
			ModeMapping.MapDepartments(DeptCollection[4]);
			ModeMapping.SelectedMode = "CHB";
			ModeMapping.MapDepartments(DeptCollection[3]);
			ModeMapping.SelectedMode = "WPT";
			ModeMapping.MapDepartments(DeptCollection[2]);
			ModeMapping.SelectedMode = "NV";
			ModeMapping.MapDepartments(DeptCollection[6]);
			AssertEquals("AI", ModeMapping.GetCognosMode(DeptCollection[0].PK));
			AssertEquals("AE", ModeMapping.GetCognosMode(DeptCollection[1].PK));
			AssertEquals("WPT", ModeMapping.GetCognosMode(DeptCollection[2].PK));
			AssertEquals("CHB", ModeMapping.GetCognosMode(DeptCollection[3].PK));
			AssertEquals("ME", ModeMapping.GetCognosMode(DeptCollection[4].PK));
			AssertEquals("MI", ModeMapping.GetCognosMode(DeptCollection[5].PK));
			AssertEquals("NV", ModeMapping.GetCognosMode(DeptCollection[6].PK));
			AssertEquals("", ModeMapping.GetCognosMode(DeptCollection[7].PK));
		}

		public void TestGetDepartmentPKs()
		{
			ModeMapping.SelectedMode = "AI";
			ModeMapping.MapDepartments(DeptCollection[0], DeptCollection[1]);
			ModeMapping.SelectedMode = "AE";
			ModeMapping.MapDepartments(DeptCollection[2], DeptCollection[3]);
			ModeMapping.SelectedMode = "MI";
			ModeMapping.MapDepartments(DeptCollection[5], DeptCollection[4]);
			ModeMapping.SelectedMode = "ME";
			ModeMapping.MapDepartments(DeptCollection[6]);
			AssertGetDepartmentPKs("AI", DeptCollection[0].PK, DeptCollection[1].PK);
			AssertGetDepartmentPKs("AE", DeptCollection[2].PK, DeptCollection[3].PK);
			AssertGetDepartmentPKs("MI", DeptCollection[4].PK, DeptCollection[5].PK);
			AssertGetDepartmentPKs("ME", DeptCollection[6].PK);
			AssertGetDepartmentPKs("CHB");
		}

		void AssertGetDepartmentPKs(ZString mode, params ZGuid[] expectedPKs)
		{
			ZGuid[] deptPKs = ModeMapping.GetDepartmentPKs(mode);
			AssertEquals(expectedPKs.Length, deptPKs.Length);
			foreach (ZGuid expectedPK in expectedPKs)
			{
				Assert(((IList<ZGuid>)deptPKs).Contains(expectedPK));
			}
		}

		#region Implementation
		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			CognosModeMapping result = new CognosModeMapping();
			result.SelectedMode = "AI";
			result.MapDepartments(DeptCollection[0], DeptCollection[1]);
			result.SelectedMode = "AE";
			result.MapDepartments(DeptCollection[2]);
			result.SelectedMode = "ME";
			result.MapDepartments(DeptCollection[3], DeptCollection[4]);
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CognosModeMapping();
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeptCollection = LoadDepartmentCollection();
		}

		internal GlbDepartmentCollection LoadDepartmentCollection()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbDepartmentCollection collection = new GlbDepartmentCollection(factory);
			ZQuery query = new ZQuery();
			query.MaximumRows = 10;
			collection.AdditionalFilter = query;
			AssertEquals("Pre-condition. There should be at least 10 Departments in test database, add manually if this fails", 10, collection.Count);
			collection = new GlbDepartmentCollection(factory);
			return collection;
		}

		internal CognosModeMapping ModeMapping
		{
			get
			{
				if (fModeMapping == null)
				{
					fModeMapping = new CognosModeMapping();
				}

				return fModeMapping;
			}
		}

		internal GlbDepartmentCollection DeptCollection;
		internal CognosModeMapping fModeMapping;
		#endregion
	}
}
