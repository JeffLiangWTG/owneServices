using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Licensing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceModulesDependentCollection : DependentBusinessObjectCollection<LicenceModules, BusinessObject>
	{
		public LicenceModulesDependentCollection(LicenceHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public LicenceModulesDependentCollection(LicenceHeader parent, ZQuery additionalFilter)
			: base(parent, additionalFilter)
		{
		}

		public LicenceModules FindByCode(string code)
		{
			LicenceModules result = null;

			foreach (LicenceModules module in this)
			{
				if (module.LM_GroupModuleCode == code)
				{
					result = module;
					break;
				}
			}

			return result;
		}

		#region Default Modules

		/// <summary>
		/// Synchronize this collection with the current list of modules in the code.
		/// Modules in the code list, but not in this collection are added.
		/// Modules in this collection, but not in the code list are removed.
		/// Modules will be added to this collection in the same order as the code list.
		/// </summary>
		public void MergeWithDefaultModules(bool sortOnly = false)
		{
			using (MasterHeader.SuspendSettingHasChanges())
			{
				Dictionary<string, LicenceModules> existingModules = new Dictionary<string, LicenceModules>();
				foreach (LicenceModules module1 in this)
				{
					existingModules.Add(module1.LM_GroupModuleCode.ToString(), module1);
				}
				RemoveAllButLeaveRelationshipsIntact();

				// New modules will be ODM if Core is ODM, otherwise NON
				string defaultLicenceType = LicenceTypes.Codes.NON;
				LicenceModules coreModule;
				if (existingModules.TryGetValue(LegacyLicence.Codes.Core, out coreModule))
				{
					if (coreModule.LM_LicenceType == LicenceTypes.Codes.ODM)
					{
						defaultLicenceType = LicenceTypes.Codes.ODM;
					}
				}
				else
				{
					// Core doesn't exist - this must be a completely new licence
					defaultLicenceType = GetDefaultLicenceType(MasterHeader);
				}

				var checkPoints = LegacyLicence.Instance.CheckpointOrder;
				foreach (var checkPoint in checkPoints)
				{
					LicenceModules module;
					bool isExistingModule = existingModules.TryGetValue(checkPoint.Name, out module);
					if (isExistingModule)
					{
						Add(module);
						existingModules.Remove(checkPoint.Name);
					}
					else if (!sortOnly)
					{
						module = AddNew();
						using (module.SuspendSettingHasChanges())
						{
							module.LM_GroupModuleCode = checkPoint.Name;
							if (checkPoint.IsManuallyEnabled)
							{
								module.LM_LicenceType = LicenceTypes.Codes.NON;
							}
							else if (checkPoint.GetDefaultEnabledLicenceValue() == LicenceTypes.Codes.NON)
							{
								module.LM_LicenceType = defaultLicenceType;
							}
							else
							{
								module.LM_LicenceType = checkPoint.GetDefaultLicenceValue();
							}
						}
					}
				}

				if (!sortOnly)
				{
					foreach (LicenceModules obsoleteModule in existingModules.Values)
					{
						obsoleteModule.Delete();
					}
				}
			}
		}

		public void UpdateForChangedEdition()
		{
			LicenceModules core = MasterHeader.GetCoreModule();
			if (core == null)
			{
				return;
			}

			string defaultLicenceType = GetDefaultLicenceType(MasterHeader);
			var lic = LegacyLicence.Instance;
			bool isNewOdmLicence = defaultLicenceType == LicenceTypes.Codes.ODM && !core.LM_Calc_IsEnabled;
			bool isConversionToOdm = defaultLicenceType == LicenceTypes.Codes.ODM &&
				(core.LM_LicenceType != LicenceTypes.Codes.ODM || MasterHeader.LA_LicenceAdvStdOth == LicenceAdvStdOthList.Codes.SeatTransaction);
			bool shouldEnable = isNewOdmLicence || isConversionToOdm;

			foreach (LicenceModules module in this)
			{
				var checkPoint = lic.GetCheckpointFromCode(module.LM_GroupModuleCode);

				// Ensure the LM_Calc_IsEnabled property is calculated
				// otherwise it may only be calculated if it is showing in the visible rows of the grid
				// which leads to confusing differences with rows on screen and rows off screen
				// when the LM_LicenceType is set to NON
				bool isEnabled = module.LM_Calc_IsEnabled;
				bool isValidLicenceType = module.Lookups.LicenceTypesList.ContainsCode(module.LM_LicenceType);
				bool shouldClearUserCount = false;

				if (!isEnabled)
				{
					if (shouldEnable)
					{
						module.LM_Calc_IsEnabled = !checkPoint.IsManuallyEnabled;
					}
					else if (!isValidLicenceType)
					{
						module.LM_LicenceType = LicenceTypes.Codes.NON;
					}
				}
				else if ((defaultLicenceType == LicenceTypes.Codes.ODM && module.LM_LicenceType == LicenceTypes.Codes.NON)
					|| !isValidLicenceType)
				{
					ZString initialLicenceType = module.LM_LicenceType;
					string enabledValue = checkPoint.GetDefaultEnabledLicenceValue();
					module.LM_LicenceType = (enabledValue == LicenceTypes.Codes.NON) ? defaultLicenceType : enabledValue;

					// Rental not supported for ODM
					if (isConversionToOdm && initialLicenceType == LicenceTypes.Codes.REN)
					{
						shouldClearUserCount = true;
					}
				}

				if (shouldClearUserCount)
				{
					module.LM_UserCount = 0;
					module.LM_ExpiryDate = ZDateTime.Empty;
				}
			}
		}

		static string GetDefaultLicenceType(LicenceHeader licenceHeader)
		{
			return licenceHeader.IsOnDemandModuleTypeAllowed
				&& licenceHeader.SupportsLineLevelOnDemandLicenceTypes != Enterprise.Customs.Business.TriState.False
				? LicenceTypes.Codes.ODM
				: LicenceTypes.Codes.NON;
		}

		#endregion

		public bool HasLineLevelOnDemandLicenceType
		{
			get
			{
				foreach (LicenceModules module in this)
				{
					if (module.HasLineLevelOnDemandLicenceType)
					{
						return true;
					}
				}

				return false;
			}
		}

		public void PopulateFromQuote(Dictionary<string, int> codeUserCountMap)
		{
			foreach (var pair in codeUserCountMap)
			{
				var module = FindByCode(pair.Key);
				if (module != null)
				{
					module.LM_Calc_IsEnabled = true;
					module.LM_LicenceType = LicenceTypes.Codes.PUR;
					module.LM_UserCount = (ZShort)pair.Value;
				}
			}
		}

		public LicenceHeader MasterHeader
		{
			get { return (LicenceHeader)Master; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}
	}
}

