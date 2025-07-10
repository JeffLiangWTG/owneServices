using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPackage))]
	sealed class NctsPackageTests : CusInvPackTest<NctsCommonCargoDesc>
	{
		public void TestReadOnlyMemberAttributes()
		{
			var allProperties = typeof(NctsPackage)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
				.ToDictionary(property => property.Name, property => property);
			var allPublicProperties = typeof(NctsPackage)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
			var propertiesWithReadOnlyMember = allPublicProperties
				.Select(property => (Property: property, ReadOnlyMemberName: property.GetCustomAttribute<ReadOnlyMemberAttribute>(inherit: true)?.Member))
				.Where(property => property.ReadOnlyMemberName is not null)
				.ToArray();

			CombineAssertions(() =>
			{
				foreach (var (property, readOnlyMemberName) in propertiesWithReadOnlyMember)
				{
					var propertyExists = allProperties.TryGetValue(readOnlyMemberName, out var propertyInfo);
					Assert($"Property {readOnlyMemberName} used in {property.Name} ReadOnlyMember attribute does not exist in NctsPackage type", propertyExists);
					if (propertyExists)
					{
						var accessors = propertyInfo.GetAccessors(nonPublic: true);
						var isAccessorPublicOrProtected = accessors.Any(accessor => accessor.IsPublic || accessor.IsFamily);
						Assert($"Property {readOnlyMemberName} used in {property.Name} ReadOnlyMember attribute is not public or protected", isAccessorPublicOrProtected);
					}
				}
			});
		}

		public void TestFetchStrategy()
		{
			var package = Factory.New<NctsPackage>();
			AssertType<NctsPackageFetchStrategy>(package.FetchStrategy);
		}

		public void TestLinkingContainerWillTriggerHasChanges()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var headerContainer2 = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_ContainerNum = "CONTAINER2";
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();
			Factory.Save();
			var containerPivot = package.ContainersPivotsForBindingOnly[0];
			AssertEquals("containerPivot.ContainerSelected", false, containerPivot.ContainerSelected);
			AssertEquals("package.HasChanges", false, package.HasChanges);
			containerPivot.ContainerSelected = true;
			AssertEquals("package.HasChanges", true, package.HasChanges);
		}

		public void TestValidation_Phase4()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsPackagePhase4Validation>(packagePivot.Validation);
		}

		public void TestValidation_Phase5()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsPackagePhase5Validation>(packagePivot.Validation);
		}

		public void TestIsPhase5()
		{
			CombineAssertions(() =>
			{
				packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("False when BH_ApplicationCode is set to NCT", false, packagePivot.IsPhase5);
				packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("True when BH_ApplicationCode is set to NC5", true, packagePivot.IsPhase5);
			});
		}

		public void TestB5_UnitType_MaxLength_Phase4()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals(3, packagePivot.B5_UnitTypeInfo.MaxLength);
		}

		public void TestB5_UnitType_MaxLength_Phase5()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals(2, packagePivot.B5_UnitTypeInfo.MaxLength);
		}

		public void TestB5_UnitType_Phase4Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(packagePivot.B5_UnitTypeInfo, NctsHeader.Phase4CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Package Type", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Type", captionResourceString.ShortCaption);
			});
		}

		public void TestB5_UnitType_Phase5Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(packagePivot.B5_UnitTypeInfo, NctsHeader.Phase5CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Package Type", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Pack Type", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Type", captionResourceString.ShortCaption);
			});
		}

		public void TestB5_UnitCount_Phase4Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(packagePivot.B5_UnitCountInfo, NctsHeader.Phase4CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Number of Packages", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Qty.", captionResourceString.ShortCaption);
			});
		}

		public void TestB5_UnitCount_Phase5Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(packagePivot.B5_UnitCountInfo, NctsHeader.Phase5CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Number of Packages", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Pack Qty.", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Qty.", captionResourceString.ShortCaption);
			});
		}

		public void TestB5_BrandCaption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(packagePivot.B5_BrandInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Brand", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Brand", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Brand", captionResourceString.ShortCaption);
				AssertEquals("Description", "Vehicle Brand", captionResourceString.FullDescription);
			});
		}

		public void TestB5_ModelCaption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(packagePivot.B5_ModelInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Model", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Model", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Model", captionResourceString.ShortCaption);
				AssertEquals("Description", "Vehicle Model", captionResourceString.FullDescription);
			});
		}

		public void TestB5_MarksAndNumbers_MaxLength_Phase4()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals(42, packagePivot.B5_MarksAndNumbersInfo.MaxLength);
		}

		public void TestB5_MarksAndNumbers_MaxLength_Phase5()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals(512, packagePivot.B5_MarksAndNumbersInfo.MaxLength);
		}

		public void TestB5_MarksAndNumbers_Phase4Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(packagePivot.B5_MarksAndNumbersInfo, NctsHeader.Phase4CaptionKey);
			AssertEquals("Marks & Numbers", captionResourceString.Caption);
		}

		public void TestB5_MarksAndNumbers_Phase5Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(packagePivot.B5_MarksAndNumbersInfo, NctsHeader.Phase5CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Marks and Numbers", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Marks and No.", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Marks", captionResourceString.ShortCaption);
			});
		}

		public void TestMultipleKeysToUse()
		{
			CombineAssertions(() =>
			{
				var package = GetNewBusinessObjectForP4Arrival(Factory);
				AssertSequencesEqual("Header.BH_ApplicationCode is NCT", new[] { NctsHeader.Phase4CaptionKey }, package.MultipleKeysToUse);

				package = GetNewBusinessObjectForP5Departure(Factory);
				AssertSequencesEqual("Header.BH_ApplicationCode is NC5 Departure", new[] { NctsHeader.Phase5DepartureCaptionKey, NctsHeader.Phase5CaptionKey }, package.MultipleKeysToUse);

				package = GetNewBusinessObjectForP5Arrival(Factory);
				AssertSequencesEqual("Header.BH_ApplicationCode is NC5 Arrival", new[] { NctsHeader.Phase5CaptionKey }, package.MultipleKeysToUse);
			});
		}

		public void TestIsBulk()
		{
			var bulkType = Factory.SetupBulkCusCode();
			CombineAssertions(() =>
			{
				packagePivot.B5_UnitType = "BX";
				AssertEquals("Not Bulk", false, packagePivot.IsBulk);
				packagePivot.B5_UnitType = bulkType;
				AssertEquals("Is Bulk", true, packagePivot.IsBulk);
			});
		}

		public void TestIsUnpacked()
		{
			var unpackedType = Factory.SetupUnpackCusCode();
			CombineAssertions(() =>
			{
				packagePivot.B5_UnitType = "BX";
				AssertEquals("Not Unpacked", false, packagePivot.IsUnpacked);
				packagePivot.B5_UnitType = unpackedType;
				AssertEquals("Is Unpacked", true, packagePivot.IsUnpacked);
			});
		}

		public void TestB5_TypeOfDifference_Phase5Enabled_empty()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = ZString.Empty;
			AssertEquals(false, packagePivot.B5_TypeOfDifferenceInfo.ReadOnly);
		}

		public void TestB5_TypeOfDifference_Phase5Disabled_NEW()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			AssertEquals(true, packagePivot.B5_TypeOfDifferenceInfo.ReadOnly);
		}

		public void TestB5_TypeOfDifference_Phase5EnabledFields_empty()
		{
			packagePivot.B5_TypeOfDifference = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("Number of packages enabled", true, !packagePivot.B5_UnitCountInfo.ReadOnly);
				AssertEquals("Type enabled", true, !packagePivot.B5_UnitCountInfo.ReadOnly);
				AssertEquals("Marks & Numbers enabled", true, !packagePivot.B5_UnitCountInfo.ReadOnly);
			});
		}

		public void TestB5_TypeOfDifference_Phase5EnabledFields_NEW()
		{
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			CombineAssertions(() =>
			{
				AssertEquals("Number of packages enabled", true, !packagePivot.B5_UnitCountInfo.ReadOnly);
				AssertEquals("Type enabled", true, !packagePivot.B5_UnitCountInfo.ReadOnly);
				AssertEquals("Marks & Numbers enabled", true, !packagePivot.B5_UnitCountInfo.ReadOnly);
			});
		}

		public void TestB5_TypeOfDifference_Phase5DisabledFields_DIF()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			CombineAssertions(() =>
			{
				AssertEquals("Number of packages disabled", true, packagePivot.B5_UnitCountInfo.ReadOnly);
				AssertEquals("Type disabled", true, packagePivot.B5_UnitTypeInfo.ReadOnly);
				AssertEquals("Marks & Numbers disabled", true, packagePivot.B5_MarksAndNumbersInfo.ReadOnly);

				var packDifference = packagePivot.PackDifference;
				packDifference.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Number of packages disabled for child", false, packDifference.B5_UnitCountInfo.ReadOnly);
				AssertEquals("Type disabled for child", false, packDifference.B5_UnitTypeInfo.ReadOnly);
				AssertEquals("Marks & Numbers disabled for child", false, packDifference.B5_MarksAndNumbersInfo.ReadOnly);
			});
		}

		public void TestB5_TypeOfDifference_Phase5DisabledFields_DEC()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			CombineAssertions(() =>
			{
				AssertEquals("Number of packages disabled", true, packagePivot.B5_UnitCountInfo.ReadOnly);
				AssertEquals("Type disabled", true, packagePivot.B5_UnitTypeInfo.ReadOnly);
				AssertEquals("Marks & Numbers disabled", true, packagePivot.B5_MarksAndNumbersInfo.ReadOnly);
			});
		}

		public void TestB5_TypeOfDifference_Phase5DisabledFields_MIS()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			CombineAssertions(() =>
			{
				AssertEquals("Number of packages disabled", true, packagePivot.B5_UnitCountInfo.ReadOnly);
				AssertEquals("Type disabled", true, packagePivot.B5_UnitTypeInfo.ReadOnly);
				AssertEquals("Marks & Numbers disabled", true, packagePivot.B5_MarksAndNumbersInfo.ReadOnly);
			});
		}

		public void TestB5_TypeOfDifference_Phase5Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(packagePivot.B5_TypeOfDifferenceInfo, NctsHeader.Phase5CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "State of unloading", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Unloaded State", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Unloaded State", captionResourceString.ShortCaption);
			});
		}

		public void TestIsPackDifference()
		{
			CombineAssertions(() =>
			{
				var nctsPackage = Factory.New<NctsPackage>();
				AssertEquals("Default", false, nctsPackage.IsPackDifference);

				var nctsPackDifference = Factory.New<NctsPackage>();
				nctsPackDifference.B5_B5_ParentPackage = nctsPackage.PK;
				AssertEquals("IsPackDifference", true, nctsPackDifference.IsPackDifference);
			});
		}

		public void TestB5_SequenceNumber_Phase5ReadOnly()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals(true, packagePivot.B5_SequenceNumberInfo.ReadOnly);
		}

		public void TestB5_SequenceNumber_Phase5Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(packagePivot.B5_SequenceNumberInfo, NctsHeader.Phase5CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Sequence Number", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Sequence Number", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Sequence No", captionResourceString.ShortCaption);
			});
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(packagePivot.Parent.Header.ArrivalMovementHeader, x => ((NctsPackage)x).AreUnloadingRemarksFullyAccepted, packagePivot);
		}

		public void TestB5_TypeOfDifference_ReadOnly_UnloadingRemarksAcceptedByCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(packagePivot.Parent.Header.ArrivalMovementHeader, x => ((NctsPackage)x).B5_TypeOfDifferenceInfo.ReadOnly, packagePivot);
		}

		public void TestB5_UnitCount_ReadOnly_UnloadingRemarksAcceptedByCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(packagePivot.Parent.Header.ArrivalMovementHeader, x => ((NctsPackage)x).B5_UnitCountInfo.ReadOnly, packagePivot);
		}

		public void TestB5_UnitCount_ReadOnly_BulkCode()
		{
			SetUpDeparturePhase5Package();
			var bulkType = Factory.SetupBulkCusCode();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				AssertEquals("Editable - empty type", false, packagePivot.B5_UnitCountInfo.ReadOnly);
				packagePivot.B5_UnitType = bulkType;
				AssertEquals("ReadOnly - bulk code", true, packagePivot.B5_UnitCountInfo.ReadOnly);
				packagePivot.B5_UnitType = "AA";
				AssertEquals("Editable - not bulk code", false, packagePivot.B5_UnitCountInfo.ReadOnly);
			});
		}

		public void TestB5_UnitType_ReadOnly_UnloadingRemarksAcceptedByCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(packagePivot.Parent.Header.ArrivalMovementHeader, x => ((NctsPackage)x).B5_UnitTypeInfo.ReadOnly, packagePivot);
		}

		public void TestB5_MarksAndNumbers_ReadOnly_UnloadingRemarksAcceptedByCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(packagePivot.Parent.Header.ArrivalMovementHeader, x => ((NctsPackage)x).B5_MarksAndNumbersInfo.ReadOnly, packagePivot);
		}

		public void TestFieldsEnabledForUnloadedStateNEW()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				AssertEquals("SequenceNumber should be disabled for unloaded state NEW", true, packagePivot.B5_SequenceNumberInfo.ReadOnly);
				AssertEquals("TypeOfDifference should be disabled for unloaded state NEW", true, packagePivot.B5_TypeOfDifferenceInfo.ReadOnly);
				AssertEquals("UnitCount should be enabled for unloaded state NEW", false, packagePivot.B5_UnitCountInfo.ReadOnly);
				AssertEquals("UnitType should be enabled for unloaded state NEW", false, packagePivot.B5_UnitTypeInfo.ReadOnly);
				AssertEquals("MarksAndNumbers should be enabled for unloaded state NEW", false, packagePivot.B5_MarksAndNumbersInfo.ReadOnly);
			});
		}

		public void TestFieldsEnabledForUnloadedStateDIF()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;

			CombineAssertions(() =>
			{
				AssertEquals("SequenceNumber should be disabled for unloaded state DIF", true, packagePivot.B5_SequenceNumberInfo.ReadOnly);
				AssertEquals("TypeOfDifference should be enabled for unloaded state DIF", false, packagePivot.B5_TypeOfDifferenceInfo.ReadOnly);

				AssertEquals("UnitCount should be disabled for unloaded state DIF", true, packagePivot.B5_UnitCountInfo.ReadOnly);
				AssertEquals("UnitType should be disabled for unloaded state DIF", true, packagePivot.B5_UnitTypeInfo.ReadOnly);
				AssertEquals("MarksAndNumbers should be disabled for unloaded state DIF", true, packagePivot.B5_MarksAndNumbersInfo.ReadOnly);

				AssertEquals("PackDifference.UnitCount should be enabled for unloaded state DIF", false, packagePivot.PackDifference.B5_UnitCountInfo.ReadOnly);
				AssertEquals("PackDifference.UnitType should be enabled for unloaded state DIF", false, packagePivot.PackDifference.B5_UnitTypeInfo.ReadOnly);
				AssertEquals("PackDifference.MarksAndNumbers should be enabled for unloaded state DIF", false, packagePivot.PackDifference.B5_MarksAndNumbersInfo.ReadOnly);
			});
		}

		public void TestPackDifference()
		{
			CombineAssertions(() =>
			{
				var nctsPackage = Factory.New<NctsPackage>();
				AssertNull("Default", nctsPackage.PackDifference);
				var nctsPackDifference = Factory.New<NctsPackage>();
				nctsPackDifference.B5_B5_ParentPackage = nctsPackage.PK;
				AssertType<NctsPackage>("Exists", nctsPackage.PackDifference);
			});
		}

		public void TestRegisterEditableChildForPackDifference()
		{
			var nctsPackage = Factory.New<NctsPackage>();
			var nctsPackDifference = Factory.New<NctsPackage>();
			nctsPackDifference.B5_B5_ParentPackage = nctsPackage.PK;
			AssertEquals("IsRegisteredEditableChildObject", true, nctsPackage.IsRegisteredEditableChildObject(nctsPackage.PackDifference));
		}

		public void TestSetTypeOfDifferenceDIFSetsPackDifference()
		{
			var nctsPackage = GetNewBusinessObjectForP5Arrival(Factory);
			nctsPackage.B5_TypeOfDifference = "DIF";
			AssertEquals("Child package should be set", nctsPackage.PK, nctsPackage.PackDifference.B5_B5_ParentPackage);
		}

		public void TestUnloadingRemarksAreSentToCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, packagePivot.IsUnloadingRemarksReadOnly);
				packagePivot.Parent.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, packagePivot.IsUnloadingRemarksReadOnly);
			});
		}

		public void TestB5_TypeOfDifference_ReadOnly_SentToCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, packagePivot.B5_TypeOfDifferenceInfo.ReadOnly);
				packagePivot.Parent.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, packagePivot.B5_TypeOfDifferenceInfo.ReadOnly);
			});
		}

		public void TestB5_UnitCount_ReadOnly_SentToCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, packagePivot.B5_UnitCountInfo.ReadOnly);
				packagePivot.Parent.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, packagePivot.B5_UnitCountInfo.ReadOnly);
			});
		}

		public void TestB5_UnitType_ReadOnly_SentToCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, packagePivot.B5_UnitTypeInfo.ReadOnly);
				packagePivot.Parent.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, packagePivot.B5_UnitTypeInfo.ReadOnly);
			});
		}

		public void TestB5_MarksAndNumbers_ReadOnly_SentToCustoms()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, packagePivot.B5_MarksAndNumbersInfo.ReadOnly);
				packagePivot.Parent.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, packagePivot.B5_MarksAndNumbersInfo.ReadOnly);
			});
		}

		public void TestDeleteExistingPackDifference()
		{
			CombineAssertions(() =>
			{
				var nctsPackage = GetNewBusinessObjectForP5Arrival(Factory);
				nctsPackage.B5_TypeOfDifference = "DIF";
				var differencePackage = nctsPackage.PackDifference;
				AssertNotNull("PackDifference not null", differencePackage);
				nctsPackage.B5_TypeOfDifference = "DEC";
				AssertEquals("UnloadedGoodsItem is deleted", true, differencePackage.IsDeleted);
			});
		}

		public void TestTypeOfDifferenceNewEnabled()
		{
			CombineAssertions(() =>
			{
				var nctsPackage = GetNewBusinessObjectForP5Arrival(Factory);
				AssertEquals("The default status is NEW and then the type of difference should be disabled", true, nctsPackage.B5_TypeOfDifferenceInfo.ReadOnly);

				var package = Factory.New<NctsPackage>();
				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("When the default status is not NEW and you have set the value to NEW manually, the field should be enabled", false, package.B5_TypeOfDifferenceInfo.ReadOnly);
			});
		}

		public void TestCanDelete()
		{
			packagePivot.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			CombineAssertions(() =>
			{
				packagePivot.B5_TypeOfDifference = ZString.Empty;
				AssertEquals("B5_TypeOfDifference empty", false, packagePivot.CanDelete);

				packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("B5_TypeOfDifference = 'DEC'", false, packagePivot.CanDelete);

				packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("B5_TypeOfDifference = 'NEW'", true, packagePivot.CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			AssertEquals("Cannot delete Package from Customs.", packagePivot.ReasonForNotAbleToDelete);
		}

		public void TestAutomaticSequenceNumberEnabled()
		{
			var goodsItem = GetNewParentArrival();

			CombineAssertions(() =>
			{
				var package = goodsItem.Packages.AddNew();
				AssertEquals("First Line", 1, (int)package.B5_SequenceNumber);
				var secondLine = goodsItem.Packages.AddNew();
				AssertEquals("Second Line", 2, (int)secondLine.B5_SequenceNumber);
				var thirdLine = goodsItem.Packages.AddNew();
				AssertEquals("Third Line", 3, (int)thirdLine.B5_SequenceNumber);
				secondLine.Delete();
				AssertEquals("First Line same as second deleted", 1, (int)package.B5_SequenceNumber);
				AssertEquals("Third Line renumbered as second deleted", 2, (int)thirdLine.B5_SequenceNumber);
				var newThirdLine = goodsItem.Packages.AddNew();
				AssertEquals("New Third added", 3, (int)newThirdLine.B5_SequenceNumber);
			});
		}

		public void TestAutomaticSequenceNumberDisabled()
		{
			var goodsItem = GetNewParentArrival();

			CombineAssertions(() =>
			{
				var firstLine = Factory.New<NctsPackageForTest>();
				goodsItem.Packages.RemoveAndDeleteAll();
				firstLine.AttachToParent(goodsItem);
				AssertEquals("First Line", ZShort.Zero, firstLine.B5_SequenceNumber);
				firstLine.B5_SequenceNumber = 20;
				var secondLine = Factory.New<NctsPackageForTest>();
				secondLine.AttachToParent(goodsItem);
				AssertEquals("Second Line", ZShort.Zero, secondLine.B5_SequenceNumber);
				secondLine.B5_SequenceNumber = 35;
				firstLine.Delete();
				AssertEquals("Second Line remains the same as the first line is deleted", 35, (int)secondLine.B5_SequenceNumber);
			});
		}

		public void TestContainersPivotsForBindingOnlyIsNotCached()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			var collection = package.ContainersPivotsForBindingOnly;
			AssertEquals("collection.Count", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { container1, container2 }, collection.Select(x => x.Container));
			Assert("ContainersPivotsForBindingOnly should not be cached", !object.ReferenceEquals(collection, package.ContainersPivotsForBindingOnly));
		}

		public void TestIsPhase5Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();

			CombineAssertions(() =>
			{
				packagePivot = (NctsPackage)GetNewPackagePivot();
				AssertEquals("Package Parent not assigned", false, packagePivot.IsPhase5Departure);

				packagePivot.B5_ParentTableCode = goodsItem.TablePrefix;
				packagePivot.B5_ParentID = goodsItem.PK;
				AssertEquals("Arrival Package type", false, packagePivot.IsPhase5Departure);

				packagePivot = (NctsPackage)GetNewPackagePivot();
				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var goodsItemDeparture = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				packagePivot.B5_ParentTableCode = goodsItemDeparture.TablePrefix;
				packagePivot.B5_ParentID = goodsItemDeparture.PK;
				AssertEquals("Departure", true, packagePivot.IsPhase5Departure);
			});
		}

		public void TestContainerPivotsChildEditable()
		{
			var package = (NctsPackage)GetNewBusinessObject();
			AssertEquals("ContainersPivots should not be marked as ChildEditable as they they are non-persistent and this can cause performance issues when sending NCTS messages to customs",
				false, package.IsRegisteredEditableChildObject(package.ContainersPivotsForBindingOnly));
		}

		public void TestB5_PackageID_ReadOnly()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, packagePivot.B5_PackageIDInfo.ReadOnly);
				packagePivot.Parent.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, packagePivot.B5_PackageIDInfo.ReadOnly);
			});
		}

		public void TestB5_Brand_ReadOnly()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, packagePivot.B5_BrandInfo.ReadOnly);
				packagePivot.Parent.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, packagePivot.B5_BrandInfo.ReadOnly);
			});
		}

		public void TestB5_Model_ReadOnly()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, packagePivot.B5_ModelInfo.ReadOnly);
				packagePivot.Parent.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, packagePivot.B5_ModelInfo.ReadOnly);
			});
		}

		public void TestB5_GrossWeight_ResourceStringData()
		{
			CombineAssertions(() =>
			{
				SetUpArrivalPhase5Package();

				var captionResourceString = DataBoundResourceStrings.GetDataForProperty(packagePivot.B5_GrossWeightInfo);
				AssertEquals("Caption", "Gross Weight", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Gross W.", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Gross", captionResourceString.ShortCaption);
				AssertEquals("FullDescription", "Gross Weight Quantity", captionResourceString.FullDescription);
			});
		}

		public void TestB5_GrossWeight_Value()
		{
			CombineAssertions(() =>
			{
				SetUpArrivalPhase5Package();

				packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Default value when B5_TypeOfDifference is NEW", ZDecimal.Zero, packagePivot.B5_GrossWeight);

				packagePivot.B5_GrossWeight = 3;
				AssertEquals("When manually change B5_GrossWeight to 3", new ZDecimal(3), packagePivot.B5_GrossWeight);

				packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("When we change B5_TypeOfDifference to MIS", ZDecimal.Zero, packagePivot.B5_GrossWeight);
			});
		}

		public void TestB5_GrossWeight_ReadOnly()
		{
			SetUpArrivalPhase5Package();
			packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, packagePivot.B5_GrossWeightInfo.ReadOnly);
				packagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("ReadOnly", true, packagePivot.B5_GrossWeightInfo.ReadOnly);
			});
		}

		public void TestB5_GrossWeightUQ_ResourceStringData()
		{
			CombineAssertions(() =>
			{
				SetUpArrivalPhase5Package();

				var captionResourceString = DataBoundResourceStrings.GetDataForProperty(packagePivot.B5_GrossWeightUQInfo);
				AssertEquals("Caption", "Unit", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Unit", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "UQ", captionResourceString.ShortCaption);
				AssertEquals("FullDescription", "Gross Weight Unit", captionResourceString.FullDescription);
			});
		}

		public void TestB5_GrossWeightUQ_Value()
		{
			CombineAssertions(() =>
			{
				SetUpArrivalPhase5Package();

				packagePivot.B5_GrossWeight = 0;
				AssertEquals("When B5_GrossWeight is 0", string.Empty, packagePivot.B5_GrossWeightUQ);

				packagePivot.B5_GrossWeight = 3;
				AssertEquals("When B5_GrossWeight is 0", "KG", packagePivot.B5_GrossWeightUQ);

				packagePivot.B5_GrossWeight = 0;
				AssertEquals("When set back B5_GrossWeight to 0", string.Empty, packagePivot.B5_GrossWeightUQ);
			});
		}

		public void TestB5_GrossWeightUQ_ReadOnly()
		{
			SetUpArrivalPhase5Package();
			AssertEquals("ReadOnly", true, packagePivot.B5_GrossWeightUQInfo.ReadOnly);
		}

		public void TestValidationDeciderPhase4Arrival()
		{
			var package = GetNewBusinessObjectForP4Arrival(Factory);
			AssertNull("Phase4 Arrival", package.ValidationDecider);
		}

		public void TestValidationDeciderPhase4Departure()
		{
			var package = GetNewBusinessObjectForP4Departure(Factory);
			AssertNull("Phase4 Departure", package.ValidationDecider);
		}

		public void TestValidationDeciderPhase5Departure()
		{
			var package = GetNewBusinessObjectForP5Departure(Factory);
			AssertType<NctsPackageDeparturePhase5ValidationDecider>("Phase5 Departure", package.ValidationDecider);
		}

		public void TestValidationDeciderPhase5Arrival()
		{
			var package = GetNewBusinessObjectForP5Arrival(Factory);
			AssertType<NctsPackageArrivalPhase5ValidationDecider>("Phase5 Arrival", package.ValidationDecider);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForP5Arrival(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObjectForP5Arrival(factory);

		protected override NctsCommonCargoDesc GetNewParent()
		{
			var nctsHeader = Factory.New<NctsHeaderForTest>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		}

		NctsCommonCargoDesc GetNewParentArrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			return nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		}

		NctsPackage GetNewBusinessObjectForP5Arrival(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			return nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().Packages.AddNew();
		}

		NctsPackage GetNewBusinessObjectForP5Departure(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.Bills.AddNew().GoodsItems.AddNew().Packages.AddNew();
		}

		NctsPackage GetNewBusinessObjectForP4Arrival(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return header.ArrivalMovementHeader.GoodsItems.AddNew().Packages.AddNew();
		}

		NctsPackage GetNewBusinessObjectForP4Departure(BusinessObjectFactory factory)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.MovementHeader.GoodsItems.AddNew().Packages.AddNew();
		}

		void SetUpArrivalPhase5Package()
		{
			var nctsHeader = Factory.New<NctsHeaderForTest>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();

			packagePivot = (NctsPackage)GetNewPackagePivot();
			packagePivot.B5_ParentTableCode = goodsItem.TablePrefix;
			packagePivot.B5_ParentID = goodsItem.PK;
		}

		void SetUpDeparturePhase5Package()
		{
			var nctsHeader = Factory.New<NctsHeaderForTest>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			packagePivot = (NctsPackage)GetNewPackagePivot();
			packagePivot.B5_ParentTableCode = goodsItem.TablePrefix;
			packagePivot.B5_ParentID = goodsItem.PK;
		}

		new NctsPackage packagePivot { get => (NctsPackage)base.packagePivot; set => base.packagePivot = value; }
	}
}
