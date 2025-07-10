using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAOrgSupplierPartAndClassificationDataLoad_PGAHelper
	{
		internal CAOrgSupplierPartAndClassificationDataLoad_PGAHelper(BusinessObjectFactory factory, IExposeMethodsForPGADataLoad methods)
		{
			this.factory = factory;
			this.methods = methods;
		}

		readonly BusinessObjectFactory factory;
		readonly IExposeMethodsForPGADataLoad methods;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal void SetPGAIndicator(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			SetCFIAPGAIndicators(pgaSupporter, data);
			SetGACPGAIndicators(pgaSupporter, data);
			SetDFOPGAIndicators(pgaSupporter, data);
			SetECCCPGAIndicators(pgaSupporter, data);
			SetHCPGAIndicators(pgaSupporter, data);
			SetNRCanPGAIndicators(pgaSupporter, data);
			SetPHACPGAIndicators(pgaSupporter, data);
			SetTCPGAIndicators(pgaSupporter, data);
			SetCNSCPGAIndicators(pgaSupporter, data);
		}

		void SetCNSCPGAIndicators(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			var hasCNSCProgramInd = data.PGA_CNSC_Indicator.HasValue;
			if (hasCNSCProgramInd)
			{
				var cnsc_Ind = GetYesNoString(data.PGA_CNSC_Indicator.Value);
				if (YesNoList.IsYesOrNo(cnsc_Ind))
				{
					SetValue(pgaSupporter.CNSCIndInfo, cnsc_Ind, false, null, setterSuspender: pgaSupporter.SetterSuspender, resumeSuspender: true);
					if (pgaSupporter.CNSCRequirementProvider is CNSCPGAHeader cnsc)
					{
						methods.AddToDisposableList(cnsc.SetterSuspender.SuspendSetting(CNSCPGAHeader.Schema.CA_AllProgramInd));
						SetValue(cnsc.CA_AllProgramIndInfo, cnsc_Ind, false, setterSuspender: cnsc.SetterSuspender, resumeSuspender: true);
						if (YesNoList.IsYes(cnsc_Ind))
						{
							SetValue(cnsc.CA_CategoryInfo, data.PGA_CNSC_Category);
							SetValue(cnsc.CA_NNIECRSchePartNoInfo, data.PGA_CNSC_NNIECRSchedulePartNo);
							SetValue(cnsc.CA_PackMarksInfo, data.PGA_CNSC_PackMarks);
							SetPGACNSCLPCOs(cnsc, data);
						}
					}
				}
			}
		}

		void SetPGACNSCLPCOs(CNSCPGAHeader cnscPGAHeader, IPGADataToLoad data)
		{
			var hasCNSCLPCOs = data.PGA_CNSC_LPCOs.HasValue;
			if (hasCNSCLPCOs)
			{
				var cnscLPCOs = data.PGA_CNSC_LPCOs.Value;
				if (cnscLPCOs.IsEmpty)
				{
					cnscPGAHeader.LPCOViews.RemoveAndDeleteAll();
				}
				else
				{
					var lpcos = cnscPGAHeader.LPCOViews.Cast<LPCOView>().ToList();
					var importedLPCOs = cnscLPCOs.Split(";");
					foreach (var importedLPCO in importedLPCOs)
					{
						var fields = importedLPCO.Split("/");
						if (fields.Length == 2)
						{
							var type = fields[0].Left(CusCALPCO.Schema.CLP_TypeMaxLength);
							var refNo = fields[1].Left(CusCALPCO.Schema.CLP_RefNoMaxLength);
							var lpco = lpcos.FirstOrDefault(x => x.CLP_Type == type && x.CLP_RefNo == refNo);
							if (lpco == null)
							{
								lpco = cnscPGAHeader.LPCOViews.AddNew();
								SetValue(lpco.CLP_TypeInfo, type);
								SetValue(lpco.CLP_RefNoInfo, refNo);
							}
							else
							{
								lpcos.Remove(lpco);
							}
						}
					}
					lpcos.ForEach(x => x.LPCO.Delete());
				}
			}
		}

		void SetCFIAPGAIndicators(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			var hasCFIAIndicator = data.PGA_CFIA_Indicator.HasValue;
			if (hasCFIAIndicator)
			{
				var cCA_CFIAIndicator = GetYesNoString(data.PGA_CFIA_Indicator.Value);
				if (YesNoList.IsYesOrNo(cCA_CFIAIndicator))
				{
					SetValue(pgaSupporter.CFIAIndInfo, cCA_CFIAIndicator, false, null, setterSuspender: pgaSupporter.SetterSuspender, resumeSuspender: true);
					if (pgaSupporter.CFIARequirementProvider is CFIAPGAHeader cfia)
					{
						methods.AddToDisposableList(cfia.SetterSuspender.SuspendSetting(CFIAPGAHeader.Schema.CA_AllProgramInd));
						SetValue(cfia.CA_AllProgramIndInfo, cCA_CFIAIndicator, false, setterSuspender: cfia.SetterSuspender, resumeSuspender: true);
						if (YesNoList.IsYes(cCA_CFIAIndicator))
						{
							SetValue(cfia.RN_NKCountryOfSourceInfo, data.PGA_CFIA_SourceCountry);
							if (cfia.RN_NKCountryOfSource == Core.Constants.CountryCodes.UnitedStates)
							{
								SetValue(cfia.RW_NKSourceStateInfo, data.PGA_CFIA_SourceState);
							}
							SetValue(cfia.CA_AIRSExtensionCodeInfo, data.PGA_CFIA_AIRSExtensionCode);
							SetValue(cfia.CA_AIRSEndUseInfo, data.PGA_CFIA_AIRSEndUse);
							SetValue(cfia.CA_AIRSMiscellaneousInfo, data.PGA_CFIA_AIRSMiscellaneous);
							AddLPCOsToCFIAPGAHeader(cfia, data.PGA_CFIA_LPCOs);
							AddRegistrationsToCFIAPGAHeader(cfia, data.PGA_CFIA_AIRSRegistrations);
						}
					}
				}
			}
		}

		void AddLPCOsToCFIAPGAHeader(CFIAPGAHeader header, ZString? stream)
		{
			if (stream.HasValue)
			{
				var lpcosValue = stream.Value;
				if (lpcosValue.IsEmpty)
				{
					header.LPCOViews.RemoveAndDeleteAll();
				}
				else
				{
					var lpcos = header.LPCOViews.Cast<LPCOView>().ToList();
					var importedLPCOs = lpcosValue.Split(";");
					foreach (var importedLPCO in importedLPCOs)
					{
						var fields = importedLPCO.Split("/");
						if (fields.Length > 1)
						{
							var type = fields[0].Left(CusCALPCO.Schema.CLP_TypeMaxLength);
							var refNo = fields[1].Left(CusCALPCO.Schema.CLP_RefNoMaxLength);
							var lpco = lpcos.FirstOrDefault(x => x.CLP_Type == type && x.CLP_RefNo == refNo);
							if (lpco == null)
							{
								lpco = header.LPCOViews.AddNew();
								SetValue(lpco.CLP_TypeInfo, type);
								SetValue(lpco.CLP_RefNoInfo, refNo);
							}
							else
							{
								lpcos.Remove(lpco);
							}

							if (fields.Length > 2)
							{
								SetValue(lpco.CLP_DIFRefNumberOrLocationInfo, fields[2]);
							}
						}
					}

					lpcos.ForEach(x => x.LPCO.Delete());
				}
			}
		}

		void AddRegistrationsToCFIAPGAHeader(CFIAPGAHeader header, ZString? stream)
		{
			if (stream.HasValue)
			{
				var registrationNumbersValue = stream.Value;
				if (registrationNumbersValue.IsEmpty)
				{
					header.AIRSRegistrationNumbers.RemoveAndDeleteAll();
				}
				else
				{
					var registrationNumbers = header.AIRSRegistrationNumbers.Cast<AIRSRegistrationNumber>().ToList();
					var importedRegs = registrationNumbersValue.Split(";");
					foreach (var importedReg in importedRegs)
					{
						var fields = importedReg.Split("/");
						if (fields.Length > 1)
						{
							var code = fields[0].Left(AIRSRegistrationNumber.Schema.CY_CodeMaxLength);
							var data = fields[1].Left(AIRSRegistrationNumber.Schema.CY_DataMaxLength);
							var registrationNumber = registrationNumbers.FirstOrDefault(x => x.CY_Code == code && x.CY_Data == data);
							if (registrationNumber == null)
							{
								registrationNumber = header.AIRSRegistrationNumbers.AddNew();
								SetValue(registrationNumber.CY_CodeInfo, code);
								SetValue(registrationNumber.CY_DataInfo, data);
							}
							else
							{
								registrationNumbers.Remove(registrationNumber);
							}
						}
					}

					registrationNumbers.DeleteAll();
				}
			}
		}

		void SetGACPGAIndicators(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			var hasGACIndicator = data.PGA_GAC_Indicator.HasValue;
			if (hasGACIndicator)
			{
				var cCA_GACIndicator = GetYesNoString(data.PGA_GAC_Indicator.Value);
				if (YesNoList.IsYesOrNo(cCA_GACIndicator))
				{
					SetValue(pgaSupporter.GACIndInfo, cCA_GACIndicator, false, null, setterSuspender: pgaSupporter.SetterSuspender, resumeSuspender: true);
					if (pgaSupporter.GACRequirementProvider is GACPGAHeader gac)
					{
						SetValue(gac.CA_AllProgramIndInfo, cCA_GACIndicator);
						methods.AddToDisposableList(gac.SetterSuspender.SuspendSetting(GACPGAHeader.Schema.CA_AllProgramInd));
					}
				}
			}
		}

		void SetDFOPGAIndicators(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			var propertiesToSuspendSetting = new List<ZString>();
			var dfoPGASetters = new List<Action<DFOPGAHeader>>();
			var cCA_DFOABIIndicator = ZString.Empty;
			var hasDFOABIInd = data.PGA_DFO_ABIInd.HasValue;
			if (hasDFOABIInd)
			{
				cCA_DFOABIIndicator = GetYesNoString(data.PGA_DFO_ABIInd.Value);
				if (YesNoList.IsYesOrNo(cCA_DFOABIIndicator))
				{
					propertiesToSuspendSetting.Add(DFOPGAHeader.Schema.CA_ABIProgramInd);
					dfoPGASetters.Add((x) =>
					{
						SetValue(x.CA_ABIProgramIndInfo, cCA_DFOABIIndicator, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var cCA_DFOAISIndicator = ZString.Empty;
			var hasDFOAISInd = data.PGA_DFO_AISInd.HasValue;
			if (hasDFOAISInd)
			{
				cCA_DFOAISIndicator = GetYesNoString(data.PGA_DFO_AISInd.Value);
				if (YesNoList.IsYesOrNo(cCA_DFOAISIndicator))
				{
					propertiesToSuspendSetting.Add(DFOPGAHeader.Schema.CA_AISProgramInd);
					dfoPGASetters.Add((x) =>
					{
						SetValue(x.CA_AISProgramIndInfo, cCA_DFOAISIndicator, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var cCA_DFOTTPIndicator = ZString.Empty;
			var hasDFOTTPInd = data.PGA_DFO_TTPInd.HasValue;
			if (hasDFOTTPInd)
			{
				cCA_DFOTTPIndicator = GetYesNoString(data.PGA_DFO_TTPInd.Value);
				if (YesNoList.IsYesOrNo(cCA_DFOTTPIndicator))
				{
					propertiesToSuspendSetting.Add(DFOPGAHeader.Schema.CA_TTPProgramInd);
					dfoPGASetters.Add((x) =>
					{
						SetValue(x.CA_TTPProgramIndInfo, cCA_DFOTTPIndicator, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			if (propertiesToSuspendSetting.Count > 0)
			{
				bool isDFOIndicatorSetToYes = YesNoList.IsYes(cCA_DFOABIIndicator) || YesNoList.IsYes(cCA_DFOAISIndicator) || YesNoList.IsYes(cCA_DFOTTPIndicator);
				var dfoIndicator = isDFOIndicatorSetToYes ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				SetValue(pgaSupporter.DFOIndInfo, dfoIndicator, false, null, setterSuspender: pgaSupporter.SetterSuspender, resumeSuspender: true);
				if (pgaSupporter.DFORequirementProvider is DFOPGAHeader dfo)
				{
					methods.AddToDisposableList(dfo.SetterSuspender.SuspendSetting(propertiesToSuspendSetting.ToArray()));
					dfoPGASetters.ForEach(x => x(dfo));
				}
			}
		}

		void SetECCCPGAIndicators(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			var propertiesToSuspendSetting = new List<ZString>();
			var ecccPGASetters = new List<Action<ECCCPGAHeader>>();
			var pGA_ECCC_WRMInd = ZString.Empty;
			var hasWRMProgramInd = data.PGA_ECCC_WRMInd.HasValue;
			if (hasWRMProgramInd)
			{
				pGA_ECCC_WRMInd = GetYesNoString(data.PGA_ECCC_WRMInd.Value);
				if (YesNoList.IsYesOrNo(pGA_ECCC_WRMInd))
				{
					propertiesToSuspendSetting.Add(ECCCPGAHeader.Schema.CA_WRMProgramInd);
					ecccPGASetters.Add((x) =>
					{
						SetValue(x.CA_WRMProgramIndInfo, pGA_ECCC_WRMInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var pGA_ECCC_ODSInd = ZString.Empty;
			var hasODSProgramInd = data.PGA_ECCC_ODSInd.HasValue;
			if (hasODSProgramInd)
			{
				pGA_ECCC_ODSInd = GetYesNoString(data.PGA_ECCC_ODSInd.Value);
				if (YesNoList.IsYesOrNo(pGA_ECCC_ODSInd))
				{
					propertiesToSuspendSetting.Add(ECCCPGAHeader.Schema.CA_ODSProgramInd);
					ecccPGASetters.Add((x) =>
					{
						SetValue(x.CA_ODSProgramIndInfo, pGA_ECCC_ODSInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var pGA_ECCC_WENInd = ZString.Empty;
			var hasWENProgramInd = data.PGA_ECCC_WENInd.HasValue;
			if (hasWENProgramInd)
			{
				pGA_ECCC_WENInd = GetYesNoString(data.PGA_ECCC_WENInd.Value);
				if (YesNoList.IsYesOrNo(pGA_ECCC_WENInd))
				{
					propertiesToSuspendSetting.Add(ECCCPGAHeader.Schema.CA_WENProgramInd);
					ecccPGASetters.Add((x) =>
					{
						SetValue(x.CA_WENProgramIndInfo, pGA_ECCC_WENInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var pGA_ECCC_VEEInd = ZString.Empty;
			var hasVEEProgramInd = data.PGA_ECCC_VEEInd.HasValue;
			if (hasVEEProgramInd)
			{
				pGA_ECCC_VEEInd = GetYesNoString(data.PGA_ECCC_VEEInd.Value);
				if (YesNoList.IsYesOrNo(pGA_ECCC_VEEInd))
				{
					propertiesToSuspendSetting.Add(ECCCPGAHeader.Schema.CA_VEEProgramInd);
					ecccPGASetters.Add((x) =>
					{
						SetValue(x.CA_VEEProgramIndInfo, pGA_ECCC_VEEInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			if (propertiesToSuspendSetting.Count > 0)
			{
				if (data.PGA_ECCC_IntendedUseCode.HasValue)
				{
					propertiesToSuspendSetting.Add(ECCCPGAHeader.Schema.CA_IntendedUseCode);
					ecccPGASetters.Add((x) =>
					{
						SetValue(x.CA_IntendedUseCodeInfo, data.PGA_ECCC_IntendedUseCode, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
				if (data.PGA_ECCC_CASNumber.HasValue)
				{
					propertiesToSuspendSetting.Add(ECCCPGAHeader.Schema.CA_CASNumber);
					ecccPGASetters.Add((x) =>
					{
						SetValue(x.CA_CASNumberInfo, data.PGA_ECCC_CASNumber, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}

				bool isEcccIndicatorSetToYes = YesNoList.IsYes(pGA_ECCC_WRMInd) || YesNoList.IsYes(pGA_ECCC_ODSInd) || YesNoList.IsYes(pGA_ECCC_WENInd) || YesNoList.IsYes(pGA_ECCC_VEEInd);
				var eCCCIndicator = isEcccIndicatorSetToYes ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				SetValue(pgaSupporter.ECCCIndInfo, eCCCIndicator, false, null, setterSuspender: pgaSupporter.SetterSuspender, resumeSuspender: true);

				if (pgaSupporter.ECCCRequirementProvider is ECCCPGAHeader eccc)
				{
					methods.AddToDisposableList(eccc.SetterSuspender.SuspendSetting(propertiesToSuspendSetting.ToArray()));

					if (hasWENProgramInd)
					{
						if (YesNoList.IsYes(pGA_ECCC_WENInd))
						{
							SetValue(eccc.CA_SourceOfSpecimenInfo, data.PGA_ECCC_SourceOfSpecimen);
							SetValue(eccc.CA_LifeStageInfo, data.PGA_ECCC_LifeStage);
							SetValue(eccc.CA_AgeInfo, data.PGA_ECCC_Age);
							SetValue(eccc.CA_SexInfo, data.PGA_ECCC_Sex);
							if (data.PGA_ECCC_Regulated.HasValue)
							{
								eccc.CA_ComplianceDeclaration = IsValidYesReply(data.PGA_ECCC_Regulated.Value);
							}
							SetValue(eccc.CA_ScientificNameInfo, data.PGA_ECCC_ScientificName);
							SetValue(eccc.CA_TSNInfo, data.PGA_ECCC_TSN);
							SetValue(eccc.CA_AphiaIDInfo, data.PGA_ECCC_AphiaID);
						}
					}
					if (hasVEEProgramInd)
					{
						if (YesNoList.IsYes(pGA_ECCC_VEEInd))
						{
							SetValue(eccc.CA_ProcessCodeInfo, data.PGA_ECCC_ProcessCode);
							if (data.PGA_ECCC_NationalMark.HasValue)
							{
								eccc.CA_NationalMark = IsValidYesReply(data.PGA_ECCC_NationalMark.Value);
							}
							if (data.PGA_ECCC_EPACertified.HasValue)
							{
								eccc.CA_EPACertified = IsValidYesReply(data.PGA_ECCC_EPACertified.Value);
							}
							if (data.PGA_ECCC_Incomplete.HasValue)
							{
								eccc.CA_Incomplete = IsValidYesReply(data.PGA_ECCC_Incomplete.Value);
							}
							if (data.PGA_ECCC_CanadaUnique.HasValue)
							{
								eccc.CA_CanadaUnique = IsValidYesReply(data.PGA_ECCC_CanadaUnique.Value);
							}
							if (data.PGA_ECCC_BulkReporting.HasValue)
							{
								eccc.CA_BulkReporting = IsValidYesReply(data.PGA_ECCC_BulkReporting.Value);
							}
							switch (eccc.CA_ProcessCode)
							{
								case ProcessCodes.Codes.XE01:
								case ProcessCodes.Codes.EC01:
									SetValue(eccc.CA_VehicleClassInfo, data.PGA_ECCC_VehicleClass);
									SetValue(eccc.CA_EngineClassInfo, data.PGA_ECCC_EngineClass);
									SetValue(eccc.CA_MakeOfEngineInfo, data.PGA_ECCC_EngineMake);
									SetValue(eccc.CA_ModelOfEngineInfo, data.PGA_ECCC_EngineModel);
									SetValue(eccc.CA_EngineModelYearInfo, data.PGA_ECCC_EngineModelYear);
									SetValue(eccc.CA_EngineIDNumberInfo, data.PGA_ECCC_EngineIDNumber);
									SetValue(eccc.CA_EngineManufacturerInfo, data.PGA_ECCC_EngineManufacturer);
									SetValue(eccc.CA_EngineFamilyNameInfo, data.PGA_ECCC_EngineFamilyName);
									SetValue(eccc.CA_TestGroupNameInfo, data.PGA_ECCC_EngineTestGroup);
									break;
								case ProcessCodes.Codes.XE02:
								case ProcessCodes.Codes.EC02:
									if (data.PGA_ECCC_Transition.HasValue)
									{
										eccc.CA_Transition = IsValidYesReply(data.PGA_ECCC_Transition.Value);
									}
									SetValue(eccc.CA_EngineClassInfo, data.PGA_ECCC_EngineClass);
									SetValue(eccc.CA_MakeOfEngineInfo, data.PGA_ECCC_EngineMake);
									SetValue(eccc.CA_ModelOfEngineInfo, data.PGA_ECCC_EngineModel);
									SetValue(eccc.CA_EngineModelYearInfo, data.PGA_ECCC_EngineModelYear);
									SetValue(eccc.CA_EngineIDNumberInfo, data.PGA_ECCC_EngineIDNumber);
									SetValue(eccc.CA_EngineManufacturerInfo, data.PGA_ECCC_EngineManufacturer);
									SetValue(eccc.CA_EngineFamilyNameInfo, data.PGA_ECCC_EngineFamilyName);
									SetValue(eccc.CA_EnginePowerRatingInfo, data.PGA_ECCC_EnginePowerRating);
									SetValue(eccc.CA_PowerRatingUQInfo, data.PGA_ECCC_EnginePowerRatingUQ);

									SetValue(eccc.CA_MakeOfMachineInfo, data.PGA_ECCC_MachineMake);
									SetValue(eccc.CA_ModelOfMachineInfo, data.PGA_ECCC_MachineModel);
									SetValue(eccc.CA_MachineModelYearInfo, data.PGA_ECCC_MachineModelYear);
									if (data.PGA_ECCC_MachineManufacturer.HasValue)
									{
										eccc.CA_MachineManufacturer_ZAddress.OrgPK = OrgHeader.LoadFromCode(factory, data.PGA_ECCC_MachineManufacturer.Value)?.PK ?? ZGuid.Empty;
									}
									if (data.PGA_ECCC_EngineLocation.HasValue)
									{
										eccc.CA_OA_EngineLocation_ZAddress.OrgPK = OrgHeader.LoadFromCode(factory, data.PGA_ECCC_EngineLocation.Value)?.PK ?? ZGuid.Empty;
									}
									if (data.PGA_ECCC_EvidenceOfConfirmityLocation.HasValue)
									{
										eccc.CA_OA_EvidenceOfConformityLocation_ZAddress.OrgPK = OrgHeader.LoadFromCode(factory, data.PGA_ECCC_EvidenceOfConfirmityLocation.Value)?.PK ?? ZGuid.Empty;
									}
									break;
								case ProcessCodes.Codes.XE03:
								case ProcessCodes.Codes.EC03:
									SetValue(eccc.CA_EngineClassInfo, data.PGA_ECCC_EngineClass);
									SetValue(eccc.CA_MakeOfEngineInfo, data.PGA_ECCC_EngineMake);
									SetValue(eccc.CA_ModelOfEngineInfo, data.PGA_ECCC_EngineModel);
									SetValue(eccc.CA_EngineModelYearInfo, data.PGA_ECCC_EngineModelYear);
									SetValue(eccc.CA_EngineIDNumberInfo, data.PGA_ECCC_EngineIDNumber);
									SetValue(eccc.CA_EngineManufacturerInfo, data.PGA_ECCC_EngineManufacturer);
									SetValue(eccc.CA_EngineFamilyNameInfo, data.PGA_ECCC_EngineFamilyName);
									SetValue(eccc.CA_EnginePowerRatingInfo, data.PGA_ECCC_EnginePowerRating);
									SetValue(eccc.CA_PowerRatingUQInfo, data.PGA_ECCC_EnginePowerRatingUQ);

									SetValue(eccc.CA_MakeOfMachineInfo, data.PGA_ECCC_MachineMake);
									SetValue(eccc.CA_ModelOfMachineInfo, data.PGA_ECCC_MachineModel);
									if (data.PGA_ECCC_MachineManufacturer.HasValue)
									{
										eccc.CA_MachineManufacturer_ZAddress.OrgPK = OrgHeader.LoadFromCode(factory, data.PGA_ECCC_MachineManufacturer.Value)?.PK ?? ZGuid.Empty;
									}
									break;
								case ProcessCodes.Codes.XE04:
								case ProcessCodes.Codes.EC04:
									SetValue(eccc.CA_VehicleClassInfo, data.PGA_ECCC_VehicleClass);
									if (data.PGA_ECCC_MachineManufacturer.HasValue)
									{
										eccc.CA_MachineManufacturer_ZAddress.OrgPK = OrgHeader.LoadFromCode(factory, data.PGA_ECCC_MachineManufacturer.Value)?.PK ?? ZGuid.Empty;
									}
									SetValue(eccc.CA_EngineClassInfo, data.PGA_ECCC_EngineClass);
									SetValue(eccc.CA_MakeOfEngineInfo, data.PGA_ECCC_EngineMake);
									SetValue(eccc.CA_ModelOfEngineInfo, data.PGA_ECCC_EngineModel);
									SetValue(eccc.CA_EngineModelYearInfo, data.PGA_ECCC_EngineModelYear);
									SetValue(eccc.CA_EngineIDNumberInfo, data.PGA_ECCC_EngineIDNumber);
									SetValue(eccc.CA_EngineManufacturerInfo, data.PGA_ECCC_EngineManufacturer);
									SetValue(eccc.CA_EngineFamilyNameInfo, data.PGA_ECCC_EngineFamilyName);
									SetValue(eccc.CA_EvaporativeFamilyInfo, data.PGA_ECCC_EngineEvaporativeFamily);
									break;
								default:
									break;
							}
						}
					}
					ecccPGASetters.ForEach(x => x(eccc));
					SetPGAECCCIdentities(eccc, data);
					SetPGAECCCLPCOs(eccc, data);
				}
			}
		}

		ZString GetYesNoString(ZString reply)
		{
			if (reply.EqualsIgnoringCase(YesNoList.Codes.Yes) || reply.EqualsIgnoringCase(YesNoList.Descriptions.Yes))
			{
				return YesNoList.Codes.Yes;
			}
			else if (reply.EqualsIgnoringCase(YesNoList.Codes.No) || reply.EqualsIgnoringCase(YesNoList.Descriptions.No))
			{
				return YesNoList.Codes.No;
			}
			else
			{
				return ZString.Empty;
			}
		}

		void SetPGAECCCIdentities(ECCCPGAHeader ecccPGAHeader, IPGADataToLoad data)
		{
			var ecccIdentities = data.PGA_ECCC_Identities.GetValueOrDefault();
			if (!ecccIdentities.IsEmpty)
			{
				var compenents = ecccPGAHeader.Components;
				compenents.RemoveAndDeleteAll();
				var identities = ecccIdentities.Split(";");
				foreach (var identity in identities)
				{
					var fields = identity.Split("/");
					if (fields.Length == 2)
					{
						var newIdentity = compenents.AddNew();
						newIdentity.CA_Type = fields[0].Left(newIdentity.CA_TypeInfo.MaxLength);
						newIdentity.CA_Name = fields[1].Left(newIdentity.CA_NameInfo.MaxLength);
					}
				}
			}
		}

		void SetPGAECCCLPCOs(ECCCPGAHeader ecccPGAHeader, IPGADataToLoad data)
		{
			var hasECCCLPCOs = data.PGA_ECCC_LPCOs.HasValue;
			if (hasECCCLPCOs)
			{
				var ecccLPCOs = data.PGA_ECCC_LPCOs.Value;
				if (ecccLPCOs.IsEmpty)
				{
					ecccPGAHeader.LPCOViews.RemoveAndDeleteAll();
				}
				else
				{
					var lpcos = ecccPGAHeader.LPCOViews.Cast<LPCOView>().ToList();
					var importedLPCOs = ecccLPCOs.Split(";");
					foreach (var importedLPCO in importedLPCOs)
					{
						var fields = importedLPCO.Split("/");
						if (fields.Length > 1)
						{
							var type = fields[0].Left(CusCALPCO.Schema.CLP_TypeMaxLength);
							var refNo = fields[1].Left(CusCALPCO.Schema.CLP_RefNoMaxLength);
							var lpco = lpcos.FirstOrDefault(x => x.CLP_Type == type && x.CLP_RefNo == refNo);
							if (lpco == null)
							{
								lpco = ecccPGAHeader.LPCOViews.AddNew();
								SetValue(lpco.CLP_TypeInfo, type);
								SetValue(lpco.CLP_RefNoInfo, refNo);
							}
							else
							{
								lpcos.Remove(lpco);
							}

							if (fields.Length > 2)
							{
								SetValue(lpco.CLP_DIFRefNumberOrLocationInfo, fields[2]);
							}

							if (fields.Length > 3)
							{
								lpco.CLP_AlternativeQuotaQuantity = ZDecimal.ParseSafe(fields[3].Left(lpco.CLP_AlternativeQuotaQuantityInfo.MaxLength), 0);
							}

							if (fields.Length > 4)
							{
								SetValue(lpco.CLP_AlternativeQuotaUQInfo, fields[4]);
							}
						}
					}

					if (lpcos.Count > 0)
					{
						var defaultTypes = ecccPGAHeader.GetDefaultLPCOTypesForAllEnalbedPrograms().Select(x => x.Code).ToHashSet();
						if (defaultTypes.Count > 0)
						{
							lpcos.ForEach(x =>
							{
								if (!defaultTypes.Contains(x.CLP_Type))
								{
									x.LPCO.Delete();
								}
							});
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void SetHCPGAIndicators(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			var propertiesToSuspendSetting = new List<ZString>();
			var hcPGASetters = new List<Action<HCPGAHeader>>();
			var shouldSetCommonFields = false;
			var fieldsActivators = new Dictionary<ZString, ZBool>();

			var hC_APIInd = ZString.Empty;
			var hasHCAPIInd = data.PGA_HC_APIInd.HasValue;
			if (hasHCAPIInd)
			{
				hC_APIInd = GetYesNoString(data.PGA_HC_APIInd.Value);
				if (YesNoList.IsYesOrNo(hC_APIInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_APIProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_APIProgramIndInfo, hC_APIInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_BBCInd = ZString.Empty;
			var hasHCBBCInd = data.PGA_HC_BBCInd.HasValue;
			if (hasHCBBCInd)
			{
				hC_BBCInd = GetYesNoString(data.PGA_HC_BBCInd.Value);
				if (YesNoList.IsYesOrNo(hC_BBCInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_BBCProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_BBCProgramIndInfo, hC_BBCInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_CPRInd = ZString.Empty;
			var hasHCCPRInd = data.PGA_HC_CPRInd.HasValue;
			if (hasHCCPRInd)
			{
				hC_CPRInd = GetYesNoString(data.PGA_HC_CPRInd.Value);
				if (YesNoList.IsYesOrNo(hC_CPRInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_CPRProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_CPRProgramIndInfo, hC_CPRInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_CTOInd = ZString.Empty;
			var hasHCCTOInd = data.PGA_HC_CTOInd.HasValue;
			if (hasHCCTOInd)
			{
				hC_CTOInd = GetYesNoString(data.PGA_HC_CTOInd.Value);
				if (YesNoList.IsYesOrNo(hC_CTOInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_CTOProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_CTOProgramIndInfo, hC_CTOInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_DSEInd = ZString.Empty;
			var hasHCDSEInd = data.PGA_HC_DSEInd.HasValue;
			if (hasHCDSEInd)
			{
				hC_DSEInd = GetYesNoString(data.PGA_HC_DSEInd.Value);
				if (YesNoList.IsYesOrNo(hC_DSEInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_DSEProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_DSEProgramIndInfo, hC_DSEInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_HDRInd = ZString.Empty;
			var hasHCHDRInd = data.PGA_HC_HDRInd.HasValue;
			if (hasHCHDRInd)
			{
				hC_HDRInd = GetYesNoString(data.PGA_HC_HDRInd.Value);
				if (YesNoList.IsYesOrNo(hC_HDRInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_HDRProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_HDRProgramIndInfo, hC_HDRInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_MDEInd = ZString.Empty;
			var hasHCMDEInd = data.PGA_HC_MDEInd.HasValue;
			if (hasHCMDEInd)
			{
				hC_MDEInd = GetYesNoString(data.PGA_HC_MDEInd.Value);
				if (YesNoList.IsYesOrNo(hC_MDEInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_MDEProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_MDEProgramIndInfo, hC_MDEInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_NHPInd = ZString.Empty;
			var hasHCNHPInd = data.PGA_HC_NHPInd.HasValue;
			if (hasHCNHPInd)
			{
				hC_NHPInd = GetYesNoString(data.PGA_HC_NHPInd.Value);
				if (YesNoList.IsYesOrNo(hC_NHPInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_NHPProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_NHPProgramIndInfo, hC_NHPInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_OCSInd = ZString.Empty;
			var hasHCOCSInd = data.PGA_HC_OCSInd.HasValue;
			if (hasHCOCSInd)
			{
				hC_OCSInd = GetYesNoString(data.PGA_HC_OCSInd.Value);
				if (YesNoList.IsYesOrNo(hC_OCSInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_OCSProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_OCSProgramIndInfo, hC_OCSInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_PESInd = ZString.Empty;
			var hasHCPESInd = data.PGA_HC_PESInd.HasValue;
			if (hasHCPESInd)
			{
				hC_PESInd = GetYesNoString(data.PGA_HC_PESInd.Value);
				if (YesNoList.IsYesOrNo(hC_PESInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_PESProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_PESProgramIndInfo, hC_PESInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_REDInd = ZString.Empty;
			var hasHCREDInd = data.PGA_HC_REDInd.HasValue;
			if (hasHCREDInd)
			{
				hC_REDInd = GetYesNoString(data.PGA_HC_REDInd.Value);
				if (YesNoList.IsYesOrNo(hC_REDInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_REDProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_REDProgramIndInfo, hC_REDInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var hC_VETInd = ZString.Empty;
			var hasHCVETInd = data.PGA_HC_VETInd.HasValue;
			if (hasHCVETInd)
			{
				hC_VETInd = GetYesNoString(data.PGA_HC_VETInd.Value);
				if (YesNoList.IsYesOrNo(hC_VETInd))
				{
					propertiesToSuspendSetting.Add(HCPGAHeader.Schema.CA_VETProgramInd);
					hcPGASetters.Add((x) =>
					{
						SetValue(x.CA_VETProgramIndInfo, hC_VETInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			if (propertiesToSuspendSetting.Count > 0)
			{
				bool isHCIndicatorSetToYes =
					YesNoList.IsYes(hC_APIInd) ||
					YesNoList.IsYes(hC_BBCInd) ||
					YesNoList.IsYes(hC_CPRInd) ||
					YesNoList.IsYes(hC_CTOInd) ||
					YesNoList.IsYes(hC_DSEInd) ||
					YesNoList.IsYes(hC_HDRInd) ||
					YesNoList.IsYes(hC_MDEInd) ||
					YesNoList.IsYes(hC_NHPInd) ||
					YesNoList.IsYes(hC_OCSInd) ||
					YesNoList.IsYes(hC_PESInd) ||
					YesNoList.IsYes(hC_REDInd) ||
					YesNoList.IsYes(hC_VETInd);

				var hcIndicator = isHCIndicatorSetToYes ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				SetValue(pgaSupporter.HCIndInfo, hcIndicator, false, null, setterSuspender: pgaSupporter.SetterSuspender, resumeSuspender: true);
				if (pgaSupporter.HCRequirementProvider is HCPGAHeader hc)
				{
					methods.AddToDisposableList(hc.SetterSuspender.SuspendSetting(propertiesToSuspendSetting.ToArray()));

					if (hasHCAPIInd)
					{
						if (YesNoList.IsYes(hC_APIInd))
						{
							SetValue(hc.CA_IntendedUseCodeAPIInfo, data.PGA_HC_IntendedUseCodeAPI);
							SetValue(hc.CA_CategoryAPIInfo, data.PGA_HC_CommodityTypeAPI);
							shouldSetCommonFields = true;
							fieldsActivators["GTINNo"] = true;
							fieldsActivators["BatchLotNo"] = true;
						}
					}

					if (hasHCBBCInd)
					{
						if (YesNoList.IsYes(hC_BBCInd))
						{
							SetValue(hc.CA_IntendedUseCodeBBCInfo, data.PGA_HC_IntendedUseCodeBBC);
							SetValue(hc.CA_CategoryBBCInfo, data.PGA_HC_CommodityTypeBBC);
							shouldSetCommonFields = true;
							fieldsActivators["GTINNo"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCCPRInd)
					{
						if (YesNoList.IsYes(hC_CPRInd))
						{
							SetValue(hc.CA_IntendedUseCodeCPRInfo, data.PGA_HC_IntendedUseCodeCPR);
							SetValue(hc.CA_CategoryCPRInfo, data.PGA_HC_CommodityTypeCPR);
							shouldSetCommonFields = true;
							fieldsActivators["GTINNo"] = true;
							fieldsActivators["BatchLotNo"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCCTOInd)
					{
						if (YesNoList.IsYes(hC_CTOInd))
						{
							SetValue(hc.CA_IntendedUseCodeCTOInfo, data.PGA_HC_IntendedUseCodeCTO);
							SetValue(hc.CA_CategoryCTOInfo, data.PGA_HC_CommodityTypeCTO);
							shouldSetCommonFields = true;
							fieldsActivators["GTINNo"] = true;
							fieldsActivators["LymphoCellOrgan"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCDSEInd)
					{
						if (YesNoList.IsYes(hC_DSEInd))
						{
							SetValue(hc.CA_IntendedUseCodeDSEInfo, data.PGA_HC_IntendedUseCodeDSE);
							SetValue(hc.CA_CategoryDSEInfo, data.PGA_HC_CommodityTypeDSE);
							shouldSetCommonFields = true;
							fieldsActivators["SemenCertification"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCHDRInd)
					{
						if (YesNoList.IsYes(hC_HDRInd))
						{
							SetValue(hc.CA_IntendedUseCodeHDRInfo, data.PGA_HC_IntendedUseCodeHDR);
							SetValue(hc.CA_CategoryHDRInfo, data.PGA_HC_CommodityTypeHDR);
							shouldSetCommonFields = true;
							fieldsActivators["GTINNo"] = true;
							fieldsActivators["BatchLotNo"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCMDEInd)
					{
						if (YesNoList.IsYes(hC_MDEInd))
						{
							SetValue(hc.CA_IntendedUseCodeMDEInfo, data.PGA_HC_IntendedUseCodeMDE);
							SetValue(hc.CA_CategoryMDEInfo, data.PGA_HC_CommodityTypeMDE);
							shouldSetCommonFields = true;
							fieldsActivators["GTINNo"] = true;
							fieldsActivators["BatchLotNo"] = true;
							fieldsActivators["UniqueDeviceID"] = true;
							fieldsActivators["MedDevEstablishLicenceExemption"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCNHPInd)
					{
						if (YesNoList.IsYes(hC_NHPInd))
						{
							SetValue(hc.CA_IntendedUseCodeNHPInfo, data.PGA_HC_IntendedUseCodeNHP);
							SetValue(hc.CA_CategoryNHPInfo, data.PGA_HC_CommodityTypeNHP);
							shouldSetCommonFields = true;
							fieldsActivators["GTINNo"] = true;
							fieldsActivators["BatchLotNo"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCOCSInd)
					{
						if (YesNoList.IsYes(hC_OCSInd))
						{
							SetValue(hc.CA_IntendedUseCodeOCSInfo, data.PGA_HC_IntendedUseCodeOCS);
							SetValue(hc.CA_CategoryOCSInfo, data.PGA_HC_CommodityTypeOCS);
							shouldSetCommonFields = true;
							fieldsActivators["BatchLotNo"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCPESInd)
					{
						if (YesNoList.IsYes(hC_PESInd))
						{
							SetValue(hc.CA_IntendedUseCodePESInfo, data.PGA_HC_IntendedUseCodePES);
							SetValue(hc.CA_CategoryPESInfo, data.PGA_HC_CommodityTypePES);
							shouldSetCommonFields = true;
							fieldsActivators["BatchLotNo"] = true;
							fieldsActivators["CASNumber"] = true;
							fieldsActivators["PMRAScheduledPestControlProducts"] = true;
							fieldsActivators["PMRAExemptPestControlProducts"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCREDInd)
					{
						if (YesNoList.IsYes(hC_REDInd))
						{
							SetValue(hc.CA_IntendedUseCodeREDInfo, data.PGA_HC_IntendedUseCodeRED);
							SetValue(hc.CA_CategoryREDInfo, data.PGA_HC_CommodityTypeRED);
							shouldSetCommonFields = true;
							fieldsActivators["FDANumber"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (hasHCVETInd)
					{
						if (YesNoList.IsYes(hC_VETInd))
						{
							SetValue(hc.CA_IntendedUseCodeVETInfo, data.PGA_HC_IntendedUseCodeVET);
							SetValue(hc.CA_CategoryVETInfo, data.PGA_HC_CommodityTypeVET);
							shouldSetCommonFields = true;
							fieldsActivators["GTINNo"] = true;
							fieldsActivators["BatchLotNo"] = true;
							fieldsActivators["DIFURN"] = true;
						}
					}

					if (shouldSetCommonFields)
					{
						SetPGAHCCommonFields(hc, data, fieldsActivators);
						SetPGAHCLPCOs(hc, data, fieldsActivators.ContainsKey("DIFURN") && fieldsActivators["DIFURN"]);
					}

					hcPGASetters.ForEach(x => x(hc));
				}
			}
		}

		void SetPGAHCCommonFields(HCPGAHeader hcPGAHeader, IPGADataToLoad data, Dictionary<ZString, ZBool> fieldsActivators)
		{
			if (fieldsActivators.ContainsKey("GTINNo") && fieldsActivators["GTINNo"])
			{
				SetValue(hcPGAHeader.CA_GTINNumberInfo, data.PGA_HC_GTINNumber);
			}
			if (fieldsActivators.ContainsKey("BatchLotNo") && fieldsActivators["BatchLotNo"])
			{
				SetValue(hcPGAHeader.CA_BatchLotNumberInfo, data.PGA_HC_BatchLotNumber);
			}
			if (fieldsActivators.ContainsKey("LymphoCellOrgan") && fieldsActivators["LymphoCellOrgan"] && IsValidYesReply(data.PGA_HC_LymphoCellOrgan))
			{
				hcPGAHeader.CA_CTO_LCO = true;
			}
			if (fieldsActivators.ContainsKey("SemenCertification") && fieldsActivators["SemenCertification"] && IsValidYesReply(data.PGA_HC_SemenCertification))
			{
				hcPGAHeader.CA_ComplianceStatement = true;
			}
			if (fieldsActivators.ContainsKey("UniqueDeviceID") && fieldsActivators["UniqueDeviceID"])
			{
				SetValue(hcPGAHeader.CA_UniqueDeviceIDNumberInfo, data.PGA_HC_MedUniqueDeviceIDNumber);
			}
			if (fieldsActivators.ContainsKey("MedDevEstablishLicenceExemption") && fieldsActivators["MedDevEstablishLicenceExemption"] && IsValidYesReply(data.PGA_HC_MedDevEstablishLicenceExemption))
			{
				hcPGAHeader.CA_MDE_LEX = true;
			}
			if (fieldsActivators.ContainsKey("CASNumber") && fieldsActivators["CASNumber"])
			{
				SetValue(hcPGAHeader.CA_CASNumberInfo, data.PGA_HC_CASNumber);
			}
			if (fieldsActivators.ContainsKey("PMRAScheduledPestControlProducts") && fieldsActivators["PMRAScheduledPestControlProducts"] && IsValidYesReply(data.PGA_HC_PMRAScheduledPestControlProducts))
			{
				hcPGAHeader.CA_PES_SPCP = true;
			}
			if (fieldsActivators.ContainsKey("PMRAExemptPestControlProducts") && fieldsActivators["PMRAExemptPestControlProducts"] && IsValidYesReply(data.PGA_HC_PMRAExemptPestControlProducts))
			{
				hcPGAHeader.CA_PES_EPCP = true;
			}
			if (fieldsActivators.ContainsKey("FDANumber") && fieldsActivators["FDANumber"])
			{
				SetValue(hcPGAHeader.CA_FDANumberInfo, data.PGA_HC_FDANumber);
			}
		}

		void SetPGAHCLPCOs(HCPGAHeader hcPGAHeader, IPGADataToLoad data, ZBool allowDIFURN)
		{
			if (data.PGA_HC_LPCOs.HasValue)
			{
				var deleteLPCOs = new List<CusCALPCO>();
				var lpcos = hcPGAHeader.LPCOViews;
				var defaultTypes = hcPGAHeader.GetDefaultLPCOTypesForAllEnalbedPrograms().Select(x => x.Code);
				foreach (CusCALPCO lpco in lpcos)
				{
					if (!defaultTypes.Contains(lpco.CLP_Type))
					{
						deleteLPCOs.Add(lpco);
					}
				}
				foreach (var lpco in deleteLPCOs)
				{
					lpcos.RemoveAndDelete(lpco);
				}
				var importedLPCOs = data.PGA_HC_LPCOs.Value.Split(";");
				foreach (var lpco in importedLPCOs)
				{
					var fields = lpco.Split("/");
					if (fields.Length >= 2)
					{
						var hcLPCO = hcPGAHeader.LPCOViews.AddNew();
						SetValue(hcLPCO.CLP_TypeInfo, fields[0]);
						SetValue(hcLPCO.CLP_RefNoInfo, fields[1]);
						if (fields.Length == 3 && allowDIFURN)
						{
							SetValue(hcLPCO.CLP_DIFRefNumberOrLocationInfo, fields[2]);
						}
					}
				}
			}
		}

		void SetNRCanPGAIndicators(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			var propertiesToSuspendSetting = new List<ZString>();
			var nrcanPGASetters = new List<Action<NRCanPGAHeader>>();
			var nRCanEEFInd = ZString.Empty;
			var hasNRCanEEFInd = data.PGA_NRCan_EEFInd.HasValue;
			if (hasNRCanEEFInd)
			{
				nRCanEEFInd = GetYesNoString(data.PGA_NRCan_EEFInd.Value);
				if (YesNoList.IsYesOrNo(nRCanEEFInd))
				{
					propertiesToSuspendSetting.Add(NRCanPGAHeader.Schema.CA_EEFProgramInd);
					nrcanPGASetters.Add((x) =>
					{
						SetValue(x.CA_EEFProgramIndInfo, nRCanEEFInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var nRCanEXPInd = ZString.Empty;
			var hasNRCanEXPInd = data.PGA_NRCan_EXPInd.HasValue;
			if (hasNRCanEXPInd)
			{
				nRCanEXPInd = GetYesNoString(data.PGA_NRCan_EXPInd.Value);
				if (YesNoList.IsYesOrNo(nRCanEXPInd))
				{
					propertiesToSuspendSetting.Add(NRCanPGAHeader.Schema.CA_EXPProgramInd);
					nrcanPGASetters.Add((x) =>
					{
						SetValue(x.CA_EXPProgramIndInfo, nRCanEXPInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var nRCanRDAInd = ZString.Empty;
			var hasNRCanRDAInd = data.PGA_NRCan_RDAInd.HasValue;
			if (hasNRCanRDAInd)
			{
				nRCanRDAInd = GetYesNoString(data.PGA_NRCan_RDAInd.Value);
				if (YesNoList.IsYesOrNo(nRCanRDAInd))
				{
					propertiesToSuspendSetting.Add(NRCanPGAHeader.Schema.CA_RDAProgramInd);
					nrcanPGASetters.Add((x) =>
					{
						SetValue(x.CA_RDAProgramIndInfo, nRCanRDAInd, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			if (propertiesToSuspendSetting.Count > 0)
			{
				bool isNRcanIndicatorSetToYes = YesNoList.IsYes(nRCanEEFInd) || YesNoList.IsYes(nRCanEXPInd) || YesNoList.IsYes(nRCanRDAInd);
				var nRcanIndicator = isNRcanIndicatorSetToYes ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				SetValue(pgaSupporter.NRCanIndInfo, nRcanIndicator, false, null, setterSuspender: pgaSupporter.SetterSuspender, resumeSuspender: true);

				if (pgaSupporter.NRCanRequirementProvider is NRCanPGAHeader nrcan)
				{
					methods.AddToDisposableList(nrcan.SetterSuspender.SuspendSetting(propertiesToSuspendSetting.ToArray()));
					nrcanPGASetters.ForEach(x => x(nrcan));
				}
			}
		}

		void SetPHACPGAIndicators(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			var hasPHACHAPInd = data.PGA_PHAC_HAPInd.HasValue;
			if (hasPHACHAPInd)
			{
				var cCA_PHACIndicator = GetYesNoString(data.PGA_PHAC_HAPInd.Value);
				if (YesNoList.IsYesOrNo(cCA_PHACIndicator))
				{
					SetValue(pgaSupporter.PHACIndInfo, cCA_PHACIndicator, false, null, setterSuspender: pgaSupporter.SetterSuspender, resumeSuspender: true);
					if (pgaSupporter.PHACRequirementProvider is PHACPGAHeader phac)
					{
						methods.AddToDisposableList(phac.SetterSuspender.SuspendSetting(PHACPGAHeader.Schema.CA_HAPProgramInd));
						SetValue(phac.CA_HAPProgramIndInfo, cCA_PHACIndicator, false, setterSuspender: phac.SetterSuspender, resumeSuspender: true);
					}
				}
			}
		}

		void SetTCPGAIndicators(IPGARequirementSupporter pgaSupporter, IPGADataToLoad data)
		{
			var propertiesToSuspendSetting = new List<ZString>();
			var tcPGASetters = new List<Action<TCPGAHeader>>();
			var cCA_TCTPRIndicator = ZString.Empty;
			var hasTCTPRInd = data.PGA_TC_TPRInd.HasValue;
			if (hasTCTPRInd)
			{
				cCA_TCTPRIndicator = GetYesNoString(data.PGA_TC_TPRInd.Value);
				if (YesNoList.IsYesOrNo(cCA_TCTPRIndicator))
				{
					propertiesToSuspendSetting.Add(TCPGAHeader.Schema.CA_TPRProgramInd);
					tcPGASetters.Add((x) =>
					{
						SetValue(x.CA_TPRProgramIndInfo, cCA_TCTPRIndicator, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			var cCA_TCVPRIndicator = ZString.Empty;
			var hasTCVPRInd = data.PGA_TC_VPRInd.HasValue;
			if (hasTCVPRInd)
			{
				cCA_TCVPRIndicator = GetYesNoString(data.PGA_TC_VPRInd.Value);
				if (YesNoList.IsYesOrNo(cCA_TCVPRIndicator))
				{
					propertiesToSuspendSetting.Add(TCPGAHeader.Schema.CA_VPRProgramInd);
					tcPGASetters.Add((x) =>
					{
						SetValue(x.CA_VPRProgramIndInfo, cCA_TCVPRIndicator, false, setterSuspender: x.SetterSuspender, resumeSuspender: true);
					});
				}
			}

			if (propertiesToSuspendSetting.Count > 0)
			{
				bool isTcIndicatorSetToYes = YesNoList.IsYes(cCA_TCTPRIndicator) || YesNoList.IsYes(cCA_TCVPRIndicator);
				var tcIndicator = isTcIndicatorSetToYes ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				SetValue(pgaSupporter.TCIndInfo, tcIndicator, false, null, setterSuspender: pgaSupporter.SetterSuspender, resumeSuspender: true);

				if (pgaSupporter.TCRequirementProvider is TCPGAHeader tc)
				{
					methods.AddToDisposableList(tc.SetterSuspender.SuspendSetting(propertiesToSuspendSetting.ToArray()));
					tcPGASetters.ForEach(x => x(tc));
				}
			}
		}

		void SetValue(ZPropertyInfo info, ZInt? value, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => value.Value, setterSuspender, resumeSuspender);
		}

		void SetValue(ZPropertyInfo info, ZDecimal? value, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => value.Value, setterSuspender, resumeSuspender);
		}

		void SetValue(ZPropertyInfo info, ZString? value, bool logMaxLengthViolation = true, string propertyIdentifier = null, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => GetStringValue(info, value.Value, logMaxLengthViolation, propertyIdentifier), setterSuspender, resumeSuspender);
		}

		ZString GetStringValue(ZPropertyInfo info, ZString value, bool logMaxLengthViolation = true, string propertyIdentifier = null)
		{
			var actualValue = value;
			var maxLength = info.MaxLength;
			var truncatedValue = actualValue.Left(maxLength);
			if (logMaxLengthViolation && maxLength < actualValue.Length)
			{
				if (string.IsNullOrEmpty(propertyIdentifier))
				{
					propertyIdentifier = info.HumanReadableName;
				}
				methods.DisplayLogMessage(Res.GetString("C172887A-84D8-4980-AD05-ADF176E58A3D", "{0} '{1}' is too long. Storing '{2}' instead.", propertyIdentifier, value, truncatedValue));
			}
			return truncatedValue;
		}

		void SetValueCore<T>(ZPropertyInfo info, bool hasValue, Func<T> getValue, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
			where T : IZType
		{
			if (hasValue)
			{
				if (setterSuspender != null && setterSuspender.IsSetterSuspended(info.Name))
				{
					if (resumeSuspender)
					{
						using (setterSuspender.ResumeSetting(info.Name))
						{
							info.Value = getValue();
						}
					}
				}
				else
				{
					info.Value = getValue();
				}
			}
		}

		public static bool IsValidYesReply(ZString? reply)
		{
			return reply.HasValue && (reply.Value.EqualsIgnoringCase("Y") || reply.Value.EqualsIgnoringCase("YES"));
		}

		internal static class PGAFieldNames
		{
			const string PGA_CFIA_Indicator = "PGA_CFIA_Indicator";
			const string PGA_CFIA_AIRSExtensionCode = "PGA_CFIA_AIRSExtensionCode";
			const string PGA_CFIA_LPCOs = "PGA_CFIA_LPCOs";
			const string PGA_CFIA_AIRSRegistrations = "PGA_CFIA_AIRSRegistrations";
			const string PGA_CFIA_AIRSEndUse = "PGA_CFIA_AIRSEndUse";
			const string PGA_CFIA_AIRSMiscellaneous = "PGA_CFIA_AIRSMiscellaneous";
			const string PGA_CFIA_SourceCountry = "PGA_CFIA_SourceCountry";
			const string PGA_CFIA_SourceState = "PGA_CFIA_SourceState";

			const string PGA_CNSC_Indicator = "PGA_CNSC_Indicator";
			const string PGA_CNSC_Category = "PGA_CNSC_Category";
			const string PGA_CNSC_NNIECRSchedulePartNo = "PGA_CNSC_NNIECRSchedulePartNo";
			const string PGA_CNSC_PackMarks = "PGA_CNSC_PackMarks";
			const string PGA_CNSC_LPCOs = "PGA_CNSC_LPCOs";

			const string PGA_GAC_Indicator = "PGA_GAC_Indicator";

			const string PGA_DFO_ABIInd = "PGA_DFO_ABIInd";
			const string PGA_DFO_AISInd = "PGA_DFO_AISInd";
			const string PGA_DFO_TTPInd = "PGA_DFO_TTPInd";

			const string PGA_ECCC_WRMInd = "PGA_ECCC_WRMInd";
			const string PGA_ECCC_ODSInd = "PGA_ECCC_ODSInd";
			const string PGA_ECCC_WENInd = "PGA_ECCC_WENInd";
			const string PGA_ECCC_VEEInd = "PGA_ECCC_VEEInd";
			const string PGA_ECCC_SourceOfSpecimen = "PGA_ECCC_SourceOfSpecimen";
			const string PGA_ECCC_LifeStage = "PGA_ECCC_LifeStage";
			const string PGA_ECCC_Age = "PGA_ECCC_Age";
			const string PGA_ECCC_Sex = "PGA_ECCC_Sex";
			const string PGA_ECCC_Regulated = "PGA_ECCC_Regulated";
			const string PGA_ECCC_ScientificName = "PGA_ECCC_ScientificName";
			const string PGA_ECCC_TSN = "PGA_ECCC_TSN";
			const string PGA_ECCC_AphiaID = "PGA_ECCC_AphiaID";
			const string PGA_ECCC_Identities = "PGA_ECCC_Identities";
			const string PGA_ECCC_ProcessCode = "PGA_ECCC_ProcessCode";
			const string PGA_ECCC_NationalMark = "PGA_ECCC_NationalMark";
			const string PGA_ECCC_EPACertified = "PGA_ECCC_EPACertified";
			const string PGA_ECCC_Transition = "PGA_ECCC_Transition";
			const string PGA_ECCC_Incomplete = "PGA_ECCC_Incomplete";
			const string PGA_ECCC_CanadaUnique = "PGA_ECCC_CanadaUnique";
			const string PGA_ECCC_BulkReporting = "PGA_ECCC_BulkReporting";
			const string PGA_ECCC_VehicleClass = "PGA_ECCC_VehicleClass";
			const string PGA_ECCC_EngineClass = "PGA_ECCC_EngineClass";
			const string PGA_ECCC_EngineMake = "PGA_ECCC_EngineMake";
			const string PGA_ECCC_EngineModel = "PGA_ECCC_EngineModel";
			const string PGA_ECCC_EngineModelYear = "PGA_ECCC_EngineModelYear";
			const string PGA_ECCC_EngineIDNumber = "PGA_ECCC_EngineIDNumber";
			const string PGA_ECCC_EngineManufacturer = "PGA_ECCC_EngineManufacturer";
			const string PGA_ECCC_EngineFamilyName = "PGA_ECCC_EngineFamilyName";
			const string PGA_ECCC_EngineTestGroup = "PGA_ECCC_EngineTestGroup";
			const string PGA_ECCC_EngineEvaporativeFamily = "PGA_ECCC_EngineEvaporativeFamily";
			const string PGA_ECCC_EnginePowerRating = "PGA_ECCC_EnginePowerRating";
			const string PGA_ECCC_EnginePowerRatingUQ = "PGA_ECCC_EnginePowerRatingUQ";
			const string PGA_ECCC_MachineMake = "PGA_ECCC_MachineMake";
			const string PGA_ECCC_MachineModel = "PGA_ECCC_MachineModel";
			const string PGA_ECCC_MachineModelYear = "PGA_ECCC_MachineModelYear";
			const string PGA_ECCC_MachineManufacturer = "PGA_ECCC_MachineManufacturer";
			const string PGA_ECCC_EngineLocation = "PGA_ECCC_EngineLocation";
			const string PGA_ECCC_EvidenceOfConfirmityLocation = "PGA_ECCC_EvidenceOfConfirmityLocation";
			const string PGA_ECCC_IntendedUseCode = "PGA_ECCC_IntendedUseCode";
			const string PGA_ECCC_LPCOs = "PGA_ECCC_LPCOs";
			const string PGA_ECCC_CASNumber = "PGA_ECCC_CASNumber";

			const string PGA_HC_APIInd = "PGA_HC_APIInd";
			const string PGA_HC_IntendedUseCodeAPI = "PGA_HC_IntendedUseCodeAPI";
			const string PGA_HC_CommodityTypeAPI = "PGA_HC_CommodityTypeAPI";
			const string PGA_HC_BBCInd = "PGA_HC_BBCInd";
			const string PGA_HC_IntendedUseCodeBBC = "PGA_HC_IntendedUseCodeBBC";
			const string PGA_HC_CommodityTypeBBC = "PGA_HC_CommodityTypeBBC";
			const string PGA_HC_CTOInd = "PGA_HC_CTOInd";
			const string PGA_HC_IntendedUseCodeCTO = "PGA_HC_IntendedUseCodeCTO";
			const string PGA_HC_CommodityTypeCTO = "PGA_HC_CommodityTypeCTO";
			const string PGA_HC_CPRInd = "PGA_HC_CPRInd";
			const string PGA_HC_IntendedUseCodeCPR = "PGA_HC_IntendedUseCodeCPR";
			const string PGA_HC_CommodityTypeCPR = "PGA_HC_CommodityTypeCPR";
			const string PGA_HC_DSEInd = "PGA_HC_DSEInd";
			const string PGA_HC_IntendedUseCodeDSE = "PGA_HC_IntendedUseCodeDSE";
			const string PGA_HC_CommodityTypeDSE = "PGA_HC_CommodityTypeDSE";
			const string PGA_HC_HDRInd = "PGA_HC_HDRInd";
			const string PGA_HC_IntendedUseCodeHDR = "PGA_HC_IntendedUseCodeHDR";
			const string PGA_HC_CommodityTypeHDR = "PGA_HC_CommodityTypeHDR";
			const string PGA_HC_OCSInd = "PGA_HC_OCSInd";
			const string PGA_HC_IntendedUseCodeOCS = "PGA_HC_IntendedUseCodeOCS";
			const string PGA_HC_CommodityTypeOCS = "PGA_HC_CommodityTypeOCS";
			const string PGA_HC_MDEInd = "PGA_HC_MDEInd";
			const string PGA_HC_IntendedUseCodeMDE = "PGA_HC_IntendedUseCodeMDE";
			const string PGA_HC_CommodityTypeMDE = "PGA_HC_CommodityTypeMDE";
			const string PGA_HC_NHPInd = "PGA_HC_NHPInd";
			const string PGA_HC_IntendedUseCodeNHP = "PGA_HC_IntendedUseCodeNHP";
			const string PGA_HC_CommodityTypeNHP = "PGA_HC_CommodityTypeNHP";
			const string PGA_HC_PESInd = "PGA_HC_PESInd";
			const string PGA_HC_IntendedUseCodePES = "PGA_HC_IntendedUseCodePES";
			const string PGA_HC_CommodityTypePES = "PGA_HC_CommodityTypePES";
			const string PGA_HC_REDInd = "PGA_HC_REDInd";
			const string PGA_HC_IntendedUseCodeRED = "PGA_HC_IntendedUseCodeRED";
			const string PGA_HC_CommodityTypeRED = "PGA_HC_CommodityTypeRED";
			const string PGA_HC_VETInd = "PGA_HC_VETInd";
			const string PGA_HC_IntendedUseCodeVET = "PGA_HC_IntendedUseCodeVET";
			const string PGA_HC_CommodityTypeVET = "PGA_HC_CommodityTypeVET";
			const string PGA_HC_GTINNumber = "PGA_HC_GTINNumber";
			const string PGA_HC_BatchLotNumber = "PGA_HC_BatchLotNumber";
			const string PGA_HC_LymphoCellOrgan = "PGA_HC_LymphoCellOrgan";
			const string PGA_HC_SemenCertification = "PGA_HC_SemenCertification";
			const string PGA_HC_MedUniqueDeviceIDNumber = "PGA_HC_MedUniqueDeviceIDNumber";
			const string PGA_HC_MedDevEstablishLicenceExemption = "PGA_HC_MedDevEstablishLicenceExemption";
			const string PGA_HC_CASNumber = "PGA_HC_CASNumber";
			const string PGA_HC_PMRAScheduledPestControlProducts = "PGA_HC_PMRAScheduledPestControlProducts";
			const string PGA_HC_PMRAExemptPestControlProducts = "PGA_HC_PMRAExemptPestControlProducts";
			const string PGA_HC_FDANumber = "PGA_HC_FDANumber";
			const string PGA_HC_LPCOs = "PGA_HC_LPCOs";

			const string PGA_NRCan_EEFInd = "PGA_NRCan_EEFInd";
			const string PGA_NRCan_EXPInd = "PGA_NRCan_EXPInd";
			const string PGA_NRCan_RDAInd = "PGA_NRCan_RDAInd";

			const string PGA_PHAC_HAPInd = "PGA_PHAC_HAPInd";

			const string PGA_TC_TPRInd = "PGA_TC_TPRInd";
			const string PGA_TC_VPRInd = "PGA_TC_VPRInd";

			internal static IEnumerable<string> GetPGAFieldNames()
			{
				foreach (var property in GetPGACFIAFieldNames())
				{
					yield return property;
				}
				foreach (var property in GetPGAGACFieldNames())
				{
					yield return property;
				}
				foreach (var property in GetPGADFOFieldNames())
				{
					yield return property;
				}
				foreach (var property in GetPGACNSCFieldNames())
				{
					yield return property;
				}
				foreach (var property in GetPGAECCCFieldNames())
				{
					yield return property;
				}
				foreach (var property in GetPGAHCFieldNames())
				{
					yield return property;
				}
				foreach (var property in GetPGANRCanFieldNames())
				{
					yield return property;
				}
				foreach (var property in GetPGAPHACFieldNames())
				{
					yield return property;
				}
				foreach (var property in GetPGATCFieldNames())
				{
					yield return property;
				}
			}

			static IEnumerable<string> GetPGACFIAFieldNames()
			{
				yield return PGA_CFIA_Indicator;
				yield return PGA_CFIA_AIRSExtensionCode;
				yield return PGA_CFIA_LPCOs;
				yield return PGA_CFIA_AIRSRegistrations;
				yield return PGA_CFIA_AIRSEndUse;
				yield return PGA_CFIA_AIRSMiscellaneous;
				yield return PGA_CFIA_SourceCountry;
				yield return PGA_CFIA_SourceState;
			}

			static IEnumerable<string> GetPGAGACFieldNames()
			{
				yield return PGA_GAC_Indicator;
			}

			static IEnumerable<string> GetPGADFOFieldNames()
			{
				yield return PGA_DFO_ABIInd;
				yield return PGA_DFO_AISInd;
				yield return PGA_DFO_TTPInd;
			}

			static IEnumerable<string> GetPGACNSCFieldNames()
			{
				yield return PGA_CNSC_Indicator;
				yield return PGA_CNSC_Category;
				yield return PGA_CNSC_NNIECRSchedulePartNo;
				yield return PGA_CNSC_PackMarks;
				yield return PGA_CNSC_LPCOs;
			}

			static IEnumerable<string> GetPGAECCCFieldNames()
			{
				yield return PGA_ECCC_WRMInd;
				yield return PGA_ECCC_ODSInd;
				yield return PGA_ECCC_WENInd;
				yield return PGA_ECCC_VEEInd;
				yield return PGA_ECCC_CASNumber;
				yield return PGA_ECCC_SourceOfSpecimen;
				yield return PGA_ECCC_LifeStage;
				yield return PGA_ECCC_Age;
				yield return PGA_ECCC_Sex;
				yield return PGA_ECCC_Regulated;
				yield return PGA_ECCC_ScientificName;
				yield return PGA_ECCC_TSN;
				yield return PGA_ECCC_AphiaID;
				yield return PGA_ECCC_Identities;
				yield return PGA_ECCC_ProcessCode;
				yield return PGA_ECCC_NationalMark;
				yield return PGA_ECCC_EPACertified;
				yield return PGA_ECCC_Transition;
				yield return PGA_ECCC_Incomplete;
				yield return PGA_ECCC_CanadaUnique;
				yield return PGA_ECCC_BulkReporting;
				yield return PGA_ECCC_VehicleClass;
				yield return PGA_ECCC_EngineClass;
				yield return PGA_ECCC_EngineMake;
				yield return PGA_ECCC_EngineModel;
				yield return PGA_ECCC_EngineModelYear;
				yield return PGA_ECCC_EngineIDNumber;
				yield return PGA_ECCC_EngineManufacturer;
				yield return PGA_ECCC_EngineFamilyName;
				yield return PGA_ECCC_EngineTestGroup;
				yield return PGA_ECCC_EngineEvaporativeFamily;
				yield return PGA_ECCC_EnginePowerRating;
				yield return PGA_ECCC_EnginePowerRatingUQ;
				yield return PGA_ECCC_MachineMake;
				yield return PGA_ECCC_MachineModel;
				yield return PGA_ECCC_MachineModelYear;
				yield return PGA_ECCC_MachineManufacturer;
				yield return PGA_ECCC_EngineLocation;
				yield return PGA_ECCC_EvidenceOfConfirmityLocation;
				yield return PGA_ECCC_IntendedUseCode;
				yield return PGA_ECCC_LPCOs;
			}

			static IEnumerable<string> GetPGAHCFieldNames()
			{
				yield return PGA_HC_APIInd;
				yield return PGA_HC_IntendedUseCodeAPI;
				yield return PGA_HC_CommodityTypeAPI;
				yield return PGA_HC_BBCInd;
				yield return PGA_HC_IntendedUseCodeBBC;
				yield return PGA_HC_CommodityTypeBBC;
				yield return PGA_HC_CTOInd;
				yield return PGA_HC_IntendedUseCodeCTO;
				yield return PGA_HC_CommodityTypeCTO;
				yield return PGA_HC_CPRInd;
				yield return PGA_HC_IntendedUseCodeCPR;
				yield return PGA_HC_CommodityTypeCPR;
				yield return PGA_HC_DSEInd;
				yield return PGA_HC_IntendedUseCodeDSE;
				yield return PGA_HC_CommodityTypeDSE;
				yield return PGA_HC_HDRInd;
				yield return PGA_HC_IntendedUseCodeHDR;
				yield return PGA_HC_CommodityTypeHDR;
				yield return PGA_HC_OCSInd;
				yield return PGA_HC_IntendedUseCodeOCS;
				yield return PGA_HC_CommodityTypeOCS;
				yield return PGA_HC_MDEInd;
				yield return PGA_HC_IntendedUseCodeMDE;
				yield return PGA_HC_CommodityTypeMDE;
				yield return PGA_HC_NHPInd;
				yield return PGA_HC_IntendedUseCodeNHP;
				yield return PGA_HC_CommodityTypeNHP;
				yield return PGA_HC_PESInd;
				yield return PGA_HC_IntendedUseCodePES;
				yield return PGA_HC_CommodityTypePES;
				yield return PGA_HC_REDInd;
				yield return PGA_HC_IntendedUseCodeRED;
				yield return PGA_HC_CommodityTypeRED;
				yield return PGA_HC_VETInd;
				yield return PGA_HC_IntendedUseCodeVET;
				yield return PGA_HC_CommodityTypeVET;
				yield return PGA_HC_GTINNumber;
				yield return PGA_HC_BatchLotNumber;
				yield return PGA_HC_LymphoCellOrgan;
				yield return PGA_HC_SemenCertification;
				yield return PGA_HC_MedUniqueDeviceIDNumber;
				yield return PGA_HC_MedDevEstablishLicenceExemption;
				yield return PGA_HC_CASNumber;
				yield return PGA_HC_PMRAScheduledPestControlProducts;
				yield return PGA_HC_PMRAExemptPestControlProducts;
				yield return PGA_HC_FDANumber;
				yield return PGA_HC_LPCOs;
			}

			static IEnumerable<string> GetPGANRCanFieldNames()
			{
				yield return PGA_NRCan_EEFInd;
				yield return PGA_NRCan_EXPInd;
				yield return PGA_NRCan_RDAInd;
			}

			static IEnumerable<string> GetPGAPHACFieldNames()
			{
				yield return PGA_PHAC_HAPInd;
			}

			static IEnumerable<string> GetPGATCFieldNames()
			{
				yield return PGA_TC_TPRInd;
				yield return PGA_TC_VPRInd;
			}

			internal static IEnumerable<ZString> GetPGAPropertiesToSuspendSetting(IExposeMethodsForPGADataLoad methods)
			{
				if (methods.HasColumn(PGA_ECCC_WRMInd) ||
					methods.HasColumn(PGA_ECCC_ODSInd) ||
					methods.HasColumn(PGA_ECCC_WENInd) ||
					methods.HasColumn(PGA_ECCC_VEEInd))
				{
					yield return CusClassPartPivot.Schema.CCA_ECCCIndicator;
				}

				if (methods.HasColumn(PGA_CFIA_Indicator))
				{
					yield return CusClassPartPivot.Schema.CCA_CFIAIndicator;
				}

				if (methods.HasColumn(PGA_CNSC_Indicator))
				{
					yield return CusClassPartPivot.Schema.CCA_CNSCIndicator;
				}

				if (methods.HasColumn(PGA_GAC_Indicator))
				{
					yield return CusClassPartPivot.Schema.CCA_GACIndicator;
				}

				if (methods.HasColumn(PGA_DFO_ABIInd) ||
					methods.HasColumn(PGA_DFO_AISInd) ||
					methods.HasColumn(PGA_DFO_TTPInd))
				{
					yield return CusClassPartPivot.Schema.CCA_DFOIndicator;
				}

				if (methods.HasColumn(PGA_HC_APIInd) ||
					methods.HasColumn(PGA_HC_BBCInd) ||
					methods.HasColumn(PGA_HC_CPRInd) ||
					methods.HasColumn(PGA_HC_CTOInd) ||
					methods.HasColumn(PGA_HC_DSEInd) ||
					methods.HasColumn(PGA_HC_HDRInd) ||
					methods.HasColumn(PGA_HC_MDEInd) ||
					methods.HasColumn(PGA_HC_NHPInd) ||
					methods.HasColumn(PGA_HC_OCSInd) ||
					methods.HasColumn(PGA_HC_PESInd) ||
					methods.HasColumn(PGA_HC_REDInd) ||
					methods.HasColumn(PGA_HC_VETInd))
				{
					yield return CusClassPartPivot.Schema.CCA_HCIndicator;
				}

				if (methods.HasColumn(PGA_NRCan_EEFInd) ||
					methods.HasColumn(PGA_NRCan_EXPInd) ||
					methods.HasColumn(PGA_NRCan_RDAInd))
				{
					yield return CusClassPartPivot.Schema.CCA_NRCanIndicator;
				}

				if (methods.HasColumn(PGA_PHAC_HAPInd))
				{
					yield return CusClassPartPivot.Schema.CCA_PHACIndicator;
				}

				if (methods.HasColumn(PGA_TC_TPRInd) ||
					methods.HasColumn(PGA_TC_VPRInd))
				{
					yield return CusClassPartPivot.Schema.CCA_TCIndicator;
				}
			}
		}
	}
}
